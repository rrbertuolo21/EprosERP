# Seed do ambiente LOCAL do EprosERP novo (apos `docker compose -f docker-compose.local.yml up -d --build`).
# Cria: admin de plataforma + cliente (tenant) com usuario admin e "tudo liberado" (perfil Administrador = bypass ABAC).
# Uso (Windows PowerShell):  ./scripts/seed-local.ps1
# Uso (Unix/macOS/Git Bash): ./scripts/seed-local.sh

$ErrorActionPreference = "Stop"

$Api = if ($env:API) { $env:API } else { "http://localhost:8080/api/v1" }
$DbContainer = if ($env:DB_CONTAINER) { $env:DB_CONTAINER } else { "epros-novo-db" }

$AdminEmail = "admin@epros.local"
$AdminSenha = "Admin@12345"
$CliEmail = "cliente@demo.local"
$CliSenha = "Cliente@12345"

function Wait-ApiReady {
    Write-Host "==> Aguardando API em $Api ..."
    for ($i = 0; $i -lt 60; $i++) {
        try {
            Invoke-RestMethod -Uri "$Api/installation/state" -Method Get -TimeoutSec 3 | Out-Null
            return
        } catch {
            Start-Sleep -Seconds 2
        }
    }
    Write-Error "API nao respondeu em 120s. Suba o stack: docker compose -f docker-compose.local.yml up -d"
}

function Resolve-UserFromDb {
    $row = docker exec $DbContainer psql -U epros -d epros -t -A -c "SELECT id::text || '|' || tenant_id FROM aplicativo.usuarios WHERE email='$CliEmail' LIMIT 1;"
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($row)) {
        return $null
    }
    $parts = $row.Trim().Split("|")
    if ($parts.Length -lt 2) { return $null }
    return @{ UserId = $parts[0]; TenantId = $parts[1] }
}

Wait-ApiReady

Write-Host "==> 1) Instalar admin de plataforma"
try {
    $bodyInstall = @{
        adminNome  = "Administrador"
        adminEmail = $AdminEmail
        adminSenha = $AdminSenha
    } | ConvertTo-Json
    Invoke-RestMethod -Uri "$Api/installation/execute" -Method Post -Body $bodyInstall -ContentType "application/json" | Out-Null
} catch {
    # Ja instalado — segue para o registro do tenant.
}

Write-Host "==> 2) Registrar cliente (tenant) + usuario admin"
$bodyTenant = @{
    nomeEmpresa = "Cliente Demo LTDA"
    cnpj        = "11222333000181"
    nomeAdmin   = "Admin Cliente"
    emailAdmin  = $CliEmail
    senhaAdmin  = $CliSenha
} | ConvertTo-Json

$userId = $null
$tenant = $null
try {
    $reg = Invoke-RestMethod -Uri "$Api/public/auth/registrar-tenant" -Method Post -Body $bodyTenant -ContentType "application/json"
    Write-Host "    $($reg | ConvertTo-Json -Compress)"
    if ($reg.sucesso -and $reg.dados) {
        $userId = $reg.dados.UsuarioAdminId
        $tenant = $reg.dados.TenantId
    }
} catch {
    $resp = $_.ErrorDetails.Message
    if ($resp) { Write-Host "    $resp" } else { Write-Host "    $($_.Exception.Message)" }
}

if (-not $userId -or -not $tenant) {
    Write-Host "    Tenant/usuario ja existia - resolvendo no banco..."
    $resolved = Resolve-UserFromDb
    if ($resolved) {
        $userId = $resolved.UserId
        $tenant = $resolved.TenantId
    }
}

if (-not $userId -or -not $tenant) {
    Write-Error "Nao foi possivel obter UsuarioAdminId/TenantId para $CliEmail."
}

Write-Host "==> 3) Semear perfil Administrador (bypass ABAC) para $userId / $tenant"
$sql = @"
INSERT INTO plataforma.perfil_colaborador
  (id, user_id, nome, email, cargo, departamento, limite_desconto, ativo, sync_id, tenant_id, sync_version, criado_em, criado_por)
SELECT gen_random_uuid(), '$userId', 'Administrador', '$CliEmail', 'Administrador', 'Administracao', 100, true,
       gen_random_uuid(), '$tenant', 1, now(), 'system-seed'
WHERE NOT EXISTS (SELECT 1 FROM plataforma.perfil_colaborador WHERE user_id='$userId');
"@

docker exec $DbContainer psql -U epros -d epros -v ON_ERROR_STOP=1 -c $sql
if ($LASTEXITCODE -ne 0) {
    Write-Error "docker exec psql falhou (exit $LASTEXITCODE). Container: $DbContainer"
}

Write-Host ""
Write-Host "==> PRONTO. Credenciais:"
Write-Host "    Front:  http://localhost:3000"
Write-Host "    API:    http://localhost:8080/swagger"
Write-Host "    Admin plataforma:  $AdminEmail / $AdminSenha"
Write-Host "    Cliente (ERP):     $CliEmail / $CliSenha   (tudo liberado)"
