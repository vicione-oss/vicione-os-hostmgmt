#!/usr/bin/env bash

# ###
#
# Set Static Hosts Script - v1.0.0
#
# The script sets the given static hosts to the /etc/hosts file.
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

readonly ERR_CODE_1="The file '%s' does not exists."
readonly ERR_CODE_2="Missing argument for '%s'."
readonly ERR_CODE_3="No IP address was specified."
readonly ERR_CODE_4="Unsupported flag '%s'."

#######################################
# Constants
#######################################

readonly HOSTS_FILE="/etc/hosts"
readonly SECTION_NAME="HostManagement"
readonly START_MARKER="# BEGIN ${SECTION_NAME}"
readonly END_MARKER="# END ${SECTION_NAME}"

#######################################
# Variables
#######################################

HOSTS_FILE_CONTENT=""

#######################################
# Methods
#######################################

usage() {
    echo "The order of the parameters must be observed!"
    echo "Usage: $0 --ip-address <ip> --hostname <hostname>"
    echo
    echo "Options:"
    echo "  --ip-address - Specifies the IP address to be mapped to the given hostname."
    echo "  --hostname   - Specifies the hostname to associate with the given IP address."
    echo
    echo "Examples:"
    echo "$0"
    echo "  --ip-address 192.168.1.1 --hostname myhost1.local"
    echo "  --ip-address 192.168.1.2 --hostname myhost2.local"
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
    if [[ "${1:-}" == "--help" || "${1:-}" == "-h" ]]; then
        usage
    fi

    if [[ ! -f $HOSTS_FILE ]]; then
        err "$(printf "$ERR_CODE_1" "$HOSTS_FILE")" 1
    fi

    load_current_hosts_file

    HOSTS_FILE_CONTENT+="$START_MARKER"$'\n'

    while (("$#")); do
        case "$1" in
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

                HOSTS_FILE_CONTENT+="${ip_address} ${hostname}"$'\n'

                ip_address=""
                hostname=""
                ;;
            *)
                err "$(printf "$ERR_CODE_4" "$1")" 4
                ;;
        esac

        shift 2
    done

    HOSTS_FILE_CONTENT+="$END_MARKER"$'\n'

    write_new_hosts_file
}

main "$@"
#eof
