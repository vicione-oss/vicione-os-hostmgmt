#!/usr/bin/env bash

# ###
#
# Is Url Reachable Script
#
# The script checks whether the url is accessible.
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

readonly ERR_CODE_1="The ViciOne Suite repository could not be reached."
readonly ERR_CODE_2="Could not access ViciOne Suite repository. wget exit code: %s"
readonly ERR_CODE_3="The ViciOne Suite repository returned an unexpected status. HTTP-Code: %s"

#######################################
# Argument handling
#######################################

main() {
    local url="${1:-}"
    local username="${2:-}"
    local password="${3:-}"

    status=$(wget --timeout=5 --tries=3 --spider --server-response --user="$username" --password="$password" "$url" 2>&1 || true)
    status_code=$?

    if [[ "$status" != *"HTTP/"* ]]; then
        err "$ERR_CODE_1" 1
    elif [[ $status_code -ne 0 ]]; then
        err "$(printf "$ERR_CODE_2" "$status_code")" 2
    fi

    status=$(grep "HTTP/" <<< "$status" | awk '{print $2}' | tail -n 1 || true)

    if [[ "$status" -ne 200 ]]; then
        err "$(printf "$ERR_CODE_3" "$status")" 3
    fi
}

main "$@"
#eof
