#!/usr/bin/env sh
set -eu

base_url="${1:-http://localhost:8080}"

curl --fail --silent --show-error "$base_url/" >/dev/null
curl --fail --silent --show-error "$base_url/health/live" >/dev/null
curl --fail --silent --show-error "$base_url/health/ready" >/dev/null

printf '%s\n' 'Production smoke checks passed.'
