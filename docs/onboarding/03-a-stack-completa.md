---
title: "A stack completa: 15 tecnologias, uma decisão por vez"
confluence_id: "193396737"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/193396737/A+stack+completa+15+tecnologias+uma+decis+o+por+vez"
last_updated: "2026-07-06"
---

> [!NOTE]
> **O que você vai aprender:** as tecnologias do Epros agrupadas por responsabilidade, o critério de escolha (open source + cloud-agnostic) e como os repositórios se organizam.

ASP.NET Core 8, Nuxt 3, Keycloak, Vault — e o **porquê** de cada escolha.

Toda decisão de tecnologia no Epros segue um critério único: **cloud-agnostic + open source**. Componente que exige contrato com AWS, Azure ou GCP é substituído por equivalente open source. O sistema roda em VPS de €20/mês ou no data center do cliente Enterprise.

---

## Nota sobre nomenclatura de repositórios

| Nome antigo | Nome atual | Conteúdo |
| --- | --- | --- |
| `epros-back` / `epros_erp` | `src/` | API, módulos, Shared, testes |
| `epros-front` | `Epros.App/` | Nuxt 3 — `pages/`, `components/` |
| `epros-api` (YARP) | `src/API/Epros.API` | Host HTTP (sem YARP no compose atual) |

---

## Bloco 1 — Backend core

| Tecnologia | Versão | Papel |
| --- | --- | --- |
| ASP.NET Core | 8.0 LTS | Runtime e pipeline HTTP |
| MediatR | 12.x | CQRS — Commands, Queries, Handlers |
| FluentValidation | 11.x | Validação de input no pipeline |
| Flunt | 2.x | Validação de domínio (sem exceção) |
| Quartz.NET | 3.x | Jobs: Outbox, vencimentos, migrações |

```csharp
// Program.cs — ordem do pipeline (resumo)
app.UseAuthentication();
app.UseMiddleware<ExcecaoGlobalMiddleware>();
app.UseMiddleware<TenantSaaSMiddleware>();
app.UseMiddleware<ModuloTenantMiddleware>();
app.UseMiddleware<DataMaskingMiddleware>();
app.UseMiddleware<AuditMiddleware>();
app.UseAuthorization();
app.MapControllers();
```

MediatR é o fim dos controllers gigantes. Cada operação vira um handler isolado de dezenas de linhas — testável sem HTTP.

---

## Bloco 2 — Persistência e cache

| Tecnologia | Versão | Papel |
| --- | --- | --- |
| PostgreSQL | 16.x | 12 schemas separados por módulo |
| EF Core + Npgsql | 8.0.x | ORM com QueryFilter automático |
| Valkey | 7.x | Cache, sessions, Catalog DB (fork OSS do Redis) |
| MinIO | 2024 | Object storage S3-compatible (XMLs NF-e, PDFs) |

PostgreSQL concentra: `financas.*`, `estoque.*`, `vendas.*`, `plataforma.*` e demais schemas. Cada módulo tem seu schema — fronteira lógica para futura extração.

---

## Bloco 3 — Segurança

| Tecnologia | Versão | Papel |
| --- | --- | --- |
| Keycloak | 24.0 | OIDC, MFA, SSO — claim `tenantId` no JWT |
| HashiCorp Vault | 1.16.x | Secrets dinâmicos — nunca em appsettings de prod |
| Hercules.NET NFe | 2026.x | Motor fiscal homologado (NF-e, NFC-e, NFS-e) |
| Caddy | 2.x | Reverse proxy, HTTPS automático |

> [!IMPORTANT]
> JWT e connection strings nunca no código-fonte. Vault injeta em runtime.

---

## Bloco 4 — Observabilidade

| Tecnologia | Papel |
| --- | --- |
| Serilog 3 | Logs estruturados JSON com enricher de `tenantId` |
| OpenTelemetry | Traces → Tempo, métricas → Prometheus |
| Grafana 10 | Dashboards unificados |
| Loki 2 | Agregação de logs |
| Prometheus 2 | SLOs — P95 leitura <200ms, escrita <500ms |

---

## Bloco 5 — Frontend e mobile

| Tecnologia | Superfície | Status |
| --- | --- | --- |
| Nuxt 3 | Web SaaS (`Epros.App`) | Ativo |
| Electron + Nuxt 3 | Desktop (mesmo app) | Em uso / evolução |
| React Native | Mobile (`Epros.Mobile`) | Submódulo |

Um time, uma stack: Pinia para estado, composables para lógica reutilizável, TypeScript strict para DTOs da API.

---

## Bloco 6 — Infra e testes

| Tecnologia | Papel |
| --- | --- |
| Docker Compose v2 | Ambiente local — 5 serviços com healthchecks (PostgreSQL, Keycloak, Vault, MinIO, Valkey) |
| OpenTofu 1.7+ | IaC cloud-agnostic |
| Testcontainers 3.x | PostgreSQL real nos testes de integração |
| xUnit | Framework de testes |
| Trivy | CVE scan no CI — bloqueia deploy crítico |

O ambiente local sobe **5 serviços** via `docker compose`: PostgreSQL, Keycloak, Vault, MinIO e Valkey. Grafana, Prometheus, Loki e Tempo **não** sobem na máquina do dev (hardware dos clientes) — ficam no servidor via `docker-compose.observability.yml`; em local use o console do Serilog. Detalhes: [docker-compose.yml](stack/docker-compose.md).

Aprofundamento por componente:

* [docker-compose.yml — sobe tudo com um comando](stack/docker-compose.md)
* [YARP + ASP.NET Core — API Gateway público (epros-api)](stack/yarp-api-gateway.md)
* [Caddy 2 — reverse proxy e TLS automático](stack/caddy-2-reverse-proxy.md)
* [OpenTofu 1.7 — infraestrutura como código](stack/opentofu-infra.md)

---

## Tabela repo × responsabilidade

| Repositório | Quem trabalha | Principais pastas |
| --- | --- | --- |
| `src/` | Backend, QA | `Modules/`, `API/`, `tests/` |
| `Epros.App/` | Frontend | `pages/erp/`, `components/` |
| `docs/fabrica/` | Todos (processo) | `agentes/`, `skills/`, `cursor/rules` |

---

## Quando alterar a stack

> [!CAUTION]
> Versões estão fixadas. Alterar qualquer componente exige **ADR nova** aprovada pelo Tech Lead.

Mudança de versão ou substituição de componente sem ADR não entra em produção — mesmo que compile localmente.

---

**Próximo passo →** [Do Command ao PR: implementando Contas a Pagar do zero](04-do-command-ao-pr.md)
