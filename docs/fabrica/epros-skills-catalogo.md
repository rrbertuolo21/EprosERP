# Epros Dev Framework — Catálogo de Skills v1.0

> **Objetivo deste documento**
> Definir o conjunto de Skills (módulos de conhecimento reutilizáveis e versionados) que os agentes do Epros Dev Framework devem carregar sob demanda, com escopo, conteúdo, gatilhos de acionamento e roadmap incremental de construção.

> ⛔ **Não é inventário de disco.** Este arquivo é **histórico de planejamento** (memória do raciocínio original).
> Inventário canônico neste repo: [`cursor/cursor-install/rules/`](cursor/cursor-install/rules/) (S01–S30 `.mdc`) + [`skills/`](skills/). Cursor: [CONFIGURAR-CURSOR.md](cursor/CONFIGURAR-CURSOR.md).

> ⚠️ **Documento de origem / planejamento.** Foi aqui que as 30 skills e a expansão de
> agentes foram *propostas*. Parte do conteúdo está no tempo verbal de proposta e reflete
> o estado inicial (13 agentes). **Decisões já fechadas** e canônicas hoje: **16 agentes**
> (os 13 + Fiscal/Migration/Support; DBA virou skill S28) e o inventário de módulos —
> canônicos em `CLAUDE.md` + `CONVENCAO_CODIGO.md`. Onde este catálogo divergir,
> **os canônicos vencem**. Use este arquivo como memória do raciocínio, não como estado atual.

---

## 1. Por que Skills, se os agentes já têm prompts?

Hoje o conhecimento do Epros está **duplicado e espalhado** pelos 13 prompts: regras de multi-tenancy aparecem no Context, Dev, Architect, Security, QA e Code Review Agent — cada um com uma redação diferente. Isso gera três problemas:

1. **Drift** — quando uma regra muda (ex: nova ADR), você precisa atualizar 6 prompts e algum sempre fica desatualizado.
2. **Contexto inchado** — a rule global (`00-epros-context.mdc`) carrega contexto em TODA interação, mesmo quando o dev só quer gerar um DTO.
3. **Conhecimento raso** — prompts têm limite prático de tamanho; não cabe ali o passo a passo completo de emissão de NF-e ou de uma migration segura.

**O modelo alvo:**

```
AGENTE  = persona + missão + formato de saída + quais skills carregar e quando
SKILL   = conhecimento profundo, procedimentos, templates, checklists, exemplos
```

O agente fica **enxuto** (quem sou, o que entrego) e a skill carrega o **como fazer**, em um único lugar, versionado, compartilhado entre agentes.

### Estrutura padrão de uma Skill

```
skills/
└── epros-multi-tenancy/
    ├── SKILL.md          # < 500 linhas: quando usar, regras, procedimento
    ├── templates/        # arquivos prontos para copiar (entidade, mapping, teste)
    ├── exemplos/         # código real anotado: certo vs errado
    ├── checklists/       # listas verificáveis (review, go-live, segurança)
    └── scripts/          # automações opcionais (validadores, geradores)
```

**Frontmatter obrigatório do SKILL.md:**

```yaml
---
name: epros-multi-tenancy
description: >
  Use quando a tarefa envolver TenantId, isolamento de dados, QueryFilter,
  RLS, criação de entidade nova, query cross-schema ou revisão de código
  que toca dados de negócio. Gatilhos: "tenant", "isolamento", "nova entidade",
  "EntidadeSaaSBase", "query", "migration".
version: 1.0.0
agentes: [dev, architect, security, code-review, qa]
revisao: trimestral
---
```

> **Adaptação ao Cursor:** skills vivem em **`.cursor/skills/Sxx-nome/SKILL.md`** (raiz e submódulos).
> Regras globais ou por glob em **`.cursor/rules/*.mdc`**. Slash-commands em **`.cursor/commands/`**.
> Se migrar para Claude Code no futuro, copie para `.claude/skills/` — o formato SKILL.md é compatível.

---

## 2. Avaliação dos agentes atuais — falta algum? _(resolvido)_

> ✅ **Esta análise já foi executada.** Os 13 agentes originais tinham 4 lacunas; a decisão
> fechada foi **criar 3 agentes** (Fiscal, Migration, Support) e tratar **DBA como skill (S28)** —
> totalizando os **16 agentes** de hoje (`.cursor/commands/`, ver `CONTEXT.md §14`). O texto abaixo
> fica como registro do raciocínio que levou a essa decisão.

Os 13 agentes originais cobriam bem o ciclo de desenvolvimento. Mas cruzando com o contexto do Epros (ERP fiscal brasileiro, 20 clientes em legado, sync offline, migração em blocos), havia **4 lacunas reais** — hoje endereçadas:

### 2.1 Fiscal Agent `[TRANSVERSAL]` — ⭐ mais crítico

O domínio fiscal é citado em **todos** os agentes (QA tem edge cases fiscais, Security trata certificado, Dev sinaliza impacto fiscal, UX exige confirmação fiscal) — mas **nenhum agente é dono do conhecimento tributário**. NF-e, NFC-e, CFOP, NCM, CST/CSOSN, Substituição Tributária, contingência, eventos de nota (cancelamento, CC-e, manifestação), SPED. Isso é o coração de um ERP brasileiro e a maior fonte de bugs P0.

- **Quem usa:** Dev, QA, Requirements, Architect, Support
- **Missão:** responder dúvidas tributárias, validar regras fiscais em specs e código, revisar XMLs, orientar homologação SEFAZ.

### 2.2 Data/Migration Agent — Fase transversal aos Blocos

Você tem **20 clientes no Epros.ERP legado** que precisarão migrar. Migração de dados de ERP é um projeto dentro do projeto: mapeamento de-para, ETL, validação de integridade, conciliação de saldos (financeiro e estoque), migração de XMLs fiscais históricos (guarda de 5 anos). Nenhum agente atual cobre isso.

- **Quem usa:** Dev Backend, Tech Lead, Suporte/Implantação
- **Missão:** planejar e validar migrações por cliente/módulo, gerar scripts de-para, checklists de conciliação pós-migração.

### 2.3 Support/Incident Agent `[TRANSVERSAL]`

O Ops Agent cobre release e monitoramento, mas com clientes em produção surge a rotina de **triagem de tickets**: reproduzir o problema, classificar severidade, identificar tenant afetado, decidir se é bug/configuração/dúvida, redigir resposta ao cliente. É um fluxo diferente de análise de log.

- **Quem usa:** Suporte N1/N2, Dev de plantão
- **Missão:** triagem estruturada, roteiro de diagnóstico por módulo, comunicação com cliente, escalonamento.

### 2.4 DBA Agent — opcional (pode ser só skill)

Performance PostgreSQL em contexto multi-tenant (12 schemas × N tenants) merece profundidade: EXPLAIN ANALYZE, estratégia de índices, particionamento, vacuum/autovacuum, connection pooling. **Recomendação:** comece como skill (`postgres-performance`) usada pelo Architect e Ops; promova a agente só se o volume justificar.

### Veredito

| Agente novo | Prioridade | Justificativa |
|---|---|---|
| Fiscal Agent | 🔴 Alta — criar já | Domínio central, risco P0, conhecimento hoje sem dono |
| Data/Migration Agent | 🟠 Média — criar antes do 1º cliente migrar | 20 clientes legados esperando |
| Support/Incident Agent | 🟡 Média-baixa — criar quando novos módulos entrarem em produção | Rotina distinta do Ops |
| DBA Agent | ⚪ Não criar ainda | Cobrir via skill `postgres-performance` |

---

## 3. Catálogo de Skills

Organizado em 4 camadas. Cada skill lista: **para quê**, **agentes que usam**, **conteúdo do SKILL.md** e **recursos anexos** (templates/exemplos/checklists).

---

### CAMADA 0 — Fundação (transversais, usadas por quase todos)

Estas skills substituem o "peso morto" que hoje está replicado nos prompts. São as primeiras a construir.

---

#### S01 · `epros-contexto-dominio`

**Para quê:** fonte única da verdade sobre o projeto. O Context Agent vira skill; a rule global (`00-epros-context.mdc`) fica enxuta e aponta para cá.

**Agentes:** todos.

**Conteúdo do SKILL.md:**
- Identidade do projeto (nome, repositórios, blocos, status)
- 17 macromódulos / 132 submódulos + 12 schemas PostgreSQL por macrodomínio (ver `CONTEXT.md §11`)
- Status dos módulos (tabela viva — atualizar a cada bloco concluído)
- Catálogo de Domain Events (produtor → consumidores)
- Glossário de negócio (tenant, bloco, DFe, GED, PDV, sync...)

**Recursos:**
- `exemplos/mapa-modulos.md` — diagrama de dependências entre módulos
- `checklists/atualizacao-contexto.md` — o que atualizar ao fechar um bloco

**Manutenção:** atualizar ao final de cada bloco. Skill mais volátil do catálogo.

---

#### S02 · `epros-convencoes-codigo`

**Para quê:** as regras SEMPRE/NUNCA com exemplos executáveis de certo vs errado — hoje só existem como lista abstrata.

**Agentes:** Dev, Code Review, Architect, QA.

**Conteúdo do SKILL.md:**
- Regras SEMPRE/NUNCA (herdar de `EntidadeSaaSBase`, `DateTime.UtcNow`, Guid, soft delete, precision 18,2, snake_case, índices compostos...)
- Anatomia de `EntidadeSaaSBase`, `CommandResult`, `OutboxMessage` — com o porquê de cada campo
- Convenções de nomenclatura (classes, handlers, migrations, arquivos Nuxt)
- Limites objetivos (200 linhas/arquivo, 20 linhas/função, complexidade ciclomática)

**Recursos:**
- `templates/entidade.cs`, `templates/command-handler.cs`, `templates/query-handler.cs`
- `exemplos/violacoes-comuns.md` — top 10 erros com correção lado a lado
- `scripts/validar-convencoes.sh` — grep automatizado por `DateTime.Now`, `context.Remove`, `long Id` etc.

---

#### S03 · `epros-multi-tenancy`

**Para quê:** a regra mais crítica do sistema ("violação de tenant é a falha mais grave possível") merece a skill mais profunda.

**Agentes:** Dev, Architect, Security, Code Review, QA, Ops.

**Conteúdo do SKILL.md:**
- Os 3 níveis (Shared/Dedicated/Private) e implicações de cada um no código
- Como o QueryFilter automático funciona (ContextBase + reflection) e **quando ele NÃO protege** (SQL raw, Dapper eventual, jobs Quartz sem escopo, background services)
- RLS como segunda barreira: como configurar e testar
- `ITenantProvider` — ciclo de vida, armadilhas com singleton/static, uso em jobs
- Padrões para operações legítimas cross-tenant (relatórios da plataforma, billing)

**Recursos:**
- `checklists/review-multi-tenancy.md` — o que verificar em todo PR
- `exemplos/vazamentos-reais.md` — cenários de vazamento anotados (job sem tenant, cache com chave sem tenant, IN clause montada errada)
- `templates/teste-isolamento.cs` — teste xUnit padrão que cria 2 tenants e prova o isolamento

---

#### S04 · `fiscal-brasil-fundamentos`

**Para quê:** o conhecimento tributário-base que todo agente precisa para não sugerir bobagem fiscal.

**Agentes:** Fiscal (dono), Dev, QA, Requirements, Architect, Support.

**Conteúdo do SKILL.md:**
- NF-e (55) vs NFC-e (65): quando cada uma, ciclo de vida (emissão → autorização → eventos)
- CFOP: estrutura, entrada vs saída, tabela dos mais usados no Epros
- NCM, CEST, CST/CSOSN — o que são e onde entram no XML
- Substituição Tributária: conceito e campos obrigatórios
- Certificado digital A1/A3: por CNPJ, nunca cross-tenant, renovação, revogação
- Contingência offline (Motor Fiscal Edge) — fluxo e reconciliação
- Prazos legais: guarda de XML 5 anos (Decreto-Lei 486/1969), prazos de cancelamento
- Ambientes: homologação vs produção SEFAZ, dados de teste válidos

**Recursos:**
- `exemplos/xml-nfe-anotado.xml` — XML de homologação com comentário campo a campo
- `checklists/pre-emissao.md` — o que validar antes de emitir (tenant configurado, NCM, certificado válido, série/numeração)
- `exemplos/tabela-cfop-epros.md` — CFOPs por operação do sistema

---

#### S05 · `epros-adrs`

**Para quê:** as 15 ADRs fechadas + o processo de criar/revisar/reverter uma decisão.

**Agentes:** Architect (dono), Docs, Strategy, Tech Lead.

**Conteúdo do SKILL.md:**
- Índice das ADRs 001–015 com resumo de uma linha e link
- Template de ADR (já existe no Architect Agent — mover para cá)
- Processo: quem propõe, quem aprova, prazo de revisão, o que dispara revisão
- Regras especiais (ADR-014: lib NF-e exige +90 dias de testes para troca)

**Recursos:**
- `templates/adr.md`
- `exemplos/adr-boa-vs-ruim.md` — uma ADR bem escrita vs uma vaga, anotadas

---

### CAMADA 1 — Engenharia (profundidade técnica para Dev, Architect, QA, Ops)

---

#### S06 · `cqrs-mediatr-epros`

**Agentes:** Dev, Code Review, Architect.

**Conteúdo:** anatomia completa de uma feature no padrão Epros — Command → Validator (FluentValidation) → Handler → Entidade (Flunt) → CommandResult → Controller fino. Quando usar Query vs Command, pipeline behaviors (logging, validação, transação), erros comuns (lógica no controller, handler gordo, validação duplicada).

**Recursos:** `templates/feature-completa/` (pasta com os 6 arquivos de uma feature exemplo, prontos para copiar e renomear), `exemplos/handler-antes-depois.md`.

---

#### S07 · `ef-core-postgres-multitenant`

**Agentes:** Dev, Architect, Code Review, Ops.

**Conteúdo:** mappings com schema por macrodomínio, snake_case, precision; **migrations seguras** (aditivas vs destrutivas, expand-contract para renomear coluna com clientes em produção, como testar reversibilidade); índices compostos `(tenant_id, campo)` — quando e como; diagnóstico de N+1 (e por que multiplica por tenant); `AsNoTracking`, split queries, paginação eficiente.

**Recursos:** `checklists/migration-segura.md`, `exemplos/migration-expand-contract.md`, `scripts/detectar-n1.md` (como usar o log do EF para achar N+1).

---

#### S08 · `outbox-domain-events`

**Agentes:** Dev, Architect, QA, Code Review.

**Conteúdo:** passo a passo para criar um Domain Event novo (definição → gravação na Outbox na mesma transação → processador → handlers consumidores); idempotência no consumo; retries e dead-letter (campo `Tentativas`/`Erro`); versionamento de payload de evento; como testar com Testcontainers; catálogo vivo de eventos (referencia S01).

**Recursos:** `templates/domain-event.cs`, `templates/event-handler.cs`, `checklists/novo-evento.md`, `exemplos/evento-vendafaturada-fluxo.md` (fluxo completo real anotado).

---

#### S09 · `sync-offline`

**Agentes:** Dev (backend e mobile), Architect, QA.

**Conteúdo:** modelo SyncId + SyncVersion (ADR-011) explicado a fundo; endpoint delta (`/sync/delta?since=`) — contrato, paginação, tombstones para deletados; detecção e resolução de conflitos (estratégias por tipo de entidade); fluxo do PDV offline com contingência NFC-e; o que NUNCA sincronizar (secrets, certificados).

**Recursos:** `exemplos/cenarios-conflito.md` (matriz de conflitos e resolução esperada), `templates/teste-sync.cs`, `checklists/entidade-syncavel.md`.

---

#### S10 · `testes-epros`

**Agentes:** Dev, QA, Code Review.

**Conteúdo:** pirâmide de testes do Epros (unit com Flunt/handlers, integration com Testcontainers, E2E); como subir PostgreSQL/Keycloak/MinIO em Testcontainers; **builders de teste** (TenantBuilder, ProdutoBuilder, NotaFiscalBuilder) para dados realistas; dados fiscais de homologação (CNPJs de teste, NCMs válidos, certificado de homologação); convenções de nomenclatura de teste; o que é cobertura obrigatória (lógica de negócio, eventos, validações).

**Recursos:** `templates/builders/`, `templates/integration-test-base.cs`, `exemplos/dados-homologacao.md`.

---

#### S11 · `nuxt4-frontend-epros`

**Agentes:** Dev Frontend, UX, Code Review.

**Conteúdo:** estrutura de pastas Nuxt 4 do projeto; composables padrão (useApi com auth/tenant headers, useLoading, useNotify); Pinia — quando usar store vs composable; tipagem TypeScript (proibição de `any`); tratamento de loading/error obrigatório; componentes do design system (azul/dourado) e como consumi-los; padrões ERP (tabela densa com filtros, formulário multi-etapa fiscal, atalhos de teclado para operador de caixa).

**Recursos:** `templates/composable-api.ts`, `templates/page-crud.vue`, `exemplos/tabela-erp-padrao.vue`.

---

#### S12 · `observabilidade-epros`

**Agentes:** Ops (dono), Dev, Architect.

**Conteúdo:** log estruturado Serilog — campos obrigatórios (TenantId, UserId, CorrelationId), o que NUNCA logar (CPF, senha, payload de certificado — conecta com S14); traces OpenTelemetry — como instrumentar handler/job novo; métricas que importam por módulo (latência p95, taxa de erro por tenant, fila da Outbox, jobs Quartz atrasados); como navegar Grafana/Loki/Tempo para investigar um erro de produção.

**Recursos:** `checklists/instrumentacao-nova-feature.md`, `exemplos/queries-loki-uteis.md`, `templates/dashboard-modulo.json`.

---

#### S13 · `keycloak-rbac`

**Agentes:** Security (dono), Dev, Architect.

**Conteúdo:** arquitetura de realms/clients do Epros; claim `tenantId` no JWT — como é injetada e validada; perfis e permissões (RBAC) — como criar permissão nova; Segregation of Duties (evento `ViolacaoSoDDetectada`); fluxo de desligamento (`ColaboradorDesligado` → revogação); troubleshooting comum (token expirado, claim ausente, CORS).

**Recursos:** `checklists/endpoint-novo-auth.md`, `exemplos/politicas-rbac.md`.

---

#### S14 · `seguranca-lgpd`

**Agentes:** Security (dono), Dev, Code Review, Docs.

**Conteúdo:** OWASP Top 10 aplicado ao stack Epros (com exemplos em C#/EF Core, não genéricos); catálogo de dados pessoais do sistema (onde mora CPF, endereço, telefone, dados bancários) e regras de mascaramento (DataMaskingMiddleware); LGPD operacional — bases legais por módulo, retenção, anonimização vs pseudonimização, direito de exclusão vs guarda fiscal de 5 anos (conflito real!); gestão de secrets com Vault (dynamic secrets, rotação).

**Recursos:** `checklists/security-review.md` (o checklist atual do agente, expandido), `exemplos/mascaramento-campos.md`, `exemplos/vulnerabilidades-corrigidas.md` (casos reais do projeto, anonimizados).

---

#### S15 · `api-conventions-epros`

**Agentes:** Dev, Architect, Docs, Code Review.

**Conteúdo:** convenções completas (base URL, versionamento, paginação, verbos de ação `/{id}/{verbo}`, Idempotency-Key — como implementar server-side com Valkey); rate limits por plano; **política de breaking change** (o que é breaking, processo de depreciação, headers de sunset); geração de OpenAPI.

**Recursos:** `checklists/endpoint-novo.md`, `exemplos/deprecacao-endpoint.md`, `templates/controller-versionado.cs`.

---

### CAMADA 2 — Fases de produto (uma skill por agente de fase)

Estas skills absorvem os formatos de saída e princípios que hoje estão nos prompts, e os aprofundam com exemplos reais do Epros. O prompt do agente encolhe; a skill cresce.

---

#### S16 · `business-case` → Strategy Agent
Template de Business Case preenchido com exemplo real (ex: módulo Compras); framework de scoring (RICE ou similar adaptado); árvore de OKRs vigente da empresa (atualizar por trimestre); matriz de conflito de roadmap.
**Recursos:** `templates/business-case.md`, `exemplos/business-case-modulo-compras.md`, `exemplos/okrs-vigentes.md`.

#### S17 · `discovery-sintese` → Discovery Agent
Método de análise de entrevistas (codificação de dores, frequência × severidade); biblioteca de personas já consolidadas do Epros (contador, gestor financeiro, operador de caixa, comprador...) para não recriar do zero; formato JTBD; template de Problem Statement; roteiros de entrevista por módulo.
**Recursos:** `exemplos/personas-epros.md`, `templates/roteiro-entrevista.md`, `exemplos/sintese-real-anotada.md`.

#### S18 · `user-stories-criterios` → Requirements Agent
Formato de US + Given/When/Then (mover do prompt); **glossário de ambiguidade** ("rápido" → p95 < Xms; "fácil" → máx N cliques); catálogo de NFRs padrão do Epros por tipo de feature (toda tela de listagem herda NFR de paginação; toda feature fiscal herda NFR de auditoria); regras de rastreabilidade (US → problema → persona).
**Recursos:** `templates/user-story.md`, `exemplos/us-fiscal-completa.md`, `checklists/dor-definition-of-ready.md`.

#### S19 · `ux-erp-patterns` → UX Agent
Design system Epros documentado (tokens, componentes, azul/dourado); padrões de interação ERP (tabela densa, formulário fiscal multi-etapa, atalhos de teclado, viewport 13"); checklist WCAG AA aplicado; catálogo de fluxos aprovados (referência de consistência); regras de confirmação para ações fiscais irreversíveis.
**Recursos:** `checklists/review-acessibilidade.md`, `exemplos/fluxos-aprovados/`, `exemplos/antipadroes-ux-erp.md`.

#### S20 · `planning-breakdown` → Planning Agent
Método de quebra técnica (por camada: migration → domínio → handler → endpoint → front → testes); calibração Fibonacci com **exemplos históricos do Epros** ("task de 3 pts = criar CRUD simples com testes"); velocity por time (dado vivo); política de spikes; heurística de complexidade oculta fiscal/DFe.
**Recursos:** `examples/breakdown-real-block5-pdv.md`, `examples/estimate-reference.md`, `checklists/jira-ep-issue-types.md`, `checklists/publish-jira-ep.md`, `checklists/enrich-jira-ep.md`, `templates/task-description.md`.

#### S21 · `test-plan-epros` → QA Agent
Formato de plano de testes (mover do prompt); **catálogo vivo de edge cases** por módulo (os 8 atuais + crescer a cada bug de produção — todo P0 vira edge case catalogado); matriz de priorização por risco; estratégia por tipo (unit/integration/E2E) referenciando S10.
**Recursos:** `exemplos/catalogo-edge-cases.md` (o ativo mais valioso do QA — alimentar continuamente), `templates/plano-testes.md`.

#### S22 · `release-runbooks` → Ops Agent
Checklist go-live (mover do prompt); estratégias de deploy por risco (canary obrigatório para fiscal/cobrança); **biblioteca de runbooks** (Outbox travada, certificado expirado, tenant sem acesso, migration falhou no meio, SEFAZ fora); template de post-mortem blameless; matriz de severidade de incidente.
**Recursos:** `runbooks/` (um arquivo por cenário — começar com 5, crescer a cada incidente), `templates/post-mortem.md`, `checklists/go-live.md`.

#### S23 · `code-review-epros` → Code Review Agent
Checklist e severidades (mover do prompt); **exemplos anotados de reviews reais** (PR com violação de tenant e o comentário ideal; PR sem testes; N+1); integração com S02/S03/S14 (a skill de review referencia as outras em vez de duplicar regras).
**Recursos:** `exemplos/reviews-anotados/`, `checklists/review-rapido.md` (versão 5-minutos para PRs pequenos).

#### S24 · `docs-changelog` → Docs Agent
Keep a Changelog (mover do prompt); padrão XML doc C#; template de página wiki por tipo (feature, integração, configuração); política de documentação por audiência (time vs cliente); geração de OpenAPI a partir de endpoint novo.
**Recursos:** `templates/changelog.md`, `templates/wiki-feature.md`, `exemplos/xml-doc-padrao.cs`.

---

### CAMADA 3 — Skills para os novos agentes e temas avançados

---

#### S25 · `nfe-emissao-hercules` → Fiscal Agent + Dev
Uso da lib Hercules.NET.NFe.NFCe v2026.3.15.14 (a lib da ADR-014): configuração por tenant, montagem do XML, assinatura com certificado, envio/retorno SEFAZ, tratamento de rejeições (catálogo de códigos de rejeição mais comuns e correção), eventos (cancelamento, CC-e, inutilização), contingência e reconciliação, armazenamento no MinIO.
**Recursos:** `exemplos/rejeicoes-sefaz-catalogo.md`, `checklists/homologacao-uf.md`, `exemplos/fluxo-contingencia.md`.

#### S26 · `sped-obrigacoes-acessorias` → Fiscal Agent
SPED Fiscal/Contribuições, layouts, prazos, geração dos blocos a partir dos dados do Epros. Construir quando o módulo Fiscal avançar.
**Recursos:** `exemplos/mapeamento-blocos-sped.md`.

#### S27 · `migracao-legado` → Data/Migration Agent
Mapeamento de-para Epros.ERP legado → novo modelo (por módulo); estratégia de ETL; validação de integridade (contagens, somas de controle); **conciliação de saldos** financeiro e estoque pós-migração; migração de XMLs históricos para o MinIO; estratégia de convivência (cliente meio migrado?); rollback de migração.
**Recursos:** `templates/plano-migracao-cliente.md`, `checklists/conciliacao-pos-migracao.md`, `exemplos/depara-financeiro.md`.

#### S28 · `postgres-performance` → Architect/Ops (o "DBA como skill")
EXPLAIN ANALYZE na prática; estratégia de índices multi-tenant; particionamento por tenant_id (quando vale); autovacuum tuning; connection pooling (PgBouncer/Npgsql); queries lentas recorrentes do Epros e correções.
**Recursos:** `exemplos/queries-diagnostico.sql`, `checklists/revisao-performance-modulo.md`.

#### S29 · `incident-triage` → Support/Incident Agent
Roteiro de triagem (reproduzir → classificar → identificar tenant/módulo → bug vs config vs dúvida); matriz de severidade voltada a cliente; roteiros de diagnóstico por módulo (financeiro, PDV, fiscal); templates de comunicação com cliente por severidade; critérios de escalonamento para dev.
**Recursos:** `templates/resposta-cliente/`, `runbooks/diagnostico-por-modulo/`.

#### S30 · `quartz-jobs-epros` → Dev/Ops
Jobs no Quartz.NET com escopo de tenant correto (armadilha nº 1 de vazamento), idempotência, monitoramento de jobs atrasados, retry policies.
**Recursos:** `templates/job-multitenant.cs`, `checklists/job-novo.md`.

---

## 4. Matriz Agente × Skills

| Agente | Skills principais | Skills de apoio |
|---|---|---|
| Context (→ vira skill S01) | — | — |
| Strategy | S16 | S01, S05 |
| Discovery | S17 | S01 |
| Requirements | S18 | S01, S04 |
| UX | S19 | S01, S11 |
| Planning | S20 | S01, S04 |
| Architect | S05, S28 | S01, S02, S03, S06, S07, S08, S15 |
| Dev | S02, S06, S07, S08, S10, S11 | S03, S04, S09, S13, S15, S30 |
| QA | S21, S10 | S03, S04, S09 |
| Ops | S22, S12 | S03, S28, S30 |
| Security | S14, S13 | S03, S04 |
| Docs | S24 | S05, S15 |
| Code Review | S23 | S02, S03, S07, S10, S14 |
| **Fiscal (novo)** | S04, S25, S26 | S01 |
| **Migration (novo)** | S27 | S01, S04, S07 |
| **Support (novo)** | S29 | S01, S12, S22 |

---

## 5. Roadmap incremental de construção

### Onda 1 — Fundação (semanas 1–3) · elimina a duplicação atual
| # | Skill | Esforço | Nota |
|---|---|---|---|
| S01 | epros-contexto-dominio | Baixo | 80% já existe no Context Agent — é reorganizar |
| S02 | epros-convencoes-codigo | Médio | O ganho está nos exemplos certo/errado |
| S03 | epros-multi-tenancy | Médio | Prioridade máxima em profundidade |
| S05 | epros-adrs | Baixo | Consolidar as 15 ADRs |

**Entrega da onda:** reescrever os prompts de Dev, Code Review e Architect para versão enxuta que referencia as skills.

### Onda 2 — Engenharia core (semanas 3–6) · acelera o Bloco 6
| # | Skill | Esforço |
|---|---|---|
| S06 | cqrs-mediatr-epros | Médio |
| S07 | ef-core-postgres-multitenant | Médio |
| S08 | outbox-domain-events | Médio |
| S10 | testes-epros | Alto (builders dão trabalho, mas pagam para sempre) |
| S04 | fiscal-brasil-fundamentos | Alto |

**Entrega da onda:** criar o **Fiscal Agent** junto com a S04.

### Onda 3 — Fases de produto (semanas 6–10) · uma por semana, em paralelo ao dev
S16 → S18 → S21 → S23 → S22 → S20 → S17 → S19 → S24 (ordem por valor: comece pelas fases que o time mais usa hoje).

### Onda 4 — Especialização (a partir da semana 10, conforme demanda)
| Gatilho | Skills |
|---|---|
| Módulo Fiscal/DFe avançar | S25, depois S26 |
| Primeira migração de cliente legado agendada | S27 + criar Migration Agent |
| Novos módulos em produção gerando tickets | S29 + criar Support Agent |
| Sync/PDV mobile (Blocos 9–10) | S09, S11 aprofundada |
| Problemas de performance recorrentes | S28 |
| Sempre | S12, S13, S14, S15, S30 conforme dor |

---

## 6. Regras de manutenção do catálogo

1. **Toda skill tem dono** (um papel, não uma pessoa) e data de revisão no frontmatter.
2. **Todo bug P0 de produção vira conteúdo**: edge case no S21, runbook no S22 ou exemplo de violação no S02/S03. É assim que o catálogo composta valor.
3. **Regra do não-duplique**: se duas skills precisam da mesma informação, uma referencia a outra. Multi-tenancy mora na S03; as demais apontam para lá.
4. **SKILL.md curto, recursos profundos**: se o SKILL.md passar de ~500 linhas, mova detalhe para `exemplos/` e `checklists/`.
5. **Versione junto com o código**: skills vivem em `.cursor/skills/` (e rules em `.cursor/rules/`), passam por PR e review como qualquer artefato.

---

*Epros Dev Framework — Catálogo de Skills v1.0 · Revisão sugerida: ao final de cada bloco*
