#!/usr/bin/env bash
#
# test_registry.sh - Test GitLab Package Registry operations
#
# Tests upload, download, and delete operations against the Package Registry.
# Requires: CI_JOB_TOKEN, CI_API_V4_URL, CI_PROJECT_ID, CI_PIPELINE_ID
#

set -euo pipefail

TEST_FILE="test-${CI_PIPELINE_ID}.txt"
TEST_PACKAGE="component-test"

echo "[INF] Testing Registry Components"
echo "[INF] Project: ${CI_PROJECT_ID} | Pipeline: ${CI_PIPELINE_ID}"

# Create test file
echo "Test content from pipeline ${CI_PIPELINE_ID}" > "${TEST_FILE}"

# Test Upload
echo "[INF] Testing upload..."
HTTP_CODE=$(curl --silent --write-out "%{http_code}" --output /dev/null \
  --header "JOB-TOKEN: ${CI_JOB_TOKEN}" \
  --upload-file "${TEST_FILE}" \
  "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/packages/generic/${TEST_PACKAGE}/${CI_PIPELINE_ID}/${TEST_FILE}")

if [ "$HTTP_CODE" = "201" ]; then
  echo "[INF] Upload successful (HTTP ${HTTP_CODE})"
else
  echo "[ERR] Upload failed (HTTP ${HTTP_CODE})"
  exit 1
fi

# Test Download
echo "[INF] Testing download..."
HTTP_CODE=$(curl --silent --write-out "%{http_code}" \
  --header "JOB-TOKEN: ${CI_JOB_TOKEN}" \
  --output "downloaded-${TEST_FILE}" \
  "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/packages/generic/${TEST_PACKAGE}/${CI_PIPELINE_ID}/${TEST_FILE}")

if [ "$HTTP_CODE" = "200" ]; then
  echo "[INF] Download successful (HTTP ${HTTP_CODE})"
  echo "[INF] Downloaded content:"
  cat "downloaded-${TEST_FILE}"
else
  echo "[ERR] Download failed (HTTP ${HTTP_CODE})"
  exit 1
fi

# Test Cleanup (delete the test package)
echo "[INF] Testing cleanup (delete)..."
PACKAGE_ID=$(curl --silent \
  --header "JOB-TOKEN: ${CI_JOB_TOKEN}" \
  "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/packages?package_name=${TEST_PACKAGE}" \
  | sed -n 's/.*"id":\([0-9]*\).*/\1/p' | head -1)

if [ -n "$PACKAGE_ID" ]; then
  HTTP_CODE=$(curl --silent --write-out "%{http_code}" --output /dev/null \
    --request DELETE \
    --header "JOB-TOKEN: ${CI_JOB_TOKEN}" \
    "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/packages/${PACKAGE_ID}")
  if [ "$HTTP_CODE" = "204" ] || [ "$HTTP_CODE" = "200" ]; then
    echo "[INF] Cleanup successful (HTTP ${HTTP_CODE})"
  else
    echo "[WRN] Cleanup returned HTTP ${HTTP_CODE}"
  fi
else
  echo "[WRN] Could not find package ID for cleanup"
fi

echo "[INF] All registry operations tested successfully!"
