#!/usr/bin/env bash

# ###
#
# Get Network Proxy Settings Script
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

readonly CONFIGURATION_PATH="/etc/environment"

#######################################
# Argument handling
#######################################

main() {
    local json_output="{"

    while IFS= read -r row
    do
        enabled="true"

        if [[ $row =~ ^# ]]; then
            enabled="false"
        fi

        proxy=$(echo "$row" | cut -d'=' -f2 | tr -d '"')

        if [[ $proxy =~ ^(http|https|ftp|sftp|socks|socks5):\/\/(([^:]+):([^@]+)@)?([^:]+):(.+)$ ]]; then
            proxy_type="${BASH_REMATCH[1]}"
            username="${BASH_REMATCH[3]}"
            password="${BASH_REMATCH[4]}"
            url="${BASH_REMATCH[5]}"
            port="${BASH_REMATCH[6]}"

            if [[ "$proxy_type" == "socks5" ]]; then
                proxy_type="socks"
            fi

            json_output+="\"${proxy_type^^}\": {"
            json_output+="\"Enabled\": ${enabled},"
            json_output+="\"Server\": \"${proxy_type}://${url}\","
            json_output+="\"Port\": ${port},"
            json_output+="\"Username\": "

            if [[ -n "$username" ]]; then
                json_output+="\"${username}\","
            else
                json_output+="null,"
            fi

            json_output+="\"Password\": "
            
            if [[ -n "$password" ]]; then
                json_output+="\"${password}\""
            else
                json_output+="null"
            fi

            json_output+="},"
        fi
    done < <(grep '_proxy=' "$CONFIGURATION_PATH" || true)

    # Extract the no_proxy/NO_PROXY variable and check its state
    no_proxy_entry=$(grep -Ei '^#*no_proxy=' "$CONFIGURATION_PATH" || true)
    no_proxy_list=$(echo "$no_proxy_entry" | cut -d'=' -f2 | tr -d '"' | tr ',' ' ')

    # Determine if no_proxy is enabled or disabled
    if [[ -z "$no_proxy_entry" || "$no_proxy_entry" =~ ^# ]]; then
        no_proxy_enabled="false"
    else
        no_proxy_enabled="true"
    fi

    # Add NoProxySettings JSON output
    json_output+="\"NoProxy\": {"
    json_output+="  \"Enabled\": ${no_proxy_enabled},"
    json_output+="  \"Entries\": ["

    if [[ -n "$no_proxy_list" ]]; then
        for host in $no_proxy_list; do
            json_output+="\"${host}\","
        done
    fi

    # Remove the trailing comma, if any, and close the JSON object
    json_output="${json_output%,}"
    json_output+="]}}"

    echo -e "$json_output"
}

main
#eof
