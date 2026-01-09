#!/usr/bin/env bash

# ###
#
# Get Network Interface IPv4 Settings Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Variables
#######################################
IFACE=""
NETPLAN_ENTRY_POINT=""
VLAN_ID="0"
VLAN_ENABLED="false"

#######################################
# Methods
#######################################

err() {
    echo -e "$1" >&2
    exit "$2"
}

define_netplan_entrypoint() {
    vlan_name=$(netplan get vlans | awk -F: '/^[^ ]+:$/ {vlan=$1} /link: "'"$IFACE"'"/ {print vlan}')

    if [ -n "$vlan_name" ]; then
        NETPLAN_ENTRY_POINT="vlans.${vlan_name//:/}"
        VLAN_ID=${vlan_name//[!0-9]/}
        VLAN_ENABLED="true"
    else
        NETPLAN_ENTRY_POINT="ethernets.${IFACE}"
    fi
}

#######################################
# Argument handling
#######################################

main() {
    IFACE=$1

    if [ -z "${IFACE}" ]; then
        err "Usage: $0 <Interface>" 1
    fi

    Scripts/Network/Helper/is_network_interface_available.sh "${IFACE}"

    if [ $? -eq 1 ]; then
        err "The interface '${IFACE}' does not exist." 2
    fi

    define_netplan_entrypoint

    dhcp=$(netplan get "${NETPLAN_ENTRY_POINT}".dhcp4)

    if [[ "$dhcp" == "null" || -z "$dhcp" ]]; then
        dhcp="false"
    fi

    gateway=$(netplan get "${NETPLAN_ENTRY_POINT}".routes | grep 'via:' | cut -d: -f2)
    ip_addresses=$(netplan get "${NETPLAN_ENTRY_POINT}".addresses | sed 's/-//g' | sed 's/"//g' | sed 's/ //g')

    if [ "${ip_addresses}" == "null" ]; then
        ip_details=""
    else
        IFS=$'\n' read -d '' -r -a ip_address_array <<< "${ip_addresses}"

        for current in "${ip_address_array[@]}"; do
            ip_address=$(cut -d'/' -f1 <<< "${current}")
            cidr=$(cut -d'/' -f2 <<< "${current}")
            netmask=$(Scripts/Network/Helper/convert_ipv4_cidr_to_netmask.sh "${cidr}")

            ip_details+=("{\"IPAddress\": \"${ip_address}\", \"Netmask\": \"${netmask}\"}")
        done
    fi

    if [ "${gateway}" == "" ]; then
        gateway="null"
    fi

    json_output="{"
    json_output+="\"DHCPEnabled\": ${dhcp},"
    json_output+="\"Gateway\": ${gateway},"
    json_output+="\"IPv4Details\": [$(printf '%s, ' "${ip_details[@]}" | sed 's/, $//')],"
    json_output+="\"VLANID\": ${VLAN_ID}",
    json_output+="\"VLANEnabled\": ${VLAN_ENABLED}"
    json_output+="}"

    echo -e "${json_output}"
}

main "$@"
#eof
