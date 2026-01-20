#!/usr/bin/env bash

#######################################
# Basic logging functions
# Follows suite repository patterns
#######################################

basic::return() {
  echo -e "\e[91m[ERR] [$(date +'%Y-%m-%dT%H:%M:%S%z')]: $*\e[39m" >&2
  return 1
}

basic::err() {
  echo -e "\e[91m[ERR] [$(date +'%Y-%m-%dT%H:%M:%S%z')]: $*\e[39m" >&2
}

basic::log() {
  echo -e "\e[92m[INF] [$(date +'%Y-%m-%dT%H:%M:%S%z')]: $*\e[39m" >&1
}

basic::warn() {
  echo -e "\e[93m[WRN] [$(date +'%Y-%m-%dT%H:%M:%S%z')]: $*\e[39m" >&1
}
