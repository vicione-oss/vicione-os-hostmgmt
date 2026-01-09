#!/usr/bin/env bash

# ###
#
# Get Systemd Unit Active State Script
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

readonly ERR_CODE_1="Unsupported flag '%s'."
readonly ERR_CODE_2="No systemd unit name was specified."
readonly ERR_CODE_3="Error querying systemd unit '%s' (systemctl exit code: %s)."

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --name <systemd-unit-name>"
    echo
    echo "Options:"
    echo "  --name - The name of the systemd unit."
    exit 0
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    if [[ "$1" != "--name" ]]; then
        err "$(printf "$ERR_CODE_1" "$1")" 1
    fi

    if [[ -z "${2-}" ]]; then
        err "$ERR_CODE_2" 2
    fi

    local unit_name="$2"
    local status

    # Use systemctl status to distinguish between "inactive" and "does not exist".
    if systemctl status "$unit_name" >/dev/null 2>&1; then
        status=0
    else
        status=$?
    fi

    if [[ $status -eq 0 || $status -eq 3 ]]; then
        local activeState
        activeState=$(systemctl show -p ActiveState --value "$unit_name")
        echo "$activeState"
    elif [[ $status -eq 4 ]]; then
        echo "not-found"
    else
        err "$(printf "$ERR_CODE_3" "$unit_name" "$status")" 3
    fi
}

main "$@"
#eof
