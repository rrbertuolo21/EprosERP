#!/usr/bin/env bash
# Seed do ambiente LOCAL do EprosERP novo (após `docker compose -f docker-compose.local.yml up -d --build`).
# Cria: admin de plataforma + cliente (tenant) com usuário admin e "tudo liberado" (perfil Administrador → bypass ABAC).
# Uso (Unix/macOS/Git Bash): ./scripts/seed-local.sh
# Uso (Windows PowerShell):  ./scripts/seed-local.ps1
set -euo pipefail

API="${API:-http://localhost:8080/api/v1}"
DB_CONTAINER="${DB_CONTAINER:-epros-novo-db}"

ADMIN_EMAIL="admin@epros.local";     ADMIN_SENHA="Admin@12345"
CLI_EMAIL="cliente@demo.local";      CLI_SENHA="Cliente@12345"

# python3 (Unix) ou python (Windows) — evita acoplar ao SO
json_get() {
  local json="$1" key="$2"
  if command -v python3 >/dev/null 2>&1; then
    printf '%s' "$json" | python3 -c "import sys,json; d=json.load(sys.stdin).get('dados') or {}; print(d.get('$key') or '')"
  elif command -v python >/dev/null 2>&1; then
    printf '%s' "$json" | python -c "import sys,json; d=json.load(sys.stdin).get('dados') or {}; print(d.get('$key') or '')"
  else
    echo "ERRO: precisa de python3 ou python no PATH para parsear o JSON." >&2
    exit 1
  fi
}

resolve_user_from_db() {
  docker exec "$DB_CONTAINER" psql -U epros -d epros -t -A -c \
    "SELECT id::text || '|' || tenant_id FROM aplicativo.usuarios WHERE email='$CLI_EMAIL' LIMIT 1;"
}

echo "==> Aguardando API em $API ..."
ready=false
for _ in $(seq 1 60); do
  if curl -sf "$API/installation/state" >/dev/null 2>&1; then
    ready=true
    break
  fi
  sleep 2
done
if [[ "$ready" != "true" ]]; then
  echo "ERRO: API não respondeu em 120s. Suba o stack: docker compose -f docker-compose.local.yml up -d" >&2
  exit 1
fi

echo "==> 1) Instalar admin de plataforma"
curl -s -X POST "$API/installation/execute" -H "Content-Type: application/json" \
  -d "{\"adminNome\":\"Administrador\",\"adminEmail\":\"$ADMIN_EMAIL\",\"adminSenha\":\"$ADMIN_SENHA\"}" >/dev/null || true

echo "==> 2) Registrar cliente (tenant) + usuário admin"
REG=$(curl -s -X POST "$API/public/auth/registrar-tenant" -H "Content-Type: application/json" \
  -d "{\"nomeEmpresa\":\"Cliente Demo LTDA\",\"cnpj\":\"11222333000181\",\"nomeAdmin\":\"Admin Cliente\",\"emailAdmin\":\"$CLI_EMAIL\",\"senhaAdmin\":\"$CLI_SENHA\"}" || true)
echo "    $REG"
USERID=$(json_get "$REG" "UsuarioAdminId" 2>/dev/null || true)
TENANT=$(json_get "$REG" "TenantId" 2>/dev/null || true)

if [[ -z "${USERID:-}" || -z "${TENANT:-}" ]]; then
  echo "    Tenant/usuário já existia — resolvendo no banco..."
  ROW=$(resolve_user_from_db)
  USERID="${ROW%%|*}"
  TENANT="${ROW#*|}"
fi

if [[ -z "${USERID:-}" || -z "${TENANT:-}" || "$USERID" == "$TENANT" ]]; then
  echo "ERRO: não foi possível obter UsuarioAdminId/TenantId para $CLI_EMAIL." >&2
  exit 1
fi

echo "==> 3) Semear perfil 'Administrador' (bypass ABAC = tudo liberado) para $USERID / $TENANT"
docker exec "$DB_CONTAINER" psql -U epros -d epros -v ON_ERROR_STOP=1 -c "
INSERT INTO plataforma.perfil_colaborador
  (id, user_id, nome, email, cargo, departamento, limite_desconto, ativo, sync_id, tenant_id, sync_version, criado_em, criado_por)
SELECT gen_random_uuid(), '$USERID', 'Administrador', '$CLI_EMAIL', 'Administrador', 'Administração', 100, true,
       gen_random_uuid(), '$TENANT', 1, now(), 'system-seed'
WHERE NOT EXISTS (SELECT 1 FROM plataforma.perfil_colaborador WHERE user_id='$USERID');
"

echo ""
echo "==> PRONTO. Credenciais:"
echo "    Front:  http://localhost:3000"
echo "    API:    http://localhost:8080/swagger"
echo "    Admin plataforma:  $ADMIN_EMAIL / $ADMIN_SENHA"
echo "    Cliente (ERP):     $CLI_EMAIL / $CLI_SENHA   (tudo liberado)"
