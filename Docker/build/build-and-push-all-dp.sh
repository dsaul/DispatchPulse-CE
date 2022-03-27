#!/bin/bash

bash ./dispatch-pulse-ce-api/buildandpush.sh
bash ./dispatch-pulse-ce-ari-oncall/buildandpush.sh
bash ./dispatch-pulse-ce-ari-projects-scheduling-time/buildandpush.sh
bash ./dispatch-pulse-ce-callcontrol-company-access/buildandpush.sh
bash ./dispatch-pulse-ce-database-bootstrap/buildandpush.sh
bash ./dispatch-pulse-ce-job-runner-assignments-report/buildandpush.sh
bash ./dispatch-pulse-ce-job-runner-companies-report/buildandpush.sh
bash ./dispatch-pulse-ce-job-runner-contacts-report/buildandpush.sh
bash ./dispatch-pulse-ce-job-runner-database-verification/buildandpush.sh
bash ./dispatch-pulse-ce-job-runner-ensure-company-s3-buckets/buildandpush.sh
bash ./dispatch-pulse-ce-job-runner-labour-report/buildandpush.sh
bash ./dispatch-pulse-ce-job-runner-materials-report/buildandpush.sh
bash ./dispatch-pulse-ce-job-runner-on-call-responder-30-day-report/buildandpush.sh
bash ./dispatch-pulse-ce-job-runner-pdflatex/buildandpush.sh
bash ./dispatch-pulse-ce-job-runner-projects-report/buildandpush.sh
bash ./dispatch-pulse-ce-job-runner-recurring-task-scheduler/buildandpush.sh
bash ./dispatch-pulse-ce-job-runner-remove-expired-jobs/buildandpush.sh
bash ./dispatch-pulse-ce-job-runner-update-web-cal-files/buildandpush.sh
bash ./dispatch-pulse-ce-minio-bootstrap/buildandpush.sh
bash ./dispatch-pulse-ce-on-call-responder-message-access/buildandpush.sh
bash ./dispatch-pulse-ce-square-payments/buildandpush.sh
bash ./dispatch-pulse-ce-twilio-disaster-recovery/buildandpush.sh
bash ./dispatch-pulse-ce-webapp/buildandpush.sh