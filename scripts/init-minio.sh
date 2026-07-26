#!/usr/bin/env bash
set -euo pipefail

MINIO_ENDPOINT="${MINIO_ENDPOINT:-http://minio:9000}"
MINIO_ROOT_USER="${MINIO_ROOT_USER:-epros_minio}"
MINIO_ROOT_PASSWORD="${MINIO_ROOT_PASSWORD:-epros_minio_password}"
BUCKET_NAME="${MINIO_BUCKET:-epros-fiscal}"

echo "==> Aguardando MinIO em ${MINIO_ENDPOINT}..."
for _ in $(seq 1 40); do
  if mc alias set epros "${MINIO_ENDPOINT}" "${MINIO_ROOT_USER}" "${MINIO_ROOT_PASSWORD}" 2>/dev/null \
    && mc ready epros 2>/dev/null; then
    break
  fi
  echo "    MinIO ainda indisponível, tentando novamente em 3s..."
  sleep 3
done

mc alias set epros "${MINIO_ENDPOINT}" "${MINIO_ROOT_USER}" "${MINIO_ROOT_PASSWORD}"

if mc ls "epros/${BUCKET_NAME}" > /dev/null 2>&1; then
  echo "==> Bucket '${BUCKET_NAME}' já existe."
else
  echo "==> Criando bucket '${BUCKET_NAME}'..."
  mc mb "epros/${BUCKET_NAME}"
fi

echo "==> MinIO inicializado."
