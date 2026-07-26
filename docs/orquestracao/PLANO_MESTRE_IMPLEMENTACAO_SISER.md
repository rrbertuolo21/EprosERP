# PLANO MESTRE — Implementação Completa do Produto Siser (via Fábrica)

> Documento vivo do agente orquestrador. Gerado em 2026-07-22.
> Fontes: 4 mapeamentos (padrão EprosERP · método Fábrica · auditoria de specs · inventário/ordem de módulos).
> Regra de ouro: **só desenvolvemos no `EprosERP`. `Epros` (legado) é consulta.**

---

## 1. Situação consolidada (as-is)

| Dimensão | Estado |
|---|---|
| **Código-alvo** | `EprosERP` — .NET 8 hexagonal/CQRS (MediatR, EF Core 8 / PostgreSQL 16, Flunt, Quartz/Outbox, Redis). 14 módulos isolados, 257 entidades, 91 controllers, **453 testes verdes**, build limpo — núcleo **deploy-ready**. |
| **Cross-module** | Só via `Guid` FK + Lookup (`ExcludeFromMigrations`) e eventos Outbox. Nunca referência de projeto entre módulos de domínio. |
| **Especificações** | `Siser_Projetos_Docs/EspecificaçõesPlataforma/` — **organizadas** (dedup feito): 17 módulos + governança, 152 EF + 152 MC pareados. Padrão `EF_/MC_{nº}_{MÓDULO}_{SUBMÓDULO}_V{n}.md`. |
| **Inventário** | 17 macromódulos · **132 submódulos** · 7 ondas · grafo de dependências (Níveis 0→5). |
| **Método** | Fábrica: "agente fino / skill profunda". 16 agentes + 30 skills + pipeline de 9 fases com gates (DoR/DoD + **8 testes que bloqueiam deploy**). |

### Escopo do trabalho = o DELTA
- **Prontos (Onda 0):** 10 submódulos (APP-TEN núcleo, EST-PR Produtos, FIN-CP/CR, VEN-GP/PDV).
- **Em execução (Onda 1):** EST-SC e COM-GC (andamento); **PLT-DFE em "estrutura" = GARGALO FISCAL**.
- **Falta implementar:** ~122 submódulos.
- **Nota de reconciliação:** o status do `.xlsx` pode estar defasado vs. o código real (que avançou até F1–F9). Por isso a **Fase 0 (mapa-mestre)** confirma código a código antes de qualquer fan-out.

### Gargalos estruturais (atacar cedo, destravam muita coisa)
1. **PLT-DFE / Faturamento Fiscal Eletrônico** — Vendas e Compras dependem para autorizar documentos. Sair de "estrutura" → "funcional".
2. **CAD-PEM (Pessoa e Organização)** — Nível 0; quase tudo depende.
3. **APP-TEN-003 + Keycloak (Usuários/Papéis, RBAC real)** — pré-requisito de GRC/SoD e permissões.
4. **PLT-UPL (Upload e Migração)** — crítico para o cutover dos 20 clientes (long→Guid).
5. **PLT-WF (Workflow)** — aprovação por alçada (Compras depende).

---

## 2. Método de orquestração (Fábrica adaptada ao modo autônomo)

A Fábrica foi desenhada para humanos no Cursor ("um agente por conversa", colar artefatos). No modo orquestrador aplico as mesmas fases e gates, mas com estas **melhorias**:

1. **Passagem programática de artefatos** entre subagentes (sem copy-paste manual).
2. **Roteador de rota por tamanho+risco:** cada submódulo entra numa das rotas:
   - **Rota Completa** (esteira 9 fases) — submódulos com regra de negócio densa, impacto fiscal ou contábil.
   - **Rota Curta** (Requirements→Planning→Dev→Review→QA) — CRUD/cadastro que segue padrão existente.
   - **Rota Scaffold** (Dev direto a partir da EF, padrão de porte) — entidades simples / módulos em quarentena.
3. **Transversais concorrentes, não bloqueantes:** Fiscal e Security rodam como checagem paralela; só o **gate final** consolida.
4. **Gates mecânicos = CI automático** (build + 453→N testes + 8 testes de segurança + `has-pending-model-changes`); gates humanos só onde há julgamento (go/no-go, cutover).

### Regras anti-erro EMBUTIDAS NA ESTRUTURA (do histórico do projeto)
- **Mapa de propriedade único** (nome→dono→schema→base class) ANTES do fan-out. ← Fase 0.
- **Arquivos disjuntos por agente:** cada agente só escreve arquivos que só ele toca.
- **Nunca editar arquivos compartilhados:** EF mapping via `IEntityTypeConfiguration` por entidade / mapping inline no `ContextX` do próprio módulo (nunca cross); handlers via **MediatR assembly-scan** (nunca `Program.cs`).
- **Migrations CONGELADAS durante o fan-out** → único passe **serial** no fim de cada onda (a única coisa que não paraleliza).
- **Contratos cross-module** (Lookups/eventos Outbox) definidos ANTES dos consumidores.
- **Passe de reconciliação por grep** caçando erros silenciosos (duplicata cross-module, shadow FK, catálogo com tenant).
- **Fidelidade à EF campo a campo** (não inventar, não simplificar, não remover campo) — regra de ouro do `CONVENCAO_CODIGO.md`.

---

## 3. Estratégia de paralelização por camadas (bottom-up)

Regra do grafo: **um submódulo só entra quando as dependências de camada inferior têm contrato publicado.** Paraleliza-se DENTRO da camada (arquivos disjuntos); serializa-se ENTRE camadas nos pontos de contrato + no passe de migrations.

```
Nível 0  APP / CAD / PLT-core (multi-tenancy, Pessoa, Workflow, Outbox, RBAC)
Nível 1  EST-PR · VEN-GP/PDV · FIN-CP/CR/CGL · PLT-DFE          [núcleo transacional]
Nível 2  EST-SC/Compras · WMS · FIN-Tesouraria/Orçamento         [ciclo completo]
Nível 3  PRD · QLD · RH                                          [operacional estendido]
Nível 4  MAN · PRJ · GRC                                         [gestão & compliance]
Nível 5  ESG · CON (DMS) · IMO                                   [enterprise & verticais]
```

---

## 4. Fases do plano (execução)

| Fase | Conteúdo | Paralelismo | Gate de saída |
|---|---|---|---|
| **F0 — Mapa Mestre** (rodando) | Reconciliação spec × código real por módulo: por submódulo → status (DONE/PARCIAL/SCAFFOLD/AUSENTE) + entidades presentes vs exigidas pela EF + gaps + tier de esforço + contratos cross-module. | 8 agentes | Tabela mestre dos 132 + backlog priorizado |
| **F1 — Fundação & Gargalos** | PLT-DFE funcional · CAD-PEM/GEO/PRM na nova plataforma · APP-TEN-002/003/008 (RBAC) · PLT-WF · PLT-UPL · APP-CAT/GED. | fan-out por submódulo (arquivos disjuntos) + migrations serial no fim | build+testes verdes; contratos publicados |
| **F2 — Núcleo restante (Ondas 2–3)** | Fechar EST-SC/COM-GC · FIN-CGL/SF · EST-MVM/APE/INV · VEN-CRM · RPT-OPB · Offline/Impressão/Dashboard. | fan-out | idem |
| **F3 — Plano Essencial (Onda 4)** | PRD (BOM/Custos/Planej.) · QLD · RH (Folha/Ponto/SSO) · VEN-LDS/GSV/Garantias/Portais · FIN-AFX/CMG/PO · BI. | fan-out por módulo | idem |
| **F4 — Plano Avançado (Onda 5)** | WMS/TMS · MES/MRP · MAN (EAM) · PRJ (PPM) · GRC · Tesouraria/Consolidação · RH estratégico. | fan-out por módulo | idem |
| **F5 — Enterprise & Verticais (Onda 6)** | ESG · CON (DMS) · IMO · Comércio exterior/Câmbio · IA/IoT. | fan-out por módulo | idem |
| **F6 — Fechamento** | Passe de reconciliação global (grep erros silenciosos) · migrations consolidadas · cobertura · frontend não-refinado das telas faltantes · homologações (tarefa humana). | serial | DoD global |

**Nota de profundidade ("completo, sem refinamento"):** por submódulo entrego **backend completo** (entidade fiel à EF + Context/mapping + CQRS commands/queries/handlers + controller fino + testes xUnit) para os 122. **Frontend Nuxt** entra completo para os planos Micro/Essencial (telas de operação) e como scaffold funcional para o restante — refinamento visual fica para depois.

---

## 5. Governança de qualidade (gates)

- **DoR** antes de entrar: EF sem termo vago; impacto multi-tenancy e fiscal respondidos.
- **DoD** para sair: Code Review Agent rodado; CI verde (build + testes + **8 testes de segurança**: TenantLeak, SoftDeleteFilter, LedgerAppendOnly, AuditTrail, OutboxDelivery, PCIDataMasking, EntitlementGate, PerformanceSLO); cobertura ≥70% nos arquivos alterados; OpenAPI + changelog.
- **Fidelidade à EF** é gate que nunca relaxa.

---

## 6. Decisões abertas (defaults que sigo se você não redirecionar)

| # | Questão | Default que assumo |
|---|---|---|
| D1 | Profundidade: backend-first ou backend+front simultâneo em tudo? | **Backend completo para os 122**; frontend completo nos planos Micro/Essencial, scaffold no resto. |
| D2 | Transmissão fiscal real (NFS-e/CT-e/MDF-e) depende de homologação SEFAZ/credenciais. | Construo **código + fallback**; homologação vira tarefa humana agendada. |
| D3 | Auth: Keycloak real agora ou manter token MVP? | **Manter token MVP**; construir RBAC de domínio (APP-TEN-003) agora; integrar Keycloak depois. |
| D4 | Fiscal macro `V1` vs `MACRO_V2`. | Adoto **MACRO_V2** como autoritativa. |
| D5 | Verticais CON (DMS) / IMO (ADR-04 placeholder). | **Scaffold fino** conforme EF; quebra fina só com cliente real. |
| D6 | Ritmo de fan-out. | Até **~20 agentes** simultâneos (sua preferência), arquivos disjuntos. |

---

## 7. Task list (nível de fase — detalhada por submódulo após F0)

- [ ] **F0.1** Mapa-mestre: reconciliação dos 17 módulos (8 agentes) → `docs/orquestracao/mapa_mestre/*.md`
- [ ] **F0.2** Consolidar tabela dos 132 (status real + tier) → `MAPA_MESTRE_132.md`
- [ ] **F0.3** Publicar contratos cross-module da F1 (Lookups + eventos Outbox)
- [ ] **F1** Fan-out Fundação & Gargalos (PLT-DFE, CAD, RBAC, WF, UPL) + migrations serial
- [ ] **F2** Fan-out Núcleo restante (Ondas 2–3) + migrations serial
- [ ] **F3** Fan-out Plano Essencial (Onda 4) + migrations serial
- [ ] **F4** Fan-out Plano Avançado (Onda 5) + migrations serial
- [ ] **F5** Fan-out Enterprise & Verticais (Onda 6) + migrations serial
- [ ] **F6** Reconciliação global + fechamento + DoD

*A lista fina por submódulo (122 itens com dono/arquivos/contratos) é gerada ao fim da F0.*
