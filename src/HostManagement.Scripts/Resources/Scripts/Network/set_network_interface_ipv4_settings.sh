#!/usr/bin/env bash

# ###
#
# Set Network Interface IPv4 Settings Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Variables
#######################################

DIRECTORY="/etc/netplan"
CONFIGURATION_PATH=""

VLAN_ID=""
IFACE=""
ENABLED="true"
FILE_CONTENT=""
GATEWAY=""
GATEWAY_COUNTER=0
ADDRESS_COUNTER=0

#######################################
# Methods
#######################################

err() {
    echo -e "$1" >&2
    exit "$2"
}

activate_dhcp_mode() {
    local dhcp_enabled=$1

    if [ "${dhcp_enabled}" == "true" ]; then
        FILE_CONTENT+="      dhcp4: true"$'\n'

        write_configuration_to_file
    fi

    FILE_CONTENT+="      dhcp4: false"$'\n'
}

add_gateway() {
    FILE_CONTENT+="      routes:"$'\n'
    FILE_CONTENT+="        - to: 0.0.0.0/0"$'\n'
    FILE_CONTENT+="          via: ${GATEWAY}"$'\n'
}

add_ip_address() {
    local ip_address=$1

    if [ $ADDRESS_COUNTER -eq 0 ]; then
        FILE_CONTENT+="      addresses:"$'\n'
    fi
    
    FILE_CONTENT+="        - ${ip_address}"$'\n'
}

add_netplan_base_configuration() {
    FILE_CONTENT+="#"$'\n'
    FILE_CONTENT+="# This file is managed by ifm HostManagement application."$'\n'
    FILE_CONTENT+="# Manual adjustments may not be compatible."$'\n'
    FILE_CONTENT+="#"$'\n'
    FILE_CONTENT+="network:"$'\n'
    FILE_CONTENT+="  version: 2"$'\n'
    FILE_CONTENT+="  renderer: networkd"$'\n'
    FILE_CONTENT+="  ethernets:"$'\n'
    FILE_CONTENT+="    ${IFACE}:"$'\n'

    if [ "${ENABLED}" == "false" ]; then
        FILE_CONTENT+="      activation-mode: off"$'\n'
    fi

    if [ -n "${VLAN_ID}" ] ; then
        FILE_CONTENT+="      dhcp4: false"$'\n'
        FILE_CONTENT+="  vlans:"$'\n'
        FILE_CONTENT+="    vlan${VLAN_ID}:"$'\n'
        FILE_CONTENT+="      id: ${VLAN_ID}"$'\n'
        FILE_CONTENT+="      link: ${IFACE}"$'\n'
    fi
}

check_for_network_interface() {
    Scripts/Network/Helper/is_network_interface_available.sh "${IFACE}"

    if [ $? -eq 1 ]; then
        err "The interface '${IFACE}' does not exist." 2
    fi
}

write_configuration_to_file() {
    echo "${FILE_CONTENT}" > "${CONFIGURATION_PATH}"
    chmod 600 "${CONFIGURATION_PATH}"
    exit 0
}

#######################################
# Argument handling
#######################################

main() {
    if [[ "$1" == "--help" ]] || [[ "$1" == "-h" ]]; then
        echo "The order of the parameters must be observed!"
        echo "Examples:"
        echo
        echo "$0"
        echo "    --vlan-id 10 --interface-name ens33 --enabled true --dhcp-enabled true"
        echo 
        echo "$0"
        echo "    --vlan-id 10 --interface-name ens33 --enabled true --dhcp-enabled false"
        echo "    --ip-address 192.168.2.2/24 --gateway 192.168.2.1"
        echo "    --ip-address 192.168.2.3/24"

        exit 0
    fi

    if [[ "$1" -eq 0 ]]; then
        err "Please enter all required parameters." 1
    fi

    while (("$#")); do
        case "$1" in
            --vlan-id)
                # 0:      No VLAN, only Qos-Tags;
                # 1:      Standard VLAN
                # 2-4094: User defined VLAN
                # 4095:   Reserved for internal usage
                if [[ "$2" -gt 0 ]]; then
                    VLAN_ID="$2"
                
                    if [[ "${VLAN_ID}" -lt 1 || "${VLAN_ID}" -gt 4094 ]]; then
                        err "An VLAN-ID must be greater than or equal to 1 and less than or equal to 4094." 5
                    fi
                fi
                ;;
            --interface-name)
                IFACE="$2"
                check_for_network_interface
                CONFIGURATION_PATH="${DIRECTORY}/50-${2}.yaml"
                ;;
            --enabled)
                ENABLED="$2"
                add_netplan_base_configuration
                ;;
            --dhcp-enabled)
                activate_dhcp_mode "$2"
                ;;
            --ip-address)
                add_ip_address "$2"
                ((ADDRESS_COUNTER++))
                ;;
            --gateway)
                if [ $GATEWAY_COUNTER -gt 0 ]; then
                    err "Only one gateway can be defined. The gateway '${GATEWAY}' is already in use." 3
                fi 
                GATEWAY="$2"
                ((GATEWAY_COUNTER++))
                ;;
            -*)
                err "Unsupported flag ${1}" 4
                ;;
        esac
        shift 2
    done

    if [[ -n "${GATEWAY}" ]]; then
        add_gateway
    fi

    write_configuration_to_file
}

main "$@"
#eof
