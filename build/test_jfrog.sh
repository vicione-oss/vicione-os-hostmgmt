#!/usr/bin/env bash
#
# test_jfrog.sh - Test JFrog Artifactory operations
#
# Tests JFrog CLI: connection, upload, download, and delete.
# Requires: Raw_User_RW_JFrog, Raw_PW_RW_JFrog CI variables
#

set -euo pipefail

# Use provided vars or fall back to standard CI variables
JFROG_SERVER="${JFROG_SERVER:-https://ifm.jfrog.io/artifactory}"
JFROG_USER="${JFROG_USER:-${Raw_User_RW_JFrog:-}}"
JFROG_PASSWORD="${JFROG_PASSWORD:-${Raw_PW_RW_JFrog:-}}"
TEST_REPO="${TEST_REPO:-vicione-suite-dev}"
TEST_FILE="test-pipeline-${CI_PIPELINE_ID:-local}.txt"
TEST_PATH="${TEST_REPO}/ci-test/${TEST_FILE}"

echo "[INF] Testing JFrog Operations"
echo "[INF] Server: ${JFROG_SERVER}"
echo "[INF] Repository: ${TEST_REPO}"
echo "[INF] Test file: ${TEST_FILE}"

# Check if credentials are available
if [ -z "$JFROG_USER" ] || [ -z "$JFROG_PASSWORD" ]; then
  echo "[ERR] JFrog credentials not configured"
  echo "[ERR] Set Raw_User_RW_JFrog and Raw_PW_RW_JFrog CI variables"
  exit 1
fi

# Configure JFrog CLI
echo "[INF] Configuring JFrog CLI..."
jf config add --user "$JFROG_USER" --password "$JFROG_PASSWORD" --artifactory-url "$JFROG_SERVER" jfrog-test
jf config use jfrog-test

# Test 1: Connection
echo "[INF] Test 1: Connection (ping)..."
if jf rt ping; then
  echo "[INF] Connection successful"
else
  echo "[ERR] Connection failed"
  exit 1
fi

# Test 2: Upload
echo "[INF] Test 2: Upload..."
echo "Test content from pipeline ${CI_PIPELINE_ID:-local} at $(date -u +%Y-%m-%dT%H:%M:%SZ)" > "${TEST_FILE}"
if jf rt upload "${TEST_FILE}" "${TEST_PATH}"; then
  echo "[INF] Upload successful"
else
  echo "[ERR] Upload failed"
  exit 1
fi

# Test 3: Download
echo "[INF] Test 3: Download..."
rm -f "${TEST_FILE}"
if jf rt download "${TEST_PATH}" --flat; then
  echo "[INF] Download successful"
  echo "[INF] Content: $(cat "${TEST_FILE}")"
else
  echo "[ERR] Download failed"
  exit 1
fi

# Test 4: Delete
echo "[INF] Test 4: Delete..."
if jf rt delete "${TEST_PATH}" --quiet; then
  echo "[INF] Delete successful"
else
  echo "[ERR] Delete failed"
  exit 1
fi

# Cleanup
rm -f "${TEST_FILE}"
echo "[INF] Cleaning up test config..."
jf config remove jfrog-test --quiet || true

echo "[INF] All JFrog operations tested successfully!"
