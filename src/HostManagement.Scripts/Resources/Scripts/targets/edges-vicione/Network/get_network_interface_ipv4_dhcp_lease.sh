#!/usr/bin/env bash

# ###
#
# Get Network Interface IPv4 DHCP Lease Script
#
# The script returns 0 if ok, non zero for error.
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
readonly ERR_CODE_2="Could not read DHCP lease information."

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
DHCP_LEASE_OBTAINED="null"
DHCP_LEASE_EXPIRES="null"

LEASE_ADDRESS=""
LEASE_DNS=""
LEASE_DOMAINNAME=""
LEASE_LIFETIME=""
LEASE_NETMASK=""
LEASE_ROUTER=""

IP_ADDRESS="null"
GATEWAY="null"

DNS_NAME_SERVERS_JSON="[]"
DNS_NAME_SERVERS_ENABLED="false"

SEARCH_DOMAINS_JSON="[]"
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
    if [[ -n "$LEASE_LIFETIME" ]]; then
        DHCP_LEASE_EXPIRES=$(date -d "$DHCP_LEASE_OBTAINED + $LEASE_LIFETIME seconds" "+%Y-%m-%dT%H:%M:%S%z")
    fi
}

determine_dns_name_servers() {
    dns_name_servers=()

    for dns_entry in $LEASE_DNS; do
        dns_name_servers+=("\"${dns_entry}\"")
    done

    if [ ${#dns_name_servers[@]} -gt 0 ]; then
        DNS_NAME_SERVERS_ENABLED="true"
        printf -v DNS_NAME_SERVERS_JSON '[%s]' "$(IFS=,; echo "${dns_name_servers[*]}")"
    fi
}

determine_gateway() {
    if [[ -n "$LEASE_ROUTER" ]]; then
        GATEWAY="\"${LEASE_ROUTER}\""
    fi
}

determine_ip_address_information() {
    if [[ -n "$LEASE_ADDRESS" &&
          -n "$LEASE_NETMASK" ]]; then
        IP_ADDRESS="{ \"IPAddress\": \"${LEASE_ADDRESS}\", \"Netmask\": \"${LEASE_NETMASK}\" }"
    fi
}

determine_search_domains() {
    search_domains=()

    for domain_name_entry in $LEASE_DOMAINNAME; do
        search_domains+=("\"${domain_name_entry}\"")
    done

    if [[ ${#search_domains[@]} -gt 0 ]]; then
        SEARCH_DOMAINS_ENABLED="true"
        printf -v SEARCH_DOMAINS_JSON '[%s]' "$(IFS=,; echo "${search_domains[*]}")"
    fi
}

load_configuration_content() {
    if [[ -e "$CONFIGURATION_PATH" ]]; then
        CONFIGURATION_CONTENT=$(< "$CONFIGURATION_PATH")
    fi
}

load_dhcp_lease_content() {
    if [[ -e "$DHCP_LEASE_PATH" ]]; then
        while IFS='=' read -r key val; do
            case "$key" in
                ADDRESS)    LEASE_ADDRESS="$val" ;;
                DNS)        LEASE_DNS="$val" ;;
                DOMAINNAME) LEASE_DOMAINNAME="$val" ;;
                NETMASK)    LEASE_NETMASK="$val" ;;
                ROUTER)     LEASE_ROUTER="$val" ;;
                LIFETIME)   LEASE_LIFETIME="$val" ;;
            esac
        done < "$DHCP_LEASE_PATH"
    else
        err "$ERR_CODE_2" 2
    fi
}

load_link_local_content() {
    ip_address=$(ip a show dev "$INTERFACE_NAME" | awk '/inet/ {print $2}')
    
    LEASE_ADDRESS=$(cut -d/ -f1 <<< "$ip_address")
    LEASE_NETMASK=$(cut -d/ -f2 <<< "$ip_address")
    LEASE_NETMASK=$(Scripts/Network/Helper/convert_ipv4_cidr_to_netmask.sh "$LEASE_NETMASK")
}

set_configuration_path() {
    CONFIGURATION_PATH="${DIRECTORY_NETPLAN}/50-${INTERFACE_NAME}.yaml"
}

set_dhcp_lease_path() {
    interface_id=$(
        networkctl list --no-pager |
        awk -v iface="$INTERFACE_NAME" '$2 == iface {print $1}')

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

                determine_gateway
                determine_dns_name_servers
                determine_search_domains
            else
                load_link_local_content
            fi

            determine_ip_address_information
        fi
    fi

    json_output="{"
    json_output+="\"LeaseObtained\": ${DHCP_LEASE_OBTAINED},"
    json_output+="\"LeaseExpires\": ${DHCP_LEASE_EXPIRES},"
    json_output+="\"Gateway\": ${GATEWAY},"
    json_output+="\"IPv4Detail\": ${IP_ADDRESS},"
    json_output+="\"NetworkDNSSettings\":"
    json_output+="{"
    json_output+="\"NameServersEnabled\": ${DNS_NAME_SERVERS_ENABLED},"
    json_output+="\"NameServers\": ${DNS_NAME_SERVERS_JSON},"
    json_output+="\"SearchDomainsEnabled\": ${SEARCH_DOMAINS_ENABLED},"
    json_output+="\"SearchDomains\": ${SEARCH_DOMAINS_JSON}"
    json_output+="}"
    json_output+="}"

    echo "$json_output"
}

main "$@"
#eof
