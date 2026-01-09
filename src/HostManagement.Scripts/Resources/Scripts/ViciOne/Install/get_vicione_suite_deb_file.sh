#!/usr/bin/env bash

# ###
#
# Get ViciOne Suite DEB File Script - v1.0.0
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
        echo "  --url           - The URL of the ViciOne Suite repository."
        echo "  --deb-file      - The deb-file to be downloaded."
        echo "  --username      - The username for the ViciOne Suite repository."
        echo "  --password      - The password for the ViciOne Suite repository."
        echo "  --download-path - The target folder for the download process."

        exit 0
    fi

    while [[ "$#" -gt 0 ]]; do
        case $1 in
            --url)           url="$2";           shift;;
            --deb-file)      deb_file="$2";      shift;;
            --username)      username="$2";      shift;;
            --password)      password="$2";      shift;;
            --download-path) download_path="$2"; shift;;
        esac
        shift
    done

    if [[ -z "${url}" ]] || [[ -z "${deb_file}" ]] || [[ -z "${username}" ]] || [[ -z "${password}" ]] || [[ -z "${download_path}" ]]; then
        err "Please enter all required parameters." 1
    fi

    if [[ "${url}" == */ ]]; then
        url="${url::-1}"
    fi

    url_deb_file="${url}/${deb_file}"

    authorization=$(Scripts/ViciOne/Install/get_authorization_data.sh "${username}" "${password}")
    status=$(wget --spider --server-response --header="${authorization}" "${url_deb_file}" 2>&1)
    status_code=$?

    if [ $status_code -ne 0 ]; then
        err "Could not access ViciOne Suite repository. wget exit code: ${status_code}" 2
    fi

    status=$(echo "${status}" | grep "HTTP/" | awk '{print $2}' | tail -n 1)

    if [ "$status" -ne 200 ]; then
        err "The ViciOne Suite repository could not be reached. Http-Code: ${status}" 3
    fi

    deb_file_path="${download_path}/${deb_file}"

    wget -nv -O "${deb_file_path}" --header="${authorization}" "${url_deb_file}"

    if [ ! -e "${deb_file_path}" ]; then
        err "The deb file could not be loaded from the ViciOne Suite repository." 4
    fi

    repository="${url#*/repository/}"
    repository="${repository%%/*}"
    repository_path="${url#*/repository/"${repository}"/}"
    url_schema="${url%%://*}"
    url_info="${url#*://}"
    url_info="${url_schema}://${url_info%%/*}/service/rest/v1/search?repository=${repository}&name=/${repository_path}/${deb_file}"

    content=$(wget --header="${authorization}" -qO- "${url_info}" 2>&1)
    sha256Json=$(echo "${content}" | grep "sha256" | awk -F': ' '{print $2}' | tr -d '",')
    sha256Calculated=$(sha256sum -b "${deb_file_path}" | cut -d ' ' -f 1)

    if [ "${sha256Json}" != "${sha256Calculated}" ]; then
        rm -f "${deb_file_path}"
        err "The hash value of the loaded deb file does not match the hash value from the ViciOne Suite repository." 5
    fi
}

main "$@"
#eof
