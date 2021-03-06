#!/usr/bin/env bash
set -o errexit
set -o nounset
#set -o xtrace
set -o pipefail

envsubst '$DOCKERHOSTIP' < /etc/asterisk/pjsip.conf.template > /etc/asterisk/pjsip.conf
envsubst '$ARI_HOST,$ARI_PORT,$ARI_ENDPOINT' < /etc/asterisk/extensions.ael.template > /etc/asterisk/extensions.ael

/script.sh > /proc/1/fd/1
