#!/usr/bin/env bash

# ###
#
# Set Execute Permission Script
#
# The script returns 0 if ok, non zero for error.
# ###

# shellcheck disable=SC2059

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
readonly ERR_CODE_2="No file was specified."
readonly ERR_CODE_3="Setting permissions for '%s' failed."

#######################################
# Variables
#######################################

PIPE_PERMISSION="${HOSTMANAGEMENT__PIPE__CHMOD:-664}"

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --file <file>"
    echo
    echo "Options:"
    echo "  --file - The file to which the configured permissions should be assigned."
    echo
    echo "Environment variables:"
    echo "  HOSTMANAGEMENT__PIPE__CHMOD - Permission to apply (default: 664)."
    exit 0
}

set_permission() {
    local permission=$1
    local file=$2

    if ! chmod "$permission" "$file" &>/dev/null; then
        err "$(printf "$ERR_CODE_3" "$file")" 3
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    if [[ "$1" != "--file" ]]; then
        err "$(printf "$ERR_CODE_1" "$1")" 1
    fi

    if [[ -z "${2:-}" ]]; then
        err "$ERR_CODE_2" 2
    fi

    set_permission "$PIPE_PERMISSION" "$2"
}

main "$@"
#eof
