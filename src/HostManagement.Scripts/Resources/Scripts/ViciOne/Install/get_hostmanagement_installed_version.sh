#!/usr/bin/env bash

# ###
#
# Get HostManagement Installed Version Script - v1.0.0
#
# The script gets the current installed version
# of the Host-Management.
# ###

version=$(Scripts/ViciOne/Install/get_installed_package_version.sh hostmanagement)

echo -e "${version}"
#eof
