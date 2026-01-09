#!/usr/bin/env bash

# ###
#
# Set Service State Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

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

SERVICE_NAME=""
SERVICE_STATE=""

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --name <service-name> --state <${ENABLED}|${DISABLED}|${START}|${STOP}|${RESTART}>"
    echo
    echo "Options:"
    echo "  --name      - The name of the service."
    echo "  --state     - Desired state: ${ENABLED}, ${DISABLED}, or ${START}, ${STOP}, ${RESTART}."
    exit 0
}

err() {
    echo -e "$1" >&2
    exit "$2"
}

isServiceEnabled () {
    [[ "$(systemctl is-enabled "$SERVICE_NAME" 2>/dev/null)" == "enabled" ]]
}

isServiceActive () {
    [[ "$(systemctl is-active "$SERVICE_NAME" 2>/dev/null)" == "active" ]]
}

start() {
    # Only attempt to start the service if it's inactive
    if ! isServiceActive; then
        if ! systemctl start "$SERVICE_NAME" &>/dev/null; then
            err "Failed to start service '$SERVICE_NAME'." 5
        fi
    fi
}

stop() {
    # Only attempt to stop the service if it's active
    if isServiceActive; then
        if ! systemctl stop "$SERVICE_NAME" &>/dev/null; then
            err "Failed to stop service '$SERVICE_NAME'." 7
        fi
    fi
}

enable() {
    if ! isServiceEnabled; then
        if ! systemctl enable "$SERVICE_NAME" &>/dev/null; then
            err "Failed to enable service '$SERVICE_NAME'." 4
        fi
    fi

    start
}

disable() {
    if isServiceEnabled; then
        if ! systemctl disable "$SERVICE_NAME" &>/dev/null; then
            err "Failed to disable service '$SERVICE_NAME'." 6
        fi
    fi

    stop
}

restart() {
    if isServiceEnabled; then
        if systemctl restart "$SERVICE_NAME" &>/dev/null; then
            # Check if the service is oneshot; if not, wait for it to become active.
            service_type=$(systemctl show "$SERVICE_NAME" -p Type --value)
            if [[ "$service_type" != "oneshot" ]]; then
                if ! Scripts/Common/wait_for_service.sh "${SERVICE_NAME}" 10; then
                    err "Service '$SERVICE_NAME' did not become active after restart." 8
                fi
            fi
        else
            err "Failed to restart service '$SERVICE_NAME'." 9
        fi
    else
        err "Service '$SERVICE_NAME' is not enabled. Restart aborted." 10
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ "$1" == "--help" ]] || [[ "$1" == "-h" ]]; then
        usage 0
    fi

    while (("$#")); do
        case "$1" in
            --name)
                SERVICE_NAME="$2"
                shift 2
                ;;
            --state)
                SERVICE_STATE="$2"
                shift 2
                ;;
            *)
                usage 1
                ;;
        esac
    done

    # Validate parameters
    if [[ -z "$SERVICE_NAME" ]] || [[ -z "$SERVICE_STATE" ]]; then
        usage 2
    fi

    if [[ "$SERVICE_STATE" != "$ENABLED" ]] &&
       [[ "$SERVICE_STATE" != "$DISABLED" ]] &&
       [[ "$SERVICE_STATE" != "$START" ]] &&
       [[ "$SERVICE_STATE" != "$STOP" ]] &&
       [[ "$SERVICE_STATE" != "$RESTART" ]]; then
        usage 3
    fi

    # Set the requested state
    case "$SERVICE_STATE" in
        Enabled)  enable;;
        Disabled) disable;;
        Start)  start;;
        Stop)  stop;;
        Restart)  restart;;
    esac
}

main "$@"
# eof
