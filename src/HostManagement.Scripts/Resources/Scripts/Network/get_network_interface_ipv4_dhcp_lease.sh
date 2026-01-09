#!/usr/bin/env bash

# ###
#
# Get Network Interface IPv4 DHCP Lease Script - v1.0.0
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
# Error codes
#######################################

readonly ERR_CODE_1="The interface '%s' does not exist."

#######################################
# Constants
#######################################

readonly DIRECTORY_NETPLAN="/etc/netplan"
readonly DIRECTORY_DHCP_LEASE="/run/systemd/netif/leases"

#######################################
# Variables
#######################################

CONFIGURATION_PATH=""
CONFIGURATION_CONTENT=""

INTERFACE_NAME=""
DHCP_ENABLED="true"
DHCP_LEASE_PATH=""
DHCP_LEASE_CONTENT=""
DHCP_LEASE_OBTAINED="null"
DHCP_LEASE_EXPIRES="null"

IP_ADDRESS="null"
GATEWAY="null"

DNS_NAME_SERVERS=()
SEARCH_DOMAINS=()

DNS_NAME_SERVERS_ENABLED="false"
SEARCH_DOMAINS_ENABLED="false"

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 <interface_name>"
    exit 0
}

determine_dhcp_enabled() {
    if ! Scripts/Network/Helper/is_dhcp_enabled.sh "$CONFIGURATION_CONTENT"; then
        DHCP_ENABLED="false"
    fi
}

determine_dhcp_lease_obtained() {
    DHCP_LEASE_OBTAINED=$(stat -c %y "$DHCP_LEASE_PATH")
    DHCP_LEASE_OBTAINED=$(date -d "$DHCP_LEASE_OBTAINED" "+%Y-%m-%dT%H:%M:%S%z")
}

determine_dhcp_lease_expires() {
    lifetime=$(
        grep -oP '(?<=LIFETIME=)\d+' <<< "$DHCP_LEASE_CONTENT")

    if [[ -n "$lifetime" ]]; then
        DHCP_LEASE_EXPIRES=$(date -d "$DHCP_LEASE_OBTAINED + $lifetime seconds" "+%Y-%m-%dT%H:%M:%S%z")
    fi
}

determine_dns_name_servers() {
    dns=$(
        grep '^DNS' <<< "$DHCP_LEASE_CONTENT" |
        cut -d= -f2)

    for dns_entry in $dns; do
        DNS_NAME_SERVERS+=("\"${dns_entry}\"")
    done

    if [ ${#DNS_NAME_SERVERS[@]} -gt 0 ]; then
        DNS_NAME_SERVERS_ENABLED="true"
    fi
}

determine_gateway() {
    GATEWAY=$(
        grep '^ROUTER' <<< "$DHCP_LEASE_CONTENT" |
        cut -d= -f2)

    if [[ -n "$GATEWAY" ]]; then
        GATEWAY="\"${GATEWAY}\""
    fi
}

determine_ip_address_information() {
    IP_ADDRESS=$(
        grep '^ADDRESS' <<< "$DHCP_LEASE_CONTENT" |
        cut -d= -f2)

    netmask=$(
        grep '^NETMASK' <<< "$DHCP_LEASE_CONTENT" |
        cut -d= -f2)

    if [[ -n "$IP_ADDRESS" ]]; then
        IP_ADDRESS="{ \"IPAddress\": \"${IP_ADDRESS}\", \"Netmask\": \"${netmask}\" }"
    fi
}

determine_search_domains() {
    domain_names=$(
        grep '^DOMAINNAME' <<< "$DHCP_LEASE_CONTENT" |
        cut -d= -f2)

    for domain_name_entry in $domain_names; do
        SEARCH_DOMAINS+=("\"${domain_name_entry}\"")
    done

    if [[ ${#SEARCH_DOMAINS[@]} -gt 0 ]]; then
        SEARCH_DOMAINS_ENABLED="true"
    fi
}

load_configuration_content() {
    if [[ -e "$CONFIGURATION_PATH" ]]; then
        CONFIGURATION_CONTENT=$(< "$CONFIGURATION_PATH")
    fi
}

load_dhcp_lease_content() {
    if [[ -e "$DHCP_LEASE_PATH" ]]; then
        DHCP_LEASE_CONTENT=$(< "$DHCP_LEASE_PATH")
    fi
}

set_configuration_path() {
    CONFIGURATION_PATH="${DIRECTORY_NETPLAN}/50-${INTERFACE_NAME}.yaml"
}

set_dhcp_lease_path() {
    interface_id=$(
        networkctl list |
        grep "$INTERFACE_NAME" |
        awk '{print $1}')

    DHCP_LEASE_PATH="${DIRECTORY_DHCP_LEASE}/${interface_id}"
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    INTERFACE_NAME="${1:-}"

    if ! Scripts/Network/Helper/is_network_interface_available.sh "$INTERFACE_NAME"; then
        err "$(printf "$ERR_CODE_1" "$INTERFACE_NAME")" 1
    fi

    set_configuration_path
    load_configuration_content

    if [[ -n "$CONFIGURATION_CONTENT" ]]; then
        determine_dhcp_enabled

        if [[ "$DHCP_ENABLED" == "true" ]]; then
            set_dhcp_lease_path

            if [[ -e "$DHCP_LEASE_PATH" ]]; then
                load_dhcp_lease_content

                determine_dhcp_lease_obtained
                determine_dhcp_lease_expires

                DHCP_LEASE_OBTAINED="\"${DHCP_LEASE_OBTAINED}\""
                DHCP_LEASE_EXPIRES="\"${DHCP_LEASE_EXPIRES}\""

                determine_ip_address_information
                determine_gateway
                determine_dns_name_servers
                determine_search_domains
            fi
        fi
    fi

    json_output="{"
    json_output+="\"LeaseObtained\": ${DHCP_LEASE_OBTAINED},"
    json_output+="\"LeaseExpires\": ${DHCP_LEASE_EXPIRES},"
    json_output+="\"Gateway\": ${GATEWAY},"
    json_output+="\"IPv4Detail\": ${IP_ADDRESS},"
    json_output+="\"NetworkDNSSettings\":"
    json_output+="{"
    json_output+="\"StaticHostsEnabled\": false," # Not necessary! Think of another object!
    json_output+="\"StaticHosts\": [],"           # Not necessary! Think of another object!
    json_output+="\"NameServersEnabled\": ${DNS_NAME_SERVERS_ENABLED},"
    json_output+="\"NameServers\": [$(printf '%s, ' "${DNS_NAME_SERVERS[@]}" | sed 's/, $//')],"
    json_output+="\"SearchDomainsEnabled\": ${SEARCH_DOMAINS_ENABLED},"
    json_output+="\"SearchDomains\": [$(printf '%s, ' "${SEARCH_DOMAINS[@]}" | sed 's/, $//')]"
    json_output+="}"
    json_output+="}"

    echo "$json_output"
}

main "$@"
#eof
