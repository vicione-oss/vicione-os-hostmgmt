#!/usr/bin/env bash

# ###
#
# Set Hostname Script - v1.0.0
#
# The script sets the hostname for the system.
# ###

new_hostname=$1

# Sets the hostname temporary
hostname "${new_hostname}"

# Sets the hostname permanently
hostname > /etc/hostname
sed -i "s/127.0.1.1.*/127.0.1.1\t${new_hostname}/g" /etc/hosts
#eof
