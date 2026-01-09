#!/usr/bin/env bash

# ###
#
# Restart NTP Service Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

SERVICE_NAME="systemd-timesyncd"

systemctl restart "${SERVICE_NAME}"
Scripts/Common/wait_for_service.sh "${SERVICE_NAME}"
#eof
