#!/usr/bin/env bash

# ###
#
# Get Network DNS Settings Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Variables
#######################################

NETPLAN_DIRECTORY="/etc/netplan"
DNS_SUFFIX_FILE="${NETPLAN_DIRECTORY}/dns_suffix"
SECTION_NAME="HostManagement"
START_MARKER="# BEGIN ${SECTION_NAME}"
END_MARKER="# END ${SECTION_NAME}"
HOSTS_FILE="/etc/hosts"
HOSTS_FILE_CONTENT=null

#######################################
# Methods
#######################################

err() {
    echo -e "$1" >&2
    exit "$2"
}

load_current_hosts_file() {
    HOSTS_FILE_CONTENT=$(awk "/$START_MARKER/{flag=1; next} /$END_MARKER/{flag=0} flag" "${HOSTS_FILE}")
}

static_hosts_to_json() {
    while IFS= read -r line; do
        [[ -z ${line} ]] && continue

        IFS=' ' read -r ip_address hostname <<< "${line}"
        static_hosts_json+="{\"IPAddress\": \"${ip_address}\", \"Hostname\": \"${hostname}\"},"
        static_hosts_enabled="true" 
    done <<< "${HOSTS_FILE_CONTENT}"

    static_hosts_json="${static_hosts_json%,}"

    echo "${static_hosts_json}"
}

#######################################
# Argument handling
#######################################

main() {
    nameservers_json=()
    network_interfaces=$(Scripts/Network/Helper/get_network_interfaces.sh)

    IFS=$'\n'
    for current in $network_interfaces; do
        iface=$(echo "${current}" | awk '{print $1}' | sed 's/ //;s/@.*//')
        nameservers=$(netplan get ethernets."${iface}".nameservers.addresses | sed 's/-//g' | sed 's/ //g')
        search_domains=$(netplan get ethernets."${iface}".nameservers.search | sed 's/-//g' | sed 's/ //g')

        # Get multicast DNS enabled state
        multicast_dns_enabled="false"
        multicast_enabled_state = $(resolvectl mdns ${iface} | awk -F': ' '{print $2}')

        if [[ "${multicast_enabled_state}" == "yes" ]]; then
            multicast_dns_enabled="true"
        fi

        name_servers_enabled="false"

        if [ "${nameservers}" != "null" ]; then
            IFS=$'\n' read -d '' -r -a nameserver_array <<< "${nameservers}"

            for nameserver in "${nameserver_array[@]}"; do
                if [[ "${nameservers_json[*]}" != *"${nameserver}"* ]]; then
                    nameservers_json+=("\"${nameserver}\"")
                fi
            done

            name_servers_enabled="true"
        fi

        search_domains_enabled="false"
        dns_suffix_enabled="false"

        if [ -e "${DNS_SUFFIX_FILE}" ]; then
            dns_suffix=$(cat "${DNS_SUFFIX_FILE}")

            if [ -n "${dns_suffix}" ]; then
                dns_suffix_enabled="true"
            fi
        fi

        if [ "${search_domains}" != "null" ]; then
            search_domains_enabled="true"

            IFS=$'\n' read -d '' -r -a search_domains_array <<< "${search_domains}"

            for search_domain in "${search_domains_array[@]}"; do
                if [[ "${search_domain}" != "${dns_suffix}" ]] && [[ "${search_domains_json[*]}" != *"\"${search_domain}\""* ]]; then
                    search_domains_json+=("\"${search_domain}\"")
                fi
            done
        fi
    done

    load_current_hosts_file
    static_hosts_json=$(static_hosts_to_json)

    if [[ -z "${static_hosts_json}" ]]; then
        static_hosts_enabled="false" 
    else
        static_hosts_enabled="true"
    fi 

    hostname=$(Scripts/Network/DNS/get_hostname.sh)

    json_output="{"
    json_output+="\"Hostname\": \"${hostname}\","
    json_output+="\"MulticastDNSEnabled\": ${multicast_dns_enabled},"
    json_output+="\"StaticHostsEnabled\": ${static_hosts_enabled},"
    json_output+="\"StaticHosts\": [${static_hosts_json}],"
    json_output+="\"NameServersEnabled\": ${name_servers_enabled},"
    json_output+="\"NameServers\": [$(printf '%s, ' "${nameservers_json[@]}" | sed 's/, $//')],"
    json_output+="\"DNSSuffixEnabled\": ${dns_suffix_enabled},"
    json_output+="\"DNSSuffix\": \"${dns_suffix}\","
    json_output+="\"SearchDomainsEnabled\": ${search_domains_enabled},"
    json_output+="\"SearchDomains\": [$(printf '%s, ' "${search_domains_json[@]}" | sed 's/, $//')]"
    json_output+="}"
    
    echo -e "${json_output}"
}

main
#eof
