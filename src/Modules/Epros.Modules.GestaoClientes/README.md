# Epros.Modules.GestaoClientes

Módulo de **cadastros de clientes/pessoas/empresas, geografia e backoffice SaaS** do EprosERP.
Porte do domínio de Cadastros do legado (`Epros.ERP.Domain/Entities/Cadastros`) para a arquitetura
modular CQRS/EF Core (PostgreSQL), com `EntidadeSaaSBase` (PK/FK `Guid`, multi-tenant, soft delete,
sincronização offline).

## Responsabilidades

- **Pessoas** — `Pessoa` (agregado raiz) e seus papéis: `PessoaFisica`, `PessoaJuridica`,
  `PessoaCliente`, `PessoaFuncionario`, `PessoaTransportadora`, `PessoaMotorista`,
  `PessoaPrestadorServico`, `PessoaEstrangeiro`, `PessoaContato`, `PessoaVeiculo`, `PessoaGrupo`.
  Ciclo de vida por enum `Status` (`EEstadoPessoa`): Submeter/Aprovar/Rejeitar/Inativar/Bloquear/Reativar.
- **Empresas** — `Empresa`, `EmpresaContato`, `EmpresaCertificado`, `EmpresaParametrosDfe`
  (owned types de NF-e/NFC-e), `IeSt`.
- **Endereços** — entidade `Endereco` (1:N em `Pessoa`; escopo por `EmpresaId?`) + o value-object
  `Endereco` embutido em `Empresa`. Ver seção "Fidelidade ao legado" abaixo.
- **Geografia** — `Pais`, `Municipio`, `Subdivisao`, `ZonaEntrega`, `FormatoCodigoPostal`,
  cache de CEP (`CodigoPostalCache`) com enriquecimento via ViaCEP.
- **Backoffice SaaS** — `Plano`, `AssinaturaCliente`, `Contrato`/`ContratoItem`, `Cupom`/`UsoCupom`,
  faturamento recorrente, `PagamentoGlobal`/`ComprovantePagamento`, `ExecucaoMassa` (maker-checker),
  `ConfiguracaoGlobal` (com cache L1/L2 Redis).
- **Permissões/Menu** — `Menu`, `MenuItemNivel1/2`, `PerfilAcesso`, `PerfilAcessoMenu`.

## Estrutura

```
Domain/
  Entities/        entidades e agregados (~68)
  ValueObjects/    Cpf, Cnpj, Cep, Placa, Endereco (VO)
  Extensions/      FluntExtensions
Application/
  Commands/  Queries/  Handlers/  Dtos/  Contracts/
Infrastructure/
  Data/      ContextGestaoClientes (71 DbSets)
  Services/  ConfiguracaoGlobalCache (Redis pub/sub L1/L2)
  Jobs/      ProcessarFaturamentoRecorrenteJob, ReajusteContratoJob
Migrations/
```

## Padrões

- **CQRS**: comandos/consultas finos; regra de negócio no agregado (Flunt no construtor) e no handler.
- **Validação**: Flunt (`Notifiable`/`Contract`) nas entidades; nunca DataAnnotations de domínio.
- **Multi-tenant**: `TenantId` em toda entidade; query filters no `DbContext`; RLS habilitada por migration.

## Fidelidade ao legado — endereços (D3)

Auditoria confirmou que **dissolver as junções `PessoaEndereco`/`EmpresaEndereco` (N:N → 1:N) não
perde a capacidade de múltiplos endereços**:

- **Pessoa** mantém `ICollection<Endereco> Enderecos` (N endereços por pessoa), com as regras portadas
  do legado: no máximo um endereço `Principal` e "CEP só pode ser Nulo para estrangeiro". A dimensão
  Empresa da junção legada é preservada em `Endereco.EmpresaId?`.
- **Empresa** usa **um** endereço (value-object `Endereco`), exatamente como o legado, onde
  `Empresa.Endereco` era um único `EmpresaEndereco` (1:1) — nunca uma coleção.

Conclusão: **sem perda de capacidade; nenhuma restauração necessária.**

## Testes

`tests/Epros.Tests` — cobertura de domínio para `Pessoa` (papéis, endereço Principal, contatos),
`Endereco` (regra de entrega exige recebedor, CEP, transportadora), `Empresa`, geografia, CQRS de
cadastros, backoffice SaaS, permissões e cache de configuração global.
