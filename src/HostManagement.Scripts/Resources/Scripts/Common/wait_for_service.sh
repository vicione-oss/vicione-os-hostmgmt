#!/usr/bin/env bash

# ###
#
# Wait For Service Script - v1.0.0
#
# The script waits until the specified service has been started.
# The check is performed a maximum of 2 times.
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

readonly ERR_CODE_1="The service '%s' is not in 'running' state."

#######################################
# Methods
#######################################

usage() {
    echo "The order of the parameters must be observed!"
    echo "Usage: $0 <name> [<timeout>]"
    exit 0
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    service_name=$1
    timeout="${2:-2}"
    counter=0

    while [ $counter -lt "$timeout" ]; do
        status=$(systemctl show -p SubState "$service_name" --value)

        if [[ "$status" == "running" ]]; then
            break;
        fi

        counter=$((counter + 1))
        sleep 1
    done

    if [[ $counter -eq "$timeout" ]]; then
        err "$(printf "$ERR_CODE_1" "$service_name")" 1
    fi
}

main "$@"
#eof
