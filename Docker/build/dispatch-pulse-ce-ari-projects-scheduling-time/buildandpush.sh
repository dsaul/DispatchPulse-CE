 #!/usr/bin/env bash
set -o errexit
set -o nounset
#set -o xtrace
set -o pipefail

IMAGENAME="dispatch-pulse-ce-ari-projects-scheduling-time"
TAGNAME="$(date +'%Y-%m-%d.%H-%M-%S.%N')" 
TAGNAME2="$(date +'%Y-%m-%d')" 
docker image build --no-cache \
	-t maskawanian/$IMAGENAME:$TAGNAME \
	-t maskawanian/$IMAGENAME:$TAGNAME2 \
	-t maskawanian/$IMAGENAME:latest \
	.
docker image push maskawanian/$IMAGENAME:$TAGNAME
docker image push maskawanian/$IMAGENAME:$TAGNAME2
docker image push maskawanian/$IMAGENAME:latest
