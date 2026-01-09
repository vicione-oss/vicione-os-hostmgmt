#!/usr/bin/env bash

# ###
#
# Get Network DNS Settings Script
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

readonly DNS_SUFFIX_MARKER=" # dns-suffix"
readonly HOSTS_FILE_PATH="/etc/hosts"
readonly SECTION_NAME="HostManagement"
readonly START_MARKER="# BEGIN ${SECTION_NAME}"
readonly END_MARKER="# END ${SECTION_NAME}"

#######################################
# Variables
#######################################

DNS_SUFFIX=""
DNS_SUFFIX_ENABLED="false"

CONFIGURATION_PATH=""
CONFIGURATION_CONTENT=""
CONFIGURATION_ENTRY_POINT=""
HOSTS_FILE_CONTENT=""
INTERFACE_NAME=""
NAMESERVERS_ENABLED="false"
NAMESERVERS_JSON=()
NETWORK_INTERFACES=""
MULTICAST_DNS_ENABLED="false"
SEARCH_DOMAINS_ENABLED="false"
SEARCH_DOMAINS_JSON=()
STATIC_HOSTS_ENABLED="false"
STATIC_HOSTS_JSON=""

#######################################
# Methods
#######################################

define_configuration_entrypoint() {
    local vlan_name

    vlan_name=$(Scripts/Network/Helper/get_network_vlan_name.sh "$CONFIGURATION_CONTENT" "$INTERFACE_NAME" || true)

    if [[ -n "$vlan_name" ]]; then
        CONFIGURATION_ENTRY_POINT="$vlan_name"
    else
        CONFIGURATION_ENTRY_POINT="${INTERFACE_NAME}:"
    fi
}

determine_dns_multicast() {
    local multicast_enabled_state

    multicast_enabled_state=$(
        resolvectl mdns "$INTERFACE_NAME" |
        cut -d':' -f2 |
        tr -d '[:space:]' || true)

    if [[ "$multicast_enabled_state" == "yes" ]]; then
        MULTICAST_DNS_ENABLED="true"
    fi
}

determine_nameservers() {
    local nameservers

    nameservers=$(
        grep -A100 "$CONFIGURATION_ENTRY_POINT" <<< "$CONFIGURATION_CONTENT" |
        grep -A100 "nameservers:" |
        grep -A100 "addresses:" |
        sed "/search:/q" |
        grep '^[[:space:]]*[-#][[:space:]]' || true)

    if [[ -n "$nameservers" ]]; then
        mapfile -t nameserver_array <<< "$nameservers"

        for nameserver in "${nameserver_array[@]}"; do
            trimmed=$(sed 's/^[[:space:]]* //' <<< "$nameserver")
            ip=$(grep -Eo '[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+' <<< "$trimmed")

            if [[ -n "$ip" ]]; then
                if [[ "$trimmed" =~ ^[[:space:]]*- ]]; then
                    NAMESERVERS_ENABLED="true"
                fi

                if [[ "${NAMESERVERS_JSON[*]}" != *"$ip"* ]]; then
                    NAMESERVERS_JSON+=("\"${ip}\"")
                fi
            fi
        done
    fi
}

determine_search_domains() {
    local search_domains

    search_domains=$(
        grep -A100 "$CONFIGURATION_ENTRY_POINT" <<< "$CONFIGURATION_CONTENT" |
        grep -A100 "nameservers:" |
        grep -A100 "search:" |
        grep '^[[:space:]]*[-#][[:space:]]' || true)

    if [[ -n "$search_domains" ]]; then
        mapfile -t search_domains_array <<< "$search_domains"

        for search_domain in "${search_domains_array[@]}"; do
            to_add=""
            trimmed="${search_domain//$DNS_SUFFIX_MARKER/}"
            trimmed=$(sed 's/^[[:space:]]* //' <<< "$trimmed")

            if [[ "$search_domain" == *"$DNS_SUFFIX_MARKER" ]]; then
                if [[ -z "$DNS_SUFFIX" ]]; then
                    DNS_SUFFIX=$(sed 's/^[-#] //' <<< "$trimmed")

                    if [[ "$trimmed" =~ ^[[:space:]]*- ]]; then
                        DNS_SUFFIX_ENABLED="true"
                    fi
                fi
            else
                if [[ "$trimmed" =~ ^[[:space:]]*- ]]; then
                    SEARCH_DOMAINS_ENABLED="true"
                fi

                to_add=$(sed 's/^[-#] //' <<< "$trimmed")
            fi

            if [[ -n "$to_add" && 
                  "${SEARCH_DOMAINS_JSON[*]}" != *"\"${to_add}\""* ]]; then
                SEARCH_DOMAINS_JSON+=("\"${to_add}\"")
            fi
        done
    fi
}

determine_static_hosts() {
    if [[ -n "$HOSTS_FILE_CONTENT" ]]; then
        while IFS= read -r line; do
            [[ -z "$line" ]] && continue

            IFS=' ' read -r current_ip_address current_hostname <<< "$line"

            if ! [[ "$current_ip_address" =~ ^[[:space:]]*# ]]; then
                STATIC_HOSTS_ENABLED="true"
            fi

            STATIC_HOSTS_JSON+="{\"IPAddress\": \"${current_ip_address#\#}\", \"Hostname\": \"${current_hostname}\"},"
        done <<< "$HOSTS_FILE_CONTENT"

        STATIC_HOSTS_JSON="${STATIC_HOSTS_JSON%,}"
    fi
}

load_configuration_content() {
    if [[ -f "$CONFIGURATION_PATH" ]]; then
        CONFIGURATION_CONTENT=$(< "$CONFIGURATION_PATH")
    else
        CONFIGURATION_CONTENT=""
    fi
}

load_current_hosts_file() {
    if [[ -f "$HOSTS_FILE_PATH" ]]; then
        HOSTS_FILE_CONTENT=$(awk "/$START_MARKER/{flag=1; next} /$END_MARKER/{flag=0} flag" "$HOSTS_FILE_PATH" || true)
    fi
}

set_configuration_path() {
    CONFIGURATION_PATH="${DIRECTORY_NETPLAN}/50-${INTERFACE_NAME}.yaml"
}

#######################################
# Argument handling
#######################################

main() {
    local hostname
    local json_output

    hostname=$(Scripts/Network/DNS/get_hostname.sh)
    NETWORK_INTERFACES=$(Scripts/Network/Helper/get_network_interfaces.sh)

    read -r first_line <<< "$NETWORK_INTERFACES"
    INTERFACE_NAME=$(awk '{sub(/@.*/, "", $1); print $1}' <<< "$first_line")
    
    set_configuration_path
    load_configuration_content

    if [[ -n "$CONFIGURATION_CONTENT" ]]; then
        define_configuration_entrypoint
        determine_nameservers
        determine_search_domains
        determine_dns_multicast
    fi

    load_current_hosts_file
    determine_static_hosts

    json_output="{"
    json_output+="\"Hostname\": \"${hostname}\","
    json_output+="\"MulticastDNSEnabled\": ${MULTICAST_DNS_ENABLED},"
    json_output+="\"StaticHostsEnabled\": ${STATIC_HOSTS_ENABLED},"
    json_output+="\"StaticHosts\": [${STATIC_HOSTS_JSON}],"
    json_output+="\"NameServersEnabled\": ${NAMESERVERS_ENABLED},"
    json_output+="\"NameServers\": [$(IFS=, ; echo "${NAMESERVERS_JSON[*]}")],"
    json_output+="\"DNSSuffixEnabled\": ${DNS_SUFFIX_ENABLED},"
    json_output+="\"DNSSuffix\": \"${DNS_SUFFIX}\","
    json_output+="\"SearchDomainsEnabled\": ${SEARCH_DOMAINS_ENABLED},"
    json_output+="\"SearchDomains\": [$(IFS=, ; echo "${SEARCH_DOMAINS_JSON[*]}")]"
    json_output+="}"

    echo "$json_output"
}

main
#eof
