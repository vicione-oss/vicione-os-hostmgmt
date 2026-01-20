#!/usr/bin/env bash

#######################################
# JFrog Artifactory release operations
#######################################

SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "${SCRIPT_DIR}/package_basic.sh"

#######################################
# Get version string for artifacts
# Sets: APP_PIPELINE, APP_VERSION, APP_TAG, VERSION
#######################################
jfrog::set_version() {
  export APP_PIPELINE="${CI_PIPELINE_ID}"
  export APP_VERSION="$(cat VERSION)"
  export APP_TAG="${CI_COMMIT_TAG}"
  export VERSION="${APP_VERSION}"

  if [[ -z "${APP_TAG}" ]]; then
    VERSION="${VERSION}~${APP_PIPELINE}"
  fi

  basic::log "Version: ${VERSION} (Pipeline: ${APP_PIPELINE})"
}

#######################################
# Create artifacts list file
# Arguments:
#   $1 - Artifact prefix (e.g., "hostmanagement")
# Output:
#   Creates artifacts.list file
#######################################
jfrog::create_artifacts_list() {
  local PREFIX="$1"

  echo "${PREFIX}_${VERSION}_amd64.deb" > artifacts.list
  echo "${PREFIX}_${VERSION}_arm64.deb" >> artifacts.list

  basic::log "Created artifacts.list:"
  cat artifacts.list
}

#######################################
# Download .deb from Package Registry and upload to JFrog
# Arguments:
#   $1 - JFrog repository destination
#   $2 - JFrog upload path (e.g., "hostmanagements")
# Environment:
#   CI_JOB_TOKEN, CI_API_V4_URL, CI_PROJECT_ID
#   PACKAGE_NAME, APP_PIPELINE
#######################################
jfrog::release_packages() {
  local REPOSITORY="$1"
  local UPLOAD_PATH="${2:-hostmanagements}"

  basic::log "Releasing packages to JFrog: ${REPOSITORY}"

  while IFS= read -r artifact; do
    basic::log "Processing: ${artifact}"

    if [[ "${artifact}" == *.deb ]]; then
      # Download from Package Registry
      basic::log "Downloading ${artifact} from Package Registry"
      curl --fail --silent --show-error \
        --header "JOB-TOKEN: ${CI_JOB_TOKEN}" \
        --output "${artifact}" \
        "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/packages/generic/${PACKAGE_NAME}/${APP_PIPELINE}/${artifact}"

      if [[ $? -eq 0 ]]; then
        # Upload to JFrog
        basic::log "Uploading ${artifact} to JFrog"
        jf rt upload "${artifact}" "${REPOSITORY}/${UPLOAD_PATH}/${artifact}"
      else
        basic::err "Failed to download ${artifact}"
        return 1
      fi
    fi
  done < artifacts.list

  basic::log "Release complete"
}

#######################################
# Promote/move packages between JFrog repositories
# Arguments:
#   $1 - Source repository
#   $2 - Destination repository
#######################################
jfrog::promote_packages() {
  local SOURCE_REPO="$1"
  local DEST_REPO="$2"

  basic::log "Promoting packages: ${SOURCE_REPO} -> ${DEST_REPO}"

  while IFS= read -r artifact; do
    if [[ "${artifact}" == *.deb ]]; then
      local source="${SOURCE_REPO}/*/${artifact}"
      local destination="${DEST_REPO}/${artifact}"

      basic::log "Moving: ${artifact}"
      jf rt mv "${source}" "${destination}"
    fi
  done < artifacts.list

  basic::log "Promotion complete"
}

#######################################
# Cleanup Package Registry after JFrog upload
# Environment:
#   CI_JOB_TOKEN, CI_API_V4_URL, CI_PROJECT_ID
#   PACKAGE_NAME, APP_PIPELINE
#######################################
jfrog::cleanup_registry() {
  basic::log "Cleaning up Package Registry version: ${APP_PIPELINE}"

  local PACKAGE_ID
  PACKAGE_ID=$(curl --silent --header "JOB-TOKEN: ${CI_JOB_TOKEN}" \
    "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/packages?package_name=${PACKAGE_NAME}&per_page=100" \
    | jq -r ".[] | select(.version == \"${APP_PIPELINE}\") | .id")

  if [ -n "$PACKAGE_ID" ] && [ "$PACKAGE_ID" != "null" ]; then
    basic::log "Deleting package ID: ${PACKAGE_ID}"
    curl --silent --request DELETE --header "JOB-TOKEN: ${CI_JOB_TOKEN}" \
      "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/packages/${PACKAGE_ID}" || true
    basic::log "Cleanup complete"
  else
    basic::warn "No package found for version ${APP_PIPELINE}"
  fi
}
