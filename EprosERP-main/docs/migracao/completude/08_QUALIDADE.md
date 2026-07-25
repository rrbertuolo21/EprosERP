# 08 — Qualidade (código comentado, documentado, testado e manutenível)

> Auditoria READ-ONLY — 2026-07-05. Alvo: `EprosERP` backend (`src/`, `tests/`) + frontend `Epros.App`.
> Build: **0 erros / 467 avisos**. Testes: **327 métodos `[Fact]`/`[Theory]` em 71 arquivos**.

---

## RESUMO EXECUTIVO

- **Testes:** 327 casos, cobertura desigual. Fortes em cadastros (GestãoClientes, SuperAdmin, Fiscal-cadastro, Financeiro-contas). **Fracos/ausentes na cauda transacional fiscal** (NFS-e, CT-e/MDF-e = 0 testes) e em vários fluxos de sincronização/importação (ImportarCompraXml = 0).
- **Documentação:** grave. **85/90 controllers sem XML-doc de classe**; **168/192 handlers sem nenhum `///`**. GestãoClientes (53/53), Aplicativo (23/23), DMS/Manutencao/Producao/Projetos/Qualidade/RH = 100% sem doc. **0 READMEs por módulo** (nem backend nem frontend).
- **Warnings:** 467, quase tudo **nullable-reference** (CS86xx). **766 das ~934 linhas de aviso concentram-se num único projeto: `External.DfeCalculos`** (motor fiscal legado portado) — é o epicentro de dívida técnica.
- **Manutenibilidade:** poucos TODOs reais (10, a maioria "honestos" documentando mock legado); god-files concentrados em Aplicativo (LandingPageSettings 1759 linhas) e nos `Context*.cs` (ContextEstoque 1048 linhas / 56 DbSets).

### Nº de testes por arquivo (top)
| Arquivo | Casos |
|---|---|
| GestaoClientesTests | 25 |
| SuperAdminDomainTests | 18 |
| FiscalTests | 14 |
| PessoaTests | 12 |
| SuperAdminCqrsTests | 11 |
| UsuariosPapeisTests | 10 |
| CompraFiscalAgregadoTests / EmpresaCertificadoTests / PermissoesMenuTests | 9 |
| (demais) | ≤ 8 |

Cauda longa preocupante: 12 arquivos têm só **1–2 casos** (VendasTests=1, VendasCqrsTests=1, ProdutoCqrsTests=1, CompraCqrsTests=1, MakerCheckerExecucaoMassa=1) — são "smoke tests", não cobrem regra de negócio.

### Nº de handlers por módulo (denominador da cobertura)
| Módulo | Handlers | Controllers* |
|---|---|---|
| GestaoClientes | 53 | — |
| Fiscal | 33 | — |
| Aplicativo | 23 | — |
| Estoque (compra/produto) | 22 | — |
| Financeiro | 11 | — |
| GRC | 7 | — |
| Vendas | 7 | — |
| ESG / Producao | 6 | — |
| DMS / Manutencao / Projetos / RH | 5 | — |
| Qualidade | 4 | — |

\* Todos os 90 controllers vivem em `src/API/Epros.API/Controllers` (agregam vários módulos), não dentro das pastas de módulo.

### Top lacunas de teste (crítico)
1. **NFS-e** (`NfseHandlers`, `NfseFiscalService*`) — **0 testes**.
2. **CT-e / MDF-e** (`CteMdfeHandlers`, `CteMdfeFiscalService*`) — **0 testes** (ainda mock/homologação, mas sem teste de contrato do fallback).
3. **ImportarCompraXml** (`ImportarCompraXmlCommandHandler`) — **0 testes** (parsing de XML de NF-e de entrada = risco alto).
4. **Vendas transacional**: `RegistrarVenda`/`CancelarVenda` têm só smoke (VendasTests=1, VendasCqrsTests=1); regras de rateio/estorno pouco exercitadas.
5. **VendaFiscalHandlers** (852 linhas, geração fiscal na venda) sem teste dedicado próprio além do agregado.
6. **PlanoDeContas/Natureza** — 1 arquivo de teste apenas para um handler grande de financeiro.

### Principais categorias de warning (467)
| Código | Qtd (linhas) | Tipo | Significado |
|---|---|---|---|
| CS8618 | 392 | Nullable | Propriedade non-nullable não inicializada no construtor (entidades EF) |
| CS8602 | 226 | Nullable | Desreferência de possível `null` |
| CS8604 | 90 | Nullable | Argumento possivelmente `null` |
| CS8603 | 60 | Nullable | Retorno possivelmente `null` |
| CS8601 | 54 | Nullable | Atribuição de referência possivelmente `null` |
| CS8600 | 48 | Nullable | Conversão de `null` para tipo não-anulável |
| CS8625 / CS8605 / CS8765 | ~28 | Nullable | Literal null / unbox / override de nulabilidade |
| xUnit2017 / 2012 / 2013 | 14 | **Test smell** | Uso de `Assert` genérico onde há assert específico de coleção |
| CS0219 / CS0162 | 16 | Dead code | Variável atribuída e nunca usada / código inalcançável |
| CS0618 | 4 | Deprecação | Uso de API `[Obsolete]` |
| EF1001 | 2 | EF interno | Uso de API interna do EF Core |

> **~90% dos avisos são nullable-reference**, tratáveis em lote (inicializadores `= null!`, `required`, ou anotações). **766 linhas estão em `External.DfeCalculos`** — corrigir esse projeto sozinho elimina a maior parte.

### 10 pontos de manutenibilidade mais críticos
1. **`External.DfeCalculos` = 766 linhas de aviso** — motor fiscal legado sem nullable annotations; núcleo de cálculo de imposto com maior densidade de dívida.
2. **`LandingPageSettingsHandlers.cs` — 1759 linhas** (Aplicativo): god-file, múltiplos handlers num só arquivo, sem doc.
3. **`ContextEstoque.cs` — 1048 linhas / 56 DbSets**: contexto EF "deus" concentrando meio módulo.
4. **`UsuarioHandlers.cs` (883) / `PedidosHandlers.cs` (754) / `MenusQueryHandlers.cs` (572) / `AuthHandlers.cs` (556)**: arquivos-agregado grandes no módulo Aplicativo, todos sem `///`.
5. **`VendaFiscalHandlers.cs` — 852 linhas**: lógica fiscal-na-venda densa, sem teste próprio e sem doc.
6. **`MontaImpostoHercules.cs` — 661 linhas** + `GeraXmlDfe.cs` (886): cálculo/serialização XML legado, alta complexidade, nomes obscuros ("Hercules").
7. **XML-doc quase ausente**: 168/192 handlers e 85/90 controllers sem summary → onboarding e manutenção caros.
8. **0 README por módulo** (14 módulos backend + Epros.App): nenhum ponto de entrada de arquitetura por domínio.
9. **Smoke tests disfarçados de cobertura**: 12 arquivos de teste com 1–2 casos dão falsa sensação de verde no CQRS de Vendas/Produto/Compra.
10. **10 TODO/HACK no motor fiscal** (CT-e/MDF-e/NFS-e/DANFE/MinIO/EmitenteProvider) — pendências reais de emissão marcadas em `MotorLegadoFiscalService.cs` e serviços `*NaoConfigurado.cs`.

---

## DIMENSÃO 1 — COBERTURA DE TESTES

327 casos / 71 arquivos. Presença de handler crítico nos testes:

| Handler / fluxo | Onde (módulo) | Falta o quê | Severidade |
|---|---|---|---|
| `NfseHandlers` | Fiscal | Nenhum teste (0 arquivos referenciam) | **Alta** |
| `CteMdfeHandlers` + fallback `*NaoConfigurado` | Fiscal | Nenhum teste, nem de contrato do fallback | **Alta** |
| `ImportarCompraXmlCommandHandler` | Estoque | 0 testes — parsing XML NF-e entrada | **Alta** |
| `RegistrarVendaCommandHandler` | Vendas | Só smoke (VendasTests=1) — regras de rateio/estorno | **Alta** |
| `VendaFiscalHandlers` (852 l) | Vendas | Sem teste dedicado; só via CompraFiscalAgregado indireto | **Alta** |
| `CancelarVendaCommandHandler` | Vendas | 1 arquivo — estorno financeiro/estoque pouco coberto | Média |
| `PlanoDeContasAndNaturezaHandlers` | Financeiro | 1 arquivo p/ handler grande | Média |
| `SincronizarVendas`/`SincronizarCaixas` | Vendas | Sincronização offline sem teste | Média |
| `RegistrarProdutoReajuste` | Estoque | Sem teste dedicado | Média |
| Módulos DMS/ESG/GRC/Manutencao/Producao/Projetos/Qualidade/RH | vários | Só `*ModuleTests` de 4–5 smoke cada; regra de negócio pouco exercitada | Média |
| `EmitirDocumentoFiscal`/`Cancelar`/`CartaCorrecao`/`ConsultaSefaz` | Fiscal | **Cobertos** (1 arquivo cada) — OK | Baixa |
| `ContasAPagar`/`ContasAReceber`/`Caixa`/`LancarCompra` | Financeiro/Estoque | **Bem cobertos** (4–5 arquivos) | Baixa |

**Bem coberto:** GestãoClientes (25), SuperAdmin (35 somando 4 arquivos), Fiscal-cadastro (CFOP/CST/Contador/Serviço/CBenef), Empresa/Certificado, Permissões/Usuários, Financeiro-contas.
**Descoberto:** cauda fiscal de emissão eletrônica (NFS-e/CT-e/MDF-e), importação de XML, sincronização offline do app, e módulos "novos recursos" (só smoke).

---

## DIMENSÃO 2 — DOCUMENTAÇÃO

| Item | Onde | Falta o quê | Severidade |
|---|---|---|---|
| XML-doc de classe em controllers | `src/API/Epros.API/Controllers` (90) | **85/90 sem `///` na classe** | **Alta** |
| XML-doc em handlers | `src/Modules/*/Application/Handlers` (192) | **168/192 sem nenhum `///`** | **Alta** |
| Handlers GestãoClientes | 53 arquivos | **53/53 (100%) sem doc** | **Alta** |
| Handlers Aplicativo | 23 arquivos | **23/23 (100%) sem doc** | **Alta** |
| Handlers Fiscal | 33 arquivos | 28/33 sem doc (paradoxal: domínio mais crítico) | **Alta** |
| Handlers DMS/Manutencao/Producao/Projetos/Qualidade/RH | 30 arquivos | 100% sem doc cada | Média |
| README por módulo (backend) | `src/Modules/*` | **0 READMEs** em 14 módulos | Média |
| README frontend | `Epros.App` | **0 README** (existe MAPA_FRONTEND.md na raiz, mas não por pasta) | Média |
| Composables front | `Epros.App/composables` (39) | Todos têm ≥1 comentário — **OK** (não há composable "mudo") | Baixa |

**Nota positiva:** os poucos handlers/serviços fiscais que TÊM doc são exemplares (contratos "honestos" explicando o legado — ver `ICteMdfeFiscalService.cs`, `CteMdfeFiscalServiceNaoConfigurado.cs`). O padrão existe; falta aplicá-lo em massa. Handlers com doc: ESG 2/6, Estoque 8/22, Financeiro 6/11, Fiscal 5/33, Vendas 2/7.

---

## DIMENSÃO 3 — MANUTENIBILIDADE

### Arquivos gigantes (não-migration)
| Arquivo | Linhas | Nota |
|---|---|---|
| `Aplicativo/.../LandingPageSettingsHandlers.cs` | 1759 | God-file, múltiplos handlers |
| `Estoque/Infrastructure/Data/ContextEstoque.cs` | 1048 | 56 DbSets |
| `External/DfeCalculos/Models/GeraXmlDfe.cs` | 886 | Serialização XML legada |
| `Aplicativo/.../UsuarioHandlers.cs` | 883 | |
| `Vendas/.../VendaFiscalHandlers.cs` | 852 | Fiscal-na-venda, sem teste próprio |
| `GestaoClientes/.../ContextGestaoClientes.cs` | 841 | |
| `GestaoClientes/.../ParametrosCommandHandlers.cs` | 761 | |
| `Aplicativo/.../PedidosHandlers.cs` | 754 | |
| `Vendas/.../ContextVendas.cs` | 731 | |
| `External/DfeCalculos/Impostos/MontaImpostoHercules.cs` | 661 | Nome obscuro, complexidade alta |

> Migrations (6440–3395 linhas) são auto-geradas — **ignoradas** como dívida.

### Warnings (467) — categorização por projeto
| Projeto | Linhas de aviso | Observação |
|---|---|---|
| **External.DfeCalculos** | **766** | Epicentro; nullable em massa no motor fiscal |
| Tests | 82 | inclui os 14 xUnit2017/2012/2013 (asserts fracos) |
| External.Shared | 30 | |
| Aplicativo / Estoque | 16 cada | |
| Fiscal | 12 | |
| GestaoClientes / Financeiro / Infrastructure | ≤ 6 | |

### TODO/HACK/FIXME (10 ocorrências, todas rastreáveis)
| Onde | O quê | Severidade |
|---|---|---|
| `Fiscal/Infrastructure/Services/MotorLegadoFiscalService.cs:113-114` | DANFE via FastReport + persistência XML/PDF em MinIO adiados | Média |
| `Fiscal/Infrastructure/Services/EmitenteFiscalProviderNaoConfigurado.cs:12` | Substituir por provider real (empresa emitente + cert A1) | **Alta** |
| `Fiscal/.../CteMdfeFiscalServiceNaoConfigurado.cs`, `NfseFiscalServiceNaoConfigurado.cs`, `ICteMdfeFiscalService.cs`, entidades CT-e/MDF-e | Fallbacks "honestos": emissão real fica p/ homologação SEFAZ | Média (documentado, não acidental) |

### xUnit test smells (14 avisos)
`xUnit2017/2012/2013` — asserts genéricos onde há assert de coleção (`Assert.NotNull` em `.Count`, `Assert.True(x.Any())` em vez de `Assert.NotEmpty`). Corrigir melhora legibilidade e mensagens de falha.

### Recomendações priorizadas
1. Rodar campanha nullable em `External.DfeCalculos` (`required`/`= null!`) → derruba ~2/3 dos avisos.
2. Aplicar template de XML-doc (já existe em Fiscal) via mutirão em controllers + handlers de GestãoClientes/Aplicativo/Fiscal.
3. Adicionar testes de contrato para NFS-e/CT-e/MDF-e (ao menos o fallback "NaoConfigurado" lança/retorna esperado) e para `ImportarCompraXml`.
4. Quebrar `LandingPageSettingsHandlers.cs` e `VendaFiscalHandlers.cs` por responsabilidade.
5. Substituir smoke-tests 1-caso de Vendas/Produto/Compra CQRS por suítes de regra.
6. Corrigir os 14 xUnit asserts fracos.
7. Criar README curto por módulo apontando entrada/arquitetura.
