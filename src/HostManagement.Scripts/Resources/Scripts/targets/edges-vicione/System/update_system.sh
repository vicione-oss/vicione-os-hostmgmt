#!/usr/bin/env bash

# ###
#
# Update System Script
#
# This script is intended for internal use only.
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
readonly ERR_CODE_2="No SWU update file was specified."
readonly ERR_CODE_3="The specified update file '%s' does not exist."
readonly ERR_CODE_4="CRC checksum verification failed for '%s'."
readonly ERR_CODE_5="Update directory not set."
readonly ERR_CODE_6="Update directory '%s' does not exist."
readonly ERR_CODE_7="Update directory '%s' could not be cleaned."
readonly ERR_CODE_8="The specified file '%s' could not be moved."
readonly ERR_CODE_9="fw_setenv command not found."
readonly ERR_CODE_10="Failed to set U-Boot recovery_request flag."

#######################################
# Variables
#######################################

UPDATE_DIRECTORY="${HOSTMANAGEMENT__UPDATE_DIRECTORY:-}"
UPDATE_FILE=""

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --file <update-file>"
    echo
    echo "Options:"
    echo "  --file - The SWU update file."
    exit 0
}

validate_update_file() {
    # Check if the update file exists
    if [[ ! -f "${UPDATE_FILE}" ]]; then
        err "$(printf "$ERR_CODE_3" "$UPDATE_FILE")" 3
    fi

    # Check if file is valid by checking its CRC checksum
    if ! cpio -i --only-verify-crc < "${UPDATE_FILE}" &>/dev/null; then
        err "$(printf "$ERR_CODE_4" "$UPDATE_FILE")" 4
    fi
}

check_update_directory_set() {
    if [[ -z "$UPDATE_DIRECTORY" ]]; then
        err "$(printf "$ERR_CODE_5")" 5
    fi
}

clean_up_update_directory() {
    if [[ ! -d "${UPDATE_DIRECTORY}" ]]; then
        err "$(printf "$ERR_CODE_6" "$UPDATE_DIRECTORY")" 6
    fi

    if ! rm -rf "${UPDATE_DIRECTORY:?}"/*; then
        err "$(printf "$ERR_CODE_7" "$UPDATE_DIRECTORY")" 7
    fi
}

move_update_file_to_update_directory() {
    if ! mv "${UPDATE_FILE:?}" "${UPDATE_DIRECTORY}/"; then
        err "$(printf "$ERR_CODE_8" "$UPDATE_FILE")" 8
    fi
}

set_u_boot_flag() {
    if ! command -v fw_setenv >/dev/null 2>&1; then
        err "$(printf "$ERR_CODE_9")" 9
    fi

    if ! fw_setenv recovery_request 1; then
        err "$(printf "$ERR_CODE_10")" 10
    fi
}

#######################################
# Argument handling
#######################################

main() {
    local option="${1:-}"
    local value="${2:-}"

    if [[ $# -eq 0 || "$option" == "--help" || "$option" == "-h" ]]; then
        usage
    fi

    if [[ "$option" != "--file" ]]; then
        err "$(printf "$ERR_CODE_1" "$option")" 1
    fi

    if [[ -z "$value" ]]; then
        err "$(printf "$ERR_CODE_2")" 2
    fi

    UPDATE_FILE="$value"

    validate_update_file
    check_update_directory_set
    clean_up_update_directory
    move_update_file_to_update_directory
    set_u_boot_flag

    # System restart is required if script called manually!
}

main "$@"
# eof
