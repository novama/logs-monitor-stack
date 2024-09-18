#!/bin/sh

# Replace environment variables in the template and create the final config file
sed -i 's|${MINIO_ROOT_USER}|'"$MINIO_ROOT_USER"'|g' /etc/loki/config-template.yml
sed -i 's|${MINIO_ROOT_PASSWORD}|'"$MINIO_ROOT_PASSWORD"'|g' /etc/loki/config-template.yml

# Move the processed template to the final config file
cp /etc/loki/config-template.yml /etc/loki/config.yml
