#!/usr/bin/env bash

# ###
#
# Install Debian package script
#
# The script returns 0 if ok, non zero for error.
# ###

# shellcheck disable=SC2059

set -eEuo pipefail

err() {
    echo "$1" >&2
    exit "$2"
}

trap 'err "Unknown error in line $LINENO: Command: $BASH_COMMAND (Exit-Code: $?)." 99' ERR

#######################################
# Error codes
#######################################

readonly ERR_CODE_1="Please run as root."
readonly ERR_CODE_2="Missing argument for '%s'."
readonly ERR_CODE_3="Unsupported flag '%s'."
readonly ERR_CODE_4="Please pass a valid package file path."
readonly ERR_CODE_5="Could not install .deb-package. apt exit code: %s"

#######################################
# Methods
#######################################

usage() {
    echo "Usage: ${0} --deb-package <deb-package-path>"
    echo
    echo "Options:"
    echo "  --deb-package     - The path of the Debian package to be installed."
    exit 0
}

assert_flag_has_argument() {
    local flag_argument="$1"
    local flag_name="$2"

    if [[ -z "$flag_argument" || "$flag_argument" == --* ]]; then
        err "$(printf "$ERR_CODE_2" "$flag_name")" 2
    fi
}

install_deb_package() {
    apt-get install -y -qq --reinstall --allow-downgrades "$1" > /dev/null

    status_code=$?

    if [[ $status_code -ne 0 ]]; then
        err "$(printf "$ERR_CODE_5" "$status_code")" 5
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ "$EUID" -ne 0 ]]; then
        err "$(printf "$ERR_CODE_1" "$1")" 1
    fi

    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    local deb_package=""

    export DEBIAN_FRONTEND="noninteractive"

    while (("$#")); do
        case $1 in
            --deb-package)
                assert_flag_has_argument "${2:-}" "$1"
                deb_package="$2"
                shift
                ;;
            *)
                err "$(printf "$ERR_CODE_3" "$1")" 3
                ;;
        esac
        shift
    done

    # Validate parameters
    if [[ ! -f "$deb_package" ]]; then
        err "$ERR_CODE_4" 4
    fi

    # Install deb package
    install_deb_package "$deb_package"
}

main "$@"
#eof
