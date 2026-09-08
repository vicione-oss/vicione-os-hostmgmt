#!/usr/bin/env bash

# ###
#
# Set Static Hosts Script
#
# The script sets the given static hosts to the /etc/hosts file.
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

readonly ERR_CODE_1="The file '%s' does not exists."
readonly ERR_CODE_2="Missing argument for '%s'."
readonly ERR_CODE_3="No IP address was specified."
readonly ERR_CODE_4="Unsupported flag '%s'."
readonly ERR_CODE_5="The --enabled parameter must appear first in the argument list."

#######################################
# Constants
#######################################

readonly HOSTS_FILE="/etc/hosts"
readonly SECTION_NAME="HostManagement"
readonly START_MARKER="# BEGIN ${SECTION_NAME}"
readonly END_MARKER="# END ${SECTION_NAME}"

readonly VALUE_TRUE="true"
readonly VALUE_FALSE="false"

readonly FLAG_ENABLED="--enabled"
readonly FLAG_IP_ADDRESS="--ip-address"
readonly FLAG_HOSTNAME="--hostname"

#######################################
# Variables
#######################################

HOSTS_FILE_CONTENT=""

#######################################
# Methods
#######################################

usage() {
    echo "The order of the parameters must be observed!"
    echo "Usage: $0 ${FLAG_ENABLED} <${VALUE_TRUE}|${VALUE_FALSE}> [${FLAG_IP_ADDRESS} <ip-address> ${FLAG_HOSTNAME} <hostname> ...]"
    echo
    echo "Options:"
    echo "  ${FLAG_ENABLED}    - Enables or disables the static hosts."
    echo "  ${FLAG_IP_ADDRESS} - Specifies the IP address to be mapped to the given hostname."
    echo "  ${FLAG_HOSTNAME}   - Specifies the hostname to associate with the given IP address."
    echo
    echo "Examples:"
    echo "$0"
    echo "  ${FLAG_ENABLED} ${VALUE_TRUE} ${FLAG_IP_ADDRESS} 192.168.1.1 ${FLAG_HOSTNAME} myhost1.local"
    echo "  ${FLAG_ENABLED} ${VALUE_TRUE} ${FLAG_IP_ADDRESS} 192.168.1.2 ${FLAG_HOSTNAME} myhost2.local"
    exit 0
}

assert_flag_has_argument() {
    local flag_argument="$1"
    local flag_name="$2"

    if [[ -z "$flag_argument" ]]; then
        err "$(printf "$ERR_CODE_2" "$flag_name")" 2
    fi
}

load_current_hosts_file() {
    HOSTS_FILE_CONTENT=$(< $HOSTS_FILE)
    HOSTS_FILE_CONTENT=$(awk "/${START_MARKER}/,/${END_MARKER}/ {next} {print}" <<< "$HOSTS_FILE_CONTENT")

    HOSTS_FILE_CONTENT+=$'\n'
}

write_new_hosts_file() {
    echo "$HOSTS_FILE_CONTENT" > $HOSTS_FILE
}

#######################################
# Argument handling
#######################################

main() {
    # TODO: Add flag --clear and check for empty parameter list to show usage
    if [[ $# -eq 0 || "${1:-}" == "--help" || "${1:-}" == "-h" ]]; then
        usage
    fi

    if [[ "${1:-}" != "${FLAG_ENABLED}" ]]; then
        err "$(printf "$ERR_CODE_5" "$HOSTS_FILE")" 5
    fi

    if [[ ! -f $HOSTS_FILE ]]; then
        err "$(printf "$ERR_CODE_1" "$HOSTS_FILE")" 1
    fi

    load_current_hosts_file

    local enabled_marker=""
    local static_hosts_content=""

    while (("$#")); do
        case "$1" in
            --enabled)
                assert_flag_has_argument "${2:-}" "$1"

                if [[ "$2" == "${VALUE_FALSE}" ]]; then
                    enabled_marker="#"
                fi
                ;;
            --ip-address)
                assert_flag_has_argument "${2:-}" "$1" 
                ip_address="$2"
                ;;
            --hostname)
                assert_flag_has_argument "${2:-}" "$1" 
                hostname="$2"

                if [[ -z "${ip_address:-}" ]]; then
                    err "$ERR_CODE_3" 3
                fi

                static_hosts_content+="${enabled_marker}${ip_address} ${hostname}"$'\n'

                ip_address=""
                hostname=""
                ;;
            *)
                err "$(printf "$ERR_CODE_4" "$1")" 4
                ;;
        esac

        shift 2
    done

    if [[ -n "$static_hosts_content" ]]; then
        HOSTS_FILE_CONTENT+="$START_MARKER"$'\n'
        HOSTS_FILE_CONTENT+="$static_hosts_content"
        HOSTS_FILE_CONTENT+="$END_MARKER"$'\n'
    fi

    write_new_hosts_file
}

main "$@"
#eof
