#!/usr/bin/env bash

# ###
#
# Set Network NTP Settings Script - v1.0.0
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
readonly ERR_CODE_2="No NTP server urls were specified."

#######################################
# Constants
#######################################

readonly DIRECTORY="/etc/systemd/timesyncd.conf.d"
readonly CONFIGURATION_PATH="${DIRECTORY}/50-hostmanagement.conf"

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --ntpservers <url> [<url> ...]"
    echo
    echo "Options:"
    echo "  --ntpservers - Specifies one or more NTP servers to configure."
    echo
    echo "Examples:"
    echo "$0"
    echo "  --ntpservers pool.ntp.org time.google.com"
    exit 0
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    if [[ "$1" != "--ntpservers" ]]; then
        err "$(printf "$ERR_CODE_1" "$1")" 1
    fi

    # TODO: Add argument 'none'
    #if [ $# -lt 2 ]; then
    #    err "$ERR_CODE_2" 2
    #fi

    local ntp_servers=()

    shift
    while [[ -n "${1:-}" ]]; do
        ntp_servers+=("$1")
        shift
    done

    if [[ ! -d "$DIRECTORY" ]]; then
        mkdir "$DIRECTORY"
    fi

    local file_content=""

    if [[ ${#ntp_servers[@]} -gt 0 ]]; then
        file_content+="[Time]\n"
        file_content+="NTP="
        file_content+=$(printf '%s ' "${ntp_servers[@]}" | sed 's/ $//')
    fi

    echo -e "$file_content" > "$CONFIGURATION_PATH"
}

main "$@"
#eof
