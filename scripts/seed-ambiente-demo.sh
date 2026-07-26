#!/usr/bin/env bash
# Seed do ambiente de produção — instalação + tenant demo
# Uso:
#   ./scripts/seed-ambiente-demo.sh
#   API_BASE_URL=https://api.example.com ./scripts/seed-ambiente-demo.sh
#   ./scripts/seed-ambiente-demo.sh https://api.epros.localhost

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
ENV_FILE="${ENV_FILE:-${REPO_ROOT}/.env.production}"

if [[ -n "${1:-}" ]]; then
  API_BASE_URL="$1"
elif [[ -z "${API_BASE_URL:-}" && -f "${ENV_FILE}" ]]; then
  DOMAIN_API="$(grep -E '^DOMAIN_API=' "${ENV_FILE}" | head -n1 | cut -d= -f2- | tr -d '\r')"
  if [[ -n "${DOMAIN_API}" ]]; then
    API_BASE_URL="https://${DOMAIN_API}"
  fi
fi

API_BASE_URL="${API_BASE_URL:-https://api.example.com}"
BASE_URL="${API_BASE_URL%/}/api/v1"

# Caddy local (.localhost) usa certificado interno — curl precisa de -k
CURL_OPTS=(-sf)
if [[ "${API_BASE_URL}" == *".localhost"* ]]; then
  CURL_OPTS+=(-k)
fi

echo "Aguardando API em ${BASE_URL}..."
ready=false
for _ in $(seq 1 60); do
  if curl "${CURL_OPTS[@]}" "${BASE_URL}/installation/state" > /dev/null 2>&1; then
    ready=true
    break
  fi
  sleep 2
done

if [[ "${ready}" != "true" ]]; then
  echo "ERRO: API não respondeu em 120s."
  exit 1
fi

state=$(curl "${CURL_OPTS[@]}" "${BASE_URL}/installation/state")
if echo "${state}" | grep -q '"isCompleted":false\|"isCompleted": false'; then
  echo "Executando instalação inicial..."
  curl "${CURL_OPTS[@]}" -X POST "${BASE_URL}/installation/execute" \
    -H "Content-Type: application/json" \
    -d '{"adminNome":"Super Admin","adminEmail":"admin@epros.com.br","adminSenha":"SenhaSuperSegura123"}' \
    || echo "Instalação pode já ter sido concluída."
else
  echo "Instalação já concluída."
fi

DEMO_EMAIL="demo@epros.local"
DEMO_SENHA="Demo@123456"

echo "Registrando tenant demo..."
register_payload="{\"nomeEmpresa\":\"Empresa Demo Ltda\",\"cnpj\":\"12345678000195\",\"nomeAdmin\":\"Administrador Demo\",\"emailAdmin\":\"${DEMO_EMAIL}\",\"senhaAdmin\":\"${DEMO_SENHA}\"}"
register_response_file="$(mktemp)"
register_http_code="$(curl "${CURL_OPTS[@]}" -o "${register_response_file}" -w "%{http_code}" -X POST "${BASE_URL}/public/auth/registrar-tenant" \
  -H "Content-Type: application/json" \
  -d "${register_payload}" || true)"

if [[ "${register_http_code}" == "200" ]]; then
  echo "Tenant demo criado via API."
  cat "${register_response_file}"
  echo ""
elif [[ -f "${REPO_ROOT}/scripts/seed-demo-tenant.sql" ]] && command -v docker >/dev/null 2>&1; then
  echo "Registro via API falhou (HTTP ${register_http_code:-erro}). Tentando seed SQL local..."
  cat "${register_response_file}" 2>/dev/null || true
  echo ""
  if docker exec -i epros-postgres psql -U "${POSTGRES_USER:-epros}" -d "${POSTGRES_DB:-epros}" < "${REPO_ROOT}/scripts/seed-demo-tenant.sql"; then
    echo "Tenant demo criado via scripts/seed-demo-tenant.sql"
  else
    echo "ERRO: não foi possível criar o tenant demo."
    rm -f "${register_response_file}"
    exit 1
  fi
else
  echo "Registro tenant falhou (HTTP ${register_http_code:-erro})."
  cat "${register_response_file}" 2>/dev/null || true
  echo ""
  echo "Execute manualmente: docker exec -i epros-postgres psql -U epros -d epros < scripts/seed-demo-tenant.sql"
  rm -f "${register_response_file}"
  exit 1
fi
rm -f "${register_response_file}"

echo ""
echo "========== CREDENCIAIS DEMO =========="
echo "Super Admin Plataforma: admin@epros.com.br / SenhaSuperSegura123"
echo "Tenant ERP Demo:        ${DEMO_EMAIL} / ${DEMO_SENHA}"
echo "Frontend:               leia DOMAIN_APP em .env.production"
echo "======================================="
