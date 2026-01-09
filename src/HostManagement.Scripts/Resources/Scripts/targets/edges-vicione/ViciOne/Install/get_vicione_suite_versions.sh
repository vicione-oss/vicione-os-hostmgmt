#!/usr/bin/env bash

# ###
#
# Get ViciOne Suite Versions Script
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
readonly ERR_CODE_4="Failed to fetch versions list. wget exit code: %s"

#######################################
# Methods
#######################################

usage() {
    echo "Usage: ${0} --url <url> --username <username> --password <password>"
    echo
    echo "Options:"
    echo "  --url      - The URL of the ViciOne Suite repository."
    echo "  --username - The username for the ViciOne Suite repository."
    echo "  --password - The password for the ViciOne Suite repository."
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
    local username=""
    local password=""

    while (("$#")); do
        case $1 in
            --url)
                assert_flag_has_argument "${2:-}" "$1" 
                url="$2"
                ;;
            --username)
                assert_flag_has_argument "${2:-}" "$1" 
                username="$2"
                ;;
            --password)
                assert_flag_has_argument "${2:-}" "$1" 
                password="$2"
                ;;
            *)
                err "$(printf "$ERR_CODE_2" "$1")" 2
                ;;
        esac
        shift 2
    done

    if [[ -z "$url" || -z "$username" || -z "$password" ]]; then
        err "$ERR_CODE_3" 3
    fi

    if [[ "$url" == */ ]]; then
        url="${url::-1}"
    fi

    url="${url}/versions"
    
    if ! error_msg=$(Scripts/ViciOne/Install/is_repository_reachable.sh "$url" "$username" "$password"); then
        err "$error_msg" $?
    fi

    if ! content=$(wget --timeout=5 --tries=3 --user="$username" --password="$password" -qO- "$url"); then
        err "$(printf "$ERR_CODE_4" "$?")" 4
    fi

    echo -e "$content"
}

main "$@"
#eof
