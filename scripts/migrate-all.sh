#!/usr/bin/env bash
set -euo pipefail

STARTUP_PROJECT="src/API/Epros.API/Epros.API.csproj"
BUILD_CONFIGURATION="${BUILD_CONFIGURATION:-Release}"
POSTGRES_HOST="${POSTGRES_HOST:-postgres}"
POSTGRES_PORT="${POSTGRES_PORT:-5432}"

# project_path|DbContextName
MODULES=(
  "src/Modules/Epros.Modules.GestaoClientes|ContextGestaoClientes"
  "src/Modules/Epros.Modules.Aplicativo|ContextAplicativo"
  "src/Modules/Epros.Modules.Estoque|ContextEstoque"
  "src/Modules/Epros.Modules.Fiscal|ContextFiscal"
  "src/Modules/Epros.Modules.Financeiro|ContextFinanceiro"
  "src/Modules/Epros.Modules.Vendas|ContextVendas"
  "src/Modules/Epros.Modules.Qualidade|ContextQualidade"
  "src/Modules/Epros.Modules.Producao|ContextProducao"
  "src/Modules/Epros.Modules.RH|ContextRH"
  "src/Modules/Epros.Modules.Projetos|ContextProjetos"
  "src/Modules/Epros.Modules.Manutencao|ContextManutencao"
  "src/Modules/Epros.Modules.GRC|ContextGRC"
  "src/Modules/Epros.Modules.ESG|ContextESG"
  "src/Modules/Epros.Modules.DMS|ContextDMS"
)

echo "==> Aguardando PostgreSQL em ${POSTGRES_HOST}:${POSTGRES_PORT}..."
until (echo > "/dev/tcp/${POSTGRES_HOST}/${POSTGRES_PORT}") >/dev/null 2>&1; do
  echo "    PostgreSQL ainda indisponível, tentando novamente em 3s..."
  sleep 3
done

# O stage migrate não herda o cache NuGet do stage build — restore obrigatório.
echo "==> Restaurando pacotes NuGet..."
dotnet restore "$STARTUP_PROJECT"

echo "==> Compilando API (${BUILD_CONFIGURATION})..."
dotnet build "$STARTUP_PROJECT" -c "$BUILD_CONFIGURATION" --no-restore

echo "==> Aplicando migrations EF Core (${#MODULES[@]} módulos)..."
for entry in "${MODULES[@]}"; do
  module="${entry%%|*}"
  context="${entry##*|}"
  echo "    -> $module ($context)"
  dotnet ef database update \
    --project "$module" \
    --startup-project "$STARTUP_PROJECT" \
    --context "$context" \
    --configuration "$BUILD_CONFIGURATION" \
    --no-build
done

echo "==> Semeando catálogos fiscais..."
dotnet run \
  --project "$STARTUP_PROJECT" \
  --configuration "$BUILD_CONFIGURATION" \
  --no-build \
  -- \
  --seed-fiscal

if command -v psql >/dev/null 2>&1; then
  echo "==> Aplicando políticas RLS de autenticação cross-tenant..."
  PGPASSWORD="${POSTGRES_PASSWORD:?POSTGRES_PASSWORD is required}" \
    psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER:-epros}" -d "${POSTGRES_DB:-epros}" \
    -v ON_ERROR_STOP=1 -f scripts/apply-auth-rls-policies.sql
fi

echo "==> Migrations e seed concluídos."
