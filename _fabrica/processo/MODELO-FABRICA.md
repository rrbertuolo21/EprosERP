# MODELO DA FÁBRICA — a fábrica de software perfeita com IA

> A tese, os produtos e o que ainda falta criar. Este documento é o mapa mental da fábrica.
> Detalhes de arquitetura de pastas: [ARQUITETURA-CONHECIMENTO.md](ARQUITETURA-CONHECIMENTO.md).

## A tese: o multiplicador

Um desenvolvedor com IA empoderada **não escreve código mais rápido — ele dirige e valida um
time de agentes**. Ele vira **diretor**: especifica a tarefa, confere o resultado, libera. A IA
executa. Por isso 7 desenvolvedores viram capacidade de 15+: cada um comanda uma célula de agentes.

**A regra que faz isso escalar:** o humano só *confere e libera* — nunca refaz. Para isso, cada
tarefa precisa sair da IA **já auto-validada**. Sem esse portão (gate), 1 dev não vira 15 — vira
1 dev revisando código ruim. O gate é o coração do modelo.

## Os 3 produtos da fábrica

| Produto | O que é | Onde vive |
|---|---|---|
| **Agente** | O worker — executa uma função (Dev, Arquiteto, QA, PO…) | `agentes/` |
| **Skill** | O conhecimento que calibra o worker | `Conhecimento-acumulado/` (agnóstico) + skills de projeto |
| **Projeto** | A instância onde a fábrica produz | `projetos/<nome>/` |

Agente e Skill são **produtos vendáveis**: um "Dev PHP" (agente + skills de PHP) é um
desenvolvedor competente e barato que você aluga por projeto.

## Os 4 tipos de Skill (a chave do sistema)

Nem toda skill é igual. São quatro tipos, e um projeto usa os quatro:

| Tipo | O que carrega | Reutilizável? | Exemplo |
|---|---|---|---|
| **1. Conhecimento** (agnóstica) | O saber universal de engenharia | Sim, em todo projeto | `linguagens/php`, `arquitetura/ddd` |
| **2. Formato de Projeto** | As **convenções daquele projeto**: stack escolhida, layout de pastas, nomenclatura, DoR/DoD, fluxo de PR, design system | Não — é do projeto | "Padrões do Projeto Gov" |
| **3. Negócio / Domínio** | O **conhecimento de negócio** do cliente/setor: glossário, regras, regulações, processos — suporta a **especificação** | Parcial — por setor | "Domínio: licitações públicas" |
| **4. Playbook** | Procedimentos repetíveis do projeto (deploy, incidente, publicação) | Não — é do projeto | "Runbook de deploy" |

> A fórmula: **Skills tipo 1 (conhecimento) o projeto REFERENCIA; skills tipo 2, 3 e 4 o projeto
> CRIA.** Ver `projetos/_TEMPLATE/`.

## O modelo humano-diretor (o gate — como 1 vira 15)

```
ESPECIFICA (humano + agente PO/Requirements, com skill de NEGÓCIO)
   → QUEBRA em tarefas (agente Planning)
   → EXECUTA (agente Dev, com skills de CONHECIMENTO + FORMATO)  ← a IA faz
   → AUTO-VALIDA (o próprio agente roda o checklist da skill)    ← entrada, não prova
   → RE-EXECUTA (o ORQUESTRADOR reconfere no ambiente VIVO)      ← a IA prova a IA
   → PRÉ-APROVA (agente Code Review/QA — pega o que passou)
   → CONFERE E LIBERA (humano — só o veredito final)             ← o diretor
```

Cada skill de execução traz um **Checklist de pronto** (como a de PHP). É isso que permite ao
diretor "só conferir": ele confia no gate, não relê linha a linha.

**A lição da primeira produção real (EprosERP):** o gate NÃO é um checkpoint — são **três**. O
auto-relato do agente ("build verde", "testes passando") **não é prova** — agentes reportam verde
que não é. Entre o agente e o humano existe um **orquestrador de IA** que **re-executa a validação
no ambiente vivo** (roda o build/test de novo, sobe o banco real, faz a chamada externa real) e só
então consolida. É esse **gate do meio** que faz o "verde" ser verdade. "Build verde" ≠ "funciona":
compila e passa nos testes, mas pode faltar migration, ter URL hardcoded, ou não ter o adaptador de
integração — só o ambiente vivo pega. Detalhe: [RETROSPECTIVA-EPROSERP.md](RETROSPECTIVA-EPROSERP.md).

## Produção em escala — o fan-out dirigido por IA

1 dev vira 15+ não por um agente rápido, mas por um **orquestrador** que dispara **muitos agentes em
paralelo, cada um dono de uma pasta disjunta** (um `Epros.Modules.X` / uma `pages/erp/<mod>/`), sem
colisão de escrita. O padrão provado: **gargalo serial primeiro** (scaffolding compartilhado) →
**prova o molde** (1 fatia de referência) → **fan-out** (N agentes) → **re-execução + consolidação**
serializada → **report por bloco com números reverificados**. Para porte/reengenharia, antes do
fan-out há um **gate de cobertura**: recon exaustivo → ERD + matriz → validação humana. Ver
`processo-agile/fan-out-paralelo` e `PIPELINE.md` (Modo Fan-out).

## O que ainda falta criar (roadmap da fábrica perfeita)

| Peça | Status | O que é |
|---|---|---|
| Skills de conhecimento (agnósticas) | ✅ 78 skills (padrão-produto) | O acervo reutilizável |
| **Fórmula de skills de projeto** | 🔨 construindo | Templates de skill Formato + Negócio (`projetos/_TEMPLATE/`) |
| **Bootstrap de projeto** | 🔨 construindo | Como instanciar um projeto novo em 1 dia |
| **Agentes especialistas por stack** | ⬜ próximo | "Dev PHP", "Dev Java" — worker com as skills certas carregadas |
| **Biblioteca de negócio por setor** | ⬜ | Skills de domínio reutilizáveis (governo, varejo, saúde…) |
| **Camada de orquestração de tarefas** | ⬜ | Como spec→tarefas→execução→validação flui e é rastreada |
| **Métricas de gate** | ⬜ | Como medir que o agente entregou certo (taxa de retrabalho, cobertura) |
| **Onboarding do diretor** | ⬜ | Manual de como o dev conduz e valida sua célula de IA |

## Como um projeto nasce (resumo — detalhe em `projetos/_TEMPLATE/LEIA-ME.md`)

1. `projetos/<nome>/` — cria a pasta do projeto.
2. **Declara a stack** no `CONTEXT.md` do projeto.
3. **Referencia** as skills de conhecimento da stack (tipo 1) — não copia.
4. **Cria** a skill de Formato do projeto (tipo 2) e as de Negócio (tipo 3) a partir dos templates.
5. **Atribui os agentes** especialistas (tipo worker) que carregam essas skills.
6. Roda a esteira do `PIPELINE.md` com o gate humano em cada portão.
