#!/usr/bin/env bash

# ###
#
# Install DEB File Script - v1.0.0
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

warn() {
    echo -e "$1" >&2
    exit 0
}

delete_deb_file() {
    if [ -f "$1" ]; then
        rm -f "$1"
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ "${1}" == "--help" ]] || [[ "${1}" == "-h" ]]; then
        echo "Usage: ${0} [Option]..."
        echo ""
        echo "Options:"
        echo "  --deb-file          - The deb-file to be downloaded."
        echo "  --download-path     - The target folder for the download process."
        echo "  [--force-reinstall] - Forces the reinstallation of the deb-file."

        exit 0
    fi

    force_reinstall=0
    export DEBIAN_FRONTEND="noninteractive"

    while [[ "$#" -gt 0 ]]; do
        case $1 in
            --deb-file)        deb_file="$2";      shift;;
            --download-path)   download_path="$2"; shift;;
            --force-reinstall) force_reinstall=1;  shift;;
        esac
        shift
    done

    if [[ -z "${deb_file}" ]] || [[ -z "${download_path}" ]]; then
        err "Please enter all required parameters." 1
    fi

    if [[ "${download_path}" == */ ]]; then
        download_path="${download_path::-1}"
    fi

    deb_file_path="${download_path}/${deb_file}"

    if [ ! -f "${deb_file_path}" ]; then
        err "The required deb file is not available." 2
    fi

    package_info=$(dpkg-deb --info "${deb_file_path}")
    package_name=$(echo "${package_info}" | grep 'Package:' | awk '{print $2}')
    package_version=$(echo "${package_info}" | grep 'Version:' | awk '{print $2}')

    if [ -z "${package_name}" ] || [ -z "${package_version}" ]; then
        delete_deb_file "${deb_file_path}"
        err "Package information could not be extracted." 3
    fi

    installed_version=$(Scripts/ViciOne/Install/get_installed_package_version.sh "${package_name}")

    if dpkg --compare-versions "${package_version}" lt "${installed_version}"; then
        delete_deb_file "${deb_file_path}"
        err "A version downgrade is not permitted." 4
    fi

    if [ "${installed_version}" == "${package_version}" ]; then
        if [[ force_reinstall -eq 0 ]]; then
            delete_deb_file "${deb_file_path}"
            warn "${package_name} is already the newest version (${package_version}). Use --force-reinstall to force reinstall."
        fi
    fi

    download_path_rights=$(stat -c "%a" "${download_path}")

    chmod 755 "${download_path}"                       # Mit Max klären, ob Rechte entsprechend gesetzt sind

    if [[ "${installed_version}" != "0.0.0.0" ]]; then
        apt-get remove -y -qq "${package_name}"  > /dev/null

        status_code=$?

        if [ $status_code -ne 0 ]; then
            delete_deb_file "${deb_file_path}"
            err "Could not remove the current version. apt exit code: ${status_code}" 5
        fi
    fi

    apt-get install -y -qq "${deb_file_path}" > /dev/null

    status_code=$?

    chmod "${download_path_rights}" "${download_path}" # Mit Max klären, ob Rechte entsprechend gesetzt sind

    if [ $status_code -ne 0 ]; then
        delete_deb_file "${deb_file_path}"
        err "Could not install .deb-file. apt exit code: ${status_code}" 6
    fi

    delete_deb_file "${deb_file_path}"
}

main "$@"
#eof
