#!/usr/bin/env bash
# Backup diário de produção: Postgres dump + volumes opcionais.
# Instalado via cron no server-bootstrap.sh (02:00 UTC).
set -euo pipefail

INSTALL_DIR="${INSTALL_DIR:-/opt/epros}"
BACKUP_DIR="${BACKUP_DIR:-/backups}"
RETENTION_DAYS="${RETENTION_DAYS:-7}"
COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.prod.yml}"
TIMESTAMP="$(date -u +%Y%m%dT%H%M%SZ)"

cd "${INSTALL_DIR}"

if [[ ! -f .env.production ]]; then
  echo "Erro: .env.production não encontrado."
  exit 1
fi

# shellcheck disable=SC1091
set -a
source .env.production
set +a

mkdir -p "${BACKUP_DIR}"

PG_DUMP="${BACKUP_DIR}/postgres-${TIMESTAMP}.sql.gz"
echo "==> Dump PostgreSQL -> ${PG_DUMP}"
docker compose -f "${COMPOSE_FILE}" exec -T postgres \
  pg_dump -U "${POSTGRES_USER:-epros}" -d "${POSTGRES_DB:-epros}" --no-owner --no-acl \
  | gzip > "${PG_DUMP}"

MINIO_ARCHIVE="${BACKUP_DIR}/minio-data-${TIMESTAMP}.tar.gz"
if docker volume inspect epros-prod_minio_data >/dev/null 2>&1 || \
   docker volume inspect epros_minio_data >/dev/null 2>&1; then
  VOLUME_NAME="$(docker volume ls -q | grep -E 'minio_data$' | head -1 || true)"
  if [[ -n "${VOLUME_NAME}" ]]; then
    echo "==> Snapshot MinIO volume ${VOLUME_NAME} -> ${MINIO_ARCHIVE}"
    docker run --rm \
      -v "${VOLUME_NAME}:/data:ro" \
      -v "${BACKUP_DIR}:/backup" \
      alpine:3.20 \
      tar czf "/backup/minio-data-${TIMESTAMP}.tar.gz" -C /data .
  fi
fi

DFE_ARCHIVE="${BACKUP_DIR}/dfe-storage-${TIMESTAMP}.tar.gz"
VOLUME_DFE="$(docker volume ls -q | grep -E 'dfe_storage$' | head -1 || true)"
if [[ -n "${VOLUME_DFE}" ]]; then
  echo "==> Snapshot DFe volume ${VOLUME_DFE} -> ${DFE_ARCHIVE}"
  docker run --rm \
    -v "${VOLUME_DFE}:/data:ro" \
    -v "${BACKUP_DIR}:/backup" \
    alpine:3.20 \
    tar czf "/backup/dfe-storage-${TIMESTAMP}.tar.gz" -C /data .
fi

echo "==> Removendo backups com mais de ${RETENTION_DAYS} dias..."
find "${BACKUP_DIR}" -type f \( -name 'postgres-*.sql.gz' -o -name 'minio-data-*.tar.gz' -o -name 'dfe-storage-*.tar.gz' \) \
  -mtime "+${RETENTION_DAYS}" -delete

echo "==> Backup concluído."
