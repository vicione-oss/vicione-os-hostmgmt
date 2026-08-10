#!/usr/bin/env bash

# ###
#
# Get Network Interfaces Common Information Script
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

readonly ERR_CODE_1="Unsupported flag '%s'."
readonly ERR_CODE_2="Missing argument for '%s'."

#######################################
# Constants
#######################################

readonly DIRECTORY_NETPLAN="/etc/netplan"

readonly FLAG_INTERFACES="--interfaces"

#######################################
# Methods
#######################################

warn() {
    echo -e "$1" >&2
}

usage() {
    echo "Usage: $0 ${FLAG_INTERFACES} <interfaces>"
    echo
    echo "Options:"
    echo "  ${FLAG_INTERFACES} - The interfaces with format as returned from 'ip -br link show'."
    exit 0
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    if [[ "$1" != "${FLAG_INTERFACES}" ]]; then
        err "$(printf "$ERR_CODE_1" "$1")" 1
    fi

    if [[ -z "${2:-}" ]]; then
        err "$(printf "$ERR_CODE_2" "${FLAG_INTERFACES}")" 2
    fi

    local network_interfaces=$2

    if [[ -z "$network_interfaces" ]]; then
        warn "No network interfaces have been found."
        echo -e "[ ]"
        exit 0
    fi

    mapfile -t network_interfaces_array <<< "$network_interfaces"

    for current in "${network_interfaces_array[@]}"; do
        name=${current%% *}
        user_defined_mac_address=""
        user_defined_mac_address_enabled="false"
        enabled="true"
        netplan_config="${DIRECTORY_NETPLAN}/50-${name}.yaml"

        if [[ -e "$netplan_config" ]]; then
            netplan_config_content=$(< "$netplan_config")

            if grep -q "activation-mode: off" <<< "$netplan_config_content"; then
                enabled="false"
            fi

            mac_address_information=$(sed -n "/^[[:space:]]*${name}:/,/macaddress:/p" <<< "$netplan_config_content")
            user_defined_mac_address=$(grep -w "macaddress:" <<< "$mac_address_information" | sed -E 's/.*(#|macaddress:)[[:space:]]*//')
            original_mac_address=$(./Scripts/Network/get_original_physical_address.sh "$name")

            if [[ "$user_defined_mac_address" == "$original_mac_address" ]]; then
                user_defined_mac_address=""
            elif [[ "$mac_address_information" != *macaddress:*#* ]]; then
                user_defined_mac_address_enabled="true"
            fi
        fi

        json+="{"
        json+="\"Enabled\": ${enabled},"
        json+="\"Name\": \"${name}\","
        json+="\"UserDefinedMACAddress\": "
        json+="{"
        json+="  \"Enabled\": ${user_defined_mac_address_enabled},"
        json+="  \"Address\": \"${user_defined_mac_address}\""
        json+="}"
        json+="}, "
    done

    echo "[ ${json::-2} ]"
}

main "$@"
#eof
