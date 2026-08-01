---
title: "Fluxo de desenvolvimento — branches, ciclo e processo"
confluence_id: "142737410"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/142737410/Fluxo+de+desenvolvimento+branches+ciclo+e+processo"
last_updated: "2026-07-20"
---

> [!NOTE]
> **O que você vai aprender:** como uma task percorre o Git e o Jira — branches, PR, homologação, deploy e hotfix — e o que cada papel faz em cada transição.

Você já viu **quem faz o quê** no [artigo 07](07-squads-cerimonias.md). Este artigo fecha a trilha base com o **fluxo operacional**: nomenclatura de branches, proteções no GitHub, ciclo semanal de validação, status no Jira e responsabilidades do time.

Em caso de dúvida sobre nomenclatura, responsabilidades ou ciclo de entrega, consulte aqui primeiro.

> **EprosERP:** o fluxo de branches/Jira abaixo veio do monorepo com `epros-back` + `epros-front`. Aqui o trabalho é no clone único **EprosERP** (`src/` + `Epros.App/`). Onde o texto citar dois repos, aplique no mesmo PR ou PRs sequenciais neste repositório.

---

## Branches

### Branches longas (permanentes)

| Branch | Função | Proteção |
| --- | --- | --- |
| `main` | Código em produção | PR obrigatório + block force push + restrição |
| `homolog` | Validação de negócio — deploy automático para o homolog | Block force push |
| `develop` | Fila de seleção — só entra via PR aprovado em code review, deploy automático para dev | PR obrigatório + block force push + branch atualizada obrigatória |

### Branches de trabalho (temporárias)

| Prefixo | Uso | Nasce de |
| --- | --- | --- |
| `feature/EP-123-descricao` | Nova funcionalidade | `develop` |
| `bugfix/EP-123-descricao` | Correção de bug no fluxo normal | `develop` ou `homolog` |
| `hotfix/EP-123-descricao` | Correção crítica em produção | `main` |
| `refactor/EP-123-descricao` | Refatoração | `develop` |
| `test/EP-123-descricao` | Criação ou modificação de testes | `develop` |
| `chore/EP-123-descricao` | Atualizações de dependências, config ou CI | `develop` |
| `sandbox/EP-123-descricao` | Execução de teste ou prova de conceito | `develop` |
| `cycle/YYYY-MM-DD-homolog` | Snapshot semanal pós-reunião: reverts + versão do release | `homolog` |
| `cycle/YYYY-MM-DD-develop` | Snapshot semanal: reverts + sync hotfix + versão do próximo ciclo | `develop` |
| `merge/YYYY-MM-DD-develop` | PR de integração da cycle develop → `develop` longa | `cycle/…-develop` |
| `merge/YYYY-MM-DD-homolog` | PR de integração da cycle develop → `homolog` (abre validação) | `cycle/…-develop` |

**Regra:** o número da task do Jira é **obrigatório** no nome da branch de task (`feature/`, `bugfix/`, etc.). O GitHub linka automaticamente à issue. Branches `cycle/` e `merge/` usam a **data da reunião** (`YYYY-MM-DD`), não o id da task — detalhe operacional na [Rotina de segunda — Tech Lead](tech-lead/rotina-segunda-feira.md).

Slug da descrição: ASCII, kebab-case, sem acentos — alinhado ao padrão dos `AGENTS.md` dos repositórios.

### Ciclo de vida (temporárias)

Nos repositórios de código (**epros-back**, **epros-front**), o GitHub está configurado com **Automatically delete head branches**: ao **mergear o PR**, a branch **head** some no **remoto**. As longas (`main`, `homolog`, `develop`) não são apagadas — o histórico (commits e merge commits) permanece nelas.

* **Task** (`feature/`, `bugfix/`, `hotfix/`, etc.): após o merge, não conte com a mesma branch no remoto. Retomar correção ou nova entrega → **criar branch nova** a partir da base correta (`develop`; hotfix crítico → `main`).
* **Ciclo semanal:** as heads `cycle/…-homolog`, `merge/…-develop` e `cycle/…-develop` somem no remoto ao mergear os três PRs da Etapa 6 — [Rotina de segunda — Etapa 6](tech-lead/rotina-segunda-feira.md#etapa-6--branches-merge-e-prs-para-as-longas).

**Pré-requisito (admin GitHub):** *Settings → General → Pull Requests* → **Automatically delete head branches** em cada repo de código (e no meta-repo se houver PRs de feature).

---

## Fluxo de uma tarefa

```
A fazer
  → Em desenvolvimento              (branch criada)
    → Bloqueado                   (depende de ação externa — flag visível na coluna Em desenvolvimento)
    → Code Review                 (PR aberto — aguarda code review)
      → Pronto p/ Homolog         (PR aprovado — merge na develop)
        → Em validação            (merge develop → homolog)
          → Pronto p/ Deploy      (homologação aprovada)
            → Concluído           (deploy realizado com sucesso)
          → Rejeitado             (reverter merge do PR)
            → Em desenvolvimento  (enviado para correção)
            → Cancelado           (abandono — decisão de negócio)
            → Bloqueado           (depende de ação externa — flag visível na coluna Em desenvolvimento)
```

### Ações principais

| Ação | Destino no Git | Status no Jira |
| --- | --- | --- |
| Iniciar andamento | Criar branch de trabalho a partir da `develop` | Em desenvolvimento |
| Enviar para review | Abrir PR — branch de trabalho → `develop` | Code Review |
| Aprovar review | Mesclar o PR | Pronto p/ Homolog |
| Merge develop → homolog | Via `cycle/` + `merge/YYYY-MM-DD-homolog` → `homolog` (rotina de segunda) · múltiplas tarefas | Em validação |
| Aprovar homologação | — | Pronto p/ Deploy |
| Deploy | Via `cycle/YYYY-MM-DD-homolog` → `main` (versão já no commit do ciclo) | Concluído |

**Atualize o Jira em cada transição** — não deixe para o final.

Após o merge do PR na `develop`, a branch de task é **eliminada no remoto** (auto-delete). Task **Rejeitado** ou nova rodada de trabalho → branch **nova** a partir de `develop` atualizada — não reutilize a branch antiga.

---

## Hotfix — fluxo paralelo

Usado exclusivamente para bugs críticos em produção que não podem aguardar o ciclo normal. Fura o ciclo semanal — **não fura o code review**.

```
main
└── hotfix/EP-123-descricao
    └── correção
        └── PR urgente (Agent + Tech Lead aprova)
            └── merge na main → deploy imediato
                ├── merge na homolog (obrigatório)
                └── merge na develop (obrigatório)
```

**Quem declara um hotfix:** Análise de negócio — nunca o dev que desenvolveu a correção.

**Regra inegociável:** o merge na `homolog` e `develop` após o hotfix é obrigatório.

Fluxo completo com agentes: [Tutorial — Suporte / Migração](suporte/tutorial-suporte-migracao.md) e [Tutorial — QA / SDET](qa/tutorial-qa-sdet.md) (regressão pós-hotfix).

---

## Ciclo semanal

| Momento | Evento |
| --- | --- |
| Segunda — manhã | Reunião de segunda (75–90 min): homolog, fila de entrega, review curta e planning — pauta no artigo 10 |
| Segunda — pós-reunião | Abrir `cycle/…` → reverts + sync hotfix + **versão** → `merge/…` → PRs para `main` / `develop` / `homolog` — passo a passo: [Rotina de segunda — Tech Lead](tech-lead/rotina-segunda-feira.md) |
| Terça | Deploy automático em produção a partir do merge na `main` |
| Terça a sexta | Desenvolvimento · PRs · code review · testa o novo homolog |

### Regras do ciclo

* O pós-reunião **não** altera `homolog`/`develop` por CLI: preparo nas `cycle/YYYY-MM-DD-*`, integração via PR (`cycle/…-homolog` → `main`; `merge/…` → `develop` e `homolog`)
* Após os PRs do ciclo nos repos de código, o meta-repo `epros` atualiza os ponteiros `backend/` e `frontend/` (submodules) — ver [Rotina de segunda §6.3](tech-lead/rotina-segunda-feira.md)
* O bump de versão (API, DFe, RealTime, Front — independentes) ocorre **nas `cycle/…` antes** dos PRs de release, para o build de produção já sair versionado
* O merge da cycle develop para `homolog` na segunda já abre o próximo ciclo de validação
* Features não validadas até a reunião de segunda são tratadas como rejeitadas — revert nas `cycle/…` (depois propagado às longas), task volta para **Rejeitado** no Jira
* Deploy em produção é automático a partir do merge na `main` — o merge é o gate
* Não mergeia na `main` se houver dúvida sobre qualquer item aprovado
* Skip e abort do pós-reunião Git (adiar merges, pular etapas, parar por CI/conflito): [Rotina de segunda — Pré-condições, skip e abort](tech-lead/rotina-segunda-feira.md#pré-condições-skip-e-abort)
* Com auto-delete no GitHub, cada PR de ciclo remove a branch head ao mergear (`cycle/…-homolog`, `merge/…`); só `cycle/…-develop` fica órfã até limpeza manual — ver [Rotina de segunda](tech-lead/rotina-segunda-feira.md)

A reunião de segunda **unifica** entrega Git/Jira e produto (review + planning) numa única cerimônia — ver [Cerimônias semanais no artigo 07](07-squads-cerimonias.md). Os blocos 0–2 e 5 tratam do fluxo operacional; os blocos 3–4, do sprint.

---

## Reunião de segunda — pauta padrão (75–90 min)

Uma única reunião, dois momentos: **decisão de entrega** (homolog, fila, reverts) e **produto** (review + planning).

| Bloco | Tempo | Conteúdo | Dono |
| --- | --- | --- | --- |
| 0 — Abertura | 5 min | Incidentes/P0 da semana · hotfixes pendentes de merge em `homolog`/`develop` | Tech Lead |
| 1 — Homolog (decisão) | 20 min | Percorrer board Homolog: itens em **Em validação** ou aguardando aprovação · aprovar ou rejeitar **na hora** no Jira (campos obrigatórios se rejeitado) | PO + time |
| 2 — Rejeitados + fila | 15 min | Revisar **Rejeitado**: corrige / cancela / prazo · cards parados em **Code Review** (>1 dia) e **Em desenvolvimento** sem movimento | Tech Lead |
| 3 — Sprint Review (curta) | 15 min | O que foi entregue vs sprint · carryover explícito | PO |
| 4 — Próximo homolog + Planning | 25 min | O que entra no próximo ciclo (`cycle/…-develop` → `homolog`) · prioridades da semana · bloqueios | PO + Tech Lead |
| 5 — Encerramento | 5 min | Checklist de saída (abaixo) · merges autorizados ou adiados com motivo | Tech Lead |

**Regras:** blocos 0, 1, 2 e 5 são inegociáveis. Bloco 3 pode encurtar se homolog estiver cheio.

### Checklist de encerramento

Antes de encerrar a reunião, confirme:

- [ ] Todo item em homolog foi **Pronto p/ Deploy** ou **Rejeitado** — nenhum card pendente de decisão
- [ ] Rejeitados têm motivo, responsável pela correção e estimativa de esforço
- [ ] Code reviews parados têm Tech Lead ou prazo definido na reunião
- [ ] Lista de reverts para pós-reunião está fechada
- [ ] Merge `cycle/…-homolog` → `main` e `merge/…-homolog` → `homolog` autorizados (ou adiados com motivo registrado)

**Pós-reunião (Git):** `cycle/` → reverts/sync/versão → `merge/` → PRs — [Rotina de segunda — Tech Lead](tech-lead/rotina-segunda-feira.md).

---

## Status no Jira

### Colunas do board (Desenvolvimento)

`A fazer` · `Em desenvolvimento` · `Code Review` · `Pronto p/ Homolog` · `Em validação` · `Pronto p/ Deploy` · `Concluído`

**Nota:** tasks com status `Bloqueado` permanecem visíveis na coluna `Em desenvolvimento` com flag de bloqueio. Tasks **Canceladas** ficam ocultas no board mas preservam histórico.

### Mapeamento — board Homolog

No board Homolog (validação de negócio), use estes nomes — equivalentes ao fluxo acima:

| Board Homolog | Status canônico (board Desenvolvimento) |
| --- | --- |
| Homologação | Em validação |
| Correção em homologação | Em validação (retrabalho) |
| Aprovado | Pronto p/ Deploy |

Decisões de aprovação/rejeição na reunião de segunda percorrem o **board Homolog**; o status canônico acima mantém o alinhamento com o board Desenvolvimento.

### Campos obrigatórios ao mover para Rejeitado

* **Motivo** — descrição específica do o que foi rejeitado e por quê
* **Tipo** — correção longa ou bloqueio externo
* **Responsável pela correção** — quem vai resolver
* **Estimativa de esforço** — para priorização no próximo ciclo

### Campos obrigatórios ao mover para Cancelado

* **Justificativa de negócio** — por que foi cancelado

---

## GitHub Actions

### Exclusão automática de branch head

Configuração de repositório (não é workflow): **Automatically delete head branches** remove a branch de origem do PR assim que o merge conclui. Reverts e rastreio usam merge commits nas longas (`git log --grep`, hash do commit) — não dependem da branch de task continuar existindo.

### Verificação de branch atualizada (PR para develop)

PR bloqueado automaticamente se a branch estiver atrás da `develop`. A mensagem de erro orienta o dev a executar:

```shell
git fetch origin
git rebase origin/develop
```

### Deploy de produção

Acionado automaticamente pelo merge na `main`. Não há gate manual — o merge é o gate.

---

## Regras de ouro

1. Ninguém commita direto em `main`, `homolog` ou `develop` — apenas via PR.
2. PR só é mergeado na `develop` após Code Review Agent no PR e **aprovação do Tech Lead**.
3. Branch desatualizada em relação à `develop` bloqueia o PR automaticamente.
4. Feature rejeitada no homolog sai da `develop` imediatamente via revert — nunca fica em estado ambíguo.
5. Hotfix fura o ciclo, não fura o review — e obrigatoriamente volta para a `develop`.
6. O merge na `main` na segunda é o gate de produção — não mergeia se houver dúvida.
7. Task em Rejeitado é revisada em toda reunião de segunda até ser resolvida.
8. O status no Jira é atualizado em cada transição, não no final.
9. Após merge do PR, a branch head some no remoto — retrabalho ou reabertura exige **nova branch**, não push na branch antiga.

---

## Checklist por papel

### Dev (backend e frontend)

- [ ] Branch com prefixo correto e `EP-xxx` no nome
- [ ] Code Review Agent no diff antes do PR
- [ ] PR aberto para `develop` com issue linkada
- [ ] Jira em **Code Review** ao abrir o PR
- [ ] CI verde (build + testes + 8 testes de segurança quando aplicável)

Detalhe: [Code Review — checklist do autor](code-review-checklists-e-boas-praticas.md)

Ver também: [Tutorial — Dev Backend](backend/tutorial-dev-backend.md) · [Tutorial — Dev Frontend](frontend/tutorial-dev-frontend.md)

### Tech Lead

- [ ] Revisão final humana após Code Review Agent
- [ ] Zero bloqueante antes do merge na `develop`
- [ ] Decisões de segunda registradas no Jira antes do merge `homolog` → `main`

Checklist de gate: [Tutorial Tech Lead — gate do PR](tech-lead/tutorial-tech-lead-arquiteto.md#checklist-de-gate-do-pr)

Ver também: [Tutorial — Tech Lead / Arquiteto](tech-lead/tutorial-tech-lead-arquiteto.md)

### QA / SDET

- [ ] Validação em homolog conforme plano de testes
- [ ] Regressão obrigatória após hotfix
- [ ] Edge case adicionado ao catálogo em bugs P0/P1

Ver também: [Tutorial — QA / SDET](qa/tutorial-qa-sdet.md)

### Suporte / Migração

- [ ] Hotfix declarado por Análise de negócio — não pelo dev
- [ ] Bug triado entra na esteira com contexto completo
- [ ] Pós-hotfix: merge obrigatório em `homolog` e `develop`

Ver também: [Tutorial — Suporte / Migração](suporte/tutorial-suporte-migracao.md)

---

## Base comum concluída

Você percorreu: produto → arquitetura → stack → código → segurança → IA → operação → **fluxo Git/Jira**.

Agora siga a **ramificação completa da sua função** (conceito + tutorial prático):

### Backend

1. [Trilha Backend — CQRS, DDD e EF Core](trilha-backend-cqrs-ddd.md)
2. [Trilha Backend — observabilidade, eventos e CI](trilha-backend-observabilidade.md)
3. [Tutorial — Dev Backend](backend/tutorial-dev-backend.md)

### Frontend

1. [Estrutura de pastas do epros-front](estrutura-pastas-front.md)
2. [Trilha Frontend — Nuxt 4 em três superfícies](trilha-frontend-nuxt.md)
3. [Tutorial — Dev Frontend](frontend/tutorial-dev-frontend.md)

### QA / SDET

1. [Trilha QA — testes e plano a partir dos ACs](trilha-qa.md)
2. [Tutorial — QA / SDET](qa/tutorial-qa-sdet.md)

### Tech Lead

1. [Trilha Tech Lead — ADRs, fases e guardião de domínio](trilha-tech-lead.md)
2. [Tutorial — Tech Lead / Arquiteto](tech-lead/tutorial-tech-lead-arquiteto.md)

### PO / Facilitador

1. [Tutorial — PO / Facilitador](po-facilitador/tutorial-po-facilitador.md)

### Guardião Fiscal (transversal)

1. [Tutorial — Guardião de Domínio (Fiscal)](fiscal/tutorial-guardiao-fiscal.md)

### Suporte / Migração

1. [Tutorial — Suporte / Migração](suporte/tutorial-suporte-migracao.md)

Índice completo: [Onboarding Epros ERP — Índice](README.md) · Referência de agentes: [índice de tutoriais](indice-tutoriais.md)

<!-- Manutenção Confluence: após alterar branches/ciclo neste artigo, republicar wiki id 142737410 (confluence_url no frontmatter). -->
