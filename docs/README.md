# Documentação — EprosERP

> **Novo no projeto?** → [ROTEIRO-ONBOARDING.md](ROTEIRO-ONBOARDING.md) (ordem de leitura)  
> **Só subir agora?** → [QUICKSTART-LOCAL.md](QUICKSTART-LOCAL.md) (Docker + seed)  
> Canônicos na raiz: [CLAUDE.md](../CLAUDE.md) · [CONVENCAO_CODIGO.md](../CONVENCAO_CODIGO.md)  
> Fábrica: [fabrica/](fabrica/)

## Árvore

| Pasta / arquivo | Papel |
|---|---|
| [ROTEIRO-ONBOARDING.md](ROTEIRO-ONBOARDING.md) | Índice: o que ler e em que ordem |
| [QUICKSTART-LOCAL.md](QUICKSTART-LOCAL.md) | Subir ambiente local em 1 comando |
| [fabrica/](fabrica/) | Agentes, processo, skills, rules `.mdc`, guias, Jira |
| [onboarding/](onboarding/) | Trilha humana longa (produto, stack, tutoriais) |
| [orquestracao/](orquestracao/) | Mapa mestre / plano por submódulo |
| [migracao/](migracao/) | DePara, GAPs, logs de completude, molde de porte |
| [historico/](historico/) | Planos e memórias arquivados |
| [ops/](ops/) | Instalação local expandida, deploy |
| [processos/](processos/) | Endpoints por ambiente e nomenclatura |

## Primeiro dia

1. [QUICKSTART-LOCAL.md](QUICKSTART-LOCAL.md) — Docker + `./scripts/seed-local.sh`  
2. [fabrica/cursor/CONFIGURAR-CURSOR.md](fabrica/cursor/CONFIGURAR-CURSOR.md) — ligar as rules  
3. [onboarding/](onboarding/) — trilha longa (opcional, ~1h45)  
4. [fabrica/processo/PIPELINE.md](fabrica/processo/PIPELINE.md) — esteira de fases  

## Retomada / backlog

| Preciso de… | Onde |
|---|---|
| Diário da fábrica | [HISTORICO-DESENVOLVIMENTO-IA.md](../HISTORICO-DESENVOLVIMENTO-IA.md) |
| Gaps vivos | [CONSOLIDACAO-GAPS.md](../CONSOLIDACAO-GAPS.md) |
| Índice IA | [MEMORY.md](../MEMORY.md) |
| Prompt nova sessão | [PROMPT_REINICIO.md](../PROMPT_REINICIO.md) |

## Adaptação (material vindo do monorepo `epros`)

| Conceito nos textos antigos | Neste repo |
|---|---|
| `epros-back` / `epros-front` | Monólito `EprosERP`: `src/` + `Epros.App/` |
| Nuxt 4 + `app/modules/...` | **Nuxt 3** + `pages/erp\|plataforma\|area-cliente/` |
| Gateway YARP | API host em `src/API/Epros.API` |
| `_fabrica/` (pasta na raiz) | **`docs/fabrica/`** |
| `COMECE-POR-AQUI.md` / `COMECE-AQUI.md` | **`docs/ROTEIRO-ONBOARDING.md`** / **`docs/QUICKSTART-LOCAL.md`** |
| `PADRAO_PORTE_LEGADO.md` na raiz | **`docs/migracao/PADRAO_PORTE_LEGADO.md`** |
| `./seed-local.sh` na raiz | **`./scripts/seed-local.sh`** |
| “CONTEXT.md fonte única” | `CLAUDE.md` + `CONVENCAO_CODIGO.md` |

Front real: [onboarding/estrutura-pastas-front.md](onboarding/estrutura-pastas-front.md).  
Memória de reorganização: [MEMORIA_SESSAO_REORGANIZACAO.md](../MEMORIA_SESSAO_REORGANIZACAO.md).
