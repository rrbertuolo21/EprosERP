#!/usr/bin/env bash
# Seed do ambiente LOCAL do EprosERP novo (após `docker compose -f docker-compose.local.yml up -d --build`).
# Cria: admin de plataforma + cliente (tenant) com usuário admin e "tudo liberado" (perfil Administrador → bypass ABAC).
set -euo pipefail

API="${API:-http://localhost:8080/api/v1}"
DB_CONTAINER="${DB_CONTAINER:-epros-novo-db}"

ADMIN_EMAIL="admin@epros.local";     ADMIN_SENHA="Admin@12345"
CLI_EMAIL="cliente@demo.local";      CLI_SENHA="Cliente@12345"

echo "==> 1) Instalar admin de plataforma"
curl -s -X POST "$API/installation/execute" -H "Content-Type: application/json" \
  -d "{\"adminNome\":\"Administrador\",\"adminEmail\":\"$ADMIN_EMAIL\",\"adminSenha\":\"$ADMIN_SENHA\"}" >/dev/null || true

echo "==> 2) Registrar cliente (tenant) + usuário admin"
REG=$(curl -s -X POST "$API/public/auth/registrar-tenant" -H "Content-Type: application/json" \
  -d "{\"nomeEmpresa\":\"Cliente Demo LTDA\",\"cnpj\":\"11222333000181\",\"nomeAdmin\":\"Admin Cliente\",\"emailAdmin\":\"$CLI_EMAIL\",\"senhaAdmin\":\"$CLI_SENHA\"}")
echo "    $REG"
USERID=$(echo "$REG" | python3 -c "import sys,json;print(json.load(sys.stdin)['dados']['UsuarioAdminId'])")
TENANT=$(echo "$REG" | python3 -c "import sys,json;print(json.load(sys.stdin)['dados']['TenantId'])")

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
