#!/usr/bin/env bash

# ###
#
# Restart System Script
#
# The script returns 0 if ok, non zero for error.
# ###

set -eEuo pipefail

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

if ! systemctl reboot; then
    rc=$?
    err "Failed to issue restart command (exit code $rc)." "$rc"
fi

# eof
