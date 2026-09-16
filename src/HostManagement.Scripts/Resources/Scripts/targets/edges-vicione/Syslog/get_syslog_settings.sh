#!/usr/bin/env bash

# ###
#
# Get Netlogd Settings Script
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
# Constants
#######################################

readonly CONFIGURATION_PATH="/etc/systemd/netlogd.conf.d/50-hostmanagement.conf"
readonly DEFAULT_PROTOCOL="udp"
readonly DEFAULT_LOG_FORMAT="rfc5424"
readonly DEFAULT_CONNECTION_RETRY_SEC="30"

#######################################
# Methods
#######################################

trim() {
    local value="$1"

    # Remove leading whitespace.
    value="${value#"${value%%[![:space:]]*}"}"

    # Remove trailing whitespace.
    value="${value%"${value##*[![:space:]]}"}"

    printf '%s' "$value"
}

json_escape() {
    local value="$1"

    # Only escape backslash and double quotes, the setter script is responsible
    # to prevent illegal charachters like tab etc. (whitespace is ok).
    value=${value//\\/\\\\}
    value=${value//\"/\\\"}

    printf '%s' "$value"
}

json_array() {
    local -a values=("$@")
    local json="["
    local separator=""
    local value

    for value in "${values[@]}"; do
        json+="${separator}\"$(json_escape "$value")\""
        separator=","
    done

    json+="]"

    printf '%s' "$json"
}

#######################################
# Argument handling
#######################################

main() {
    local enabled="false"
    local address=""
    local protocol="$DEFAULT_PROTOCOL"
    local log_format="$DEFAULT_LOG_FORMAT"
    local connection_retry_sec="$DEFAULT_CONNECTION_RETRY_SEC"
    local -a exclude_syslog_facilities=()
    local -a exclude_syslog_levels=()

    local section=""
    local line=""
    local key=""
    local value=""
    local line_enabled="true"

    if [[ -f "$CONFIGURATION_PATH" ]]; then
        while IFS= read -r line || [[ -n "$line" ]]; do
            line="$(trim "$line")"

            if [[ -z "$line" || "$line" == \;* ]]; then
                continue
            fi

            line_enabled="true"

            if [[ "$line" == \#* ]]; then
                line_enabled="false"
                line="${line#\#}"
                line="$(trim "$line")"
            fi

            # Keep track of the current section.
            if [[ "$line" == \[*\] ]]; then
                section="$line"
                continue
            fi

            # Only evaluate settings from the [Network] section.
            if [[ "$section" != "[Network]" || "$line" != *=* ]]; then
                continue
            fi

            key="$(trim "${line%%=*}")"
            value="$(trim "${line#*=}")"

            case "$key" in
                Address)
                    enabled="$line_enabled"
                    address="$value"
                    ;;
                Protocol)
                    protocol="$value"
                    ;;
                LogFormat)
                    log_format="$value"
                    ;;
                ConnectionRetrySec)
                    connection_retry_sec="$value"
                    ;;
                ExcludeSyslogFacility)
                    read -r -a exclude_syslog_facilities <<< "$value"
                    ;;
                ExcludeSyslogLevel)
                    read -r -a exclude_syslog_levels <<< "$value"
                    ;;
            esac
        done < "$CONFIGURATION_PATH"

        protocol="${protocol:-$DEFAULT_PROTOCOL}"
        log_format="${log_format:-$DEFAULT_LOG_FORMAT}"
        connection_retry_sec="${connection_retry_sec:-$DEFAULT_CONNECTION_RETRY_SEC}"
    fi

    local json_output="{"
    json_output+="\"Enabled\": ${enabled},"
    json_output+="\"Address\": "

    if [[ -n "$address" ]]; then
        json_output+="\"$(json_escape "$address")\""
    else
        json_output+="null"
    fi

    json_output+=",\"TransportProtocol\": \"$(json_escape "$protocol")\""
    json_output+=",\"LogFormat\": \"$(json_escape "$log_format")\""
    json_output+=",\"ConnectionRetrySeconds\": "$connection_retry_sec""
    json_output+=",\"ExcludeSyslogFacilities\": $(json_array "${exclude_syslog_facilities[@]}")"
    json_output+=",\"ExcludeSyslogLevels\": $(json_array "${exclude_syslog_levels[@]}")"
    json_output+="}"

    echo "$json_output"
}

main
#eof
