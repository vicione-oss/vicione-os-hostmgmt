#!/usr/bin/env bash

# ###
#
# Set Service State Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --name <service-name> --state <Enabled|Disabled|Restart>"
    exit "$1"
}

err() {
    echo -e "$1" >&2
    exit "$2"
}

#######################################
# Constants
#######################################

readonly ENABLED="Enabled"
readonly DISABLED="Disabled"
readonly RESTART="Restart"

#######################################
# Argument handling
#######################################

main() {
    if [[ "$1" == "--help" ]] || [[ "$1" == "-h" ]]; then
        usage 0
    fi

    name=""
    state=""

    while (("$#")); do
        case "$1" in
            --name)
                name="$2"
                ;;
            --state)
                state="$2"
                ;;
            *)
                usage 1
                ;;
        esac
        shift 2
    done

    # Validate parameters
    if [[ -z "$name" ]] || [[ -z "$state" ]]; then
        usage 2
    fi

    if [[ "$state" != "$ENABLED" ]] && [[ "$state" != "$DISABLED" ]] && [[ "$state" != "$RESTART" ]]; then
        usage 3
    fi

    # Execute the state and check the result
    case "$state" in
        Enabled)
            if systemctl enable --now "$name" &>/dev/null; then
                exit 0
            else
                err "Failed to enable and start service $name." 4
            fi
            ;;
        Disabled)
            if systemctl disable --now "$name" &>/dev/null; then
                exit 0
            else
                err "Failed to disable and stop service $name." 5
            fi
            ;;
        Restart)
            if systemctl restart "$name" &>/dev/null; then
                exit 0
            else
                err "Failed to restart service $name." 6
            fi
            ;;
    esac
}

main "$@"
#eof
