#!/usr/bin/env bash

# ###
#
# Check User Exists Script - v1.0.0
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
readonly ERR_CODE_2="No user was specified."
readonly ERR_CODE_3="User '%s' does not exist."

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --user <user>"
    echo
    echo "Options:"
    echo "  --user - The name of the user whose existence is to be checked."
    exit 0
}

check_user_exists() {
    local user=$1

    if ! id "$user" &>/dev/null; then
        err "$(printf "$ERR_CODE_3" "$user")" 3
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    if [[ "$1" != "--user" ]]; then
        err "$(printf "$ERR_CODE_1" "$1")" 1
    fi

    if [[ -z "${2:-}" ]]; then
        err "$ERR_CODE_2" 2
    fi

    check_user_exists "$2"
}

main "$@"
#eof
