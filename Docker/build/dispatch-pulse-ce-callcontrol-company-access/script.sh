#!/usr/bin/env bash
set -o errexit
set -o nounset
#set -o xtrace
set -o pipefail

systemctl start asterisk

while:
do
	sleep 1000
done