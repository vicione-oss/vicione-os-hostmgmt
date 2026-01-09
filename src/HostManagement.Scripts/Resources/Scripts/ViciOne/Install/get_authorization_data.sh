#!/usr/bin/env bash

# ###
#
# Get Authorization Data Script - v1.0.0
#
# The script creates the authorization header
# for accessing the ViciOne Suite repository.
# ###

username=$1
password=$2

authorization=$(echo -n "${username}:${password}" | base64)

echo -e "Authorization: Basic ${authorization}"
#eof
