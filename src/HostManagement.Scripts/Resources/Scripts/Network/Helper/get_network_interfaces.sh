#!/usr/bin/env bash

# ###
#
# Get Network Interfaces Script - v1.0.0
#
# Gets a list of physical and virtual network interfaces.
# The physical network interfaces are only listed
# if they are not connected to a virtual network interface.
# ###

interfaces=$(ip -br link show | grep -v lo | grep -v docker | grep -vE '^vlan[0-9]+@')

physical_interfaces=()
virtual_interfaces=()

declare -A interfaces_details

while read -r current; do
    interface="${current%% *}"

    if grep -q '@' <<< "${interface}"; then
        virtual_interfaces+=("${interface}")
    else
        physical_interfaces+=("${interface}")
    fi

    interfaces_details["${interface}"]="${current}"
done <<< "$interfaces"

for physical in "${physical_interfaces[@]}"; do
    found=false

    for virtual in "${virtual_interfaces[@]}"; do
        if [[ "${virtual}" == *@$physical ]]; then
            found=true
            break;
        fi
    done

    if [ "${found}" = false ]; then
        result+=("${physical}")
    fi
done

for virtual in "${virtual_interfaces[@]}"; do
    result+=("${virtual}")
done

for item in "${result[@]}"; do
    echo "${interfaces_details[$item]}" | sed 's/@[^ ]*/ /'
done

#eof
