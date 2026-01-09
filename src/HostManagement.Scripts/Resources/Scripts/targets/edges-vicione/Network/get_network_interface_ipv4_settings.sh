#!/usr/bin/env bash

# ###
#
# Get Network Interface IPv4 Settings Script
#
# The script returns 0 if ok or warning, non zero for error.
# ###

set -eEuo pipefail

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

#######################################
# Error codes
#######################################

readonly ERR_CODE_1="The interface '%s' does not exist."

#######################################
# Constants
#######################################

readonly DIRECTORY_NETPLAN="/etc/netplan"

#######################################
# Variables
#######################################

CONFIGURATION_PATH=""
CONFIGURATION_CONTENT=""
CONFIGURATION_ENTRY_POINT=""

INTERFACE_NAME=""
DHCP_ENABLED="true"
GATEWAY="null"
VLAN_ID="0"
VLAN_ENABLED="false"

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 <interface_name>"
    exit 0
}

define_configuration_entrypoint() {
    vlan_name=$(Scripts/Network/Helper/get_network_vlan_name.sh "$CONFIGURATION_CONTENT" "$INTERFACE_NAME")

    if [[ -n "$vlan_name" ]]; then
        CONFIGURATION_ENTRY_POINT="$vlan_name"
        VLAN_ID=${vlan_name//[!0-9]/}
        VLAN_ENABLED="true"
    else
        CONFIGURATION_ENTRY_POINT="${INTERFACE_NAME}:"
    fi
}

determine_dhcp_enabled() {
    if ! Scripts/Network/Helper/is_dhcp_enabled.sh "$CONFIGURATION_CONTENT"; then
        DHCP_ENABLED="false"
    fi
}

determine_gateway() {
    GATEWAY=$(
        grep -A100 "$CONFIGURATION_ENTRY_POINT" <<< "$CONFIGURATION_CONTENT" |
        grep -A3 "routes:" |
        grep "via:" |
        tr -d ' ' |
        cut -d: -f2 || true)

    if [[ -z "$GATEWAY" ]]; then
        GATEWAY="null"
    else
        GATEWAY="\"${GATEWAY}\""
    fi
}

determine_ip_addresses() {
    ip_addresses=$(
        grep -A100 "$CONFIGURATION_ENTRY_POINT" <<< "$CONFIGURATION_CONTENT" |
        grep -A100 "addresses:" |
        sed "/routes:/q; /nameservers:/q" |
        grep -Eo '[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+/[0-9]+' || true)

    if [[ -n "$ip_addresses" ]]; then
        mapfile -t ip_address_array <<< "$ip_addresses"

        for current in "${ip_address_array[@]}"; do
            ip_address=$(cut -d'/' -f1 <<< "$current")
            cidr=$(cut -d'/' -f2 <<< "$current")
            netmask=$(Scripts/Network/Helper/convert_ipv4_cidr_to_netmask.sh "$cidr")

            IP_DETAILS+=("{\"IPAddress\": \"${ip_address}\", \"Netmask\": \"${netmask}\"}")
        done
    fi
}

load_configuration_content() {
    if [[ -e "$CONFIGURATION_PATH" ]]; then
        CONFIGURATION_CONTENT=$(< "$CONFIGURATION_PATH")
    fi
}

set_configuration_path() {
    CONFIGURATION_PATH="${DIRECTORY_NETPLAN}/50-${INTERFACE_NAME}.yaml"
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    INTERFACE_NAME="${1:-}"

    if ! Scripts/Network/Helper/is_network_interface_available.sh "$INTERFACE_NAME"; then
        err "$(printf "$ERR_CODE_1" "$INTERFACE_NAME")" 1
    fi

    set_configuration_path
    load_configuration_content

    if [[ -n "$CONFIGURATION_CONTENT" ]]; then
        define_configuration_entrypoint
        determine_dhcp_enabled

        determine_gateway
        determine_ip_addresses
    fi

    json_output="{"
    json_output+="\"DHCPEnabled\": ${DHCP_ENABLED},"
    json_output+="\"Gateway\": ${GATEWAY},"
    json_output+="\"IPv4Details\": [$(printf '%s, ' "${IP_DETAILS[@]}" | sed 's/, $//')],"
    json_output+="\"VLANID\": ${VLAN_ID}",
    json_output+="\"VLANEnabled\": ${VLAN_ENABLED}"
    json_output+="}"

    echo "$json_output"
}

main "$@"
#eof
