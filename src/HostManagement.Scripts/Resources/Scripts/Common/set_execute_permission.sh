#!/usr/bin/env bash

# ###
#
# Set Execute Permission Script - v1.0.0
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
readonly ERR_CODE_2="No permission was specified."
readonly ERR_CODE_3="No file was specified."
readonly ERR_CODE_4="Setting permissions for '%s' failed."

#######################################
# Methods
#######################################

usage() {
    echo "The order of the parameters must be observed!"
    echo "Usage: $0 --permission <permission> --file <file>"
    echo
    echo "Options:"
    echo "  --permission - The permissions to be applied to the specified file."
    echo "  --file       - The file to which the specified permissions should be assigned."
    echo
    echo "Examples:"
    echo "$0 --permission 755 --file file.txt"
    echo "$0 --permission u=rwx,g=rx,o=rx --file file.txt"
    exit 0
}

set_permission() {
    local permission=$1
    local file=$2

    if ! chmod "$permission" "$file" &>/dev/null; then
        err "$(printf "$ERR_CODE_4" "$file")" 4
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    if [[ "$1" != "--permission" ]]; then
        err "$(printf "$ERR_CODE_1" "$1")" 1
    fi

    if [[ -z "${2:-}" ]]; then
        err "$ERR_CODE_2" 2
    fi

    if [[ $# -lt 3 ]]; then
        err "$ERR_CODE_3" 3
    fi

    if [[ "$3" != "--file" ]]; then
        err "$(printf "$ERR_CODE_1" "$3")" 1
    fi

    if [[ -z "${4:-}" ]]; then
        err "$ERR_CODE_3" 3
    fi

    set_permission "$2" "$4"
}

main "$@"
#eof
