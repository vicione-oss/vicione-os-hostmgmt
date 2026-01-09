#!/usr/bin/env bash

# ###
#
# Get System Architecture Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Methods
#######################################

err() {
    echo -e "$1" >&2
    exit "$2"
}

#######################################
# Argument handling
#######################################

main() {
    architecture=$(uname -m)

    if [[ "$architecture" == "amd64" ]] || [[ "$architecture" == "x86_64" ]]; then
        echo -e "amd64"
    elif [[ "$architecture" == "aarch64" ]]; then
        echo -e "arm64"
    else
        echo -e "Unknown architecture: ${architecture}"
    fi
}

main "$@"
#eof
