# Especificacao Funcional - Epros

**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Capacidade:** NFE_SAIDA  
**Versao:** V1  
**Empresa:** Siser  
**Status:** Concluido para validacao humana  

## 1. Controle do documento

| Item | Conteudo |
|---|---|
| Responsavel pela elaboracao | Analise funcional assistida |
| Responsavel pela validacao funcional | Siser |
| Responsavel pela validacao tecnica | Siser |
| Area dona do processo | Fiscal, Vendas, Financeiro, Estoque, Cadastros, Plataforma |
| Publico-alvo | Produto, negocio, implantacao, desenvolvimento, suporte, operacao fiscal |
| Fonte de verdade | Esta EF descreve a emissao de NF-e de saida no Epros |

## 2. Objetivo funcional

NF-e Saida existe para emitir, transmitir, registrar, consultar e disponibilizar a Nota Fiscal Eletronica modelo 55 gerada a partir de uma venda, faturamento ou integracao interna do Epros.

O processo deve validar dados fiscais, montar o documento, assinar/validar quando aplicavel, transmitir para a autoridade fiscal, registrar retorno de autorizacao ou rejeicao, gravar XML de envio e retorno, disponibilizar DANFE/PDF, permitir consulta/download e manter vinculo operacional com o fato gerador.

## 3. Escopo funcional

### 3.1 Dentro do escopo

| Capacidade | Descricao | Observacao |
|---|---|---|
| Emissao NF-e modelo 55 | Gera e transmite NF-e de saida. | Modelo fiscal 55 informado no material. |
| Emissao simplificada | Recebe dados estruturados de venda e transmite documento. | Contrato final deve ser validado na MC. |
| Emissao completa | Recebe estrutura mais completa de documento fiscal. | Material cita emissao completa. |
| Previa/DANFE sem autorizacao | Gera DANFE/visualizacao antes de autorizacao quando solicitado. | Nao altera status autorizado. |
| Autorizacao fiscal | Assina, valida, transmite e registra retorno. | Retorno autorizado usa status fiscal e protocolo. |
| Rejeicao fiscal | Registra motivo da rejeicao e permite retransmissao quando status permitir. | Estado rejeitado pode retornar para transmissao. |
| XML autorizado | Mantem XML de envio e XML de retorno/autorizado. | XML deve ficar disponivel para download. |
| PDF/DANFE | Gera, regenera e baixa PDF da NF-e. | DANFE autorizado e previa aparecem no material. |
| Download por chave | Baixa XML/PDF por chave fiscal. | Erro funcional se arquivo/documento nao existir. |
| Download por localizador | Baixa XML de envio por vinculo externo ou chave composta. | Material cita localizador externo, ambiente, modelo, documento, serie e numero. |
| Listagem por periodo | Consulta documentos por emitente/destinatario e periodo. | Usada tambem por contador e telas fiscais. |
| Cancelamento e CC-e relacionados | NF-e saida possui relacao com cancelamento e carta de correcao. | Detalhamento profundo fica nas EFs especificas de eventos. |
| Itens e tributos | Registra itens, CFOP, NCM, CST/CSOSN, PIS, COFINS, IPI, rateios e totais. | Calculo detalhado fica no motor tributario. |

### 3.2 Fora do escopo

| Item | Tratamento |
|---|---|
| NFC-e | Possui EF especifica. |
| NF-e entrada | Possui EF especifica. |
| Devolucao | Possui EF especifica. |
| Cancelamento completo | Possui EF especifica; esta EF apenas referencia relacao. |
| Carta de correcao completa | Possui EF especifica; esta EF apenas referencia relacao. |
| Inutilizacao | Possui EF especifica. |
| Pedido, faturamento comercial e contas a receber | Permanecem nos modulos donos; esta EF cobre protocolo fiscal e persistencia fiscal. |
| Cadastro mestre de pessoa, empresa, produto e endereco | Permanecem em Cadastros Base. |
| Calculo tributario detalhado | Possui EF especifica de motor tributario. |

## 4. Glossario funcional

| Termo | Definicao | Observacao |
|---|---|---|
| NF-e | Nota Fiscal Eletronica. | Modelo fiscal 55. |
| Documento fiscal | Registro fiscal emitido pelo Epros. | Possui status fiscal e retorno da autoridade fiscal. |
| XML de envio | XML montado e enviado para autorizacao. | Deve ser preservado. |
| XML de retorno | XML retornado pela autorizacao ou processamento fiscal. | Pode estar vazio quando ainda nao houver retorno. |
| DANFE | Documento auxiliar em PDF. | Pode ser previa ou documento autorizado. |
| Chave | Identificador fiscal da NF-e. | Usada para downloads, eventos e consulta. |
| Recibo | Identificador de processamento quando informado. | Usado em transmissao assincrona quando aplicavel. |
| Protocolo | Numero de autorizacao/retorno fiscal. | Gravado quando retornado. |
| Localizador externo | Identificador de vinculo com venda/compra/processo interno. | Usado para download do XML de envio. |
| Status fiscal | Situacao do documento no Epros. | Recebido, Autorizado, Rejeitado, Cancelado ou equivalente. |
| Status da autoridade fiscal | Codigo numerico retornado pela autoridade fiscal. | cStat 100 aparece como autorizado no material. |

## 5. Atores, papeis e responsabilidades

| Ator/Papel | Responsabilidade | Permissoes esperadas | Restricoes |
|---|---|---|---|
| Operador fiscal | Emitir, consultar, retransmitir, baixar XML/PDF e acompanhar rejeicoes. | Criar, consultar, baixar e retransmitir quando permitido. | Nao altera parametros fiscais criticos sem permissao. |
| Operador de vendas | Solicitar emissao a partir de venda/faturamento. | Acionar emissao ligada ao fato gerador. | Nao edita dados fiscais apos autorizacao sem fluxo permitido. |
| Gestor fiscal | Validar cadastros, regras e rejeicoes fiscais. | Consultar, corrigir parametros e aprovar ajustes. | Deve respeitar tenant e empresa. |
| Contador | Baixar XML/PDF por periodo e consultar documentos fiscais. | Consulta e download. | Sem manutencao de parametros, salvo permissao. |
| Integracao interna | Enviar dados de venda/documento para emissao. | Contrato autenticado. | Deve respeitar tenant, empresa e idempotencia. |
| Suporte | Diagnosticar falhas de transmissao, certificado, arquivo ou rejeicao. | Consulta auditada. | Nao altera documento fiscal autorizado sem fluxo formal. |

## 6. Pre-condicoes

| Pre-condicao | Regra |
|---|---|
| Empresa existente | Empresa deve estar identificada e vinculada ao tenant. |
| Parametros fiscais validos | Ambiente NF-e, serie/proximo numero e certificado devem estar prontos quando exigidos. |
| Certificado disponivel | Certificado nao encontrado ou invalido bloqueia transmissao. |
| Emitente valido | CPF/CNPJ do emitente deve ser valido e possuir dados fiscais minimos. |
| Destinatario valido | CPF/CNPJ do destinatario deve ser valido quando informado/obrigatorio. |
| Endereco do destinatario | NF-e exige endereco do cliente quando aplicavel ao fluxo de transmissao. |
| Municipio do emitente | Codigo IBGE do municipio do emitente deve ser diferente de zero. |
| Tipo de operacao fiscal | Parametros, CFOP e tipo de operacao devem existir quando a emissao depender deles. |
| Itens validos | Itens devem possuir campos fiscais e valores necessarios ao calculo. |

## 7. Visao operacional

1. O usuario ou integracao solicita emissao de NF-e de saida.
2. O Epros identifica tenant, empresa, venda/faturamento e parametros fiscais.
3. O Epros valida emitente, destinatario, endereco, certificado, ambiente, serie e numero.
4. O Epros monta dados fiscais do documento, itens, totais, pagamentos, transporte e informacoes adicionais quando informados.
5. O Epros aplica validacoes fiscais e de motor tributario.
6. O Epros gera XML de envio.
7. O Epros transmite para a autoridade fiscal.
8. Quando autorizado, grava chave, numero, protocolo, XML, PDF/DANFE, status e vinculo operacional.
9. Quando rejeitado, grava motivo, status e permite nova transmissao se o estado permitir.
10. O Epros disponibiliza consulta e download de XML/PDF.

## 8. Capacidades funcionais detalhadas

### 8.1 Emitir NF-e de saida

| Item | Especificacao |
|---|---|
| Objetivo | Transmitir NF-e modelo 55 e registrar o retorno fiscal. |
| Acionamento | Venda, faturamento, tela fiscal ou integracao interna. |
| Pre-condicoes | Empresa parametrizada, certificado valido, itens e destinatario validos. |
| Dados de entrada | Emitente, destinatario, itens, pagamentos, transporte, totais, ambiente, serie, numero, finalidade, tipo de operacao, CFOP, informacoes fiscais e localizador externo. |
| Processamento | Validar dados, montar XML, assinar/validar quando aplicavel, transmitir e registrar retorno. |
| Resultado esperado | Documento autorizado ou rejeitado com status, mensagem e evidencias fiscais. |
| Pos-condicoes | XML/PDF disponiveis; venda/faturamento pode receber chave/status quando aplicavel. |
| Excecoes | Certificado ausente, documento invalido, destinatario invalido, falha de comunicacao, rejeicao fiscal, arquivo nao localizado. |
| Auditoria | Usuario/processo, data/hora, empresa, chave, serie, numero, status, protocolo, motivo e localizador. |

### 8.2 Gerar previa ou DANFE sem autorizacao

| Item | Especificacao |
|---|---|
| Objetivo | Gerar representacao da NF-e antes da autorizacao. |
| Acionamento | Usuario solicita previa/DANFE antes de transmitir. |
| Pre-condicoes | Dados minimos de documento suficientes para montar XML/visualizacao. |
| Dados de entrada | Documento fiscal e itens. |
| Processamento | Gerar XML ou PDF sem alterar status para autorizado. |
| Resultado esperado | Arquivo de previa disponivel para conferencia. |
| Excecoes | Dados insuficientes, erro de validacao ou fonte de dados de relatorio ausente. |
| Auditoria | Usuario, data/hora, documento e resultado. |

### 8.3 Registrar autorizacao

| Item | Especificacao |
|---|---|
| Objetivo | Persistir retorno autorizado da autoridade fiscal. |
| Acionamento | Retorno da transmissao. |
| Pre-condicoes | Documento transmitido com retorno autorizado. |
| Dados de entrada | Chave, protocolo, recibo quando informado, XML de envio, XML de retorno, status fiscal, status da autoridade fiscal, numero e serie. |
| Processamento | Atualizar documento, vincular XML, gerar/gravar PDF quando disponivel e atualizar localizador. |
| Resultado esperado | NF-e autorizada e disponivel para consulta/download. |
| Excecoes | Retorno sem dados minimos, falha ao gravar XML/PDF ou documento nao localizado. |
| Auditoria | Status, protocolo, chave, XML, PDF, usuario/processo e data/hora. |

### 8.4 Registrar rejeicao

| Item | Especificacao |
|---|---|
| Objetivo | Registrar retorno rejeitado sem perder evidencias. |
| Acionamento | Autoridade fiscal ou validacao do Epros rejeita a transmissao. |
| Pre-condicoes | Documento em transmissao. |
| Dados de entrada | Status da autoridade fiscal, motivo de rejeicao, XML de envio e dados do documento. |
| Processamento | Alterar status para rejeitado e preservar motivo. |
| Resultado esperado | Documento rejeitado, consultavel e elegivel para correcao/retransmissao quando permitido. |
| Excecoes | Motivo nao informado, falha de comunicacao ou erro tecnico. |
| Auditoria | Status, motivo, data/hora, usuario/processo e tentativa. |

### 8.5 Baixar XML/PDF

| Item | Especificacao |
|---|---|
| Objetivo | Disponibilizar XML e PDF de NF-e autorizada ou enviada. |
| Acionamento | Usuario, contador ou integracao solicita download. |
| Pre-condicoes | Documento existente, permissao valida e arquivo disponivel. |
| Dados de entrada | Chave, periodo, localizador externo, ambiente, modelo, documento, serie e numero conforme tipo de busca. |
| Processamento | Localizar documento, validar permissao, localizar arquivo e retornar conteudo. |
| Resultado esperado | XML, PDF ou erro funcional claro. |
| Excecoes | Chave invalida, documento nao localizado, arquivo nao encontrado ou permissao insuficiente. |
| Auditoria | Usuario/processo, chave, tipo de arquivo, data/hora e origem da consulta. |

## 9. Regras de negocio

| Regra | Descricao | Condicao | Resultado | Severidade | Observacoes |
|---|---|---|---|---|---|
| REG-NFE-001 | NF-e de saida usa modelo fiscal 55. | Emissao NF-e. | Gerar documento modelo 55. | Bloqueante |  |
| REG-NFE-002 | XML de NF-e deve usar versao 4.00 quando essa versao for a configurada no material. | Montagem XML. | Montar XML na versao informada. | Bloqueante | Material cita versao 4.00. |
| REG-NFE-003 | Numeracao de NF-e deve vir da serie e proximo/ultimo numero fiscal da empresa. | Emissao. | Atribuir numero fiscal. | Bloqueante | Concorrencia final fica na MC. |
| REG-NFE-004 | Cliente/destinatario deve possuir CPF/CNPJ quando a emissao exigir destinatario. | Emissao NF-e. | Bloquear emissao. | Bloqueante |  |
| REG-NFE-005 | NF-e ja emitida/autorizada nao deve ser transmitida novamente pelo fluxo normal. | Documento com numero/status autorizado. | Direcionar para detalhe/download. | Bloqueante |  |
| REG-NFE-006 | Documento em estado novo ou rejeitado pode ser transmitido. | Transmissao. | Permitir transmissao. | Bloqueante | Material mostra retransmissao de rejeitado. |
| REG-NFE-007 | Documento autorizado deve gravar chave, numero e status autorizado. | Retorno autorizado. | Atualizar documento. | Bloqueante |  |
| REG-NFE-008 | Documento rejeitado deve gravar status rejeitado e motivo de rejeicao. | Retorno rejeitado. | Atualizar documento. | Bloqueante |  |
| REG-NFE-009 | XML autorizado deve ser gravado apos transmissao bem-sucedida. | Retorno autorizado. | Preservar XML. | Bloqueante |  |
| REG-NFE-010 | Download de XML deve validar permissao e existencia do arquivo. | Download XML. | Entregar arquivo ou erro funcional. | Bloqueante |  |
| REG-NFE-011 | Forma de pagamento em previa deve exibir descricao unica quando houver um metodo e indicacao multipla quando houver mais de um. | Previa/DANFE. | Exibir pagamento conforme quantidade. | Media | Material cita essa regra de preview. |
| REG-NFE-012 | Destino da operacao deve considerar UF do emitente, UF do destinatario e exterior quando informado. | Montagem fiscal. | Definir idDest. | Bloqueante | Mesma UF=1, diferente=2, exterior=3 no material. |
| REG-NFE-013 | Finalidade da NF-e deve vir da natureza/tipo de operacao fiscal quando informada. | Montagem fiscal. | Preencher finalidade. | Bloqueante |  |
| REG-NFE-014 | Tipo de operacao fiscal deve definir entrada/saida quando informado. | Montagem fiscal. | Preencher tipo da NF-e. | Bloqueante |  |
| REG-NFE-015 | Indicador de consumidor final deve vir do cadastro do destinatario/contato quando informado. | Montagem fiscal. | Preencher indicador. | Media |  |
| REG-NFE-016 | Indicador de intermediador pode ser marcado em homologacao para pedido de comercio eletronico quando informado. | Ambiente homologacao e pedido eletronico. | Preencher indicador. | Media | Material traz condicao especifica. |
| REG-NFE-017 | Indicador de inscricao estadual do destinatario deve considerar contribuinte, isento/nulo e exterior. | Montagem fiscal. | Preencher indicador IE. | Bloqueante | Exterior usa indicador 9 no material. |
| REG-NFE-018 | CRT/regime do emitente deve estar disponivel. | Montagem XML. | Bloquear ou rejeitar montagem se ausente. | Bloqueante |  |
| REG-NFE-019 | Emitente e destinatario devem ter CPF/CNPJ validos. | Pre-emissao. | Bloquear emissao. | Bloqueante |  |
| REG-NFE-020 | Certificado digital deve existir para transmitir. | Pre-emissao. | Bloquear transmissao. | Bloqueante |  |
| REG-NFE-021 | Parametros fiscais, tipo de operacao e CFOP da venda nao podem estar nulos quando exigidos. | Pre-emissao. | Bloquear emissao. | Bloqueante |  |
| REG-NFE-022 | NF-e exige endereco do cliente quando aplicavel. | Pre-emissao. | Bloquear emissao. | Bloqueante |  |
| REG-NFE-023 | CEP do cliente deve possuir exatamente 8 caracteres quando exigido na emissao. | Pre-emissao. | Bloquear emissao. | Bloqueante |  |
| REG-NFE-024 | E-mail do cliente deve ser valido quando tipo de contato for e-mail. | Pre-emissao. | Bloquear ou alertar conforme fluxo de contato. | Media |  |
| REG-NFE-025 | Telefone da empresa deve possuir entre 8 e 14 caracteres quando utilizado. | Pre-emissao. | Bloquear emissao se invalido. | Media |  |
| REG-NFE-026 | Retorno autorizado com codigo 100 deve salvar movimento/documento fiscal. | Retorno fiscal. | Registrar autorizacao. | Bloqueante | Codigo 100 aparece como autorizado no material. |
| REG-NFE-027 | XML de envio e retorno deve ser preservado na entidade de XML. | Autorizacao/rejeicao. | Gravar XML. | Bloqueante |  |
| REG-NFE-028 | Falha de comunicacao ou validacao de esquema deve ser tratada como erro funcional da transmissao. | Transmissao. | Registrar erro e nao autorizar documento. | Bloqueante |  |
| REG-NFE-029 | Download por periodo deve permitir listagem de documentos por documento fiscal e intervalo. | Consulta. | Retornar pagina de documentos. | Media |  |
| REG-NFE-030 | Download por localizador externo deve localizar XML de envio vinculado ao fato gerador. | Download XML envio. | Entregar XML ou erro. | Media |  |
| REG-NFE-031 | Regeneracao de PDF deve localizar NF-e pela chave. | Regerar PDF. | Gerar novo PDF ou erro. | Media |  |
| REG-NFE-032 | Arquivo fiscal nao localizado deve retornar erro claro e nao alterar status do documento. | Download/regeneracao. | Bloquear download/regeneracao. | Bloqueante |  |
| REG-NFE-033 | Documento de devolucao exige chaves referenciadas, mas o detalhamento pertence a EF de devolucao. | Finalidade devolucao. | Bloquear emissao sem referencia. | Bloqueante | Referenciado aqui por impacto na NF-e. |

## 10. Parametros de configuracao

| Parametro | Finalidade | Tipo/formato | Valor padrao | Obrigatorio | Nivel | Quem pode alterar | Impacto |
|---|---|---|---|---|---|---|---|
| Ambiente NF-e | Define ambiente de emissao. | Enum | Nao informado no material | Sim | Empresa | Gestor fiscal | Direciona transmissao. |
| Serie NF-e | Define serie fiscal da NF-e. | Numero | Nao informado no material | Sim/condicional | Empresa | Gestor fiscal | Controla numeracao. |
| Proximo/ultimo numero NF-e | Define numero fiscal a emitir. | Numero | Nao informado no material | Sim/condicional | Empresa | Gestor fiscal | Controla numeracao. |
| Certificado digital | Assina/transmite NF-e. | Arquivo/senha | Nao informado no material | Sim para transmissao | Empresa | Gestor fiscal | Sem certificado nao ha transmissao. |
| Tipo de operacao fiscal | Define finalidade, tipo, CFOP e informacoes. | Cadastro fiscal | Nao informado no material | Condicional | Empresa | Gestor fiscal | Alimenta XML. |
| Regime/CRT emitente | Define regime fiscal do emitente. | Enum/codigo | Nao informado no material | Sim | Empresa | Gestor fiscal | Afeta XML/totais. |
| Caminho XML/PDF | Armazena XML e PDF. | Texto | Nao informado no material | Sim | Tenant/empresa | Administrador Siser | Impacta downloads. |
| Timeout de transmissao | Limita comunicacao fiscal. | Numero | Nao informado no material | Nao informado no material | Global/empresa | Administrador Siser | Impacta transmissao. |

## 11. Modelo de dados funcional e implantavel

### 11.1 Entidades/tabelas

| Entidade/tabela | Papel funcional | Conteudo |
|---|---|---|
| nfe_simplificado | Documento fiscal NF-e de saida. | Com conteudo completo para campos extraidos. |
| nfe_simplificado_item | Itens da NF-e de saida. | Com conteudo completo para campos extraidos. |
| nfe_simplificado_xml | XML de envio e retorno da NF-e. | Com conteudo completo para campos extraidos. |
| nfe_simplificado_cancelamento | Cancelamento relacionado a NF-e. | Referenciado nesta EF; detalhe em EF de cancelamento. |
| nfe_simplificado_carta_correcao | Cartas de correcao relacionadas a NF-e. | Referenciado nesta EF; detalhe em EF de CC-e. |

### 11.2 Relacionamentos

| Origem | Relacionamento | Destino | Cardinalidade | Obrigatorio | Regra de integridade |
|---|---|---|---|---|---|
| nfe_simplificado | possui | nfe_simplificado_item | 1:N | Sim | NF-e deve possuir itens quando emitida. |
| nfe_simplificado | possui | nfe_simplificado_xml | 1:1 | Sim | XML de envio deve ser preservado. |
| nfe_simplificado | possui | nfe_simplificado_cancelamento | 1:1 | Condicional | Criado quando cancelamento for autorizado. |
| nfe_simplificado | possui | nfe_simplificado_carta_correcao | 1:N | Condicional | Cada CC-e possui sequencia. |
| nfe_simplificado | referencia | localizador externo | Nao informado no material | Condicional | Vincula a venda/faturamento/processo de origem. |

### 11.3 Dicionario de dados implantavel - nfe_simplificado

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| Id | Identificador | Nao informado no material | Nao informado no material | PK | Nao informado no material. |
| TenantId | Texto | varchar(200) | Sim | Tenant | Isolamento do documento. |
| NfeSimplificadoXmlId | Identificador | Nao informado no material | Nao | FK XML | Relacao com XML da NF-e. |
| NfeSimplificadoCancelamentoId | Identificador | Nao informado no material | Nao | FK cancelamento | Preenchido quando cancelado. |
| Ambiente | Numero/enum | Nao informado no material | Sim | Atributo | Ambiente fiscal. |
| Crt | Numero/enum | Nao informado no material | Sim | Atributo | CRT/regime do emitente. |
| DocumentoEmitente | Texto/documento | varchar(20) | Sim | Empresa emitente | CPF/CNPJ do emitente. |
| DocumentoDestinatario | Texto/documento | varchar(20) | Nao | Pessoa destinataria | CPF/CNPJ destinatario quando informado. |
| Uf | Texto | varchar(2) | Sim | UF | UF do documento. |
| Chave | Texto | varchar(50) | Nao | Chave fiscal | Chave da NF-e quando gerada/autorizada. |
| Recibo | Texto | varchar(50) | Nao | Retorno fiscal | Recibo quando informado. |
| Protocolo | Texto | varchar(50) | Nao | Retorno fiscal | Protocolo da autorizacao. |
| Serie | Numero | Nao informado no material | Sim | Numeracao | Serie fiscal. |
| Numero | Numero | Nao informado no material | Sim | Numeracao | Numero fiscal. |
| Status | Enum | Nao informado no material | Sim | Estado | Status fiscal no Epros. |
| StatusSefaz | Numero | Nao informado no material | Sim | Retorno fiscal | Codigo retornado pela autoridade fiscal. |
| MotivoRejeicaoSefaz | Texto | nvarchar(max) | Nao | Retorno fiscal | Motivo de rejeicao. |
| Total | Decimal | decimal(18,2) | Sim | Valor | Total da NF-e. |
| PdfCaminho | Texto | varchar(500) | Nao | Arquivo | Caminho do PDF/DANFE. |
| XmlCaminho | Texto | varchar(500) | Nao | Arquivo | Caminho do XML. |
| JsonRecebido | Texto/JSON | nvarchar(max) | Sim | Payload | Dados recebidos para emissao. |
| DataEmissao | Data/hora | Nao informado no material | Nao | Data fiscal | Data da emissao. |
| LocalizadorExternoId | Texto | varchar(300) | Nao | Vinculo externo | Relaciona documento a origem operacional. |
| TipoNFe | Enum | Nao informado no material | Sim | Tipo fiscal | Tipo da NF-e. |

### 11.4 Dicionario de dados implantavel - nfe_simplificado_item

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| Id | Identificador | long | Sim | PK | Identificador do item. |
| TenantId | Texto | varchar(200) | Sim | Tenant | Isolamento do item. |
| NfeSimplificadoId | Identificador | long | Sim | FK NF-e | Vincula item ao documento. |
| CodigoProduto | Texto | varchar(60) | Sim | Produto | Codigo do produto. |
| NomeProduto | Texto | varchar(120) | Sim | Produto | Nome do produto. |
| CodigoBarras | Texto | varchar(20) | Sim | Produto | Codigo de barras. |
| Ncm | Texto | varchar(50) | Sim | NCM | NCM do item. |
| Cfop | Numero/codigo | Nao informado no material | Sim | CFOP | CFOP do item. |
| Unidade | Texto | varchar(50) | Sim | Unidade | Unidade comercial/tributavel. |
| ValorUnitario | Decimal | decimal(21,10) | Sim | Valor | Valor unitario. |
| Quantidade | Decimal | decimal(15,4) | Sim | Quantidade | Quantidade do item. |
| Origem | Texto | varchar(5) | Sim | Tributacao | Origem da mercadoria. |
| Csosn | Texto | varchar(5) | Nao | Tributacao | CSOSN quando aplicavel. |
| CstIcms | Texto | varchar(5) | Nao | Tributacao | CST ICMS quando aplicavel. |
| ValorAliquotaIcms | Decimal | decimal(18,3) | Sim | Tributacao | Aliquota ICMS. |
| ValorReducaoIcmsPercentual | Decimal | decimal(18,2) | Sim | Tributacao | Percentual reducao ICMS. |
| TipoReducaoIcms | Enum | Nao informado no material | Sim | Tributacao | Tipo de reducao ICMS. |
| ValorBaseCalculoStRetidoOperacaoAnterior | Decimal | decimal(18,3) | Sim | Tributacao | Base ST retida anterior. |
| ValorAlioquotaSt | Decimal | decimal(18,2) | Sim | Tributacao | Aliquota ST conforme nome preservado no material. |
| ValorIcmsStRetidoOperacaoAnterior | Decimal | decimal(18,2) | Sim | Tributacao | ICMS ST retido anterior. |
| ValorIcmsProprioSubstituto | Decimal | decimal(18,2) | Sim | Tributacao | ICMS proprio substituto. |
| CstPisCofins | Texto | varchar(3) | Sim | Tributacao | CST PIS/COFINS. |
| ValorAliquotaPis | Decimal | decimal(18,2) | Sim | Tributacao | Aliquota PIS. |
| ValorAliquotaPisReal | Decimal | decimal(18,4) | Sim | Tributacao | Aliquota PIS real. |
| ValorAliquotaCofins | Decimal | decimal(18,2) | Sim | Tributacao | Aliquota COFINS. |
| ValorAliquotaCofinsReal | Decimal | decimal(18,4) | Sim | Tributacao | Aliquota COFINS real. |
| CompoeValorTotal | Enum | Nao informado no material | Sim | Totalizacao | Indica se compoe total. |
| ValorDesconto | Decimal | decimal(18,2) | Sim | Rateio/valor | Desconto do item. |
| ValorDescontoRateado | Decimal | decimal(18,2) | Sim | Rateio | Desconto rateado. |
| ValorFreteRateado | Decimal | decimal(18,2) | Sim | Rateio | Frete rateado. |
| ValorSeguroRateado | Decimal | decimal(18,2) | Sim | Rateio | Seguro rateado. |
| ValorAcrescimoRateado | Decimal | decimal(18,2) | Sim | Rateio | Acrescimo rateado. |
| ValorOutroRateado | Decimal | decimal(18,2) | Sim | Rateio | Outros valores rateados. |
| EnquadramentoIpi | Texto | varchar(5) | Nao | IPI | Enquadramento IPI. |
| CstIpi | Texto | varchar(5) | Nao | IPI | CST IPI. |
| ValorAliquotaIpi | Decimal | decimal(18,2) | Sim | IPI | Aliquota IPI. |
| ValorReducaoIpiPercentual | Decimal | decimal(18,2) | Sim | IPI | Reducao IPI. |
| TipoReducaoIpi | Enum | Nao informado no material | Sim | IPI | Tipo reducao IPI. |
| Deletado | Data/hora | Nao informado no material | Nao | Exclusao logica | Data de exclusao logica quando houver. |

### 11.5 Dicionario de dados implantavel - nfe_simplificado_xml

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| Id | Identificador | Nao informado no material | Nao informado no material | PK | Nao informado no material. |
| TenantId | Texto | varchar(200) | Sim | Tenant | Isolamento do XML. |
| XmlEnvio | Texto/XML | nvarchar(max) | Sim | XML | XML enviado. |
| XmlRetorno | Texto/XML | nvarchar(max) | Nao | XML | XML retornado/autorizado quando houver. |
| NfeSimplificadoId | Identificador | Nao informado no material | Sim | FK NF-e | Relacao com NF-e. |

## 12. Estados

| Estado | Significado | Acoes permitidas |
|---|---|---|
| Novo | Documento ainda sem numero/autorizacao fiscal. | Previa e transmissao. |
| Rejeitado | Autoridade fiscal ou validacao recusou transmissao. | Correcao e retransmissao. |
| Autorizado | Documento foi autorizado. | DANFE, XML, CC-e, cancelamento, e-mail e ZIP quando aplicavel. |
| Cancelado | Documento recebeu evento de cancelamento. | XML/PDF de cancelamento e consulta. |
| Erro | Falha tecnica ou funcional sem autorizacao. | Diagnostico e nova tentativa conforme causa. |

## 13. Fluxos funcionais

### 13.1 Emissao e autorizacao

| Passo | Ator | Acao | Validacao | Resultado |
|---:|---|---|---|---|
| 1 | Usuario/integracao | Solicita emissao de NF-e. | Empresa, venda/faturamento e tenant. | Documento recebido. |
| 2 | Epros | Carrega parametros fiscais. | Ambiente, serie, numero e certificado. | Parametros prontos ou bloqueio. |
| 3 | Epros | Valida emitente/destinatario. | CPF/CNPJ, endereco, UF, municipio, CEP e contato quando exigidos. | Dados validos ou rejeicao funcional. |
| 4 | Epros | Monta documento e itens. | CFOP, NCM, CST/CSOSN, PIS, COFINS, IPI, totais, pagamento e transporte. | XML de envio preparado. |
| 5 | Epros | Transmite documento. | Certificado e XML validos. | Retorno fiscal. |
| 6 | Epros | Registra retorno autorizado. | Codigo autorizado e dados minimos. | Chave, protocolo, XML/PDF e status autorizado. |
| 7 | Epros | Registra retorno rejeitado. | Motivo de rejeicao. | Status rejeitado e motivo para correcao. |

### 13.2 Consulta e downloads

| Passo | Ator | Acao | Validacao | Resultado |
|---:|---|---|---|---|
| 1 | Usuario/contador | Solicita consulta por periodo ou chave. | Permissao e empresa. | Lista/documento localizado. |
| 2 | Epros | Localiza XML/PDF. | Arquivo existente. | Arquivo preparado. |
| 3 | Epros | Entrega arquivo. | Tipo solicitado. | XML/PDF/ZIP ou erro funcional. |

## 14. Telas e operacoes esperadas

| Tela/operacao | Campos principais | Acoes | Observacao |
|---|---|---|---|
| Emissao NF-e | Empresa, venda/faturamento, destinatario, itens, serie, numero, total, status. | Gerar previa, transmitir, corrigir rejeicao. | Tela final nao detalhada no material. |
| Detalhe NF-e | Chave, protocolo, XML, PDF, status, motivo rejeicao, itens. | Baixar XML, baixar PDF, regenerar PDF, cancelar, CC-e. | Cancelamento e CC-e possuem EFs proprias. |
| Listagem NF-e | Periodo, emitente/destinatario, status, numero, serie. | Consultar, abrir detalhe, baixar arquivos. | Material cita listagem por periodo e downloads. |
| Transmissoes fiscais | Chave, status, arquivos, localizador. | Baixar XML/PDF por chave/localizador. | Operacao voltada a suporte/contador. |

## 15. Integracoes

| Integracao | Direcao | Dados | Regra |
|---|---|---|---|
| Vendas/faturamento | Entrada/Saida | Venda, itens, pagamentos, chave, status fiscal. | Regras comerciais ficam no modulo dono. |
| Cadastros Base | Entrada | Empresa, pessoa, endereco, municipio, produto. | Dados mestres nao sao duplicados. |
| Parametros fiscais | Entrada | Ambiente, serie, numero, certificado, tipo de operacao. | Sem parametros validos nao ha transmissao. |
| Motor tributario | Entrada/Saida | Itens, CFOP, NCM, CST/CSOSN, tributos e rateios. | Detalhe em EF propria. |
| Financeiro | Saida | Efeitos de faturamento autorizados. | Geracao financeira fica no modulo dono. |
| Relatorios/contador | Saida | XML, PDF, periodo e ZIP. | Detalhe em XML contador/downloads. |

## 16. Mensagens e erros funcionais

| Codigo | Mensagem funcional | Quando ocorre |
|---|---|---|
| MSG-NFE-001 | NF-e nao localizada. | Consulta/download por chave ou id sem documento. |
| MSG-NFE-002 | Certificado nao encontrado. | Transmissao exige certificado inexistente. |
| MSG-NFE-003 | Certificado invalido. | Certificado nao pode ser usado. |
| MSG-NFE-004 | Arquivo nao encontrado. | XML/PDF requerido nao existe. |
| MSG-NFE-005 | Documento ja autorizado. | Tentativa de retransmitir documento autorizado. |
| MSG-NFE-006 | Documento rejeitado. | Autoridade fiscal rejeita transmissao. |
| MSG-NFE-007 | CPF/CNPJ do emitente invalido. | Pre-emissao. |
| MSG-NFE-008 | CPF/CNPJ do destinatario invalido. | Pre-emissao. |
| MSG-NFE-009 | Endereco do cliente obrigatorio. | NF-e exige endereco. |
| MSG-NFE-010 | CEP do cliente invalido. | CEP diferente de 8 caracteres. |
| MSG-NFE-011 | Tipo de operacao fiscal obrigatorio. | Tipo de operacao ausente. |
| MSG-NFE-012 | CFOP obrigatorio. | CFOP ausente quando exigido. |
| MSG-NFE-013 | Erro de comunicacao com autoridade fiscal. | Falha de transmissao. |
| MSG-NFE-014 | Erro de validacao do documento fiscal. | Schema/dados invalidos antes ou durante transmissao. |

## 17. Requisitos nao funcionais

| Categoria | Requisito |
|---|---|
| Seguranca | Downloads, emissao e retransmissao devem exigir permissao e contexto de tenant/empresa. |
| Auditoria | Toda transmissao, rejeicao, autorizacao, download e regeneracao deve ser auditada. |
| Integridade | XML de envio e retorno nao devem ser perdidos apos autorizacao/rejeicao. |
| Idempotencia | Estrategia final de idempotencia de emissao e numeracao esta na MC. |
| Observabilidade | Erros de certificado, arquivo, comunicacao e rejeicao devem ser rastreaveis. |
| Retencao | Politica legal de retencao de XML/PDF deve ser definida na MC. |

## 18. Criterios de aceite

| Criterio | Resultado esperado |
|---|---|
| Modelo 55 | Toda NF-e de saida e emitida como modelo 55. |
| Pre-condicoes fiscais | Emissao bloqueia sem empresa, parametros, certificado e dados obrigatorios. |
| Autorizacao | Retorno autorizado grava chave, protocolo, XML, PDF/caminho e status. |
| Rejeicao | Retorno rejeitado grava motivo e permite nova transmissao quando estado permitir. |
| XML | XML de envio e retorno aparecem em nfe_simplificado_xml. |
| Itens | Todos os campos comprovados de item aparecem no dicionario. |
| Download | XML/PDF por chave/localizador retornam arquivo ou erro claro. |
| Sem invencao | Campo sem detalhe no material aparece como `Nao informado no material` ou item de MC. |

## 19. Itens para MC

| Item | Motivo |
|---|---|
| Contrato final de emissao completa | Material cita emissao completa, mas contrato funcional final precisa validacao. |
| Idempotencia de emissao | Material mostra bloqueio de documento autorizado, mas nao define chave idempotente final. |
| Concorrencia de numeracao | Material cita ultimo/proximo numero, mas nao define reserva transacional. |
| Politica legal de retencao XML/PDF | Caminhos e downloads existem, politica final nao informada. |
| Regras completas de e-mail da NF-e | Material cita e-mail como acao, mas nao detalha fluxo completo. |
| Validade de certificado | Material cita checagem/validade, mas politica final de bloqueio e alerta precisa decisao. |
| Regras completas de transporte e cobranca | Material cita volumes, peso, marca, duplicatas e fatura, mas detalhamento deve ser validado. |
