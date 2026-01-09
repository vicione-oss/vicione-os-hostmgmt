#!/usr/bin/env bash

# ###
#
# Get Network VLAN Name Script
#
# Gets the VLAN name based on the given netplan configuration and
# the name of the network interface.
# ###

set -eEuo pipefail

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

CONFIGURATION_CONTENT="${1:-}"
INTERFACE_NAME="${2:-}"

vlan_name=$(
    grep -A3 "vlans:" <<< "$CONFIGURATION_CONTENT" |
    grep -B2 "link: ${INTERFACE_NAME}" |
    head -n 1 |
    tr -d ' ' || true)

echo "$vlan_name"
#eof
