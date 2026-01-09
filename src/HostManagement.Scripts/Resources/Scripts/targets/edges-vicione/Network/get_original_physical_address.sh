#!/usr/bin/env bash

# ###
#
# Get Original Physical Address Script
#
# The script returns the physical address of the given network interface and exit code 0.
# If the script fails it returns a non zero error code.
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

readonly ERR_CODE_1="The specified interface was not defined via U-Boot."
readonly ERR_CODE_2="The physical MAC address could not be determined."

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 <interface_name>"
    exit 0
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    local interface_name="${1:-}"
    local mac_address=""

    if command -v fw_printenv >/dev/null ; then
        # The data refers to the U-boot configuration of the respective platform.
        # Currently only the VHIP4 platform is supported.
        # TODO: Extend for further platforms

        case "$interface_name" in
            lan1) eth="ethaddr"         ;; # VHIP4
            lan2) eth="eth1addr"        ;; # VHIP4
            *)    err "$ERR_CODE_1" 1 ;;
        esac

        mac_address=$(fw_printenv "$eth" 2>/dev/null | cut -d= -f2)

        if [[ -z "$mac_address" ]]; then
            err "$ERR_CODE_2" 2
        fi
    else
        # Has been added for virtual machines to be able to perform tests.

        if ! mac_address=$(ip link show "$interface_name" 2>/dev/null | grep -oP 'permaddr \K[^ ]+'); then
            if ! mac_address=$(ip link show "$interface_name" 2>/dev/null | grep -oP 'link/ether \K[^ ]+'); then
                err "$ERR_CODE_2" 2
            fi
        fi
    fi

    echo "$mac_address"
}

main "$@"
#eof
