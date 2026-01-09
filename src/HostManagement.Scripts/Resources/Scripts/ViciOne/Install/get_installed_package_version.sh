#!/usr/bin/env bash

# ###
#
# Get Installed Package Version Script - v1.0.0
#
# The script gets the currently installed version
# of the given package.
# ###

package="${1:-}"

export DEBIAN_FRONTEND="noninteractive"

version=$(LANG=EN apt-cache policy "$package" | grep Installed | cut -d ' ' -f4)

if [[ -z "$version" || "$version" == "(none)" ]]; then
    version="0.0.0.0"
fi

echo "$version"
#eof
