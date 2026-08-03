# GUIA DE USO POR FUNÇÃO — Fábrica de Software Epros

> **Para quem é este guia:** todo mundo do time. Ache o seu papel, veja **qual agente e qual
> skill usar, em que momento e como**. Sem decorar nada — é só seguir o passo a passo.
>
> **As 3 peças (decore só isto):**
> - **Agente** = *com quem você fala* (o perfil que você abre no chat).
> - **Skill** = *o conhecimento que ele consulta* (a fonte da verdade; você não precisa abrir, o agente puxa).
> - **Prompt de partida** = *o que você cola* pra começar (arquivos em `prompts/`).
>
> Fonte da verdade do produto: [CLAUDE.md](../../CLAUDE.md) + [CONVENCAO_CODIGO.md](../../CONVENCAO_CODIGO.md) · A esteira: [PIPELINE.md](processo/PIPELINE.md) · Cursor: [CONFIGURAR-CURSOR.md](cursor/CONFIGURAR-CURSOR.md) · **Tutoriais:** [indice-tutoriais.md](../onboarding/indice-tutoriais.md)

---

## 1. Como usar um agente (o passo a passo que vale para TODOS)

Sempre a mesma receita, 5 passos:

1. **Abra um chat novo** (um agente por conversa — trocou de fase, chat novo).
2. **Execute o slash-command** da fase no Cursor (ex.: `/strategy`, `/dev`). O **Context** já está ativo via rules.
3. **Cole o prompt de partida** da fase (arquivo em `prompts/`) e **preencha os `{campos}`**. Placeholder vazio = resposta genérica.
4. **Anexe o artefato da fase anterior** (a US, a spec, o diff…). Anexe, não resuma de memória — o artefato É o contexto.
5. **Confira o gate** (o critério de pronto da fase) e siga o bloco **"→ Handoff"** no fim do prompt: ele diz qual é a próxima fase.

> ⚠️ **3 regras que nunca mudam:** (a) a **skill vence a memória do modelo** — se o agente disser algo que contraria uma skill, a skill está certa; (b) **um humano é dono de cada gate** — o agente recomenda, você decide; (c) **um agente por conversa**.

---

## 2. Mapa rápido — quem usa o quê

| Sua função | Seus agentes | Suas skills principais | Suas fases |
|---|---|---|---|
| **PO / Facilitador** | Strategy, Discovery, Requirements | S16, S17, S18 (+S04 p/ impacto fiscal) | 01 · 02 · 03 |
| **Tech Lead / Arquiteto** | Planning, Architect, Code Review (revisão final) | S20, S05, S03, S06, S07, S08, S28, S15 | 05 · 06 · gate do PR |
| **Dev Backend** | Dev, Code Review, (consulta Architect) | S02, S03, S06, S07, S08, S10 (+S09/S30/S04) | 06 · 07 |
| **Dev Frontend** | Dev, UX, Code Review | S11, S19, S15, S10 | 04 · 07 |
| **QA / SDET** | QA, (consulta Fiscal) | S21, S10, S04 | 08 |
| **Guardião de Domínio (Fiscal)** | Fiscal | S04, S25, S26 | transversal (obrig. 03 · 07 · 08) |
| **Suporte / Migração** | Support, Migration | S29 · S27 | fora da esteira |

*(Transversais que qualquer um aciona quando aplicável: **Security**, **Docs**, **Fiscal**, **Code Review**.)*

---

## 3. O manual, função por função

Cada ficha responde: **quem você é · quando entra · o que abre · como faz (passo a passo) · seu gate.**

---

### 👤 PO / Facilitador — *transformar demanda em User Story pronta*

**Quando você entra:** no começo de tudo (uma demanda nova) e sempre que o time precisa de requisito claro.

**Seu kit:**
| Fase | Agente | Cole este prompt | Skills que ele puxa |
|---|---|---|---|
| 01 Strategy | Strategy Agent | `prompts/fase-01-strategy.md` | S16 (business case) |
| 02 Discovery | Discovery Agent | `prompts/fase-02-discovery.md` | S17 (síntese) |
| 03 Requirements | Requirements Agent | `prompts/fase-03-requirements.md` | S18 (US + ACs) + S04 (impacto fiscal) |

**Como faz (passo a passo):**
1. Demanda nova? Abra **Strategy**, cole `fase-01`, descreva a demanda → sai um **Business Case com go/no-go**. Leve o go/no-go pra liderança decidir (gate humano).
2. Com o **go**: abra **Discovery**, cole `fase-02`, anexe as notas/entrevistas → sai o **Problem Statement**.
3. Abra **Requirements**, cole `fase-03`, anexe o Problem Statement → saem **User Stories com critérios Given/When/Then**.
4. Antes de passar pra frente, rode o check de **DoR** (está no próprio prompt): sem termo vago, com impacto fiscal e multi-tenancy respondidos.

**Seu gate (o que entrega pronto):** US com ACs testáveis + DoR ok. → entra na esteira (Planning).

> 💡 Melhoria pequena, sem discovery? Pule direto pra **Requirements** (`prompts/fluxos-curtos.md`).

---

### 🏗️ Tech Lead / Arquiteto — *garantir que o desenho respeita os padrões*

**Quando você entra:** ao planejar a sprint (05), ao desenhar algo novo/decisão técnica (06) e como **aprovador final do PR**.

**Seu kit:**
| Fase | Agente | Cole este prompt | Skills que ele puxa |
|---|---|---|---|
| 05 Planning | Planning Agent | `prompts/fase-05-planning.md` | S20 (breakdown, estimativa) |
| 06 Architect | Architect Agent | `prompts/fase-06-architect.md` | S05 (ADR), S03, S06, S07, S08, S28, S15 |
| Gate do PR | Code Review Agent | `/code-review` + link do PR | S23 |

**Como faz (passo a passo):**
1. **Planning:** abra **Planning**, cole `fase-05`, anexe as US aprovadas → sai o **breakdown em tasks estimadas**. Gate: total ≤ velocity (cabe na sprint) ou replaneje.
2. **Architect (só quando precisa):** decisão nova, spike ou desenho fora do padrão? Abra **Architect**, cole `fase-06` → sai **tech design + ADR** se houver decisão. **Task simples que segue padrão existente NÃO precisa desta fase** — o padrão já é a decisão.
3. **Aprovação de PR:** o Code Review Agent já rodou no PR — o relatório está publicado como comentário (o dev passou o link). Você faz a **revisão final humana**, focando lógica de negócio — o padrão já foi checado pela máquina. Para re-rodar: `/code-review` + link do PR.

**Seu gate:** tech design sem violação (tenancy, Outbox, hexagonal) · zero bloqueante 🔴 no PR antes do merge.

> 💡 Toda decisão nova que você tomar **vira ADR** (S05) e entra no CONTEXT.md. É o que impede rediscutir a mesma coisa.

---

### ⚙️ Dev Backend — *implementar a feature no padrão da casa*

**Quando você entra:** na fase 07, com a task já quebrada. É o seu dia a dia.

**Seu kit:**
| Momento | Agente | Cole este prompt | Skills que ele puxa |
|---|---|---|---|
| Implementar / refatorar / bug | Dev Agent | `prompts/fase-07-dev.md` | S02, S03, S06, S07, S08, S10 |
| Task especial | Dev Agent | idem (ganchos no prompt) | +S09 (sync) · +S30 (job) · +S04 (fiscal) |
| Antes do PR | Code Review Agent | `/code-review` + link do PR | S23 |
| Dúvida de desenho | Architect Agent | `prompts/fase-06-architect.md` → Prompt B | S03/S06/S07/S08 |

**Como faz (passo a passo):**
1. Pegue a task. Abra o perfil **Dev Agent** (o Context já está ativo).
2. Cole `fase-07-dev.md` **Prompt A**, preencha (task, submódulo, ACs). O agente consulta S02/S03/S06/S07/S08/S10 e entrega **código + testes juntos**, por arquivo.
3. Se a task for **frontend, sync offline ou job**, o próprio prompt tem os ganchos (S11 / S09 / S30). Se **tocar fiscal**, chame o **Fiscal Agent** pra validar (ver ficha Fiscal).
4. **Antes de abrir o PR:** execute `/code-review` e cole **apenas o link do PR** no GitHub. O agente busca o diff via `gh`, exibe o relatório no chat e **publica como comentário no PR**. Corrija os 🔴 (bloqueantes) e 🟡 (avisos).
5. Se tocar **auth / dados sensíveis**, rode também o **Security Agent**.
6. Abra o PR (CI roda os 8 testes) → Tech Lead faz a revisão final.

**Seu gate:** build verde + testes passando + auto-review sem 🔴.

> ⚠️ Nunca peça só "faça funcionar". Peça no padrão: "implemente conforme a spec, com testes, seguindo as convenções." O agente já sabe o que isso significa via skills.

---

### 🎨 Dev Frontend — *tela consistente com o design system*

**Quando você entra:** revisando o fluxo de telas (04) e implementando o frontend (07).

**Seu kit:**
| Momento | Agente | Cole este prompt | Skills que ele puxa |
|---|---|---|---|
| Fluxo de telas | UX Agent | `prompts/fase-04-ux.md` | S19 (padrões ERP), S11 |
| Implementar componente/página | Dev Agent | `prompts/fase-07-dev.md` (gancho frontend) | S11 (Nuxt 4), S15, S10 |
| Antes do PR | Code Review Agent | `/code-review` + link do PR | S23 |

**Como faz (passo a passo):**
1. Tela nova? Abra **UX**, cole `fase-04`, descreva a tela → sai o fluxo/estrutura de componentes contra o design system (azul/dourado), WCAG e confirmações fiscais.
2. Abra **Dev Agent**, cole `fase-07-dev.md` → o gancho de **frontend** aciona a **S11** (composable `useApi` com auth+tenant, Pinia, TypeScript estrito, tabelas densas).
3. **Antes do PR:** `/code-review` com o link do PR. O relatório é exibido no chat e publicado no PR. Checagens típicas: token nunca em localStorage, chamada via `useApi`, estado no Pinia, tratamento de erro no catch.

**Seu gate:** componente no padrão do design system + Code Review sem 🔴.

---

### 🧪 QA / SDET — *provar que funciona antes de ir pro ar*

**Quando você entra:** na fase 08, com o build de release. E toda vez que um bug P0 aparece.

**Seu kit:**
| Momento | Agente | Cole este prompt | Skills que ele puxa |
|---|---|---|---|
| Plano de teste / cobertura / bug→teste | QA Agent | `prompts/fase-08-qa.md` | S21 (edge cases), S10 (mecânica) |
| Cenário fiscal | consulta Fiscal Agent | `prompts/transversais.md` → Fiscal | S04, S25 |

**Como faz (passo a passo):**
1. Abra **QA Agent**, cole `fase-08-qa.md`, anexe os critérios de aceite → sai o **plano de testes priorizado por risco** + os **edge cases do catálogo** (S21) pra verificar (tenant sem config emitindo NF-e, produto sem NCM, certificado expirado…).
2. Rode os cenários **fiscais e de multi-tenancy** — são os que mais quebram. Dúvida fiscal? Chame o Fiscal Agent.
3. Bug de produção? Além de corrigir, use o QA Agent pra transformar em **teste de regressão** e **adicione o edge case ao catálogo (S21)** — isso é obrigatório e é o que faz a fábrica melhorar.

**Seu gate:** zero P0/P1 aberto · cenários fiscais e de tenancy verdes.

---

### 📊 Guardião de Domínio (Fiscal) — *o dono do conhecimento tributário*

**Quando você entra:** sempre que houver dúvida ou validação fiscal. É **obrigatório** nas specs (03), no código (07) e nos testes (08) de features fiscais.

**Seu kit:**
| Situação | Agente | Cole este prompt | Skills que ele puxa |
|---|---|---|---|
| Dúvida tributária | Fiscal Agent | `prompts/transversais.md` → Fiscal `[Dúvida]` | S04, S25 |
| Rejeição SEFAZ | Fiscal Agent | `prompts/transversais.md` → Fiscal `[Rejeição SEFAZ]` | S25 |
| SPED / obrigação acessória | Fiscal Agent | `prompts/transversais.md` → Fiscal `[SPED]` | S26 |

**Como faz (passo a passo):**
1. Alguém tem dúvida de CFOP/NCM/CST/ST? Abra **Fiscal Agent**, cole o bloco `[Dúvida]`, informe UF e regime do tenant → resposta com a referência e o impacto no Epros (campos, validações, eventos).
2. Rejeição da SEFAZ? Bloco `[Rejeição SEFAZ]` → o agente diagnostica pelo catálogo (config do tenant × dado do documento × SEFAZ).
3. Fechar SPED? Bloco `[SPED]` → layout dos registros, prazos e o de-para dado-do-Epros → registro.

**Seu gate:** nenhuma feature fiscal passa por spec/código/teste sem o seu ok.

---

### 🎫 Suporte / Migração — *operar e trazer aprendizado de volta*

**Suporte (fora da esteira):**
1. Ticket chegou? Abra **Support Agent**, cole `prompts/projetos.md` → triagem (reproduzir → classificar → identificar tenant → decidir: bug / config / dúvida), com skill **S29**.
2. Virou bug? Alimenta a esteira: entra como melhoria/hotfix (`fluxos-curtos.md`) no próximo ciclo.

**Migração de cliente legado (Bloco 7, fluxo próprio):**
1. Abra **Migration Agent**, cole o fluxo de migração de `prompts/projetos.md` → plano do cliente com skill **S27** (de-para long→Guid, ETL, conciliação de saldos).
2. Sequência: ensaio em staging → janela → convivência (novo ativo, legado read-only) → **gate: saldos batem** → corte → Support monitora 2 semanas.

---

## 4. Tabela mestre — a esteira inteira numa olhada

| # | Fase | Agente | Prompt | Skills | Gate (pronto quando…) | Dono do gate |
|---|---|---|---|---|---|---|
| 01 | Strategy | Strategy | `fase-01-strategy.md` | S16 | go aprovado + OKR vinculado | Liderança |
| 02 | Discovery | Discovery | `fase-02-discovery.md` | S17 | Problem Statement validado | PO |
| 03 | Requirements | Requirements | `fase-03-requirements.md` | S18 (+S04) | DoR: sem termo vago, fiscal+tenancy respondidos | PO |
| 04 | UX | UX | `fase-04-ux.md` | S19, S11 | aprovado p/ dev (consistência + WCAG) | UX/PO |
| 05 | Planning | Planning | `fase-05-planning.md` | S20 | cabe na sprint (≤ velocity) | Tech Lead |
| 06 | Architect | Architect | `fase-06-architect.md` | S05, S03, S06, S07, S08, S28, S15 | zero violação de padrão; spikes resolvidos | Tech Lead |
| 07 | Dev | Dev | `fase-07-dev.md` | S02, S03, S06, S07, S08, S10 (+S09/S11/S30/S04) | build + testes verdes | Dev |
| T | Code Review | Code Review | `transversais.md` | S23 | zero bloqueante 🔴 | Tech Lead |
| T | Security | Security | `transversais.md` | S14, S13, S03 | sem crítica/alta aberta | Security |
| 08 | QA | QA | `fase-08-qa.md` | S21, S10, S04 | zero P0/P1; fiscal+tenancy verdes | QA |
| 09 | Ops | Ops | `fase-09-ops.md` | S22, S12 | checklist go-live 100% + rollback testado | Ops/Tech Lead |
| P | Docs | Docs | `transversais.md` | S24, S05, S15 | changelog/wiki/OpenAPI atualizados | Docs |
| — | Fiscal | Fiscal | `transversais.md` | S04, S25, S26 | ok fiscal (obrig. 03·07·08) | Guardião |

---

## 5. Fluxos curtos (nem tudo percorre a esteira toda) — `prompts/fluxos-curtos.md`

- **Bug de produção (hotfix):** Support tria → Dev corrige → Code Review → Ops faz deploy com runbook → QA roda regressão → Docs changelog → **QA adiciona o edge case ao catálogo (S21)** *(obrigatório)*.
- **Melhoria pequena (sem discovery):** Requirements (US direto) → Planning → Dev → Code Review → QA → Ops.
- **Spike / dúvida técnica:** Architect (com as skills de engenharia) → vira ADR ou nota técnica → volta ao Planning.

---

## 6. Erros comuns (o que NÃO fazer)

| ❌ Não faça | ✅ Faça |
|---|---|
| Misturar duas fases no mesmo chat | Um agente por conversa; trocou de fase, chat novo |
| Resumir de memória o artefato anterior | Anexe o arquivo (US, spec, diff) |
| Deixar `{placeholders}` em branco | Preencha todos antes de enviar |
| Aceitar resposta que contraria uma skill | A skill vence; reporte o desvio |
| Abrir PR sem passar pelo Code Review Agent | Execute `/code-review` com o link do PR antes de abrir |
| Pedir código fiscal sem o Fiscal Agent | Feature fiscal SEMPRE valida com Fiscal |
| Deixar o agente "decidir" o gate | O humano dono do gate decide |

---

## 7. FAQ

**Não sei em que fase estou.** Abra o [PIPELINE.md](processo/PIPELINE.md) — ele é o mapa. Você está sempre entre dois gates.

**Preciso saber qual skill usar?** Não. Você escolhe o **agente** (pela sua fase) e cola o **prompt** — o agente puxa a skill certa sozinho. As colunas "Skills" aqui são só pra você entender de onde vem o conhecimento.

**Como ativo as rules no Cursor?** Siga [CONFIGURAR-CURSOR.md](cursor/CONFIGURAR-CURSOR.md). Agentes: [`agentes/`](agentes/).

**Comecei agora, por onde começo?** Trio de maior uso: **Context (rules) + `/dev` + `/code-review`**. O resto você adiciona conforme sua função pede.

---

*Fábrica EprosERP · Guia por função · se divergir, vencem [PIPELINE](processo/PIPELINE.md), [CLAUDE.md](../../CLAUDE.md) e [CONVENCAO_CODIGO.md](../../CONVENCAO_CODIGO.md).*
