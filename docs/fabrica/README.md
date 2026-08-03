# Fábrica de Software — EprosERP

> Processo, agentes, skills e rules do Cursor — tudo em `docs/fabrica/`.  
> Canônicos de produto/código: [CLAUDE.md](../../CLAUDE.md) · [CONVENCAO_CODIGO.md](../../CONVENCAO_CODIGO.md).  
> Índice geral da documentação: [../README.md](../README.md).

## Árvore

| Pasta / arquivo | Papel |
|---|---|
| [cursor/CONFIGURAR-CURSOR.md](cursor/CONFIGURAR-CURSOR.md) | **Comece aqui** — `.cursor/` já vem versionado (rules + skills + commands) |
| [cursor/cursor-install/](cursor/cursor-install/) | Pacote gerado + `sync-to-cursor.ps1` (espelha para `.cursor/`) |
| [agentes/](agentes/) | Workers por fase (fonte dos slash-commands `/dev`, `/qa`…) |
| [processo/](processo/) | [PIPELINE](processo/PIPELINE.md) · [MODELO](processo/MODELO-FABRICA.md) · [RETROSPECTIVA](processo/RETROSPECTIVA-EPROSERP.md) |
| [skills/](skills/) | Skills destiladas de porte/ops (fan-out, Docker, Outbox, gateway, armadilhas) |
| [GUIA-POR-FUNCAO.md](GUIA-POR-FUNCAO.md) | Quem usa qual agente/skill, por papel |
| [epros-skills-catalogo.md](epros-skills-catalogo.md) | Histórico do catálogo S01–S31 (inventário vivo = rules `.mdc`) |
| [CURSOR-MODELOS.md](CURSOR-MODELOS.md) | Modelos/config Cursor |
| [code-review-comentario-via-ci.md](code-review-comentario-via-ci.md) | Review via CI |
| [jira/](jira/) | Governança de taxonomia Jira |

## Fluxo rápido

1. Abrir o repo no Cursor — [CONFIGURAR-CURSOR](cursor/CONFIGURAR-CURSOR.md) (sem cópia)
2. Slash-command da fase (`/dev`, `/qa`…) ou agente em [agentes/](agentes/) ([PIPELINE](processo/PIPELINE.md))
3. Negócio/fiscal → skill correspondente (S04, S25, S26…)
4. Porte grande → [skills/fan-out-paralelo.md](skills/fan-out-paralelo.md)

## Relação com o restante de `docs/`

| Pasta | Papel |
|---|---|
| [../onboarding/](../onboarding/) | Trilha humana (produto, stack, tutoriais) |
| [../orquestracao/](../orquestracao/) | Mapa mestre / plano por submódulo |
| [../migracao/](../migracao/) | DePara e GAPs do porte |
| [../processos/](../processos/) | Ops: endpoints por ambiente |
