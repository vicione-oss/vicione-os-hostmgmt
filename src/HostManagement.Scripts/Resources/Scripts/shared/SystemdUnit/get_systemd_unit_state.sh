#!/usr/bin/env bash

# ###
#
# Get Systemd Unit State Script
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

readonly ERR_CODE_1="Unsupported flag '{1}'."
readonly ERR_CODE_2="No systemd unit name was specified."

#######################################
# Constants
#######################################

readonly ENABLED="Enabled"
readonly DISABLED="Disabled"

#######################################
# Variables
#######################################

SYSTEMD_UNIT_NAME=""
SYSTEMD_UNIT_STATE=$DISABLED

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --name <systemd-unit-name>"
    echo
    echo "Options:"
    echo "  --name - The name of the systemd unit."
    exit 0
}

get_systemd_unit_state() {
    # check if the systemd unit is permanently active; active means: autostart + running
    if systemctl is-active "$SYSTEMD_UNIT_NAME" &>/dev/null && systemctl is-enabled "$SYSTEMD_UNIT_NAME" &>/dev/null; then
        SYSTEMD_UNIT_STATE=$ENABLED
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    if [[ "$1" != "--name" ]]; then
        err "${ERR_CODE_1//\{1\}/$1}" 1
    fi

    if [[ -z "$2" ]]; then
        err "${ERR_CODE_2}" 2
    fi

    SYSTEMD_UNIT_NAME="$2"

    get_systemd_unit_state

    json_output="{"
    json_output+="\"Name\": \"${SYSTEMD_UNIT_NAME}\","
    json_output+="\"State\": \"${SYSTEMD_UNIT_STATE}\""
    json_output+="}"

    echo -e "${json_output}"
}

main "$@"
#eof
