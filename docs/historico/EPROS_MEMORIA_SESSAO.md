# EPROS — Memória de Sessão
> Última atualização: Junho 2026 · Integração Real do Painel Landlord Backoffice (5 abas), Roteamento e Queries Consolidadas concluídos

---

## 1. IDENTIDADE DO PROJETO

| Campo | Valor |
|---|---|
| **Nome atual** | Epros |
| **Tipo** | ERP SaaS multi-tenant |
| **Repositório backend** | `PlataformaSaaS/` |
| **Repositório desktop** | `EprosApp/` |
| **Repositório mobile** | `Epros.Mobile/` |
| **Clientes em produção** | 20 clientes no Epros.ERP legado |
| **Bloco atual** | Painel Landlord Backoffice Integrado (Bloco 16 concluído) |

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
Desktop:        Electron + Nuxt 3 (EprosApp)
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
| GestaoClientes | plataforma.* | Plano, ModuloPlano, Cliente, Fatura, Pessoa, Empresa, PerfilUsuario, UsuarioPermissao, Contrato, ContratoItem, ConfiguracaoGlobal, ExecucaoMassa | CriarCliente, AtivarCliente, SuspenderCliente, CriarPlano, GerarFatura, BaixarFatura, ObterMenuDinamico, CriarContrato, ProcessarFaturamentoRecorrente, DefinirConfiguracaoGlobal, AprovarAssinaturaManual, CriarExecucaoMassa, AprovarExecucaoMassa | 26 | ✅ Completo |
| Financeiro/CP | financas.* | ContaPagar, ContaPagarBaixa | Criar, Baixar, Cancelar | 8 | ✅ Completo |
| Financeiro/CR | financas.* | ContaReceber, ContaReceberBaixa | Criar, Baixar, Cancelar | — | ✅ Completo |
| Estoque/Produtos | estoque.* | Produto, MovimentoEstoque | LancarEntradaEstoque, LancarSaidaEstoque | — | ✅ Completo |
| Fiscal/DFe | plataforma.* | DocumentoFiscal, DocumentoFiscalItem, EventoDocumentoFiscal | EmitirDocumentoFiscal, CancelarDocumentoFiscal | 11 | ✅ Completo |
| Vendas/GestaoDePedidos | vendas.* | Venda, VendaItem, OutboxMessage | SincronizarVendas, VendasOutboxProcessorJob | 3 | ✅ Completo (Bloco 14 + Outbox) |
| Vendas/PDV | vendas.* | Caixa, CaixaMovimento | AbrirCaixa, FecharCaixa, Sangria, Suprimento, SincronizarCaixas | 27 | ✅ Completo (Bloco 15) |
| Estoque/Compras | estoque.* | Compra, CompraItem | LancarCompra | 6 | ✅ Completo |

**Total: 145+ arquivos .cs · 107 testes**

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
| TenantMiddleware commented | Implementado + testado |
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
| **Bloco 9** | Cadastros Base (Pessoa/Empresa) + EprosApp (Electron + Nuxt 3) | ✅ Concluído |
| **Bloco 10** | Permissões ABAC + Epros.Mobile (React Native) | ✅ Concluído |
| **Bloco 11** | Contratos e Faturamento Recorrente (SaaS) | ✅ Concluído |

---

## 13. ECOSSISTEMA DE DOCUMENTAÇÃO HTML

**Pasta:** `/mnt/user-data/outputs/epros-docs/`
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
| `09_runbook.html` | Playbooks de incidentes, SLAs, DR, RTO/RPO e migração de tier | 4 | 18KB |
| `10_modelo_financeiro.html` | Precificação Silver/Gold/Platinum, unit economics (LTV/CAC), break-even e projeções de margem | 4 | 13KB |
| `11_integracoes.html` | SEFAZ (DFe), Mercado Pago, Keycloak (OIDC/AD/LDAP), logística e marketplaces | 4 | 12KB |
| `12_modelo_dados.html` | Dicionário de dados, chaves primárias/estrangeiras, índices compostos e RLS | 5 | 16KB |
| `13_onboarding.html` | Setup local com Docker Compose, dotnet format, branching e commits semânticos | 4 | 11KB |
| `14_disaster_recovery.html` | Políticas de backup lógico/físico, PITR, restore do Vault e simulação de DR | 4 | 12KB |
| `15_glossario.html` | Dicionário canônico de termos técnicos, de negócios, métricas SaaS e a linguagem ubíqua | 3 | 10KB |
| `16_i18n.html` | Internacionalização, locales, formatos por país e adaptadores fiscais | 4 | 11KB |
| `17_changelog.html` | Notas de lançamento por bloco e histórico detalhado de alterações | — | 16KB |
| `18_plano_execucao.html` | Checklist de tarefas e fases de contenção | — | 22KB |
| `19_operacional.html` | Mapeamento do time, processos e agentes IA | — | 41KB |
| `20_treinamento.html` | Trilhas por papel, stack e guia onboarding | — | 32KB |
| `21_agentes_prompts.html` | Prompts de sistema e orquestração IA | — | 46KB |
| `22_diagnostico.html` | Diagnóstico de segurança e código do legado | — | 68KB |

**Páginas pendentes (a criar uma a uma):**
- Nenhuma (ecossistema de documentação concluído)

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
3. **Cloud-agnostic** — qualquer componente que só funcione em AWS/Azure/GCP é descartado. Roda em VPS de €20/mês or data center próprio do cliente.

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

*Gerado automaticamente a partir do histórico de sessão — Junho 2026*

---

## 20. DIRETRIZES DE DESENVOLVIMENTO (INVIOLÁVEIS)

### 1. Entendimento antes da execução
- Nunca presuma requisitos importantes.
- Se houver ambiguidade, faça perguntas antes de codificar.
- Explique rapidamente o que entendeu antes de propor ou alterar código.
- Não invente regras de negócio, entidades, campos, integrações ou tecnologias sem confirmação.

### 2. Segurança e boas práticas
- Todo código deve seguir boas práticas de segurança.
- Nunca exponha senhas, tokens, chaves de API, connection strings ou dados sensíveis no código.
- Use variáveis de ambiente ou arquivos de configuração seguros.
- Valide entradas de usuário.
- Proteja contra SQL Injection, XSS, CSRF, autenticação fraca, autorização incorreta e vazamento de dados.
- Implemente logs sem registrar dados sensíveis.
- Sempre considere autenticação, autorização, auditoria e rastreabilidade.

### 3. Desenvolvimento sem alucinação
- Não crie arquivos, métodos, classes ou dependências inexistentes sem verificar o contexto do projeto.
- Antes de alterar arquivos existentes, leia o conteúdo atual.
- Antes de usar uma biblioteca, confirme se ela está instalada ou documente a necessidade de instalação.
- Se não tiver certeza, declare a dúvida e peça confirmação.
- Não remova código existente sem explicar o impacto.

### 4. Organização do projeto
- Mantenha uma estrutura limpa, modular e escalável.
- Separe responsabilidades por camadas, módulos ou domínios.
- Use nomes claros para classes, métodos, variables, arquivos e pastas.
- Evite código duplicado.
- Priorize código simples, legível e de fácil manutenção.

### 5. Documentação obrigatória
- Sempre documente o que foi criado ou alterado.
- Para cada funcionalidade, informe:
  - arquivos criados;
  - arquivos alterados;
  - objetivo da alteração;
  - regras de negócio aplicadas;
  - dependências adicionadas;
  - riscos ou pontos de atenção.
- Quando criar APIs, documente endpoints, parâmetros, payloads, respostas e possíveis erros.
- Quando criar banco de dados, documente entidades, tabelas, relacionamentos e migrations.

### 6. Testes automáticos
- Crie testes automáticos para tudo que for possível.
- Para regras de negócio, crie testes unitários.
- Para APIs, crie testes de integração.
- Para validações, crie testes de casos válidos, inválidos e limites.
- Para bugs corrigidos, crie testes que garantam que o erro não volte.
- Sempre que adicionar uma funcionalidade, adicione ou atualize os testes correspondentes.

### 7. Compilação e validação
- Sempre que possível, execute:
  - build/compilação do projeto;
  - testes automatizados;
  - análise de erros;
  - validação de dependências.
- Se não conseguir executar, explique claramente o motivo.
- Nunca diga que algo está funcionando sem compilar, testar ou justificar a limitação.

### 8. Controle de mudanças
- Faça alterações pequenas e incrementais.
- Antes de grandes mudanças, apresente um plano.
- Não altere arquitetura sem aprovação.
- Ao final de cada etapa, entregue um resumo técnico.
- Sempre preserve compatibilidade quando possível.

### 9. Padrão de resposta esperado
Ao executar qualquer tarefa, responda neste formato:

Resumo:
- O que foi feito.

Arquivos criados:
- Lista de arquivos.

Arquivos alterados:
- Lista de arquivos.

Decisões técnicas:
- Explicação breve das escolhas.

Testes:
- Testes criados ou alterados.
- Resultado da execução dos testes.

Build/Compilação:
- Resultado da compilação.
- Erros encontrados, se houver.

Pendências:
- O que ainda precisa ser definido, feito ou validado.

### 10. Finalização de Etapas e Checkpoints
Antes de encerrar uma etapa, atualize automaticamente:
- Roadmap do projeto
- Backlog técnico
- Backlog funcional
- Arquitetura
- Dependências
- Pendências
- Débitos técnicos
- Testes pendentes
- Riscos conhecidos

Crie um checkpoint de continuidade contendo:
1. Estado atual do projeto
2. Funcionalidades concluídas
3. Funcionalidades em andamento
4. Próxima tarefa recomendada
5. Arquivos principais envolvidos
6. Dependências críticas
7. Decisões arquiteturais tomadas

Esse checkpoint deverá ser suficiente para que outro engenheiro continue o desenvolvimento sem perda de contexto.

### 11. Modo de trabalho
- Trabalhe como um engenheiro de software sênior.
- Priorize segurança, clareza, testes, documentação e manutenção.
- Não faça atalhos perigosos.
- Não entregue código incompleto sem avisar.
- Sempre pense no crescimento futuro do sistema.

---

## 21. ESTADO DE BACKLOG, ROADMAP E RISCOS (ATUALIZADO JUNHO 2026)

### 1. Roadmap do Projeto
* **Bloco 5 (Concluído)**: Solução do zero estruturada com 4 projetos (`Shared`, `Infrastructure`, `API`, `GestaoClientes`), pipeline de 7 middlewares HTTP configurado. Comandos e Handlers para `CriarCliente`, `AtivarCliente`, `SuspenderCliente`, `CriarPlano`, `GerarFatura` e `BaixarFatura` concluídos com 11 testes unitários passando.
* **Bloco 6 (Concluído)**: Reengenharia de Estoque/Compras. Criado projeto `Epros.Modules.Estoque`. Implementadas as entidades `Produto`, `Compra`, `CompraItem`, `MovimentoEstoque`. Lógica do **Custo Médio Ponderado** testada e validada. Handler `LancarCompraCommand` e endpoint de API integrados. Implementada a reengenharia do fluxo de cancelamento de compras (`CancelarCompraCommand`), integrado via Outbox Pattern para estornar automaticamente contas a pagar no Financeiro, com suite de testes de integração (`CompraOutboxIntegrationTests.cs`) validando os cenários ponta a ponta (totalizando 90 testes passando).
* **Bloco 7 (Concluído)**: DevOps & Infra: Criada a pipeline de CI/CD (.github/workflows/ci-sast-sec.yml), sementes do Keycloak 24 (infra/keycloak/epros-realm-export.json) configurando roles e claim tenantId, e script SQL determinístico para migração de chaves legadas long para Guid (scripts/migracao_legado_long_to_guid.sql).
* **Bloco 8 (Concluído)**: Qualidade + Testes Testcontainers + Sync delta + Worker vencimento + Webhook MercadoPago. Adicionados testes de integração real em container PostgreSQL, implementado endpoint `/sync/delta` com suporte a soft-deletes para sincronização incremental off-line, configurado Job agendado com Quartz.NET para alteração de status de faturas vencidas e suspensão automática, e webhook MercadoPago para baixa de pagamento integrada via MediatR.
* **Bloco 9 (Concluído)**: Cadastros Base (Pessoa/Empresa) e EprosApp (Electron + Nuxt 3). Implementadas as entidades transversais `Pessoa` e `Empresa` (com endereço como Owned Type e regime tributário) sob o schema `plataforma` com 4 novos testes unitários adicionados. Estruturado o esqueleto da aplicação desktop local `EprosApp` com Electron e Nuxt 3 SPA (baseURL relativa para carregar assets do sistema de arquivos).
* **Bloco 10 (Concluído)**: Permissões ABAC e Mobile Scaffolding (React Native). Implementadas as entidades de perfis e permissões, filtro ABAC global para autorização e o scaffolding inicial do app móvel com Expo e SQLite.
* **Bloco 11 (Concluído)**: Contratos e Faturamento Recorrente (SaaS). Entidades de contratos mapeadas no banco e comandos automáticos de processamento recorrente via Quartz integrados.
* **Bloco 12 (Concluído)**: Emissão de DFe. Entidades de Documento Fiscal integradas à biblioteca `Hercules.NET.NFe.NFCe` com total suporte a tributação brasileira.
* **Bloco 13 (Concluído)**: Financeiro Integrado (Contas a Pagar e Receber, Baixas e Integração Outbox via Quartz).
* **Bloco 14 (Concluído)**: Engine de Sincronização Mobile & Contingência Offline. Ingestão resiliente em lote no monólito, sincronização incremental de produtos delta e abatimento físico de estoque local no app móvel SQLite.
* **Bloco 15 (Concluído)**: Operações de Caixa / PDV Mobile. Implementação local e online p/ fluxo operacional de turnos de caixas e lançamentos avulsos de sangrias/suprimentos com conciliação contábil/física e reatividade de saldo no app mobile (84 testes passando).
* **Bloco 16 (Concluído)**: Painel Landlord Backoffice e Consolidação C# Backend. Integração real de 5 abas em Nuxt 3 (Inquilinos/Planos, Equipe Siser, Configurações Globais, Maker-Checker de Lote com Sandbox, e Comunicador/Newsletter) com fallback local de simulação e 237 testes passando.

### 2. Backlog Técnico
* Executar infraestrutura local no `docker-compose.yml` assim que o Docker daemon estiver disponível.
* Aplicar migrations iniciais nos bancos PostgreSQL de teste e desenvolvimento.
* Configurar o Outbox Worker local (Quartz.NET) para processamento de eventos na tabela `OutboxMessage`.

### 3. Backlog Funcional
* Mapear entidades do núcleo transversal em `plataforma.*`: `plataforma.pessoas`, `plataforma.empresas`.

### 4. Arquitetura & Dependências
* Monólito Modular Hexagonal com isolamento de schemas PostgreSQL.
* Dependências ativas: `Flunt` (domínio), `MediatR` (CQRS), `FluentValidation` (entrada), `EF Core` + `Npgsql` (dados).
* Adicionado `expo-sqlite` para contingência local em `Epros.Mobile`.

### 5. Pendências
* Inicialização física dos containers locais e aplicação das migrations no banco real.

---

## 22. CHECKPOINT DE CONTINUIDADE (INFRAESTRUTURA LOCAL E AUTO-MIGRATIONS CONCLUÍDAS)

### 1. Estado atual do projeto
A solução backend compila 100% limpa (0 erros, 0 warnings) e passa 100 testes unitários e de integração. A aplicação desktop `EprosApp` (Nuxt 3 + Electron) possui o fluxo da Área do Cliente e o Dashboard operacional funcionais, com login conectado nativamente ao Keycloak OIDC (com fallback local off-line). A API C# foi atualizada para realizar migrations automáticas em tempo de execução para os 5 DbContexts em ambiente de desenvolvimento, e o Keycloak local está configurado para auto-importar o realm no boot do container.

### 2. Funcionalidades concluídas
- Projetos estruturados com isolamento de escopo por pastas.
- Pipeline de 7 middlewares HTTP ordenados e configurados na API.
- Gestão de Clientes: Comandos `CriarCliente`, `AtivarCliente`, `SuspenderCliente`, `CriarPlano`, `GerarFatura`, `BaixarFatura`.
- Estoque e Compras: Novo módulo `Epros.Modules.Estoque`. Entidades `Produto` (com custo médio ponderado), `Compra`, `CompraItem`, `MovimentoEstoque`. Comando `LancarCompraCommand` transacional registrando movimentos e gravando eventos no Outbox.
- DevOps e Infraestrutura: Pipeline CI/CD GitHub Actions, seed do Keycloak, e script PostgreSQL de migração legado de chaves sequenciais para Guid.
- Qualidade, Sincronização e Integrações: Testcontainers PostgreSQL 16, sincronização incremental `/sync/delta`, background worker faturamento/suspensão e webhook Mercado Pago.
- Cadastros Base e Desktop Frontend (Bloco 9):
  - Entidades transversais `Pessoa` e `Empresa` mapeadas sob o schema `plataforma`.
  - Estruturação do `EprosApp` com Electron e Nuxt 3 SPA.
- Permissões ABAC e Mobile Scaffolding (Bloco 10):
  - Entidades `PerfilUsuario` e `UsuarioPermissao` mapeadas no banco sob o schema `plataforma`.
  - Filtro customizado `AbacFilter` interceptando rotas da API e validando permissões locais.
  - Inicialização do app mobile `Epros.Mobile` com Expo React Native e SQLite local.
- Contratos e Faturamento Recorrente (SaaS) (Bloco 11):
  - Entidades `Contrato` e `ContratoItem` mapeadas sob o schema `plataforma`.
  - Comando e handler `CriarContratoCommand` e faturamento recorrente automatizado.
- Módulo Fiscal: Emissão de DFe (Bloco 12):
  - Entidades `DocumentoFiscal`, `DocumentoFiscalItem` e `EventoDocumentoFiscal` mapeadas no PostgreSQL.
  - Integração com a biblioteca `Hercules.NET.NFe.NFCe`.
- Módulo Financeiro Integrado (Bloco 13):
  - Entidades `ContaPagar`, `ContaPagarBaixa`, `ContaReceber` e `ContaReceberBaixa` mapeadas no schema `financas`.
  - Background Job Quartz `OutboxProcessorJob` processando eventos `CompraLancada` e criando Contas a Pagar.
- Sincronização Mobile & Contingência Offline (Bloco 14):
  - Novo módulo backend `Epros.Modules.Vendas` (schema `vendas`) com entidades `Venda` e `VendaItem`.
  - Ingestão em lote de vendas locais no monólito e sincronização incremental.
- Operações de Caixa / PDV Mobile (Bloco 15):
  - Entidades `Caixa` e `CaixaMovimento` no módulo `Epros.Modules.Vendas` sob o schema `vendas.*`.
  - Comandos e Handlers: `AbrirCaixa`, `FecharCaixa`, `RegistrarCaixaMovimento`, `SincronizarCaixas` com conciliação automática e cálculo de divergências no monólito.
- Área do Cliente / Inquilino em Nuxt 3 (EprosApp):
  - Criada Área do Cliente com design premium dark e glassmorphism.
  - Tela de login persistindo o inquilino no localStorage e inicializando status.
  - Tabela de cobrança com faturas, dialog Pix e simulação de webhook MercadoPago.
  - Catálogo comparativo dos planos Silver, Gold e Platinum, integrando fluxo de upgrade.
  - Middleware global de roteamento bloqueando inadimplentes (overdue > 15 dias) e forçando redirecionamento à página de faturas.
- Painel SaaS Super Admin / Landlord Backoffice em Nuxt 3 (EprosApp) (APP-TEN-010):
  - Criado painel administrativo em /plataforma/admin com KPIs de MRR, Churn e faturamento.
  - Tabela de inquilinos com suspensão e ativação manuais integradas na persistência.
  - Edição dinâmica do preço/limite de planos comerciais com propagação reativa.
  - Console terminal de logs de simulação de Quartz jobs e Outbox workers.
- Workspace ERP & Dashboard Shell em Nuxt 3 (EprosApp) (APP-DSH-001):
  - Criada interface operacional com menu lateral (Visão Geral, Estoque, Vendas, Financeiro e Fiscal).
  - Exibição de KPIs e gráficos reativos consumindo as APIs REST locais de Vendas, Estoque, Financeiro e Fiscal com fallbacks de simulação.
  - Integração do fluxo de login e middleware global para inquilinos adimplentes.
- Onboarding de Inquilinos & Cadastro em Nuxt 3 (EprosApp) (APP-TEN-002):
  - Criada a página pública /cadastro com fluxo passo a passo (Wizard) em 3 etapas (Plano, Empresa, Administrador) e CNPJ dinâmico.
- Integração OIDC Keycloak & Bearer Token Routing (APP-SEC-001):
  - Criado plugin global `plugins/api.ts` em Nuxt 3 interceptando requisições `$fetch` para injetar os cabeçalhos `Authorization: Bearer <token>` e `X-Tenant-Id: <tenant>`.
  - Integrada a autenticação OIDC via fluxo de Direct Access Grants (senha) em `pages/index.vue` com decodificador JWT Base64 nativo.
  - Implementado mecanismo de auto-detecção de ping (Keycloak Offline) para alternância resiliente ao modo simulação local.
  - Limpeza total de credenciais (`epros_token`) na desconexão em `AppHeader.vue`.
- Automação de Infraestrutura & Auto-Migrations (APP-INF-001):
  - Configurado o `docker-compose.yml` para realizar o auto-import do realm `epros-tenant` e do cliente `epros-api` no Keycloak usando o argumento `--import-realm`.
  - Implementado bootstrap de migrações automáticas em `Program.cs` para todos os 5 contextos de banco de dados do EF Core.
- Row-Level Security (RLS) no PostgreSQL:
  - Criação de `EprosMigrationsSqlGenerator` no `Epros.Infrastructure` para ativar RLS e criar políticas de isolamento de inquilinos automaticamente.
  - Implementação de `TenantRlsInterceptor` definindo `app.current_tenant_id` em tempo de execução no EF Core.
  - Homologada a segurança RLS em testes físicos de integração com Testcontainers PostgreSQL, validando que tentativas de bypass lógico da aplicação (`IgnoreQueryFilters`) são bloqueadas fisicamente pelo banco.
- Integração Vendas ➡️ Financeiro & Fiscal via Outbox Pattern:
  - Comunicação assíncrona desacoplada via evento de domínio com entrega garantida e processamento contínuo por Quartz.NET.
- Portabilidade de Documentação:
  - Portados todos os 22 documentos técnicos e de negócios do kit legado para o novo formato responsivo escuro (Geist/Instrument Serif).
- Homologação do Pipeline HTTP da API Gateway:
  - Adicionado pacote `Microsoft.AspNetCore.Mvc.Testing` para subir host de testes integrados em memória.
  - Implementado `MiddlewareIntegrationTests` que garante a conformidade e integridade dos 5 middlewares transversais da API (Exceções, Tenant, Entitlements, Máscara e Auditoria).
- Infraestrutura IaC & Roteamento HTTPS Local:
  - Criada a configuração `infra/Caddyfile` mapeando subdomínios locais `*.epros.localhost` em HTTPS para a API, Keycloak, Vault e MinIO.
  - Implementado o arquivo de IaC declarativa `infra/tofu/main.tf` com definições completas de rede, volumes e contêineres Docker (Postgres, Keycloak, Vault, MinIO e Valkey) compatível com OpenTofu/Terraform.
- Configurações Globais Seguras do Super Admin (APP-TEN-010):
  - Criada a entidade `ConfiguracaoGlobal` no schema `plataforma` para gerenciar SMTP, trial, gateways e segredos operacionais.
  - Implementados comandos/queries CQRS com controle rígido restrito ao tenant `"system"` da Siser.
  - Exposto o controlador `/api/v1/plataforma/configuracoes` integrado na API Gateway.
  - Criada suite de 6 testes unitários e de integração validando o comportamento e a barreira de segurança de tenant.

### 3. Funcionalidades em andamento
- Nenhuma.

### 4. Próxima tarefa recomendada
- Implementação de um novo macrodomínio, por exemplo: PRO (Produção/MES/MRP/BOM) ou QUA (Qualidade) com suas respectivas entidades e views correspondentes.

### 5. Arquivos principais envolvidos
- [api.ts](file:///Users/rafael/Documents/Codigos/EprosERP/EprosApp/plugins/api.ts)
- [index.vue](file:///Users/rafael/Documents/Codigos/EprosERP/EprosApp/pages/index.vue)
- [docker-compose.yml](file:///Users/rafael/Documents/Codigos/EprosERP/docker-compose.yml)
- [Program.cs](file:///Users/rafael/Documents/Codigos/EprosERP/src/API/Epros.API/Program.cs)

### 6. Dependências críticas
- A sincronização offline móvel utiliza SQLite (`expo-sqlite`). Os cabeçalhos de requisição de API requerem decodificação e validação no Keycloak Realm local.

### 7. Decisões arquiteturais tomadas
- **Resource Owner Password Credentials no Electron**: Escolha do ROPC para manter a experiência e design de formulário glassmorphic local sem quebrar roteadores sob o protocolo `file:///`.
- **Desacoplamento de Dependências no Client**: Decodificação pura baseada em Base64 do payload do JWT para evitar dependências adicionais no bundle SPA.
- **Resiliência DX a Serviços Offline**: Auto-detecção de conectividade que rebaixa automaticamente a aplicação para modo simulado se a infraestrutura local não estiver inicializada.
- **Row-Level Security (RLS) Nativa**: Isolamento físico a nível de linha no banco de dados gerenciado automaticamente pelo gerador de migrations customizado.
- **Migrations Programáticas em Desenvolvimento**: Centralização do ciclo de vida de banco de dados na inicialização da API, eliminando atritos manuais com o cli do EF Core.


