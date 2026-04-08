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
        address=$(awk '{ if (NF==4) { print $3 } else { print "none" } }' <<< "$current")
        enabled="true"
        netplan_config="${DIRECTORY_NETPLAN}/50-${name}.yaml"

        if [[ -e "$netplan_config" ]] && grep -q "activation-mode: off" "$netplan_config"; then
            enabled="false"
        fi

        json+="{\"Enabled\": ${enabled}, \"Name\": \"${name}\", \"PhysicalAddress\": \"${address}\"}, "
    done

    echo "[ ${json::-2} ]"
}

main "$@"
#eof
