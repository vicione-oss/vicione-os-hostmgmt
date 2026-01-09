#!/usr/bin/env bash

# ###
#
# Get System Architecture Script - v1.0.0
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
# Argument handling
#######################################

architecture=$(uname -m)

if [[ "$architecture" == "amd64" || "$architecture" == "x86_64" ]]; then
    echo "amd64"
elif [[ "$architecture" == "aarch64" ]]; then
    echo "arm64"
else
    echo "Unknown architecture: ${architecture}"
fi
#eof
