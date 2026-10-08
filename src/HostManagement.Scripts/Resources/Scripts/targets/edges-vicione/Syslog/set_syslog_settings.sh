#!/usr/bin/env bash

# ###
#
# Set Netlogd Settings Script
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
# Error codes
#######################################

readonly ERR_CODE_1="Unsupported flag '%s'."
readonly ERR_CODE_2="Missing argument for '%s'."

#######################################
# Constants
#######################################

readonly DIRECTORY="/etc/systemd/netlogd.conf.d"
readonly CONFIG_FILE="50-hostmanagement.conf"
readonly CONFIGURATION_PATH="${DIRECTORY}/${CONFIG_FILE}"

readonly VALUE_TRUE="true"
readonly VALUE_FALSE="false"

readonly DEFAULT_PROTOCOL="udp"
readonly DEFAULT_LOG_FORMAT="rfc5424"
readonly DEFAULT_CONNECTION_RETRY_SEC="30"

readonly FLAG_ENABLED="--enabled"
readonly FLAG_ADDRESS="--address"
readonly FLAG_PROTOCOL="--protocol"
readonly FLAG_LOG_FORMAT="--log-format"
readonly FLAG_CONNECTION_RETRY_SEC="--connection-retry-sec"
readonly FLAG_EXCLUDE_SYSLOG_FACILITY="--exclude-syslog-facility"
readonly FLAG_EXCLUDE_SYSLOG_LEVEL="--exclude-syslog-level"
readonly FLAG_CLEAR="--clear"

#######################################
# Methods
#######################################

usage() {
    local script_name="${0##*/}"

    echo "Usage: ${script_name} [options]"
    echo
    echo "Options:"
    echo "  ${FLAG_ENABLED} <${VALUE_TRUE}|${VALUE_FALSE}>                   - Enables or disables the syslog forwarding. [default: ${VALUE_TRUE}]"
    echo "  ${FLAG_ADDRESS} <host:port>                  - Destination address and port (e.g.: 192.168.1.100:514)."
    echo "  ${FLAG_PROTOCOL} <udp|tcp>                   - Transport protocol. [default: ${DEFAULT_PROTOCOL}]"
    echo "  ${FLAG_LOG_FORMAT} <rfc5424|rfc3164>         - Syslog message format. [default: ${DEFAULT_LOG_FORMAT}]"
    echo "  ${FLAG_CONNECTION_RETRY_SEC} <seconds>       - Reconnect delay in seconds. [default: ${DEFAULT_CONNECTION_RETRY_SEC}]"
    echo "  ${FLAG_EXCLUDE_SYSLOG_FACILITY} <facilities> - Space-separated facility list to exclude. [default: None]"
    echo "                                           Allowed values are kern, user, mail, daemon, auth, syslog, lpr, news, uucp,"
    echo "                                           cron, authpriv, ftp, ntp, security, console, solaris-cron, local0-local7."
    echo "  ${FLAG_EXCLUDE_SYSLOG_LEVEL} <levels>        - Space-separated log level list to exclude. [default: None]"
    echo "                                           Allowed values are emerg, alert, crit, err, warning, notice, info, debug."
    echo "  ${FLAG_CLEAR}                                - Removes the configuration managed by this script."
    echo
    echo "Examples:"
    echo "${script_name} ${FLAG_ENABLED} true ${FLAG_ADDRESS} 192.168.1.100:514 ${FLAG_PROTOCOL} udp ${FLAG_LOG_FORMAT} rfc3164"
    echo "${script_name} ${FLAG_ENABLED} false ${FLAG_ADDRESS} 192.168.1.100:514 ${FLAG_PROTOCOL} tcp ${FLAG_CONNECTION_RETRY_SEC} 42"
    echo "${script_name} ${FLAG_ADDRESS} 192.168.1.100:514 ${FLAG_EXCLUDE_SYSLOG_FACILITY} auth authpriv"
    echo "${script_name} ${FLAG_CLEAR}"
    exit 0
}

assert_flag_has_argument() {
    local argument_count="$1"
    local flag_argument="$2"
    local flag_name="$3"

    if [[ "$argument_count" -lt 2 || "$flag_argument" == --* ]]; then
        # shellcheck disable=SC2059
        err "$(printf "$ERR_CODE_2" "$flag_name")" 2
    fi
}

clear_configuration() {
    rm -f -- "$CONFIGURATION_PATH"
}

write_configuration_content() {
    local enabled_marker="$1"
    local address="$2"
    local protocol="$3"
    local log_format="$4"
    local connection_retry_sec="$5"
    local exclude_syslog_facility="$6"
    local exclude_syslog_level="$7"

    cat <<EOF_CONFIG || return 1
#
# This file is managed by ViciOne open automation HostManagement application.
# Manual adjustments may not be compatible.
#
[Network]
${enabled_marker}Address=${address}
${enabled_marker}Protocol=${protocol}
${enabled_marker}LogFormat=${log_format}
${enabled_marker}ConnectionRetrySec=${connection_retry_sec}
EOF_CONFIG

    if [[ -n "$exclude_syslog_facility" ]]; then
        echo "${enabled_marker}ExcludeSyslogFacility=${exclude_syslog_facility}" || return 1
    fi

    if [[ -n "$exclude_syslog_level" ]]; then
        echo "${enabled_marker}ExcludeSyslogLevel=${exclude_syslog_level}" || return 1
    fi
}

set_configuration_ownership() {
    local temporary_path=$1

    if [[ -e "$CONFIGURATION_PATH" ]]; then
        chown --reference="$CONFIGURATION_PATH" -- "$temporary_path"
    fi
}

write_configuration() {
    local enabled="$1"
    local address="$2"
    local protocol="$3"
    local log_format="$4"
    local connection_retry_sec="$5"
    local exclude_syslog_facility="$6"
    local exclude_syslog_level="$7"
    local enabled_marker=""
    local temporary_path

    if [[ "$enabled" == "$VALUE_FALSE" ]]; then
        enabled_marker="#"
    fi

    mkdir -p -- "$DIRECTORY"
    temporary_path=$(mktemp "${DIRECTORY}/.${CONFIG_FILE}.XXXXXX") || exit 1

    if ! {
        write_configuration_content \
            "$enabled_marker" \
            "$address" \
            "$protocol" \
            "$log_format" \
            "$connection_retry_sec" \
            "$exclude_syslog_facility" \
            "$exclude_syslog_level" \
            > "$temporary_path" &&
        set_configuration_ownership "$temporary_path" &&
        chmod 0644 "$temporary_path" &&
        mv -f -- "$temporary_path" "$CONFIGURATION_PATH"
    }; then
        rm -f -- "$temporary_path"
        exit 1
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    local enabled="$VALUE_TRUE"
    local address=""
    local protocol="$DEFAULT_PROTOCOL"
    local log_format="$DEFAULT_LOG_FORMAT"
    local connection_retry_sec="$DEFAULT_CONNECTION_RETRY_SEC"
    local -a exclude_syslog_facilities=()
    local -a exclude_syslog_levels=()

    while [[ $# -gt 0 ]]; do
        local flag_name="${1:-}"

        case "$flag_name" in
            "$FLAG_CLEAR")
                if [[ $# -ne 1 ]]; then
                    # shellcheck disable=SC2059
                    err "$(printf "$ERR_CODE_1" "${2:-}")" 1
                fi

                clear_configuration
                exit 0
                ;;

            "$FLAG_ENABLED"|\
            "$FLAG_ADDRESS"|\
            "$FLAG_PROTOCOL"|\
            "$FLAG_LOG_FORMAT"|\
            "$FLAG_CONNECTION_RETRY_SEC")
                local flag_value="${2:-}"
                assert_flag_has_argument "$#" "$flag_value" "$flag_name"

                case "$flag_name" in
                    "$FLAG_ENABLED")              enabled="$flag_value" ;;
                    "$FLAG_ADDRESS")              address="$flag_value" ;;
                    "$FLAG_PROTOCOL")             protocol="$flag_value" ;;
                    "$FLAG_LOG_FORMAT")           log_format="$flag_value" ;;
                    "$FLAG_CONNECTION_RETRY_SEC") connection_retry_sec="$flag_value" ;;
                esac

                shift 2
                ;;

            "$FLAG_EXCLUDE_SYSLOG_FACILITY")
                shift

                if [[ $# -eq 0 || "${1:-}" == --* ]]; then
                    # shellcheck disable=SC2059
                    err "$(printf "$ERR_CODE_2" "$flag_name")" 2
                fi

                while [[ $# -gt 0 && "${1:-}" != --* ]]; do
                    exclude_syslog_facilities+=("$1")
                    shift
                done
                ;;

            "$FLAG_EXCLUDE_SYSLOG_LEVEL")
                shift

                if [[ $# -eq 0 || "${1:-}" == --* ]]; then
                    # shellcheck disable=SC2059
                    err "$(printf "$ERR_CODE_2" "$flag_name")" 2
                fi

                while [[ $# -gt 0 && "${1:-}" != --* ]]; do
                    exclude_syslog_levels+=("$1")
                    shift
                done
                ;;

            *)
                # shellcheck disable=SC2059
                err "$(printf "$ERR_CODE_1" "$flag_name")" 1
                ;;
        esac
    done

    local exclude_syslog_facility=""
    local exclude_syslog_level=""

    if [[ ${#exclude_syslog_facilities[@]} -gt 0 ]]; then
        exclude_syslog_facility="${exclude_syslog_facilities[*]}"
    fi

    if [[ ${#exclude_syslog_levels[@]} -gt 0 ]]; then
        exclude_syslog_level="${exclude_syslog_levels[*]}"
    fi

    write_configuration \
        "$enabled" \
        "$address" \
        "$protocol" \
        "$log_format" \
        "$connection_retry_sec" \
        "$exclude_syslog_facility" \
        "$exclude_syslog_level"
}

main "$@"
#eof
