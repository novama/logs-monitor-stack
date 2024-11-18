#!/bin/sh

# Create a new file with replaced environment variables
sed 's|${MINIO_ROOT_USER}|'"$MINIO_ROOT_USER"'|g; s|${MINIO_ROOT_PASSWORD}|'"$MINIO_ROOT_PASSWORD"'|g' /etc/loki/config-template.yml > /etc/loki/config.yml

