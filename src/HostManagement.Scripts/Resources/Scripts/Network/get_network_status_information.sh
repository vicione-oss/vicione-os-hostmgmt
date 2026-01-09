#!/usr/bin/env bash

# ###
#
# Get Network Status Information Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Variables
#######################################

DEFAULT_GATEWAY=""
INTERFACES_STATE="[]"
IS_DEFAULT_ROUTE_CONFIGURED="false"
IS_DEFAULT_GATEWAY_AVAILABLE="false"
IS_INTERNET_AVAILABLE="false"
IS_DNS_FUNCTIONAL="false"

#######################################
# Methods
#######################################

check_for_default_gateway() {
    if [ -n "${DEFAULT_GATEWAY}" ] && get_ping_result "${DEFAULT_GATEWAY}"; then
        IS_DEFAULT_GATEWAY_AVAILABLE="true"
    fi
}

check_for_default_route() {
    if [ -n "${DEFAULT_GATEWAY}" ]; then
        IS_DEFAULT_ROUTE_CONFIGURED="true"
    fi
}

check_for_dns_functionality() {
    if [[ "${IS_INTERNET_AVAILABLE}" == "true" ]] && get_ping_result "google.com"; then
        IS_DNS_FUNCTIONAL="true"
    fi
}

check_for_internet_connection() {
    if get_ping_result "8.8.8.8"; then
        IS_INTERNET_AVAILABLE="true"
    fi
}

get_default_gateway() {
    DEFAULT_GATEWAY=$(ip route | grep default | awk '{print $3}' | head -n 1)
}

get_network_interfaces_state() {
    script_path=$(readlink -f "Scripts/Network/Helper/get_network_interfaces.sh")

    if [ ! -x "$script_path" ]; then
        warn "Permission denied: The script $script_path does not have execute permission."
    fi

    network_interfaces=$($script_path)

    json="[ "

    IFS=$'\n'
    for current in $network_interfaces;
    do
        name=$(echo "${current%% *}" | sed 's/ //;s/@.*//')
        state=$(echo "${current}" | awk '$2 !~ /UP/ {print "\"DOWN\""} $2 ~ /UP/ {print "\"UP\""}')

        json="${json} {\"Name\": \"${name}\", \"State\": ${state}}, "
    done

    INTERFACES_STATE="${json::-2} ]"
}

get_ping_result() {
    ping -c 1 -W 1 "$1" > /dev/null 2>&1
    
    return $(( $? == 0 ? 0 : 1))
}

warn() {
    echo -e "$1" >&2
}

#######################################
# Argument handling
#######################################

main() {
    get_network_interfaces_state
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
    json_output+="  \"Interfaces\": ${INTERFACES_STATE}"
    json_output+="}"

    echo -e "${json_output}"
}

main
#eof
