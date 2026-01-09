#!/usr/bin/env bash

# ###
#
# Get Installed Package Version Script - v1.0.0
#
# The script gets the current installed version
# of the given package.
# ###

package=$1

export DEBIAN_FRONTEND="noninteractive"

version=$(LANG=EN apt-cache policy "${package}" | grep Installed | awk '{print $2}')

if [[ -z "${version}" ]] || [[ "${version}" == "(none)" ]]; then
    echo -e "0.0.0.0"
else
    echo -e "${version}"
fi
#eof
