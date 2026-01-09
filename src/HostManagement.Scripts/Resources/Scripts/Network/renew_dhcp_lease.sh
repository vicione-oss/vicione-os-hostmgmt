#!/usr/bin/env bash

# ###
#
# Renew DHCP Lease Script
#
# The script returns 0 if ok or non zero for error.
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
# Methods
#######################################

usage() {
    echo "Usage: $0 <interface>"
    exit 0
}

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    INTERFACE_NAME="${1:-}"

    if ! Scripts/Network/Helper/is_network_interface_available.sh "$INTERFACE_NAME"; then
        err "$(printf "$ERR_CODE_1" "$INTERFACE_NAME")" 1
    fi

    networkctl reconfigure "$INTERFACE_NAME"
}

main "$@"
#eof
