#!/usr/bin/env bash

# ###
#
# Is Network Interface Available Script
#
# The script returns 0 if the given network interface is available, otherwise 1.
# ###

set -eEuo pipefail

if [[ ! -z "${HOSTMANAGEMENT__BYPASS_VALIDATION_CHECKS:-}" ]]; then exit 0; fi

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

iface_with_at="${1:-}"
iface_without_at=$(sed 's/ //;s/@.*//' <<< "$iface_with_at")

if ! ip link show "$iface_with_at" > /dev/null 2>&1 &&
   ! ip link show "$iface_without_at" > /dev/null 2>&1; then
    exit 1
fi
#eof
