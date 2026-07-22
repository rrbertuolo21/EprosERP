# 07 — Auditoria Arquitetural (Cética, READ-ONLY)

> **Escopo:** verificar que, além de funcionalidade+visual portados do legado, TUDO está no **formato arquitetural novo** (monolito modular hexagonal, schema por módulo, CQRS/MediatR, `EntidadeSaaSBase`, `IGlobalEntity`, Outbox, soft-delete, Flunt).
> **Regras canônicas:** `CONVENCAO_CODIGO.md` + `PADRAO_PORTE_LEGADO.md`.
> **Método:** varredura dos 14 módulos + sweep dos 90 controllers. Achados verificados em código (paths + linhas). Nenhum arquivo de código foi alterado.
> **Data:** jul/2026. **Modo atual:** Porte→Consolidação (build verde, 370 testes).

---

## Resumo executivo

O porte está em **conformidade arquitetural forte**. Os pilares estruturais foram respeitados em todos os módulos:

- **Herança:** 100% das entidades multi-tenant herdam `EntidadeSaaSBase` (256/256 nos módulos com domínio real).
- **Guid:** PKs/FKs são `Guid` em praticamente todo o código. **Um** desvio (`PessoaVeiculo.PaisId long`). `SequenciaExibicao long?` respeitada como exceção oficial.
- **Ownership:** sem duplicação de entidade entre módulos. Decisões fixadas respeitadas — modelo financeiro canônico (`ContasAPagar/AReceber`, sem os simplificados), `FatoGeradorFinanceiro` só em Financeiro (Vendas integra via Outbox), IeSt só em GestaoClientes, RBAC deprecado em Aplicativo (dono = GestaoClientes), catálogos nacionais Fiscais são `IGlobalEntity`.
- **Cross-module:** Lookups com `ExcludeFromMigrations()` + FK Guid; sem navegação de projeto cruzada.
- **Controllers:** 89 de 90 são finos (só `IMediator`). **Um** injeta DbContext (`AccountController`).

Os problemas restantes são **dois padrões sistêmicos de baixo/médio risco** e **um controller gordo crítico isolado**:

1. **`string` onde deveria haver `enum`** (`Status`, `Tipo`, `Categoria`) — o desvio mais espalhado, presente em GestaoClientes, Vendas, Fiscal e nos 8 módulos-satélite. É o principal débito "de formato" que resta.
2. **`Validar()` Flunt central ausente** em algumas entidades (validam inline no ctor mas não expõem `Validar()` reutilizável para o `Alterar()`), e alguns `Validar()` vazios no Estoque.
3. **`AccountController`** — único ponto **Crítico**: injeta 2 DbContexts e contém regra de negócio SaaS de inadimplência inline.

---

## Contagem por severidade

| Severidade | Qtd |
|------------|-----|
| **Crítica** | **1** |
| **Alta** | **4** |
| **Média** | **9** |
| **Baixa** | **13** |
| **Total** | **27** |

> A contagem consolida achados equivalentes. O padrão "string-como-enum" foi contado por módulo-cluster (não por campo individual), senão somaria +30 linhas de baixa severidade.

---

## As 15 violações mais graves (ordenadas)

| # | Item | Módulo/arquivo | Violação | Padrão correto | Severidade |
|---|------|----------------|----------|----------------|------------|
| 1 | Controller gordo com DbContext + regra de negócio | `API/Controllers/AccountController.cs:24-25, 94-149` | Injeta **2** DbContexts (`ContextAplicativo` + `ContextGestaoClientes`); método `Session()` faz parsing de token, queries EF diretas (`Usuarios`, `Empresas`, `Clientes`, `Faturas`), monta DTOs em loop e aplica regra SaaS de inadimplência (`DateTime.UtcNow.AddDays(-15)`, `f.Status != "Paga"`) inline. Único controller com DbContext em todo o sweep. | Controller thin: só `_mediator.Send()`. `Session()` → Query+Handler. Zero DbContext no ctor. | **Crítica** |
| 2 | Outbox aponta para schema de outro módulo | `Financeiro/Infrastructure/Data/ContextFinanceiro.cs:161-165` | `modelBuilder.Entity<OutboxMessage>().ToTable("outbox_messages", "estoque")` **sem** `ExcludeFromMigrations`. Todos os outros módulos mapeiam o Outbox no **próprio** schema (Vendas→vendas, GestaoClientes→plataforma, Aplicativo→aplicativo). Financeiro (schema `financas`) grava/possui `estoque.outbox_messages` — risco de conflito de migration e ownership cruzado da tabela. | `ToTable("outbox_messages", "financas")` no próprio schema. | **Alta** |
| 3 | FK como `long` em vez de `Guid` | `GestaoClientes/Domain/Entities/PessoaVeiculo.cs:11,21,32,50` | `public long PaisId { get; private set; }` — FK para `Pais` (cujo `Id` é `Guid`; `Municipio.PaisId`/`Endereco.PaisId` são `Guid`). Não é `SequenciaExibicao`. Validação `IsGreaterThan(paisId, 0, ...)` denuncia semântica de código numérico legado não traduzido. | `Guid PaisId` (tradução legado long→Guid). | **Alta** |
| 4 | `Status`/`Situacao` como `string` em entidades transacionais de negócio | `GestaoClientes`: `Fatura.cs:13`, `PagamentoFatura.cs:11`, `PagamentoTransferencia.cs:13`, `PedidoSaaS.cs:17`, `SessaoPagamento.cs:10`, `ExecucaoMassa.cs:13`, `ExercicioFinanceiro.cs:12` | `Status` string com valores mágicos comentados ("Pendente/Paga/Cancelada" etc.); comparações por string em runtime (ex.: `AccountController` `f.Status != "Paga"`). Não existe `EStatusFatura`/`EStatusPagamento` em `Enums.cs`. | Enum real em `Domain/Enums`, `HasConversion<string>()`/`<int>()` no Context. | **Alta** |
| 5 | `Status`/`Tipo`/`Categoria` como `string` — padrão sistêmico nos 8 satélites | DMS `GarantiaMontadora/OrdemServicoDms/VendaVeiculo`; ESG `RelatorioESG`; GRC `ControleInterno/Denuncia/IncidenteCompliance/RiscoCorporativo`; Manutencao `Equipamento/OrdemManutencao`; Producao `OrdemProducao`; Projetos `Projeto/WbsItem`; Qualidade `InspecaoLote/NaoConformidade`; RH `Colaborador/FolhaPagamento/FolhaPagamentoVerba` | ~20 campos de máquina-de-estado modelados como `string` com valores fixos no comentário (ex.: `OrdemProducao.Status = "Pendente" // Pendente, EmProducao, Encerrada, Cancelada`). Regra: "enum real, nunca string/int". | Enums tipados por domínio (ex.: `EOrdemProducaoStatus`) em `Domain/Enums` do módulo. | **Alta** |
| 6 | Comandos de escrita de RBAC hospedados no módulo errado | `Aplicativo/Application/Commands/MenusCommands.cs` + `Application/Handlers/MenusHandlers.cs:18,82,173` + `MenusQueryHandlers.cs` | `Criar/Atualizar/DeletarPerfilAcessoCommand` e queries de Menu vivem em **Aplicativo**, mas todos os handlers operam sobre `ContextGestaoClientes` (`_context.PerfisAcessos`, `_context.Menus`). RBAC dono = GestaoClientes. Acoplamento cruzado: a escrita deveria residir no módulo dono. | Mover Commands/Handlers de PerfilAcesso/Menu para GestaoClientes; Aplicativo só consome. | **Média** |
| 7 | `Modelo`/`Status` string em documento fiscal | `Fiscal/Domain/Entities/DocumentoFiscal.cs:10-11` | `Status` ("Rascunho"/"Autorizado") como `string` — já existe `EDocumentoFiscalStatus` no módulo Vendas para reuso. (`Modelo` "55"/"65" é código SEFAZ, tolerável.) | `Status` como enum de domínio. | **Média** |
| 8 | `CaixaMovimento.Tipo` string | `Vendas/Domain/Entities/CaixaMovimento.cs:10` (mapeado `HasMaxLength(20)` em `ContextVendas.cs:687`) | `Tipo` com valores fixos ('Suprimento'/'Sangria') como `string`. | Enum `ECaixaMovimentoTipo`. | **Média** |
| 9 | `Cupom.Tipo` string | `GestaoClientes/Domain/Entities/Cupom.cs:10,36-38,92` | `Tipo = "Fixo" // Fixo, Percentual`; validado e ramificado (`CalcularDesconto`) por comparação de string. | Enum `ETipoDescontoCupom`. | **Média** |
| 10 | `Venda` sem `Validar()` reutilizável | `Vendas/Domain/Entities/Venda.cs:107-111,185` | Validação inline só no ctor; `Alterar(...)` e vários `Definir...` mutam sem revalidar o contrato. | `Validar()` Flunt central, chamado no ctor e no `Alterar()`. | **Média** |
| 11 | `FatoGeradorFinanceiro` sem `Validar()` | `Financeiro/Domain/Entities/FatoGeradorFinanceiro.cs:26,35` | Sem `Validar()` Flunt; ctor e `Alterar()` não validam. As pares (`ContasAPagar/AReceber`, `Banco`, `ContaBancaria`) validam. | `Validar()` no ctor/`Alterar()`. | **Média** |
| 12 | `EstoqueProduto` sem validação (Validar vazio + ctor sem Flunt) | `Estoque/Domain/Entities/EstoqueProduto.cs:29,44` | `public void Validar() { }` vazio **e** ctor sem `Contract<T>`. Entidade com quantidade/valores monetários sem nenhuma invariante. (Também vazios: `EstoqueMovimentoManual.cs:36`, `FatoGeradorEstoque.cs:38`, `ProdutoFichaEstoqueEntrada.cs:46`, `ProdutoFichaEstoqueSaida.cs:45`.) | `Validar()` com Flunt, invocado no ctor. | **Média** |
| 13 | `MovimentoEstoque.Tipo` string (enum de negócio) | `Estoque/Domain/Entities/MovimentoEstoque.cs:11` (mapping `ContextEstoque.cs:364-367` sem `HasMaxLength`) | `Tipo` ("Entrada"/"Saida") como `string`, usado ativamente em handlers (`LancarCompraCommandHandler`, `VendaFaturadaEstoqueHandler`). Existe `EOrigem` no domínio. | Enum tipado (`ETipoMovimentoEstoque`/`EOrigem`). | **Média** |
| 14 | `ContasAPagar.DataEmissao` / `ContasAReceber.DataEmissao` com `set` público | `Financeiro/Domain/Entities/ContasAPagar.cs:26`; `ContasAReceber.cs` (paralelo) | `public DateTime DataEmissao { get; set; }` — setter público quebra o encapsulamento (demais props usam `private set`). | `private set`, mutação só via ctor/`Alterar()`. | **Média** |
| 15 | Esquema de "token" não-JWT / inseguro | `Aplicativo/AccountController.cs:64-91,164-191`; `MenusQueryHandlers.cs:556` | "Token" é string concatenada `jwt-token-completo-{tenant}-{userId}-{empresa}-{Guid}`, parseada por `Split('-')`. Sem assinatura criptográfica. Fora do escopo estrito de convenção arquitetural, mas risco de segurança material no caminho auditado. | JWT assinado (HS256/RS256) em serviço dedicado. | **Alta**\* |

> \* Item 15 é de severidade "Alta" por risco de segurança, mas está **fora** do escopo estrito "formato arquitetural" — registrado por transparência. Se contado como Alta, o total de Alta sobe para 5.

---

## Achados de baixa severidade (detalhe)

| # | Item | Módulo/arquivo | Violação | Padrão correto | Severidade |
|---|------|----------------|----------|----------------|------------|
| B1 | Catálogo `IGlobalEntity` recebe `tenantId` real no ctor | `Fiscal/Domain/Entities/Cfop.cs:35-59` + `Application/Handlers/CriarCfopCommandHandler.cs:34-58` | Handler passa `_tenantProvider.GetTenantId()` para catálogo global (demais passam `"system"`). Neutralizado por `ContextBase.ProcessarEntidadesSaaS` (força `"system"` no save), mas parâmetro morto/enganoso. | Passar `"system"` no ctor. | Baixa |
| B2 | Idem em `CstIbsCbs` | `Fiscal/Domain/Entities/CstIbsCbs.cs:20-33` + `CstIbsCbsHandlers.cs:24-27` | Mesmo caso de B1. | `"system"` no ctor. | Baixa |
| B3 | Índices `TenantId` vestigiais em catálogos globais | `Fiscal/ContextFiscal.cs:254-443` (Ncm, Cest, CstIbsCbs, CodigoAnp, EnquadramentoIpi, FcpAliquotaUf, IcmsAliquotaInterestadual) | `HasIndex(new { TenantId, ... })` em catálogo `IGlobalEntity` (coluna sempre `"system"`). Inócuo, mas modelagem multi-tenant vestigial. | Índice por chave de negócio, sem `TenantId`. | Baixa |
| B4 | Catálogos `IGlobalEntity` mantêm coluna física `TenantId` | `GestaoClientes`: `Pais/Municipio/Subdivisao/Moeda/FusoHorario/FormatoCodigoPostal/CodigoPostalCache/SincronizacaoGeografica` | Herdam `EntidadeSaaSBase` (traz `TenantId`) + marcam `IGlobalEntity`. `ContextBase` isenta RLS por convenção, mas a coluna física persiste. Conformidade estrita "no tenant" a confirmar. | Confirmar que `IGlobalEntity` isenta tenant; senão não expor `TenantId` no catálogo. | Baixa |
| B5 | `Cupom.Validar()` com semântica divergente | `GestaoClientes/Domain/Entities/Cupom.cs:55` | `public bool Validar()` retorna **elegibilidade de negócio** (ativo/validade/limite), não invariantes Flunt (que estão no ctor). Colide com a convenção de nome. | Reservar `Validar()` para invariantes; renomear (ex.: `EstaElegivel()`). | Baixa |
| B6 | `HasMaxLength` esparso no mapping | `GestaoClientes/ContextGestaoClientes.cs` | ~18 `HasMaxLength` para ~62 entidades com muitas strings; depende de convenção global do `ContextBase`. Regra pede explícito. | `HasMaxLength` por string, ou documentar default global. | Baixa |
| B7 | `RegistrarVendaCommand.Status` string no contrato de entrada | `Vendas/Application/Commands/RegistrarVendaCommand.cs:10`; parse em `RegistrarVendaCommandHandler.cs:147-174` | DTO recebe `string Status` (parse tolerante 'emitida'/'contingencia'). A **entidade** `Venda.Status` já é `EVendaStatus` (correto) — o desvio é só o contrato do command. | Command deveria receber `EVendaStatus`. | Baixa |
| B8 | Duas convenções de command coexistem no mesmo módulo | `Vendas`: `RegistrarVenda/SincronizarVendas/CancelarVenda` usam `: ICommand`; `VendaFiscalCommands`/`VendaAcoesCommands` usam `: IRequest<CommandResult>` | Ambos os estilos são aceitos pela convenção, mas a mistura dentro de um módulo é inconsistência. | Padronizar em um contrato por módulo. | Baixa |
| B9 | `VendaItem` sem `Validar()` central | `Vendas/Domain/Entities/VendaItem.cs:68` | Validação não centralizada em `Validar()` (mesmo desvio de #10). `private set` e ctor OK. | `Validar()` Flunt central. | Baixa |
| B10 | Typo de nomenclatura propagado | `Financeiro/Domain/Entities/ImportacacaoArquivoOfx.cs` / `...Transacao.cs` + DbSets `ContextFinanceiro.cs:35-36` | "Importacacao" (grafia errada) na entidade, DbSet e commands. Dívida de nomenclatura, não quebra de regra. | `ImportacaoArquivoOfx`. | Baixa |
| B11 | `Cupom.Tipo` — mapping sem `HasMaxLength` no `MovimentoEstoque` | `Estoque/ContextEstoque.cs:364-367` | `Tipo`/`Historico` de `MovimentoEstoque` sem `HasMaxLength` (reforça #13). | `HasMaxLength` em todas as strings. | Baixa |
| B12 | `FatoGeradorEstoque.Validar()` vazio | `Estoque/Domain/Entities/FatoGeradorEstoque.cs:38` | `Validar()` vazio; não valida `Origem` nem coerência de FKs venda/compra/manual. (Entidade Estoque-própria, distinta de FatoGeradorFinanceiro — ownership correto.) | `Validar()` Flunt no ctor. | Baixa |
| B13 | `ConfiguracaoEmpresa.TimeZoneId/CurrencyId` como `int` | `Aplicativo/Domain/Entities/ConfiguracaoEmpresa.cs:13,15` | `int` — provável referência a catálogo IANA/ISO (aceitável) ou a catálogo-entidade interno (deveria ser Guid). Provável falso-positivo; registrado para verificação. | Guid se referencia entidade interna; int/string se código IANA/ISO. | Baixa |

---

## Conformidades verificadas (SEM violação) — o que está correto

Registrado para dar peso à conclusão de conformidade forte:

- **Schemas por módulo** conferidos via `HasDefaultSchema`: Aplicativo=`aplicativo`, Estoque=`estoque`, Financeiro=`financas`, Vendas=`vendas`, Fiscal=`plataforma` (correto, **não** `fiscal`), GestaoClientes=`plataforma`, DMS=`concessionarias`, ESG=`esg`, GRC=`grc`, Manutencao=`manutencao`, Producao=`producao`, Projetos=`projetos`, Qualidade=`qualidade`, RH=`rh`.
- **Modelo financeiro canônico:** existem **apenas** `ContasAPagar`/`ContasAReceber` (agregados completos com itens/juros/multa/desconto + FK `FatoGeradorFinanceiro`). Busca por `class Conta(Pagar|Receber)` simplificado → **zero** resultados. Decisão fixa respeitada.
- **`FatoGeradorFinanceiro`** declarado **só** em `ContextFinanceiro` (DbSet único). Vendas integra Venda→Financeiro via `OutboxMessage` + `VendaFaturadaEventNotification` — sem ownership cruzado.
- **`IeSt`** só em GestaoClientes (`IeSt.cs` + DbSet). Fiscal referencia como `ExcludeFromMigrations()` Lookup de leitura. Decisão fixa respeitada.
- **RBAC** deprecado em Aplicativo: `Menu/MenuItemNivel1/2/PerfilUsuario/PerfilUsuarioAcesso` removidos dos DbSets; `UsuarioEmpresa.PerfilAcessoId` é FK-Guid para o `PerfilAcesso` de GestaoClientes. Dono único confirmado. (Resta o acoplamento organizacional do item #6.)
- **Catálogos nacionais Fiscais** (`Ncm, Cest, Cfop, CfopPadrao, CodigoAnp, EnquadramentoIpi, IcmsAliquotaInterestadual, FcpAliquotaUf, CodigoServicoSefaz, CstIbsCbs`) implementam `IGlobalEntity`. `Banco` também.
- **Motor de cálculo fiscal reaproveitado**, não reescrito: `ICalculoFiscalService` → `MotorLegadoCalculoFiscalService` (adapter que delega a `Epros.ERP.DfeCalculos`); emissão/SEFAZ via `IHerculesFiscalService`. Nenhuma reimplementação.
- **Agregado Compra** (Estoque) completo: raiz + Emitente/Destinatario/Transporte/Nfe/Total/Imposto/Fatura/Itens/Pagamentos (~40 sub-entidades). **Agregado Venda** completo: Itens/Pagamentos/Emitente/Destinatario/Transporte/Nfe/Nfce/Fatura.
- **`Venda.Status` e `Compra.Status` são enum `EVendaStatus`** (não string) — decisão fixa respeitada.
- **Cross-module** sempre por Lookup + `ExcludeFromMigrations()` + FK Guid; nenhuma navegação de projeto cruzada (`PessoaLookup`, `ProdutoLookup`, `EmpresaLookup`, `ServicoLookup`, `ContaAPagarLookup` etc.).
- **Controllers:** 89/90 finos (só `IMediator`). Sweep de `SaveChangesAsync`/`.Add(`/`.Where(` em campos `_context*` nos controllers → zero, exceto `AccountController` (#1). `EnumsController` usa `typeof(Context*)` só como âncora de reflexão para enums — **não** injeta DbContext (falso-positivo caracterizado).
- **CQRS:** commands são `record` (`ICommand` ou `IRequest<CommandResult>`), 1 handler por command (ex.: 9 commands ContasAReceber → 9 handlers), nomenclatura Criar/Atualizar/Deletar/verbos de negócio, `CommandResult.Ok/Falha`. Validators FluentValidation presentes onde Estilo B é usado.
- **Herança/tipos:** 256 entidades herdam `EntidadeSaaSBase`; PKs Guid; `SequenciaExibicao long?` respeitada; nenhum int/long FK além de `PessoaVeiculo.PaisId` (#3). Módulos-satélite: **todas** as entidades validam via `new Contract<T>` no ctor (a auditoria inicial de "sem Validar()" era falso-positivo — validam inline, não via método nomeado).

---

## Recomendações priorizadas (para o Modo Consolidação)

1. **[Crítica]** Refatorar `AccountController.Session()` para uma Query+Handler; remover ambos os DbContexts do controller (#1).
2. **[Alta]** Corrigir o schema do Outbox do Financeiro para `financas` (#2).
3. **[Alta]** Traduzir `PessoaVeiculo.PaisId` para `Guid` (#3).
4. **[Alta/Média]** Campanha única "string→enum": criar os enums de `Status`/`Tipo`/`Categoria` em `Domain/Enums` de cada módulo e aplicar `HasConversion` (#4, #5, #7, #8, #9, #13). É o maior débito de formato remanescente e é mecânico.
5. **[Média]** Mover Commands/Handlers de RBAC (PerfilAcesso/Menu) de Aplicativo para GestaoClientes (#6).
6. **[Média]** Preencher `Validar()` Flunt vazio/ausente (`EstoqueProduto` e demais vazios do Estoque; `Venda`; `FatoGeradorFinanceiro`) e fechar `set` público de `DataEmissao` (#10-#14).
7. **[Alta — segurança, fora de escopo de formato]** Substituir o "token" concatenado por JWT assinado (#15).

> **Nota de método:** os itens acima são "formato/consolidação", coerentes com o estado atual (Porte→Consolidação). Nenhum viola a fidelidade campo-a-campo do legado — são traduções de plataforma ainda pendentes (string→enum, long→Guid) ou disciplina de encapsulamento/CQRS.
