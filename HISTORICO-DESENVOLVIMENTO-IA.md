# EprosERP — Histórico de desenvolvimento assistido por IA

> Registro do ciclo de construção do EprosERP conduzido pela **Fábrica de Software com IA** (Claude).
> Estado em **2026-07-26**. Branch `main` = commit `882c748` (em paridade com `origin/main`).
> Este documento é o "diário de bordo": o que foi feito, por quê, e como retomar.

## 1. O que é o EprosERP (contexto de arquitetura)

ERP SaaS multi-tenant, **monólito modular** em **.NET 8**, Clean Architecture (Domain/Application/
Infrastructure/Migrations por módulo), **CQRS com MediatR**, **PostgreSQL + EF Core**, isolamento
multi-tenant por **RLS** (`TenantRlsInterceptor` + `current_setting`), padrão **Outbox**, autorização
**ABAC** (`[AbacAuthorize(recurso, acao)]`), validação **Flunt**. Front **Nuxt 3 + TypeScript** (SPA,
`EprosApp`), IO 100% via `useApi`/`useApiList` (base do `runtimeConfig`, token/tenant automáticos).
Mobile: **React Native** (submódulo `Epros.Mobile`).

**Estrutura do repo (achatada, deploy-ready):** `src/` (backend), `EprosApp/` (front), `scripts/`,
`infra/`, `docker-compose.prod.yml`, `tests/`, `Epros.sln` na raiz.

## 2. Números atuais (verificados no banco/git vivos)

| | |
|---|---|
| Commits da fábrica | 14 (sobre a base `45b4986`) |
| Módulos backend | **15** (Aplicativo, GestaoClientes, Estoque, Fiscal, Financeiro, Vendas, Qualidade, Producao, RH, Projetos, Manutencao, GRC, ESG, DMS, Imobiliaria) |
| Tabelas no PostgreSQL | **~870** (14 schemas) |
| Endpoints da API | **~1059** (swagger 200) |
| Telas ERP (Nuxt) | **306** |
| Telas Admin/Landlord | **22** |

## 3. Linha do tempo (blocos de trabalho)

- **Bloco 0–1 — Fundação + segurança (P0):** build verde; token JWT assinado HS256 (`EprosTokenService`,
  substituiu token plaintext forjável — furo cross-tenant); secrets fail-closed (webhook MP, JWT key);
  RBAC/ABAC em controllers sensíveis. Harness de teste em **PostgreSQL real** (RLS não roda em InMemory).
- **Bloco 2 — Frente 1 (núcleo):** cadastros/vendas/compras/estoque/fiscal/financeiro — build-out CQRS.
- **Blocos 4–6 — Fan-out por módulo (Modo Porte):** RH (151 tab), Vendas (115), Estoque (99), Financeiro
  (70), Manutenção, Qualidade, Produção, Projetos, GRC, ESG, DMS. Consolidação serializada + banco vivo
  (bugs de produção caçados: transação cross-context no registro de tenant; agregado-filho como `Modified`;
  colisão de coluna). Módulo **Imobiliária** criado do zero.
- **Bloco 7 — Fundação do frontend:** OpenAPI novo (fix de 8 controllers `[FromForm] IFormFile` que
  quebravam o swagger); scaffolding compartilhado (useApi/useApiList/DataTable/campos/layouts) — descoberto
  já pronto; 63 telas do núcleo já existiam.
- **Bloco 8 — Front dos módulos avançados:** **242 telas** Nuxt geradas por fan-out (1 agente/módulo),
  fiéis ao swagger + EF, padrão `useApi`. Menu lateral do ERP fiado.
- **Bloco 9 — Docker local full-stack:** `docker-compose.local.yml` (infra + migrate + API:8080 + front:3000).
  Bugs reais caçados ao subir de verdade: Dockerfile sem o `.csproj` da Imobiliária; 3 design-time factories
  (Estoque/Vendas/DMS) hardcodando `localhost/epros_design`; `migrate-all.sh` sem `POSTGRES_*`. Corrigidos.
  Seed: admin de plataforma + cliente demo com perfil "Administrador" (bypass ABAC).
- **Bloco 10 — Consolidação com o git + paridade:** o `origin/main` tinha achatado a estrutura ("correções
  para publicação no servidor"); achatei minha branch (`EprosERP-main/*` → raiz), mergeei preservando os dois
  lados, e subi a main (fast-forward). **Login de SuperAdmin/operador interno** implementado (não existia):
  `POST /public/plataforma/login` contra `UsuariosInternos` + curto-circuito do `AbacFilter` p/ tenant
  `system`. 9 telas legadas religadas (tiravam `localhost:5000` morto → `useApi`).
- **Bloco 11 — Área Landlord/Admin reconstruída:** shell com **sidebar** (Cadastros/Comercial/Faturamento/
  Operação/Sistema) + Dashboard; telas: Planos (form com sub-lista de módulos e valor por módulo, total=Σ),
  Grupos de Planos, Módulos, Empresas, Clientes, Revendas, Vendedores, Faturas, Assinaturas, Equipe,
  Configurações, Execuções, Mensagens, Tarefas, Sobre. Backend: CRUD de Planos/Grupos/Faturas. Migration
  `AddCatalogoSaaSAddOns` (26 tabelas do GestaoClientes que nunca foram migradas — corrigiu 500 de add-ons).
  Campos ricos de Plano/ModuloPlano/GrupoPlano passaram a persistir (migration `AddCamposRicos...`).
- **Bloco 12 — Cobertura 100% + pagamentos (em andamento):** pente-fino do app antigo (`ControleTenant.API`
  + Blazor) + EFs do tenant → ERD + matriz de cobertura (validados com o Rafael). **P0 entregue:**
  integração **Mercado Pago outbound** — entidade `ConfiguracaoGatewayPagamento` (segredos cifrados no cofre,
  `TenantAlvo` null=global/preenchido=override), `IPaymentGateway` + `MercadoPagoGateway` (gerar PIX
  `POST /v1/payments`, consultar, testar-conexão), CRUD de gateways (token mascarado), endpoint
  `gerar-cobranca-pix` na fatura, tela **Integrações/Gateways** + botão "Gerar Pix" (QR+copia-e-cola).
  Migration `AddGatewayPagamentoConfig`. Validado: adaptador alcança a `api.mercadopago.com` real.

## 4. Como rodar (local)

```bash
cd EprosERP
docker compose -f docker-compose.local.yml up -d --build   # se o BuildKit der DeadlineExceeded, use: DOCKER_BUILDKIT=0 docker compose -f docker-compose.local.yml build && docker compose -f docker-compose.local.yml up -d
./scripts/seed-local.sh
```
- **Front:** http://localhost:3000 · **API/Swagger:** http://localhost:8080/swagger
- **Admin/Landlord:** `admin@epros.local` / `Admin@12345` → mesmo login, cai em `/plataforma/admin`
- **ERP (cliente demo):** `cliente@demo.local` / `Cliente@12345`
- Deploy servidor: `docker compose -f docker-compose.prod.yml up -d --build` (precisa `.env.production`).

## 5. Estratégia respeitada

- **Fidelidade campo-a-campo** à EF/legado (Modo Porte); nada fakeado.
- **Módulos sobem DESABILITADOS** (ABAC nega por padrão) — liberar por plano/cliente.
- **Fiscal travado** até validação de contador (regra da fábrica).
- **Gate humano:** o build/test verde foi sempre reconferido de verdade (não por relato de agente).

## 6. O que falta (honesto) — roadmap

**Cobrança (P1):** conciliação (polling de status no Mercado Pago); webhook tratando
cancel/estorno/expiração/em-análise (hoje só dá baixa); **Empresa** (múltiplos endereços + cascata
UF→Município); **Plano** (ação Duplicar + recursos estruturados); **Cliente** (sub-aba Quantidade de
Permissões). **P2:** API externa por token (`externo/*`) — só se houver sistema externo; Portal do
Desenvolvedor (tokens DFe) + histórico de transmissões SEFAZ.

**Backend geral:** fechar CRUD (GET/{id}, PUT, DELETE) dos agregados dos módulos avançados (muitos só têm
`GET lista`+`POST`); eventos de domínio tipados no `Epros.Shared` (MES→Estoque etc.); seed de permissões
ABAC por plano; validação fiscal humana; CI com serviço PostgreSQL. Detalhe em `CONSOLIDACAO-GAPS.md`.

**Front:** rodar `npx nuxi typecheck` no Mac (o ambiente da fábrica não tem node no PATH — o build real
é validado via Docker/`nuxt generate`, que transpila sem type-check).

## 7. Credenciais/segredos pendentes do Rafael

- **Mercado Pago:** cadastrar o access token (sandbox/produção) em **Operação → Integrações / Gateways**,
  testar conexão e então "Gerar Pix" na fatura emite QR real.
- **Deploy:** `.env.production` com `POSTGRES_PASSWORD`, `COFRE_KEK_LOCAL`, `MINIO_*`, `DOMAIN_*`.
