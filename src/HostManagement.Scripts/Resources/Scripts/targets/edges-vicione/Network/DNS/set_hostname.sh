#!/usr/bin/env bash

# ###
#
# Set Hostname Script
#
# The script sets the hostname for the system.
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

readonly HOSTS_FILE_PATH="/etc/hosts"

#######################################
# Methods
#######################################

usage() {
    echo "Usage: $0 <hostname>"
    exit 0
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    new_hostname=$1

    # Sets the hostname temporary
    hostname "$new_hostname"

    # Sets the hostname permanently
    hostname > /etc/hostname
    sed -i "s/127.0.1.1.*/127.0.1.1\t${new_hostname}/g" "$HOSTS_FILE_PATH"
}

main "$@"
#eof
