# EF_CADASTROS_FISCAIS_V1

## 1. Identificacao

| Item | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | Plataforma Compartilhada |
| Submodulo | Faturamento Fiscal Eletronico |
| Documento | Especificacao funcional - Cadastros fiscais |
| Versao | V1 |
| Status | Concluido |

## 2. Objetivo funcional

Os cadastros fiscais mantem os codigos, tabelas, parametros e relacionamentos usados pelo Epros para emissao, validacao e calculo fiscal. Eles devem permitir cadastrar e manter CFOP, CFOP padrao, NCM, tributacao por NCM e grupo, ST, FCP, tipo de operacao fiscal, beneficios fiscais, observacoes fiscais, CEST, codigo ANP, enquadramento IPI, aliquotas FCP por UF, aliquotas ICMS interestaduais e classificacoes IBS/CBS quando o material traz estrutura disponivel.

O objetivo e tornar os cadastros fiscais uma base operacional validavel, com campos, tamanhos, dominios e chaves funcionais suficientes para implantacao e homologacao humana.

## 3. Escopo

| Area | Incluso |
|---|---|
| CFOP | Cadastro por tenant, base padrao, vigencia, indicadores fiscais, CFOP correlacionado e CFOP de devolucao. |
| NCM | Cadastro NCM com codigo, descricao, vigencia e ato normativo. |
| Tributacao por NCM | Regras por grupo tributario, CFOPs, origem, CSOSN, CST ICMS, PIS, COFINS, IPI, ICMS, IBS/CBS e textos fiscais. |
| ST por NCM | Parametros por UF, tipo de calculo, aliquota, MVA, reducao, valor unitario e FCP ST. |
| FCP por regra NCM | Percentual por UF vinculado a regra de tributacao. |
| Grupo tributario | Agrupamento fiscal por tenant usado por empresas, produtos e regras. |
| Tipo de operacao fiscal | Natureza operacional com grupo, CFOP NF-e/NFC-e, finalidade, atendimento, frete e movimento. |
| Beneficio fiscal | Codigo, descricao, UF, relacao com CSOSN e CST. |
| Observacao NF-e | Textos complementares para documento fiscal. |
| CEST | Codigo e descricao de classificacao auxiliar. |
| Codigo ANP | Codigo, descricao e vigencia. |
| Enquadramento IPI | Codigo, descricao e tipo de operacao. |
| FCP UF | Aliquota FCP por UF e observacao. |
| ICMS interestadual | Aliquota por UF origem e UF destino. |
| IBS/CBS | CST, classificacoes, anexos e indicadores por modelo fiscal conforme estrutura disponivel. |

## 4. Principios funcionais

| Principio | Regra |
|---|---|
| Tenant | Cadastros com `TenantId` devem ser isolados por tenant. |
| Vigencia | Tabelas com data de inicio/fim devem respeitar periodo valido. |
| Validacao antes do uso | CFOP, NCM, grupos, beneficios e aliquotas devem existir antes de serem usados em emissao ou calculo. |
| Auditoria | Alteracoes em CFOP, NCM, beneficios, grupo tributario, regras e aliquotas devem registrar trilha de alteracao. |
| Cache | Alteracoes em aliquotas e regras fiscais devem invalidar cache quando aplicavel. |
| Sem duplicidade funcional | Codigos e regras com unicidade funcional devem bloquear duplicidade antes de salvar. |

## 5. Capacidades operacionais

| Capacidade | Funcao | Acoes | Resultado |
|---|---|---|---|
| Manter CFOP | Gerenciar CFOP ativo/inativo por tenant. | Criar, editar, excluir/inativar, ativar base padrao e listar por filtros. | CFOP disponivel para tipos de operacao e regras fiscais. |
| Atualizar CFOP padrao | Carregar tabela padrao de CFOP. | Atualizar por carga. | Base padrao atualizada e disponivel para ativacao. |
| Manter NCM | Consultar/manter tabela NCM. | Consultar e atualizar carga. | NCM disponivel para produtos, regras e IBPT. |
| Manter tributacao NCM | Gerenciar regras por grupo. | Criar, editar, excluir e consultar por grupo. | Matriz fiscal por grupo disponivel para emissao/calculo. |
| Manter tipo de operacao | Definir natureza operacional. | Criar, editar e excluir. | Operacao fiscal vinculada a grupo e CFOPs. |
| Manter observacoes NF-e | Gerenciar textos complementares. | Criar, editar, excluir, localizar e paginar. | Texto fiscal disponivel para documento. |
| Manter beneficio fiscal | Gerenciar beneficio por UF. | Criar, editar e excluir. | Beneficio fiscal disponivel para regras de ICMS. |
| Manter CEST | Consultar codigo e descricao. | Listar e buscar por codigo. | Catalogo auxiliar disponivel para emissao. |
| Manter codigo ANP | Consultar codigo vigente. | Listar e buscar por codigo. | Catalogo vigente disponivel para produtos/operacoes aplicaveis. |
| Manter enquadramento IPI | Consultar enquadramento. | Listar e buscar por codigo. | Enquadramento disponivel para tributacao IPI. |
| Manter FCP UF | Atualizar aliquotas FCP. | Criar, editar, excluir e atualizar por carga. | Aliquotas disponiveis e cache invalidado. |
| Manter ICMS interestadual | Gerenciar aliquota origem/destino. | Criar, editar e excluir. | Aliquota disponivel e cache invalidado. |

## 6. Regras funcionais

| Codigo | Regra |
|---|---|
| CADFIS-001 | CFOP deve possuir codigo e deve respeitar o tenant quando o cadastro for tenant. |
| CADFIS-002 | CFOP padrao deve conter vigencia inicial e pode conter vigencia final. |
| CADFIS-003 | CFOP deve possuir descricao, natureza de operacao, indicadores fiscais e, quando houver, CFOP correlacionado e CFOP de devolucao. |
| CADFIS-004 | CFOP para listagem operacional deve considerar indicador MEI quando o regime da empresa exigir. |
| CADFIS-005 | CFOP para tipo de operacao deve permitir filtro por entrada/saida a partir do codigo CFOP. |
| CADFIS-006 | Ativacao em lote de CFOP deve usar registros marcados para inclusao/ativacao. |
| CADFIS-007 | Exclusao em lote de CFOP deve usar registros marcados para exclusao/inativacao. |
| CADFIS-008 | NCM deve possuir codigo de 8 caracteres e descricao de ate 1500 caracteres. |
| CADFIS-009 | NCM pode possuir vigencia, tipo de ato inicial, numero de ato inicial e ano de ato inicial. |
| CADFIS-010 | Atualizacao NCM por carga deve registrar usuario, data e quantidade de registros quando disponivel. |
| CADFIS-011 | Regra de tributacao NCM deve estar vinculada a grupo tributario valido. |
| CADFIS-012 | `CodRegra` deve ser maior que zero e unico no grupo tributario. |
| CADFIS-013 | Descricao de regra de tributacao NCM deve respeitar limite funcional de 200 caracteres e persistencia text. |
| CADFIS-014 | CFOP informado na regra de tributacao deve existir. |
| CADFIS-015 | NCM vinculado a configuracao deve existir e nao pode estar duplicado na mesma regra quando o material indicar vinculo unico. |
| CADFIS-016 | Origem da mercadoria deve pertencer ao dominio valido. |
| CADFIS-017 | CSOSN de NFC-e e NF-e deve pertencer ao dominio valido. |
| CADFIS-018 | CST ICMS de NFC-e, NF-e interna e NF-e interestadual deve pertencer ao dominio valido. |
| CADFIS-019 | CST PIS e CST COFINS devem pertencer ao dominio valido. |
| CADFIS-020 | CST IPI saida e entrada devem pertencer ao dominio valido. |
| CADFIS-021 | Nao pode ser selecionado CST de entrada no campo de CST de saida. |
| CADFIS-022 | Nao pode ser selecionado CST de saida no campo de CST de entrada. |
| CADFIS-023 | Enquadramento IPI na regra NCM deve ter ate 3 caracteres. |
| CADFIS-024 | Codigo de beneficio fiscal ICMS na regra NCM deve ter ate 10 caracteres. |
| CADFIS-025 | Informacoes complementares da regra NCM devem ter ate 5000 caracteres. |
| CADFIS-026 | Informacoes adicionais ao fisco da regra NCM devem ter ate 2000 caracteres. |
| CADFIS-027 | Deve ser informado ao menos um dado IBS/CBS quando a regra exigir classificacao da reforma tributaria. |
| CADFIS-028 | Classificacao tributaria NF-e deve ser informada quando a regra de IBS/CBS exigir NF-e. |
| CADFIS-029 | Classificacao tributaria NFC-e deve ser informada quando a regra de IBS/CBS exigir NFC-e. |
| CADFIS-030 | ST por NCM deve estar vinculada a regra de tributacao NCM. |
| CADFIS-031 | UF de ST por NCM deve ter ate 2 caracteres. |
| CADFIS-032 | Tipo de calculo ST deve pertencer ao dominio `MargemAgregada=0` ou `ValorFixo=1` quando esse dominio for usado. |
| CADFIS-033 | FCP por regra NCM deve estar vinculado a regra de tributacao NCM. |
| CADFIS-034 | UF de FCP por regra NCM deve ter ate 2 caracteres. |
| CADFIS-035 | Grupo tributario deve possuir `TenantId` e descricao obrigatorios. |
| CADFIS-036 | Descricao do grupo tributario deve ter ate 100 caracteres. |
| CADFIS-037 | Tipo de operacao fiscal deve possuir grupo tributario maior que zero. |
| CADFIS-038 | Descricao do tipo de operacao fiscal deve ter ate 150 caracteres. |
| CADFIS-039 | Finalidade, atendimento, tipo de frete e tipo de movimento do tipo de operacao devem pertencer aos dominios validos. |
| CADFIS-040 | `SobescreveTributacaoNcm` do tipo de operacao fiscal e obrigatorio. |
| CADFIS-041 | Descricao duplicada de tipo de operacao fiscal deve ser bloqueada. |
| CADFIS-042 | CFOP NF-e e CFOP NFC-e do tipo de operacao devem existir quando informados ou exigidos. |
| CADFIS-043 | Beneficio fiscal deve possuir codigo de ate 10 caracteres. |
| CADFIS-044 | Beneficio fiscal deve possuir descricao de ate 1000 caracteres. |
| CADFIS-045 | UF do beneficio fiscal deve pertencer a lista de UFs validas. |
| CADFIS-046 | Beneficio fiscal deve exigir ao menos um CSOSN ou CST. |
| CADFIS-047 | Beneficio fiscal deve ser unico por codigo e UF. |
| CADFIS-048 | Observacao NF-e deve possuir descricao de ate 5000 caracteres. |
| CADFIS-049 | Consulta de observacao NF-e deve aceitar localizar, pagina e tamanho de pagina. |
| CADFIS-050 | Retorno de observacao NF-e deve conter dados, mensagem, indicador de sucesso, total de paginas e total de registros quando usado na consulta. |
| CADFIS-051 | CEST deve possuir codigo de ate 7 caracteres e descricao de ate 1000 caracteres quando persistido. |
| CADFIS-052 | Codigo ANP deve considerar vigencia ativa na consulta por codigo. |
| CADFIS-053 | Enquadramento IPI deve possuir codigo de ate 7 caracteres e descricao de ate 500 caracteres. |
| CADFIS-054 | Tipo de operacao de enquadramento IPI deve pertencer ao dominio `Imunidade=1`, `Suspensao=2`, `Isencao=3`, `Reducao=4` ou `Outros=5`. |
| CADFIS-055 | FCP por UF deve possuir UF obrigatoria de 2 caracteres e aliquota decimal. |
| CADFIS-056 | Alteracao em FCP por UF deve invalidar cache quando aplicavel. |
| CADFIS-057 | ICMS interestadual deve possuir UF origem, UF destino e aliquota decimal. |
| CADFIS-058 | Alteracao em ICMS interestadual deve invalidar cache quando aplicavel. |
| CADFIS-059 | CST IBS/CBS deve possuir CST, descricao, data de inicio de vigencia e data de cadastro. |
| CADFIS-060 | Classificacao IBS/CBS deve estar vinculada ao CST IBS/CBS. |
| CADFIS-061 | Classificacao IBS/CBS deve indicar aplicabilidade por modelo quando os indicadores forem informados. |
| CADFIS-062 | Anexo de classificacao IBS/CBS deve estar vinculado a classificacao e informar numero de anexo quando disponivel. |

## 7. Estados, dominios e validacoes

| Dominio | Valores informados |
|---|---|
| Tipo de calculo ST | MargemAgregada=0, ValorFixo=1 |
| Tipo de operacao de enquadramento IPI | Imunidade=1, Suspensao=2, Isencao=3, Reducao=4, Outros=5 |
| Tipo CFOP | NaoDefinido=0, DentroEstadoEntrada=1, DentroEstadoSaida=5, ForaEstadoEntrada=2, ForaEstadoSaida=6, ExteriorImportacao=3, ExteriorExportacao=7 |
| CSOSN | NaoUtiliza=-1, Csosn101=101, Csosn102=102, Csosn103=103, Csosn201=201, Csosn202=202, Csosn203=203, Csosn300=300, Csosn400=400, Csosn500=500, Csosn900=900 |
| CST ICMS | Cst00=0, Cst02=2, Cst10=10, Cst12=12, Cst13=13, Cst15=15, Cst20=20, Cst30=30, Cst40=40, Cst41=41, Cst50=50, Cst51=51, Cst52=52, Cst53=53, Cst60=60, Cst61=61, Cst70=70, Cst72=72, Cst74=74, Cst90=90 |
| CST IPI | Cst00=00, Cst01=01, Cst02=02, Cst03=03, Cst04=04, Cst05=05, Cst49=49, Cst50=50, Cst51=51, Cst52=52, Cst53=53, Cst54=54, Cst55=55, Cst99=99 |
| CST PIS/COFINS | Cst01=01, Cst02=02, Cst03=03, Cst04=04, Cst05=05, Cst06=06, Cst07=07, Cst08=08, Cst09=09, Cst49=49, Cst50=50, Cst51=51, Cst52=52, Cst53=53, Cst54=54, Cst55=55, Cst56=56, Cst60=60, Cst61=61, Cst62=62, Cst63=63, Cst64=64, Cst65=65, Cst66=66, Cst67=67, Cst70=70, Cst71=71, Cst72=72, Cst73=73, Cst74=74, Cst75=75, Cst98=98, Cst99=99 |
| Codigo valor fiscal ICMS | NaoUtiliza=-1, Tributado=0, Isento=1, Outros=2 |

## 8. Modelo de dados funcional e implantavel

### 8.1 Entidades

| Entidade | Finalidade | Cardinalidade | Observacao |
|---|---|---|---|
| `cfop` | Manter CFOP operacional por tenant. | 0..N por tenant | Usado por tipo de operacao e regras fiscais. |
| `cfop_padrao` | Manter base padrao de CFOP com vigencia. | 0..N | Usado para carga/ativacao de CFOP. |
| `ncm` | Manter NCM e vigencia. | 0..N por tenant quando aplicavel | Usado por produto, tributacao e IBPT. |
| `ncm_configuracao` | Vincular NCM a configuracao tributaria. | 0..N por NCM | Possui indice por `NcmId`. |
| `ncm_tributacao` | Manter regra tributaria por grupo. | 0..N por grupo tributario | `CodRegra` unico por grupo. |
| `ncm_tributacao_st` | Manter ST por UF para regra NCM. | 0..N por regra NCM | Complementa ICMS ST. |
| `ncm_tributacao_fundo_combate_pobreza` | Manter FCP por UF para regra NCM. | 0..N por regra NCM | Complementa FCP. |
| `tributario_grupo` | Agrupar regras fiscais por tenant. | 0..N por tenant | Empresa pode referenciar grupo tributario. |
| `tipo_operacao_fiscal` | Definir natureza fiscal por grupo e CFOPs. | 0..N por grupo tributario | Pode sobrescrever tributacao NCM. |
| `codigo_beneficio_fiscal` | Manter beneficio por codigo e UF. | 0..N por tenant | Exige CSOSN ou CST. |
| `codigo_beneficio_fiscal_csosn` | Relacionar beneficio a CSOSN. | 0..N por beneficio | Filho do beneficio fiscal. |
| `codigo_beneficio_fiscal_cst` | Relacionar beneficio a CST. | 0..N por beneficio | Filho do beneficio fiscal. |
| `observacao_nfe` | Manter textos complementares fiscais. | 0..N por tenant | Usado em NF-e. |
| `cest` | Manter codigo CEST e descricao. | 0..N | Catalogo auxiliar. |
| `codigo_anp` | Manter codigo ANP, descricao e vigencia. | 0..N | Consulta deve respeitar vigencia ativa. |
| `enquadramento_ipi` | Manter enquadramento IPI. | 0..N | Usado pela regra NCM. |
| `fcp_aliquota_uf` | Manter aliquota FCP por UF. | 0..N | Alteracao invalida cache quando aplicavel. |
| `icms_aliquota_interestadual` | Manter aliquota por UF origem/destino. | 0..N | Alteracao invalida cache quando aplicavel. |
| `cst_ibs_cbs` | Manter CST IBS/CBS. | 0..N | Possui vigencia e data de cadastro. |
| `classificacao_ibs_cbs` | Manter classificacoes vinculadas ao CST IBS/CBS. | 0..N por CST | Possui indicadores por modelo. |
| `anexo_classificacao_ibs_cbs` | Manter anexos da classificacao IBS/CBS. | 0..N por classificacao | Possui numero de anexo quando informado. |

### 8.2 Relacionamentos

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| Tenant | possui | `cfop` | Isolamento por `TenantId`. |
| Tenant | possui | `ncm` | Isolamento por `TenantId` quando informado. |
| `ncm` | possui | `ncm_configuracao` | Configuracao referencia `NcmId`. |
| `tributario_grupo` | possui | `ncm_tributacao` | Uma regra pertence a um grupo. |
| `codigo_beneficio_fiscal` | pode ser usado por | `ncm_tributacao` | Beneficio fiscal e opcional na regra. |
| `ncm_tributacao` | possui | `ncm_tributacao_st` | ST por UF complementa a regra. |
| `ncm_tributacao` | possui | `ncm_tributacao_fundo_combate_pobreza` | FCP por UF complementa a regra. |
| `tributario_grupo` | possui | `tipo_operacao_fiscal` | Tipo de operacao exige grupo valido. |
| `tipo_operacao_fiscal` | referencia | `cfop` | CFOP NF-e e NFC-e devem existir quando usados. |
| `codigo_beneficio_fiscal` | possui | `codigo_beneficio_fiscal_csosn` | Beneficio pode se aplicar a CSOSN. |
| `codigo_beneficio_fiscal` | possui | `codigo_beneficio_fiscal_cst` | Beneficio pode se aplicar a CST. |
| `cst_ibs_cbs` | possui | `classificacao_ibs_cbs` | Classificacao pertence ao CST. |
| `classificacao_ibs_cbs` | possui | `anexo_classificacao_ibs_cbs` | Anexos pertencem a classificacao. |

### 8.3 Unicidades e indices funcionais

| Entidade | Regra |
|---|---|
| `cfop` | `CfopCodigo` por tenant deve ser unico funcionalmente. |
| `codigo_beneficio_fiscal` | Codigo + UF deve ser unico. |
| `ncm_tributacao` | `CodRegra` deve ser unico por `TributarioGrupoId`. |
| `ncm_configuracao` | Indice por `NcmId`. |
| `fcp_aliquota_uf` | UF deve identificar aliquota vigente quando usada. |
| `icms_aliquota_interestadual` | UF origem + UF destino deve identificar aliquota vigente quando usada. |

## 9. Dicionario de dados implantavel

### 9.1 `cfop`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| TenantId | Texto | varchar(200) | Sim | Tenant | Isolamento por tenant. |
| CfopCodigo | Numero/codigo | 4 caracteres quando informado | Sim | Indice funcional | Codigo CFOP. |
| Descricao | Texto | varchar(1000) | Nao informado no material | Informativo | Descricao da operacao. |
| NaturezaOperacao | Texto | varchar(1000) | Nao informado no material | Informativo | Natureza fiscal. |
| CfopCorrelacao | Texto | varchar(4) | Nao | Informativo | CFOP correlacionado. |
| IntegraFaturamento | Booleano | Nao informado no material | Nao informado no material | Regra | Indica integracao com faturamento. |
| IndicadorNfe | Booleano | Nao informado no material | Nao informado no material | Regra | Uso em NF-e. |
| IndicadorComunicacao | Booleano | Nao informado no material | Nao informado no material | Regra | Uso comunicacao. |
| IndicadorTransporte | Booleano | Nao informado no material | Nao informado no material | Regra | Uso transporte. |
| IndicadorDevolucao | Booleano | Nao informado no material | Nao informado no material | Regra | Uso devolucao. |
| IndicadorRetorno | Booleano | Nao informado no material | Nao informado no material | Regra | Uso retorno. |
| IndicadorAnulacao | Booleano | Nao informado no material | Nao informado no material | Regra | Uso anulacao. |
| IndicadorRemessa | Booleano | Nao informado no material | Nao informado no material | Regra | Uso remessa. |
| IndicadorCombustivel | Booleano | Nao informado no material | Nao informado no material | Regra | Uso combustivel. |
| IndicadorTransferencia | Booleano | Nao informado no material | Nao informado no material | Regra | Uso transferencia. |
| IndicadorNfce | Booleano | Nao informado no material | Nao informado no material | Regra | Uso NFC-e. |
| IndicadorCiap | Booleano | Nao informado no material | Nao informado no material | Regra | Uso CIAP. |
| IndicadorUsoConsumo | Booleano | Nao informado no material | Nao informado no material | Regra | Uso/consumo. |
| IndicadorUsoSemOperacao | Booleano | Nao informado no material | Nao informado no material | Regra | Uso sem operacao. |
| IndicadorSt | Booleano | Nao informado no material | Nao informado no material | Regra | Substituicao tributaria. |
| IndicadorMei | Booleano | Nao informado no material | Nao informado no material | Regra | Uso MEI. |
| IncidenciaSimples | Enum | Dominio EIncidenciaSimples | Nao informado no material | Regra | Deve pertencer ao dominio. |
| CfopDevolucao | Texto | varchar(4) | Nao | Informativo | CFOP de devolucao. |

### 9.2 `cfop_padrao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| TenantId | Texto | varchar(200) | Sim | Tenant | Obrigatorio no material consolidado. |
| CfopCodigo | Numero/codigo | 4 caracteres quando informado | Sim | Indice funcional | Codigo CFOP padrao. |
| DataInicioVigencia | Data | Nao informado no material | Nao informado no material | Vigencia | Inicio da vigencia. |
| DataFimVigencia | Data | Nao informado no material | Nao | Vigencia | Fim da vigencia. |
| Descricao | Texto | varchar(1000) | Nao informado no material | Informativo | Descricao da operacao. |
| NaturezaOperacao | Texto | varchar(1000) | Nao informado no material | Informativo | Natureza fiscal. |
| CfopCorrelacao | Texto | varchar(4) | Nao | Informativo | CFOP correlacionado. |
| IntegraFaturamento | Booleano | Nao informado no material | Nao informado no material | Regra | Indica integracao com faturamento. |
| IndicadorNfe | Booleano | Nao informado no material | Nao informado no material | Regra | Uso NF-e. |
| IndicadorComunicacao | Booleano | Nao informado no material | Nao informado no material | Regra | Uso comunicacao. |
| IndicadorTransporte | Booleano | Nao informado no material | Nao informado no material | Regra | Uso transporte. |
| IndicadorDevolucao | Booleano | Nao informado no material | Nao informado no material | Regra | Uso devolucao. |
| IndicadorRetorno | Booleano | Nao informado no material | Nao informado no material | Regra | Uso retorno. |
| IndicadorAnulacao | Booleano | Nao informado no material | Nao informado no material | Regra | Uso anulacao. |
| IndicadorRemessa | Booleano | Nao informado no material | Nao informado no material | Regra | Uso remessa. |
| IndicadorCombustivel | Booleano | Nao informado no material | Nao informado no material | Regra | Uso combustivel. |
| IndicadorTransferencia | Booleano | Nao informado no material | Nao informado no material | Regra | Uso transferencia. |
| IndicadorNfce | Booleano | Nao informado no material | Nao informado no material | Regra | Uso NFC-e. |
| IndicadorCiap | Booleano | Nao informado no material | Nao informado no material | Regra | Uso CIAP. |
| IndicadorUsoConsumo | Booleano | Nao informado no material | Nao informado no material | Regra | Uso/consumo. |
| IndicadorUsoSemOperacao | Booleano | Nao informado no material | Nao informado no material | Regra | Uso sem operacao. |
| IndicadorSt | Booleano | Nao informado no material | Nao informado no material | Regra | Substituicao tributaria. |
| IndicadorMei | Booleano | Nao informado no material | Nao informado no material | Regra | Uso MEI. |
| IncidenciaSimples | Enum | Dominio EIncidenciaSimples | Nao informado no material | Regra | Deve pertencer ao dominio. |
| CfopDevolucao | Texto | varchar(4) | Nao | Informativo | CFOP de devolucao. |

### 9.3 `ncm`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| TenantId | Texto | varchar(200) | Nao informado no material | Tenant | Presente no cadastro. |
| CodigoNcm | Texto | char(8) | Sim | Indice | Codigo NCM. |
| Descricao | Texto | varchar(1500) | Sim | Informativo | Descricao NCM. |
| DataInicio | Data | Nao informado no material | Nao informado no material | Vigencia | Inicio da vigencia. |
| DataFim | Data | Nao informado no material | Nao | Vigencia | Fim da vigencia. |
| TipoAtoIni | Texto | varchar(200) | Nao | Informativo | Tipo do ato inicial. |
| NumeroAtoIni | Texto | varchar(60) | Nao | Informativo | Numero do ato inicial. |
| AnoAtoIni | Texto | varchar(4) | Nao | Informativo | Ano do ato inicial. |

### 9.4 `ncm_configuracao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| TenantId | Texto | varchar(200) | Nao informado no material | Tenant | Isolamento por tenant. |
| NcmId | Identificador | Nao informado no material | Sim | NCM | Indice por NCM. |
| NcmTributacaoId | Identificador | Nao informado no material | Condicional | Regra tributaria | Vinculo com regra NCM quando informado. |
| SequenciaTenantId | Numero | Nao informado no material | Nao informado no material | Exibicao | Sequencial do tenant. |

### 9.5 `ncm_tributacao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| TenantId | Texto | varchar(200) | Nao informado no material | Tenant | Isolamento por tenant. |
| TributarioGrupoId | Numero | Nao informado no material | Sim | Grupo tributario | Deve ser maior que zero. |
| CodigoBeneficioFiscalId | Numero | Nao informado no material | Nao | Beneficio fiscal | Beneficio associado. |
| CodRegra | Numero | Nao informado no material | Sim | Unico funcional | Deve ser maior que zero e unico por grupo. |
| Descricao | Texto | text; limite funcional 200 caracteres | Sim | Informativo | Descricao da regra. |
| CfopNotaConsumidor | Numero | Nao informado no material | Nao informado no material | CFOP | CFOP NFC-e. |
| CfopNotaFiscal | Numero | Nao informado no material | Nao informado no material | CFOP | CFOP NF-e interna. |
| CfopNotaFiscalInterestadual | Numero | Nao informado no material | Nao informado no material | CFOP | CFOP NF-e interestadual. |
| Origem | Enum | Dominio origem mercadoria | Nao informado no material | Regra | Deve pertencer ao dominio. |
| CsosnNotaConsumidor | Enum | Dominio CSOSN | Nao informado no material | Regra | CSOSN NFC-e. |
| CstIcmsNotaConsumidor | Enum | Dominio CST ICMS | Nao informado no material | Regra | CST ICMS NFC-e. |
| CsosnNotaFiscal | Enum | Dominio CSOSN | Nao informado no material | Regra | CSOSN NF-e. |
| CstIcmsNotaFiscalInterna | Enum | Dominio CST ICMS | Nao informado no material | Regra | CST NF-e interna. |
| CstIcmsNotaFiscalInterstadual | Enum | Dominio CST ICMS | Nao informado no material | Regra | CST NF-e interestadual. |
| CstPis | Enum | Dominio CST PIS/COFINS | Nao informado no material | Regra | CST PIS. |
| CstCofins | Enum | Dominio CST PIS/COFINS | Nao informado no material | Regra | CST COFINS. |
| ValorUnitFixoPis | Decimal | decimal(11,4) | Nao | Valor | Valor unitario fixo PIS. |
| ValorUnitFixoCofins | Decimal | decimal(11,4) | Nao | Valor | Valor unitario fixo COFINS. |
| ValorAliquotaPis | Decimal | decimal(11,4) | Nao | Aliquota | Aliquota PIS. |
| ValorAliquotaCofins | Decimal | decimal(11,4) | Nao | Aliquota | Aliquota COFINS. |
| CstPisCofinsEntrada | Enum | Dominio CST PIS/COFINS | Nao | Regra | Entrada. |
| CstIpiSaida | Enum | Dominio CST IPI | Nao | Regra | Saida. |
| CstIpiEntrada | Enum | Dominio CST IPI | Nao | Regra | Entrada. |
| ValorAliquotaIpi | Decimal | decimal(11,4) | Nao | Aliquota | Aliquota IPI. |
| ValorPercentualReducacaoBcIpi | Decimal | decimal(11,4) | Nao | Reducao | Reducao BC IPI. |
| TipoReducaoIpi | Enum | Dominio reducao | Nao | Regra | Tipo reducao IPI. |
| DestinoReducaoIpi | Enum | Dominio destino reducao | Nao | Regra | Destino reducao IPI. |
| IpiEmbutido | Booleano | Nao informado no material | Nao | Regra | Indica IPI embutido. |
| EnquadramentoIpi | Texto | char(3) | Nao | Enquadramento IPI | Enquadramento IPI da regra. |
| CodigoValorFiscalIcmsInterna | Enum | Dominio codigo valor fiscal | Nao | Regra | Valor fiscal interno. |
| CodigoValorFiscalcmsInterstadual | Enum | Dominio codigo valor fiscal | Nao | Regra | Valor fiscal interestadual. |
| ValorAliquotaIcmsInterna | Decimal | decimal(11,4) | Nao | Aliquota | Aliquota interna. |
| ValorPercentualReducacaoBcIcmsInterna | Decimal | decimal(11,4) | Nao | Reducao | Reducao interna. |
| TipoReducaoIcmsInterna | Enum | Dominio reducao | Nao | Regra | Tipo reducao interna. |
| DestinoReducaoIcmsInterna | Enum | Dominio destino reducao | Nao | Regra | Destino reducao interna. |
| ValorAliquotaIcmsInterstadual | Decimal | decimal(11,4) | Nao | Aliquota | Aliquota interestadual. |
| ValorPercentualReducacaoBcIcmsInterstadual | Decimal | decimal(11,4) | Nao | Reducao | Reducao interestadual. |
| TipoReducaoIcmsInterstadual | Enum | Dominio reducao | Nao | Regra | Tipo reducao interestadual. |
| DestinoReducaoIcmsInterstadual | Enum | Dominio destino reducao | Nao | Regra | Destino reducao interestadual. |
| CodigoBeneficioFiscalIcms | Texto | varchar(10) | Nao | Beneficio fiscal | Codigo de beneficio fiscal ICMS. |
| MotivoDesoneracaoIcms | Numero/enum | Nao informado no material | Nao | Regra | Motivo de desoneracao. |
| InformacoesComplementares | Texto | varchar(5000) | Nao | Texto fiscal | Texto complementar. |
| InformacoesAdicionaisAoFisco | Texto | varchar(2000) | Nao | Texto fiscal | Texto ao fisco. |
| SequenciaTenantId | Numero | Nao informado no material | Nao informado no material | Exibicao | Sequencial do tenant. |
| CstIbsCbsNfe | Texto | varchar(5000) | Condicional | IBS/CBS | CST IBS/CBS NF-e. |
| CClassTribNfe | Texto | varchar(5000) | Condicional | IBS/CBS | Classificacao NF-e. |
| CstIbsCbsNfce | Texto | varchar(5000) | Condicional | IBS/CBS | CST IBS/CBS NFC-e. |
| CClassTribNfce | Texto | varchar(5000) | Condicional | IBS/CBS | Classificacao NFC-e. |

### 9.6 `ncm_tributacao_st`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| TenantId | Texto | varchar(200) | Nao informado no material | Tenant | Isolamento por tenant. |
| NcmTributacaoId | Numero | Nao informado no material | Sim | Regra NCM | Regra tributaria. |
| Uf | Texto | char(2) | Nao informado no material | UF | UF da regra. |
| TipoCalculo | Enum | MargemAgregada=0, ValorFixo=1 | Nao informado no material | Regra | Tipo de calculo ST. |
| ValorAliquotaIcmsSt | Decimal | decimal(11,4) | Nao | Aliquota | Aliquota ST. |
| ValorMva | Decimal | decimal(11,4) | Nao | MVA | Margem de valor agregado. |
| ValorPercentualReducaoBcIcmsSt | Decimal | decimal(11,4) | Nao | Reducao | Reducao BC ST. |
| TipoReducaoIcmsSt | Enum/numero | Nao informado no material | Nao | Regra | Tipo reducao ST. |
| ValorUnitarioSt | Decimal | decimal(15,4) | Nao | Valor | Valor unitario ST. |
| ValorPercentualFcpSt | Decimal | decimal(11,4) | Nao | FCP ST | FCP ST. |

### 9.7 `ncm_tributacao_fundo_combate_pobreza`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| TenantId | Texto | varchar(200) | Nao informado no material | Tenant | Isolamento por tenant. |
| NcmTributacaoId | Numero | Nao informado no material | Sim | Regra NCM | Regra tributaria. |
| Uf | Texto | char(2) | Nao informado no material | UF | UF do FCP. |
| ValorPercentual | Decimal | decimal(11,4) | Nao | Percentual | Percentual FCP por UF. |

### 9.8 `tributario_grupo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| TenantId | Texto | varchar(200) | Sim | Tenant | Isolamento por tenant. |
| Descricao | Texto | varchar(100) | Sim | Informativo | Descricao obrigatoria. |
| SequenciaTenantId | Numero | Nao informado no material | Nao informado no material | Exibicao | Sequencial do tenant. |

### 9.9 `tipo_operacao_fiscal`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| TenantId | Texto | varchar(200) | Sim | Tenant | Isolamento por tenant. |
| SequenciaTenantId | Numero | Nao informado no material | Nao informado no material | Exibicao | Sequencial de exibicao. |
| TributarioGrupoId | Numero | Nao informado no material | Sim | Grupo tributario | Deve ser maior que zero. |
| CfopNfeId | Numero | Nao informado no material | Condicional | CFOP NF-e | CFOP para NF-e. |
| CfopNfceId | Numero | Nao informado no material | Condicional | CFOP NFC-e | CFOP para NFC-e. |
| Descricao | Texto | varchar(150) | Sim | Informativo | Descricao obrigatoria. |
| SobescreveTributacaoNcm | Booleano | Nao informado no material | Sim | Regra | Define se sobrescreve regra NCM. |
| Finalidade | Enum | Nao informado no material | Sim | Regra | Dominio de finalidade. |
| Atendimento | Enum | Nao informado no material | Sim | Regra | Dominio de atendimento. |
| TipoFrete | Enum | Nao informado no material | Sim | Regra | Dominio de frete. |
| TipoMovimento | Enum | Nao informado no material | Sim | Regra | Dominio de movimento. |

### 9.10 `codigo_beneficio_fiscal`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| TenantId | Texto | varchar(200) | Nao informado no material | Tenant | Isolamento por tenant. |
| SequenciaTenantId | Numero | Nao informado no material | Nao informado no material | Exibicao | Sequencial do tenant. |
| Codigo | Texto | varchar(10) | Condicional | Indice | Codigo do beneficio. |
| Descricao | Texto | varchar(1000) | Condicional | Informativo | Descricao do beneficio. |
| Uf | Texto/enum | varchar(2) | Condicional | UF | UF do beneficio. |
| Csosns | Lista | Dominio CSOSN | Condicional | Relacionamento | Ao menos CSOSN ou CST. |
| Csts | Lista | Dominio CST | Condicional | Relacionamento | Ao menos CSOSN ou CST. |

### 9.11 `codigo_beneficio_fiscal_csosn`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| SequenciaTenantId | Numero | Nao informado no material | Nao informado no material | Exibicao | Sequencial do tenant. |
| CodigoBeneficioFiscalId | Numero | Nao informado no material | Sim | Beneficio fiscal | Beneficio pai. |
| Csosn | Enum | Dominio CSOSN | Sim | Regra | CSOSN relacionado. |

### 9.12 `codigo_beneficio_fiscal_cst`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| SequenciaTenantId | Numero | Nao informado no material | Nao informado no material | Exibicao | Sequencial do tenant. |
| CodigoBeneficioFiscalId | Numero | Nao informado no material | Sim | Beneficio fiscal | Beneficio pai. |
| Cst | Enum | Dominio CST | Sim | Regra | CST relacionado. |

### 9.13 `observacao_nfe`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| TenantId | Texto | varchar(200) | Nao informado no material | Tenant | Isolamento por tenant. |
| SequenciaTenantId | Numero | Nao informado no material | Nao informado no material | Exibicao | Sequencial do tenant. |
| Descricao | Texto | varchar(5000) | Condicional | Texto fiscal | Observacao fiscal. |

### 9.14 `cest`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| Codigo | Texto | varchar(7) | Condicional | Codigo | Codigo CEST. |
| Descricao | Texto | varchar(1000) | Condicional | Informativo | Descricao CEST. |

### 9.15 `codigo_anp`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| Codigo | Texto/numero | Nao informado no material | Condicional | Codigo | Codigo ANP. |
| Descricao | Texto | Nao informado no material | Condicional | Informativo | Descricao ANP. |
| DataInicioVigencia | Data | Nao informado no material | Nao informado no material | Vigencia | Inicio de vigencia. |
| DataFinalVigencia | Data | Nao informado no material | Nao | Vigencia | Fim de vigencia. |

### 9.16 `enquadramento_ipi`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| Codigo | Texto | varchar(7) | Condicional | Codigo | Codigo do enquadramento. |
| Descricao | Texto | varchar(500) | Condicional | Informativo | Descricao do enquadramento. |
| TipoOperacao | Enum | Imunidade=1, Suspensao=2, Isencao=3, Reducao=4, Outros=5 | Nao informado no material | Regra | Tipo de operacao do enquadramento. |

### 9.17 `fcp_aliquota_uf`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| Uf | Texto/enum | varchar(2) | Sim | UF | UF do FCP. |
| ValorAliquota | Decimal | decimal(16,4) | Sim | Aliquota | Aliquota FCP. |
| Observacao | Texto | varchar(200) | Nao | Informativo | Observacao FCP. |

### 9.18 `icms_aliquota_interestadual`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| UfOrigem | Texto/enum | varchar(2) | Sim | UF origem | UF origem ICMS. |
| UfDestino | Texto/enum | varchar(2) | Sim | UF destino | UF destino ICMS. |
| ValorAliquota | Decimal | decimal(16,4) | Sim | Aliquota | Aliquota ICMS interestadual. |

### 9.19 `cst_ibs_cbs`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| Cst | Texto | varchar(3) | Sim | Indice | CST IBS/CBS. |
| Descricao | Texto | varchar(2000) | Sim | Informativo | Descricao. |
| DataInicioVigencia | Data | Nao informado no material | Sim | Vigencia | Inicio da vigencia. |
| DataFimVigencia | Data | Nao informado no material | Nao | Vigencia | Fim da vigencia. |
| DataCadastro | Data/hora | Nao informado no material | Sim | Auditoria | Data de cadastro. |

### 9.20 `classificacao_ibs_cbs`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| CstIbsCbsId | Numero | Nao informado no material | Sim | CST IBS/CBS | CST pai. |
| Codigo | Texto | varchar(6) | Sim | Indice | Codigo da classificacao. |
| Descricao | Texto | Nao informado no material | Nao informado no material | Informativo | Descricao da classificacao. |
| DataInicioVigencia | Data | Nao informado no material | Nao informado no material | Vigencia | Inicio da vigencia quando informado. |
| DataFimVigencia | Data | Nao informado no material | Nao | Vigencia | Fim da vigencia quando informado. |
| IndNfe | Booleano | Nao informado no material | Sim | Regra | Aplicavel NF-e. |
| IndNfce | Booleano | Nao informado no material | Sim | Regra | Aplicavel NFC-e. |
| IndCte | Booleano | Nao informado no material | Sim | Regra | Aplicavel CT-e. |
| IndCteos | Booleano | Nao informado no material | Sim | Regra | Aplicavel CT-e OS. |
| IndNfse | Booleano | Nao informado no material | Sim | Regra | Aplicavel NFS-e. |

### 9.21 `anexo_classificacao_ibs_cbs`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno. |
| ClassificacaoIbsCbsId | Numero | Nao informado no material | Sim | Classificacao | Classificacao pai. |
| Codigo | Texto | varchar(10) | Sim | Indice | Codigo do anexo quando informado. |
| NroAnexo | Numero | Nao informado no material | Sim | Relacionamento | Numero do anexo. |
| DataInicioVigencia | Data | Nao informado no material | Nao informado no material | Vigencia | Inicio da vigencia quando informado. |
| DataFimVigencia | Data | Nao informado no material | Nao | Vigencia | Fim da vigencia quando informado. |

## 10. Consultas, cargas e retornos

| Operacao | Entrada | Saida | Regra |
|---|---|---|---|
| Consulta CFOP | Codigo, tipo entrada/saida, regime quando aplicavel | Lista de CFOPs | Deve considerar indicadores e filtro MEI quando aplicavel. |
| Ativacao CFOP padrao | Lista de CFOPs | CFOPs ativados | Deve permitir ativacao em lote. |
| Inativacao CFOP | Lista de CFOPs | CFOPs inativados | Deve permitir exclusao/inativacao em lote. |
| Carga CFOP padrao | Arquivo de tabela CFOP | Base padrao atualizada | Erro de leitura e tabela vazia devem ser tratados. |
| Consulta NCM | Codigo, descricao ou filtros nao detalhados | Lista NCM | Deve refletir carga NCM vigente quando aplicavel. |
| Carga NCM | Arquivo/tabela NCM | NCM atualizado | Registrar usuario, data e registros quando disponivel. |
| Consulta regra NCM | Grupo tributario | Regras do grupo | Deve rejeitar grupo nao localizado. |
| Consulta observacao NF-e | Localizar, pagina, tamanhoPagina | Dados, mensagem, sucesso, totalPaginas, totalRegistros | Retorno paginado comprovado. |
| Consulta codigo ANP | Codigo | Registro vigente | Deve filtrar vigencia ativa. |
| Atualizacao FCP | Arquivo de aliquotas | Aliquotas atualizadas | Deve invalidar cache. |

## 11. Integracoes funcionais

| Integracao | Dados consumidos | Efeito |
|---|---|---|
| Empresa | Grupo tributario da empresa, regime e tenant | Define escopo das regras fiscais aplicaveis. |
| Produto | NCM, CEST, ANP, regra tributaria e grupo | Alimenta emissao e calculo. |
| NF-e | CFOP, NCM, CST, CSOSN, PIS, COFINS, IPI, ICMS, beneficio e textos | Valida itens e compoe documento. |
| NFC-e | CFOP, NCM, CST, CSOSN, FCP, ICMS e indicadores NFC-e | Valida itens e compoe documento. |
| CT-e | Indicadores de classificacao quando informados | Usa classificacao aplicavel quando escopo CT-e exigir. |
| NFS-e | Indicadores de classificacao quando informados | Usa classificacao aplicavel quando escopo NFS-e exigir. |
| Motor de calculo tributario | Regras NCM, aliquotas, dominios e beneficios | Calcula e valida tributos. |
| IBPT e classificacoes | NCM, UF e classificacao IBS/CBS | Complementa tributos aproximados e reforma tributaria. |

## 12. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-CADFIS-001 | CFOP deve respeitar tamanho, indicadores e tenant. |
| CA-CADFIS-002 | CFOP padrao deve ser carregavel e ativavel sem duplicar CFOP funcional. |
| CA-CADFIS-003 | NCM deve aceitar codigo de 8 caracteres e descricao obrigatoria. |
| CA-CADFIS-004 | Regra NCM deve bloquear `CodRegra` duplicado no mesmo grupo. |
| CA-CADFIS-005 | Regra NCM deve rejeitar grupo tributario inexistente. |
| CA-CADFIS-006 | Regra NCM deve rejeitar CFOP invalido ou inexistente. |
| CA-CADFIS-007 | Tipo de operacao deve rejeitar descricao duplicada. |
| CA-CADFIS-008 | Tipo de operacao deve exigir grupo, descricao, sobrescrita, finalidade, atendimento, frete e movimento. |
| CA-CADFIS-009 | Beneficio fiscal deve exigir ao menos CSOSN ou CST. |
| CA-CADFIS-010 | Beneficio fiscal deve bloquear duplicidade por codigo e UF. |
| CA-CADFIS-011 | FCP UF e ICMS interestadual devem invalidar cache quando alterados. |
| CA-CADFIS-012 | Observacao NF-e deve retornar paginacao com totais quando consultada. |
| CA-CADFIS-013 | Codigo ANP por codigo deve retornar apenas registro vigente quando houver vigencia aplicavel. |
| CA-CADFIS-014 | Classificacao IBS/CBS deve respeitar CST pai, vigencia e indicadores por modelo. |

## 13. Pontos pendentes para validacao

| Ponto | Impacto |
|---|---|
| Obrigatoriedade final de alguns campos fiscais marcados como condicionais | Necessaria para formularios e validacoes finais. |
| Dominio completo de finalidade, atendimento, tipo de frete e tipo de movimento | Necessario para tipo de operacao fiscal. |
| Matriz completa CFOP x CST x CSOSN | Necessaria para reduzir rejeicoes fiscais. |
| Regra completa de vigencia para CFOP, NCM, ANP, FCP e ICMS | Necessaria para operacao fiscal historica. |
| Modelo final de permissao por cadastro | Necessario para seguranca operacional. |
| Politica de carga oficial e auditoria de atualizacao | Necessaria para governanca fiscal. |

## 14. Notas de autoria

Nao foram adicionados cadastros fiscais fora do material. Entidades de IBS/CBS foram mantidas aqui porque aparecem como cadastros fiscais estruturados; o detalhamento de calculo, aliquotas e aplicabilidade fiscal ampla permanece como ponto de aprofundamento do documento especifico de IBPT e classificacoes.
