#!/usr/bin/env bash
set -o errexit
set -o nounset
#set -o xtrace
set -o pipefail

echo "NGINX reverse proxy entry point..."

echo "removing pid file"

rm /var/run/nginx.pid || true

echo "Passing control to Nginx..."

exec nginx -g "daemon off;"