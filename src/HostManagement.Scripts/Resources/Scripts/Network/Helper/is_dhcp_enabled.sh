#!/usr/bin/env bash

# ###
#
# Is DHCP Enabled Script - v1.0.0
#
# The script returns 0 if DHCP enabled, otherwise 1.
# ###

set -eEuo pipefail

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

CONFIGURATION_CONTENT="${1:-}"

dhcp_helper=$(
    grep "dhcp4: true" <<< "$CONFIGURATION_CONTENT" || true)

if [[ -z "$dhcp_helper" ]]; then
    exit 1
fi
#eof
