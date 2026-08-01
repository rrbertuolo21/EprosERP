# Configurar o Cursor para o EprosERP — e começar a usar hoje

> O repositório **já vem preparado**: `.cursor/rules`, `.cursor/skills` e `.cursor/commands`
> estão versionados. Clone, abra no Cursor e use — **sem passo de cópia**.

## Como a fábrica mapeia no Cursor

| Peça | Onde (já no repo) | Comportamento |
|---|---|---|
| `CLAUDE.md` (raiz) | lido automaticamente | disciplinas do projeto, toda conversa |
| `00-context.mdc` | `.cursor/rules/` (`alwaysApply: true`) | contexto/regra-#0 sempre ativo |
| Skills S01…S30 | `.cursor/rules/*.mdc` + `.cursor/skills/Sxx-*/SKILL.md` | o Cursor puxa por contexto, ou você chama `@S03-...` / a skill |
| Agentes por fase | `.cursor/commands/*.md` | slash-commands: `/dev`, `/qa`, `/code-review`… |
| Fonte editável | `docs/fabrica/` (agentes, cursor-install) | edite aqui e sincronize (ver Manutenção) |

## Passo 1 — Abrir o repo (já pronto)

Abra a raiz `EprosERP/` no Cursor. Carrega sozinho:

- **`00-context.mdc`** — sempre ativo.
- **`S01…S30`** — por contexto (Agent-requested / skills), ou `@S03-epros-multi-tenancy`.
- **Slash-commands** — `/dev`, `/architect`, `/planning`, `/qa`, `/ops`, `/security`, `/code-review`, `/engenharia-reversa`.

> Alternativa legada: um único `.cursorrules` na raiz. Ainda existe (regras de commit); o contexto
> rico fica em `.cursor/rules/*.mdc`.

## Passo 2 — Usar os agentes por fase

No chat, rode o slash-command da fase (ex.: `/dev`). Os prompts canônicos também estão em
`docs/fabrica/agentes/` se quiser Custom Mode:

- Construir → `/dev` · Revisar → `/code-review` · Testar → `/qa` ·
  Decisão técnica → `/architect` · Dados sensíveis/auth → `/security`.

## Passo 3 — Rodar a primeira task hoje

1. Abra o repo no Cursor (o `CLAUDE.md` e o `00-context` já estão ativos).
2. Chame `/dev`, aponte a task e a EF/regra (negócio vem SEMPRE da skill de negócio).
3. Deixe o agente executar e auto-validar → **você re-executa** o build/test no ambiente vivo
   (*"verde" do agente é entrada, não prova*) → `/code-review` → você libera.

## Adoção gradual (não precisa das 30 de cara)

Comece pelas de maior uso diário — **S01–S15** (contexto, convenções, multi-tenancy, fiscal, CQRS, EF,
segurança). As de fase (S16–S24) e especialização (S25–S30) entram conforme o time formaliza cada etapa.

## Manutenção

As `.mdc` / skills são **geradas** a partir das skills/agentes (`cursor-install/gen_cursor_rules.py`).
Edite a origem em `docs/fabrica/` e regenere o pacote em `cursor-install/rules/`; depois sincronize
para a raiz:

```powershell
pwsh -File docs/fabrica/cursor/cursor-install/sync-to-cursor.ps1
```

No dia a dia: use o que já está em `.cursor/` — não edite as cópias na mão.

Índice da fábrica: [../README.md](../README.md) · Pipeline: [../processo/PIPELINE.md](../processo/PIPELINE.md).
