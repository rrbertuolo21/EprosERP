# Seed do ambiente local EprosERP — instalação + tenant demo
# Executar com a API rodando em http://localhost:5000

$baseUrl = "http://localhost:5000/api/v1"
$ErrorActionPreference = "Stop"

Write-Host "Aguardando API em $baseUrl ..." -ForegroundColor Cyan
$ready = $false
for ($i = 0; $i -lt 60; $i++) {
    try {
        Invoke-RestMethod -Uri "$baseUrl/installation/state" -Method Get -TimeoutSec 3 | Out-Null
        $ready = $true
        break
    } catch {
        Start-Sleep -Seconds 2
    }
}
if (-not $ready) {
    Write-Error "API não respondeu em 120s. Inicie: dotnet run --project src/API/Epros.API --urls http://localhost:5000"
}

# 1. Instalação do super-admin da plataforma (Siser)
$state = Invoke-RestMethod -Uri "$baseUrl/installation/state" -Method Get
if (-not $state.isCompleted) {
    Write-Host "Executando instalação inicial..." -ForegroundColor Yellow
    $bodyInstall = @{
        adminNome  = "Super Admin"
        adminEmail = "admin@epros.com.br"
        adminSenha = "SenhaSuperSegura123"
    } | ConvertTo-Json
    $install = Invoke-RestMethod -Uri "$baseUrl/installation/execute" -Method Post -Body $bodyInstall -ContentType "application/json"
    if (-not $install.sucesso) {
        Write-Warning "Instalação retornou: $($install | ConvertTo-Json -Depth 5)"
    } else {
        Write-Host "Instalação concluída." -ForegroundColor Green
    }
} else {
    Write-Host "Instalação já concluída." -ForegroundColor Green
}

# 2. Tenant demo para ERP operacional
$demoEmail = "demo@epros.local"
$demoSenha = "Demo@123456"
$demoCnpj  = "12345678000195"

Write-Host "Registrando tenant demo..." -ForegroundColor Yellow
try {
    $bodyTenant = @{
        nomeEmpresa = "Empresa Demo Ltda"
        cnpj        = $demoCnpj
        nomeAdmin   = "Administrador Demo"
        emailAdmin  = $demoEmail
        senhaAdmin  = $demoSenha
    } | ConvertTo-Json
    $tenant = Invoke-RestMethod -Uri "$baseUrl/public/auth/registrar-tenant" -Method Post -Body $bodyTenant -ContentType "application/json"
    if ($tenant.sucesso) {
        Write-Host "Tenant demo criado." -ForegroundColor Green
    } else {
        Write-Host "Registro tenant (pode já existir): $($tenant.erros -join ', ')" -ForegroundColor DarkYellow
    }
} catch {
    Write-Host "Registro tenant ignorado (provavelmente já existe): $_" -ForegroundColor DarkYellow
    Write-Host "Aplicando seed SQL de fallback..." -ForegroundColor Yellow
    $sqlPath = Join-Path $PSScriptRoot "seed-demo-tenant.sql"
    if (Test-Path $sqlPath) {
        Get-Content $sqlPath | docker exec -i epros-postgres psql -U epros -d epros | Out-Null
        Write-Host "Tenant demo inserido via SQL." -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "========== CREDENCIAIS DEMO ==========" -ForegroundColor Cyan
Write-Host "Super Admin Plataforma: admin@epros.com.br / SenhaSuperSegura123"
Write-Host "Tenant ERP Demo:        $demoEmail / $demoSenha"
Write-Host "Tenant ID (login):      use e-mail acima; campo tenant pode ficar vazio"
Write-Host "Swagger API:            http://localhost:5000/swagger"
Write-Host "Frontend:               http://localhost:3000"
Write-Host "Keycloak:               http://localhost:8080 (admin/admin)"
Write-Host "MinIO Console:          http://localhost:9001 (epros_minio / epros_minio_password)"
Write-Host "=======================================" -ForegroundColor Cyan
