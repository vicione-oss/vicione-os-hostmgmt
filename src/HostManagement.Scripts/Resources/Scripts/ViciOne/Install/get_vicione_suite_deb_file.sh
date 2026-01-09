#!/usr/bin/env bash

# ###
#
# Get ViciOne Suite DEB File Script
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
readonly ERR_CODE_4="Not supported repository."
readonly ERR_CODE_5="The deb file could not be downloaded from the ViciOne Suite repository."
readonly ERR_CODE_6="Could not extract SHA256 from repository response."
readonly ERR_CODE_7="The hash value of the downloaded deb file does not match the hash value from the ViciOne Suite repository."

#######################################
# Methods
#######################################

usage() {
    echo "Usage: ${0}"
    echo "  --url <url> --deb-file <deb-file-name>"
    echo "  --username <username> --password <password>"
    echo "  --download-path <download-path>"
    echo
    echo "Options:"
    echo "  --url           - The URL of the ViciOne Suite repository."
    echo "  --deb-file      - The name of the deb-file to be downloaded."
    echo "  --username      - The username for the ViciOne Suite repository."
    echo "  --password      - The password for the ViciOne Suite repository."
    echo "  --download-path - The target folder for the download process."
    exit 0
}

assert_flag_has_argument() {
    local flag_argument="$1"
    local flag_name="$2"

    if [[ -z "$flag_argument" || "$flag_argument" == --* ]]; then
        err "$(printf "$ERR_CODE_1" "$flag_name")" 1
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    local url=""
    local deb_file=""
    local username=""
    local password=""
    local download_path=""

    while (("$#")); do
        case $1 in
            --url)
                assert_flag_has_argument "${2:-}" "$1" 
                url="$2"
                ;;
            --deb-file)
                assert_flag_has_argument "${2:-}" "$1" 
                deb_file="$2"
                ;;
            --username)
                assert_flag_has_argument "${2:-}" "$1" 
                username="$2"
                ;;
            --password)
                assert_flag_has_argument "${2:-}" "$1" 
                password="$2"
                ;;
            --download-path)
                assert_flag_has_argument "${2:-}" "$1" 
                download_path="$2"
                ;;
            *)
                err "$(printf "$ERR_CODE_2" "$1")" 2
                ;;
        esac
        shift 2
    done

    if [[ -z "$url" || -z "$deb_file" || -z "$username" || -z "$password" || -z "$download_path" ]]; then
        err "$ERR_CODE_3" 3
    fi

    if [[ "$url" == */ ]]; then
        url="${url::-1}"
    fi

    local repository=""
    local repository_path=""
    local url_schema="${url%%://*}"
    local url_info="${url#*://}"

    url_info="${url_schema}://${url_info%%/*}"

    if [[ "$url" == *"artifactory"* ]]; then
        repository_path="${url#*/artifactory/}"
        url_info="${url_info}/artifactory/api/storage/${repository_path}/${deb_file}"
    elif [[ "$url" == *"nexus"* ]]; then
        repository="${url#*/repository/}"
        repository="${repository%%/*}"
        repository_path="${url#*/repository/"${repository}"/}"
        url_info="${url_info}/service/rest/v1/search?repository=${repository}&name=/${repository_path}/${deb_file}"
    else
        err "$ERR_CODE_4" 4
    fi

    local url_deb_file="${url}/${deb_file}"

    if ! error_msg=$(Scripts/ViciOne/Install/is_repository_reachable.sh "$url_deb_file" "$username" "$password"); then
        err "$error_msg" $?
    fi

    local deb_file_path="${download_path}/${deb_file}"

    if ! wget --quiet --timeout=5 --tries=3 -nv -O "$deb_file_path" --user="$username" --password="$password" "$url_deb_file"; then
        rm -f deb_file_path
        err "$ERR_CODE_5" 5
    fi

    authorization=$(Scripts/ViciOne/Install/get_authorization_data.sh "$username" "$password")
    content=$(wget --header="$authorization" -qO- "$url_info" 2>&1)
    sha256_json=$(grep "sha256" <<< "$content" | head -n 1 | awk -F': ' '{print $2}' | tr -d '",' || true)
    sha256_calculated=$(sha256sum -b "$deb_file_path" | cut -d ' ' -f 1 || true)

    if [[ -z "$sha256_json" ]]; then
        err "$ERR_CODE_6" 6
    fi

    if [[ "$sha256_json" != "$sha256_calculated" ]]; then
        rm -f "$deb_file_path"
        err "$ERR_CODE_7" 7
    fi
}

main "$@"
#eof
