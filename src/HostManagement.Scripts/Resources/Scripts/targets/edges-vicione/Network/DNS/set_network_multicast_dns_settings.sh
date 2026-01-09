#!/usr/bin/env bash

# ###
#
# Set Network Multicast DNS Settings Script
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

readonly ERR_CODE_1="Unsupported flag '%s'."
readonly ERR_CODE_2="Missing argument for '%s'."
readonly ERR_CODE_3="Unsupported value '%s'."

#######################################
# Constants
#######################################

readonly DIRECTORY_NETWORK="/etc/systemd/network"
readonly INTERFACE_DIRECTORY_PREFIX="10-netplan-"
readonly INTERFACE_DIRECTORY_SUFFIX=".network.d"

readonly VALUE_TRUE="true"
readonly VALUE_FALSE="false"

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 --multicast-dns-enabled <${VALUE_TRUE}|${VALUE_FALSE}>"
    echo
    echo "Options:"
    echo "  --multicast-dns-enabled - Enables or disables multicast DNS."
    exit 0
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    if [[ "$1" != "--multicast-dns-enabled" ]]; then
        err "$(printf "$ERR_CODE_1" "$1")" 1
    fi

    if [[ -z "${2:-}" ]]; then
    	err "$(printf "$ERR_CODE_2" "--multicast-dns-enabled")" 2
    fi

    if [[ "$2" != "$VALUE_TRUE" ]] && [[ "$2" != "$VALUE_FALSE" ]]; then
        err "$(printf "$ERR_CODE_3" "$2")" 3
    fi

    local multicast_dns_enabled=$2

    network_interfaces=$(Scripts/Network/Helper/get_network_interfaces.sh)

    IFS=$'\n'
    for current in $network_interfaces; do
        interface=$(awk '{print $1}' <<< "$current" | sed 's/ //;s/@.*//')

        local interface_directory="${DIRECTORY_NETWORK}/${INTERFACE_DIRECTORY_PREFIX}${interface}${INTERFACE_DIRECTORY_SUFFIX}"

        if [[ ! -d "$interface_directory" ]]; then
            mkdir "$interface_directory"
        fi

        local interface_conf_path="${interface_directory}/mdns.conf"
        local interface_mdns_state="no"

        if [[ "$multicast_dns_enabled" == "$VALUE_TRUE" ]]; then
            interface_mdns_state="yes"
        fi

        file_content="[Match]\n"
        file_content+="Name=${interface}\n\n"
        file_content+="[Network]\n"
        file_content+="MulticastDNS=${interface_mdns_state}" # Debian 12+ required (previously 'Multicast')

        echo -e "$file_content" > "$interface_conf_path"
    done
}

main "$@"
#eof
