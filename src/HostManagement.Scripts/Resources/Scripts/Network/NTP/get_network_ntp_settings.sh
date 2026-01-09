#!/usr/bin/env bash

# ###
#
# Get Network NTP Settings Script - v1.0.0
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
# Constants
#######################################

readonly DIRECTORY="/etc/systemd/timesyncd.conf.d"
readonly CONFIGURATION_PATH="${DIRECTORY}/50-hostmanagement.conf"

#######################################
# Methods
#######################################

get_fallback_ntp_servers() {
    timedatectl show-timesync --all | grep "FallbackNTPServers" | cut -d "=" -f2-
}

get_ntp_servers() {
    grep -E '^(#?NTP=)' "$CONFIGURATION_PATH" | sed 's/^#\?NTP=//' || true
}

#######################################
# Argument handling
#######################################

main() {
    IFS=' ' read -r -a fallback_ntp_array <<< "$(get_fallback_ntp_servers)"

    for fallback_ntp_server in "${fallback_ntp_array[@]}"; do
        fallback_ntp_servers+=("\"${fallback_ntp_server}\"")
    done

    if [[ -f "$CONFIGURATION_PATH" ]];
    then
        IFS=' ' read -r -a ntp_array <<< "$(get_ntp_servers)"

        for ntp_server in "${ntp_array[@]}"; do
            ntp_servers+=("\"${ntp_server}\"")
        done
    fi

    json_output="{"
    json_output+="\"FallbackNTPServers\": [$(printf '%s, ' "${fallback_ntp_servers[@]}" | sed 's/, $//')],"
    json_output+="\"NTPServers\": [$(printf '%s, ' "${ntp_servers[@]}" | sed 's/, $//')]"
    json_output+="}"

    echo "$json_output"
}

main
#eof
