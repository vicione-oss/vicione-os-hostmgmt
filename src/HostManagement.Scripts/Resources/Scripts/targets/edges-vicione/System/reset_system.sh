#!/usr/bin/env bash

# ###
#
# Reset System Script
#
# The script returns 0 if ok, non zero for error.
# ###

set -eEuo pipefail

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

# Delete persistent system configuration

systemConfigurationDir="${HOSTMANAGEMENT__SYSTEM_CONFIGURATION_DIRECTORY:-}"

if [[ -n "$systemConfigurationDir" && -f "$systemConfigurationDir/SystemConfiguration.json" ]]; then
    rm -f "$systemConfigurationDir/SystemConfiguration.json"
fi

# System restart is required if script called manually!
# eof
