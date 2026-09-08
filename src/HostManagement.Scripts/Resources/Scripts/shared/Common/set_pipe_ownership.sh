#!/usr/bin/env bash

# ###
#
# Set Ownership Script
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
readonly ERR_CODE_3="Setting ownership for '%s' failed."

#######################################
# Variables
#######################################

PIPE_USER="${HOSTMANAGEMENT__PIPE__USER:-vicione-suite}"
PIPE_GROUP="${HOSTMANAGEMENT__PIPE__GROUP:-vicione-suite}"

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --file <file>"
    echo
    echo "Options:"
    echo "  --file  - Path to the file whose ownership should be changed."
    exit 0
}

set_ownership() {
    local user=$1
    local group=$2
    local file=$3

    if ! chown "${user}:${group}" "$file" &>/dev/null; then
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

    local user="$PIPE_USER"
    local group="$PIPE_GROUP"

    if ! id "$user" &>/dev/null; then
        user="root"
    fi

    if ! getent group "$group" &>/dev/null; then
        group="root"
    fi

    set_ownership "$user" "$group" "$2"
}

main "$@"
#eof
