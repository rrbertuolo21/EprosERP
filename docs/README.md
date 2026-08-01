# Documentação — EprosERP

> Onboarding rápido na raiz: [COMECE-POR-AQUI.md](../COMECE-POR-AQUI.md) · [COMECE-AQUI.md](../COMECE-AQUI.md).  
> Canônicos: [CLAUDE.md](../CLAUDE.md) · [CONVENCAO_CODIGO.md](../CONVENCAO_CODIGO.md).  
> Fábrica (agentes / skills / Cursor): [fabrica/](fabrica/).

## Árvore

| Pasta | Papel |
|---|---|
| [fabrica/](fabrica/) | **Fábrica completa** — agentes, processo, skills, rules `.mdc`, guias, Jira |
| [onboarding/](onboarding/) | Trilha humana (produto, stack, fluxo, tutoriais por função) |
| [orquestracao/](orquestracao/) | Mapa mestre / plano de implementação por submódulo |
| [migracao/](migracao/) | DePara, GAPs e logs de completude do porte |
| [processos/](processos/) | Ops: endpoints por ambiente e nomenclatura |

## Primeiro dia

1. [COMECE-AQUI.md](../COMECE-AQUI.md) — subir Docker + credenciais  
2. [fabrica/cursor/CONFIGURAR-CURSOR.md](fabrica/cursor/CONFIGURAR-CURSOR.md) — ligar as rules  
3. [onboarding/](onboarding/) — trilha longa (opcional, ~1h45)  
4. [fabrica/processo/PIPELINE.md](fabrica/processo/PIPELINE.md) — esteira de fases  

## Adaptação (material vindo do monorepo `epros`)

| Conceito nos textos antigos | Neste repo |
|---|---|
| `epros-back` / `epros-front` | Monólito `EprosERP`: `src/` + `Epros.App/` |
| Nuxt 4 + `app/modules/...` | **Nuxt 3** + `pages/erp\|plataforma\|area-cliente/` |
| Gateway YARP | API host em `src/API/Epros.API` |
| `_fabrica/` (pasta na raiz) | **`docs/fabrica/`** (migrado) |
| “CONTEXT.md fonte única” | `CLAUDE.md` + `CONVENCAO_CODIGO.md` |

Front real: [onboarding/estrutura-pastas-front.md](onboarding/estrutura-pastas-front.md).  
Memória de reorganização: [MEMORIA_SESSAO_REORGANIZACAO.md](../MEMORIA_SESSAO_REORGANIZACAO.md).
