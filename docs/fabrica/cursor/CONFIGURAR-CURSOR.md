# Configurar o Cursor para o EprosERP — e começar a usar hoje

> Deixa o Cursor "formatado para o EprosERP": ele passa a seguir as convenções do projeto
> (multi-tenancy, CQRS, EF, fiscal, segurança…) automaticamente. **~2 minutos.** As regras já vêm
> prontas — você só copia.

## Como a fábrica mapeia no Cursor

| Peça (aqui no projeto) | Mecanismo do Cursor | Comportamento |
|---|---|---|
| `CLAUDE.md` (raiz) | lido automaticamente | as disciplinas do projeto, toda conversa |
| `docs/fabrica/cursor/cursor-install/rules/00-context.mdc` | regra `alwaysApply: true` | contexto/regra-#0 sempre ativo |
| `docs/fabrica/cursor/cursor-install/rules/S01…S30.mdc` | regras acionadas por contexto | o Cursor puxa a skill certa na hora (ou você chama `@S03-...`) |
| `docs/fabrica/agentes/*.md` | prompts salvos / Custom Modes | você escolhe o worker por fase (Dev, QA, Arquiteto…) |

## Passo 1 — Instalar as regras (copiar, pronto)
Na **raiz do repositório** (`EprosERP/`), copie as regras já geradas para `.cursor/rules/`:
```bash
mkdir -p .cursor/rules
cp docs/fabrica/cursor/cursor-install/rules/*.mdc .cursor/rules/
```
No PowerShell:
```powershell
New-Item -ItemType Directory -Force -Path .cursor\rules | Out-Null
Copy-Item docs\fabrica\cursor\cursor-install\rules\*.mdc .cursor\rules\
```
Pronto. O Cursor carrega sozinho:
- **`00-context.mdc`** (`alwaysApply: true`) — sempre ativo.
- **`S01…S30.mdc`** (`alwaysApply: false` + `description`) — o Cursor puxa por contexto, ou você força
  com `@S03-epros-multi-tenancy` no chat.

> Alternativa legada: um único `.cursorrules` na raiz com o corpo de
> `docs/fabrica/agentes/00-context-agent.md`. Funciona, mas `.cursor/rules/*.mdc` é melhor (acionamento por contexto).

## Passo 2 — Usar os agentes por fase (opcional, recomendado)
Cada arquivo em `docs/fabrica/agentes/` é um **worker** (persona + missão + gate). Salve o corpo como um
prompt/Custom Mode no Cursor e ative por fase:
- Construir → `07-dev-agent.md` · Revisar → `12-code-review-agent.md` · Testar → `08-qa-agent.md` ·
  Decisão técnica → `06-architect-agent.md` · Dados sensíveis/auth → `10-security-agent.md`.

## Passo 3 — Rodar a primeira task hoje
1. Abra o repo no Cursor (o `CLAUDE.md` já está ativo).
2. Chame o Dev Agent, aponte a task e a EF/regra (negócio vem SEMPRE da skill de negócio).
3. Deixe o agente executar e auto-validar → **você re-executa** o build/test no ambiente vivo
   (*"verde" do agente é entrada, não prova*) → Code Review → você libera.

## Adoção gradual (não precisa das 30 de cara)
Comece pelas de maior uso diário — **S01–S15** (contexto, convenções, multi-tenancy, fiscal, CQRS, EF,
segurança). As de fase (S16–S24) e especialização (S25–S30) entram conforme o time formaliza cada etapa.

## Manutenção
As `.mdc` são **geradas** a partir das skills/agentes (`cursor-install/gen_cursor_rules.py`).
Edite a skill/agente de origem e regenere o pacote; no dia a dia do projeto: use as regras prontas.

Índice da fábrica: [../README.md](../README.md) · Pipeline: [../processo/PIPELINE.md](../processo/PIPELINE.md).
