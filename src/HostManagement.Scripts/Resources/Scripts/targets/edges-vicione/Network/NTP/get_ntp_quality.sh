#!/bin/bash

# ###
#
# Get NTP quality script
#
# The script returns 0 if ok or non zero for error.
# ###

set -eEuo pipefail

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

########################################
# Constants
########################################

readonly UNKNOWN_QUALITY="Unknown"

########################################
# Variables
########################################

MESSAGE=""
SYNCHRONIZED="false"
SERVER_ADDRESS=""
SERVER_NAME=""
STRATUM="16"
STRATUM_QUALITY="$UNKNOWN_QUALITY"
OFFSET="0"
OFFSET_QUALITY="$UNKNOWN_QUALITY"
JITTER="0"
JITTER_QUALITY="$UNKNOWN_QUALITY"

########################################
# Methods
########################################

get_jitter_enum() {
    if [ -z "$JITTER" ]; then
        echo "$UNKNOWN_QUALITY"
        return
    fi

    LC_NUMERIC=C awk -v val="$JITTER" '
        BEGIN {
            if (val < 1) { print "Excellent"; exit }
            if (val < 5) { print "Good"; exit }
            if (val < 20) { print "Fair"; exit }
            print "Poor"
        }
    '
}

get_offset_enum() {
    if [ -z "$OFFSET" ]; then
        echo "$UNKNOWN_QUALITY"
        return
    fi

    LC_NUMERIC=C awk -v raw_val="$OFFSET" '
        BEGIN {
            val = (raw_val < 0) ? -raw_val : raw_val

            if (val < 1) { print "Excellent"; exit }
            if (val < 10) { print "Good"; exit }
            if (val < 100) { print "Fair"; exit }
            print "Poor"
        }
    '
}

get_stratum_enum() {
    case "$STRATUM" in
        ""|0) echo "$UNKNOWN_QUALITY" ;;
        1|2)  echo "High" ;;
        3|4)  echo "Medium" ;;
        *)    echo "Low" ;;
    esac
}

get_timesync_quality() {
    local status

    status=$(timedatectl timesync-status 2>/dev/null)

    SERVER_ADDRESS=$(echo "$status" | awk '/Server:/ { print $2 }')
    SERVER_NAME=$(echo "$status" | awk '/Server:/ { print $3 }' | tr -d '()')

    if [ "$SERVER_ADDRESS" == "n/a" ]; then
        SERVER_ADDRESS=""
        SERVER_NAME=""
    fi

    tmp_stratum=$(echo "$status" | awk '/Stratum:/ { print $2 }')

    [ -n "$tmp_stratum" ] && STRATUM="$tmp_stratum"

    if [ -z "$status" ] || [ -z "$SERVER_ADDRESS" ] || [ "$STRATUM" -eq 16 ] 2>/dev/null; then
        MESSAGE="The system time has not been synchronized. The system is searching for time servers."
    else
        STRATUM_QUALITY=$(get_stratum_enum)

        if [[ "$STRATUM_QUALITY" != "$UNKNOWN_QUALITY" ]]; then
            SYNCHRONIZED="true"
        fi

        local offset_raw
        local jitter_raw

        offset_raw=$(echo "$status" | awk '/Offset:/ { print $2 }')
        jitter_raw=$(echo "$status" | awk '/Jitter:/ { print $2 }')

        OFFSET=$(to_milliseconds "$offset_raw")
        JITTER=$(to_milliseconds "$jitter_raw")

        OFFSET_QUALITY=$(get_offset_enum)
        JITTER_QUALITY=$(get_jitter_enum)
    fi
}

to_milliseconds() {
    local value=$1
    local clean_value="${value//,/.}"

    if [[ $value == *ms ]]; then
        echo "${clean_value%ms}" | LC_NUMERIC=C awk '{ printf "%.6f\n", $1 }'
    elif [[ $value == *us ]] || [[ "$value" == *µs ]]; then
        echo "${clean_value%*s}" | LC_NUMERIC=C awk '{ printf "%.6f\n", $1 / 1000 }'
    elif [[ $value == *s ]]; then
        echo "${clean_value%s}" | LC_NUMERIC=C awk '{ printf "%.6f\n", $1 * 1000 }'
    else
        echo "0.0"
        return
    fi | sed 's/\.*0*$//;s/^\./0./;s/^$/0/'
}

main() {
    get_timesync_quality

    local json_output="{"

    json_output+="\"Synchronized\": ${SYNCHRONIZED},"
    json_output+="\"ServerAddress\": \"${SERVER_ADDRESS}\","
    json_output+="\"ServerName\": \"${SERVER_NAME}\","
    json_output+="\"Stratum\": ${STRATUM},"
    json_output+="\"StratumQuality\": \"${STRATUM_QUALITY}\","
    json_output+="\"Offset\": ${OFFSET},"
    json_output+="\"OffsetQuality\": \"${OFFSET_QUALITY}\","
    json_output+="\"Jitter\": ${JITTER},"
    json_output+="\"JitterQuality\": \"${JITTER_QUALITY}\","
    json_output+="\"Message\": \"${MESSAGE}\""
    json_output+="}"

    echo "$json_output"
}

main "$@"
#eof
