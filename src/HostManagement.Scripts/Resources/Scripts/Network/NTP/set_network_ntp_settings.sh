#!/usr/bin/env bash

# ###
#
# Set Network NTP Settings Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Variables
#######################################

DIRECTORY="/etc/systemd/timesyncd.conf.d"
CONFIGURATION_PATH="${DIRECTORY}/50-hostmanagement.conf"

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
    if [[ "$1" == "--help" ]] || [[ "$1" == "-h" ]]; then
        echo "Examples:"
        echo
        echo "$0"
        echo "    --ntpservers pool.ntp.org time.google.com"

        exit 0
    fi

    if [[ "$1" -eq 0 ]]; then
        err "Please enter all required parameters." 1
    fi

    ntp_servers=()

    while (("$#")); do
        case "$1" in
            --ntpservers)
                shift
                while [[ -n "$1" ]] && [[ "$1" != "--ntpservers" ]]; do
                    ntp_servers+=("$1")
                    shift
                done
                ;;
            -*)
                err "Unsupported flag ${1}" 4
                ;;
        esac
    done

    if [ ! -d "${DIRECTORY}" ]; then
        mkdir "${DIRECTORY}"
    fi

    file_content+="[Time]\n"
    file_content+="NTP="   

    if [ ${#ntp_servers[@]} -eq 0 ]; then
        file_content="#${file_content}"
    else
        file_content+=$(printf '%s ' "${ntp_servers[@]}" | sed 's/ $//')
    fi

    echo -e "${file_content}" > "${CONFIGURATION_PATH}"
}

main "$@"
#eof
