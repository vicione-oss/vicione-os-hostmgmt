#!/usr/bin/env bash

# ###
#
# Get Network Interface IPv4 DHCP Lease Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Methods
#######################################

err() {
    echo -e "$1" >&2
    exit "$2"
}

#######################################
# Argument handling
#######################################

main() {
    iface=$1
    
    if [ -z "${iface}" ]; then
        err "Usage: $0 <Interface>" 1
    fi

    Scripts/Network/Helper/is_network_interface_available.sh "${iface}"

    if [ $? -eq 1 ]; then
        err "The interface '${iface}' does not exist." 2
    fi

    iface_id=$(networkctl list | grep "${iface}" | awk '{print $1}')
    is_dhcp_enabled=$(netplan get network.ethernets."${iface}".dhcp4)
    ip_address="null"
    lease_obtained="null"
    lease_expires="null"

    if [[ "${is_dhcp_enabled}" == "true" ]]; then
        dhcp_lease=$(netplan ip leases "${iface}" 2> /dev/null)

        if [ $? -eq 0 ]; then
            lease_obtained=$(stat -c %y /run/systemd/netif/leases/"${iface_id}")
            lease_obtained=$(date -d "$lease_obtained" "+%Y-%m-%dT%H:%M:%S%z")

            ip_address=$(echo "${dhcp_lease}" | grep '^ADDRESS' | cut -d= -f2)
            netmask=$(echo "${dhcp_lease}" | grep '^NETMASK' | cut -d= -f2)

            if [ -n "${ip_address}" ]; then
                ip_address="{ \"IPAddress\": \"${ip_address}\", \"Netmask\": \"${netmask}\" }"
                gateway=$(echo "${dhcp_lease}" | grep '^ROUTER' | cut -d= -f2)

                dns=$(echo "${dhcp_lease}" | grep '^DNS' | cut -d= -f2)

                for dns_entry in $dns; do
                    dns_nameservers+=("\"${dns_entry}\"")
                done

                domain_name=$(echo "${dhcp_lease}" | grep '^DOMAINNAME' | cut -d= -f2)

                for domain_name_entry in $domain_name; do
                    search_domains+=("\"${domain_name_entry}\"")
                done
            fi

            lifetime=$(echo "${dhcp_lease}" | grep -oP '(?<=LIFETIME=)\d+')

            if [ -n "${lifetime}" ]; then
                lease_expires=$(date -d "$lease_obtained + $lifetime seconds" "+%Y-%m-%dT%H:%M:%S%z")
                lease_expires="\"${lease_expires}\""
            fi

            lease_obtained="\"${lease_obtained}\""
        fi
    fi

    if [ -z "${gateway}" ]; then
        gateway="null"
    else
        gateway="\"${gateway}\""
    fi

    name_servers_enabled="true"

    if [ ${#dns_nameservers[@]} -eq 0 ]; then
        name_servers_enabled="false"
    fi

    search_domains_enabled="true"

    if [ ${#search_domains[@]} -eq 0 ]; then
        search_domains_enabled="false"
    fi

    json_output="{"
    json_output+="\"LeaseObtained\": ${lease_obtained},"
    json_output+="\"LeaseExpires\": ${lease_expires},"
    json_output+="\"Gateway\": ${gateway},"
    json_output+="\"IPv4Detail\": ${ip_address},"
    json_output+="\"NetworkDNSSettings\":"
    json_output+="{"
    json_output+="\"StaticHostsEnabled\": false," # Not necessary! Think of another object!
    json_output+="\"StaticHosts\": [],"           # Not necessary! Think of another object!
    json_output+="\"NameServersEnabled\": ${name_servers_enabled},"
    json_output+="\"NameServers\": [$(printf '%s, ' "${dns_nameservers[@]}" | sed 's/, $//')],"
    json_output+="\"SearchDomainsEnabled\": ${search_domains_enabled},"
    json_output+="\"SearchDomains\": [$(printf '%s, ' "${search_domains[@]}" | sed 's/, $//')]"
    json_output+="}"
    json_output+="}"

    echo -e "${json_output}"
}

main "$@"
#eof
