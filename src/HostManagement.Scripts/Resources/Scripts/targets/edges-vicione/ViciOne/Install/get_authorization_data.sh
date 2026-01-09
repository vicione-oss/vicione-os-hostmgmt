#!/usr/bin/env bash

# ###
#
# Get Authorization Data Script
#
# The script generates a basic authorization header
# for HTTP requests.
# ###

username="${1:-}"
password_or_token="${2:-}"

authorization=$(echo -n "${username}:${password_or_token}" | base64 -w 0)

echo "Authorization: Basic ${authorization}"
#eof
