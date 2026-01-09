#!/usr/bin/env bash

# ###
#
# Check Group Exists Script
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
readonly ERR_CODE_2="No group was specified."
readonly ERR_CODE_3="Group '%s' does not exist."

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --group <group>"
    echo
    echo "Options:"
    echo "  --group - The name of the group whose existence is to be checked."
    exit 0
}

check_group_exists() {
    local group=$1

    if ! getent group "$group" &>/dev/null; then
        err "$(printf "$ERR_CODE_3" "$group")" 3
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    if [[ "$1" != "--group" ]]; then
        err "$(printf "$ERR_CODE_1" "$1")" 1
    fi

    if [[ -z "${2:-}" ]]; then
        err "$ERR_CODE_2" 2
    fi

    check_group_exists "$2"
}

main "$@"
#eof
