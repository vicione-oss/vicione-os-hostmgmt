#!/usr/bin/env bash

# ###
#
# Set Ownership Script
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
readonly ERR_CODE_3="No group was specified."
readonly ERR_CODE_4="No file was specified."
readonly ERR_CODE_5="Setting ownership for '%s' failed."

#######################################
# Methods
#######################################

usage() {
    echo "The order of the parameters must be observed!"
    echo "Usage: $0 --user <user> --group <group> --file <file>"
    echo
    echo "Options:"
    echo "  --user  - Specifies the owner (user) of the file."
    echo "  --group - Specifies the group for the file."
    echo "  --file  - Path to the file whose ownership should be changed."
    exit 0
}

set_ownership() {
    local user=$1
    local group=$2
    local file=$3

    if ! chown "${user}:${group}" "$file" &>/dev/null; then
        err "$(printf "$ERR_CODE_5" "$file")" 5
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

    if [[ $# -lt 3 ]]; then
        err "$ERR_CODE_3" 3
    fi

    if [[ "$3" != "--group" ]]; then
        err "$(printf "$ERR_CODE_1" "$3")" 1
    fi

    if [[ -z "${4:-}" ]]; then
        err "$ERR_CODE_3" 3
    fi

    if [[ $# -lt 5 ]]; then
        err "$ERR_CODE_4" 4
    fi

    if [[ "$5" != "--file" ]]; then
        err "$(printf "$ERR_CODE_1" "$5")" 1
    fi

    if [[ -z "${6:-}" ]]; then
        err "$ERR_CODE_4" 4
    fi

    set_ownership "$2" "$4" "$6"
}

main "$@"
#eof
