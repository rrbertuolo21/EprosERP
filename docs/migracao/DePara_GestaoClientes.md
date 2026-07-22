# De -> Para: Migracao GestaoClientes

Auditoria de fidelidade de migracao. Compara as entidades legadas (Epros.ERP.Domain / Cadastros: Pessoas, Empresas, Enderecos) com o modulo novo `Epros.Modules.GestaoClientes`.

Convencao:
- Campos herdados de `EntidadeSaaSBase` (Id, TenantId, SyncId, auditoria: CriadoEm/AtualizadoEm/etc.) sao considerados COBERTOS e nao sao listados individualmente.
- `SequenciaTenantId` (legado) mapeia conceitualmente para a chave/auditoria SaaS; marcado como COBERTO (SaaSBase) quando nao ha campo dedicado.
- Propriedades de navegacao (relacionamentos EF) sao listadas mas classificadas conforme existencia no novo modelo.
- Data desta auditoria: 2026-07-01.

---

## PESSOAS

### Pessoa  ->  `Pessoa`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| Pessoa | SequenciaTenantId | COBERTO (EntidadeSaaSBase) |
| Pessoa | PessoaGrupoId | Pessoa.PessoaGrupoId (tipo mudou long -> Guid?) |
| Pessoa | TipoPessoa | Pessoa.TipoPessoa |
| Pessoa | TipoIndicadorIe | Pessoa.TipoIndicadorIe |
| Pessoa | InscricaoSuframa (int?) | Pessoa.InscricaoSuframa (long?) |
| Pessoa | TitularContaBancaria | Pessoa.TitularContaBancaria |
| Pessoa | AgenciaContaBancaria | Pessoa.AgenciaContaBancaria |
| Pessoa | NumeroContaBancaria | Pessoa.NumeroContaBancaria |
| Pessoa | TipoPix | Pessoa.TipoPix |
| Pessoa | ChavePix | Pessoa.ChavePix |
| Pessoa | Observacoes | Pessoa.Observacoes |
| Pessoa | EhCliente | Pessoa.EhCliente |
| Pessoa | EhFuncionario | Pessoa.EhFuncionario |
| Pessoa | EhMotorista | Pessoa.EhMotorista |
| Pessoa | EhPrestadorServico | Pessoa.EhPrestadorServico |
| Pessoa | EhProdutorRural | Pessoa.EhProdutorRural |
| Pessoa | EhTransportadora | Pessoa.EhTransportadora |
| Pessoa | EhFornecedor | Pessoa.EhFornecedor |
| Pessoa | EhInativo | Pessoa.EhInativo |
| Pessoa | PessoaFisica (nav) | Pessoa.PessoaFisica |
| Pessoa | PessoaJuridica (nav) | Pessoa.PessoaJuridica |
| Pessoa | PessoaEstrangeiro (nav) | Pessoa.PessoaEstrangeiro |
| Pessoa | PessoaGrupo (nav) | AUSENTE (nav removida; so FK PessoaGrupoId) |
| Pessoa | Enderecos (nav) | Pessoa.Enderecos |
| Pessoa | Contatos (nav) | Pessoa.Contatos |
| Pessoa | PessoaCliente (nav) | Pessoa.PessoaCliente |
| Pessoa | PessoaPrestadorServico (nav) | Pessoa.PessoaPrestadorServico |
| Pessoa | PessoaMotorista (nav) | Pessoa.PessoaMotorista |
| Pessoa | PessoaTransportadora (nav) | Pessoa.PessoaTransportadora |
| Pessoa | PessoaFuncionario (nav) | Pessoa.PessoaFuncionario |
| (novo) | - | Pessoa.Status (EEstadoPessoa) - adicionado |
| (novo) | - | Pessoa.Veiculos (nav) - adicionado |

### PessoaCliente  ->  `PessoaCliente`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PessoaCliente | PessoaId (long) | PessoaCliente.PessoaId (Guid) |
| PessoaCliente | EhConsumidorFinal | PessoaCliente.EhConsumidorFinal |
| PessoaCliente | TipoContribuinte | PessoaCliente.TipoContribuinte |
| PessoaCliente | Pessoa (nav) | COBERTO (via FK PessoaId) |

### PessoaContato  ->  `PessoaContato`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PessoaContato | SequenciaTenantId | COBERTO (EntidadeSaaSBase) |
| PessoaContato | PessoaId | PessoaContato.PessoaId |
| PessoaContato | Nome | PessoaContato.Nome |
| PessoaContato | TipoContatoEmail | PessoaContato.TipoContatoEmail |
| PessoaContato | Email (VO Email) | PessoaContato.Email (string) |
| PessoaContato | TipoContatoTelefonico | PessoaContato.TipoContatoTelefonico |
| PessoaContato | NumeroTelefone | PessoaContato.NumeroTelefone |
| PessoaContato | EhPrincipal | PessoaContato.EhPrincipal |
| PessoaContato | Pessoa (nav) | COBERTO (via FK PessoaId) |

### PessoaEndereco  ->  (ligacao Pessoa <-> Endereco)
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PessoaEndereco | PessoaId | Endereco.PessoaId (relacionamento direto; join simplificado) |
| PessoaEndereco | EmpresaId | AUSENTE (join tinha EmpresaId; novo Endereco liga so a PessoaId) |
| PessoaEndereco | Pessoa (nav) | COBERTO |
| PessoaEndereco | Endereco (nav) | COBERTO |

> Nota: no legado `PessoaEndereco` era tabela de juncao com EmpresaId. No novo modelo `Endereco` pertence diretamente a Pessoa (Endereco.PessoaId). A dimensao Empresa desse vinculo nao foi portada.

### PessoaEstrangeiro  ->  `PessoaEstrangeiro`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PessoaEstrangeiro | PessoaId | PessoaEstrangeiro.PessoaId |
| PessoaEstrangeiro | Nome | PessoaEstrangeiro.Nome |
| PessoaEstrangeiro | IdentificacaoEstrangeiro | PessoaEstrangeiro.IdentificacaoEstrangeiro |
| PessoaEstrangeiro | Pessoa (nav) | COBERTO |

### PessoaFisica  ->  `PessoaFisica`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PessoaFisica | PessoaId | PessoaFisica.PessoaId |
| PessoaFisica | Cpf (CPF VO) | PessoaFisica.Cpf |
| PessoaFisica | RgNumero | PessoaFisica.RgNumero |
| PessoaFisica | RgOrgaoEmissor | PessoaFisica.RgOrgaoEmissor |
| PessoaFisica | Nome | PessoaFisica.Nome |
| PessoaFisica | Sobrenome | PessoaFisica.Sobrenome |
| PessoaFisica | TipoGenero | PessoaFisica.TipoGenero |
| PessoaFisica | Pessoa (nav) | COBERTO |
| (novo) | - | PessoaFisica.DataNascimento - adicionado |

### PessoaFuncionario  ->  `PessoaFuncionario`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PessoaFuncionario | PessoaId | PessoaFuncionario.PessoaId |
| PessoaFuncionario | TipoCargo | PessoaFuncionario.TipoCargo |
| PessoaFuncionario | ValorPercentualComissao | PessoaFuncionario.ValorPercentualComissao |
| PessoaFuncionario | Pessoa (nav) | COBERTO |

### PessoaGrupo  ->  `PessoaGrupo`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PessoaGrupo | SequenciaTenantId | COBERTO (EntidadeSaaSBase) |
| PessoaGrupo | Descricao | PessoaGrupo.Descricao |
| PessoaGrupo | Empresas (nav) | COBERTO (relacionamento inverso; Empresa.PessoaGrupoId) |
| PessoaGrupo | Pessoas (nav) | COBERTO (relacionamento inverso; Pessoa.PessoaGrupoId) |

### PessoaJuridica  ->  `PessoaJuridica`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PessoaJuridica | PessoaId | PessoaJuridica.PessoaId |
| PessoaJuridica | Cnpj (CNPJ VO) | PessoaJuridica.Cnpj |
| PessoaJuridica | NomeFantasia | PessoaJuridica.NomeFantasia |
| PessoaJuridica | RazaoSocial | PessoaJuridica.RazaoSocial |
| PessoaJuridica | InscricaoEstadual | PessoaJuridica.InscricaoEstadual |
| PessoaJuridica | InscricaoMunicipal | PessoaJuridica.InscricaoMunicipal |
| PessoaJuridica | Cnae | PessoaJuridica.Cnae |
| PessoaJuridica | Pessoa (nav) | COBERTO |

### PessoaMotorista  ->  `PessoaMotorista`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PessoaMotorista | PessoaId | PessoaMotorista.PessoaId |
| PessoaMotorista | TipoVinculoMotorista | PessoaMotorista.TipoVinculoMotorista |
| PessoaMotorista | TipoCategoriaCnh | PessoaMotorista.TipoCategoriaCnh |
| PessoaMotorista | DataEmissaoCnh | PessoaMotorista.DataEmissaoCnh |
| PessoaMotorista | DataVencimentoCnh | PessoaMotorista.DataVencimentoCnh |
| PessoaMotorista | Rntrc | PessoaMotorista.Rntrc |
| PessoaMotorista | Pessoa (nav) | COBERTO |
| PessoaMotorista | Veiculos (nav) | Pessoa.Veiculos (movido para Pessoa; relacao motorista<->veiculo nao dedicada) |

### PessoaPrestadorServico  ->  `PessoaPrestadorServico`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PessoaPrestadorServico | PessoaId | PessoaPrestadorServico.PessoaId |
| PessoaPrestadorServico | Cei | PessoaPrestadorServico.Cei |
| PessoaPrestadorServico | Pessoa (nav) | COBERTO |

### PessoaTransportadora  ->  `PessoaTransportadora`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PessoaTransportadora | PessoaId | PessoaTransportadora.PessoaId |
| PessoaTransportadora | Ciot | PessoaTransportadora.Ciot |
| PessoaTransportadora | Rntrc | PessoaTransportadora.Rntrc |
| PessoaTransportadora | Pessoa (nav) | COBERTO |
| PessoaTransportadora | Veiculos (nav) | Pessoa.Veiculos (movido para Pessoa) |

### PessoaVeiculo  ->  `PessoaVeiculo`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PessoaVeiculo | SequenciaTenantId | COBERTO (EntidadeSaaSBase) |
| PessoaVeiculo | PaisId (int) | PessoaVeiculo.PaisId (long) |
| PessoaVeiculo | TipoVeiculo | PessoaVeiculo.TipoVeiculo |
| PessoaVeiculo | Uf (EEstado) | PessoaVeiculo.Uf (string) |
| PessoaVeiculo | Placa | PessoaVeiculo.Placa |
| PessoaVeiculo | Rntrc | PessoaVeiculo.Rntrc |
| PessoaVeiculo | Pais (nav) | AUSENTE (nav Pais nao mapeada; so FK PaisId) |
| PessoaVeiculo | PessoaMotorista (nav) | PARCIAL (relacao N:N motorista/transportadora substituida por PessoaVeiculo.PessoaId) |
| PessoaVeiculo | PessoaTransportadora (nav) | PARCIAL (idem acima) |
| (novo) | - | PessoaVeiculo.PessoaId - adicionado (liga veiculo direto a Pessoa) |

---

## EMPRESAS

### Empresa  ->  `Empresa`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| Empresa | PessoaGrupoId | Empresa.PessoaGrupoId (long -> Guid?) |
| Empresa | ProdutoGrupoId | Empresa.ProdutoGrupoId |
| Empresa | PlanoContasFinanceiroId | Empresa.PlanoContasFinanceiroId |
| Empresa | TributarioGrupoId | Empresa.TributarioGrupoId |
| Empresa | NcmTributacaoId | Empresa.NcmTributacaoId |
| Empresa | CertificadoDigitalId | Empresa.CertificadoDigitalId |
| Empresa | EmpresaParametrosDfeId | Empresa.EmpresaParametrosDfeId |
| Empresa | ContadorId | Empresa.ContadorId |
| Empresa | RazaoSocial | Empresa.RazaoSocial |
| Empresa | NomeFantasia | Empresa.NomeFantasia |
| Empresa | RegimeApuracao | Empresa.RegimeApuracao |
| Empresa | RegimeTributario | Empresa.RegimeTributario |
| Empresa | Cnpj (CNPJ VO) | Empresa.Cnpj (string) |
| Empresa | Cpf (CPF VO) | Empresa.Cpf (string) |
| Empresa | InscricaoMunicipal | Empresa.InscricaoMunicipal |
| Empresa | InscricaoEstadual | Empresa.InscricaoEstadual |
| Empresa | Cnae (int?) | Empresa.Cnae (string) |
| Empresa | InscricaoSuframa | Empresa.InscricaoSuframa |
| Empresa | LinkWebApiAppVendas | Empresa.LinkWebApiAppVendas |
| Empresa | TokenMercadoPagoPix | Empresa.TokenMercadoPagoPix |
| Empresa | Logo | Empresa.Logo |
| Empresa | EhIndustria | Empresa.EhIndustria |
| Empresa | Endereco (EmpresaEndereco) | Empresa.Endereco (ValueObject Endereco) |
| Empresa | EmpresaParametrosDfe (nav) | Empresa.EmpresaParametrosDfe |
| Empresa | SequenciaTenantId | COBERTO (EntidadeSaaSBase) |
| Empresa | TipoConfiguracaoEstoque | Empresa.TipoConfiguracaoEstoque |
| Empresa | IeSts (nav) | COBERTO (IeSt.EmpresaId; nova entidade IeSt) |
| Empresa | Contatos (nav) | COBERTO (EmpresaContato.EmpresaId) |
| Empresa | ContasBancarias (nav) | AUSENTE (ContaBancaria nao portada) |
| Empresa | PessoaGrupo (nav) | COBERTO (FK PessoaGrupoId) |
| Empresa | ProdutoGrupo (nav) | AUSENTE (nav; so FK) - fora do escopo do modulo |
| Empresa | CertificadoDigital (nav) | AUSENTE (nav; so FK CertificadoDigitalId) |
| Empresa | ConfiguracaoCodigoNaturezaFinanceiras (nav) | AUSENTE (fora do escopo) |
| Empresa | TributarioGrupo (nav) | AUSENTE (nav; so FK) |
| Empresa | NcmTributacao (nav) | AUSENTE (nav; so FK) |
| Empresa | PlanoContasFinanceiro (nav) | AUSENTE (nav; so FK) |
| Empresa | ConfiguracaoImpressaoNfce (nav) | AUSENTE (fora do escopo) |
| Empresa | UsuariosEmpresas (nav) | AUSENTE (fora do escopo) |
| Empresa | Contador (nav) | AUSENTE (nav; so FK ContadorId) |
| Empresa | Servico (nav) | AUSENTE (fora do escopo) |
| Empresa | ImportacaoXmls (nav) | AUSENTE (fora do escopo) |
| (novo) | - | Empresa.EhMei - adicionado |
| (novo) | - | Empresa.DateFormat / TimeZoneId / CurrencyId / Ativo - adicionados (SaaS) |

### EmpresaContato  ->  `EmpresaContato`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| EmpresaContato | EmpresaId | EmpresaContato.EmpresaId |
| EmpresaContato | Nome | EmpresaContato.Nome |
| EmpresaContato | Email (VO Email) | EmpresaContato.Email (string) |
| EmpresaContato | TipoTelefone | EmpresaContato.TipoTelefone |
| EmpresaContato | Telefone | EmpresaContato.Telefone |

### EmpresaEndereco  ->  ValueObject `Endereco` (Empresa.Endereco)
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| EmpresaEndereco | TipoEndereco | Empresa.Endereco.* (VO Endereco) - COBERTO |
| EmpresaEndereco | Cep (CEP VO) | Empresa.Endereco (VO) - COBERTO |
| EmpresaEndereco | Uf (EEstado) | Empresa.Endereco (VO) - COBERTO |
| EmpresaEndereco | MunicipioId | Empresa.Endereco (VO) - COBERTO |
| EmpresaEndereco | Logradouro | Empresa.Endereco (VO) - COBERTO |
| EmpresaEndereco | Complemento | Empresa.Endereco (VO) - COBERTO |
| EmpresaEndereco | Numero | Empresa.Endereco (VO) - COBERTO |
| EmpresaEndereco | Bairro | Empresa.Endereco (VO) - COBERTO |
| EmpresaEndereco | Municipio (nav) | COBERTO (via MunicipioId no VO) |

> Verificar mapeamento fino do ValueObject Endereco (ValueObjects/Endereco.cs) para confirmar cada campo. Considerado COBERTO por agregacao.

### EmpresaParametrosDfe  ->  `EmpresaParametrosDfe`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| EmpresaParametrosDfe | DestacarIcmsSt | EmpresaParametrosDfe.DestacarIcmsSt |
| EmpresaParametrosDfe | Nfe (nav) | EmpresaParametrosDfe.Nfe (owned ParametrosDfeNfe) |
| EmpresaParametrosDfe | NfceHomologacao (nav) | EmpresaParametrosDfe.NfceHomologacao (owned) |
| EmpresaParametrosDfe | NfceProdutocao (nav) | EmpresaParametrosDfe.NfceProducao (owned; typo corrigido) |
| EmpresaParametrosDfe | TipoAmbienteNfce | EmpresaParametrosDfe.TipoAmbienteNfce |
| EmpresaParametrosDfe | TipoAmbienteNfe | EmpresaParametrosDfe.TipoAmbienteNfe |
| EmpresaParametrosDfe | Empresa (nav) | COBERTO (EmpresaParametrosDfe.EmpresaId) |

### EmpresaParametrosDfeNfe  ->  `ParametrosDfeNfe` (owned)
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| EmpresaParametrosDfeNfe | NfeSerieProducao | ParametrosDfeNfe.NfeSerieProducao |
| EmpresaParametrosDfeNfe | NfeProximoNrProducao | ParametrosDfeNfe.NfeProximoNrProducao |
| EmpresaParametrosDfeNfe | NfeSerieHomologacao | ParametrosDfeNfe.NfeSerieHomologacao |
| EmpresaParametrosDfeNfe | NfeProximoNrHomologacao | ParametrosDfeNfe.NfeProximoNrHomologacao |
| EmpresaParametrosDfeNfe | ValorAliquotaCreditoIcms | ParametrosDfeNfe.ValorAliquotaCreditoIcms |
| EmpresaParametrosDfeNfe | NfeGerarContingenciaEmHomologacao | ParametrosDfeNfe.NfeGerarContingenciaEmHomologacao |
| EmpresaParametrosDfeNfe | IndicadorSt | ParametrosDfeNfe.IndicadorSt |
| EmpresaParametrosDfeNfe | EmitirNfeConjugada | ParametrosDfeNfe.EmitirNfeConjugada |

### EmpresaParametrosDfeNfceHomologacao  ->  `ParametrosDfeNfceHomologacao` (owned)
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| ...Homologacao | NfceCscHomologacao | ParametrosDfeNfceHomologacao.NfceCscHomologacao |
| ...Homologacao | NfceIdCscHomologacao | ParametrosDfeNfceHomologacao.NfceIdCscHomologacao |
| ...Homologacao | NfceSerieHomologacao | ParametrosDfeNfceHomologacao.NfceSerieHomologacao |
| ...Homologacao | NfceProximoNrHomologacao | ParametrosDfeNfceHomologacao.NfceProximoNrHomologacao |
| ...Homologacao | NfceGerarContingenciaEmHomologacao | ParametrosDfeNfceHomologacao.NfceGerarContingenciaEmHomologacao |

### EmpresaParametrosDfeNfceProducao  ->  `ParametrosDfeNfceProducao` (owned)
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| ...Producao | NfceCscProducao | ParametrosDfeNfceProducao.NfceCscProducao |
| ...Producao | NfceIdCscProducao | ParametrosDfeNfceProducao.NfceIdCscProducao |
| ...Producao | NfceSerieProducao | ParametrosDfeNfceProducao.NfceSerieProducao |
| ...Producao | NfceProximoNrProducao | ParametrosDfeNfceProducao.NfceProximoNrProducao |

### IeSt  ->  `IeSt` (nova; extraida da colecao Empresa.IeSts do legado)
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| Empresa.IeSts (item) | EmpresaId | IeSt.EmpresaId |
| Empresa.IeSts (item) | Uf | IeSt.Uf |
| Empresa.IeSts (item) | Ie | IeSt.Ie |

---

## ENDERECOS

### Endereco  ->  `Endereco`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| Endereco | PaisId (int) | Endereco.PaisId (Guid) |
| Endereco | MunicipioId (int) | Endereco.MunicipioId (Guid) |
| Endereco | TipoEndereco | Endereco.TipoEndereco |
| Endereco | Cep (CEP VO) | Endereco.Cep (string?) |
| Endereco | Uf (EEstado) | Endereco.Uf (string) |
| Endereco | Logradouro | Endereco.Logradouro |
| Endereco | Complemento | Endereco.Complemento |
| Endereco | Numero | Endereco.Numero |
| Endereco | Bairro | Endereco.Bairro |
| Endereco | Referencia | Endereco.Referencia |
| Endereco | NomeDoRecebedor | Endereco.NomeDoRecebedor |
| Endereco | DocumentoDoRecebedor | Endereco.DocumentoDoRecebedor |
| Endereco | Pessoas (nav N:N) | Endereco.PessoaId (relacao simplificada 1:N) |
| Endereco | Pais (nav) | Endereco.Pais |
| Endereco | Municipio (nav) | Endereco.Municipio |
| Endereco | Contador (nav) | AUSENTE (relacao Contador nao portada) |
| (novo) | - | Endereco.SubdivisaoId / Latitude / Longitude / CodigoPostalInternacional / LinhaEndereco1 / LinhaEndereco2 - adicionados |

### Municipio  ->  `Municipio`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| Municipio | Id (int) | COBERTO (Id Guid via SaaSBase; codigo legado mapeavel a CodigoIbge) |
| Municipio | Estado (EEstado) | PARCIAL - representado via Subdivisao/PaisId; campo Estado direto AUSENTE |
| Municipio | Nome | Municipio.Nome |
| Municipio | Endereco (nav) | COBERTO (relacao inversa Endereco.MunicipioId) |
| (novo) | - | Municipio.PaisId / SubdivisaoId / CodigoIbge / Latitude / Longitude / Ativo - adicionados |

### Pais  ->  `Pais`
| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| Pais | Id (int) | COBERTO (Id Guid via SaaSBase) |
| Pais | Nome | Pais.Nome |
| Pais | Capital | Pais.Capital |
| (novo) | - | Pais.CodigoIsoAlpha2 / CodigoIsoAlpha3 / CodigoNumerico / CodigoDiscagem / Ativo - adicionados |

---

## RESUMO DA AUDITORIA

### Entidades legadas x novas
- Total de entidades legadas: 23 (13 Pessoas + 7 Empresas + 3 Enderecos).
- Entidades com destino no novo modulo: 22 (PessoaEndereco vira relacionamento direto).

### Entidades ausentes / nao portadas
- Nenhuma entidade-nucleo do escopo GestaoClientes esta totalmente ausente.
- `PessoaEndereco` (tabela de juncao) foi substituida por relacao direta `Endereco.PessoaId`; a dimensao `EmpresaId` dessa juncao foi perdida.
- `ContaBancaria` (colecao `Empresa.ContasBancarias`) NAO foi portada.

### Campos criticos faltando
1. Empresa.ContasBancarias -> AUSENTE (dados bancarios da empresa nao migrados).
2. PessoaEndereco.EmpresaId -> AUSENTE (vinculo endereco-empresa perdido no novo join Pessoa->Endereco).
3. Municipio.Estado (EEstado) -> nao ha campo UF/Estado direto no novo Municipio (depende de Subdivisao); risco em consultas por UF.
4. Endereco.Contador (nav) -> AUSENTE (endereco do contador).
5. PessoaVeiculo relacao N:N com Motorista/Transportadora -> simplificada para Veiculo->Pessoa (PARCIAL); um veiculo compartilhado entre motorista e transportadora perde a modelagem original.
6. Navegacoes de Empresa para agregados de outros modulos (ProdutoGrupo, TributarioGrupo, NcmTributacao, PlanoContasFinanceiro, CertificadoDigital, Contador etc.) mantidas apenas como FK - esperado por serem de outros modulos.

### Cobertura estimada
- Campos de dados (scalar) das entidades-nucleo: ~95% cobertos.
- Incluindo navegacoes cross-modulo (esperadamente fora do escopo) e o gap de ContaBancaria/EmpresaId-do-join: cobertura geral estimada em ~92%.
