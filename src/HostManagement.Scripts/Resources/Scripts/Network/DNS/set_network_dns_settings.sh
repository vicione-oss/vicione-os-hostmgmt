#!/usr/bin/env bash

# ###
#
# Set Network DNS Settings Script - v1.0.0
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

readonly ERR_CODE_1="Unsupported flag '%s'."
readonly ERR_CODE_2="Missing arguments for '%s'."

#######################################
# Constants
#######################################

readonly DIRECTORY_NETPLAN="/etc/netplan"
readonly DNS_SUFFIX_FILE="${DIRECTORY_NETPLAN}/dns_suffix"

readonly FLAG_NAMESERVERS="--nameservers"
readonly FLAG_DNS_SUFFIX="--dns-suffix"
readonly FLAG_SEARCH_DOMAINS="--search-domains"
readonly FLAG_HOSTNAME="--hostname"

#######################################
# Methods
#######################################

usage() {
    echo "Usage:"
    echo "$0"
    echo "    [${FLAG_NAMESERVERS} <ip> [<ip> ...]]"
    echo "    [${FLAG_DNS_SUFFIX} <suffix>]"
    echo "    [${FLAG_SEARCH_DOMAINS} <domain> [<domain> ...]]"
    echo "    [${FLAG_HOSTNAME} <hostname>]"
    echo
    echo "Options:"
    echo "  ${FLAG_NAMESERVERS}    - Specifies one or more DNS nameservers to configure."
    echo "  ${FLAG_DNS_SUFFIX}     - Specifies a DNS suffix to configure."
    echo "  ${FLAG_SEARCH_DOMAINS} - Specifies one or more search domains to configure."
    echo "  ${FLAG_HOSTNAME}       - Specifies the hostname."
    echo
    echo "Examples:"
    echo "$0"
    echo "  ${FLAG_NAMESERVERS} 8.8.8.8 8.8.4.4"
    echo "  ${FLAG_DNS_SUFFIX} ifm.com"
    echo "  ${FLAG_SEARCH_DOMAINS} test.de google.com"
    echo "  ${FLAG_HOSTNAME} MyHost"
    exit 0
}

assert_flag_has_arguments() {
    local argument_count="$1"
    local first_argument="$2"
    local flag_name="$3"

    if [[ "$argument_count" -eq 0 || "$first_argument" == --* ]]; then
        err "$(printf "$ERR_CODE_2" "$flag_name")" 2
    fi
}

#######################################
# Argument handling
#######################################

main() {
    if [[ $# -eq 0 || "$1" == "--help" || "$1" == "-h" ]]; then
        usage
    fi

    local nameservers=()
    local search_domains=()
    local dns_suffix=""
    local hostname=""

    while [[ $# -gt 0 ]]; do
        flag_name="$1"
        shift

        case "$flag_name" in
            "$FLAG_NAMESERVERS")
                # TODO: Add argument 'none'
                #assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                while [[ $# -gt 0 && "$1" != --* ]]; do
                    nameservers+=("$1")
                    shift
                done
                ;;
            "$FLAG_SEARCH_DOMAINS")
                # TODO: Add argument 'none'
                #assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                while [[ $# -gt 0 && "$1" != --* ]]; do
                    search_domains+=("$1")
                    shift
                done
                ;;
            "$FLAG_DNS_SUFFIX" )
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                dns_suffix="$1"
                shift
                ;;
            "$FLAG_HOSTNAME")
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                hostname="$1"
                shift
                ;;
            *)
                err "$(printf "$ERR_CODE_1" "$flag_name")" 1
                ;;
        esac
    done

    echo "$dns_suffix" > "${DNS_SUFFIX_FILE}"

    if [[ "${#nameservers[@]}" -gt 0 || "${#search_domains[@]}" -gt 0 || -n "$dns_suffix" ]]; then
        mapfile -t network_interfaces < <(Scripts/Network/Helper/get_network_interfaces.sh)

        for current in "${network_interfaces[@]}"; do
            interface=$(awk '{print $1}' <<< "$current" | sed 's/ //;s/@.*//')
            configuration_path="${DIRECTORY_NETPLAN}/50-${interface}.yaml"

            if [[ -e "$configuration_path" ]]; then
                echo "      nameservers:" >> "$configuration_path"
                
                if [[ "${#nameservers[@]}" -gt 0 ]]; then
                    echo "        addresses:" >> "$configuration_path"

                    for nameserver in "${nameservers[@]}"; do
                        echo "          - ${nameserver}" >> "$configuration_path"
                    done
                fi

                if [[ -n "$dns_suffix" || "${#search_domains[@]}" -gt 0 ]]; then
                    echo "        search:" >> "$configuration_path"

                    if [[ -n "$dns_suffix" ]]; then
                        echo "          - ${dns_suffix}" >> "$configuration_path"
                    fi

                    if [[ "${#search_domains[@]}" -gt 0 ]]; then
                        for search_domain in "${search_domains[@]}"; do
                            echo "          - ${search_domain}" >> "$configuration_path"
                        done
                    fi
                fi
            fi
        done
    fi

    if [[ -n "$hostname" ]]; then
        Scripts/Network/DNS/set_hostname.sh "$hostname"
    fi
}

main "$@"
#eof
