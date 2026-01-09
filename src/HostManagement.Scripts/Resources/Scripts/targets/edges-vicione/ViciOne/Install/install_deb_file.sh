#!/usr/bin/env bash

# ###
#
# Install DEB File Script
#
# The script returns 0 if ok or warning, non zero for error.
# ###

set -eEuo pipefail

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

#######################################
# Error codes
#######################################

readonly ERR_CODE_1="Missing argument for '%s'."
readonly ERR_CODE_2="Unsupported flag '%s'."
readonly ERR_CODE_3="Please enter all required parameters."
readonly ERR_CODE_4="The required deb file is not available."
readonly ERR_CODE_5="Package information could not be extracted."
readonly ERR_CODE_6="A version downgrade is not permitted."
readonly ERR_CODE_7="Could not remove the current version. apt exit code: %s"
readonly ERR_CODE_8="Could not install .deb-file. apt exit code: %s"

#######################################
# Warning codes
#######################################

readonly WARN_CODE_1="%s is already the newest version (%s). Use --force-reinstall to force reinstall."

#######################################
# Methods
#######################################

usage() {
    echo "Usage: ${0} --deb-file <deb-file-name> --download-path <download-path> [--force-reinstall]"
    echo
    echo "Options:"
    echo "  --deb-file        - The name of the deb-file to be installed."
    echo "  --download-path   - The folder in which the deb file was downloaded."
    echo "  --force-reinstall - Forces the reinstallation of the deb-file. (optional)"
    exit 0
}

warn() {
    echo -e "$1" >&2
    exit 0
}

assert_flag_has_argument() {
    local flag_argument="$1"
    local flag_name="$2"

    if [[ -z "$flag_argument" || "$flag_argument" == --* ]]; then
        err "$(printf "$ERR_CODE_1" "$flag_name")" 1
    fi
}

delete_deb_file() {
    if [[ -f "$1" ]]; then
        rm -f "$1"
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    local deb_file=""
    local download_path=""
    local force_reinstall=0
    
    export DEBIAN_FRONTEND="noninteractive"

    while (("$#")); do
        case $1 in
            --deb-file)
                assert_flag_has_argument "${2:-}" "$1" 
                deb_file="$2"
                shift
                ;;
            --download-path)
                assert_flag_has_argument "${2:-}" "$1" 
                download_path="$2"
                shift
                ;;
            --force-reinstall)
                force_reinstall=1
                ;;
            *)
                err "$(printf "$ERR_CODE_2" "$1")" 2
                ;;
        esac
        shift
    done

    if [[ -z "$deb_file" || -z "$download_path" ]]; then
        err "$ERR_CODE_3" 3
    fi

    if [[ "$download_path" == */ ]]; then
        download_path="${download_path::-1}"
    fi

    deb_file_path="${download_path}/${deb_file}"

    if [[ ! -f "$deb_file_path" ]]; then
        err "$ERR_CODE_4" 4
    fi

    package_info=$(dpkg-deb --info "$deb_file_path")
    package_name=$(grep 'Package:' <<< "$package_info" | awk '{print $2}')
    package_version=$(grep 'Version:' <<< "$package_info" | awk '{print $2}')

    if [[ -z "$package_name" || -z "$package_version" ]]; then
        delete_deb_file "$deb_file_path"
        err "$ERR_CODE_5" 5
    fi

    installed_version=$(Scripts/ViciOne/Install/get_installed_package_version.sh "$package_name")

    if dpkg --compare-versions "$package_version" lt "$installed_version"; then
        delete_deb_file "$deb_file_path"
        err "$ERR_CODE_6" 6
    fi

    if [[ "$installed_version" == "$package_version" ]]; then
        if [[ force_reinstall -eq 0 ]]; then
            delete_deb_file "$deb_file_path"
            warn "$(printf "$WARN_CODE_1" "$package_name" "$package_version")"
        fi
    fi

    download_path_rights=$(stat -c "%a" "${download_path}")

    chmod 755 "$download_path"

    if [[ "$installed_version" != "0.0.0.0" ]]; then
        apt-get remove -y -qq "$package_name"  > /dev/null

        status_code=$?

        if [[ $status_code -ne 0 ]]; then
            delete_deb_file "$deb_file_path"
            err "$(printf "$ERR_CODE_7" "$status_code")" 7
        fi
    fi

    apt-get install -y -qq "$deb_file_path" > /dev/null

    status_code=$?

    chmod "$download_path_rights" "$download_path"

    if [[ $status_code -ne 0 ]]; then
        delete_deb_file "$deb_file_path"
        err "$(printf "$ERR_CODE_8" "$status_code")" 8
    fi

    delete_deb_file "$deb_file_path"
}

main "$@"
#eof
