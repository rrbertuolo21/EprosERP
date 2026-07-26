# CONFIGURAR NO CURSOR — e começar a usar hoje

> Passo a passo para instalar a fábrica no Cursor e rodar a primeira task **hoje**.
> As 3 peças da fábrica mapeiam direto em 3 mecanismos do Cursor:

| Peça | Mecanismo do Cursor | Comportamento |
|---|---|---|
| **Context Agent** (00) | Regra `alwaysApply` (ou `.cursorrules`) | Sempre ativo, toda conversa |
| **Skills** (S01–S30) | `.cursor/rules/*.mdc` | Acionadas por contexto (o `description` dispara) |
| **Agentes** (personas de fase) | Custom Modes / prompts salvos | Você escolhe por fase |

---

## Passo 1 — Colocar a fábrica no repo do produto
Copie a pasta para dentro do `PlataformaSaaS/` (versiona junto com o código, passa por PR):
```
PlataformaSaaS/docs/fabrica/     ← agentes/ skills/ prompts/ CONTEXT.md ...
```

## Passo 2 — Context Agent sempre ativo
Crie `PlataformaSaaS/.cursor/rules/00-context.mdc`:
```mdc
---
description: Contexto e regras invioláveis do Epros — sempre ativo
alwaysApply: true
---
<cole o corpo de agentes/00-context-agent.md>
```
*(Alternativa simples: o arquivo único `.cursorrules` na raiz — funciona, formato legado.)*

## Passo 3 — Skills como regras acionadas por contexto
Um arquivo `.cursor/rules/Sxx.mdc` por skill. O campo **`description`** é o que faz o Cursor
puxar a skill na hora certa — use o `description:` que já existe no frontmatter da skill:
```mdc
---
description: Isolamento multi-tenant do Epros — QueryFilter, RLS, pontos cegos. Use ao criar entidades, queries, jobs ou revisar PRs.
globs:
alwaysApply: false
---
<corpo de skills/S03-epros-multi-tenancy/SKILL.md>
```
Os 4 tipos de regra do Cursor:
- **Always** (`alwaysApply: true`) — sempre no contexto (use só no 00-context).
- **Auto-attached** (por `globs`) — dispara quando você edita arquivos que casam o padrão.
- **Agent-requested** (por `description`) — o modelo puxa por relevância. **É o que as skills usam.**
- **Manual** (`@S03`) — você chama na mão.

> Comece pelas Camadas 0 e 1 (S01–S15, as de maior uso). O resto entra sob demanda com `@`.

## Passo 4 — Agentes como Custom Modes
No Cursor: **Settings → Chat → Custom Modes** (ou salve como prompt). Um modo por fase que você
usa, colando o prompt do agente (`../../agentes/07-dev-agent.md` etc.). Quem usa quais agentes está
no [GUIA-POR-FUNCAO.md](GUIA-POR-FUNCAO.md).

## Passo 5 — O dia a dia
Abra o modo do agente da fase → cole o **prompt de partida** de `../../prompts/fase-0X.md` → preencha
os `{campos}` → anexe o artefato da fase anterior → confira o gate. Context (Passo 2) e skills
(Passo 3) entram sozinhos.

---

## Começar a usar HOJE (o mínimo que funciona)

Não precisa configurar tudo pra começar. O trio de maior uso resolve o dia a dia:

1. **Instale o Context** (Passo 2) — 5 minutos.
2. **Instale 3 skills** (Passo 3): **S01** (contexto), **S02** (convenções), **S03** (tenancy).
3. **Salve 2 modos** (Passo 4): **Dev** e **Code Review**.
4. **Pegue uma task pequena** do Bloco 6 e rode:
   - Modo **Dev** → cole `../../prompts/fase-07-dev.md` (Prompt A) → preencha → gere código + testes.
   - Modo **Code Review** → cole `../../prompts/transversais.md` (Code Review) no diff → corrija 🔴.
   - Abra o PR.
5. **Sinal de que funcionou:** o código já sai com EntidadeSaaSBase, CQRS, snake_case e testes — sem você pedir. Se sair errado, a skill (não o prompt) é que precisa de ajuste.

> Expanda depois: adicione Architect + as skills S06/S07/S08/S10 conforme as fases forem sendo
> formalizadas no time. Ordem sugerida das ondas: `MANUAL.md §6`.
