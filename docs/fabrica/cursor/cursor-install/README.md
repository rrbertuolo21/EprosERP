# Pacote de instalação no Cursor — espelho de `.cursor/`

Regras do Cursor **geradas automaticamente** a partir das skills e do Context Agent
(`agentes/00-context-agent.md`). **Não edite aqui no dia a dia** — edite a skill/agente de origem,
regenere e sincronize para a raiz do repo.

O repositório já versiona `.cursor/{rules,skills,commands}/`. Quem clona **não precisa copiar nada**.
Ver [CONFIGURAR-CURSOR.md](../CONFIGURAR-CURSOR.md).

## O que vai para onde

| Origem | Destino versionado |
|---|---|
| `rules/*.mdc` | `.cursor/rules/` |
| `rules/S*.mdc` (corpo) | `.cursor/skills/Sxx-*/SKILL.md` |
| `docs/fabrica/agentes/*.md` | `.cursor/commands/` (`/dev`, `/qa`, …) |

## Regenerar + sincronizar

```powershell
# 1) regenerar .mdc (quando o gerador apontar para este pacote)
python docs/fabrica/cursor/cursor-install/gen_cursor_rules.py

# 2) espelhar para .cursor/ na raiz
pwsh -File docs/fabrica/cursor/cursor-install/sync-to-cursor.ps1
```

## Comportamento no Cursor

- **`00-context.mdc`** → `alwaysApply: true` — sempre ativo.
- **`S01…S30.mdc`** / **skills** → por contexto, ou `@S03-epros-multi-tenancy`.
- **commands** → slash-commands no chat (`/dev`, `/code-review`, …).

## Adoção

Comece pelas **Camadas 0 e 1** (S01–S15). Especialização (S25–S30) e fases (S16–S24) conforme o time.
Detalhe em [CONFIGURAR-CURSOR.md](../CONFIGURAR-CURSOR.md).
