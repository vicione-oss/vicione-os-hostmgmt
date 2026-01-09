#!/usr/bin/env bash

# ###
#
# Restart System Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

set -eEuo pipefail

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

if ! shutdown -r now; then
    err "Failed to issue restart command." 1
fi

# eof
