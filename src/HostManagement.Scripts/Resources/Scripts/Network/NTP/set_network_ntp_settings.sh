#!/usr/bin/env bash

# ###
#
# Set Network NTP Settings Script
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

readonly ERR_CODE_1="Unsupported flag '%s'."
readonly ERR_CODE_2="Missing arguments for '%s'."
readonly ERR_CODE_3="No NTP server urls were specified."

#######################################
# Constants
#######################################

readonly DIRECTORY="/etc/systemd/timesyncd.conf.d"
readonly CONFIGURATION_PATH="${DIRECTORY}/50-hostmanagement.conf"

readonly FLAG_NTPSERVERS_ENABLED="--ntpservers-enabled"
readonly FLAG_NTPSERVERS="--ntpservers"

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0"
    echo "  [${FLAG_NTPSERVERS_ENABLED} <true|false>]"
    echo "  ${FLAG_NTPSERVERS} <url|none> [<url> ...]"
    echo
    echo "Options:"
    echo "  ${FLAG_NTPSERVERS_ENABLED} - Enables or disables the NTP servers. (optional)"
    echo "  ${FLAG_NTPSERVERS}         - Specifies one or more NTP servers to configure."
    echo
    echo "Examples:"
    echo "$0"
    echo "  ${FLAG_NTPSERVERS_ENABLED} true"
    echo "  ${FLAG_NTPSERVERS} pool.ntp.org time.google.com"
    exit 0
}

assert_flag_has_arguments() {
    local argument_count="$1"
    local first_argument="$2"
    local flag_name="$3"

    if [[ "$argument_count" -eq 0 || "$first_argument" == --* ]]; then
        err "$(printf "$ERR_CODE_2" "$flag_name")" 2
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    local ntpservers_enabled_marker=""
    local ntp_servers=()

    while [[ $# -gt 0 ]]; do
        flag_name="$1"
        shift

        case "$flag_name" in
            "$FLAG_NTPSERVERS_ENABLED" )
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                if [[ $1 == "false" ]]; then
                    ntpservers_enabled_marker="#"
                fi
                shift
                ;;
            "$FLAG_NTPSERVERS" )
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                if [[ "${1:-}" == "none" ]]; then
                    shift
                else
                    while [[ -n "${1:-}" && "${1:-}" != -* ]]; do
                        if [[ "${1:-}" != "none" ]]; then
                            ntp_servers+=("$1")
                        fi
                        shift
                    done
                fi
                ;;
            *)
                err "$(printf "$ERR_CODE_1" "$flag_name")" 1
                ;;
        esac
    done

    if [[ ! -d "$DIRECTORY" ]]; then
        mkdir "$DIRECTORY"
    fi

    local file_content=""

    if [[ ${#ntp_servers[@]} -gt 0 ]]; then
        file_content+="[Time]\n"
        file_content+="${ntpservers_enabled_marker}NTP="
        file_content+=$(printf '%s ' "${ntp_servers[@]}" | sed 's/ $//')
    fi

    echo -e "$file_content" > "$CONFIGURATION_PATH"
}

main "$@"
#eof
