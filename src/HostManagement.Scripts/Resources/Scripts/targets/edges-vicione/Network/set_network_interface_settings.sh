#!/usr/bin/env bash

# ###
#
# Set Network Interface Settings Script
#
# The script returns 0 if ok, non zero for error.
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

readonly ERR_CODE_1="Only one gateway can be defined. The gateway '%s' is already in use."
readonly ERR_CODE_2="Unsupported flag '%s'."
readonly ERR_CODE_3="An VLAN-ID must be greater than or equal to 1 and less than or equal to 4094."
readonly ERR_CODE_4="The specified MAC address is not valid."
readonly ERR_CODE_5="Unsupported value '%s'."
readonly ERR_CODE_6="Missing arguments for '%s'."
readonly ERR_CODE_7="No network interface was specified."
readonly ERR_CODE_8="Please enter the required parameters."

#######################################
# Constants
#######################################

readonly DIRECTORY_NETPLAN="/etc/netplan"
readonly VALUE_TRUE="true"
readonly VALUE_FALSE="false"

readonly FLAG_VLAN_ID="--vlan-id"
readonly FLAG_INTERFACE_NAME="--interface-name"
readonly FLAG_MAC_ADDRESS="--mac-address"
readonly FLAG_ENABLED="--enabled"
readonly FLAG_DHCP_ENABLED="--dhcp-enabled"
readonly FLAG_IP_ADDRESS="--ip-address"
readonly FLAG_GATEWAY="--gateway"

#######################################
# Variables
#######################################

CONFIGURATION_PATH=""

VLAN_ID=""
INTERFACE_NAME=""
ENABLED="$VALUE_TRUE"
DHCP_ENABLED="$VALUE_TRUE"
IP_ADDRESS_DISABLED_MARKER="#"
FILE_CONTENT=""
GATEWAY=""
MAC_ADDRESS=""
HAS_IP_ADDRESS_BEEN_ADDED="$VALUE_FALSE"

#######################################
# Methods
#######################################

usage() {
    echo "The order of the parameters must be observed!"
    echo "Usage: $0"
    echo "  [${FLAG_VLAN_ID} <id>] ${FLAG_INTERFACE_NAME} <name> [${FLAG_MAC_ADDRESS} <address>] ${FLAG_ENABLED} <${VALUE_TRUE}|${VALUE_FALSE}> ${FLAG_DHCP_ENABLED} <${VALUE_TRUE}|${VALUE_FALSE}>"
    echo "  [${FLAG_IP_ADDRESS} <ip>/<cidr>] [--gateway <ip>]"
    echo
    echo "Options:"
    echo "  ${FLAG_VLAN_ID}        - Specifies the VLAN ID to configure for the given network interface. (optional)"
    echo "  ${FLAG_INTERFACE_NAME} - Specifies the name of the network interface to configure."
    echo "  ${FLAG_MAC_ADDRESS}    - Sets the MAC address for the given network interface. (optional)"
    echo "  ${FLAG_ENABLED}        - Enables or disables the given network interface. (optional)"
    echo "  ${FLAG_DHCP_ENABLED}   - Specifies whether DHCP should be used to obtain an IP address automatically."
    echo "  ${FLAG_IP_ADDRESS}     - Specifies the static IP address to assign to the given network interface."
    echo "                     Must be provided in CIDR notation if DHCP is disabled. (optional)"
    echo "  ${FLAG_GATEWAY}        - Specifies the default gateway IP address to use for this network interface. (optional)"
    echo
    echo "Examples:"
    echo "$0"
    echo "  [${FLAG_VLAN_ID} 10] ${FLAG_INTERFACE_NAME} ens33 [${FLAG_MAC_ADDRESS} aa:bb:cc:dd:ee:ff] ${FLAG_ENABLED} ${VALUE_TRUE} ${FLAG_DHCP_ENABLED} ${VALUE_TRUE}"
    echo 
    echo "$0"
    echo "  [${FLAG_VLAN_ID} 10] ${FLAG_INTERFACE_NAME} ens33 [${FLAG_MAC_ADDRESS} aa:bb:cc:dd:ee:ff] ${FLAG_ENABLED} ${VALUE_TRUE} ${FLAG_DHCP_ENABLED} ${VALUE_FALSE}"
    echo "  ${FLAG_IP_ADDRESS} 192.168.2.2/24 ${FLAG_GATEWAY} 192.168.2.1"
    echo "  ${FLAG_IP_ADDRESS} 192.168.2.3/24"
    exit 0
}

activate_dhcp_mode() {
    if [[ "$DHCP_ENABLED" == "$VALUE_TRUE" ]]; then
        FILE_CONTENT+="      dhcp4: true"$'\n'
        FILE_CONTENT+="      link-local: [ipv4]"$'\n'
    else
        FILE_CONTENT+="      dhcp4: false"$'\n'
        IP_ADDRESS_DISABLED_MARKER=""
    fi
}

add_gateway() {
    FILE_CONTENT+="      ${IP_ADDRESS_DISABLED_MARKER}routes:"$'\n'
    FILE_CONTENT+="        ${IP_ADDRESS_DISABLED_MARKER}- to: 0.0.0.0/0"$'\n'
    FILE_CONTENT+="        ${IP_ADDRESS_DISABLED_MARKER}  via: ${GATEWAY}"$'\n'
}

add_ip_address() {
    local ip_address=$1

    if [[ "$HAS_IP_ADDRESS_BEEN_ADDED" == "$VALUE_FALSE" ]]; then
        HAS_IP_ADDRESS_BEEN_ADDED="$VALUE_TRUE"

        FILE_CONTENT+="      ${IP_ADDRESS_DISABLED_MARKER}addresses:"$'\n'
    fi

    FILE_CONTENT+="        ${IP_ADDRESS_DISABLED_MARKER}- ${ip_address}"$'\n'
}

add_netplan_base_configuration() {
    FILE_CONTENT+="#"$'\n'
    FILE_CONTENT+="# This file is managed by ifm HostManagement application."$'\n'
    FILE_CONTENT+="# Manual adjustments may not be compatible."$'\n'
    FILE_CONTENT+="#"$'\n'
    FILE_CONTENT+="network:"$'\n'
    FILE_CONTENT+="  version: 2"$'\n'
    FILE_CONTENT+="  renderer: networkd"$'\n'
    FILE_CONTENT+="  ethernets:"$'\n'
    FILE_CONTENT+="    ${INTERFACE_NAME}:"$'\n'

    if [[ "$ENABLED" == "$VALUE_FALSE" ]]; then
        FILE_CONTENT+="      activation-mode: off"$'\n'
    fi

    if [[ -n "$VLAN_ID" ]]; then
        if [[ -n "$MAC_ADDRESS" ]]; then
            FILE_CONTENT+="      macaddress: ${MAC_ADDRESS}"$'\n'
        fi
        
        FILE_CONTENT+="      dhcp4: false"$'\n'
        FILE_CONTENT+="  vlans:"$'\n'
        FILE_CONTENT+="    vlan${VLAN_ID}:"$'\n'
        FILE_CONTENT+="      id: ${VLAN_ID}"$'\n'
        FILE_CONTENT+="      link: ${INTERFACE_NAME}"$'\n'
    fi

    if [[ -n "$MAC_ADDRESS" ]]; then
        FILE_CONTENT+="      macaddress: ${MAC_ADDRESS}"$'\n'
    fi
}

assert_flag_has_arguments() {
    local argument_count="$1"
    local first_argument="$2"
    local flag_name="$3"

    if [[ "$argument_count" -eq 0 || "$first_argument" == --* ]]; then
        err "$(printf "$ERR_CODE_6" "$flag_name")" 6
    fi
}

check_bool_value() {
    if [[ "$1" != "$VALUE_TRUE" && "$1" != "$VALUE_FALSE" ]]; then
        err "$(printf "$ERR_CODE_5" "$1")" 5
    fi
}

check_for_existing_gateway() {
    if [[ -n "$GATEWAY" ]]; then
        err "$(printf "$ERR_CODE_1" "$GATEWAY")" 1
    fi
}

check_for_netplan_base_configuration() {
    if [[ -z "$FILE_CONTENT" ]]; then
        err "$ERR_CODE_8" 8
    fi
}

check_for_network_interface() {
    if [[ -z "$INTERFACE_NAME" ]]; then
        err "$ERR_CODE_7" 7
    fi
}

check_mac_address() {
    if [[ ! "$MAC_ADDRESS" =~ ^([a-fA-F0-9]{2}:){5}[a-fA-F0-9]{2}$ ]]; then
        err "$ERR_CODE_4" 4
    fi
}

set_vlan_id() {
    local vlan_id="$1"

    # 0:      No VLAN, only Qos-Tags;
    # 1:      Standard VLAN
    # 2-4094: User defined VLAN
    # 4095:   Reserved for internal usage

    if [[ "$vlan_id" -lt 1 || "$vlan_id" -gt 4094 ]]; then
        err "$ERR_CODE_3" 3
    fi

    VLAN_ID="$vlan_id"
}

write_configuration_to_file() {
    echo "$FILE_CONTENT" > "$CONFIGURATION_PATH"
    chmod 600 "$CONFIGURATION_PATH"
    exit 0
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    while [[ $# -gt 0 ]]; do
        flag_name="$1"
        shift

        case "$flag_name" in
            "$FLAG_VLAN_ID")
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                set_vlan_id "$1"
                ;;
            "$FLAG_INTERFACE_NAME")
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                INTERFACE_NAME="$1"
                CONFIGURATION_PATH="${DIRECTORY_NETPLAN}/50-${1}.yaml"
                ;;
            "$FLAG_MAC_ADDRESS")
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                MAC_ADDRESS="$1"
                check_mac_address
                ;;
            "$FLAG_ENABLED")
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                check_bool_value "$1"
                ENABLED="$1"
                ;;
            "$FLAG_DHCP_ENABLED")
                check_for_network_interface
                add_netplan_base_configuration
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                check_bool_value "$1"
                DHCP_ENABLED="$1"
                activate_dhcp_mode
                ;;
            "$FLAG_IP_ADDRESS")
                check_for_netplan_base_configuration
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                add_ip_address "$1"
                ;;
            "$FLAG_GATEWAY")
                check_for_existing_gateway
                check_for_netplan_base_configuration
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                GATEWAY="$1"
                ;;
            *)
                err "$(printf "$ERR_CODE_2" "$flag_name")" 2
                ;;
        esac
        shift
    done

    if [[ -n "$GATEWAY" ]]; then
        add_gateway
    fi

    write_configuration_to_file
}

main "$@"
#eof
