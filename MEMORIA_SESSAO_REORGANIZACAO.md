# Memória de Sessão — Reorganização EprosERP

> **Criada:** 2026-08-01 · Leitura completa do repo para preparar reorganização.  
> **Atualizada:** 2026-08-01 — `_fabrica/` migrado para `docs/fabrica/`.  
> **Uso:** carregar este arquivo no início de qualquer tarefa de reorganização de pastas/docs/código.  
> **Não substitui:** `CONVENCAO_CODIGO.md` (canônico de código) nem `CLAUDE.md` (disciplinas).

---

## 0. Objetivo desta memória

Capturar **o que o projeto é hoje** (estrutura, docs, estilo, padrões reais) para reorganizar **sem quebrar** o monólito modular, o deploy achatado e as convenções já fechadas.

---

## 1. Identidade e stack (fixada)

| Campo | Valor |
|---|---|
| Produto | ERP SaaS multi-tenant |
| Forma | **Monólito modular** .NET 8 |
| Arch por módulo | Clean Arch: Domain / Application / Infrastructure / Migrations |
| CQRS | MediatR (+ 2 estilos válidos — ver §5) |
| Banco | PostgreSQL + EF Core 8, **schema por módulo**, RLS |
| Validação domínio | Flunt (`Notifiable` / `Contract`) |
| AuthZ | ABAC `[AbacAuthorize]` — módulos sobem **desabilitados** |
| Integração inter-módulo | Outbox + Domain Events (sem DbContext cruzado) |
| Front | Nuxt 3 + TypeScript (`Epros.App`) — IO só via `useApi` / `useApiList` |
| Mobile | React Native (`Epros.Mobile`, submódulo git) |
| IaC / deploy | `infra/tofu`, `docker-compose.{local,prod}.yml` |

**Fonte canônica de código:** `CONVENCAO_CODIGO.md` (prevalece sobre memórias antigas e `PADRAO_PORTE_LEGADO.md`).

---

## 2. Estrutura de pastas (estado atual — deploy-ready achatado)

```
EprosERP/                          ← raiz = solução pronta p/ publish
├── src/
│   ├── API/Epros.API/             Controllers finos (~190), Program.cs, middlewares
│   ├── Infrastructure/Epros.Infrastructure/
│   ├── Shared/Epros.Shared/
│   ├── External/                  Epros.ERP.DfeCalculos, Epros.ERP.Shared
│   └── Modules/Epros.Modules.<X>/ 15 módulos (ver §3)
├── Epros.App/                     Front Nuxt (pages/erp|plataforma|area-cliente)
├── Epros.Mobile/                  Submódulo
├── tests/Epros.Tests/
├── scripts/
├── infra/                         keycloak, tofu
├── docs/
│   ├── fabrica/                   agentes, processo, skills, cursor/rules .mdc
│   ├── onboarding/                trilha humana
│   ├── migracao/, orquestracao/, processos/
├── pasta_old/                     arquivo histórico — NÃO é runtime
├── docker-compose*.yml
├── Epros.sln
└── [~24 .md na raiz]              onboarding + planos + memórias (ruído — ver §8)
```

**Decisão estrutural (HISTORICO Bloco 10):** estrutura **achatada** na raiz.  
**2026-08-01:** pasta `_fabrica/` da raiz **migrada** para `docs/fabrica/`.

---

## 3. Módulos backend ↔ schemas PostgreSQL

| Módulo | Schema EF | Notas |
|---|---|---|
| GestaoClientes | `plataforma` | Pessoas, Empresas, RBAC/menu tenant |
| Estoque | `estoque` | |
| Fiscal | `plataforma` | Mesmo schema que GestaoClientes |
| RH | `rh` | |
| Aplicativo | `aplicativo` | Landlord / config global |
| Financeiro | `financas` | Nome schema ≠ módulo |
| Vendas | `vendas` | Inclui CRM/GSV |
| Producao | `producao` | |
| DMS | `concessionarias` | |
| Projetos | `projetos` | |
| GRC | `grc` | |
| ESG | `esg` | |
| Manutencao | `manutencao` | |
| Qualidade | `qualidade` | |
| Imobiliaria | `imobiliaria` | |

Layout interno: `Domain/` · `Application/` · `Infrastructure/` · `Migrations/` (só Modo Consolidação).

---

## 4. Front (Epros.App)

| Área | Path |
|---|---|
| ERP tenant | `pages/erp/<domínio>/` |
| Landlord | `pages/plataforma/` |
| Portal | `pages/area-cliente/` |

IO: **somente** `useApi` / `useApiList`. UX: rule `S19-ux-erp-patterns.mdc`. Fan-out: 1 agente por pasta disjunta.

---

## 5–6. Código e pipeline

Ver `CONVENCAO_CODIGO.md` e `CLAUDE.md`. Pipeline conceitual: Auth → Exceção → Tenant → Módulo → DataMasking → Audit → Controllers (ABAC).

---

## 7. Documentação — hierarquia

### Canônicos

| Doc | Papel |
|---|---|
| `COMECE-POR-AQUI.md` / `COMECE-AQUI.md` | Onboarding humano |
| `CLAUDE.md` | Cérebro da IA |
| `CONVENCAO_CODIGO.md` | Convenção de código |
| `HISTORICO-DESENVOLVIMENTO-IA.md` | Diário |
| `CONSOLIDACAO-GAPS.md` | Roadmap de gaps |
| **`docs/fabrica/**`** | Agentes, processo, skills, rules `.mdc`, guias, Jira |
| `docs/onboarding/` | Trilha longa |
| `docs/migracao/`, `docs/orquestracao/` | Porte / mapa mestre |
| `docs/processos/` | Endpoints por ambiente |

### Cursor rules

`docs/fabrica/cursor/cursor-install/rules/` — S01–S30 + `00-context.mdc`.  
Instalar: ver `docs/fabrica/cursor/CONFIGURAR-CURSOR.md`.

---

## 8. Preferências

- Paralelismo máximo (fan-out por pasta disjunta)
- Autonomia: executar e relatar
- Commit/push só sob pedido
- “Verde” do agente = entrada; prova = ambiente vivo
- Comunicação em português

---

## 9. Checklist pós-migração `_fabrica` → `docs/fabrica`

- [x] Pastas movidas (`agentes`, `processo`, `skills`, `cursor`)
- [x] Referências em `CLAUDE.md`, `COMECE-*`, `docs/README`
- [x] `CONFIGURAR-CURSOR.md` com path `docs/fabrica/...`
- [ ] Reinstalar rules locais: `cp docs/fabrica/cursor/cursor-install/rules/*.mdc .cursor/rules/`
- [ ] Atualizar links soltos em tutoriais longos (Slack/Jira) conforme uso

---

*Fim da memória. Próximo passo de reorganização: usuário define escopo (só docs feito; raiz `.md` históricos ainda pendente).*
