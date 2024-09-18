#!/bin/bash
set -e

# Use the GRAFANA_HOST environment variable
GRAFANA_HOST="${GRAFANA_HOST:-localhost:3000}"

shift
cmd="$@"
check_url="$GRAFANA_HOST/api/health"

until curl -s "$check_url" | grep -q "ok"; do
  >&2 echo "Grafana $check_url is unavailable - sleeping"
  sleep 5
done

>&2 echo "Grafana $check_url is up - executing command"
exec $cmd