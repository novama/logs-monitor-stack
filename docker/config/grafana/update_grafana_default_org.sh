#!/bin/bash

# Use environment variables for the credentials
GRAFANA_HOST="${GRAFANA_HOST:-localhost:3000}"
GRAFANA_USER="${GF_SECURITY_ADMIN_USER}"
GRAFANA_PASSWORD="${GF_SECURITY_ADMIN_PASSWORD}"

# Send the PUT request with Basic Auth
curl -X PUT \
  $GRAFANA_HOST/api/orgs/1 \
  -u "$GRAFANA_USER:$GRAFANA_PASSWORD" \
  -H "Accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{"name":"Orthotech1"}'
