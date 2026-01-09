#!/usr/bin/env bash

# ###
#
# Get Network Interfaces Common Information Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

set -eEuo pipefail

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

#######################################
# Constants
#######################################

readonly DIRECTORY_NETPLAN="/etc/netplan"

#######################################
# Methods
#######################################

warn() {
    echo -e "$1" >&2
}

#######################################
# Argument handling
#######################################

main() {
    network_interfaces=$(Scripts/Network/Helper/get_network_interfaces.sh)

    if [[ -z "$network_interfaces" ]]; then
        warn "No network interfaces have been found."
        echo -e "[ ]"
        exit 0
    fi

    mapfile -t network_interfaces_array <<< "$network_interfaces"

    for current in "${network_interfaces_array[@]}"; do
        name=${current%% *}
        address=$(awk '{ if (NF==4) { print $3 } else { print "none" } }' <<< "$current")
        enabled="true"
        netplan_config="${DIRECTORY_NETPLAN}/50-${name}.yaml"

        if [[ -e "$netplan_config" ]] && grep -q "activation-mode: off" "$netplan_config"; then
            enabled="false"
        fi

        json+="{\"Enabled\": ${enabled}, \"Name\": \"${name}\", \"PhysicalAddress\": \"${address}\"}, "
    done

    echo "[ ${json::-2} ]"
}

main
#eof
