#!/usr/bin/env bash

# ###
#
# Apply Netplan Configuration Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

# Removes all open vlan connections.
# Netplan cannot do this independently.

open_vlans=$(ip -o link show type vlan | grep -oE 'vlan[0-9]+' | tr -d ' ')

if [ -n "${open_vlans}" ]; then
    for current in $open_vlans; do
        ip link delete "${current}"
    done
fi

# Applies the netplan configuration

netplan apply
#eof
