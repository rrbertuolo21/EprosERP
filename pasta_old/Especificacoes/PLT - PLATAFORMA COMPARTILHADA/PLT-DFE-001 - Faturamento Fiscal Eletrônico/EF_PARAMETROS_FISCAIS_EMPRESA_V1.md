# Especificacao Funcional - Epros

**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Capacidade:** PARAMETROS_FISCAIS_EMPRESA  
**Versao:** V1  
**Empresa:** Siser  
**Status:** Concluido para validacao humana  

## 1. Controle do documento

| Item | Conteudo |
|---|---|
| Responsavel pela elaboracao | Analise funcional assistida |
| Responsavel pela validacao funcional | Siser |
| Responsavel pela validacao tecnica | Siser |
| Area dona do processo | Fiscal, Cadastros, Plataforma, Vendas, PDV |
| Publico-alvo | Produto, negocio, implantacao, desenvolvimento, suporte, operacao fiscal |
| Fonte de verdade | Esta EF descreve os parametros fiscais de empresa do Epros |

## 2. Objetivo funcional

Parametros Fiscais por Empresa existem para determinar se uma empresa esta apta a emitir documentos fiscais eletronicos no Epros, principalmente NF-e e NFC-e, controlando ambiente fiscal, numeracao, serie, CSC, certificado digital, comportamento de impressao e dados de tenant fiscal usados pela transmissao.

Esta especificacao tambem define os pontos de fronteira: cadastro mestre de certificado e empresa permanece no cadastro/parametrizacao corporativa, enquanto este subdominio fiscal consome os dados necessarios para emissao, cancelamento, inutilizacao, consulta, download e geracao de documentos fiscais.

## 3. Escopo funcional

### 3.1 Dentro do escopo

| Capacidade | Descricao | Status de conteudo |
|---|---|---|
| Parametros fiscais gerais da empresa | Define ambientes NF-e/NFC-e, objeto de NF-e, objeto de NFC-e homologacao/producao, destaque de ICMS ST e empresa relacionada. | Com conteudo |
| Parametros NFC-e homologacao | Controla CSC, ID CSC, serie, proximo numero e geracao de contingencia em homologacao. | Com conteudo |
| Parametros NFC-e producao | Controla CSC, ID CSC, serie e proximo numero de NFC-e em producao. | Com conteudo |
| Parametros NF-e producao | Exige serie e proximo numero de NF-e em producao, mas o material nao traz a estrutura completa da entidade NF-e. | Parcial |
| Certificado digital fiscal | Mantem caminho, senha, serial, datas de validade, tipo e ultima transmissao no tenant fiscal. | Com conteudo parcial |
| Transmissao de certificado para servico fiscal | Envia certificado para o contexto fiscal utilizado em emissao, consulta, cancelamento e inutilizacao. | Com conteudo |
| Configuracao de impressao NFC-e | Controla detalhes de venda normal/contingencia, margens, modo de impressao, QR Code e segunda via em contingencia. | Com conteudo |
| Tipo de ambiente | Define Producao=1 e Homologacao=2. | Com conteudo |
| Tag de codigo do produto | Define CodigoInterno=1 e CodigoProduto=2. | Com conteudo |
| Parametros de servico fiscal | Material cita timeout, protocolo, numero de serie, ultimo numero NF-e, impressora e versoes de servico. | Parcial |

### 3.2 Fora do escopo

| Item | Tratamento |
|---|---|
| Emissao NF-e e NFC-e | Detalhada nas EFs especificas de emissao. |
| Cancelamento, carta de correcao e inutilizacao | Consomem parametros e certificado, mas possuem EFs proprias. |
| Cadastro completo de empresa, endereco e municipio | Pertence a Cadastros Base. |
| Cadastro mestre do certificado digital | Pertence ao cadastro/parametrizacao corporativa; esta EF cobre uso e transmissao fiscal. |
| Motor de calculo tributario | Consome parametros e cadastros fiscais, mas possui EF propria. |

## 4. Glossario funcional

| Termo | Definicao | Observacao |
|---|---|---|
| Ambiente fiscal | Contexto de emissao fiscal. | Valores comprovados: Producao=1, Homologacao=2. |
| CSC | Codigo de seguranca do contribuinte para NFC-e. | Possui limite de tamanho informado. |
| ID CSC | Identificador do CSC da NFC-e. | Possui limite de tamanho informado. |
| Serie | Serie fiscal usada na numeracao do documento. | Em producao, NF-e e NFC-e nao podem iniciar como zero quando exigidas. |
| Proximo numero | Proximo numero fiscal a ser usado na emissao. | Em producao, NF-e e NFC-e nao podem iniciar como zero quando exigido. |
| Certificado digital | Arquivo e dados de validade usados para transmissao fiscal. | Sem certificado valido a transmissao deve ser bloqueada. |
| Tenant fiscal | Registro operacional usado pelo servico fiscal. | Inclui TenantId, nome, certificado, senha, serial e validade. |
| Impressao NFC-e | Parametros de DANFCE e impressao fiscal da NFC-e. | Configuracao unica por empresa. |

## 5. Atores, papeis e responsabilidades

| Ator/Papel | Responsabilidade | Permissoes esperadas | Restricoes |
|---|---|---|---|
| Gestor fiscal | Configurar ambientes, series, numeros, CSC, layout NFC-e e validar prontidao fiscal. | Criar, alterar, consultar e auditar parametros fiscais. | Nao deve acessar senha/certificado em claro. |
| Operador fiscal | Consultar situacao fiscal da empresa e identificar bloqueios de emissao. | Consultar parametros e pendencias. | Nao altera parametros criticos sem permissao. |
| Administrador Siser | Apoiar configuracao fiscal, tenant fiscal, armazenamento e suporte operacional. | Administracao fiscal auditada. | Acesso a dados sensiveis deve ser justificado. |
| Integracao fiscal | Consumir parametros para emissao, consulta, cancelamento e inutilizacao. | Leitura controlada por contrato autenticado. | Nao altera parametros sem fluxo autorizado. |
| Suporte | Diagnosticar erro de certificado, arquivo, ambiente ou numeracao. | Consulta e evidencias. | Nao altera dados fiscais sem autorizacao. |

## 6. Visao operacional

1. Usuario autorizado acessa a configuracao fiscal da empresa.
2. O Epros identifica tenant e empresa.
3. O usuario informa ou revisa ambientes NF-e e NFC-e.
4. Para NFC-e, o usuario informa parametros de homologacao e/ou producao conforme o ambiente.
5. Para NF-e em producao, o Epros exige campos de producao informados no material: serie e proximo numero.
6. O certificado digital deve existir e estar disponivel para transmissao fiscal quando a operacao exigir transmissao.
7. A configuracao de impressao NFC-e deve existir quando a empresa utilizar emissao NFC-e com DANFCE.
8. O Epros valida obrigatoriedade, tamanhos, numeracao e relacao com empresa.
9. Quando valido, os parametros ficam disponiveis para emissao, consulta, cancelamento, inutilizacao e downloads fiscais.
10. Quando invalido, o Epros bloqueia a operacao fiscal e apresenta erro funcional claro.

## 7. Capacidades funcionais detalhadas

### 7.1 Manter parametros fiscais gerais da empresa

| Item | Especificacao |
|---|---|
| Objetivo | Manter a configuracao fiscal base por empresa. |
| Acionamento | Manual por usuario autorizado ou integracao administrativa. |
| Pre-condicoes | Empresa existente, tenant identificado e usuario autorizado. |
| Dados de entrada | Descricao, DestacarIcmsSt, Nfe, NfceHomologacao, NfceProducao, TipoAmbienteNfce, TipoAmbienteNfe e Empresa. |
| Processamento | Validar descricao, ambientes obrigatorios, objetos obrigatorios conforme ambiente e integridade com empresa. |
| Resultado esperado | Empresa fiscalmente parametrizada ou bloqueada com erro funcional. |
| Pos-condicoes | Parametros disponiveis para emissao e eventos fiscais. |
| Excecoes | Empresa nao encontrada, ambiente ausente, producao sem campos obrigatorios, certificado ausente quando exigido. |
| Auditoria | Usuario, data/hora, empresa, campos alterados e motivo quando houver. |

### 7.2 Manter parametros NFC-e de homologacao

| Item | Especificacao |
|---|---|
| Objetivo | Permitir configuracao de NFC-e no ambiente de homologacao. |
| Acionamento | Manual por usuario autorizado. |
| Pre-condicoes | Empresa e parametros fiscais gerais existentes. |
| Dados de entrada | NfceCscHomologacao, NfceIdCscHomologacao, NfceSerieHomologacao, NfceProximoNrHomologacao, NfceGerarContingenciaEmHomologacao. |
| Processamento | Validar tamanho maximo do CSC e ID CSC; manter serie, proximo numero e indicador de contingencia. |
| Resultado esperado | Parametros de homologacao disponiveis para testes fiscais NFC-e. |
| Excecoes | CSC homologacao acima de 36 caracteres; ID CSC homologacao acima de 6 caracteres. |
| Auditoria | Usuario, data/hora, empresa e valores alterados. |

### 7.3 Manter parametros NFC-e de producao

| Item | Especificacao |
|---|---|
| Objetivo | Permitir emissao de NFC-e em producao. |
| Acionamento | Manual por usuario autorizado. |
| Pre-condicoes | TipoAmbienteNfce definido como producao, empresa existente e certificado disponivel quando a transmissao exigir. |
| Dados de entrada | NfceCscProducao, NfceIdCscProducao, NfceSerieProducao, NfceProximoNrProducao. |
| Processamento | Validar obrigatoriedade, tamanho e impedir serie/proximo numero zero quando producao. |
| Resultado esperado | NFC-e apta para emissao em producao. |
| Excecoes | CSC producao ausente, ID CSC producao ausente, serie zero, proximo numero zero, tamanho excedido. |
| Auditoria | Usuario, data/hora, empresa e campos alterados. |

### 7.4 Manter parametros NF-e de producao

| Item | Especificacao |
|---|---|
| Objetivo | Permitir emissao de NF-e em producao. |
| Acionamento | Manual por usuario autorizado. |
| Pre-condicoes | TipoAmbienteNfe definido como producao e empresa existente. |
| Dados de entrada | NfeSerieProducao e NfeProximoNrProducao aparecem no material como obrigatorios em producao. |
| Processamento | Validar existencia dos campos de producao e impedir serie/proximo numero zero. |
| Resultado esperado | NF-e apta para emissao em producao quanto a numeracao minima comprovada. |
| Excecoes | Campos de producao ausentes, serie zero ou proximo numero zero. |
| Auditoria | Usuario, data/hora, empresa e campos alterados. |

### 7.5 Manter tenant fiscal e certificado

| Item | Especificacao |
|---|---|
| Objetivo | Manter dados fiscais do tenant usados pela transmissao. |
| Acionamento | Cadastro/alteracao de certificado e transmissao para contexto fiscal. |
| Pre-condicoes | Empresa existente e certificado digital cadastrado quando necessario. |
| Dados de entrada | TenantId, Nome, CaminhoCertDigital, SenhaCertDigital, Serial, DataValidadeInicial, DataValidadeFinal, Tipo e DataUltimaTransmissao. |
| Processamento | Registrar caminho/senha/serial/validade e transmitir certificado para o contexto fiscal quando aplicavel. |
| Resultado esperado | Certificado disponivel para emissao, consulta, cancelamento e inutilizacao. |
| Excecoes | Certificado nao encontrado, certificado invalido, falha de transmissao do certificado, arquivo nao encontrado. |
| Auditoria | Usuario/processo, empresa, tenant, data/hora, validade, status da transmissao e erro quando houver. |

### 7.6 Configurar impressao NFC-e

| Item | Especificacao |
|---|---|
| Objetivo | Controlar layout de impressao da NFC-e por empresa. |
| Acionamento | Manual por usuario autorizado. |
| Pre-condicoes | Empresa existente e uso de NFC-e definido. |
| Dados de entrada | EmpresaId, TenantId, DetalheVendaNormal, DetalheVendaContingencia, ImprimeDescontoItem, ImprimeFoneEmitente, MargemEsquerda, MargemDireita, ModoImpressao, NfceLayoutQrCode, VersaoQrCode, SegundaViaContingencia. |
| Processamento | Validar empresa, unicidade por empresa e campos obrigatorios de identificacao. |
| Resultado esperado | Parametros de DANFCE e impressao disponiveis para emissao NFC-e. |
| Excecoes | EmpresaId ausente, Id ausente quando requerido em alteracao, configuracao duplicada para empresa. |
| Auditoria | Usuario, data/hora, empresa e campos de layout alterados. |

## 8. Regras de negocio

| Regra | Descricao | Condicao | Resultado | Severidade | Observacoes |
|---|---|---|---|---|---|
| REG-PARAM-001 | A descricao dos parametros fiscais e obrigatoria e deve ter no maximo 100 caracteres. | Cadastro/alteracao de parametros. | Bloquear salvamento. | Bloqueante | Material informa mensagem de descricao obrigatoria e limite de 100. |
| REG-PARAM-002 | O ambiente de NFC-e e obrigatorio. | Cadastro/alteracao de parametros. | Bloquear salvamento. | Bloqueante | Valores comprovados: Producao=1, Homologacao=2. |
| REG-PARAM-003 | O ambiente de NF-e e obrigatorio. | Cadastro/alteracao de parametros. | Bloquear salvamento. | Bloqueante | Valores comprovados: Producao=1, Homologacao=2. |
| REG-PARAM-004 | Quando o ambiente NFC-e for producao, os campos de producao NFC-e sao obrigatorios. | TipoAmbienteNfce=Producao. | Bloquear salvamento. | Bloqueante | Inclui CSC, ID CSC, serie e proximo numero. |
| REG-PARAM-005 | Quando o ambiente NF-e for producao, os campos de producao NF-e sao obrigatorios. | TipoAmbienteNfe=Producao. | Bloquear salvamento. | Bloqueante | Material cita NfeSerieProducao e NfeProximoNrProducao. |
| REG-PARAM-006 | CSC de producao da NFC-e e obrigatorio quando NFC-e operar em producao. | NFC-e em producao. | Bloquear salvamento. | Bloqueante |  |
| REG-PARAM-007 | ID CSC de producao da NFC-e e obrigatorio quando NFC-e operar em producao. | NFC-e em producao. | Bloquear salvamento. | Bloqueante |  |
| REG-PARAM-008 | Serie de producao NFC-e nao pode iniciar como zero. | NFC-e em producao. | Bloquear salvamento. | Bloqueante |  |
| REG-PARAM-009 | Proximo numero de producao NFC-e nao pode iniciar como zero. | NFC-e em producao. | Bloquear salvamento. | Bloqueante |  |
| REG-PARAM-010 | Serie de producao NF-e nao pode iniciar como zero. | NF-e em producao. | Bloquear salvamento. | Bloqueante |  |
| REG-PARAM-011 | Proximo numero de producao NF-e nao pode iniciar como zero. | NF-e em producao. | Bloquear salvamento. | Bloqueante |  |
| REG-PARAM-012 | CSC de homologacao NFC-e deve ter no maximo 36 caracteres. | NFC-e homologacao. | Bloquear salvamento se exceder. | Bloqueante |  |
| REG-PARAM-013 | ID CSC de homologacao NFC-e deve ter no maximo 6 caracteres. | NFC-e homologacao. | Bloquear salvamento se exceder. | Bloqueante |  |
| REG-PARAM-014 | CSC de producao NFC-e deve ter no maximo 36 caracteres. | NFC-e producao. | Bloquear salvamento se exceder. | Bloqueante |  |
| REG-PARAM-015 | ID CSC de producao NFC-e deve ter no maximo 6 caracteres. | NFC-e producao. | Bloquear salvamento se exceder. | Bloqueante |  |
| REG-PARAM-016 | Certificado nao encontrado deve bloquear transmissao fiscal. | Emissao, consulta, cancelamento ou inutilizacao que exija certificado. | Retornar erro funcional e impedir transmissao. | Bloqueante |  |
| REG-PARAM-017 | Certificado invalido deve bloquear transmissao fiscal antes do envio. | Pre-emissao ou transmissao. | Registrar erro de validade. | Bloqueante |  |
| REG-PARAM-018 | Certificado deve ser transmitido para o contexto fiscal quando cadastrado/alterado. | Cadastro/alteracao de certificado. | Disponibilizar certificado para operacoes fiscais. | Bloqueante | Material cita transmissao multipart para empresa-certificado. |
| REG-PARAM-019 | Cancelamento, inutilizacao e consulta leem certificado em caminho interno por documento fiscal. | Operacoes fiscais posteriores. | Usar certificado armazenado para o documento. | Bloqueante | Caminho comprovado: Certificados/{documento}. |
| REG-PARAM-020 | Configuracao de impressao NFC-e deve ser unica por empresa. | Cadastro/alteracao de layout. | Bloquear duplicidade. | Bloqueante | Material informa indice por EmpresaId. |
| REG-PARAM-021 | Configuracao de impressao NFC-e exige EmpresaId. | Cadastro/alteracao de layout. | Bloquear salvamento. | Bloqueante | Material informa validacao obrigatoria. |
| REG-PARAM-022 | Configuracao de impressao NFC-e exige Id quando aplicavel. | Alteracao/DTO com Id requerido. | Bloquear salvamento. | Bloqueante | Material informa validacao obrigatoria de Id. |
| REG-PARAM-023 | Codigo de produto no documento deve respeitar a tag configurada. | Geracao fiscal que use codigo do item. | Usar CodigoInterno ou CodigoProduto. | Media | Valores comprovados: CodigoInterno=1, CodigoProduto=2. |
| REG-PARAM-024 | Nome de impressora, quando usado por parametro de servico fiscal, deve ter no maximo 250 caracteres. | Parametro de impressora fiscal. | Bloquear salvamento se exceder. | Media | Material traz limite de 250. |
| REG-PARAM-025 | Municipio do emitente deve possuir codigo IBGE diferente de zero para emissao. | Configuracao fiscal do emitente. | Bloquear emissao/configuracao incompleta. | Bloqueante | Material cita obrigatoriedade de codigo IBGE do municipio do emitente. |
| REG-PARAM-026 | Regime tributario/CRT do emitente deve estar definido para emissao. | Configuracao fiscal do emitente. | Bloquear transmissao quando ausente ou invalido. | Bloqueante | Material cita CRT e regime tributario do emitente. |
| REG-PARAM-027 | Erro ao transmitir certificado para o contexto fiscal deve ser registrado. | Transmissao de certificado. | Retornar erro funcional e manter auditoria. | Bloqueante | Material cita erro de transmissao de certificado. |

## 9. Parametros de configuracao

| Parametro | Finalidade | Tipo/formato | Valor padrao | Obrigatorio | Nivel | Quem pode alterar | Impacto |
|---|---|---|---|---|---|---|---|
| Descricao | Nome/descricao dos parametros fiscais. | Texto | Nao informado no material | Sim | Empresa | Gestor fiscal | Identifica configuracao fiscal. |
| TipoAmbienteNfce | Define ambiente de NFC-e. | Enum | Nao informado no material | Sim | Empresa | Gestor fiscal | Direciona emissao NFC-e. |
| TipoAmbienteNfe | Define ambiente de NF-e. | Enum | Nao informado no material | Sim | Empresa | Gestor fiscal | Direciona emissao NF-e. |
| DestacarIcmsSt | Define destaque de ICMS ST. | Booleano | Nao informado no material | Sim | Empresa | Gestor fiscal | Afeta documento/calculo quando consumido. |
| Tag de produto | Define codigo do produto usado no fiscal. | Enum | Nao informado no material | Nao informado no material | Empresa | Gestor fiscal | CodigoInterno ou CodigoProduto. |
| NfceCscHomologacao | CSC da NFC-e em homologacao. | Texto | Nao informado no material | Nao informado no material | Empresa | Gestor fiscal | Usado em testes fiscais. |
| NfceIdCscHomologacao | ID CSC da NFC-e em homologacao. | Texto | Nao informado no material | Nao informado no material | Empresa | Gestor fiscal | Usado em testes fiscais. |
| NfceSerieHomologacao | Serie NFC-e homologacao. | Numero | Nao informado no material | Sim | Empresa | Gestor fiscal | Controla numeracao em homologacao. |
| NfceProximoNrHomologacao | Proximo numero NFC-e homologacao. | Numero | Nao informado no material | Sim | Empresa | Gestor fiscal | Controla numeracao em homologacao. |
| NfceGerarContingenciaEmHomologacao | Indica contingencia em homologacao. | Booleano | Nao informado no material | Sim | Empresa | Gestor fiscal | Afeta teste/contingencia NFC-e. |
| NfceCscProducao | CSC NFC-e producao. | Texto | Nao informado no material | Condicional | Empresa | Gestor fiscal | Obrigatorio em producao. |
| NfceIdCscProducao | ID CSC NFC-e producao. | Texto | Nao informado no material | Condicional | Empresa | Gestor fiscal | Obrigatorio em producao. |
| NfceSerieProducao | Serie NFC-e producao. | Numero | Nao informado no material | Sim/condicional | Empresa | Gestor fiscal | Nao pode iniciar zero em producao. |
| NfceProximoNrProducao | Proximo numero NFC-e producao. | Numero | Nao informado no material | Sim/condicional | Empresa | Gestor fiscal | Nao pode iniciar zero em producao. |
| NfeSerieProducao | Serie NF-e producao. | Numero | Nao informado no material | Condicional | Empresa | Gestor fiscal | Nao pode iniciar zero em producao. |
| NfeProximoNrProducao | Proximo numero NF-e producao. | Numero | Nao informado no material | Condicional | Empresa | Gestor fiscal | Nao pode iniciar zero em producao. |
| CaminhoCertDigital | Caminho do certificado no tenant fiscal. | Texto | Nao informado no material | Nao informado no material | Tenant fiscal | Administrador Siser/Gestor fiscal | Habilita transmissao. |
| SenhaCertDigital | Senha do certificado. | Texto secreto | Nao informado no material | Nao informado no material | Tenant fiscal | Administrador Siser/Gestor fiscal | Dado sensivel. |
| Serial | Serial do certificado. | Texto | Nao informado no material | Nao informado no material | Tenant fiscal | Administrador Siser/Gestor fiscal | Identificacao do certificado. |
| DataValidadeInicial | Inicio da validade do certificado. | Data | Nao informado no material | Nao informado no material | Tenant fiscal | Administrador Siser/Gestor fiscal | Validacao do certificado. |
| DataValidadeFinal | Fim da validade do certificado. | Data | Nao informado no material | Nao informado no material | Tenant fiscal | Administrador Siser/Gestor fiscal | Alerta/bloqueio de vencimento. |
| Tipo | Tipo do certificado/tenant fiscal. | Nao informado no material | Nao informado no material | Nao informado no material | Tenant fiscal | Administrador Siser/Gestor fiscal | Nao informado no material. |
| DataUltimaTransmissao | Ultima transmissao de certificado. | Data/hora | Nao informado no material | Nao informado no material | Tenant fiscal | Sistema | Auditoria operacional. |
| MargemEsquerda | Margem esquerda do DANFCE. | Numero real | Nao informado no material | Nao informado no material | Empresa | Gestor fiscal | Impressao NFC-e. |
| MargemDireita | Margem direita do DANFCE. | Numero real | Nao informado no material | Nao informado no material | Empresa | Gestor fiscal | Impressao NFC-e. |
| NomeImpressora | Nome da impressora fiscal. | Texto | Nao informado no material | Nao informado no material | Empresa | Gestor fiscal | Ate 250 caracteres quando usado. |
| TimeOut | Tempo limite de comunicacao fiscal. | Numero inteiro | Nao informado no material | Nao informado no material | Global/empresa | Administrador Siser | Parametro de servico fiscal. |
| Versoes de servico fiscal | Versoes de recepcao/autorizacao/distribuicao. | Texto/configuracao | Nao informado no material | Nao informado no material | Global/empresa | Administrador Siser | Material cita 15 campos de versao. |

## 10. Modelo de dados funcional e implantavel

### 10.1 Entidades/tabelas

| Entidade/tabela | Papel funcional | Conteudo |
|---|---|---|
| empresa_parametros_dfe | Parametros fiscais principais da empresa. | Com conteudo parcial: campos gerais, ambientes, NF-e, NFC-e homologacao/producao e empresa. |
| empresa_parametros_dfe_nfce_homologacao | Parametros NFC-e homologacao. | Com conteudo completo no material. |
| empresa_parametros_dfe_nfce_producao | Parametros NFC-e producao. | Com conteudo completo no material. |
| empresa_parametros_dfe_nfe | Parametros NF-e. | Nao informado no material como tabela/campos completos; regras citam campos de producao. |
| cliente_tenant | Tenant fiscal/certificado. | Com conteudo de mapeamento de campos. |
| configuracao_impressao_nfce | Layout de impressao NFC-e por empresa. | Com conteudo de mapeamento de campos e indice por EmpresaId. |
| parametros_servico_fiscal | Parametros de timeout, protocolo, versoes, impressora e numeracao. | Material cita campos, mas nao traz modelo final completo. |

### 10.2 Relacionamentos

| Origem | Relacionamento | Destino | Cardinalidade | Obrigatorio | Regra de integridade |
|---|---|---|---|---|---|
| empresa_parametros_dfe | pertence a | empresa | 1:1 | Sim | Parametros fiscais dependem de empresa existente. |
| empresa_parametros_dfe | possui | empresa_parametros_dfe_nfce_homologacao | 1:1 | Sim | Estrutura informada como obrigatoria. |
| empresa_parametros_dfe | possui | empresa_parametros_dfe_nfce_producao | 1:1 | Sim | Estrutura informada como obrigatoria, com campos obrigatorios em producao. |
| empresa_parametros_dfe | possui | empresa_parametros_dfe_nfe | 1:1 | Sim | Material cita objeto NF-e obrigatorio, mas campos completos nao informados. |
| cliente_tenant | representa | tenant fiscal | 1:1 | Sim | TenantId obrigatorio e Nome obrigatorio. |
| cliente_tenant | armazena | certificado digital | 1:1 | Condicional | Certificado e requerido para transmissao. |
| configuracao_impressao_nfce | pertence a | empresa | 1:1 | Sim | Indice por EmpresaId; configuracao unica por empresa. |

### 10.3 Persistencia e retencao

| Item | Especificacao |
|---|---|
| Certificado digital | Caminho, senha, serial e validade devem ter protecao e auditoria; mecanismo de criptografia nao informado no material. |
| Numeracao fiscal | Deve ser preservada por empresa/ambiente/modelo/serie; concorrencia final nao informada no material. |
| Historico de parametros | Material recomenda parametros versionados e auditados na EF consolidada; estrutura de historico nao informada no material. |
| Configuracao de impressao | Relacionada a empresa e unica por EmpresaId. |
| Storage certificado | Material informa uso de caminho interno `Certificados/{documento}` para cancelamento, inutilizacao e consulta. |

### 10.4 Dicionario de dados implantavel - empresa_parametros_dfe

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| Id | Identificador | Nao informado no material | Nao informado no material | PK | Nao informado no material. |
| Descricao | Texto | max 100 | Sim | Atributo | Descricao obrigatoria. |
| DestacarIcmsSt | Booleano | true/false | Sim | Atributo | Define destaque de ICMS ST. |
| Nfe | Objeto/relacao | Nao informado no material | Sim | Relacao | Campos completos nao informados no material. |
| NfceHomologacao | Objeto/relacao | Nao informado no material | Sim | Relacao | Relacao com parametros NFC-e homologacao. |
| NfceProducao | Objeto/relacao | Nao informado no material | Sim | Relacao | Relacao com parametros NFC-e producao. |
| TipoAmbienteNfce | Enum | Producao=1; Homologacao=2 | Sim | Atributo | Ambiente NFC-e obrigatorio. |
| TipoAmbienteNfe | Enum | Producao=1; Homologacao=2 | Sim | Atributo | Ambiente NF-e obrigatorio. |
| EmpresaId | Identificador | Nao informado no material | Sim | FK empresa | Empresa vinculada aos parametros. |
| TagCprod | Enum | CodigoInterno=1; CodigoProduto=2 | Nao informado no material | Atributo | Define codigo de produto usado no fiscal. |

### 10.5 Dicionario de dados implantavel - empresa_parametros_dfe_nfce_homologacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| Id | Identificador | Nao informado no material | Nao informado no material | PK | Nao informado no material. |
| NfceCscHomologacao | Texto | max 36 | Nao | Atributo | CSC homologacao. |
| NfceIdCscHomologacao | Texto | max 6 | Nao | Atributo | ID CSC homologacao. |
| NfceSerieHomologacao | Numero inteiro | int/long | Sim | Atributo | Serie NFC-e homologacao. |
| NfceProximoNrHomologacao | Numero inteiro | long | Sim | Atributo | Proximo numero NFC-e homologacao. |
| NfceGerarContingenciaEmHomologacao | Booleano | true/false | Sim | Atributo | Indica geracao de contingencia em homologacao. |

### 10.6 Dicionario de dados implantavel - empresa_parametros_dfe_nfce_producao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| Id | Identificador | Nao informado no material | Nao informado no material | PK | Nao informado no material. |
| NfceCscProducao | Texto | max 36 | Condicional | Atributo | Obrigatorio quando NFC-e operar em producao. |
| NfceIdCscProducao | Texto | max 6 | Condicional | Atributo | Obrigatorio quando NFC-e operar em producao. |
| NfceSerieProducao | Numero inteiro | int/long | Sim/condicional | Atributo | Nao pode iniciar zero em producao. |
| NfceProximoNrProducao | Numero inteiro | long | Sim/condicional | Atributo | Nao pode iniciar zero em producao. |

### 10.7 Dicionario de dados implantavel - empresa_parametros_dfe_nfe

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| Id | Identificador | Nao informado no material | Nao informado no material | PK | Nao informado no material. |
| NfeSerieProducao | Numero inteiro | Nao informado no material | Condicional | Atributo | Material cita obrigatoriedade e bloqueio de zero em producao. |
| NfeProximoNrProducao | Numero inteiro | Nao informado no material | Condicional | Atributo | Material cita obrigatoriedade e bloqueio de zero em producao. |
| Demais campos NF-e producao | Nao informado no material | Nao informado no material | Nao informado no material | Nao informado no material | Estrutura completa nao informada no material. |

### 10.8 Dicionario de dados implantavel - cliente_tenant

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| Id | Identificador | Nao informado no material | Nao informado no material | PK | Nao informado no material. |
| TenantId | Texto | varchar(200) | Sim | Identificador tenant | Tenant fiscal. |
| Nome | Texto | varchar(150) | Sim | Atributo | Nome do tenant/cliente fiscal. |
| CaminhoCertDigital | Texto | varchar(500) | Nao | Atributo sensivel | Caminho do certificado. |
| SenhaCertDigital | Texto secreto | varchar(100) | Nao | Atributo sensivel | Deve ser protegido; mecanismo nao informado no material. |
| Serial | Texto | varchar(50) | Nao | Atributo | Serial do certificado. |
| DataValidadeInicial | Data | Nao informado no material | Nao | Atributo | Inicio de validade. |
| DataValidadeFinal | Data | Nao informado no material | Nao | Atributo | Fim de validade. |
| Tipo | Nao informado no material | Nao informado no material | Nao | Atributo | Nao informado no material. |
| DataUltimaTransmissao | Data/hora | Nao informado no material | Nao | Atributo | Ultima transmissao do certificado. |

### 10.9 Dicionario de dados implantavel - configuracao_impressao_nfce

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| Id | Identificador | Nao informado no material | Sim quando aplicavel | PK | Material informa validacao de Id obrigatorio no contrato de entrada. |
| EmpresaId | Identificador | Nao informado no material | Sim | FK empresa | Indice por EmpresaId; uma configuracao por empresa. |
| TenantId | Texto | varchar(200) | Nao informado no material | Tenant | Tenant da configuracao. |
| DetalheVendaNormal | Nao informado no material | Nao informado no material | Nao informado no material | Atributo | Detalhe de venda normal. |
| DetalheVendaContingencia | Nao informado no material | Nao informado no material | Nao informado no material | Atributo | Detalhe de venda em contingencia. |
| ImprimeDescontoItem | Booleano | true/false | Nao informado no material | Atributo | Imprime desconto no item. |
| ImprimeFoneEmitente | Booleano | true/false | Nao informado no material | Atributo | Imprime telefone do emitente. |
| MargemEsquerda | Numero real | real | Nao informado no material | Atributo | Margem esquerda. |
| MargemDireita | Numero real | real | Nao informado no material | Atributo | Margem direita. |
| ModoImpressao | Nao informado no material | Nao informado no material | Nao informado no material | Atributo | Modo de impressao. |
| NfceLayoutQrCode | Nao informado no material | Nao informado no material | Nao informado no material | Atributo | Layout QR Code NFC-e. |
| VersaoQrCode | Nao informado no material | Nao informado no material | Nao informado no material | Atributo | Versao do QR Code. |
| SegundaViaContingencia | Booleano | true/false | Nao informado no material | Atributo | Segunda via em contingencia. |

### 10.10 Dicionario de dados implantavel - parametros_servico_fiscal

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| ModeloDocumento | Enum/codigo | NF-e, NFC-e, CF-e quando aplicavel | Nao informado no material | Atributo | Material cita modelos fiscais. |
| TipoEmissao | Enum | Nao informado no material | Nao informado no material | Atributo | Material cita tipo de emissao. |
| TipoAmbiente | Enum | Producao=1; Homologacao=2 | Nao informado no material | Atributo | Ambiente fiscal. |
| TimeOut | Numero inteiro | int | Nao informado no material | Atributo | Tempo limite de comunicacao. |
| Protocolo | Nao informado no material | Nao informado no material | Nao informado no material | Atributo | Material cita protocolo. |
| NumeroSerie | Numero | Nao informado no material | Nao informado no material | Atributo | Numero de serie. |
| UltimoNumeroNFe | Numero | Nao informado no material | Nao informado no material | Atributo | Contador NF-e. |
| NomeImpressora | Texto | max 250 | Nao informado no material | Atributo | Nome da impressora. |
| VersaoServicoRecepcao | Texto | Nao informado no material | Nao informado no material | Atributo | Um dos campos de versao citados. |
| VersoesServicoDemais | Texto/configuracao | 15 campos citados no material | Nao informado no material | Atributo | Lista completa de nomes nao consolidada nesta EF por falta de detalhe funcional final. |

## 11. Estados

| Estado | Significado | Entrada | Saida |
|---|---|---|---|
| Rascunho | Parametros ainda incompletos. | Inicio de cadastro. | Validacao ou exclusao. |
| Valido para homologacao | Parametros minimos de homologacao preenchidos. | Ambientes e dados homologacao validos. | Uso em testes fiscais. |
| Valido para producao | Parametros de producao obrigatorios preenchidos. | Producao com serie/numero/CSC quando aplicavel. | Emissao em producao. |
| Bloqueado por parametro | Falta ambiente, serie, numero, CSC ou empresa. | Validacao falhou. | Correcao de dados. |
| Bloqueado por certificado | Certificado ausente, invalido ou nao transmitido. | Validacao ou transmissao falhou. | Cadastro/transmissao de certificado. |
| Valido para impressao NFC-e | Configuracao de impressao da empresa existe e e unica. | Configuracao salva. | Emissao/impressao NFC-e. |

## 12. Fluxos funcionais

### 12.1 Configurar empresa fiscal

| Passo | Ator | Acao | Validacao | Resultado |
|---:|---|---|---|---|
| 1 | Gestor fiscal | Seleciona empresa. | Empresa existente. | Contexto fiscal carregado. |
| 2 | Gestor fiscal | Informa descricao e ambientes NF-e/NFC-e. | Descricao obrigatoria ate 100; ambientes obrigatorios. | Parametros gerais preenchidos. |
| 3 | Gestor fiscal | Informa NFC-e homologacao/producao. | Tamanho CSC/ID CSC; producao com campos obrigatorios. | Parametros NFC-e validados. |
| 4 | Gestor fiscal | Informa NF-e producao quando aplicavel. | Serie e proximo numero nao podem iniciar zero. | Parametros NF-e validados parcialmente conforme material. |
| 5 | Epros | Persiste configuracao. | Relacao com empresa e tenant. | Empresa fiscalmente parametrizada. |

### 12.2 Transmitir certificado para uso fiscal

| Passo | Ator | Acao | Validacao | Resultado |
|---:|---|---|---|---|
| 1 | Gestor fiscal | Cadastra ou atualiza certificado. | Certificado existente e dados minimos. | Certificado registrado. |
| 2 | Epros | Envia certificado para contexto fiscal. | Integracao disponivel. | Certificado transmitido. |
| 3 | Epros | Atualiza tenant fiscal. | TenantId e Nome obrigatorios. | Caminho, senha, serial e validade armazenados. |
| 4 | Epros | Usa certificado em operacoes fiscais. | Certificado valido. | Emissao/eventos fiscais habilitados. |

### 12.3 Configurar impressao NFC-e

| Passo | Ator | Acao | Validacao | Resultado |
|---:|---|---|---|---|
| 1 | Gestor fiscal | Acessa configuracao de impressao NFC-e. | Empresa existente. | Tela/configuracao carregada. |
| 2 | Gestor fiscal | Preenche parametros de layout. | EmpresaId obrigatorio; unicidade por empresa. | Layout salvo. |
| 3 | Epros | Disponibiliza layout para emissao. | Configuracao valida. | DANFCE usa parametros cadastrados. |

## 13. Telas e operacoes esperadas

| Tela/operacao | Campos principais | Acoes | Observacao |
|---|---|---|---|
| Parametros fiscais da empresa | Descricao, ambiente NF-e, ambiente NFC-e, destaque ICMS ST, tag produto, serie/numero NF-e/NFC-e, CSC/ID CSC | Consultar, criar, alterar, validar | Material cita configuracao em area de parametros. |
| Certificado digital | Arquivo, senha, validade, serial, status de transmissao | Upload, consultar validade, transmitir | Cadastro mestre fica fora desta EF; uso fiscal fica aqui. |
| Configuracao de impressao NFC-e | Empresa, detalhes de venda, margens, modo, QR Code, segunda via contingencia | Criar, alterar, consultar | Deve ser unica por empresa. |
| Diagnostico fiscal da empresa | Ambiente, certificado, validade, numeracao, impressao | Consultar pendencias | Nota criada nesta EF para consolidar validacoes existentes.[^nota1] |

## 14. Integracoes

| Integracao | Direcao | Dados | Regra |
|---|---|---|---|
| Empresa | Entrada | EmpresaId, documento, endereco, municipio, regime/CRT | Empresa deve existir; municipio do emitente deve ter codigo IBGE diferente de zero para emissao. |
| Certificado digital | Entrada/Saida | Arquivo, senha, serial, validade, caminho | Certificado deve existir e ser valido para transmissao. |
| Servico fiscal | Saida | Certificado e parametros de transmissao | Falha deve gerar erro funcional auditavel. |
| Emissao NF-e/NFC-e | Saida | Ambiente, serie, numero, CSC, certificado | Emissao so deve seguir com parametros validos. |
| Cancelamento/inutilizacao/consulta | Saida | Certificado, ambiente, documento | Operacoes leem certificado do caminho fiscal informado. |
| Impressao NFC-e | Saida | Layout, margens, QR Code, segunda via | DANFCE deve usar configuracao da empresa. |

## 15. Relatorios e consultas

| Consulta | Filtros | Colunas | Observacao |
|---|---|---|---|
| Empresas aptas para emissao | Empresa, ambiente, modelo fiscal, status certificado | Empresa, ambiente NF-e, ambiente NFC-e, certificado, validade, pendencias | Nota criada nesta EF a partir das validacoes funcionais existentes.[^nota1] |
| Parametros de impressao NFC-e | Empresa | Empresa, margens, modo, QR Code, segunda via | Baseado na entidade de configuracao de impressao. |
| Certificados fiscais | Empresa, validade | Empresa, serial, validade inicial/final, ultima transmissao | Baseado em cliente_tenant. |

## 16. Mensagens e erros funcionais

| Codigo | Mensagem funcional | Quando ocorre |
|---|---|---|
| MSG-PARAM-001 | Descricao dos parametros fiscais obrigatoria e limitada a 100 caracteres. | Descricao ausente ou excedida. |
| MSG-PARAM-002 | Tipo de ambiente NFC-e obrigatorio. | Ambiente NFC-e ausente. |
| MSG-PARAM-003 | Tipo de ambiente NF-e obrigatorio. | Ambiente NF-e ausente. |
| MSG-PARAM-004 | Campos de producao NFC-e obrigatorios. | NFC-e em producao sem CSC, ID CSC, serie ou proximo numero. |
| MSG-PARAM-005 | Campos de producao NF-e obrigatorios. | NF-e em producao sem campos de producao informados. |
| MSG-PARAM-006 | Serie de producao nao pode iniciar como zero. | Serie NF-e/NFC-e producao igual a zero. |
| MSG-PARAM-007 | Proximo numero de producao nao pode iniciar como zero. | Proximo numero NF-e/NFC-e producao igual a zero. |
| MSG-PARAM-008 | CSC NFC-e excede o tamanho permitido. | CSC maior que 36 caracteres. |
| MSG-PARAM-009 | ID CSC NFC-e excede o tamanho permitido. | ID CSC maior que 6 caracteres. |
| MSG-PARAM-010 | Certificado nao encontrado. | Operacao fiscal exige certificado inexistente. |
| MSG-PARAM-011 | Certificado invalido. | Certificado nao pode ser usado na transmissao. |
| MSG-PARAM-012 | Arquivo nao encontrado. | Arquivo fiscal/certificado requerido nao localizado. |
| MSG-PARAM-013 | Erro ao transmitir certificado digital para o contexto fiscal. | Falha na transmissao do certificado. |
| MSG-PARAM-014 | Configuracao de impressao NFC-e ja existe para a empresa. | Tentativa de duplicar configuracao. |

## 17. Requisitos nao funcionais

| Categoria | Requisito |
|---|---|
| Seguranca | Senha, arquivo, serial e caminho do certificado devem ser protegidos e nao expostos em logs ou telas sem permissao. |
| Auditoria | Alteracoes de ambiente, serie, numero, CSC, certificado e layout devem registrar usuario/processo e data/hora. |
| Integridade | Numeracao fiscal deve preservar consistencia por empresa, ambiente, modelo e serie. |
| Disponibilidade | Parametros fiscais devem estar disponiveis antes da emissao e eventos fiscais. |
| Observabilidade | Falhas de certificado, arquivo, ambiente e numeracao devem ser diagnosticaveis. |
| Retencao | Retencao de certificado e historico de parametros nao informada no material. |

## 18. Criterios de aceite

| Criterio | Resultado esperado |
|---|---|
| Ambientes obrigatorios | NF-e e NFC-e nao salvam sem ambiente definido. |
| Producao NFC-e completa | NFC-e em producao exige CSC, ID CSC, serie e proximo numero validos. |
| Producao NF-e minima | NF-e em producao exige serie e proximo numero validos, conforme material. |
| Tamanhos CSC | CSC respeita max 36 e ID CSC respeita max 6 em homologacao e producao. |
| Certificado | Operacoes fiscais bloqueiam transmissao sem certificado valido. |
| Impressao NFC-e | Apenas uma configuracao de impressao NFC-e por empresa. |
| Modelo de dados | Todos os campos comprovados aparecem no dicionario com tipo/formato, tamanho/dominio, obrigatoriedade, chave/relacao e regra. |
| Lacunas | Campos nao comprovados estao marcados como `Nao informado no material` e aparecem na MC. |

## 19. Itens para MC

| Item | Motivo |
|---|---|
| Estrutura completa de NF-e producao | Material cita campos obrigatorios, mas nao traz entidade completa. |
| Concorrencia de numeracao | Material exige nao-zero, mas nao define bloqueio transacional/idempotencia. |
| Historico/versionamento de parametros | EF consolidada cita versionamento/auditoria; estrutura nao informada. |
| Criptografia de senha/certificado | Material traz senha/caminho, mas nao define mecanismo seguro. |
| Alertas de vencimento do certificado | Material traz validade, mas nao define politica de alerta. |
| Versoes de servico fiscal | Material cita 15 campos, mas nao consolida nomes/uso final. |
| Parametros CF-e/SAT | Material cita modelo/status, mas nao detalha parametros neste recorte. |

## 20. Notas de rodape

[^nota1]: As consultas de diagnostico de aptidao fiscal foram criadas nesta EF como organizacao operacional das validacoes ja comprovadas no material; os campos usados nelas devem permanecer restritos aos dados existentes e lacunas devem ser mantidas na MC.
