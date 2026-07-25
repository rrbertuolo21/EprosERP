# EF_NFSE_V1

## 1. Identificacao

| Campo | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | FATURAMENTO_FISCAL_ELETRONICO |
| Documento | Especificacao funcional - NFS-e |
| Versao | V1 |
| Status | Concluido |
| Data | 2026-06-07 |

## 2. Objetivo funcional

A NFS-e permite ao Epros emitir lote de nota fiscal de servico, consultar lote, consultar por RPS e cancelar NFS-e conforme dados de prestador, tomador, servico, valores, competencia, ambiente e parametros municipais disponiveis.

Esta EF organiza o conteudo comprovado para NFS-e no material canonico, preservando campos, obrigatoriedades, dominios indicados e lacunas sem completar regras municipais nao informadas.

## 3. Escopo

| Area | Incluso | Status |
|---|---|---|
| Configuracao NFS-e | Consulta de configuracao por municipio IBGE e provedor | Parcial |
| Emissao de lote | Numero do lote, modo sincrono, ambiente, natureza, regime, simples, incentivo, competencia, RPS, prestador, tomador e servico | Com conteudo |
| Consulta de lote | Numero do lote, protocolo, ambiente e prestador | Com conteudo |
| Consulta por RPS | Numero, serie, tipo, competencia, ambiente e prestador | Com conteudo |
| Cancelamento | Numero da NFS-e, codigo de cancelamento, motivo, ambiente e prestador | Com conteudo |
| Prestador | Documento, CRT, inscricao municipal, razao social, nome fantasia, municipio IBGE, UF, endereco, contato e certificado | Com conteudo |
| Tomador | Documento, inscricao municipal, razao social, nome fantasia, CRT, endereco e contato | Com conteudo |
| Servico | Item lista servico, CNAE, tributacao municipal, NBS, discriminacao, municipio, pais, exigibilidade ISS, municipio incidencia, processo e valores | Com conteudo |
| Valores | Servicos, deducoes, PIS, COFINS, INSS, IR, CSLL, retencoes, ISS, descontos, total tributos, retencoes e IBS/CBS | Com conteudo |
| Regras municipais completas | Parametrizacao por municipio/provedor, autenticacao por municipio, padroes de retorno e dominios finais | Incompleto |
| Persistencia final | Estruturas comprovadas sao de operacao; tabela final de historico NFS-e nao foi informada | Incompleto |

## 4. Fora de escopo

| Item | Motivo |
|---|---|
| Parametrizacao municipal exaustiva | Nao informado no material. |
| Cadastro completo de pessoa, endereco, municipio e empresa | Pertence a Cadastros Base. |
| Motor tributario completo de ISS, IBS/CBS, PIS/COFINS e retencoes | Possui EF especifica de motor tributario na fila macro. |
| Regras legais municipais especificas por prefeitura | Nao informado no material. |
| Armazenamento final de XML/PDF de NFS-e | Nao informado no material para NFS-e. |

## 5. Atores e responsabilidades

| Ator | Responsabilidade | Observacao |
|---|---|---|
| Usuario fiscal | Emitir, consultar e cancelar NFS-e conforme permissao. | Permissoes finais nao informadas no material. |
| Administrador Siser | Administrar configuracoes e suporte fiscal. | Parametros municipais finais estao na MC. |
| Epros | Validar dados, montar solicitacoes, comunicar servico fiscal e registrar retornos funcionais. | Persistencia final de NFS-e nao informada. |
| Prestador | Emitente do servico. | Identificado por documento e municipio IBGE. |
| Tomador | Destinatario do servico. | Identificado por documento. |

## 6. Conceitos funcionais

| Conceito | Definicao |
|---|---|
| NFS-e | Nota Fiscal de Servico Eletronica. |
| Lote NFS-e | Agrupamento operacional de emissao identificado por numero de lote. |
| RPS | Recibo Provisorio de Servico usado para consulta e emissao. |
| Prestador | Empresa emissora/prestadora do servico. |
| Tomador | Pessoa ou organizacao recebedora do servico. |
| Servico | Conteudo tributavel da NFS-e, com item de lista, municipio, discriminacao e valores. |
| Competencia | Data ou periodo fiscal relacionado ao servico. |
| Protocolo | Identificador de consulta de lote retornado/informado para acompanhamento. |
| Ambiente | Ambiente fiscal da operacao. Dominio final nao informado no material. |

## 7. Capacidades funcionais

| Capacidade | Descricao | Entrada principal | Saida esperada |
|---|---|---|---|
| Consultar configuracao NFS-e | Recupera municipio IBGE e provedor configurado para emissao municipal. | Empresa/prestador | Configuracao disponivel ou lacuna operacional. |
| Emitir lote NFS-e | Envia lote de NFS-e com RPS, prestador, tomador, servico e valores. | Estrutura de emissao de lote | Lote transmitido, retorno registrado e consulta possivel. |
| Consultar lote NFS-e | Consulta processamento por numero de lote e protocolo. | NumeroLote, Protocolo, Prestador | Status/resultado da consulta. |
| Consultar RPS | Consulta NFS-e por dados do RPS e competencia. | NumeroRps, Serie, Tipo, MesCompetencia, AnoCompetencia, Prestador | Resultado da consulta por RPS. |
| Cancelar NFS-e | Solicita cancelamento de NFS-e. | NumeroNfse, CodigoCancelamento, Motivo, Prestador | Cancelamento aceito, rejeitado ou pendente conforme retorno municipal. |
| Calcular ISS e retencoes | Calcula ISS e retencoes indicadas no material. | Valores do servico, regime, simples, limites informados | Valores calculados ou lacuna quando regra municipal nao existir. |

## 8. Regras funcionais

| Regra | Descricao | Contexto | Resultado esperado | Severidade | Fonte funcional |
|---|---|---|---|---|---|
| REG-NFSE-001 | A emissao de NFS-e deve ocorrer por lote. | Emissao | Exigir NumeroLote e estrutura de emissao. | Bloqueante | Material informa operacao emitir lote. |
| REG-NFSE-002 | NumeroLote e obrigatorio na emissao e na consulta de lote. | Emissao/consulta | Bloquear sem numero de lote. | Bloqueante | Campo obrigatorio informado. |
| REG-NFSE-003 | A emissao deve informar se o lote e sincrono. | Emissao | Registrar Sincrono como booleano obrigatorio. | Bloqueante | Campo obrigatorio informado. |
| REG-NFSE-004 | NaturezaOperacao, RegimeEspecialTributacao, OptanteSimplesNacional e IncentivoFiscal sao obrigatorios na emissao. | Emissao | Bloquear emissao sem esses campos. | Bloqueante | Campos obrigatorios informados. |
| REG-NFSE-005 | Competencia pode ser informada na emissao, mas nao e obrigatoria no material. | Emissao | Aceitar competencia nula quando permitido. | Media | Campo opcional informado. |
| REG-NFSE-006 | RPS e obrigatorio na emissao. | Emissao | Bloquear lote sem RPS. | Bloqueante | Campo obrigatorio informado. |
| REG-NFSE-007 | O RPS deve conter numero, serie e tipo obrigatorios. | RPS | Bloquear RPS incompleto. | Bloqueante | Campos obrigatorios informados. |
| REG-NFSE-008 | DataEmissao do RPS e opcional no material. | RPS | Permitir ausencia quando regra municipal permitir. | Media | Campo opcional informado. |
| REG-NFSE-009 | Prestador e obrigatorio na emissao, consulta de lote, consulta por RPS e cancelamento. | Operacoes NFS-e | Bloquear operacao sem prestador. | Bloqueante | Campo obrigatorio informado. |
| REG-NFSE-010 | Prestador deve possuir Documento, CRT e CodigoMunicipioIbge obrigatorios. | Prestador | Bloquear prestador sem esses campos. | Bloqueante | Campos obrigatorios informados. |
| REG-NFSE-011 | InscricaoMunicipal, RazaoSocial, NomeFantasia, UF, endereco, contato, caminho de certificado e senha do certificado do prestador sao opcionais no material. | Prestador | Nao tornar obrigatorio sem regra municipal. | Media | Campos opcionais informados. |
| REG-NFSE-012 | Tomador e obrigatorio na emissao. | Emissao | Bloquear lote sem tomador. | Bloqueante | Campo obrigatorio informado. |
| REG-NFSE-013 | Tomador deve possuir Documento e CRT obrigatorios. | Tomador | Bloquear tomador sem esses campos. | Bloqueante | Campos obrigatorios informados. |
| REG-NFSE-014 | InscricaoMunicipal, RazaoSocial, NomeFantasia, endereco e contato do tomador sao opcionais no material. | Tomador | Nao exigir sem regra municipal. | Media | Campos opcionais informados. |
| REG-NFSE-015 | Servico e obrigatorio na emissao. | Emissao | Bloquear lote sem servico. | Bloqueante | Campo obrigatorio informado. |
| REG-NFSE-016 | Servico deve possuir ItemListaServico, CodigoMunicipioIbge, CodigoPais, ExigibilidadeIss, MunicipioIncidencia e Valores obrigatorios. | Servico | Bloquear servico incompleto. | Bloqueante | Campos obrigatorios informados. |
| REG-NFSE-017 | CodigoCnae, CodigoTributacaoMunicipio, CodigoNbs, Discriminacao e NumeroProcesso sao opcionais no material. | Servico | Nao exigir sem regra municipal. | Media | Campos opcionais informados. |
| REG-NFSE-018 | Valores de NFS-e devem conter todos os campos monetarios obrigatorios informados no dicionario. | Valores | Bloquear ausencia dos valores obrigatorios. | Bloqueante | Campos obrigatorios informados. |
| REG-NFSE-019 | ValorServicos, ValorDeducoes, ValorPis, ValorCofins, ValorInss, ValorIr, ValorCsll, OutrasRetencoes, ValorIss, ValorIssRetido, Aliquota, DescontoIncondicionado, DescontoCondicionado, ValTotTributos, IssRetido e PisCofinsRetido sao obrigatorios. | Valores | Bloquear calculo/emissao sem os campos. | Bloqueante | Campos obrigatorios informados. |
| REG-NFSE-020 | IBS/CBS e total de tributos sao opcionais no material para valores da NFS-e. | Valores | Aceitar ausencia. | Media | Campos opcionais informados. |
| REG-NFSE-021 | Consulta de lote deve informar NumeroLote, Protocolo e Prestador. | Consulta de lote | Bloquear consulta incompleta. | Bloqueante | Campos obrigatorios informados. |
| REG-NFSE-022 | Consulta por RPS deve informar NumeroRps, Serie, Tipo, MesCompetencia, AnoCompetencia e Prestador. | Consulta RPS | Bloquear consulta incompleta. | Bloqueante | Campos obrigatorios informados. |
| REG-NFSE-023 | Cancelamento deve informar NumeroNfse, CodigoCancelamento e Prestador. | Cancelamento | Bloquear cancelamento incompleto. | Bloqueante | Campos obrigatorios informados. |
| REG-NFSE-024 | Motivo do cancelamento e opcional no material. | Cancelamento | Nao exigir motivo sem regra municipal. | Media | Campo opcional informado. |
| REG-NFSE-025 | Ambiente e opcional nas operacoes NFS-e conforme material. | Emissao/consulta/cancelamento | Usar ambiente quando informado; dominio final fica na MC. | Media | Campo opcional informado. |
| REG-NFSE-026 | Configuracao NFS-e deve expor municipio IBGE e provedor. | Configuracao | Permitir verificacao previa da emissao. | Alta | Material informa configuracao com municipio/provedor. |
| REG-NFSE-027 | O calculo de ISS da NFS-e deve existir como capacidade fiscal. | Calculo | Calcular ISS quando dados suficientes existirem. | Alta | Material informa calculo ISS NFS-e. |
| REG-NFSE-028 | Para optante pelo Simples Nacional, o material indica aliquota de ISS de 3.9%. | Calculo | Aplicar somente quando a regra de regime e municipio permitir. | Alta | Valor informado no material. |
| REG-NFSE-029 | Para regime normal, o material indica retencoes PIS, COFINS e IR com limites 215 e 666.80. | Calculo | Registrar como regra parcial ate completar matriz municipal. | Alta | Valores informados no material. |
| REG-NFSE-030 | Operacoes NFS-e sem autenticacao explicita devem ser tratadas como lacuna de seguranca. | Seguranca | Registrar na MC antes de implantacao definitiva. | Bloqueante | Material aponta ausencia de autenticacao explicita. |
| REG-NFSE-031 | Dados de contato possuem telefone e e-mail opcionais. | Prestador/tomador | Nao exigir sem regra municipal ou cadastro mestre. | Baixa | Campos opcionais informados. |
| REG-NFSE-032 | Endereco deve suportar codigo de municipio IBGE e codigo de pais obrigatorios quando endereco for usado. | Endereco | Exigir campos obrigatorios dentro da estrutura de endereco. | Alta | Campos obrigatorios informados. |
| REG-NFSE-033 | Tributos totais devem suportar percentuais federal, estadual, municipal e Simples Nacional opcional. | Total tributos | Registrar percentuais informados. | Media | Campos informados. |
| REG-NFSE-034 | IBS/CBS deve suportar finalidade, indicador final, codigo de indicador de operacao, indicador destinatario e valores quando informados. | IBS/CBS | Preservar dados quando enviados. | Media | Campos informados. |
| REG-NFSE-035 | PercentualRedutor em valores IBS/CBS e obrigatorio quando a estrutura de valores IBS/CBS for usada. | IBS/CBS | Bloquear estrutura incompleta. | Media | Campo obrigatorio informado. |
| REG-NFSE-036 | Situacao e classificacao tributaria IBS/CBS devem ser preservadas quando informadas. | IBS/CBS | Registrar codigos opcionais. | Media | Campos informados. |

## 9. Estados e situacoes

| Estado | Descricao | Origem |
|---|---|---|
| Configurada | Prestador possui municipio IBGE e provedor consultaveis. | Configuracao NFS-e. |
| Lote enviado | Lote de NFS-e foi submetido. | Emissao de lote. |
| Lote consultado | Numero de lote e protocolo foram usados em consulta. | Consulta de lote. |
| RPS consultado | Numero, serie, tipo e competencia foram usados em consulta. | Consulta por RPS. |
| Cancelamento solicitado | Numero da NFS-e e codigo de cancelamento foram enviados. | Cancelamento. |
| Rejeitado | Operacao nao aceita. | Dominio final de rejeicao nao informado no material. |
| Autorizado | Operacao aceita. | Dominio final de autorizacao nao informado no material. |

## 10. Modelo de dados funcional e implantavel

O material comprova estruturas operacionais completas para NFS-e, mas nao informa uma tabela fisica definitiva de historico. Para implantacao, a EF organiza as estruturas em registros funcionais persistiveis de operacao e componentes embarcados, mantendo como lacuna a decisao de tabela final, chaves fisicas e armazenamento XML/PDF.[^1]

| Entidade funcional | Finalidade | Cardinalidade | Persistencia indicada |
|---|---|---|---|
| nfse_operacao | Registrar a operacao de emissao, consulta ou cancelamento de NFS-e. | 1 por operacao | Consolidacao funcional; tabela final nao informada no material. |
| nfse_emitir_lote | Estrutura de emissao de lote. | 1 por emissao | Estrutura comprovada. |
| nfse_consultar_lote | Estrutura de consulta de lote. | 1 por consulta | Estrutura comprovada. |
| nfse_consultar_rps | Estrutura de consulta por RPS. | 1 por consulta | Estrutura comprovada. |
| nfse_cancelamento | Estrutura de cancelamento. | 1 por cancelamento | Estrutura comprovada. |
| nfse_rps | RPS vinculado a emissao ou consulta. | 1 por emissao/consulta RPS | Estrutura comprovada. |
| nfse_prestador | Prestador usado nas operacoes. | 1 por operacao | Estrutura comprovada. |
| nfse_tomador | Tomador usado na emissao. | 1 por emissao | Estrutura comprovada. |
| nfse_servico | Servico emitido. | 1 por emissao | Estrutura comprovada. |
| nfse_valores | Valores e retencoes da NFS-e. | 1 por servico | Estrutura comprovada. |
| nfse_endereco | Endereco de prestador/tomador. | 0..1 por prestador/tomador | Estrutura comprovada. |
| nfse_contato | Contato de prestador/tomador. | 0..1 por prestador/tomador | Estrutura comprovada. |
| nfse_ibscbs | Informacoes IBS/CBS opcionais. | 0..1 por valores | Estrutura comprovada. |
| nfse_ibscbs_valores | Valores de IBS/CBS opcionais. | 0..1 por IBS/CBS | Estrutura comprovada. |
| nfse_ibscbs_tributos | Tributos IBS/CBS opcionais. | 0..1 por valores IBS/CBS | Estrutura comprovada. |
| nfse_tributos_situacao_classificacao | Situacao e classificacao tributaria IBS/CBS. | 0..1 por tributos IBS/CBS | Estrutura comprovada. |
| nfse_total_tributos | Percentuais totais de tributos. | 0..1 por valores | Estrutura comprovada. |

### 10.1 Relacionamentos funcionais

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| nfse_operacao | referencia | nfse_emitir_lote | Quando TipoOperacao = Emissao. |
| nfse_operacao | referencia | nfse_consultar_lote | Quando TipoOperacao = ConsultaLote. |
| nfse_operacao | referencia | nfse_consultar_rps | Quando TipoOperacao = ConsultaRps. |
| nfse_operacao | referencia | nfse_cancelamento | Quando TipoOperacao = Cancelamento. |
| nfse_emitir_lote | possui | nfse_rps | Obrigatorio. |
| nfse_emitir_lote | possui | nfse_prestador | Obrigatorio. |
| nfse_emitir_lote | possui | nfse_tomador | Obrigatorio. |
| nfse_emitir_lote | possui | nfse_servico | Obrigatorio. |
| nfse_consultar_lote | possui | nfse_prestador | Obrigatorio. |
| nfse_consultar_rps | possui | nfse_prestador | Obrigatorio. |
| nfse_cancelamento | possui | nfse_prestador | Obrigatorio. |
| nfse_servico | possui | nfse_valores | Obrigatorio. |
| nfse_prestador | pode possuir | nfse_endereco | Opcional. |
| nfse_prestador | pode possuir | nfse_contato | Opcional. |
| nfse_tomador | pode possuir | nfse_endereco | Opcional. |
| nfse_tomador | pode possuir | nfse_contato | Opcional. |
| nfse_valores | pode possuir | nfse_ibscbs | Opcional. |
| nfse_valores | pode possuir | nfse_total_tributos | Opcional. |

## 11. Dicionario de dados implantavel

### 11.1 nfse_operacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno da operacao.[^1] |
| TipoOperacao | Enum/texto | Emissao, ConsultaLote, ConsultaRps, Cancelamento | Sim | Classificacao | Consolidado a partir das operacoes comprovadas.[^1] |
| Ambiente | Enum | Nao informado no material | Nao | Ambiente fiscal | Opcional nas estruturas comprovadas. |
| StatusOperacao | Enum/texto | Nao informado no material | Nao informado no material | Status | Dominio final nao informado no material. |
| MensagemRetorno | Texto | Nao informado no material | Nao | Retorno | Campo funcional necessario para registrar retorno; estrutura final nao informada.[^1] |
| DataOperacao | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Campo funcional necessario para historico; estrutura final nao informada.[^1] |

### 11.2 nfse_emitir_lote

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| NumeroLote | Numero inteiro | long | Sim | Identificador do lote | Obrigatorio. |
| Sincrono | Booleano | verdadeiro/falso | Sim | Modo de envio | Obrigatorio. |
| Ambiente | Enum | Nao informado no material | Nao | Ambiente fiscal | Opcional. |
| NaturezaOperacao | Numero inteiro | long | Sim | Regra fiscal | Dominio nao informado no material. |
| RegimeEspecialTributacao | Numero inteiro | long | Sim | Regra fiscal | Dominio nao informado no material. |
| OptanteSimplesNacional | Booleano | verdadeiro/falso | Sim | Regime | Obrigatorio. |
| IncentivoFiscal | Booleano | verdadeiro/falso | Sim | Regra fiscal | Obrigatorio. |
| Competencia | Data/hora | datetime | Nao | Competencia | Opcional. |
| Rps | Estrutura | nfse_rps | Sim | Relacao obrigatoria | RPS da emissao. |
| Prestador | Estrutura | nfse_prestador | Sim | Relacao obrigatoria | Prestador da emissao. |
| Tomador | Estrutura | nfse_tomador | Sim | Relacao obrigatoria | Tomador da emissao. |
| Servico | Estrutura | nfse_servico | Sim | Relacao obrigatoria | Servico da emissao. |

### 11.3 nfse_consultar_lote

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| NumeroLote | Numero inteiro | long | Sim | Identificador do lote | Obrigatorio. |
| Protocolo | Texto | Nao informado no material | Sim | Protocolo fiscal | Obrigatorio. |
| Ambiente | Enum | Nao informado no material | Nao | Ambiente fiscal | Opcional. |
| Prestador | Estrutura | nfse_prestador | Sim | Relacao obrigatoria | Prestador da consulta. |

### 11.4 nfse_consultar_rps

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| NumeroRps | Texto | Nao informado no material | Sim | Identificacao RPS | Obrigatorio. |
| Serie | Texto | Nao informado no material | Sim | Identificacao RPS | Obrigatorio. |
| Tipo | Numero inteiro | long | Sim | Tipo RPS | Dominio nao informado no material. |
| MesCompetencia | Numero inteiro | long | Sim | Competencia | Obrigatorio. |
| AnoCompetencia | Numero inteiro | long | Sim | Competencia | Obrigatorio. |
| Ambiente | Enum | Nao informado no material | Nao | Ambiente fiscal | Opcional. |
| Prestador | Estrutura | nfse_prestador | Sim | Relacao obrigatoria | Prestador da consulta. |

### 11.5 nfse_cancelamento

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| NumeroNfse | Texto | Nao informado no material | Sim | Identificacao NFS-e | Obrigatorio. |
| CodigoCancelamento | Texto | Nao informado no material | Sim | Motivo codificado | Obrigatorio. |
| Motivo | Texto | Nao informado no material | Nao | Justificativa | Opcional. |
| Ambiente | Enum | Nao informado no material | Nao | Ambiente fiscal | Opcional. |
| Prestador | Estrutura | nfse_prestador | Sim | Relacao obrigatoria | Prestador do cancelamento. |

### 11.6 nfse_rps

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Numero | Texto | Nao informado no material | Sim | Identificacao RPS | Obrigatorio. |
| Serie | Texto | Nao informado no material | Sim | Identificacao RPS | Obrigatorio. |
| Tipo | Numero inteiro | long | Sim | Tipo RPS | Dominio nao informado no material. |
| DataEmissao | Data/hora | datetime | Nao | Emissao RPS | Opcional. |

### 11.7 nfse_prestador

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Documento | Texto | Nao informado no material | Sim | Identificacao fiscal | Obrigatorio. |
| Crt | Numero inteiro | long | Sim | Regime fiscal | Dominio nao informado no material. |
| InscricaoMunicipal | Texto | Nao informado no material | Nao | Cadastro municipal | Opcional. |
| RazaoSocial | Texto | Nao informado no material | Nao | Cadastro | Opcional. |
| NomeFantasia | Texto | Nao informado no material | Nao | Cadastro | Opcional. |
| CodigoMunicipioIbge | Numero inteiro | long | Sim | Municipio | Obrigatorio. |
| Uf | Texto | Nao informado no material | Nao | UF | Opcional. |
| Endereco | Estrutura | nfse_endereco | Nao | Relacao opcional | Endereco do prestador. |
| Contato | Estrutura | nfse_contato | Nao | Relacao opcional | Contato do prestador. |
| CertificadoPath | Texto | Nao informado no material | Nao | Certificado | Caminho do certificado opcional no material. |
| CertificadoSenha | Texto secreto | Nao informado no material | Nao | Certificado | Senha opcional no material; regra de seguranca final fica na MC. |

### 11.8 nfse_tomador

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Documento | Texto | Nao informado no material | Sim | Identificacao fiscal | Obrigatorio. |
| InscricaoMunicipal | Texto | Nao informado no material | Nao | Cadastro municipal | Opcional. |
| RazaoSocial | Texto | Nao informado no material | Nao | Cadastro | Opcional. |
| NomeFantasia | Texto | Nao informado no material | Nao | Cadastro | Opcional. |
| Crt | Numero inteiro | long | Sim | Regime fiscal | Dominio nao informado no material. |
| Endereco | Estrutura | nfse_endereco | Nao | Relacao opcional | Endereco do tomador. |
| Contato | Estrutura | nfse_contato | Nao | Relacao opcional | Contato do tomador. |

### 11.9 nfse_servico

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| ItemListaServico | Texto | Nao informado no material | Sim | Classificacao servico | Obrigatorio. |
| CodigoCnae | Texto | Nao informado no material | Nao | CNAE | Opcional. |
| CodigoTributacaoMunicipio | Texto | Nao informado no material | Nao | Tributacao municipal | Opcional. |
| CodigoNbs | Texto | Nao informado no material | Nao | NBS | Opcional. |
| Discriminacao | Texto | Nao informado no material | Nao | Descricao | Opcional. |
| CodigoMunicipioIbge | Numero inteiro | long | Sim | Municipio | Obrigatorio. |
| CodigoPais | Numero inteiro | long | Sim | Pais | Obrigatorio. |
| ExigibilidadeIss | Numero inteiro | long | Sim | ISS | Dominio nao informado no material. |
| MunicipioIncidencia | Numero inteiro | long | Sim | Municipio incidencia | Obrigatorio. |
| NumeroProcesso | Texto | Nao informado no material | Nao | Processo | Opcional. |
| Valores | Estrutura | nfse_valores | Sim | Relacao obrigatoria | Valores do servico. |

### 11.10 nfse_valores

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| ValorServicos | Decimal | decimal | Sim | Valor | Obrigatorio. |
| ValorDeducoes | Decimal | decimal | Sim | Valor | Obrigatorio. |
| ValorPis | Decimal | decimal | Sim | Valor | Obrigatorio. |
| ValorCofins | Decimal | decimal | Sim | Valor | Obrigatorio. |
| ValorInss | Decimal | decimal | Sim | Valor | Obrigatorio. |
| ValorIr | Decimal | decimal | Sim | Valor | Obrigatorio. |
| ValorCsll | Decimal | decimal | Sim | Valor | Obrigatorio. |
| OutrasRetencoes | Decimal | decimal | Sim | Valor | Obrigatorio. |
| ValorIss | Decimal | decimal | Sim | Valor | Obrigatorio. |
| ValorIssRetido | Decimal | decimal | Sim | Valor | Obrigatorio. |
| Aliquota | Decimal | decimal | Sim | Aliquota | Obrigatorio. |
| DescontoIncondicionado | Decimal | decimal | Sim | Valor | Obrigatorio. |
| DescontoCondicionado | Decimal | decimal | Sim | Valor | Obrigatorio. |
| ValTotTributos | Decimal | decimal | Sim | Valor | Obrigatorio. |
| IssRetido | Numero inteiro | long | Sim | Indicador | Dominio nao informado no material. |
| PisCofinsRetido | Numero inteiro | long | Sim | Indicador | Dominio nao informado no material. |
| IBSCBS | Estrutura | nfse_ibscbs | Nao | Relacao opcional | Opcional. |
| TotTrib | Estrutura | nfse_total_tributos | Nao | Relacao opcional | Opcional. |

### 11.11 nfse_endereco

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Logradouro | Texto | Nao informado no material | Nao | Endereco | Opcional. |
| Numero | Texto | Nao informado no material | Nao | Endereco | Opcional. |
| Complemento | Texto | Nao informado no material | Nao | Endereco | Opcional. |
| Bairro | Texto | Nao informado no material | Nao | Endereco | Opcional. |
| CodigoMunicipioIbge | Numero inteiro | long | Sim | Municipio | Obrigatorio quando endereco for usado. |
| Uf | Texto | Nao informado no material | Nao | UF | Opcional. |
| Cep | Texto | Nao informado no material | Nao | CEP | Opcional. |
| CodigoPais | Numero inteiro | long | Sim | Pais | Obrigatorio quando endereco for usado. |

### 11.12 nfse_contato

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Telefone | Texto | Nao informado no material | Nao | Contato | Opcional. |
| Email | Texto | Nao informado no material | Nao | Contato | Opcional. |

### 11.13 nfse_ibscbs

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| FinalidadeNFSe | Texto | Nao informado no material | Nao | Finalidade | Opcional. |
| IndicadorFinal | Texto | Nao informado no material | Nao | Indicador | Opcional. |
| CodigoIndicadorOperacao | Texto | Nao informado no material | Nao | Indicador | Opcional. |
| IndicadorDestinatario | Texto | Nao informado no material | Nao | Indicador | Opcional. |
| Valores | Estrutura | nfse_ibscbs_valores | Nao | Relacao opcional | Opcional. |

### 11.14 nfse_ibscbs_valores

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| CodigoLocalidadeIncidencia | Texto | Nao informado no material | Nao | Localidade | Opcional. |
| PercentualRedutor | Decimal | decimal | Sim | Percentual | Obrigatorio quando a estrutura for usada. |
| Tributos | Estrutura | nfse_ibscbs_tributos | Nao | Relacao opcional | Opcional. |

### 11.15 nfse_ibscbs_tributos

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| SituacaoClassificacao | Estrutura | nfse_tributos_situacao_classificacao | Nao | Relacao opcional | Opcional. |

### 11.16 nfse_tributos_situacao_classificacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| CodigoSituacaoTributaria | Texto | Nao informado no material | Nao | Codigo fiscal | Opcional. |
| CodigoClassificacaoTributaria | Texto | Nao informado no material | Nao | Codigo fiscal | Opcional. |

### 11.17 nfse_total_tributos

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| PTotTribFed | Decimal | decimal | Sim | Percentual | Obrigatorio. |
| PTotTribEst | Decimal | decimal | Sim | Percentual | Obrigatorio. |
| PTotTribMun | Decimal | decimal | Sim | Percentual | Obrigatorio. |
| PTotTribSN | Decimal | decimal | Nao | Percentual | Opcional. |

## 12. Fluxos funcionais

### 12.1 Consultar configuracao NFS-e

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Solicita configuracao NFS-e. | Prestador/empresa | Pedido recebido. |
| 2 | Epros | Localiza municipio IBGE e provedor configurado. | Dados do prestador | Configuracao encontrada ou incompleta. |
| 3 | Epros | Retorna configuracao. | Municipio/provedor | Usuario sabe se ha base para emitir. |

### 12.2 Emitir lote NFS-e

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Inicia emissao de lote. | NumeroLote, Sincrono, NaturezaOperacao, regime, simples, incentivo, competencia | Lote em preparacao. |
| 2 | Epros | Valida RPS obrigatorio. | Numero, Serie, Tipo, DataEmissao | RPS aceito ou bloqueado. |
| 3 | Epros | Valida prestador. | Documento, CRT, CodigoMunicipioIbge e campos opcionais | Prestador aceito ou bloqueado. |
| 4 | Epros | Valida tomador. | Documento, CRT e campos opcionais | Tomador aceito ou bloqueado. |
| 5 | Epros | Valida servico. | ItemListaServico, municipio, pais, ISS, incidencia e valores | Servico aceito ou bloqueado. |
| 6 | Epros | Calcula ou recebe valores fiscais. | Valores obrigatorios, ISS, retencoes, IBS/CBS quando houver | Valores prontos para envio. |
| 7 | Epros | Transmite lote. | Estrutura completa | Retorno fiscal recebido. |
| 8 | Epros | Registra resultado funcional. | Retorno | Historico operacional disponivel.[^1] |

### 12.3 Consultar lote NFS-e

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Solicita consulta de lote. | NumeroLote, Protocolo, Ambiente, Prestador | Consulta iniciada. |
| 2 | Epros | Valida campos obrigatorios. | NumeroLote, Protocolo, Prestador | Consulta aceita ou bloqueada. |
| 3 | Epros | Consulta processamento. | Dados validados | Resultado retornado. |
| 4 | Epros | Registra resultado funcional. | Retorno | Historico consultavel.[^1] |

### 12.4 Consultar por RPS

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Solicita consulta por RPS. | NumeroRps, Serie, Tipo, MesCompetencia, AnoCompetencia, Prestador | Consulta iniciada. |
| 2 | Epros | Valida dados obrigatorios. | Dados do RPS e prestador | Consulta aceita ou bloqueada. |
| 3 | Epros | Consulta RPS. | Dados validados | Resultado retornado. |
| 4 | Epros | Registra resultado funcional. | Retorno | Historico consultavel.[^1] |

### 12.5 Cancelar NFS-e

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Solicita cancelamento. | NumeroNfse, CodigoCancelamento, Motivo, Ambiente, Prestador | Cancelamento em preparacao. |
| 2 | Epros | Valida numero, codigo e prestador. | Campos obrigatorios | Cancelamento aceito ou bloqueado. |
| 3 | Epros | Transmite cancelamento. | Estrutura validada | Retorno fiscal recebido. |
| 4 | Epros | Registra resultado funcional. | Retorno | Historico operacional atualizado.[^1] |

## 13. Validacoes e mensagens

| Codigo | Mensagem | Condicao |
|---|---|---|
| MSG-NFSE-001 | Numero do lote e obrigatorio. | Emissao ou consulta de lote sem NumeroLote. |
| MSG-NFSE-002 | Protocolo e obrigatorio para consultar lote. | Consulta de lote sem Protocolo. |
| MSG-NFSE-003 | RPS e obrigatorio para emitir NFS-e. | Emissao sem RPS. |
| MSG-NFSE-004 | Prestador e obrigatorio. | Operacao sem prestador. |
| MSG-NFSE-005 | Tomador e obrigatorio na emissao de NFS-e. | Emissao sem tomador. |
| MSG-NFSE-006 | Servico e obrigatorio na emissao de NFS-e. | Emissao sem servico. |
| MSG-NFSE-007 | Valores do servico sao obrigatorios. | Servico sem valores. |
| MSG-NFSE-008 | Numero da NFS-e e obrigatorio para cancelamento. | Cancelamento sem NumeroNfse. |
| MSG-NFSE-009 | Codigo de cancelamento e obrigatorio. | Cancelamento sem CodigoCancelamento. |
| MSG-NFSE-010 | Mes e ano de competencia sao obrigatorios para consulta por RPS. | Consulta por RPS incompleta. |
| MSG-NFSE-011 | Configuracao municipal de NFS-e incompleta. | Municipio/provedor ausente. |
| MSG-NFSE-012 | Autenticacao da operacao NFS-e precisa ser definida. | Operacao sem regra de seguranca final. |

## 14. Integracoes

| Integracao | Direcao | Dados | Regra | Lacuna |
|---|---|---|---|---|
| Cadastros Base | Entrada | Prestador, tomador, endereco, municipio IBGE, pais, contato | Dados mestres nao devem ser duplicados. | Obrigatoriedade municipal final. |
| Parametros fiscais | Entrada | Ambiente, certificado, configuracao municipal, provedor | Usado antes de emitir/consultar/cancelar. | Matriz municipal completa. |
| Motor tributario | Entrada/Saida | ISS, PIS, COFINS, IR, IBS/CBS, totais de tributos | Calculo deve respeitar material e lacunas municipais. | Motor detalhado em EF propria. |
| Vendas/servicos | Entrada | Dados comerciais do servico | Pode originar emissao de NFS-e. | Contrato final nao informado no material. |
| Financeiro | Saida | Efeitos de faturamento/recebiveis | Nao informado no material para NFS-e. | Definir integracao. |

## 15. Permissoes e seguranca

| Controle | Regra |
|---|---|
| Autenticacao | O material indica operacoes NFS-e sem autenticacao explicita; a definicao final e lacuna P0 na MC. |
| Autorizacao | Permissoes por papel para emitir, consultar e cancelar NFS-e nao informadas no material. |
| Certificado | Prestador pode possuir caminho e senha de certificado; armazenamento seguro e obrigatoriedade final ficam na MC. |
| Dados pessoais | Documento, razao social, endereco, telefone e e-mail devem seguir politicas corporativas de privacidade. |
| Auditoria | Historico de operacao e retorno e necessario para operacao fiscal; estrutura final nao informada no material.[^1] |

## 16. Relatorios e consultas

| Consulta | Filtros comprovados | Resultado |
|---|---|---|
| Configuracao NFS-e | Prestador/empresa | Municipio IBGE e provedor. |
| Consulta de lote | NumeroLote, Protocolo, Ambiente, Prestador | Resultado do lote. |
| Consulta por RPS | NumeroRps, Serie, Tipo, MesCompetencia, AnoCompetencia, Ambiente, Prestador | Resultado por RPS. |
| Cancelamento | NumeroNfse, CodigoCancelamento, Ambiente, Prestador | Resultado do cancelamento. |

## 17. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-NFSE-001 | Deve ser possivel montar emissao de lote com todos os campos obrigatorios de `nfse_emitir_lote`. |
| CA-NFSE-002 | Emissao sem NumeroLote deve ser bloqueada. |
| CA-NFSE-003 | Emissao sem RPS, prestador, tomador ou servico deve ser bloqueada. |
| CA-NFSE-004 | Prestador sem Documento, CRT ou CodigoMunicipioIbge deve ser bloqueado. |
| CA-NFSE-005 | Tomador sem Documento ou CRT deve ser bloqueado. |
| CA-NFSE-006 | Servico sem ItemListaServico, municipio, pais, exigibilidade ISS, municipio incidencia ou valores deve ser bloqueado. |
| CA-NFSE-007 | Valores sem campos monetarios obrigatorios devem bloquear emissao. |
| CA-NFSE-008 | Consulta de lote deve exigir NumeroLote, Protocolo e Prestador. |
| CA-NFSE-009 | Consulta por RPS deve exigir NumeroRps, Serie, Tipo, MesCompetencia, AnoCompetencia e Prestador. |
| CA-NFSE-010 | Cancelamento deve exigir NumeroNfse, CodigoCancelamento e Prestador. |
| CA-NFSE-011 | Configuracao NFS-e deve expor municipio IBGE e provedor quando existentes. |
| CA-NFSE-012 | Ausencia de autenticacao/seguranca final deve permanecer registrada na MC antes da implantacao. |

## 18. Lacunas encaminhadas para MC

| Lacuna | Impacto |
|---|---|
| Parametrizacao municipal completa por municipio/provedor | Pode impedir emissao em municipios diferentes. |
| Dominios de ambiente, natureza, regime especial, tipo RPS, exigibilidade ISS e indicadores de retencao | Necessario para validacao implantavel. |
| Persistencia final de NFS-e, XML, PDF, protocolo, status e retorno municipal | Necessario para historico fiscal. |
| Autenticacao e autorizacao das operacoes NFS-e | Risco fiscal e operacional. |
| Contrato final com financeiro/vendas/servicos | Necessario para faturamento integrado. |
| Armazenamento seguro de certificado e senha | Necessario para seguranca. |
| Regras municipais de cancelamento | Necessario para aceitar/rejeitar por prefeitura. |
| Regras completas de ISS e retencoes | Necessario para calculo fiscal confiavel. |

## 19. Proximo passo

O proximo documento especifico da fila macro e `EF_CTE`, detalhando CT-e conforme material disponivel.

[^1]: Consolidacao funcional criada para tornar implantavel a especificacao, pois o material comprova estruturas de operacao NFS-e, mas nao informa tabela final, chaves fisicas, historico, armazenamento de retorno, XML ou PDF para NFS-e.
