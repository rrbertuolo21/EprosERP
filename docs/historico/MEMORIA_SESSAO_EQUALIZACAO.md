# Memória de Sessão — Equalização EprosERP (handoff para reinício)

> **Objetivo deste doc:** permitir retomar o trabalho em um chat NOVO sem perda de contexto. Última atualização: 03/07/2026.
> Complementa: `CONVENCAO_CODIGO.md` (regras canônicas), `PADRAO_PORTE_LEGADO.md` (molde), `PLANO_EQUALIZACAO.md` (v1.3), `PLANO_100_PORTE_TOTAL.md` (Ondas 6-12, objetivo 100%).

---

## 0. ⭐ ESTADO ATUAL (05-jul) — AUDITORIA DE FECHAMENTO CONCLUÍDA (LER PRIMEIRO)

**🔴 VIRADA (05-jul):** usuário sentiu que estávamos "patinando nos 15% finais" e mandou PARAR de entregar e AUDITAR a fundo. Rodei 8 auditores céticos read-only → **`docs/migracao/LOG_COMPLETUDE.md`** (fonte única do que falta) + **`PLANO_FECHAMENTO_FINAL.md`** (9 frentes, 1 passada). Detalhe por eixo em `docs/migracao/completude/0X_*.md`.
**Diagnóstico:** o "99,3%" media campo-na-entidade (correto: domínio 94,5%, espinha fiscal 100%). Os gaps REAIS estão fora disso: **(1) VISUAL ~0% de paridade** — o front novo é um REBRAND completo (escuro/indigo/Plus Jakarta/glass vs legado claro/navy #14325a/Manrope/Vuetify); item 4 do usuário exige "cliente não sentir a mudança". **(2) SEGURANÇA: API aberta** — `Program.cs` `AddAuthorization()` sem FallbackPolicy, ZERO `[Authorize]`, `InquilinoSaaSMiddleware.cs:44-47` faz anônimo→`tenant-padrao` e segue; RBAC por-menu do legado não portado. **(3) FISCAL pontas:** NFC-e SEM QR (ilegal), IBPT sem tabela, NFS-e/CT-e/MDF-e fallback, emissão virou multi-passo (quebra PDV stateless), `salvar-nf-com-xml`/CertificadoDigital ausentes. **(4) ESCOPO:** 8 módulos NOVOS (Produção/Qualidade/RH/Manutenção/Projetos/DMS/GRC/ESG) sem tela e sem teste. **(5) QUALIDADE:** 467 warnings (766 no External.DfeCalculos), 0 teste em NFS-e/CT-e/MDF-e, doc fraca.
**DECISÕES TOMADAS (usuário mandou "Siga" + "não me pergunte mais"):** DEC-1 = **clone fiel só nas telas core** (dia a dia) + tema claro default; moderno tolerado no resto. DEC-2 = **quarentenar os 8 módulos** (já eram API-only, nunca tiveram menu — quarentena era fato).

**RODADA DE FECHAMENTO 1 EXECUTADA (05-jul) — F1/F3/F5/F6/F7 pousaram:**
- **F1 Segurança** ✅: API fechada por padrão (`Program.cs` FallbackPolicy `RequireAuthenticatedUser` + `Security/EprosTokenAuthenticationHandler.cs` que valida o token estruturado real `jwt-token-basico/completo-...` — NÃO é Keycloak). 401 sem credencial; `tenant-padrao` só em Development; `[AllowAnonymous]` em login/public/webhooks. RBAC por-menu JÁ existia (`Security/PermissaoMenuFilter.cs`+`PermissaoMenuAttribute.cs`, 16 controllers). AccountController afinado (SessaoQueries). ContextFinanceiro OutboxMessage=consumidor do outbox do Estoque → `ExcludeFromMigrations` (NÃO mover schema, quebra leitura cross-módulo). D4 `PessoaVeiculo.PaisId` long→Guid+FK. D5 Status string→enum em 5 entidades (`HasConversion<string>()`, coluna varchar, valores=nomes legados). **381 testes verdes.**
- **F3 Fiscal** ✅: NFC-e QR (QRCoder, lê infNFeSupl do XML, sem fabricar hash); Code128 DANFE; entidade `Ibpt` (IGlobalEntity)+endpoint `ibpt-dfe/obter-aliquotas-por-ncm-uf`; `salvar-nf-com-xml`+`registrar-cancelamento-xml`; cStat 573 no CancelarDocumentoFiscalCommandHandler. Certificado A1 JÁ existia completo no GestaoClientes (`EmpresaCertificado`); só reforçou rejeição de vencido. TODO honesto: contingência (tpEmis/SVC/EPEC) não feita; NFS-e real segue NaoConfigurado (ambiente).
- **F5** ✅ Venda `ModeloFiscal/ModalidadeFrete/VendaOrigem` int?→enum (sem migration). **F6** ✅ `composables/useRules.ts` + telefone 9º dígito + CNPJ alfanumérico + SEM GTIN + validações estoque + arred 4 casas NF-e. **F7** ✅ quarentena (só nota em menu.ts; 8 módulos nunca tiveram tela).

**ESTRUTURA CONFIRMADA (07-jul):** 14 módulos, TODOS no padrão hexagonal idêntico (`Domain/{Entities,Enums}` · `Application/{Commands,Handlers,Queries,Services}` · `Infrastructure/Data` · `Migrations`). 257 entidades (núcleo legado 232 em GestaoClientes 68/Estoque 54/Vendas 37/Fiscal 36/Aplicativo 22/Financeiro 15; +25 nos 8 módulos novos). 90 controllers na camada única `src/API/Epros.API/Controllers`. Motor legado isolado em `src/External` (reuso). **Nada ficou no formato antigo.** Conformidade de padrão: pilares 100% (EntidadeSaaSBase/Guid/CQRS/thin controllers/schema-por-módulo/decisões canônicas); dívida só de refinamento (string→enum nos módulos novos).

**✅ GATE RODADA-1 VERDE E VERIFICADO PELO LEAD (07-jul):** build **0 erros/0 avisos**, **381 testes** aprovados, as 3 migrations (`Fechamento_Fiscal_Ibpt`, `Fechamento_GestaoClientes_PaisIdGuid_Status`, `Fechamento_Financeiro_OutboxExclude`) **aplicadas no Postgres** — `pessoas_veiculos.pais_id` já é `uuid`, tabela `ibpt` existe (bigint→uuid passou limpa). **Backend DEPLOY-READY.** Tokens visuais (tema claro default + navy #14325a + Manrope) aplicados pelo lead em `useTheme.ts`+`assets/css/main.css`, confirmado no preview.

**F4 NÃO entrou** (loaders NCM/CFOP/serviço/FCP/IBPT `atualizar` por arquivo; resolução CST por NCM+modelo `obter-por-cst`/`obter-ncm-classificados`; DELETE em CFOP/TipoOperacaoFiscal/CodigoBeneficioFiscal) → **ajuste fino pós-validação** (loaders ligam ao carregamento NCM/CEST que é ambiente; emissão real via Hercules não depende deles).

**✅ FRONTEND FECHADO + DEPLOY-READY (07-jul):** F2 pousou. Achado raiz: `app.vue` chamava `<NuxtPage/>` SEM `<NuxtLayout>` → sidebar/header NUNCA montavam (metade dos "AUSENTE" do audit era isso). Corrigido + 8 páginas self-contained com `layout:false`. Componentes Material claro (DataTable/modais/botões navy sólidos), ícones Tabler SVG (fim dos emojis), CFOP 7→19 colunas, NF-e entrada (pagamentos/transporte cards), perfis-detalhe consertado (PUT faltava `/{id}` + estados loading), PDV teclas F. `typecheck` 0, `npm run build` limpo, console SEM erros, login renderiza no visual claro/navy.
**GOTCHA (importante):** tela-em-branco no preview era **cache `.nuxt` corrompido** após sessão gigante de HMR (erro `Error matching route rules ... reading 'entries'` no radix3) — NÃO é bug. Fix: parar dev + `rm -rf EprosApp/.nuxt` + reiniciar. Produção (`npm run build`) gera do zero, não afetada.

**🏁 FECHAMENTO COMPLETO (07-jul) — F1..F9 TODOS ✅.** O usuário mandou finalizar F4/F5/F8/F9 (não adiar). Feito por 4 agentes disjuntos + gate do lead:
- **F4 Fiscal:** loaders `POST .../atualizar` (NCM/CFOP-padrão/CódServiçoSefaz/FCP/IBPT, IFormFile), resolução CST/cClassTrib por NCM+modelo (novo controller `classificacoes-tributarias`: obter-por-cst/{cst}/{modelo}, obter-por-ncm-lst, obter-ncm-classificados), DELETE em CFOP/TipoOperacaoFiscal/CodigoBeneficioFiscal. +25 testes. Sem migration (reusa entidades).
- **F5 D2/D3:** endereços SEM perda (múltiplos vivem em Pessoa.Enderecos 1:N; Empresa sempre foi 1:1 — auditoria supôs N:N errado). Compra Configuracao/Entrega mantidos OPCIONAIS (legado nunca exigia; obrigar quebraria LancarCompra). Sem migration.
- **F8 Qualidade:** **warnings 467→0** na solução inteira (DfeCalculos 396→0, o maior foco; via `= null!`/guards/`?`, sem #pragma em massa, sem introduzir NRE). `LandingPageSettingsHandlers` 1759→121 linhas (código morto). **+72 testes** (total 453), XML-doc + README em todos os módulos.
- **F9 GATE FINAL (lead verificou):** `dotnet build Epros.sln` = **0 erros / 0 avisos**; `dotnet test` = **453 aprovados / 0 falhas**; `has-pending-model-changes` = "No changes" em Fiscal/GestaoClientes/Estoque/Vendas (schema em sincronia). Migrations já aplicadas no Postgres.

**FALTA SÓ A PARTE HUMANA:** 1) usuário SOBE (build limpo + API .NET de pé — gotcha: `rm -rf EprosApp/.nuxt` se der tela branca em dev). 2) negócio valida 1 semana. 3) **ETL do cutover** (long→Guid, SQL Server→PostgreSQL, loader NCM/CEST alimenta as tabelas via os loaders da F4) — roda NA VIRADA com dados de produção. Homologação SEFAZ + certificado + NFS-e transmissão real = ambiente. Checklist em `DEPLOY_READINESS.md`.

**FALTA do PLANO_FECHAMENTO (próximas rodadas, usuário quer ASAP):** F4 endpoints (loaders NCM/CFOP/serviço/FCP/IBPT import + resolução CST por NCM+modelo + DELETEs + shape totalRegistros) · F8 qualidade (warnings→0 começando por External.DfeCalculos 766; testes NFS-e/CT-e/MDF-e/venda; XML-doc; READMEs) · D2/D3 (agregados Compra, endereços dissolvidos) · F9 gate final + E2E + ETL. Depois do gate rodada-1: disparar F4+F8 (backend).

**Objetivo do usuário:** trazer 100% do código do legado, NADA para trás, para APOSENTAR o Epros. Plano em `PLANO_100_PORTE_TOTAL.md` (ondas) + `PLANO_FECHAMENTO_FINAL.md` (fechamento pós-auditoria).

**Baseline verificado (05-jul, pós-gate Onda 7):** `dotnet build` 0 erros · **370 testes verdes** · **235 tabelas** aplicam limpo · paridade ~90-92%. Hash de senha (PBKDF2) e Swagger tipado OK. OpenAPI codegen montado (`npm run api:generate` → types/api.d.ts).

**Onda 6 (mapa exaustivo) CONCLUÍDA** → docs em `docs/migracao/GAP_TOTAL_*.md`. Achado que muda tudo: **domínio 99,3% de campos** (os "318 arquivos" eram 159 reais + 159 lixo `._*` macOS; núcleo fiscal 100% campo-a-campo; único gap real = CertificadoDigital.CaminhoCompleto). **Telas 100%.** O backlog REAL é só **API (~94 endpoints, 79%)**, concentrado em: (1) NFS-e/CT-e/MDF-e = 0% portados; (2) família download/impressão DFe ~22 rotas (download PDF/XML/cancelamento/CCe/documentos-contador, verificar-status/consultar-protocolo/inutilizar); (3) ações produto/venda/compra (reajustar/importar-csv/duplicar/gtin/entrada-propria/cupom-nao-fiscal/nfe-simplificado/danfe-sem-autorizacao); (4) seed-"atualizar" + relatório simplificado01 + misc.

**ONDA 7 POUSOU (04-jul):** Fiscal = 15 rotas download/impressão DFe + **NFS-e + CT-e + MDF-e** (entidades NotaServicoEletronica, ConhecimentoTransporteEletronico, ManifestoEletronicoDocumentosFiscais no ContextFiscal schema 'plataforma'; DbSets+mappings prontos; fallback honesto — transmissão real = homologação). Estoque = ações produto/compra. Vendas = ações venda. Fiscal buildou 0 erros isolado. Transmissão real NFS-e(OpenAC.Net.NFSe)/CT-e/MDF-e(Zeus/Hercules) → Onda 9. download-pdf-cancelamento/cce servem XML do evento (sem FastReport).

**GATE ONDA 7 CONCLUÍDO (05-jul) — TUDO VERDE:**
- Build solução: 2 erros de teste (fakes desatualizados p/ `IHerculesFiscalService.VerificarStatusServicoAsync/ConsultarProtocoloAsync` e `IEmitenteFiscalProvider.ObterContextoPorDocumento`) → **corrigidos** em `tests/Epros.Tests/FiscalTests.cs` (`TestHerculesFiscalService`) e `tests/Epros.Tests/VendaOutboxIntegrationTests.cs` (`EmitenteFiscalProviderNulo`). Build final: **0 erros**.
- Migration gerada: **`Onda7_Fiscal_NfseCteMdfe`** (ContextFiscal) — só `CreateTable` das 3 entidades novas (schema plataforma) + índices, sem drop/alter. Revisão limpa. Aplicada com `dotnet ef database update` sem erro.
- **BUG achado e corrigido:** `ContextEstoque.ServicoLookup` (mapeia `Servico`, dono real = Fiscal) estava **sem `ExcludeFromMigrations()`** → gerava migration espúria tentando recriar a tabela `plataforma.servicos`. Corrigido em `src/Modules/Epros.Modules.Estoque/Infrastructure/Data/ContextEstoque.cs` (`entity.ToTable("servicos", "plataforma", t => t.ExcludeFromMigrations())`). Após o fix, Estoque não tem pending changes (as ações produto/compra portadas não mudam schema).
- ContextVendas: sem pending changes (ações venda não mudam schema).
- Testes: **370/370 verdes** (baseline mantida).
- Tabelas aplicadas: **235** (plataforma 103, estoque 55, vendas 38, financas 15, aplicativo 23, public 1).
- Gotcha novo confirmado: `dotnet ef migrations add/remove/has-pending-model-changes --no-build` usa o DLL já compilado — se você rodar `migrations add` e depois `migrations remove`/`has-pending-model-changes` SEM rebuildar entre eles, o comando reflete contra o assembly desatualizado e dá respostas inconsistentes (ex.: "migration already applied" errado, ou mostra migration já removida do disco). **Sempre rebuildar o projeto (startup+module) entre `migrations add` e a próxima operação EF.**

**ONDA 8 (polish frontend) PARCIAL POUSOU (05-jul):** (A) visual/shell — `AppLogo.vue`+`AppIcon.vue` criados; login/cadastro/recuperar unificados no layout guest com fundo único; removidos banner "Modo Simulação"/campo Tenant/rodapé técnico (auth MVP mantida no estado, só não exposta); 4 layouts quebrados corrigidos (empresas index/[id], certificado, nfe-simplificada); home acesso-rapido com saudação+empresa ativa+ícones SVG; Fornecedores/Clientes com rotas distintas (?tipo=). (B) 7 composables de fluxo criados (useNfeTransmissao/usePagamentos/useVendaAcoes/useDownloadDfe/useNfse/useCte/useMdfe/useImportacaoXml) + 15 aux da rodada anterior. (C) relatório simplificado01 portado (`GET vendas-fiscal/relatorios/simplificado01`). **Flag inutilização ligada** (backend `GET inutilizacao-dfe` já existia; corrigido carregarLista p/ envelope paginado, removida flag stale). typecheck 0, build ok, login verificado no preview. **Resta 1 stub frontend:** export planilha. Onda 8 falta: ligar telas às rotas novas de download/NFS-e/CT-e/MDF-e usando os composables novos.

**▶️ RETOMA (100%):** 1) Onda 7 GATE fechado — próximo é **Onda 7-round2** (seed-atualizar + relatórios + o que faltou) → Onda 8 (frontend: ligar downloads/NFS-e + composables aux) → Onda 10 (Keycloak real) → Onda 11 (ETL dados long→Guid + loader NCM/CEST) → Onda 12 (gate final + prova paridade 318/318, 98/98, 91/91 + E2E + warnings 0). 2) Migrations SEMPRE serial + rebuild entre add/remove/has-pending (ver gotcha acima). 3) Fan-out por módulo disjunto (Fiscal é gargalo serial — 1 dono do ContextFiscal por vez).

**DEPS EXTERNAS (código pronto, artefato do usuário):** NCM/CEST dados (base legada/IBGE, vêm na migração), certificado SEFAZ (homologação), templates FastReport `.frx` (ausentes do repo — precisam do deploy legado p/ DANFE pixel-perfect; hoje usa QuestPDF legível).

**GOTCHA recorrente:** `dotnet ef database update` dá "Build failed" por lock de processo → build limpo 1x + `--no-build`. Agentes concorrentes durante build/test = falha transitória → re-rodar parado. "N processos travados" que o usuário vê = chips de UI de agentes já concluídos (nada rodando de fato).

---

---

## 1. Identidade e caminhos
- **Legado (fonte da verdade):** `../Epros/epros_erp-main` (backend .NET 8/SQL Server), `../Epros/epros_erp_front-main` (front Nuxt3+Vuetify).
- **Novo (destino):** `EprosERP/` — backend .NET 8 modular (CQRS/MediatR/EF Core/PostgreSQL), frontend `EprosApp/` (Nuxt 3 SPA, CSS custom **sem Vuetify**), motor fiscal legado copiado em `src/External/` (Epros.ERP.DfeCalculos + Epros.ERP.Shared).
- **Solução:** `Epros.sln` (raiz). Banco dev: PostgreSQL em Docker (container `epros-postgres`, db/user/pass `epros`/`epros`/`epros_dev_password`, localhost:5432).

## 2. Estado ATUAL verificado
- `dotnet build Epros.sln` = **0 erros** · `dotnet test` = **367 testes verdes** (0 falhas).
- **Schema ↔ código reconciliados** (Fase D concluída): 6 migrations `PortEqualizacao*` aplicaram limpo em banco zerado; `has-pending-model-changes` = "No changes". 228 tabelas (plataforma 99/estoque 53/vendas 38/financas 15/aplicativo 23).
- **Frontend** `EprosApp`: 63 telas, `nuxi typecheck` 0, `npm run build` ✨ ok. Tema claro/escuro com toggle (sol/lua) + login estilo legado (split-screen) implementados.
- Paridade: domínio ~98%, ponderada **~88-90%**.

## 3. ✅ Fixes críticos desta sessão — CONCLUÍDOS E VERIFICADOS
1. **HASH DE SENHA (era crítico) — RESOLVIDO.** Era bug: senha salva em texto puro (`password_hash`="123"). Implementado `IPasswordHasher` (`src/Shared/Epros.Shared/Application/Contracts/IPasswordHasher.cs`) + `Pbkdf2PasswordHasher` (`src/Infrastructure/.../Services/`, PBKDF2-SHA256, 100k iter, salt 16B, formato `pbkdf2.sha256.<iter>.<salt>.<hash>`, verify FixedTimeEquals). DI Singleton no Program.cs. Hash na criação (CriarUsuario/RegistrarNovoTenant/ExecutarInstalacao/CriarUsuarioInterno) e troca (Alterar*Senha*); Verify no login (AutenticarUsuario). Removido `Usuario.ValidarSenha` (texto puro). Teste `PasswordHasherTests`. **Grep confirma 0 senha crua.** Nota: usuários semeados com senha crua param de logar (correto; resemear).
2. **SWAGGER — tipagem de retorno — RESOLVIDO.** Todos os controllers ganharam `[Produces("application/json")]` + `[ProducesResponseType(typeof(CommandResult),...)]` e muitas actions viraram `ActionResult<CommandResult>` (rule: só quando todo retorno é CommandResult). XML actions (ObterXml/DownloadXml) documentadas como application/xml. **Grep confirma 0 controllers sem ProducesResponseType.**

Ambos verificados juntos: build 0 erros, **367 testes 0 falhas**. (Nada rodando no fim da sessão.)

## 4. ▶️ RETOME AQUI (primeiro passo no chat novo)
Estado ao encerrar: **tudo verde e verificado** (build 0, 367 testes, schema reconciliado, hash+swagger prontos). O próximo trabalho é a **Onda 5 (§5)**. Rode primeiro este sanity rápido; se verde, siga pra Onda 5:
```bash
cd EprosERP
# 1) liberar lock se a API estiver rodando local
dotnet build-server shutdown
#   (Windows) matar API: PowerShell Get-CimInstance Win32_Process | ?{$_.CommandLine -match 'Epros\.API'} | %{Stop-Process -Id $_.ProcessId -Force}
# 2) build+test combinado (valida hash de senha + swagger juntos)
dotnet build Epros.sln -nologo
dotnet test tests/Epros.Tests/Epros.Tests.csproj --nologo
# 3) confirmar segurança da senha
grep -rn "PasswordHash ==\|passwordHash: request.Senha" src   # deve ser VAZIO
# 4) confirmar swagger
grep -rLc ProducesResponseType src/API/Epros.API/Controllers/*.cs   # controllers sem anotação (idealmente nenhum)
```
Se algo estiver vermelho: são erros barulhentos de integração (padrão da sessão) — clusterizam; corrigir ou fan-out de fixers. **Nunca** rodar migration em paralelo (serial só).

## 5. Backlog restante (Onda 5 — pra ~95% "cliente não percebe")
**Código (executável com autonomia):**
- [ ] Entidade `ImportacaoXml`/`ImportacaoArquivoXmlSaida` (import de compra já funciona via `NfeXmlParser`; falta persistir histórico do XML).
- [ ] **Seed das tabelas de referência** NCM/CEST/CFOP (VAZIAS — sem elas não emite nota). Fonte: legado tem as tabelas populadas.
- [ ] Cauda Onda 3 (P2): controllers `VendaDados`/`CompraDados`, `CnpjOnline`, `ProdutoHistoricoReajuste`, `*Enums → api/v1/enums/{dominio}`.
- [ ] Onda 4D: painel `plataforma/admin` ainda com `localhost` cru → `useApi`; alinhar hubs SignalR; `UsuarioEmpresa.PerfilUsuarioId → PerfilAcessoId` (rename da FK não foi feito na produção).
- [ ] Limpeza dos warnings nullable (470→<100).

**Validação (montar E2E; confirmar de verdade no ambiente do usuário):**
- [ ] Testes E2E: compra fiscal, venda NF-e, OFX.
- [ ] 1 emissão ponta a ponta contra o schema real.

**Só no ambiente do usuário (não codável):**
- [ ] Homologação SEFAZ (certificado A1 + webservice).

**POR ÚLTIMO (usuário agenda — cutover domingo):**
- [ ] Migração dos dados dos 20 clientes (long→Guid, SQL Server→PostgreSQL). NÃO tocar dado real de cliente sem sinal.

## 6. Convenções e gotchas CRÍTICOS (não repetir erros da sessão)
- **Modo de trabalho do usuário:** máximo paralelismo (fan-out de agentes), mapa-primeiro, fix-later. Autonomia TOTAL concedida (não pedir autorização; só reportar). Reduzir erro pela ESTRUTURA: partição por arquivos disjuntos, migrations congeladas até passe serializado, contratos cross-module por Lookup/Guid FK.
- **Build lock (MSB3021/3027):** a API roda local e trava `bin` → `dotnet build-server shutdown` + matar processo `Epros.API`. NÃO reiniciar a API.
- **Migrations:** SEMPRE serial, 1 por Context. Gotcha: enum text→int precisa `migrationBuilder.Sql("... USING (0);")`. Lookups = `ExcludeFromMigrations` (não geram tabela).
- **Frontend contrato:** `useApiList` lê `dados.{itens,total}` (handlers retornam `CommandResult.Ok(msg, new { Total, Pagina, Itens })`). NÃO reintroduzir `dados=array+totalRegistros`.
- **Decisões fixadas (CONVENCAO §1.2):** financeiro canônico = `ContasAPagar/AReceber` (fiel, simplificado removido); RBAC dono = GestaoClientes (`PerfilAcesso`); catálogos = `IGlobalEntity`; `IeSt`/`FatoGeradorFinanceiro` donos únicos; `Venda.Status`/`Compra.Status` = enum `EVendaStatus`.
- **Motor fiscal:** REUSAR o legado (DfeCalculos+Dfe.API via `IHerculesFiscalService`/`ICalculoFiscalService`), NUNCA reescrever. `GeraXmlDfe.ObterNf()` assina e exige certificado real; teste offline usa `ObterInf()`.
- **Auth:** token é string MVP (não JWT real) — endurecer futuramente. Provider de emitente fiscal = `EmpresaEmitenteFiscalProvider` (lê Empresa/EmpresaCertificado/EmpresaParametrosDfe via Lookup).

## 7. Comandos úteis
```bash
# subir só o postgres:      docker compose up -d postgres
# aplicar migrations:       dotnet ef database update --project src/Modules/Epros.Modules.<M> --startup-project src/API/Epros.API --context Context<M> --connection "Host=localhost;Port=5432;Database=epros;Username=epros;Password=epros_dev_password"
# rodar frontend:           cd EprosApp && npm run dev
# contagem tabelas:         docker exec epros-postgres psql -U epros -d epros -c "SELECT table_schema,count(*) FROM information_schema.tables WHERE table_schema IN ('plataforma','estoque','vendas','financas','aplicativo') AND table_type='BASE TABLE' GROUP BY 1;"
```
