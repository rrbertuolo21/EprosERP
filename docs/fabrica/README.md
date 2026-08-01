# Fábrica de Software — EprosERP

> Processo, agentes, skills e rules do Cursor — tudo em `docs/fabrica/`.  
> Canônicos de produto/código: [CLAUDE.md](../../CLAUDE.md) · [CONVENCAO_CODIGO.md](../../CONVENCAO_CODIGO.md).  
> Índice geral da documentação: [../README.md](../README.md).

## Árvore

| Pasta / arquivo | Papel |
|---|---|
| [cursor/CONFIGURAR-CURSOR.md](cursor/CONFIGURAR-CURSOR.md) | **Comece aqui** — instalar rules no Cursor (~2 min) |
| [cursor/cursor-install/rules/](cursor/cursor-install/rules/) | Skills S01–S30 + `00-context.mdc` (copiar para `.cursor/rules/`) |
| [agentes/](agentes/) | Workers por fase (Dev, QA, Architect, Security, Code Review…) |
| [processo/](processo/) | [PIPELINE](processo/PIPELINE.md) · [MODELO](processo/MODELO-FABRICA.md) · [RETROSPECTIVA](processo/RETROSPECTIVA-EPROSERP.md) |
| [skills/](skills/) | Skills destiladas de porte/ops (fan-out, Docker, Outbox, gateway, armadilhas) |
| [GUIA-POR-FUNCAO.md](GUIA-POR-FUNCAO.md) | Quem usa qual agente/skill, por papel |
| [epros-skills-catalogo.md](epros-skills-catalogo.md) | Histórico do catálogo S01–S31 (inventário vivo = rules `.mdc`) |
| [CURSOR-MODELOS.md](CURSOR-MODELOS.md) | Modelos/config Cursor |
| [code-review-comentario-via-ci.md](code-review-comentario-via-ci.md) | Review via CI |
| [jira/](jira/) | Governança de taxonomia Jira |

## Fluxo rápido

1. [CONFIGURAR-CURSOR](cursor/CONFIGURAR-CURSOR.md) → copiar `.mdc` para `.cursor/rules/`
2. Escolher agente em [agentes/](agentes/) conforme a fase ([PIPELINE](processo/PIPELINE.md))
3. Negócio/fiscal → carregar a rule `.mdc` correspondente (S04, S25, S26…)
4. Porte grande → [skills/fan-out-paralelo.md](skills/fan-out-paralelo.md)

## Relação com o restante de `docs/`

| Pasta | Papel |
|---|---|
| [../onboarding/](../onboarding/) | Trilha humana (produto, stack, tutoriais) |
| [../orquestracao/](../orquestracao/) | Mapa mestre / plano por submódulo |
| [../migracao/](../migracao/) | DePara e GAPs do porte |
| [../processos/](../processos/) | Ops: endpoints por ambiente |
