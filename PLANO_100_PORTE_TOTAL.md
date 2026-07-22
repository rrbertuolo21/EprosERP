# Plano de Porte TOTAL (100%) — Epros → EprosERP

> Objetivo do usuário: **trazer 100% do código do legado, nada para trás**, para aposentar o Epros e ficar só com o EprosERP.
> Complementa `PLANO_EQUALIZACAO.md` (v1.3). Estas são as Ondas 6→12. Data: 03/07/2026.

## Definição de "100%" (critério de done)
1. **318/318 entidades legadas** com **todos os campos** (auditoria campo-a-campo, 0 AUSENTE crítico).
2. **98 controllers legados** (74 V1 + 24 DFe) com equivalente ou alias documentado.
3. **91 telas + a lógica dos 234 composables** do frontend legado portadas (zero stub que mente).
4. **Motor fiscal completo**: NF-e, NFC-e, **NFS-e, CT-e, MDF-e**, DANFE/DANFCE fiel, eventos (cancel/CCe/inutilização/contingência).
5. **Auth real** (Keycloak/JWT), não o token MVP.
6. **ETL de dados** pronto (long→Guid, SQL Server→PostgreSQL) + reconciliação.
7. Build 0 erros, testes verdes, migrations limpas, **warnings → 0**.

### Dependências externas (código pronto, mas precisam de artefato do usuário/ambiente)
- **Dados NCM/CEST** — vêm da base legada / IBGE-Receita (o loader é código; os dados vêm na migração).
- **Certificado SEFAZ A1 + homologação** — ambiente do usuário.
- **Templates FastReport `.frx` + DLLs do DANFE** — ausentes do repositório legado; precisam vir do deploy legado para DANFE pixel-perfect (senão fica o QuestPDF, legível mas não idêntico).

## Princípios de execução
- **Mapa-primeiro**: Onda 6 produz o De→Para exaustivo (campo/endpoint/tela/composable) — é o que garante "nada para trás".
- **Fan-out paralelo** por área disjunta (módulo/pasta), como nas ondas anteriores.
- **Gate** após cada onda: matar locks → `dotnet build` + `dotnet test` → `dotnet ef database update --no-build` no Postgres limpo.
- **Migrations serial** sempre (1 Context por vez, passe único no fim de cada onda).

---

## Ondas

### Onda 6 — MAPA EXAUSTIVO (a garantia de completude)
Auditores paralelos (read-only) produzem `docs/migracao/GAP_TOTAL_*.md`:
- 6.1 Entidades campo-a-campo (318 legadas × novo) → lista de campos AUSENTE por entidade.
- 6.2 Controllers/endpoints (98 legados × 86 novos) → endpoints faltantes.
- 6.3 Telas (91 legadas × 77 novas) → telas faltantes.
- 6.4 Composables/diálogos (234 legados × novo) → lógica de UX faltante.
- 6.5 Fiscal (NFS-e/CT-e/MDF-e, DANFE, contingência, eventos, IBPT) na Dfe.API legada.
- 6.6 Relatórios + misc (qualquer feature legada fora do acima).
Saída: **backlog definitivo** consolidado.

### Onda 7 — BACKEND 100%
- Campos faltantes das entidades (fidelidade total).
- Controllers/endpoints faltantes (relatórios, EXPORTAR_PLANILHA, dados auxiliares, etc.).
- **NFS-e / CT-e / MDF-e** — domínio + comandos + integração (motor/OpenAC do legado).
- Cleanup: enums no módulo certo (Shared), 1R.4 catálogos (base sem tenant), duplicações.

### Onda 8 — FRONTEND 100%
- Portar as ~14+ telas faltantes.
- Portar os **234 composables** (lógica de negócio/máscaras/validações/fluxos) sobre o `useApi` + tipos do OpenAPI codegen.
- Todos os diálogos/componentes; **zero flag `*_EM_IMPLEMENTACAO`** (tudo ligado).

### Onda 9 — FISCAL 100%
- **DANFE/DANFCE fiel** (FastReport com templates do legado, se disponíveis; senão QuestPDF refinado).
- **Emissão NFS-e/CT-e/MDF-e** (transmissão real via adapter, como NF-e).
- **MinIO** real para XML/PDF.

### Onda 10 — AUTH + SEGURANÇA
- **Keycloak/JWT real** substituindo o token MVP (pipeline, middleware, claims tenant).
- Testes de isolamento de tenant (RLS), pass de segurança (injection/LGPD/auditoria).
- SignalR hubs alinhados.

### Onda 11 — ETL DE DADOS (código; roda no cutover)
- Script determinístico long→Guid (mapa estável) + ETL SQL Server→PostgreSQL por tenant.
- **Loader NCM/CEST** (da base legada).
- Harness de reconciliação (contagens, somas fiscais, saldos por tenant).

### Onda 12 — GATE FINAL + PROVA DE PARIDADE
- Build + test + migrations verdes; **warnings → 0**.
- **Auditoria de paridade campo-a-campo**: 318/318 entidades, 98/98 endpoints, 91/91 telas — checklist assinado.
- Suite E2E (venda NF-e, compra XML, OFX, permissões, NFS-e/CT-e/MDF-e).
- Smoke das jornadas críticas.

---

## Ordem e paralelismo
- Onda 6 (mapa) primeiro — serial no início (consolidação), fan-out nos auditores.
- Ondas 7, 8, 9 podem correr **em paralelo** (backend / frontend / fiscal são disjuntos), cada uma com seu gate.
- Onda 10 depois (toca auth transversal). Onda 11 (ETL) independente, roda quando o schema estabilizar. Onda 12 no fim.

## Estado ao iniciar este plano (baseline v1.3)
Build 0 erros · 370 testes · 231 tabelas · paridade ~90-92% · motor fiscal cálculo+SEFAZ reais · frontend 77 telas · OpenAPI codegen montado.
