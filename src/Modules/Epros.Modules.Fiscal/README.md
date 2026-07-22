# Módulo Fiscal (`Epros.Modules.Fiscal`)

Domínio fiscal do EprosERP: emissão e ciclo de vida de documentos fiscais eletrônicos
(NF-e 55, NFC-e 65, NFS-e, CT-e 57, MDF-e 58), catálogos de referência (NCM, CFOP, CST
IBS/CBS, códigos de serviço, alíquotas FCP/IBPT) e o motor de cálculo de impostos.

## Arquitetura

Segue CQRS com MediatR sobre `ContextFiscal` (EF Core 8, schema PostgreSQL `plataforma`).

```
Domain/                 Entidades (EntidadeSaaSBase, Flunt) + enums
Application/
  Commands/             Records ICommand + validators FluentValidation
  Queries/              Records IQuery<CommandResult> + handlers de leitura
  Handlers/             ICommandHandler<T> (escrita)
  Services/             Contratos dos adapters fiscais (Hercules/NFS-e/CT-e/MDF-e, cálculo, DANFE, storage)
Infrastructure/
  Data/                 ContextFiscal, seeders, lookups cross-module (ExcludeFromMigrations)
  Services/             Implementações reais + fallbacks "NaoConfigurado"
```

Regras de plataforma respeitadas: 100% herda `EntidadeSaaSBase`; catálogos nacionais são
`IGlobalEntity` (NCM, CFOP, CFOP-padrão, CST IBS/CBS, FCP, IBPT); PK/FK `Guid`; leitura
cross-module via lookups planos (`ProdutoLookup`, `EmpresaLookup`, …) marcados
`ExcludeFromMigrations` — o Fiscal não cria tabelas de outros módulos.

## Núcleo de emissão (NF-e / NFC-e)

`EmitirDocumentoFiscalCommand` → calcula impostos (`ICalculoFiscalService`) → transmite
via `IHerculesFiscalService` → grava estado (`Autorizado`/`Rejeitado`), evento de outbox,
DANFE (QuestPDF) e XML (`IArmazenamentoArquivoFiscal`). Cancelamento, carta de correção e
inutilização seguem o mesmo padrão com tratamento real de `cStat`.

## Documentos de transporte / serviço

- **NFS-e** (`EmitirLoteNfseCommand`, consultas, cancelamento) — adapter `INfseFiscalService`.
- **CT-e** (`EmitirCteCommand`, `CancelarCteCommand`) — adapter `ICteFiscalService`.
- **MDF-e** (`EmitirMdfeCommand`, `EncerrarMdfeCommand`) — adapter `IMdfeFiscalService`.

Sem provedor/certificado configurado, os adapters `…NaoConfigurado` retornam falha com
motivo claro — **nunca fabricam número/chave**. A integração concreta entra na homologação
do ambiente do cliente.

## Catálogos e loaders (carga em massa)

`POST .../atualizar` (multipart `IFormFile`) importa as tabelas de referência:

| Endpoint | Formato | Comando |
|----------|---------|---------|
| `api/v1/ncms/atualizar` | JSON oficial (`{ "Nomenclaturas": [...] }`) | `AtualizarTabelaNcmCommand` |
| `api/v1/cfop-padrao/atualizar` | CSV/TXT | `AtualizarTabelaCfopPadraoCommand` |
| `api/v1/codigos-servicos-sefaz/atualizar` | CSV/TXT | `AtualizarTabelaCodigoServicoSefazCommand` |
| `api/v1/fiscal/fcp-aliquotas-uf/atualizar` | CSV/TXT | `AtualizarTabelaFcpAliquotaUfCommand` |
| `api/v1/ibpt-dfe/atualizar` | CSV (`TabelaIBPTax<UF>.csv`) | `AtualizarTabelaIbptCommand` |

Todos são idempotentes (NCM/CFOP/serviço inserem só o que falta; FCP atualiza por UF; IBPT
recarrega por UF). O parse tolera separador `;`/`,`/tab, cabeçalho textual e datas pt-BR/ISO.

## Resolução CST / cClassTrib (IBS/CBS)

`api/v1/classificacoes-tributarias`:

- `GET obter-por-cst/{cst}/{modelo}` — CST por código, filtrando classes por modelo (55/65).
- `GET obter-por-ncm-lst/{ncm}/{modelo}` — classificações do NCM; fallback CST `000`.
- `POST obter-ncm-classificados` — classifica uma lista de NCMs em lote (CST + cClassTrib),
  usado na emissão. NCM sem classificação retorna `000` / `000001`.

## Contrato de resposta

Handlers retornam `CommandResult` (`Sucesso`, `Mensagem`, `Dados`, `Erros`). Listagens
paginadas usam o shape `{ Total, Pagina, Itens }`.

## Testes

xUnit em `tests/Epros.Tests` sobre `ContextFiscal` InMemory com doubles dos adapters
(`FiscalTests`, `FiscalF4F8Tests`, `FiscalTransmissaoTests`, `CalculoFiscalMotorLegadoTests`, …):
domínio, emissão NF-e/NFC-e, NFS-e/CT-e/MDF-e, loaders, resolução CST e DELETEs.

## Pendências de ambiente (fora do código)

Homologação SEFAZ por UF, CSC/Id-token da NFC-e, credenciais municipais de NFS-e,
certificado A1 e os dados oficiais de NCM/CEST/IBPT (carregados pelos loaders acima).
