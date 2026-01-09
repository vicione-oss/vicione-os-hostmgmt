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

    networkctl reconfigure "$1"
}

main "$@"
#eof
