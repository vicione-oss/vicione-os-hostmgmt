#!/usr/bin/env bash

# ###
#
# Set Network Multicast DNS Settings Script - v1.0.0
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
    if [[ "$1" == "--help" ]] || [[ "$1" == "-h" ]]; then
        echo "Examples:"
        echo
        echo "$0"
        echo "    --multicast-dns-enabled true"

        exit 0
    fi

    if [[ "$1" -eq 0 ]]; then
        err "Please enter all required parameters." 1
    fi

    multicast_dns_enabled=$2

    network_interfaces=$(Scripts/Network/Helper/get_network_interfaces.sh)

    IFS=$'\n'
    for current in $network_interfaces; do
        iface=$(echo "${current}" | awk '{print $1}' | sed 's/ //;s/@.*//')

        if [[ "${multicast_dns_enabled}" == "true" ]]; then
            resolvectl mdns ${iface} yes
        else
            resolvectl mdns ${iface} no
        fi
    done
}

main "$@"
#eof
