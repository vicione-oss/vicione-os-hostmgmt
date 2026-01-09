#!/usr/bin/env bash

# ###
#
# Get Network Interfaces Script - v1.0.0
#
# Gets a list of network interfaces.
#
# The default behaviour is to get physical and virtual network interfaces.
# Physical interfaces are only included, if they are not connected to a
# virtual network interface. VLAN interfaces are also excluded.
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

readonly ALL="all"           # physical + virtual + vlan
readonly PHYSICAL="physical" # physical
readonly VIRTUAL="virtual"   # virtual without vlan
readonly VLAN="vlan"         # vlan

#######################################
# Variables
#######################################

ALL_INTERFACES=""

#######################################
# Methods
#######################################

get_all_interfaces() {
    # If all virtual network interfaces (e.g. lan1, lan2) linked to a physical
    # network interface (e.g. end0) are to be ignored through
    # HOSTMANAGEMENT__IGNORED_NETWORK_INTERFACES, the physical network interface
    # will still be displayed, even though it should be hidden.
    local ignored_network_interfaces="${HOSTMANAGEMENT__IGNORED_NETWORK_INTERFACES:-lo,docker}"
    ignored_network_interfaces="${ignored_network_interfaces// /}"
    ignored_network_interfaces="${ignored_network_interfaces//,/|}"

    ALL_INTERFACES=$(ip -br link show | grep -vE "$ignored_network_interfaces" || true)
}

get_default_interfaces() {
    interfaces=$(grep -vE '^vlan[0-9]+@' <<< "$ALL_INTERFACES" || true)

    if [[ -z "$interfaces" ]]; then
        return 0
    fi

    physical_interfaces=()
    virtual_interfaces=()

    declare -A interfaces_details

    while read -r current; do
        interface="${current%% *}"

        if grep -q '@' <<< "$interface"; then
            virtual_interfaces+=("$interface")
        else
            physical_interfaces+=("$interface")
        fi

        interfaces_details["$interface"]="$current"
    done <<< "$interfaces"

    for physical in "${physical_interfaces[@]}"; do
        found=false

        for virtual in "${virtual_interfaces[@]}"; do
            if [[ "$virtual" == *@$physical ]]; then
                found=true
                break;
            fi
        done

        if [ "$found" = false ]; then
            result+=("$physical")
        fi
    done

    for virtual in "${virtual_interfaces[@]}"; do
        result+=("$virtual")
    done

    for item in "${result[@]}"; do
        echo "${interfaces_details[$item]}" | sed 's/@[^ ]*/ /'
    done
}

get_physical_interfaces() {
    echo "$ALL_INTERFACES" | grep -v '@' || true
}

get_virtual_interfaces() {
    echo "$ALL_INTERFACES" | grep '@' | grep -vE '^vlan[0-9]+' || true
}

get_vlan_interfaces() {
    echo "$ALL_INTERFACES" | grep -E '^vlan[0-9]+@' || true
}

#######################################
# Argument handling
#######################################

main() {
    get_all_interfaces

    case "${1:-}" in
        "$ALL")
            echo "$ALL_INTERFACES"
            ;;
        "$PHYSICAL")
            get_physical_interfaces
            ;;
        "$VIRTUAL")
            get_virtual_interfaces
            ;;
        "$VLAN")
            get_vlan_interfaces
            ;;
        *)
            get_default_interfaces
            ;;
    esac
}

main "$@"
#eof
