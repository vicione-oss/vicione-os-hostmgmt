#!/usr/bin/env bash

# ###
#
# Convert IPv4 CIDR To Netmask Script - v1.0.0
#
# Converts the given IPv4 CIDR length to the corresponding netmask.
#
# Example: 
#   16 ==> 255.255.0.0 
#   24 ==> 255.255.255.0
# ###

cidrLength=$1
netmask=""
full_octets=$((cidrLength / 8))
partial_octet=$((cidrLength % 8))

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
#eof
