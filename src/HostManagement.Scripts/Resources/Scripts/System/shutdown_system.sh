#!/usr/bin/env bash

# ###
#
# Shutdown System Script
#
# The script returns 0 if ok, non zero for error.
# ###

set -eEuo pipefail

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

if ! systemctl poweroff; then
    rc=$?
    err "Failed to issue shutdown command (exit code $rc)." "$rc"
fi

# eof
