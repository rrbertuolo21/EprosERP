# PIPELINE — A Fábrica de Software Epros em Operação

> Este documento define **a ordem de uso dos agentes** para que o conjunto funcione como uma
> fábrica: cada fase tem um agente responsável, uma entrada, uma saída e um portão (gate) que
> libera a fase seguinte. Os agentes transversais atuam em qualquer ponto da esteira.

---

## 1. O fluxo principal (esteira de um submódulo novo)

```
 DEMANDA
    │
    ▼
┌─ FASE 01 ─ Strategy Agent ──────────────────────────────────────────┐
│ Entrada: ideia / pedido de cliente / item do inventário (132 subs)  │
│ Saída:   Business Case com go/no-go                                 │
│ GATE:    go aprovado pela liderança + OKR vinculado                 │
└──────────────────────────────────────────────────────────────────────┘
    │ go
    ▼
┌─ FASE 02 ─ Discovery Agent ─────────────────────────────────────────┐
│ Entrada: Business Case + entrevistas/notas com usuários             │
│ Saída:   dores, causas raiz, personas, JTBDs, Problem Statement     │
│ GATE:    Problem Statement validado pelo PO (≥ 5 entrevistas        │
│          ou risco assumido explicitamente)                          │
└──────────────────────────────────────────────────────────────────────┘
    │
    ▼
┌─ FASE 03 ─ Requirements Agent ──────────────────────────────────────┐
│ Entrada: Problem Statement + JTBDs                                  │
│ Saída:   User Stories com critérios de aceite, NFRs e dependências  │
│ GATE:    DoR (Definition of Ready) da S18 — sem termo vago,         │
│          impacto fiscal e multi-tenancy respondidos                 │
└──────────────────────────────────────────────────────────────────────┘
    │
    ▼
┌─ FASE 04 ─ UX Agent ────────────────────────────────────────────────┐
│ Entrada: User Stories                                               │
│ Saída:   fluxos e telas revisados (aprovado / com ressalvas)        │
│ GATE:    aprovado para desenvolvimento (consistência + WCAG +       │
│          confirmações fiscais definidas)                            │
└──────────────────────────────────────────────────────────────────────┘
    │
    ▼
┌─ FASE 05 ─ Planning Agent ──────────────────────────────────────────┐
│ Entrada: US aprovadas + telas                                       │
│ Saída:   breakdown em tasks estimadas + ordem de execução           │
│ GATE:    cabe na sprint (total ≤ velocity) ou replanejado           │
└──────────────────────────────────────────────────────────────────────┘
    │
    ▼
┌─ FASE 06 ─ Architect Agent ─────────────────────────────────────────┐
│ Entrada: breakdown + questões técnicas abertas                      │
│ Saída:   tech design + ADRs para decisões novas                     │
│ GATE:    zero violação de padrão (tenancy, Outbox, hexagonal);      │
│          spikes resolvidos                                          │
│ Obs.:    para task simples que segue padrão existente, esta fase    │
│          pode ser pulada — o padrão JÁ é a decisão                  │
└──────────────────────────────────────────────────────────────────────┘
    │
    ▼
┌─ FASE 07 ─ Dev Agent ───────────────────────────────────────────────┐
│ Entrada: tasks + tech design                                        │
│ Saída:   código + testes (PR aberto) + EVIDÊNCIA reproduzível       │
│ GATE:    build verde + testes passando + auto-review com S02/S03    │
│          ⚠️ o "verde" do agente é ENTRADA, não prova: o             │
│          ORQUESTRADOR re-executa build/test e valida no AMBIENTE    │
│          VIVO (banco real, chamada externa real). Ver seção 6.      │
└──────────────────────────────────────────────────────────────────────┘
    │ PR
    ▼
┌─ TRANSVERSAL ─ Code Review Agent (+ Security Agent se tocar dados   │
│ sensíveis, auth ou endpoints públicos)                              │
│ Saída: review estruturado                                           │
│ GATE:  zero bloqueante 🔴 → segue para revisão final do Tech Lead   │
└──────────────────────────────────────────────────────────────────────┘
    │ merge
    ▼
┌─ FASE 08 ─ QA Agent ────────────────────────────────────────────────┐
│ Entrada: build de release + critérios de aceite                     │
│ Saída:   plano executado + edge cases do catálogo verificados       │
│ GATE:    zero P0/P1 aberto; cenários fiscais e de tenancy verdes    │
└──────────────────────────────────────────────────────────────────────┘
    │
    ▼
┌─ FASE 09 ─ Ops Agent ───────────────────────────────────────────────┐
│ Entrada: release candidato                                          │
│ Saída:   deploy (canary se fiscal/cobrança) + monitoramento ativo   │
│ GATE:    checklist go-live 100% + rollback testado                  │
└──────────────────────────────────────────────────────────────────────┘
    │
    ▼
┌─ PÓS-DEPLOY ─ Docs Agent ───────────────────────────────────────────┐
│ Saída: changelog, wiki, OpenAPI atualizados                         │
│ E o ciclo alimenta a fábrica: bug P0 → edge case (S21) + runbook    │
│ (S22); decisão nova → ADR (S05); módulo concluído → status (S01)    │
└──────────────────────────────────────────────────────────────────────┘
```

## 2. Agentes transversais — quando entram

| Agente | Entra quando | Em qualquer fase? |
|---|---|---|
| **Security** | PR toca auth, dados pessoais, secrets, endpoint público; revisão de spec sensível | Sim (obrigatório nas fases 03, 06, 07) |
| **Docs** | Ao final de toda fase que produz artefato; sempre pós-deploy | Sim |
| **Code Review** | Todo PR, antes do Tech Lead | Fase 07 → 08 |
| **Fiscal** | Qualquer dúvida/validação tributária; obrigatório em specs e testes de features fiscais | Sim (obrigatório nas fases 03, 07, 08) |
| **Support** | Ticket de cliente; alimenta a esteira com bugs triados (entram na fase 05 do próximo ciclo) | Fora da esteira, na operação |
| **Migration** | Projeto de migração de cliente (fluxo próprio, seção 4) | Fluxo próprio |

## 3. Fluxos curtos (nem tudo percorre a esteira inteira)

**Bug de produção (hotfix):**
Support (tria) → Dev (corrige, consultando a skill do tema) → Code Review → Ops (deploy com runbook) → QA (regressão) → Docs (changelog) → **obrigatório:** QA adiciona o edge case ao catálogo (S21).

**Melhoria pequena (sem discovery):**
Requirements (US direto) → Planning → Dev → Code Review → QA → Ops.

**Spike / dúvida técnica:**
Architect (com as skills de engenharia) → resultado vira ADR ou nota técnica → volta ao Planning.

## 4. Fluxo de migração de cliente (Bloco 7) — paralelo à esteira

```
Migration Agent: plano do cliente (S27)
  → ensaio em staging → janela de migração → convivência (novo ativo,
  legado read-only) → conciliação de saldos → GATE: saldos batem
  → corte → Support Agent monitora o cliente por 2 semanas
  → aprendizados de volta à S27
```

## 6. Modo Fan-out — produção em escala (provado no EprosERP)

A esteira acima é para **um** submódulo. Quando a demanda é grande (portar um legado inteiro, gerar
N telas, construir M módulos), a fábrica opera em **fan-out paralelo dirigido por um orquestrador de IA**:

```
GARGALO SERIAL primeiro   → 1 agente cria o scaffolding compartilhado (o que todos consomem
                            e ninguém edita: cliente de API, layouts, componentes base).
PROVA O MOLDE             → 1 "fatia de referência" (1 submódulo/tela no padrão exato). Só avança
                            se o molde valida no ambiente vivo.
FAN-OUT                   → N agentes em paralelo, cada um DONO de uma PASTA DISJUNTA
                            (1 Epros.Modules.X / 1 pages/erp/<mod>/). Zero colisão de escrita.
                            Contrato fixo entre agentes paralelos (ex.: a rota/DTO que back e front
                            compartilham) definido ANTES.
RE-EXECUÇÃO + CONSOLIDAÇÃO→ o orquestrador reconfere CADA saída no ambiente vivo (não confia no
                            relato do agente), serializa a consolidação (build/migração/teste) e
                            REPORTA por bloco com NÚMEROS REVERIFICADOS (do banco/git vivos).
```

**Gate de cobertura (porte/reengenharia):** antes do fan-out de construção, recon EXAUSTIVO
(artefato-por-artefato, campo-por-campo — não amostral) → **ERD + matriz de cobertura** → **validação
humana**. Mapear **integrações como cidadãos de primeira classe** (adaptador + config + credencial),
não só entidades. Checklist de porte: entidade → **migration aplicada** + **CRUD completo por raiz**
(GET/{id}, PUT, DELETE) + endpoint de detalhe; senão a tela não fecha. Detalhes:
`processo-agile/fan-out-paralelo`, `engenharia-reversa/armadilhas-de-porte`, `RETROSPECTIVA-EPROSERP.md`.

## 5. Regras da fábrica

1. **Nenhuma fase começa sem o gate da anterior** — exceto os fluxos curtos da seção 3.
0. **O "verde" de um agente é entrada, não prova.** O orquestrador RE-EXECUTA a validação no ambiente
   vivo antes de reportar/liberar. "Build verde" ≠ "funciona" (pode faltar migration, ter URL hardcoded,
   faltar adaptador de integração — só o ambiente vivo pega).
2. **O artefato de saída de uma fase é a entrada da seguinte** — sem retrabalho de contexto: o link/arquivo acompanha o handoff.
3. **Skills são a fonte da verdade; agentes são a interface.** Se um agente contradiz uma skill, a skill vence e o prompt do agente é corrigido.
4. **Todo P0 realimenta a fábrica** (edge case + runbook + possível ADR). É isso que faz a fábrica melhorar com o uso.
5. **Um humano é dono de cada gate.** Agente recomenda; PO, Tech Lead ou liderança decide.
