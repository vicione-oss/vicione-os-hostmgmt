#!/usr/bin/env bash

# ###
#
# Set Network DNS Settings Script
#
# The script returns 0 if ok, non zero for error.
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

readonly DNS_SUFFIX_MARKER=" # dns-suffix"

readonly FLAG_NAMESERVERS_ENABLED="--nameservers-enabled"
readonly FLAG_NAMESERVERS="--nameservers"
readonly FLAG_DNS_SUFFIX_ENABLED="--dns-suffix-enabled"
readonly FLAG_DNS_SUFFIX="--dns-suffix"
readonly FLAG_SEARCH_DOMAINS_ENABLED="--search-domains-enabled"
readonly FLAG_SEARCH_DOMAINS="--search-domains"
readonly FLAG_INTERFACES="--interfaces"

#######################################
# Methods
#######################################

usage() {
    echo "Usage:"
    echo "$0"
    echo "    [${FLAG_NAMESERVERS_ENABLED} <true|false>]"
    echo "    [${FLAG_NAMESERVERS} <ip> [<ip> ...]]"
    echo "    [${FLAG_DNS_SUFFIX_ENABLED} <true|false>]"
    echo "    [${FLAG_DNS_SUFFIX} <suffix>]"
    echo "    [${FLAG_SEARCH_DOMAINS_ENABLED} <true|false>]"
    echo "    [${FLAG_SEARCH_DOMAINS} <domain> [<domain> ...]]"
    echo "    [${FLAG_INTERFACES} <interfaces>]"
    echo
    echo "Options:"
    echo "  ${FLAG_NAMESERVERS_ENABLED}    - Enables or disables the DNS nameservers. (optional)"
    echo "  ${FLAG_NAMESERVERS}            - Specifies one or more DNS nameservers to configure."
    echo "  ${FLAG_DNS_SUFFIX_ENABLED}     - Enables or disables the DNS suffix. (optional)"
    echo "  ${FLAG_DNS_SUFFIX}             - Specifies a DNS suffix to configure."
    echo "  ${FLAG_SEARCH_DOMAINS_ENABLED} - Enables or disables the search domains. (optional)"
    echo "  ${FLAG_SEARCH_DOMAINS}         - Specifies one or more search domains to configure."
    echo "  ${FLAG_INTERFACES}             - The interfaces with format as returned from 'ip -br link show'."
    echo
    echo "Examples:"
    echo "$0"
    echo "  ${FLAG_NAMESERVERS_ENABLED} false ${FLAG_NAMESERVERS} 8.8.8.8 8.8.4.4"
    echo "  ${FLAG_DNS_SUFFIX_ENABLED} false ${FLAG_DNS_SUFFIX} ifm.com"
    echo "  ${FLAG_SEARCH_DOMAINS_ENABLED} false ${FLAG_SEARCH_DOMAINS} test.de google.com"
    echo "  ${FLAG_INTERFACES} \"ens33            UP             00:50:56:36:6d:33 <BROADCAST,MULTICAST,UP,LOWER_UP>\""
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

    local nameservers_enabled_marker="-"
    local nameservers=()
    local search_domains_enabled_marker="-"
    local search_domains=()
    local dns_suffix_enabled_marker="-"
    local dns_suffix=""
    local interfaces=""

    while [[ $# -gt 0 ]]; do
        flag_name="$1"
        shift

        case "$flag_name" in
            "$FLAG_NAMESERVERS_ENABLED" )
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                if [[ $1 == "false" ]]; then
                    nameservers_enabled_marker="#"
                fi
                shift
                ;;
            "$FLAG_NAMESERVERS")
                # TODO: Add argument 'none'
                #assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                while [[ $# -gt 0 && "$1" != --* ]]; do
                    nameservers+=("$1")
                    shift
                done
                ;;
            "$FLAG_SEARCH_DOMAINS_ENABLED" )
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                if [[ $1 == "false" ]]; then
                    search_domains_enabled_marker="#"
                fi
                shift
                ;;
            "$FLAG_SEARCH_DOMAINS")
                # TODO: Add argument 'none'
                #assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                while [[ $# -gt 0 && "$1" != --* ]]; do
                    search_domains+=("$1")
                    shift
                done
                ;;
            "$FLAG_DNS_SUFFIX_ENABLED" )
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                if [[ $1 == "false" ]]; then
                    dns_suffix_enabled_marker="#"
                fi
                shift
                ;;
            "$FLAG_DNS_SUFFIX" )
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                dns_suffix="$1"
                shift
                ;;
            "$FLAG_INTERFACES" )
                assert_flag_has_arguments "$#" "${1:-}" "$flag_name"
                interfaces="$1"
                shift
                ;;
            *)
                err "$(printf "$ERR_CODE_1" "$flag_name")" 1
                ;;
        esac
    done

    if [[ "${#nameservers[@]}" -gt 0 || "${#search_domains[@]}" -gt 0 || -n "$dns_suffix" ]]; then
        mapfile -t network_interfaces <<< "$interfaces"

        if [[ "${#nameservers[@]}" -eq 0 ]]; then
            nameservers_enabled_marker="#"
        fi

        if [[ "${#search_domains[@]}" -eq 0 ]]; then
            search_domains_enabled_marker="#"
        fi

        if [[ -z "$dns_suffix" ]]; then
            dns_suffix_enabled_marker="#"
        fi

        for current in "${network_interfaces[@]}"; do
            interface=$(awk '{print $1}' <<< "$current" | sed 's/ //;s/@.*//')
            configuration_path="${DIRECTORY_NETPLAN}/50-${interface}.yaml"

            if [[ -e "$configuration_path" ]]; then
                if [[ "$nameservers_enabled_marker" == "#" &&
                      "$dns_suffix_enabled_marker" == "#" &&
                      "$search_domains_enabled_marker" == "#" ]]; then
                    echo "      #nameservers:" >> "$configuration_path"
                else
                    echo "      nameservers:" >> "$configuration_path"
                fi
                
                if [[ "${#nameservers[@]}" -gt 0 ]]; then
                    if [[ "$nameservers_enabled_marker" == "#" ]]; then
                        echo "        #addresses:" >> "$configuration_path"
                    else
                        echo "        addresses:" >> "$configuration_path"
                    fi

                    for nameserver in "${nameservers[@]}"; do
                        echo "          ${nameservers_enabled_marker} ${nameserver}" >> "$configuration_path"
                    done
                fi

                if [[ -n "$dns_suffix" || "${#search_domains[@]}" -gt 0 ]]; then
                    if [[ "$dns_suffix_enabled_marker" == "#" && "$search_domains_enabled_marker" == "#" ]]; then
                        echo "        #search:" >> "$configuration_path"
                    else
                        echo "        search:" >> "$configuration_path"
                    fi

                    if [[ -n "$dns_suffix" ]]; then
                        echo "          ${dns_suffix_enabled_marker} ${dns_suffix}${DNS_SUFFIX_MARKER}" >> "$configuration_path"
                    fi

                    if [[ "${#search_domains[@]}" -gt 0 ]]; then
                        for search_domain in "${search_domains[@]}"; do
                            if [[ "$search_domain" != "$dns_suffix" ]]; then
                                echo "          ${search_domains_enabled_marker} ${search_domain}" >> "$configuration_path"
                            fi
                        done
                    fi
                fi
            fi
        done
    fi
}

main "$@"
#eof
