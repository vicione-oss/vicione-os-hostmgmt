#!/usr/bin/env bash

# ###
#
# Set Network Proxy Settings Script
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

readonly ERR_CODE_1="No URL was specified."
readonly ERR_CODE_2="No port number was specified."
readonly ERR_CODE_3="Unsupported flag '%s'."
readonly ERR_CODE_4="Unsupported proxy type."
readonly ERR_CODE_5="You must enter a username and a password for authentication."
readonly ERR_CODE_6="Missing argument for '%s'."

#######################################
# Constants
#######################################

readonly CONFIGURATION_PATH="/etc/environment"
readonly VALUE_TRUE="true"
readonly VALUE_FALSE="false"
readonly VALUE_NONE="NONE"

readonly PROXY_MARKER="# Proxy configuration"
readonly NO_PROXY_MARKER="# No Proxy configuration"

#######################################
# Methods
#######################################

usage() {
    local script_name="${0##*/}"

    echo "Usage: ${script_name}"
    echo "  --url <url> --port <port> [--username <username>] [--password <password>] --enabled <${VALUE_TRUE}|${VALUE_FALSE}> |"
    echo "  --no-proxy-list <${VALUE_NONE}|entry1,entry2,...> --no-proxy-list-enabled <${VALUE_TRUE}|${VALUE_FALSE}> |"
    echo "  --clear"
    echo
    echo "Options:"
    echo "  --url                   - The URL of the proxy server."
    echo "  --port                  - The port number of the proxy server."
    echo "  --username              - The username for authentication with the proxy server. (optional)"
    echo "  --password              - The password for authentication with the proxy server. (optional)"
    echo "  --enabled               - Enables or disables the given proxy server."
    echo "  --clear                 - Removes all proxy entries and related markers managed by this script."
    echo
    echo "  --no-proxy-list         - The no proxy list (comma-separated) for domains to bypass the proxy."
    echo "  --no-proxy-list-enabled - Enables or disables the no_proxy list (true/false)."
    echo
    echo "Examples:"
    echo "${script_name}"
    echo "  --url https://myproxy.de --port 1234 --username MyUser --password MyPassword --enabled true"
    echo
    echo "${script_name}"
    echo "  --no-proxy-list localhost,127.0.0.1 --no-proxy-list-enabled true"
    echo
    echo "${script_name}"
    echo "  --no-proxy-list ${VALUE_NONE} --no-proxy-list-enabled false"
    echo
    echo "${script_name}"
    echo "  --clear"
    exit 0
}

assert_flag_has_argument() {
    local argument_count="$1"
    local flag_argument="$2"
    local flag_name="$3"

    if [[ "$argument_count" -lt 2 || "$flag_argument" == --* ]]; then
        # shellcheck disable=SC2059
        err "$(printf "$ERR_CODE_6" "$flag_name")" 6
    fi
}

update_environment_file() {
    local proxy_type=$1
    local proxy_url=$2
    local proxy_enabled=$3
    local enabled_marker=""

    file_content=$(< "$CONFIGURATION_PATH")

    if [[ "$proxy_enabled" == "$VALUE_FALSE" ]]; then
        enabled_marker="#"
    fi

    if grep -q "$PROXY_MARKER" <<< "$file_content"; then
        if grep -q "^[#]*${proxy_type}_proxy=" <<< "$file_content"; then
            file_content=$(
                # shellcheck disable=SC2001
                sed "s|^[#]*${proxy_type}_proxy=.*|${enabled_marker}${proxy_type}_proxy=\"${proxy_url}\"|" <<< "$file_content")
        else
            file_content=$(
                # shellcheck disable=SC2001
                sed "s|${PROXY_MARKER}|${PROXY_MARKER}\n${enabled_marker}${proxy_type}_proxy=\"${proxy_url}\"|" <<< "$file_content")
        fi
    else
        file_content="${file_content}\n${PROXY_MARKER}\n${enabled_marker}${proxy_type}_proxy=\"${proxy_url}\""
    fi

    echo -e "$file_content" > "$CONFIGURATION_PATH"
}

update_no_proxy() {
    local no_proxy_list=$1
    local no_proxy_list_enabled=$2
    local enabled_marker=""

    file_content=$(< "$CONFIGURATION_PATH")

    if [[ "$no_proxy_list_enabled" == "$VALUE_FALSE" ]]; then
        enabled_marker="#"
    fi

    # Handle empty no_proxy list
    if [[ "$no_proxy_list" == "$VALUE_NONE" ]]; then
        # Remove the no_proxy entry if it exists
        if grep -q "no_proxy=" <<< "$file_content"; then
            file_content=$(sed "/no_proxy=/d" <<< "$file_content")
        fi
    else
        # Check if the no_proxy entry already exists and update it
        if grep -q "$NO_PROXY_MARKER" <<< "$file_content"; then
            if grep -q "^[#]*no_proxy=" <<< "$file_content"; then
                file_content=$(
                    # shellcheck disable=SC2001
                    sed "s|^[#]*no_proxy=.*|${enabled_marker}no_proxy=\"${no_proxy_list}\"|" <<< "$file_content")
            else
                file_content=$(
                    # shellcheck disable=SC2001
                    sed "s|${NO_PROXY_MARKER}|${NO_PROXY_MARKER}\n${enabled_marker}no_proxy=\"${no_proxy_list}\"|" <<< "$file_content")
            fi
        else
            file_content="${file_content}\n${NO_PROXY_MARKER}\n${enabled_marker}no_proxy=\"${no_proxy_list}\""
        fi
    fi

    echo -e "$file_content" > "$CONFIGURATION_PATH"
}

clear_proxy_settings() {
    file_content=$(< "$CONFIGURATION_PATH")

    file_content=$(sed -E \
        -e "/^${PROXY_MARKER}$/d" \
        -e "/^${NO_PROXY_MARKER}$/d" \
        -e '/^[#]*.*_proxy=.*/d' \
        <<< "$file_content")

    echo -e "$file_content" > "$CONFIGURATION_PATH"
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    local url=""
    local port=""
    local username=""
    local password=""
    local enabled="$VALUE_TRUE"
    local no_proxy_list=""
    local no_proxy_list_enabled="$VALUE_TRUE"

    while [[ "$#" -gt 0 ]]; do
        local flag_name="${1:-}"

        case "$flag_name" in
            --clear)
                clear_proxy_settings
                exit 0
                ;;

            --url|\
            --port|\
            --username|\
            --password|\
            --enabled|\
            --no-proxy-list|\
            --no-proxy-list-enabled)
                local flag_value="${2:-}"
                assert_flag_has_argument "$#" "$flag_value" "$flag_name"

                case "$flag_name" in
                    --url)                   url="$flag_value" ;;
                    --port)                  port="$flag_value" ;;
                    --username)              username="$flag_value" ;;
                    --password)              password="$flag_value" ;;
                    --enabled)               enabled="$flag_value" ;;
                    --no-proxy-list)         no_proxy_list="$flag_value" ;;
                    --no-proxy-list-enabled) no_proxy_list_enabled="$flag_value" ;;
                esac

                shift 2
                ;;

            *)
                # shellcheck disable=SC2059
                err "$(printf "$ERR_CODE_3" "$flag_name")" 3
                ;;
        esac
    done

    if [[ -n "$no_proxy_list" ]]; then
        update_no_proxy "$no_proxy_list" "$no_proxy_list_enabled"
        exit 0
    fi

    if [[ -z "$url" ]]; then
        err "$ERR_CODE_1" 1
    fi

    if [[ -z "$port" ]]; then
        err "$ERR_CODE_2" 2
    fi

    local proxy_type=""

    if [[ "$url" =~ ^http:// ]]; then
        proxy_type="http"
    elif [[ "$url" =~ ^https:// ]]; then
        proxy_type="https"
    elif [[ "$url" =~ ^ftp:// ]]; then
        proxy_type="ftp"
    elif [[ "$url" =~ ^sftp:// ]]; then
        proxy_type="sftp"
    elif [[ "$url" =~ ^socks5:// || "$url" =~ ^socks:// ]]; then
        proxy_type="socks5"
    else
        err "$ERR_CODE_4" 4
    fi

    local proxy_url="${url}:${port}"

    if [[ -n "$username" ]]; then
        if [[ -z "$password" ]]; then
            err "$ERR_CODE_5" 5
        fi

        proxy_url="${url/\/\//\/\/$username:$password@}:${port}"
    fi

    update_environment_file "$proxy_type" "$proxy_url" "$enabled"
}

main "$@"
#eof
