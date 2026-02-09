#!/usr/bin/env bash

# ###
#
# Get Network Status Information Script
#
# The script returns 0 if ok or non zero for error.
# ###

set -eEuo pipefail

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

#######################################
# Error codes
#######################################

readonly ERR_CODE_1="Unsupported flag '%s'."
readonly ERR_CODE_2="No interface name was specified."

#######################################
# Variables
#######################################

DEFAULT_GATEWAY=""
INTERFACE_NAME=""
INTERFACE_STATE="Unknown"
IS_DEFAULT_ROUTE_CONFIGURED="false"
IS_DEFAULT_GATEWAY_AVAILABLE="false"
IS_INTERNET_AVAILABLE="false"
IS_DNS_FUNCTIONAL="false"

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --interface-name <name>"
    echo
    echo "Options:"
    echo "  --interface-name - Specifies the network interface for which status information should be retrieved."
    echo
    echo "Examples:"
    echo "$0 --interface-name ens33"
    exit 0
}

check_for_default_gateway() {
    if [[ -n "$DEFAULT_GATEWAY" ]] && get_ping_result "$DEFAULT_GATEWAY"; then
        IS_DEFAULT_GATEWAY_AVAILABLE="true"
    fi
}

check_for_default_route() {
    if [[ -n "$DEFAULT_GATEWAY" ]]; then
        IS_DEFAULT_ROUTE_CONFIGURED="true"
    fi
}

check_for_dns_functionality() {
    if [[ "$IS_INTERNET_AVAILABLE" == "true" ]] && get_ping_result "google.com"; then
        IS_DNS_FUNCTIONAL="true"
    fi
}

check_for_internet_connection() {
    if get_ping_result "8.8.8.8"; then
        IS_INTERNET_AVAILABLE="true"
    fi
}

correct_interface_name_for_vlan() {
    # If a virtual local network is connected to the specified physical interface, 
    # the script must use the name of the virtual local network.

    vlan_name=$(
        ip -d link show |
        grep -B2 "vlan protocol 802.1Q" |
        grep "@${INTERFACE_NAME}" |
        cut -d ':' -f2 |
        cut -d '@' -f1 |
        tr -d ' ' || true)

    if [[ -n "$vlan_name" ]]; then
        INTERFACE_NAME="$vlan_name"
    fi
}

get_default_gateway() {
    DEFAULT_GATEWAY=$(
        ip route |
        grep "$INTERFACE_NAME" |
        grep default |
        cut -d' ' -f3 |
        head -n 1 || true)
}

get_network_interface_state() {
    INTERFACE_STATE=$(
        Scripts/Network/Helper/get_network_interfaces.sh |
        grep "$INTERFACE_NAME" |
        awk '{print $2}')
}

get_ping_result() {
    ping -I "$INTERFACE_NAME" -c 1 -W 1 "$1" > /dev/null 2>&1

    return $(( $? == 0 ? 0 : 1))
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    if [[ "$1" != "--interface-name" ]]; then
        err "$(printf "$ERR_CODE_1" "$1")" 1
    fi

    if [ -z "${2:-}" ]; then
        err "$ERR_CODE_2" 2
    fi

    INTERFACE_NAME=$2

    # Physical part

    get_network_interface_state

    # Physical or virtual part (e.g. for VLAN's)

    correct_interface_name_for_vlan

    get_default_gateway

    check_for_default_route
    check_for_default_gateway
    check_for_internet_connection
    check_for_dns_functionality

    json_output+="{"
    json_output+="  \"IsDefaultRouteConfigured\": ${IS_DEFAULT_ROUTE_CONFIGURED},"
    json_output+="  \"IsDefaultGatewayAvailable\": ${IS_DEFAULT_GATEWAY_AVAILABLE},"
    json_output+="  \"IsInternetAvailable\": ${IS_INTERNET_AVAILABLE},"
    json_output+="  \"IsDNSFunctional\": ${IS_DNS_FUNCTIONAL},"
    json_output+="  \"InterfaceState\": \"${INTERFACE_STATE}\""
    json_output+="}"

    echo "$json_output"
}

main "$@"
#eof
