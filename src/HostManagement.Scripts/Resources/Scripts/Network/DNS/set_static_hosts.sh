#!/usr/bin/env bash

# ###
#
# Set Static Hosts Script - v1.0.0
#
# The script sets the given static hosts to the /etc/hosts file.
# The script returns 0 if ok or warning, non zero for error.
# ###

#######################################
# Variables
#######################################

SECTION_NAME="HostManagement"
START_MARKER="# BEGIN ${SECTION_NAME}"
END_MARKER="# END ${SECTION_NAME}"
HOSTS_FILE="/etc/hosts"
HOSTS_FILE_CONTENT=""

#######################################
# Methods
#######################################

err() {
    echo -e "$1" >&2
    exit "$2"
}

load_current_hosts_file() {
    HOSTS_FILE_CONTENT=$(< ${HOSTS_FILE})
    HOSTS_FILE_CONTENT=$(echo "${HOSTS_FILE_CONTENT}" | awk "/${START_MARKER}/,/${END_MARKER}/ {next} {print}")

    HOSTS_FILE_CONTENT+=$'\n'
}

write_new_hosts_file() {
    echo "${HOSTS_FILE_CONTENT}" > ${HOSTS_FILE}
}

#######################################
# Argument handling
#######################################

main() {
    if [[ "$1" == "--help" ]] || [[ "$1" == "-h" ]]; then
        echo "The order of the parameters must be observed!"
        echo "Examples:"
        echo
        echo "$0"
        echo "    --ip-address 192.168.1.1 --hostname myhost1.local"
        echo "    --ip-address 192.168.1.2 --hostname myhost2.local"

        exit 0
    fi

    if [[ ! -f ${HOSTS_FILE} ]]; then
        err "The file '${HOSTS_FILE}' does not exists." 1
    fi

    load_current_hosts_file

    HOSTS_FILE_CONTENT+="${START_MARKER}"$'\n'

    while (("$#")); do
        case "$1" in
            --ip-address)
                shift
                ip_address="$1"
                shift
                ;;
            --hostname)
                shift
                hostname="$1"
                HOSTS_FILE_CONTENT+="${ip_address} ${hostname}"$'\n'
                shift
                ;;
            -*)
                err "Unsupported flag ${1}" 2
                ;;
        esac
    done

    HOSTS_FILE_CONTENT+="${END_MARKER}"$'\n'

    write_new_hosts_file
}

main "$@"
#eof
