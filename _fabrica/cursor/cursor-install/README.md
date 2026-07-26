# Pacote de instalação no Cursor — `.cursor/rules`

Regras do Cursor **geradas automaticamente** a partir das skills (`skills/Sxx/SKILL.md`) e do
Context Agent (`agentes/00-context-agent.md`). Copie e use — não edite aqui; edite a skill/agente
de origem e regenere (script em `scratchpad`/`cursor-install`).

## Como instalar

1. Copie o conteúdo de `rules/` para dentro do repositório do produto:
   ```
   PlataformaSaaS/.cursor/rules/
   ```
   (crie a pasta `.cursor/rules/` na raiz do `PlataformaSaaS` se não existir)

2. Pronto. O Cursor carrega as regras automaticamente:
   - **`00-context.mdc`** → `alwaysApply: true` — sempre ativo, em toda conversa.
   - **`S01…S30.mdc`** → `alwaysApply: false` com `description` — o Cursor puxa por contexto
     (Agent-requested), ou você chama na mão com `@S03-epros-multi-tenancy`.

## O que já vem pronto vs. o que amadurece

- O **`description`** de cada regra veio do frontmatter da skill — é ele que dispara a regra
  na hora certa. Já funciona hoje.
- O **corpo** é o `SKILL.md` atual (`v1-semente`). Conforme os `EXTRACOES.md` forem promovidos
  para os `SKILL.md`, **regenere o pacote** para as regras ficarem mais profundas.

## Recomendação de adoção (não precisa das 30 de cara)

Comece pelas **Camadas 0 e 1** (S01–S15), que são as de maior uso no dia a dia. As de
especialização (S25–S30) e de fase (S16–S24) entram conforme o time formaliza cada fase.
Detalhe em `../MANUAL.md §6` (ondas) e `../CONFIGURAR-CURSOR.md`.

## Regenerar

Quando as skills mudarem, rode o gerador (`gen_cursor_rules.py`) de novo — ele reescreve
todos os `.mdc` a partir das skills atuais. Assim as regras nunca divergem das skills.
