#!/usr/bin/env bash

# ###
#
# Shutdown System Script
#
# The script can be used to shutdown or restart the system.
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

readonly ERR_CODE_1="Missing argument for '%s'."
readonly ERR_CODE_2="Unsupported flag '%s'."
readonly ERR_CODE_3="The delay must be a positive integer number."

#######################################
# Methods
#######################################

usage() {
    echo "Usage: ${0} [--restart] [--delay <delay-in-seconds>]"
    echo
    echo "Options:"
    echo "  --restart - Shutdown and restart the system. (optional)"
    echo "  --delay   - The delay in seconds. (optional, must be > 0 if specified)"
    exit 0
}

assert_flag_has_argument() {
    local flag_name="$1"
    local argument="$2"
    local argument_count="$3"

    if [[ "$argument_count" -lt 2 || "$argument" == --* ]]; then
        err "$(printf "$ERR_CODE_1" "$flag_name")" 1
    fi
}

validate_positive_integer() {
    local flag_name="$1"
    local value="$2"

    # Checks: only positve numbers, no leading +/-, no decimal points
    if ! [[ "$value" =~ ^[0-9][0-9]*$ ]]; then
        err "$(printf "$ERR_CODE_3" "$flag_name")" 3
    fi
}

schedule_shutdown() {
    local restart_flag="$1"
    local delay_seconds="$2"

    if [[ "$delay_seconds" -gt 0 ]]; then
        local message="Shutdown will be initiated in ${delay_seconds} seconds."

        if [[ -n "$restart_flag" ]]; then
            message="Restart will be initiated in ${delay_seconds} seconds."
        fi

        # Respond to caller and start background process for delay and shutdown/restart
        echo "$message"

        if [[ -n "$restart_flag" ]]; then
            nohup bash -c "sleep ${delay_seconds}; systemctl reboot" >/dev/null 2>&1 &
        else
            nohup bash -c "sleep ${delay_seconds}; systemctl poweroff" >/dev/null 2>&1 &
        fi
    else
        # Shutdown/Restart immediately
        if [[ -n "$restart_flag" ]]; then
            systemctl reboot
        else
            systemctl poweroff
        fi
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -gt 0 && ( "$1" == "--help" || "$1" == "-h" ) ]]; then
        usage
    fi

    local delay="0"
    local restart_flag=""

    while (("$#")); do
        case $1 in
            --restart)
                restart_flag=1
                ;;
            --delay)
                assert_flag_has_argument "$1" "${2:-}" "$#"
                validate_positive_integer "$1" "$2"
                delay="$2"
                shift
                ;;
            *)
                err "$(printf "$ERR_CODE_2" "$1")" 2
                ;;
        esac
        shift
    done

    schedule_shutdown "$restart_flag" "$delay"
}

main "$@"
# eof
