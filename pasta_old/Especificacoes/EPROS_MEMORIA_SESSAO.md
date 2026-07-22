# EPROS — Memória de Sessão
> Cole este arquivo no início de qualquer nova sessão com Claude ou no Cursor antes de qualquer instrução.
> Última atualização: Maio 2026 · Bloco 5 concluído

---

## 1. IDENTIDADE DO PROJETO

| Campo | Valor |
|---|---|
| **Nome atual** | Epros |
| **Nome anterior** | WeERP (em uso por terceiros — não usar) |
| **Tipo** | ERP SaaS multi-tenant |
| **Repositório backend** | `PlataformaSaaS/` |
| **Repositório desktop** | `WeERP.App/` (a criar — Bloco 9) |
| **Repositório mobile** | `WeERP.Mobile/` (a criar — Bloco 10) |
| **Clientes em produção** | 20 clientes no Epros.ERP legado |
| **Bloco atual** | Bloco 5 concluído · Próximo: Bloco 6 |

---

## 2. STACK TÉCNICA — NÃO ALTERAR SEM ADR

```
Backend:        ASP.NET Core 8 / C#
ORM:            EF Core 8 + Npgsql
Banco:          PostgreSQL 16 (12 schemas separados por módulo)
CQRS:           MediatR 12
Validação dom:  Flunt (Notifiable<Notification>)
Validação cmd:  FluentValidation 11
Identity:       Keycloak 24 (OIDC · claim tenantId no JWT)
Secrets:        HashiCorp Vault 1.16 (dynamic secrets)
Cache/Locks:    Valkey 7 (fork open source Redis)
Storage:        MinIO (S3-compatible — XMLs NF-e, PDFs, GED)
Jobs:           Quartz.NET 3.x
Logs:           Serilog 3.x + sink Loki
Observab:       OpenTelemetry .NET → Grafana+Prometheus+Loki+Tempo
Testes:         xUnit + Testcontainers
Frontend web:   Nuxt 3 + Pinia
Desktop:        Electron + Nuxt 3 (WeERP.App)
Mobile/PDV:     React Native + Expo
Reverse proxy:  Caddy 2 (HTTPS automático)
IaC:            OpenTofu 1.7+
Lib NF-e:       Hercules.NET.NFe.NFCe v2026.3.15.14 ← NÃO SUBSTITUIR
```

---

## 3. ARQUITETURA — DECISÕES FECHADAS

### Padrão geral
**Monólito Modular com Hexagonal por bounded context.**
- Cada macrodomínio tem schema PostgreSQL próprio
- Módulos comunicam APENAS via Domain Events (Outbox Pattern)
- Sem DbContext cruzado entre módulos
- Preparado para extração em microserviços sem reescrever nada

### Pipeline HTTP (ordem obrigatória)
```
UseAuthentication → ExcecaoGlobalMiddleware → InquilinoSaaSMiddleware
→ ModuloTenantMiddleware → DataMaskingMiddleware → AuditMiddleware → Controllers
```

### Multi-tenancy — 3 níveis (mesmo binário)
- **Nível 1 Shared** → schema por tenant (Micro/Essencial)
- **Nível 2 Dedicated** → cluster exclusivo (Avançado/Enterprise)
- **Nível 3 Private** → on-premise/cloud própria (regulados)

---

## 4. REGRAS DE CÓDIGO — INVIOLÁVEIS

### SEMPRE
- Herdar `EntidadeSaaSBase` em toda entidade
- `DateTime.UtcNow` — nunca `DateTime.Now`
- `Guid` para IDs — nunca `long` ou `int`
- Soft delete via `entidade.Deletar(userId)` — nunca `context.Remove()`
- `ITenantProvider` via DI scoped — NUNCA estático
- Schema por macrodomínio no mapping EF Core
- `snake_case` nas colunas PostgreSQL
- CQRS — zero lógica de negócio no Controller
- Outbox Pattern para todo Domain Event
- `Precision(18,2)` em todo campo decimal
- Índice composto `(tenant_id, campo_negocio)` em toda tabela
- Índice único em `sync_id`

### NUNCA
- `context.Remove()` — soft delete apenas
- Lógica no Controller
- DbContext de outro módulo injetado
- Variável estática de TenantId
- JWT ou secret no código fonte
- Connection string em appsettings de produção
- `long` ou `int` como ID
- `DateTime.Now`
- Domain Event publicado diretamente (sem Outbox)
- SQL raw sem parâmetros
- Cross-schema query sem documentação

---

## 5. ENTIDADES BASE — SHARED

### EntidadeSaaSBase
```csharp
public abstract class EntidadeSaaSBase : Notifiable<Notification>, ISyncable
{
    public Guid Id { get; private set; }        // PK — gerado localmente
    public Guid SyncId { get; private set; }    // chave estável para sync offline
    public string TenantId { get; private set; }// QueryFilter automático
    public int SyncVersion { get; private set; }// detecção de conflito offline
    public DateTime CriadoEm { get; private set; }  // sempre UtcNow
    public DateTime? AlteradoEm { get; private set; }
    public DateTime? DeletadoEm { get; private set; } // soft delete
    public string? CriadoPor { get; private set; }   // userId Keycloak
    public string? AlteradoPor { get; private set; }

    protected EntidadeSaaSBase() { } // EF Core
    protected EntidadeSaaSBase(string tenantId, string criadoPor) { ... }

    public void MarcarAlterado(string alteradoPor) { ... SyncVersion++; }
    public void Deletar(string deletadoPor) { DeletadoEm = DateTime.UtcNow; }
    public bool EstaAtivo() => DeletadoEm == null;
    public bool IsValid => !Notifications.Any();
}
```

### ContextBase — QueryFilter automático por reflection
```csharp
// Aplica WHERE tenant_id = ? AND deletado_em IS NULL
// para TODAS as entidades que herdam EntidadeSaaSBase
// via reflection no OnModelCreating — nunca manual
```

### ISyncable
```csharp
public interface ISyncable {
    Guid SyncId { get; }
    string TenantId { get; }
    int SyncVersion { get; }
    DateTime CriadoEm { get; }
    DateTime? AlteradoEm { get; }
}
```

### OutboxMessage
```csharp
public class OutboxMessage {
    public Guid Id;
    public string TenantId;
    public string EventType;  // "VendaFaturada", "ColaboradorDesligado"...
    public string Payload;    // JSON do DomainEvent
    public DateTime CriadoEm;
    public DateTime? ProcessadoEm;
    public string? Erro;
    public int Tentativas;
}
```

### CommandResult
```csharp
public class CommandResult {
    public bool Sucesso;
    public string? Mensagem;
    public object? Dados;
    public IEnumerable<string> Erros;

    public static CommandResult Ok(string msg, object? dados = null) { ... }
    public static CommandResult Falha(IEnumerable<string> erros) { ... }
}
```

---

## 6. MÓDULOS — STATUS DE IMPLEMENTAÇÃO

| Módulo | Schema PG | Entidades | Handlers | Testes | Status |
|---|---|---|---|---|---|
| Shared/Base | — | EntidadeSaaSBase, OutboxMessage, DomainEvent, CommandResult, ISyncable | — | — | ✅ Completo |
| GestaoClientes | plataforma.* | Plano, ModuloPlano, Cliente, Fatura, FaturaComposicao | CriarCliente, AtivarCliente, CriarPlano, GerarFatura | 5 | ✅ Completo |
| Financeiro/CP | financas.* | ContaPagar, ContaPagarBaixa | Criar, Baixar, Cancelar | 8 | ✅ Completo |
| Financeiro/CR | financas.* | ContaReceber, ContaReceberBaixa | Criar, Baixar, Cancelar | — | ✅ Completo |
| Estoque/Produtos | estoque.* | Produto, EstoqueProduto | CriarProduto, MovimentarEstoque, Entrada | — | ✅ Completo |
| Fiscal/DFe | plataforma.* | DocumentoFiscal, EventoDocumentoFiscal | — | — | 🔵 Estrutura |
| Vendas/GestaoDePedidos | vendas.* | Venda, VendaItem, VendaPagamento, VendaDevolucao | EmitirVenda, CancelarVenda | — | ✅ Bloco 5 |
| Vendas/PDV | vendas.* | Caixa, CaixaMovimento | AbrirCaixa, FecharCaixa, Sangria, Suprimento, Contingência | 19 | ✅ Bloco 5 |
| Estoque/Compras | estoque.* | Compra, CompraItem | LancarCompra (pendente) | — | 🟡 Bloco 6 |

**Total: 92+ arquivos .cs · 32 testes unitários**

---

## 7. 11 MACRODOMÍNIOS + PLATAFORMA

### Núcleo (todos os planos)
- **FIN** — Financeiro & Contabilidade → `financas.*`
- **VEN** — Vendas & Distribuição → `vendas.*`
- **EST** — Estoque & Suprimentos → `estoque.*`

### Essencial
- **PRO** — Produção (MES/MRP/BOM) → `producao.*`
- **QUA** — Gestão da Qualidade → `qualidade.*`
- **HCM** — Capital Humano → `rh.*`

### Avançado
- **MAN** — Manutenção (EAM) → `manutencao.*`
- **PRJ** — Projetos (PPM) → `projetos.*`
- **GRC** — Governança, Risco & Compliance → `grc.*`

### Enterprise
- **ESG** — Sustentabilidade & ESG → `esg.*`
- **DMS** — Gestão de Concessionárias → `concessionarias.*`

### Plataforma transversal (sempre ativa)
Motor Fiscal BR, Motor Fiscal Edge (offline), Workflow/BPM, GED, eSignature,
Analytics/BI, IA/ML, Sync Engine, LGPD Compliance, Identity/ABAC,
API Gateway/SDK, Observabilidade

---

## 8. DOMAIN EVENTS — MAPA PRINCIPAL

| Evento | Produtor | Consumidor(es) | Efeito |
|---|---|---|---|
| VendaFaturada | VENDAS | FINANCEIRO · FISCAL | Cria ContaReceber + autoriza NF-e/NFC-e |
| VendaCancelada | VENDAS | FINANCEIRO · FISCAL · ESTOQUE | Estorna CR + cancela DFe + devolve estoque |
| CompraLancada | ESTOQUE | FINANCEIRO · FISCAL | Cria ContaPagar + NF-e entrada |
| MercadoriaRecebida | ESTOQUE | QUALIDADE · FINANCEIRO | Inspeção QUA + confirma CP |
| OrdemProducaoEncerrada | PRODUCAO | FINANCEIRO · ESTOQUE | Absorve custos GL + entrada produto |
| FolhaProcessada | HCM | FINANCEIRO | Provisão salarial no GL |
| ColaboradorAdmitido | HCM | GRC | Cria perfil de acesso Keycloak |
| ColaboradorDesligado | HCM | GRC · KEYCLOAK | Revoga todos os acessos |
| CaixaFechado | VENDAS/PDV | FINANCEIRO | Lança resumo caixa no fluxo |
| DocumentoFiscalAutorizado | FISCAL | VENDAS · ESTOQUE | Marca Venda/Compra como conferida |
| TransacaoRegistrada | FIN/VEN/EST | ESG | Alimenta cálculo de carbono |
| ViolacaoSoDDetectada | GRC | GRC · KEYCLOAK | Bloqueia acesso + abre incidente |

---

## 9. INFRA LOCAL — DOCKER COMPOSE

| Serviço | Imagem | Porta | Credenciais dev |
|---|---|---|---|
| PostgreSQL | postgres:16-alpine | 5432 | epros / epros_dev_password |
| Keycloak | keycloak:24.0 | 8080 | admin / admin |
| Vault | vault:1.16 | 8200 | token: epros-dev-token |
| MinIO | minio:latest | 9000/9001 | epros_minio / epros_minio_password |
| Valkey | valkey:7-alpine | 6379 | — |
| Grafana | grafana:latest | 3000 | admin / admin |
| Prometheus | prom/prometheus:latest | 9090 | — |
| Loki | grafana/loki:latest | 3100 | — |
| Tempo | grafana/tempo:latest | 3200/4317/4318 | — |

**URLs:**
- API + Swagger: `https://localhost:7000/swagger`
- Keycloak Admin: `http://localhost:8080`
- Grafana: `http://localhost:3000`
- MinIO Console: `http://localhost:9001`
- Vault UI: `http://localhost:8200`

---

## 10. LEGADOS ANALISADOS

| Sistema | Tech | Controllers mapeados | Status |
|---|---|---|---|
| epros_erp-main | .NET 8 / SQL Server / EF Core | CompraCtrl (3.294 linhas), VendaCtrl (2.535), VendaNfeCtrl (2.739), CPCtrl (910), CRCtrl (930) | Mapeado — CP e CR extraídos |
| epros_erp_front-main | Nuxt 3 + Vuetify | Frontend completo | Mapeado |
| epros_gestao_clientes | .NET 8 / Blazor WASM / PostgreSQL | GcFaturaCtrl, GcPlanoCtrl, GcClienteCtrl | Extraído e consolidado |

### Bugs do legado corrigidos por design
| Bug | Solução |
|---|---|
| `TenantData.TenantIdStatic` (vazamento entre tenants) | `ITenantProvider` scoped via `IHttpContextAccessor` |
| JWT secret hardcoded | Keycloak 24 com OIDC + MFA |
| SQL Server (licença comercial) | PostgreSQL 16 open source |
| `DateTime.Now` em todo código | `DateTime.UtcNow` enforced |
| IDs `long` sequenciais | `Guid` em todas as entidades |
| PDV Firebird | SQLite via Electron/Tauri |
| Controllers com 2.500–3.300 linhas | CQRS — handler por operação |
| QueryFilter manual (esquecia tenant) | ContextBase reflection automático |
| TenantMiddleware comentado | Implementado + testado |
| Blazor WASM no painel admin | Nuxt 3 unificado |

---

## 11. PROCESSO DE REENGENHARIA (3 ESTÁGIOS)

```
Estágio 0 (Cursor) → lê legado inteiro → produz MAPA_LEGADO.md
                     (só indexa, não extrai regras de negócio)

Estágio 1 (Cursor) → lê arquivos do mapa → produz RASCUNHO_EXTRACAO.md
                     por submódulo (classifica surface: WEB/DSK/MOB/PDV)

Estágio 2 (Claude) → merge semântico → preenche documentos canônicos
                     em 03_ERP_CANONICO/
```

---

## 12. PRÓXIMOS BLOCOS

| Bloco | O que | Prioridade |
|---|---|---|
| **Bloco 6** | Estoque/Compras: Compra + NF-e entrada + ContaPagar + EntradaEstoque | 🔴 Alta |
| **Bloco 7** | Infra: Migrations EF Core · Seeders Keycloak · CI/CD GitHub Actions · Script long→Guid (20 clientes) | 🔴 Alta |
| **Bloco 8** | Qualidade + Testes Testcontainers + Sync delta + Worker vencimento + Webhook MercadoPago | 🟡 Média |
| **Bloco 9** | Cadastros Base (Pessoa/Empresa) + Epros.App (Electron + Nuxt 3) | 🟡 Média |
| **Bloco 10** | Permissões ABAC + Epros.Mobile (React Native) | 🟢 Longo prazo |

---

## 13. ECOSSISTEMA DE DOCUMENTAÇÃO HTML

**Pasta:** `/mnt/user-data/outputs/weerp-docs/`
**CSS compartilhado:** `_shared.css` (tema escuro, tipografia Geist + Instrument Serif)

| Arquivo | Conteúdo | Subtabs | Tamanho |
|---|---|---|---|
| `index.html` | Home com links e status geral | — | 6KB |
| `01_produto.html` | Visão, Posicionamento, ICP, Diferenciais, Multi-Surface, Legados, Roadmap | 7 | 33KB |
| `02_modulos.html` | Módulos & Submódulos (acordeão), Planos, Dependências, Plataforma, Entitlement | 5 | 51KB |
| `03_arquitetura.html` | Decisão, Camadas, Pipeline HTTP, Princípios, Multi-tenancy, Desenvolvimento, Testes, SecDevOps, Governança TI, Prompt IA | 10 | 55KB |
| `04_stack.html` | Visão Geral, Backend, Dados & Cache, Segurança, Observabilidade, Infra & Deploy, Frontend & Mobile, Decisões de Escolha | 8 | 38KB |
| `05_compliance.html` | Regulações (12), Controles Técnicos, Direitos do Titular (8), Retenção de Dados (12), Fiscal BR | 5 | 29KB |
| `06_ddd.html` | Bounded Contexts (11), Entidades do Núcleo, Domain Events (16), Outbox Pattern, Schemas PG (12), Infra Local | 6 | 41KB |
| `07_tecnico.html` | Status Implementação, Padrões de Código, Decisões ADR (15), Bugs Corrigidos (10), Roadmap Técnico, Ambiente Local | 6 | 36KB |
| `08_api.html` | Auth (Keycloak OIDC), Convenções REST, Financeiro, Vendas & PDV, Estoque, Fiscal, Plataforma, Erros & Códigos | 8 | 45KB |

**Páginas pendentes (a criar uma a uma):**
- `09_runbook.html` — playbooks de incidente, SLAs, DR, migração de tier
- `10_modelo_financeiro.html` — pricing, unit economics, break-even, projeção de margem
- `11_integracoes.html` — SEFAZ, gateways pagamento, transportadoras, AD/LDAP, marketplaces
- `12_modelo_dados.html` — campos, tipos, constraints, relacionamentos por tabela
- `13_onboarding.html` — guia dev novo, primeiro commit, convenções
- `14_disaster_recovery.html` — RTO, RPO, procedimento de restore, teste de DR
- `15_glossario.html` — dicionário canônico de termos do projeto
- `16_i18n.html` — internacionalização, locales, formatos por país
- `17_changelog.html` — release notes por bloco

---

## 14. DECISÕES ABERTAS (ADR PENDENTE)

| Decisão | Opções em aberto |
|---|---|
| Desktop: nível de offline | Dias sem internet (SQLite full) vs. tolerância a instabilidade (minutos) |
| PDV: produto separado? | Modo do Epros.Mobile vs. produto dedicado com drivers de hardware |
| Conflict resolution offline | Last Write Wins · CRDT · Fila de reconciliação manual |
| Token JWT offline | Duração máxima sem Keycloak · biometria local para reautenticar |
| Catalog Database | Banco global de metadados de tenants — especificado, não implementado |
| Desktop: Tauri vs Electron | Tauri revisitar no Bloco 9 quando time tiver expertise Rust |

---

## 15. PRINCÍPIOS DE SELEÇÃO DE STACK

1. **Licença permissiva** — MIT ou Apache 2.0. BSL/SSPL são candidatos a substituição (como Redis → Valkey, Terraform → OpenTofu).
2. **Maturidade comprovada** — mínimo 3 anos de projeto, comunidade ativa, usado por empresas Fortune 500.
3. **Cloud-agnostic** — qualquer componente que só funcione em AWS/Azure/GCP é descartado. Roda em VPS de €20/mês ou data center próprio do cliente.

---

## 16. COMPLIANCE — REGULAÇÕES MAPEADAS

| Regulação | Região | Multa máxima |
|---|---|---|
| LGPD | Brasil | 2% faturamento BR · máx R$ 50M/infração |
| GDPR | União Europeia | 4% faturamento global ou €20M |
| PCI DSS 4.0 | Global | Variável por bandeira |
| CCPA/CPRA | California EUA | $7.500/violação intencional |
| PDPA | Tailândia | THB 5M (~€130k) |
| PIPL | China | CNY 50M (~€6,5M) |
| DPDP Act | Índia | ₹250 crore (~€27M) |
| CSRD | União Europeia | Variável por país-membro |
| SOC 2 Type II | Global (certificação) | — |

**Controles implementados por arquitetura (não configuração):**
audit_trail append-only · criptografia em repouso (pgcrypto) · TLS 1.3 (Caddy) ·
MFA obrigatório (Keycloak) · mascaramento em logs (DataMaskingMiddleware) ·
portabilidade (`/api/v1/plataforma/meus-dados/exportar`) · esquecimento (job de anonimização) ·
data residency (on-premise Nível 3) · CVE scan no CI (Trivy) · Secrets no Vault

---

## 17. TESTES DE SEGURANÇA OBRIGATÓRIOS (bloqueiam deploy)

1. **TenantLeakTest** — Tenant A não acessa dados do Tenant B
2. **SoftDeleteFilterTest** — entidades deletadas não aparecem em nenhuma query
3. **LedgerAppendOnlyTest** — lançamentos contábeis são imutáveis (trigger PG)
4. **AuditTrailTest** — toda ação sensível gera registro na `audit_trail`
5. **OutboxDeliveryTest** — Domain Events entregues mesmo com falha após SaveChanges
6. **PCIDataMaskingTest** — CPF/PAN nunca aparecem nos logs
7. **EntitlementGateTest** — módulo desabilitado retorna 403
8. **PerformanceSLOTest** — P95 leitura <200ms, escrita <500ms (bloqueia release)

---

## 18. SECDEVOPS — PIPELINE

```
pre-commit (gitleaks · dotnet-format)
→ CI① (build · unit tests · SAST Semgrep)
→ CI② (Testcontainers · 7 testes de segurança obrigatórios)
→ CI③ (Trivy CVE scan · SBOM CycloneDX · Grype)
→ CI④ (container scan · Checkov IaC)
→ STAGE (DAST OWASP ZAP · Nuclei)
→ aprovação manual
→ PROD (Vault dynamic secrets · Falco runtime · Grafana alerts)
```

---

## 19. CONVENÇÕES API REST

- Base URL: `https://api.epros.com.br/api/v1`
- Auth: `Authorization: Bearer {JWT}`
- Paginação: `?pagina=1&tamanhoPagina=20`
- Filtros: query string por campo
- Ações: `POST /{recurso}/{id}/{verbo}` (ex: `/baixar`, `/cancelar`, `/emitir`)
- Sync offline: `GET /{modulo}/sync/delta?since={ISO8601}`
- Idempotência: header `Idempotency-Key: {uuid}`
- Rate limit por tenant: 60/min (Micro) · 300/min (Essencial) · 1k/min (Avançado) · 5k/min (Enterprise)

---

*Gerado automaticamente a partir do histórico de sessão — Maio 2026*
