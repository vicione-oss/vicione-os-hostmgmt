#!/usr/bin/env bash

# ###
#
# Get Network NTP Fallback Information Script
#
# The script returns 0 if ok, non zero for error.
# ###

set -eEuo pipefail

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

fallback_ntp_servers=$(timedatectl show-timesync --property=FallbackNTPServers | cut -d "=" -f2-)

echo -e "$fallback_ntp_servers"
#eof
