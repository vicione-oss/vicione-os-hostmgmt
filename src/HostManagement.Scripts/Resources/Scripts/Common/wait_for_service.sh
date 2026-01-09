#!/usr/bin/env bash

# ###
#
# Wait For Service Script - v1.0.0
#
# The script waits until the specified service has been started.
# The check is performed a maximum of 2 times.
# ###

service_name=$1
counter=0

while [ $counter -lt 2 ]; do
    status=$(systemctl show -p SubState "${service_name}" --value)

    if [[ ${status} == "running" ]]; then
        break;
    fi

    counter=$((counter + 1))
    sleep 1
done

if [[ ${counter} -eq 2 ]]; then
    echo -e "The service '$service_name' is not in 'running' state." >&2
    exit "1"
fi
#eof
