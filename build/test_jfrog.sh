#!/usr/bin/env bash
#
# test_jfrog.sh - Test JFrog Artifactory connectivity
#
# Tests JFrog CLI configuration and basic operations.
# Requires: JFROG_SERVER, JFROG_USER, JFROG_PASSWORD (or Raw_User_RW_JFrog, Raw_PW_RW_JFrog)
#

set -euo pipefail

# Use provided vars or fall back to standard CI variables
JFROG_SERVER="${JFROG_SERVER:-https://ifm.jfrog.io/artifactory}"
JFROG_USER="${JFROG_USER:-${Raw_User_RW_JFrog:-}}"
JFROG_PASSWORD="${JFROG_PASSWORD:-${Raw_PW_RW_JFrog:-}}"
TEST_REPO="${TEST_REPO:-vicione-suite-dev}"

echo "[INF] Testing JFrog Connectivity"
echo "[INF] Server: ${JFROG_SERVER}"
echo "[INF] Repository: ${TEST_REPO}"

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

# Test connection with ping
echo "[INF] Testing connection..."
if jf rt ping; then
  echo "[INF] JFrog connection successful"
else
  echo "[ERR] JFrog connection failed"
  exit 1
fi

# Test repository access
echo "[INF] Testing repository access..."
if jf rt search "${TEST_REPO}/" --limit=1 > /dev/null 2>&1; then
  echo "[INF] Repository ${TEST_REPO} accessible"
else
  echo "[WRN] Could not search repository ${TEST_REPO} (may be empty or no read access)"
fi

# Cleanup config
echo "[INF] Cleaning up test config..."
jf config remove jfrog-test --quiet || true

echo "[INF] All JFrog connectivity tests passed!"
