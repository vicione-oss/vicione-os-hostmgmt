#!/usr/bin/env bash

# ###
#
# Get Service State Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --name <service_name>"
    exit "$1"
}

#######################################
# Constants
#######################################

readonly ENABLED="Enabled"
readonly DISABLED="Disabled"

#######################################
# Argument handling
#######################################

main() {
    if [[ "$1" == "--help" ]] || [[ "$1" == "-h" ]]; then
        usage 0
    fi

    # Validate parameters
    if [[ "$1" != "--name" ]] || [[ -z "$2" ]]; then
        usage 1
    fi

    service_name="$2"

    # Check the status of the service
    service_status=$(systemctl is-active "$service_name" 2>/dev/null)

    if [[ "${service_status}" == "active" ]]; then
        service_status=$ENABLED
    else
        service_status=$DISABLED
    fi

    json_output="{"
    json_output+="\"Name\": \"${service_name}\","
    json_output+="\"State\": \"${service_status}\""
    json_output+="}"

    echo -e "${json_output}"
}

main "$@"
#eof
