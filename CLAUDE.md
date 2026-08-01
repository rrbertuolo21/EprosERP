# CLAUDE.md — EprosERP (carregado automaticamente pela IA)

> Este arquivo é o cérebro do projeto para o Cursor/Claude Code. **Leia antes de qualquer tarefa.**
> Onboarding humano: [COMECE-AQUI.md](COMECE-AQUI.md). Processo completo: `docs/fabrica/`.

## O produto em uma frase
ERP SaaS multi-tenant. **Monólito modular** .NET 8 (Clean Arch por módulo, **CQRS/MediatR**,
**PostgreSQL + EF Core**, RLS multi-tenant, Outbox, **ABAC** `[AbacAuthorize]`, Flunt) + front
**Nuxt 3 + TypeScript** (`Epros.App`, IO só via `useApi`/`useApiList`). Mobile React Native (submódulo).

## Estrutura (achatada, deploy-ready)
`src/` backend · `Epros.App/` front · `scripts/` · `infra/` · `docs/` (incl. `fabrica/`) ·
`docker-compose.{local,prod}.yml` · `tests/` · `Epros.sln`. Módulos em `src/Modules/Epros.Modules.<X>/`.
Diário: `HISTORICO-DESENVOLVIMENTO-IA.md`.

## Rodar local (1 comando + seed)
```bash
docker compose -f docker-compose.local.yml up -d --build && ./seed-local.sh
# se o BuildKit der DeadlineExceeded: DOCKER_BUILDKIT=0 docker compose -f docker-compose.local.yml build && docker compose -f docker-compose.local.yml up -d
```
Front http://localhost:3000 · API/Swagger http://localhost:8080/swagger · Admin `admin@epros.local/Admin@12345` · Cliente `cliente@demo.local/Cliente@12345`.

## Disciplinas inegociáveis (o que faz "1 dev virar 15")

1. **O "verde" é entrada, não prova.** Nunca reporte "build passou/testes verdes" sem **re-executar**
   você mesmo e colar a **evidência** (comando + saída). *"Build verde" ≠ "funciona"* — valide no
   **ambiente vivo** (banco real, chamada externa real): pode faltar migration, ter URL hardcoded, ou
   faltar adaptador de integração. Detalhe: `docs/fabrica/processo/RETROSPECTIVA-EPROSERP.md`.
2. **Produção paralela = fan-out por pasta disjunta.** Muitas telas/módulos? 1 agente por pasta
   exclusiva (zero colisão), gargalo serial (scaffolding) primeiro, **prova o molde** antes de
   multiplicar, contrato fixo entre paralelos. Ver `docs/fabrica/skills/fan-out-paralelo.md`.
3. **Fidelidade campo-a-campo** à EF/legado. Nada fakeado. Porte fecha só com **migration aplicada +
   CRUD completo por raiz** (GET/{id}, PUT, DELETE) + detalhe. Ver `docs/fabrica/skills/armadilhas-de-porte.md`.
4. **⛔ Negócio vem SEMPRE da skill de negócio.** Qualquer regra fiscal/tributária/trabalhista/financeira
   (NF-e/NFC-e/CT-e/MDF-e/NFSe/SPED/eSocial, imposto/INSS/alíquota/prazo) **obriga** carregar e citar a
   skill de negócio antes de agir. Responder de memória = violação. Skill vazia → pare e peça validação
   humana; **nunca invente a regra.** Fiscal fica **travado** até validação de contador.
5. **Módulos sobem DESABILITADOS** (ABAC nega por padrão) — liberar por plano/cliente.
6. **Convenções de código:** `CONVENCAO_CODIGO.md` (Modo Porte vs Consolidação) + regras `.mdc` do
   Cursor em `docs/fabrica/cursor/cursor-install/rules/`.

## Armadilhas de ambiente (não trave nelas)
Sem `node` no PATH → o front valida via Docker (`nuxt generate`), typecheck (`nuxi typecheck`) na sua
máquina. `dotnet` → `export DOTNET_ROOT=~/.dotnet`. BuildKit `DeadlineExceeded` → `DOCKER_BUILDKIT=0`.
`--no-deps` rebuilda api/web sem derrubar o banco. Ver `docs/fabrica/skills/docker-deploy-armadilhas.md`.

## Onde está o processo (para a IA carregar)
- Agentes (Dev, QA, Arquiteto, Segurança, Code Review, Eng. Reversa, Context): `docs/fabrica/agentes/`
- Processo/gate/fan-out: `docs/fabrica/processo/` · Skills destiladas: `docs/fabrica/skills/`
- Retomada de onde paramos: `PROMPT_REINICIO.md` / `HISTORICO-DESENVOLVIMENTO-IA.md`

## Git
Commit/push só quando pedido; nunca na `main` sem branch. Manter **paridade** com `origin/main`
(fast-forward). Mensagem de commit termina com a linha de coautoria.
