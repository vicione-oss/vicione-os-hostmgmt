#!/usr/bin/env bash

# ###
#
# Get ViciOne Suite Versions Script - v1.0.0
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
    if [[ "${1}" == "--help" ]] || [[ "${1}" == "-h" ]]; then
        echo "Usage: ${0} [Option]..."
        echo ""
        echo "Options:"
        echo "  --url      - The URL of the ViciOne Suite repository."
        echo "  --username - The username for the ViciOne Suite repository."
        echo "  --password - The password for the ViciOne Suite repository."

        exit 0
    fi

    while [[ "$#" -gt 0 ]]; do
        case $1 in
            --url)      url="$2";      shift;;
            --username) username="$2"; shift;;
            --password) password="$2"; shift;;
        esac
        shift
    done

    if [[ -z "${url}" ]] || [[ -z "${username}" ]] || [[ -z "${password}" ]]; then
        err "Please enter all required parameters." 1
    fi

    if [[ "${url}" == */ ]]; then
        url="${url::-1}"
    fi

    url="${url}/versions"

    authorization=$(Scripts/ViciOne/Install/get_authorization_data.sh "${username}" "${password}")
    status=$(wget --spider --server-response --header="${authorization}" "${url}" 2>&1)
    status_code=$?

    if [ $status_code -eq 4 ]; then
        err "Could not access ViciOne Suite repository. Network failure" 2
    elif [ $status_code -eq 6 ]; then
        err "Could not access ViciOne Suite repository. Username/password authentication failure" 3
    elif [ $status_code -ne 0 ]; then
        err "Could not access ViciOne Suite repository. wget exit code: ${status_code}" 4
    fi

    status=$(echo "${status}" | grep "HTTP/" | awk '{print $2}' | tail -n 1)

    if [ "$status" -ne 200 ]; then
        err "Could not load versions file. Http-Code: ${status}" 5
    fi

    content=$(wget --header="${authorization}" -qO- "${url}" 2>&1)

    echo -e "${content}"
}

main "$@"
#eof
