#!/usr/bin/env bash
# Deploy de produção na VPS (pull GHCR + migrate + up).
# Uso (como usuário deploy):
#   IMAGE_TAG=<sha> REGISTRY=ghcr.io/<owner> ./scripts/deploy-prod.sh
#   ./scripts/deploy-prod.sh --build   # bootstrap: build local em vez de pull
set -euo pipefail

INSTALL_DIR="${INSTALL_DIR:-/opt/epros}"
COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.prod.yml}"
HEALTH_RETRIES="${HEALTH_RETRIES:-30}"
HEALTH_INTERVAL="${HEALTH_INTERVAL:-5}"
BUILD_LOCAL=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --build)
      BUILD_LOCAL=true
      shift
      ;;
    *)
      echo "Uso: $0 [--build]"
      exit 1
      ;;
  esac
done

cd "${INSTALL_DIR}"

if [[ ! -f .env.production ]]; then
  echo "Erro: .env.production não encontrado em ${INSTALL_DIR}."
  exit 1
fi

# shellcheck disable=SC1091
set -a
source .env.production
set +a

: "${DOMAIN_API:?DOMAIN_API is required in .env.production}"

if [[ -n "${DEPLOY_GIT_REF:-}" ]]; then
  echo "==> Checkout ${DEPLOY_GIT_REF}..."
  git fetch origin --tags
  git checkout "${DEPLOY_GIT_REF}"
fi

export IMAGE_TAG="${IMAGE_TAG:-latest}"
export REGISTRY="${REGISTRY:-ghcr.io/rrbertuolo21}"

echo "==> Deploy EprosERP — REGISTRY=${REGISTRY} IMAGE_TAG=${IMAGE_TAG}"

if [[ "${BUILD_LOCAL}" == "true" ]]; then
  echo "==> Build local (bootstrap)..."
  docker compose -f "${COMPOSE_FILE}" build api frontend migrate
else
  if [[ -n "${GHCR_TOKEN:-}" ]]; then
    echo "${GHCR_TOKEN}" | docker login ghcr.io -u "${GHCR_USER:-github-actions}" --password-stdin
  fi
  echo "==> Pull imagens api e frontend..."
  docker compose -f "${COMPOSE_FILE}" pull api frontend
fi

echo "==> Garantindo infra (postgres, valkey, minio)..."
docker compose -f "${COMPOSE_FILE}" up -d postgres valkey minio

echo "==> Aguardando postgres healthy..."
docker compose -f "${COMPOSE_FILE}" up -d --wait postgres 2>/dev/null || true

echo "==> Executando migrations..."
docker compose -f "${COMPOSE_FILE}" build migrate
docker compose -f "${COMPOSE_FILE}" run --rm --no-deps migrate

echo "==> Subindo api, frontend e caddy..."
docker compose -f "${COMPOSE_FILE}" up -d --no-deps --force-recreate api
docker compose -f "${COMPOSE_FILE}" up -d --no-deps frontend
docker compose -f "${COMPOSE_FILE}" up -d --no-deps caddy

echo "==> Healthcheck https://${DOMAIN_API}/health ..."
for i in $(seq 1 "${HEALTH_RETRIES}"); do
  if curl -fsS "https://${DOMAIN_API}/health" >/dev/null 2>&1; then
    echo "==> Deploy OK — API saudável."
    exit 0
  fi
  echo "    Tentativa ${i}/${HEALTH_RETRIES} — aguardando ${HEALTH_INTERVAL}s..."
  sleep "${HEALTH_INTERVAL}"
done

echo "Erro: healthcheck falhou após ${HEALTH_RETRIES} tentativas."
docker compose -f "${COMPOSE_FILE}" logs --tail=50 api caddy
exit 1
