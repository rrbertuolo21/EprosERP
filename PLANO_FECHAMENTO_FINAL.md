# PLANO DE FECHAMENTO FINAL — EprosERP → 100% (uma passada, sem retrabalho)
> Baseado no `docs/migracao/LOG_COMPLETUDE.md` (auditoria de 8 eixos, 05/07/2026). Objetivo: aposentar o Epros. Cobre os 7 pontos do usuário: paridade visual 100%, novos recursos alocados na tela, conformidade arquitetural, testes/docs/comentários, manutenção fácil.

## Princípios
- **Ordenar por dependência**, não por facilidade. Segurança e decisões de rumo primeiro (mudam o resto).
- **Gate por frente:** matar locks → `dotnet build` (meta 0 warning ao fim) → `dotnet test` → `dotnet ef database update --no-build`. Migrations **sempre serial**, rebuild entre `add` e a próxima op EF.
- **Fan-out por módulo disjunto**; Fiscal é gargalo serial (1 dono do ContextFiscal).
- **Definição de done por item = código + teste + doc/comentário** (senão não conta como fechado). Nada de stub que mente.

## Pré-requisito: 2 decisões do usuário (ramificam o plano)
- **DEC-1 Rumo visual:** A) clone fiel do legado · B) rebrand assumido. → afeta **F2**.
- **DEC-2 8 módulos novos:** A) finalizar · B) quarentenar. → afeta **F7**.

---

## FRENTES (ordem de execução)

### F1 — SEGURANÇA / AUTORIZAÇÃO (P0, bloqueia produção) — primeiro
Refs LOG: S1–S4, Q5, Q6.
1. Definir `FallbackPolicy = RequireAuthenticatedUser` **ou** `[Authorize]` numa `BaseAuthorizedController`; `[AllowAnonymous]` explícito só em login/health/webhook.
2. `InquilinoSaaSMiddleware`: **remover** o fallback `tenantId="tenant-padrao"` em produção (manter só em `IsDevelopment`); rejeitar 401 quando não houver tenant/usuário resolvido.
3. Portar o **RBAC por-menu** do legado (`PodeLer/PodeIncluirAlterar/PodeDeletar`) como **authorization policy/behavior** ligado ao `PerfilAcesso` (owner GestaoClientes) — um `IAuthorizationBehavior` no pipeline MediatR ou policies por controller.
4. Substituir o token não-JWT concatenado; alinhar com Keycloak real (Authority válido) OU endurecer o token MVP com assinatura/expiração até o Keycloak entrar.
5. Corrigir **Q5** (`AccountController`: tirar DbContext/regra inline → handler) e **Q6** (Outbox do Financeiro no schema próprio + `ExcludeFromMigrations`).
6. **Testes:** isolamento de tenant (RLS), 401 sem token, 403 sem permissão de menu.
- *Gate:* build+test verdes; smoke: request anônima → 401.

### F2 — PARIDADE VISUAL (o crux; depende de DEC-1) — a maior frente
**Se DEC-1 = A (clone fiel):**
1. **Design tokens → legado:** tema **claro por padrão**, primária `#14325a`, fonte **Manrope**, raios/sombras/espaçamentos estilo Material. (mantém o toggle escuro como extra).
2. **Componentes de base** reescritos p/ aparência Material: `DataTable` (colunas/densidade/paginação server), `AppDialog`, botões, inputs, **Sidebar** (cinza claro + ícones Tabler, menu vindo do backend `acessos`), **Header** (branco + dropdown usuário).
3. **Ícones:** trocar emoji por set Tabler SVG (reusar `AppIcon`).
4. **Regressão tela-a-tela** contra o legado (checklist do `03_FRONTEND_VISUAL.md`), lado a lado, antes do cutover.

**Se DEC-1 = B (rebrand assumido):** só (1) tema claro por padrão + (2) documentar o rebrand + (3) banner de "novo visual". Pula 2–4.

**Gaps funcionais de tela (valem em A e B):** CFOP 19 colunas (V5); NF-e Entrada com pagamentos/transporte/cálculo manual (V6); **Devolução de entrada** (V7, criar página); **Perfis de Acesso detalhe** (V8, consertar render em branco); **Certificado upload A1/A3** (V9, depende de F3-F6); **PDV teclas F** (V10).

### F3 — FISCAL: pontas de código (Fiscal serial) — refs F1–F10 do LOG
1. **NFC-e QR Code** (F1, legal) — add lib de barcode/QR ao `.csproj`, desenhar QR no cupom.
2. **CertificadoDigital** (F6) — entidade + upload A1 + validade + CRUD + ligar ao provider fiscal (destrava V9).
3. **IBPT** (F5) — entidade/tabela de alíquotas + loader + endpoints `obter-aliquotas-por-ncm-uf`.
4. **`salvar-nf-com-xml`** (F3) — importar NF-e/NFCe autorizada + registrar cancelamento por XML externo.
5. **Compat de emissão single-shot** (F2) — endpoint que aceite o DTO fiscal completo num tiro, orquestrando o agregado internamente (preserva PDV/integrações). *(confirmar se algum dos 20 clientes usa integração externa antes de priorizar)*.
6. **Download por localizadorExternoId** (F10) — restaurar as rotas PDV/ByFood.
7. **Contingência** (F7) — `tpEmis`/SVC/EPEC. **DANFE Code128** (F8). **Confirmar cStat 573** no handler de cancelamento (F9).
8. **NFS-e adapter real** (F4) — OpenAC.Net.NFSe (também é homologação; entra aqui o código).
- *Gate:* migration serial p/ CertificadoDigital/IBPT; build+test.

### F4 — ENDPOINTS: paridade fora do núcleo — refs E1–E7
1. **Loaders de tabela** (E1) — restaurar `POST .../atualizar` (import arquivo) de NCM/CFOP-padrão/CódServiçoSefaz/FCP/IBPT (resolve também a carga NCM/CEST).
2. **Resolução CST por NCM+modelo** (E2) — endpoints de emissão IBS/CBS.
3. **Regras em GET** (E3) — CFOP por regime/tipo-operação; endpoints de enum condicionais (`csosn-nfce`, IPI/PIS entrada×saída) fora do genérico.
4. **DELETEs faltando** (E4); **confirmar shape** `totalRegistros/totalPaginas` no `CommandResult` (E5); paginação vs cache (E6).

### F5 — ENTIDADES: semântica/tipagem — refs D1–D6
1. `Venda.ModeloFiscal/ModalidadeFrete/VendaOrigem` `int?` → **enum** (D1). `PaisId` `long`→`Guid` (D4). `Status` string → enum nas 7 do GestaoClientes (D5).
2. Reavaliar agregados obrigatórios da Compra (D2) e endereços dissolvidos (D3) — confirmar que não perde múltiplos endereços por pessoa.
3. Documentar mapeamento de renames p/ o ETL (D6).
- *Gate:* migrations serial.

### F6 — LÓGICA / VALIDAÇÕES no front — refs L1–L7
1. Criar **módulo central `rules`** (L5) portando `useDocumento` do legado.
2. Telefone 9º dígito (L1), SEM GTIN (L2), endereço principal (L3), CNPJ alfanumérico (L4), validações de estoque (L6), arredondamento 4 casas na NF-e (L7).

### F7 — 8 MÓDULOS NOVOS (depende de DEC-2) — refs B7
- **Se A (finalizar):** por módulo — `string→enum` (máquinas de estado), testes de handler, e **alocar tela** (grupo novo no menu, telas CRUD) na prioridade Produção/Qualidade/RH → Manutenção/Projetos → DMS/GRC/ESG.
- **Se B (quarentenar):** feature-flag/ocultar do menu, marcar `@beta`, congelar até pós-cutover. Mantém build verde sem investir tela/teste agora.

### F8 — QUALIDADE / TESTES / DOCS (item 7) — contínua, refs Q1–Q4
1. **Warnings → 0:** pass de nullable no `External.DfeCalculos` (mata ~766) + demais projetos (Q1).
2. **Testes:** cobrir NFS-e/CT-e/MDF-e/ImportarCompraXml, e subir Venda de smoke → regra (RegistrarVenda/CancelarVenda/VendaFiscalHandlers) (Q2).
3. **Docs:** `///` em controllers/handlers públicos + **README por módulo** (Q3).
4. **Manutenibilidade:** quebrar god-files (LandingPageSettingsHandlers, ContextEstoque, VendaFiscalHandlers) (Q4).

### F9 — GATE FINAL + PROVA DE PARIDADE
- Build **0 erros / 0 warnings**, `dotnet test` verde, migrations limpas em banco zerado.
- **Prova de paridade:** checklist assinado — entidades 165/165 campo-a-campo, endpoints legado×novo, telas legado×novo (regressão visual).
- **E2E:** venda NF-e + NFC-e (com QR), compra XML, OFX, permissões (401/403), NFS-e/CT-e/MDF-e.
- Só então: **ETL de dados** (long→Guid, SQL→Postgres, loader NCM/CEST) + reconciliação → cutover.

---

## Ordem e paralelismo
```
F1 (segurança)  ─┐  primeiro, sozinho (transversal)
                 ▼
F2 visual ═══════╗  (DEC-1) — maior frente, roda em paralelo com backend
F3 fiscal ═══════╣
F4 endpoints ════╬═ backend, disjuntos por módulo (Fiscal serial)
F5 entidades ════╣
F6 validações ═══╝  (front, disjunto de F2)
F7 módulos (DEC-2) ═ paralelo, isolado
F8 qualidade ════════ contínua, acompanha cada frente
                 ▼
F9 gate final → ETL → cutover
```

## Dependências externas (código pronto; artefato do usuário/ambiente)
- Certificado A1 + homologação SEFAZ por UF · CSC/Id-token NFC-e · credenciais municipais NFS-e · dados NCM/CEST (o **loader** entra em F4). Templates FastReport **não** necessários (decisão QuestPDF).

## Definição de "100%"
Segurança imposta · visual conforme DEC-1 · NF-e/NFC-e(QR)/NFS-e/CT-e/MDF-e completos · endpoints e validações em paridade · 8 módulos conforme DEC-2 · 0 warning · testes verdes com cobertura das regras críticas · doc/README · ETL provado. **Sem stub que mente.**
