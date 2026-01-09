#!/usr/bin/env bash

# ###
#
# Get ViciOne Suite Installed Version Script - v1.0.0
#
# The script gets the current installed version
# of the ViciOne Suite.
# ###

version=$(Scripts/ViciOne/Install/get_installed_package_version.sh vicione-suite)

echo "$version"
#eof
