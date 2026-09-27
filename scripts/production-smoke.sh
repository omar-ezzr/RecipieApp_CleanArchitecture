#!/usr/bin/env sh
set -eu

base_url="${1:-http://localhost:8080}"
base_url="${base_url%/}"

curl --fail --silent --show-error "$base_url/" >/dev/null
curl --fail --silent --show-error "$base_url/health/live" >/dev/null
curl --fail --silent --show-error "$base_url/health/ready" >/dev/null
curl --fail --silent --show-error "$base_url/api/categories" >/dev/null

printf '%s
' 'Production smoke checks passed.'
