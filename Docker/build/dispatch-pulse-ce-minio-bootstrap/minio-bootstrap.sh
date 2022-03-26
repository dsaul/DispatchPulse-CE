#!/usr/bin/env bash
set -o errexit
set -o nounset
#set -o xtrace
set -o pipefail

# Change Directory to script directory.
cd $(dirname "${BASH_SOURCE[0]}")

sleep 3s

mc alias set minio $MINIO_HOST $MINIO_ROOT_USER $MINIO_ROOT_PASSWORD 
mc mb minio/pdflatex-compiled-pdfs || true
mc mb minio/card-on-file-authorization-forms || true
mc mb minio/tts-cache || true
