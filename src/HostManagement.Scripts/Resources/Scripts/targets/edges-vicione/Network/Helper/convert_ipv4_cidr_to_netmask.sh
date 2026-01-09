#!/usr/bin/env bash

# ###
#
# Convert IPv4 CIDR To Netmask Script
#
# Converts the given IPv4 CIDR length to the corresponding netmask.
#
# Example: 
#   16 ==> 255.255.0.0 
#   24 ==> 255.255.255.0
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

readonly ERR_CODE_1="No CIDR notation specified."
readonly ERR_CODE_2="The CIDR notation must be a number from 0 to 32."

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -lt 1 ]]; then
        err "$ERR_CODE_1" 1
    fi

    cidr=$1

    if (( cidr < 0 || cidr > 32 )); then
        err "$ERR_CODE_2" 2
    fi

    netmask=""
    full_octets=$((cidr / 8))
    partial_octet=$((cidr % 8))

    for ((i=0; i<4; i++)); do
        if [ $i -lt $full_octets ]; then
            netmask+="255."
        elif [ $i -eq $full_octets ]; then
            netmask+=$((256 - 2**(8 - partial_octet))).
        else
            netmask+="0."
        fi
    done

    echo "${netmask%?}"
}

main "$@"
#eof
