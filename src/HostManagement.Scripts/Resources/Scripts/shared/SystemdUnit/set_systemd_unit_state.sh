#!/usr/bin/env bash

# ###
#
# Set Systemd Unit State Script
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

readonly ERR_CODE_1="Failed to enable systemd unit '%s'."
readonly ERR_CODE_2="Failed to disable systemd unit '%s'."
readonly ERR_CODE_3="Failed to start systemd unit '%s'."
readonly ERR_CODE_4="Failed to stop systemd unit '%s'."
readonly ERR_CODE_5="Failed to restart systemd unit '%s'."
readonly ERR_CODE_6="Systemd unit '%s' did not become active after restart."

#######################################
# Constants
#######################################

readonly ENABLED="Enabled"
readonly DISABLED="Disabled"
readonly START="Start"
readonly STOP="Stop"
readonly RESTART="Restart"

#######################################
# Variables
#######################################

SYSTEMD_UNIT_NAME=""
SYSTEMD_UNIT_STATE=""

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --name <systemd-unit-name> --state <${ENABLED}|${DISABLED}|${START}|${STOP}|${RESTART}>"
    echo
    echo "Options:"
    echo "  --name      - The name of the systemd unit."
    echo "  --state     - The desired state: ${ENABLED}, ${DISABLED}, or ${START}, ${STOP}, ${RESTART}."
    exit 0
}

wait_for_systemd_unit() {
    # Wait until the systemd unit has been started.

    timeout=10
    counter=0

    while [ $counter -lt "$timeout" ]; do
        status=$(systemctl show -p SubState "$SYSTEMD_UNIT_NAME" --value)

        if [[ "$status" == "running" ]]; then
            break;
        fi

        counter=$((counter + 1))
        sleep 1
    done

    if [[ $counter -eq "$timeout" ]]; then
        err "$(printf "$ERR_CODE_6" "$SYSTEMD_UNIT_NAME")" 6
    fi
}

start() {
    # just start the systemd unit:
    # if it's already running, the script just exits
    # if not, the unit will be started
    if ! systemctl start "$SYSTEMD_UNIT_NAME" &>/dev/null; then
        err "$(printf "$ERR_CODE_3" "$SYSTEMD_UNIT_NAME")" 3
    fi
}

stop() {
    # just stop the systemd unit:
    # if it's already stopped, the script just exits
    # if not, the unit will be stopped
    if ! systemctl stop "$SYSTEMD_UNIT_NAME" &>/dev/null; then
        err "$(printf "$ERR_CODE_4" "$SYSTEMD_UNIT_NAME")" 4
    fi
}

restart() {
    # restart is a temporary action and not a present state in the 'SystemConfiguration.json'
    # since we can start and stop systemd units "temporarily", this action does not need to
    # check the state of the unit
    if systemctl restart "$SYSTEMD_UNIT_NAME" &>/dev/null; then
        # check if the systemd unit is oneshot; if not, wait for it to become active
        systemd_unit_type=$(systemctl show "$SYSTEMD_UNIT_NAME" -p Type --value)
        if [[ "$systemd_unit_type" != "oneshot" ]]; then
            wait_for_systemd_unit
        fi
    else
        err "$(printf "$ERR_CODE_5" "$SYSTEMD_UNIT_NAME")" 5
    fi
}

enable_and_start() {
    # permanently enables (auto-start on reboot) and starts the systemd unit

    if ! systemctl enable "$SYSTEMD_UNIT_NAME" &>/dev/null; then
        err "$(printf "$ERR_CODE_1" "$SYSTEMD_UNIT_NAME")" 1
    fi

    start
}

disable_and_stop() {
    # permanently disables (no auto-start on reboot) and stops the systemd unit

    if ! systemctl disable "$SYSTEMD_UNIT_NAME" &>/dev/null; then
        err "$(printf "$ERR_CODE_2" "$SYSTEMD_UNIT_NAME")" 2
    fi

    stop
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    while (("$#")); do
        case "$1" in
            --name)
                SYSTEMD_UNIT_NAME="${2:-}"
                shift 2
                ;;
            --state)
                SYSTEMD_UNIT_STATE="${2:-}"
                shift 2
                ;;
            *)
                usage
                ;;
        esac
    done

    # Validate parameters
    if [[ -z "$SYSTEMD_UNIT_NAME" ]] || [[ -z "$SYSTEMD_UNIT_STATE" ]]; then
        usage
    fi

    if [[ "$SYSTEMD_UNIT_STATE" != "$ENABLED" ]] &&
       [[ "$SYSTEMD_UNIT_STATE" != "$DISABLED" ]] &&
       [[ "$SYSTEMD_UNIT_STATE" != "$START" ]] &&
       [[ "$SYSTEMD_UNIT_STATE" != "$STOP" ]] &&
       [[ "$SYSTEMD_UNIT_STATE" != "$RESTART" ]]; then
        usage
    fi

    # Set the requested state
    case "$SYSTEMD_UNIT_STATE" in
        Enabled)  enable_and_start;;
        Disabled) disable_and_stop;;
        Start)  start;;
        Stop)  stop;;
        Restart)  restart;;
    esac
}

main "$@"
# eof
