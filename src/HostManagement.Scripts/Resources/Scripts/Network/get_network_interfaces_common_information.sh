#!/usr/bin/env bash

# ###
#
# Get Network Interfaces Common Information Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Methods
#######################################

warn() {
    echo -e "$1" >&2
}

#######################################
# Argument handling
#######################################

main() {
    script_path=$(readlink -f "Scripts/Network/Helper/get_network_interfaces.sh")

    if [ ! -x "$script_path" ]; then
        warn "Permission denied: The script $script_path does not have execute permission."
    fi

    network_interfaces=$($script_path)
    network_interface_count=$(echo "${network_interfaces}" | awk 'NF' | wc -l)

    if [ "${network_interface_count}" -eq 0 ]; then
        warn "No network interfaces have been found."
        echo -e "[ ]"
        exit 0
    fi

    json="[ "

    IFS=$'\n'
    for current in $network_interfaces;
    do
        name=${current%% *}
        state=$(echo "${current}" | awk '$2 !~ /UP/ {print "false"} $2 ~ /UP/ {print "true"}')
        address=$(echo "${current}" | awk '{ if (NF==4) { print $3 } else { print "none" } }')

        json="${json} {\"Enabled\": ${state}, \"Name\": \"${name}\", \"PhysicalAddress\": \"${address}\"}, "
    done

    json="${json::-2} ]"

    echo -e "${json}"
}

main
#eof
