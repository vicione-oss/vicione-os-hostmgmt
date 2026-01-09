#!/usr/bin/env bash

# ###
#
# Is Network Interface Available Script - v1.0.0
#
# The script returns 0 if the given network interface is available, otherwise 1.
# ###

iface_with_at=$1
iface_without_at=$(echo "${iface_with_at}" | sed 's/ //;s/@.*//')

if ip link show "${iface_with_at}" > /dev/null 2>&1 || ip link show "${iface_without_at}" > /dev/null 2>&1; then
    exit 0
fi

exit 1
#eof
