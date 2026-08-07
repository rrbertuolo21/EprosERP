#!/usr/bin/env bash
# Validação local do setup de deploy (CI/smoke antes do go-live).
# Uso: ./scripts/validate-deploy-setup.sh
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${ROOT}"

TEMP_ENV=false
if [[ ! -f .env.production ]]; then
  cp .env.production.example .env.production
  TEMP_ENV=true
  trap 'rm -f .env.production' EXIT
fi

echo "==> Validando sintaxe dos scripts shell..."
bash -n scripts/deploy-prod.sh
bash -n scripts/backup-prod.sh
bash -n scripts/server-bootstrap.sh
bash -n scripts/validate-deploy-setup.sh

echo "==> Validando docker-compose.prod.yml..."
docker compose --env-file .env.production.example -f docker-compose.prod.yml config --quiet

echo "==> Verificando artefatos de deploy..."
test -f .env.production.example
test -f .github/workflows/deploy-prod.yml
test -f docs/ops/PRODUCAO.md

echo "==> Validando workflow deploy-prod.yml (estrutura)..."
grep -q 'name: Deploy Production' .github/workflows/deploy-prod.yml
grep -q 'epros-api' .github/workflows/deploy-prod.yml
grep -q 'epros-frontend' .github/workflows/deploy-prod.yml
grep -q 'deploy-prod.sh' .github/workflows/deploy-prod.yml

if [[ "${TEMP_ENV}" == "true" ]]; then
  rm -f .env.production
  trap - EXIT
fi

echo "==> Setup de deploy validado localmente."
echo "Próximo passo: bootstrap na VPS + secrets GitHub + workflow_dispatch."
