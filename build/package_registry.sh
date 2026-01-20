#!/usr/bin/env bash

#######################################
# GitLab Package Registry operations
#######################################

SCRIPT_DIR=$(dirname "$0")
source "${SCRIPT_DIR}/package_basic.sh"

#######################################
# Upload file to Package Registry
# Arguments:
#   $1 - Local file path
#   $2 - Remote filename
# Environment:
#   CI_JOB_TOKEN, CI_API_V4_URL, CI_PROJECT_ID
#   PACKAGE_NAME, PACKAGE_VERSION
#######################################
registry::upload() {
  local LOCAL_FILE="$1"
  local REMOTE_FILENAME="$2"

  basic::log "Uploading ${REMOTE_FILENAME} to Package Registry"

  local HTTP_CODE
  HTTP_CODE=$(curl --silent --write-out "%{http_code}" --output /dev/null \
    --header "JOB-TOKEN: ${CI_JOB_TOKEN}" \
    --upload-file "${LOCAL_FILE}" \
    "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/packages/generic/${PACKAGE_NAME}/${PACKAGE_VERSION}/${REMOTE_FILENAME}")

  if [ "$HTTP_CODE" = "201" ]; then
    basic::log "Upload successful (HTTP ${HTTP_CODE})"
    return 0
  else
    basic::err "Upload failed (HTTP ${HTTP_CODE})"
    return 1
  fi
}

#######################################
# Download file from Package Registry
# Arguments:
#   $1 - Remote filename
#   $2 - Local output path
# Environment:
#   CI_JOB_TOKEN, CI_API_V4_URL
#   DOWNLOAD_PROJECT_ID, PACKAGE_NAME, PACKAGE_VERSION
#######################################
registry::download() {
  local REMOTE_FILENAME="$1"
  local LOCAL_FILE="$2"

  basic::log "Downloading ${REMOTE_FILENAME} from Package Registry (project ${DOWNLOAD_PROJECT_ID})"

  local HTTP_CODE
  HTTP_CODE=$(curl --silent --write-out "%{http_code}" \
    --header "JOB-TOKEN: ${CI_JOB_TOKEN}" \
    --output "${LOCAL_FILE}" \
    "${CI_API_V4_URL}/projects/${DOWNLOAD_PROJECT_ID}/packages/generic/${PACKAGE_NAME}/${PACKAGE_VERSION}/${REMOTE_FILENAME}")

  if [ "$HTTP_CODE" = "200" ]; then
    local SIZE
    SIZE=$(ls -lh "${LOCAL_FILE}" | awk '{print $5}')
    basic::log "Download successful (HTTP ${HTTP_CODE}) - Size: ${SIZE}"
    return 0
  else
    basic::err "Download failed (HTTP ${HTTP_CODE})"
    return 1
  fi
}

#######################################
# Delete package version from Package Registry
# Arguments:
#   $1 - Package version to delete
# Environment:
#   CI_JOB_TOKEN, CI_API_V4_URL, CI_PROJECT_ID, PACKAGE_NAME
#######################################
registry::delete_version() {
  local VERSION="$1"

  basic::log "Finding package ID for version ${VERSION}"

  local PACKAGE_ID
  PACKAGE_ID=$(curl --silent --header "JOB-TOKEN: ${CI_JOB_TOKEN}" \
    "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/packages?package_name=${PACKAGE_NAME}&per_page=100" \
    | jq -r ".[] | select(.version == \"${VERSION}\") | .id")

  if [ -z "$PACKAGE_ID" ] || [ "$PACKAGE_ID" = "null" ]; then
    basic::warn "No package found for version ${VERSION}"
    return 0
  fi

  basic::log "Deleting package ID ${PACKAGE_ID}"

  local HTTP_CODE
  HTTP_CODE=$(curl --silent --write-out "%{http_code}" --output /dev/null \
    --request DELETE \
    --header "JOB-TOKEN: ${CI_JOB_TOKEN}" \
    "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/packages/${PACKAGE_ID}")

  if [ "$HTTP_CODE" = "204" ]; then
    basic::log "Delete successful (HTTP ${HTTP_CODE})"
    return 0
  else
    basic::err "Delete failed (HTTP ${HTTP_CODE})"
    return 1
  fi
}

#######################################
# Cleanup packages older than specified hours
# Arguments:
#   $1 - Hours threshold (default: 4)
# Environment:
#   CI_JOB_TOKEN, CI_API_V4_URL, CI_PROJECT_ID, PACKAGE_NAME
#######################################
registry::cleanup_old() {
  local HOURS="${1:-4}"

  basic::log "Cleaning up packages older than ${HOURS} hours"

  local CUTOFF_TIME
  CUTOFF_TIME=$(date -d "${HOURS} hours ago" +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || \
                date -v-${HOURS}H +%Y-%m-%dT%H:%M:%SZ)

  basic::log "Cutoff time: ${CUTOFF_TIME}"

  local PACKAGES
  PACKAGES=$(curl --silent --header "JOB-TOKEN: ${CI_JOB_TOKEN}" \
    "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/packages?package_name=${PACKAGE_NAME}&per_page=100")

  echo "$PACKAGES" | jq -r '.[] | "\(.id) \(.version) \(.created_at)"' | while read -r PKG_ID VERSION CREATED_AT; do
    if [[ "$CREATED_AT" < "$CUTOFF_TIME" ]]; then
      basic::log "Deleting old package: version ${VERSION} (created: ${CREATED_AT})"
      curl --silent --request DELETE --header "JOB-TOKEN: ${CI_JOB_TOKEN}" \
        "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/packages/${PKG_ID}" || true
    fi
  done

  basic::log "Cleanup complete"
}
