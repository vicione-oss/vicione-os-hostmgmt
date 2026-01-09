#!/usr/bin/env bash

# ###
#
# Get Service State Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Error codes
#######################################

readonly ERR_CODE_1="Unsupported flag '{1}'."
readonly ERR_CODE_2="No service name was specified."

#######################################
# Constants
#######################################

readonly ENABLED="Enabled"
readonly DISABLED="Disabled"

#######################################
# Variables
#######################################

SERVICE_NAME=""
SERVICE_STATE=$DISABLED

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --name <service-name>"
    echo
    echo "Options:"
    echo "  --name - The name of the service."
    exit 0
}

err() {
    echo -e "$1" >&2
    exit "$2"
}

get_service_state() {
    is_service_active=$(systemctl is-active "$SERVICE_NAME" 2>/dev/null)
    is_service_enabled=$(systemctl is-enabled "$SERVICE_NAME" 2>/dev/null)

    if [[ "${is_service_active}" == "active" ]] && [[ "${is_service_enabled}" == "enabled" ]]; then
        SERVICE_STATE=$ENABLED
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ "$1" == "--help" ]] || [[ "$1" == "-h" ]]; then
        usage
    fi

    if [[ "$1" != "--name" ]]; then
        err "${ERR_CODE_1//\{1\}/$1}" 1
    fi

    if [[ -z "$2" ]]; then
        err "${ERR_CODE_2}" 2
    fi

    SERVICE_NAME="$2"

    get_service_state

    json_output="{"
    json_output+="\"Name\": \"${SERVICE_NAME}\","
    json_output+="\"State\": \"${SERVICE_STATE}\""
    json_output+="}"

    echo -e "${json_output}"
}

main "$@"
#eof
