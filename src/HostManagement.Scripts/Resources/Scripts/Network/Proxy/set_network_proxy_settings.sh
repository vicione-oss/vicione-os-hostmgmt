#!/usr/bin/env bash

# ###
#
# Set Network Proxy Settings Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Variables
#######################################

CONFIGURATION_PATH="/etc/environment"

#######################################
# Methods
#######################################

err() {
    echo -e "$1" >&2
    exit "$2"
}

update_environment_file() {
    local proxy_type=$1
    local proxy_url=$2
    local proxy_enabled=$3
    local enabled_marker=""
    local comment="# Proxy configuration"
    
    file_content=$(< "${CONFIGURATION_PATH}")

    if [[ "${proxy_enabled}" == "false" ]]; then
        enabled_marker="#"
    fi
    
    if echo "${file_content}" | grep -q "${comment}"; then
        if echo "${file_content}" | grep -q "^[#]*${proxy_type}_proxy="; then
            file_content=$(echo "${file_content}" | sed "s|^[#]*${proxy_type}_proxy=.*|${enabled_marker}${proxy_type}_proxy=\"${proxy_url}\"|")
        else
            file_content=$(echo "${file_content}" | sed "s|${comment}|${comment}\n${enabled_marker}${proxy_type}_proxy=\"${proxy_url}\"|")
        fi
    else
        file_content="${file_content}\n${comment}\n${enabled_marker}${proxy_type}_proxy=\"${proxy_url}\""
    fi

    echo -e "${file_content}" > "${CONFIGURATION_PATH}"
}

update_no_proxy() {
    local no_proxy_list=$1
    local no_proxy_list_enabled=$2
    local enabled_marker=""
    local comment="# No Proxy configuration"
    local file_content=$(< "${CONFIGURATION_PATH}")

    if [[ "${no_proxy_list_enabled}" == "false" ]]; then
        enabled_marker="#"
    fi

    # Handle empty no_proxy list
    if [[ "${no_proxy_list}" == "NONE" ]]; then
        # Remove the no_proxy entry if it exists
        if echo "${file_content}" | grep -q "no_proxy="; then
            file_content=$(echo "${file_content}" | sed "/no_proxy=/d")
        fi
    else
        # Check if the no_proxy entry already exists and update it
        if echo "${file_content}" | grep -q "${comment}"; then
            if echo "${file_content}" | grep -q "^[#]*no_proxy="; then
                file_content=$(echo "${file_content}" | sed "s|^[#]*no_proxy=.*|${enabled_marker}no_proxy=\"${no_proxy_list}\"|")
            else
                file_content=$(echo "${file_content}" | sed "s|${comment}|${comment}\n${enabled_marker}no_proxy=\"${no_proxy_list}\"|")
            fi
        else
            file_content="${file_content}\n${comment}\n${enabled_marker}no_proxy=\"${no_proxy_list}\""
        fi
    fi

    echo -e "$file_content" > "$CONFIGURATION_PATH"
}

#######################################
# Argument handling
#######################################

main() {
    if [[ "${1}" == "--help" ]] || [[ "${1}" == "-h" ]]; then
        echo "Usage: ${0} [Option]..."
        echo ""
        echo "Options:"
        echo "  --url                   - The URL of the proxy server."
        echo "  --port                  - The port number of the proxy server."
        echo "  --username              - The username for authentication with the proxy server. (Optional)"
        echo "  --password              - The password for authentication with the proxy server. (Optional)"
        echo "  --enabled               - Enables or disables the given proxy server."
        echo "  --no-proxy-list         - The no_proxy list (comma-separated) for domains to bypass the proxy."
        echo "  --no-proxy-list-enabled - Enables or disables the no_proxy list (true/false)."

        exit 0
    fi

    local url=""
    local port=""
    local username=""
    local password=""
    local enabled="true"
    local no_proxy_list=""
    local no_proxy_list_enabled="true"

    while [[ "$#" -gt 0 ]]; do
        case $1 in
            --url)                   url="$2";                   shift;;
            --port)                  port="$2";                  shift;;
            --username)              username="$2";              shift;;
            --password)              password="$2";              shift;;
            --enabled)               enabled="$2";               shift;;
            --no-proxy-list)         no_proxy_list="$2";         shift;;
            --no-proxy-list-enabled) no_proxy_list_enabled="$2"; shift;;
        esac
        shift
    done

    if [[ -n "${no_proxy_list}" ]]; then
        update_no_proxy "${no_proxy_list}" "${no_proxy_list_enabled}"
        exit 0
    fi

    if [[ -z "${url}" ]] || [[ -z "${port}" ]]; then
        err "Please enter all required parameters." 1
    fi

    local proxy_type=""

    if [[ "${url}" =~ ^http:// ]]; then
        proxy_type="http"
    elif [[ "${url}" =~ ^https:// ]]; then
        proxy_type="https"
    elif [[ "${url}" =~ ^ftp:// ]]; then
        proxy_type="ftp"
    elif [[ "${url}" =~ ^sftp:// ]]; then
        proxy_type="sftp"
    elif [[ "${url}" =~ ^socks5:// ]] || [[ "${url}" =~ ^socks:// ]]; then
        proxy_type="socks5"
    else
        err "Unsupported proxy type." 2
    fi

    local proxy_url="${url}:${port}"

    if [[ -n "${username}" ]]; then
        if [[ -z "${password}" ]]; then
            err "You must enter a username and a password for authentication." 3 
        fi

        proxy_url="${url/\/\//\/\/$username:$password@}:${port}"
    fi

    update_environment_file "${proxy_type}" "${proxy_url}" "${enabled}"
}

main "$@"
#eof
