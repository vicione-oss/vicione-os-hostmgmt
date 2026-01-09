#!/usr/bin/env bash

# ###
#
# Set Network DNS Settings Script - v1.0.0
#
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Variables
#######################################

DIRECTORY="/etc/netplan"
DNS_SUFFIX_FILE="${DIRECTORY}/dns_suffix"

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
        echo "    --nameservers 8.8.8.8 8.8.4.4"
        echo "    --dns-suffix ifm.com"
        echo "    --search-domains test.de google.com"
        echo "    --hostname - The hostname for the system."

        exit 0
    fi

    if [[ "$1" -eq 0 ]]; then
        err "Please enter all required parameters." 1
    fi

    nameservers=()
    search_domains=()

    while (("$#")); do
        case "$1" in
            --nameservers)
                shift
                while [[ -n "$1" ]] && [[ "$1" != --* ]]; do
                    nameservers+=("$1")
                    shift
                done
                ;;
            --search-domains)
                shift
                while [[ -n "$1" ]] && [[ "$1" != --* ]]; do
                    search_domains+=("$1")
                    shift
                done
                ;;
            --dns-suffix)
                shift
                dns_suffix="$1"
                shift
                ;;
            --hostname)
                shift
                hostname="$1"
                shift
                ;;
            -*)
                err "Unsupported flag $1" 2
                ;;
        esac
    done

    has_nameservers=1
    has_search_domains=1

    if [ ${#nameservers[@]} -gt 0 ]; then
        has_nameservers=0
    fi

    if [ ${#search_domains[@]} -gt 0 ]; then
        has_search_domains=0
    fi

    echo "${dns_suffix}" > "${DNS_SUFFIX_FILE}"

    if [ "${has_nameservers}" -eq 0 ] || [ "${has_search_domains}" -eq 0 ] || [ -n "${dns_suffix}" ]; then
        network_interfaces=$(Scripts/Network/Helper/get_network_interfaces.sh)

        IFS=$'\n'
        for current in $network_interfaces; do
            iface=$(echo "${current}" | awk '{print $1}' | sed 's/ //;s/@.*//')
            configuration_path="${DIRECTORY}/50-${iface}.yaml"

            if [ -e "${configuration_path}" ]; then
                echo "      nameservers:" >> "${configuration_path}"
                
                if [ "${has_nameservers}" -eq 0 ]; then
                    echo "        addresses:" >> "${configuration_path}"

                    for nameserver in "${nameservers[@]}"; do
                        echo "          - ${nameserver}" >> "${configuration_path}"
                    done
                fi

                if [ -n "${dns_suffix}" ] || [ "${has_search_domains}" -eq 0 ]; then
                    echo "        search:" >> "${configuration_path}"

                    if [ -n "${dns_suffix}" ]; then
                        echo "          - ${dns_suffix}" >> "${configuration_path}"
                    fi

                    if [ "${has_search_domains}" -eq 0 ]; then
                        for search_domain in "${search_domains[@]}"; do
                            echo "          - ${search_domain}" >> "${configuration_path}"
                        done
                    fi
                fi
            fi
        done
    fi

    if [ -n "${hostname}" ]; then
        Scripts/Network/DNS/set_hostname.sh "${hostname}"
    fi
}

main "$@"
#eof
