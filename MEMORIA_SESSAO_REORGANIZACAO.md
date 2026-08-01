# Memória de Sessão — Reorganização EprosERP

> **Criada:** 2026-08-01 · **Executada:** 2026-08-01.  
> **Uso:** referência do que foi reorganizado. Retomada diária: `HISTORICO-DESENVOLVIMENTO-IA.md` + `CONSOLIDACAO-GAPS.md` + `MEMORY.md`.

---

## Mudanças aplicadas (2026-08-01)

| Ação | Detalhe |
|---|---|
| `_fabrica/` → `docs/fabrica/` | Agentes, processo, skills, cursor/rules |
| `seed-local.sh` → `scripts/seed-local.sh` | Comando: `./scripts/seed-local.sh` |
| Histórico arquivado | `docs/historico/` — planos REG-030, equalização, memórias antigas |
| Migração | `docs/migracao/PADRAO_PORTE_LEGADO.md`, `migracao-epros-eproserp.md` |
| Ops | `docs/ops/INSTALACAO_LOCAL.md` (alinhado ao compose local) |
| Raiz limpa | Canônicos + HISTORICO + CONSOLIDACAO-GAPS + MEMORY + preferências IA |
| Atualizados | MEMORY.md, PROMPT_REINICIO.md, CONVENCAO, docs/README; onboarding → `docs/ROTEIRO-ONBOARDING.md` + `docs/QUICKSTART-LOCAL.md` |

---

## Raiz — o que ficou

**Runtime:** `docker-compose*.yml`, `Epros.sln`, dotfiles, `.config/`  
**Onboarding:** `docs/ROTEIRO-ONBOARDING.md` · `docs/QUICKSTART-LOCAL.md`  
**Canônicos raiz:** `CLAUDE.md`, `CONVENCAO_CODIGO.md`  
**Backlog vivo:** `HISTORICO-DESENVOLVIMENTO-IA.md`, `CONSOLIDACAO-GAPS.md`  
**IA:** `MEMORY.md`, `PROMPT_REINICIO.md`, `preferencia-paralelismo-maximo.md`, `feedback-nao-pedir-permissao.md`

---

## Estrutura `docs/`

```
docs/
├── fabrica/       agentes, processo, skills, cursor/rules .mdc
├── onboarding/    trilha humana
├── migracao/      DePara, GAPs, PADRAO_PORTE, narrativa migração
├── historico/     planos/memórias arquivados
├── ops/           INSTALACAO_LOCAL (detalhe)
├── orquestracao/  mapa mestre
└── processos/     endpoints por ambiente
```

---

## Rodar local (canônico)

```bash
docker compose -f docker-compose.local.yml up -d --build && ./scripts/seed-local.sh
```

Front :3000 · API/Swagger :8080 · admin `admin@epros.local` · demo `cliente@demo.local`

---

## Pendência opcional

- Reinstalar rules Cursor: `cp docs/fabrica/cursor/cursor-install/rules/*.mdc .cursor/rules/`
- `.gitignore`: garantir `!.env.production.example` tracked se necessário
