# Especificacao Funcional - Epros

**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Capacidade:** NFCE_PDV  
**Versao:** V1  
**Empresa:** Siser  
**Status:** Concluido para validacao humana  

## 1. Controle do documento

| Item | Conteudo |
|---|---|
| Responsavel pela elaboracao | Analise funcional assistida |
| Responsavel pela validacao funcional | Siser |
| Responsavel pela validacao tecnica | Siser |
| Area dona do processo | Fiscal, Vendas, PDV, Caixa, Plataforma |
| Publico-alvo | Produto, negocio, implantacao, desenvolvimento, suporte, operacao fiscal e PDV |
| Fonte de verdade | Esta EF descreve a NFC-e/PDV do Epros |

## 2. Objetivo funcional

NFC-e/PDV existe para emitir, transmitir, registrar, imprimir e consultar Nota Fiscal de Consumidor Eletronica modelo 65 a partir de venda presencial, PDV ou retaguarda, preservando XML, DANFCE/PDF, status fiscal, chave, protocolo, itens e vinculo operacional com a venda.

O processo tambem deve bloquear edicao ou exclusao da venda POS quando a NFC-e ja tiver numero fiscal emitido, preservar arquivos fiscais, permitir cancelamento relacionado e usar parametros especificos de NFC-e como CSC, ID CSC e configuracao de impressao.

## 3. Escopo funcional

### 3.1 Dentro do escopo

| Capacidade | Descricao | Observacao |
|---|---|---|
| Emissao NFC-e modelo 65 | Emite NFC-e fiscal para venda presencial/PDV. | Modelo 65 comprovado no material. |
| Emissao por PDV | Finalizacao de venda POS transmite NFC-e. | Material cita transmissao AJAX do POS. |
| Emissao por retaguarda | NFC-e pode ser transmitida fora do caixa quando houver venda/documento. | Detalhe de tela final nao informado no material. |
| CSC e ID CSC | Usa CSC e ID CSC da empresa para emissao/impressao. | Obrigatorios em producao conforme parametros fiscais. |
| Autorizacao e rejeicao | Documento novo/rejeitado pode transmitir; sucesso aprova; falha rejeita. | Fluxo igual ao documento fiscal de saida. |
| XML NFC-e | Armazena XML de envio e retorno. | Tabela propria de XML. |
| DANFCE/impressao | Renderiza/imprime DANFCE e documento nao fiscal quando aplicavel. | Detalhe de layout pertence tambem a configuracao de impressao. |
| Listagem | Lista documentos aprovados/cancelados com numero fiscal. | Filtro por numero maior que zero e estado. |
| Bloqueio pos-emissao | Venda POS com numero NFC-e maior que zero nao pode editar/excluir. | Regra comprovada no material. |
| Cancelamento relacionado | NFC-e possui cancelamento com XML/PDF/status. | Detalhamento completo em EF de cancelamento. |
| Configuracao de impressao NFC-e | Usa layout, margens, modo, QR Code e segunda via contingencia por empresa. | Detalhada parcialmente no material. |

### 3.2 Fora do escopo

| Item | Tratamento |
|---|---|
| NF-e modelo 55 | Possui EF especifica. |
| Cancelamento completo | Possui EF especifica; esta EF referencia o cancelamento. |
| Inutilizacao | Possui EF especifica. |
| Motor tributario completo | Possui EF especifica; esta EF preserva validacoes NFC-e comprovadas. |
| Operacao completa de caixa | Pertence ao modulo/fluxo de Vendas/PDV; esta EF cobre emissao fiscal. |
| Configuracao fiscal geral da empresa | Possui EF especifica de parametros fiscais. |

## 4. Glossario funcional

| Termo | Definicao | Observacao |
|---|---|---|
| NFC-e | Nota Fiscal de Consumidor Eletronica. | Modelo fiscal 65. |
| PDV | Ponto de venda que finaliza a venda e aciona emissao fiscal. | Regras comerciais pertencem ao fluxo dono. |
| DANFCE | Documento auxiliar da NFC-e. | Pode ser impresso em formato fiscal. |
| CSC | Codigo de seguranca do contribuinte para NFC-e. | Usado na emissao/impressao. |
| ID CSC | Identificador do CSC. | Usado junto com CSC. |
| QR Code | Dado visual da NFC-e em impressao. | Configuracao possui layout e versao. |
| Numero NFC-e | Numero fiscal emitido. | Se maior que zero, bloqueia edicao/exclusao da venda POS. |

## 5. Atores, papeis e responsabilidades

| Ator/Papel | Responsabilidade | Permissoes esperadas | Restricoes |
|---|---|---|---|
| Operador de caixa | Finalizar venda e emitir NFC-e. | Emitir e imprimir NFC-e da propria operacao. | Nao altera parametros fiscais. |
| Supervisor de caixa | Apoiar rejeicoes, reimpressao e cancelamento quando permitido. | Consultar, imprimir, cancelar conforme permissao. | Acoes fiscais devem ser auditadas. |
| Operador fiscal | Consultar, baixar XML/PDF e acompanhar rejeicoes. | Consulta, download e retransmissao quando permitida. | Nao edita venda fiscal autorizada fora do fluxo. |
| Gestor fiscal | Parametrizar CSC, ambiente, impressao e regras fiscais. | Manter parametros e validar inconsistencias. | Deve respeitar empresa/tenant. |
| Integracao PDV | Enviar venda para emissao. | Contrato autenticado. | Deve respeitar tenant, empresa e idempotencia. |

## 6. Pre-condicoes

| Pre-condicao | Regra |
|---|---|
| Empresa parametrizada para NFC-e | Ambiente NFC-e, serie, proximo numero, CSC e ID CSC devem estar validos quando producao. |
| Certificado disponivel | Emissao/transmissao exige certificado valido quando aplicavel. |
| Venda existente | NFC-e deve estar vinculada a uma venda/operacao de origem quando emitida pelo PDV. |
| Itens validos | Itens devem possuir dados fiscais minimos como produto, NCM, CFOP, CST/CSOSN e valores. |
| Configuracao de impressao | Necessaria para DANFCE quando a operacao imprimir documento fiscal. |
| Tenant e empresa | Documento deve estar isolado por tenant e empresa. |

## 7. Visao operacional

1. Operador finaliza venda no PDV ou usuario autorizado solicita emissao.
2. Epros valida empresa, parametros NFC-e, CSC, ID CSC, certificado e itens.
3. Epros monta XML de envio e transmite NFC-e modelo 65.
4. Se autorizada, Epros grava chave, numero, protocolo, XML, PDF/caminho, status e data de emissao.
5. Se rejeitada, Epros grava status rejeitado e motivo da rejeicao.
6. Se numero NFC-e ficar maior que zero, a venda POS fica bloqueada para edicao/exclusao.
7. Epros permite impressao fiscal/DANFCE, impressao nao fiscal quando aplicavel, listagem e consulta.
8. Cancelamento relacionado atualiza status e preserva XML/PDF do evento quando autorizado.

## 8. Capacidades funcionais detalhadas

### 8.1 Emitir NFC-e pelo PDV

| Item | Especificacao |
|---|---|
| Objetivo | Transmitir NFC-e modelo 65 a partir da venda finalizada no PDV. |
| Acionamento | Finalizacao de venda POS ou comando de transmissao de NFC-e. |
| Pre-condicoes | Venda existente, parametros NFC-e validos, CSC/ID CSC, certificado e itens validos. |
| Dados de entrada | Venda, emitente, destinatario quando informado, itens, pagamentos, totais, ambiente, serie, numero, CSC, ID CSC e localizador externo. |
| Processamento | Validar dados, montar XML, transmitir, registrar retorno e bloquear edicao pos-emissao. |
| Resultado esperado | NFC-e autorizada, rejeitada ou com erro funcional. |
| Pos-condicoes | XML/PDF disponiveis; venda POS bloqueada quando numero NFC-e > 0. |
| Excecoes | Certificado ausente, CSC/ID CSC ausentes, rejeicao fiscal, item invalido, arquivo nao encontrado. |
| Auditoria | Usuario/caixa, venda, data/hora, chave, numero, status, protocolo e motivo. |

### 8.2 Imprimir DANFCE

| Item | Especificacao |
|---|---|
| Objetivo | Imprimir ou renderizar documento auxiliar da NFC-e. |
| Acionamento | Usuario solicita impressao, preview ou reimpressao. |
| Pre-condicoes | NFC-e existente e configuracao de impressao disponivel. |
| Dados de entrada | Chave, XML de retorno/autorizacao, CSC, ID CSC, nome de impressora quando informado e configuracao de layout. |
| Processamento | Validar XML, aplicar configuracao de impressao e gerar saida de impressao. |
| Resultado esperado | DANFCE impresso/renderizado ou erro funcional. |
| Excecoes | XML de retorno ausente, impressora/configuracao ausente, documento nao localizado. |
| Auditoria | Usuario, data/hora, documento e tipo de impressao. |

### 8.3 Listar NFC-e emitidas

| Item | Especificacao |
|---|---|
| Objetivo | Consultar NFC-e aprovadas/canceladas. |
| Acionamento | Usuario acessa listagem. |
| Pre-condicoes | Permissao de consulta e empresa/tenant identificados. |
| Dados de entrada | Periodo, status, numero e filtros funcionais quando existirem. |
| Processamento | Filtrar documentos com numero NFC-e maior que zero e estado fiscal. |
| Resultado esperado | Lista de documentos fiscais consultaveis. |
| Excecoes | Permissao ausente, empresa nao encontrada ou documento nao localizado. |
| Auditoria | Usuario, filtros e data/hora da consulta. |

### 8.4 Bloquear edicao da venda POS apos emissao

| Item | Especificacao |
|---|---|
| Objetivo | Impedir alteracao de venda que ja possui NFC-e emitida. |
| Acionamento | Usuario tenta editar ou excluir venda POS. |
| Pre-condicoes | Venda possui numero NFC-e maior que zero. |
| Dados de entrada | Venda e numero NFC-e. |
| Processamento | Validar numero fiscal e bloquear acao. |
| Resultado esperado | Edicao/exclusao bloqueada. |
| Excecoes | Nao informado no material. |
| Auditoria | Tentativa de edicao/exclusao, usuario, venda e data/hora. |

## 9. Regras de negocio

| Regra | Descricao | Condicao | Resultado | Severidade | Observacoes |
|---|---|---|---|---|---|
| REG-NFCE-001 | NFC-e usa modelo fiscal 65. | Emissao NFC-e. | Gerar documento modelo 65. | Bloqueante |  |
| REG-NFCE-002 | CSC e ID CSC da empresa devem estar disponiveis para NFC-e. | Emissao/impressao NFC-e. | Bloquear se ausentes quando exigidos. | Bloqueante |  |
| REG-NFCE-003 | Documento novo ou rejeitado pode ser transmitido. | Estado novo/rejeitado. | Permitir transmissao. | Bloqueante |  |
| REG-NFCE-004 | Sucesso de transmissao deve atualizar chave, numero e status autorizado/aprovado. | Retorno autorizado. | Persistir dados fiscais. | Bloqueante |  |
| REG-NFCE-005 | Falha fiscal deve atualizar status rejeitado. | Retorno rejeitado. | Persistir motivo de rejeicao. | Bloqueante |  |
| REG-NFCE-006 | Cancelamento de NFC-e deve alterar status para cancelado quando autorizado. | Evento de cancelamento. | Registrar status e arquivos. | Bloqueante | Detalhe em EF de cancelamento. |
| REG-NFCE-007 | Listagem deve considerar NFC-e com numero maior que zero e estado fiscal. | Consulta lista. | Retornar aprovadas/canceladas conforme filtro. | Media |  |
| REG-NFCE-008 | Venda POS com numero NFC-e maior que zero nao pode ser editada. | Tentativa de edicao. | Bloquear edicao. | Bloqueante |  |
| REG-NFCE-009 | Venda POS com numero NFC-e maior que zero nao pode ser excluida. | Tentativa de exclusao. | Bloquear exclusao. | Bloqueante |  |
| REG-NFCE-010 | Impressao NFC-e autorizada deve usar XML de retorno/autorizacao. | Impressao fiscal. | Imprimir DANFCE ou retornar erro. | Bloqueante | Material cita erro quando XML retorno ausente. |
| REG-NFCE-011 | Impressao NFC-e deve usar CSC, ID CSC e impressora/configuracao quando informados. | Impressao fiscal. | Gerar saida de impressao. | Media |  |
| REG-NFCE-012 | Configuracao de impressao NFC-e deve ser unica por empresa. | Cadastro de layout. | Bloquear duplicidade. | Bloqueante |  |
| REG-NFCE-013 | Configuracao de impressao NFC-e exige EmpresaId. | Cadastro/alteracao de layout. | Bloquear salvamento. | Bloqueante |  |
| REG-NFCE-014 | CFOP de NFC-e deve pertencer ao dominio permitido no material. | Validacao de item. | Bloquear emissao se invalido. | Bloqueante | Dominio: 5101, 5102, 5103, 5104, 5115, 5405, 5653, 5656, 5667, 5933. |
| REG-NFCE-015 | CSOSN de NFC-e deve pertencer ao dominio permitido no material. | Validacao de item. | Bloquear emissao se invalido. | Bloqueante | Dominio: 102, 103, 300, 400, 500, 900, 02, 15, 53, 61. |
| REG-NFCE-016 | CST ICMS de NFC-e deve pertencer ao dominio permitido no material. | Validacao de item. | Bloquear emissao se invalido. | Bloqueante | Dominio: 00, 20, 40, 41, 60, 90, 02, 15, 53, 61. |
| REG-NFCE-017 | Combinacoes CFOP x CSOSN e CFOP x CST de NFC-e devem respeitar matriz fiscal. | Validacao de item. | Bloquear emissao se combinacao invalida. | Bloqueante | Matriz completa fica na MC/motor tributario. |
| REG-NFCE-018 | NFC-e com frete exige destinatario com endereco. | Venda NFC-e com frete. | Bloquear emissao sem destinatario/endereco. | Bloqueante |  |
| REG-NFCE-019 | Valor de frete deve ser maior ou igual a zero. | Item/transporte. | Bloquear valor negativo. | Bloqueante |  |
| REG-NFCE-020 | XML de envio deve ser preservado. | Emissao NFC-e. | Gravar XML. | Bloqueante |  |
| REG-NFCE-021 | XML de retorno deve ser preservado quando houver retorno. | Autorizacao/rejeicao. | Gravar XML retorno. | Bloqueante |  |

## 10. Parametros de configuracao

| Parametro | Finalidade | Tipo/formato | Valor padrao | Obrigatorio | Nivel | Quem pode alterar | Impacto |
|---|---|---|---|---|---|---|---|
| Ambiente NFC-e | Define ambiente de emissao NFC-e. | Enum | Nao informado no material | Sim | Empresa | Gestor fiscal | Direciona transmissao. |
| Serie NFC-e | Define serie fiscal. | Numero | Nao informado no material | Sim/condicional | Empresa | Gestor fiscal | Controla numeracao. |
| Proximo numero NFC-e | Define proximo numero fiscal. | Numero | Nao informado no material | Sim/condicional | Empresa | Gestor fiscal | Controla numeracao. |
| CSC NFC-e | Codigo de seguranca. | Texto | Nao informado no material | Condicional | Empresa | Gestor fiscal | Necessario para NFC-e. |
| ID CSC NFC-e | Identificador do CSC. | Texto | Nao informado no material | Condicional | Empresa | Gestor fiscal | Necessario para NFC-e. |
| Nome impressora | Define impressora para NFC-e quando usado. | Texto | Nao informado no material | Nao informado no material | Empresa | Gestor fiscal | Impacta impressao. |
| Configuracao impressao NFC-e | Layout/margens/QR Code/segunda via. | Parametros visuais | Nao informado no material | Condicional | Empresa | Gestor fiscal | Impacta DANFCE. |
| Certificado digital | Habilita transmissao fiscal. | Arquivo/senha | Nao informado no material | Sim para transmissao | Empresa | Gestor fiscal | Sem certificado nao ha transmissao. |

## 11. Modelo de dados funcional e implantavel

### 11.1 Entidades/tabelas

| Entidade/tabela | Papel funcional | Conteudo |
|---|---|---|
| nfce_simplificado | Documento fiscal NFC-e. | Com conteudo completo para campos extraidos. |
| nfce_simplificado_item | Itens da NFC-e. | Com conteudo completo para campos extraidos. |
| nfce_simplificado_xml | XML de envio e retorno da NFC-e. | Com conteudo completo para campos extraidos. |
| nfce_simplificado_cancelamento | Cancelamento relacionado a NFC-e. | Referenciado nesta EF; detalhe em EF de cancelamento. |
| configuracao_impressao_nfce | Layout de impressao NFC-e por empresa. | Com conteudo parcial de mapeamento. |

### 11.2 Relacionamentos

| Origem | Relacionamento | Destino | Cardinalidade | Obrigatorio | Regra de integridade |
|---|---|---|---|---|---|
| nfce_simplificado | possui | nfce_simplificado_item | 1:N | Sim | NFC-e deve possuir itens quando emitida. |
| nfce_simplificado | possui | nfce_simplificado_xml | 1:1 | Sim | XML de envio deve ser preservado. |
| nfce_simplificado | possui | nfce_simplificado_cancelamento | 1:1 | Condicional | Criado quando cancelamento for autorizado. |
| nfce_simplificado | referencia | localizador externo | Nao informado no material | Condicional | Vincula a venda/PDV. |
| configuracao_impressao_nfce | pertence a | empresa | 1:1 | Sim | Uma configuracao por empresa. |

### 11.3 Dicionario de dados implantavel - nfce_simplificado

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| Id | Identificador | Nao informado no material | Nao informado no material | PK | Nao informado no material. |
| TenantId | Texto | varchar(200) | Sim | Tenant | Isolamento do documento. |
| NfceSimplificadoXmlId | Identificador | Nao informado no material | Nao | FK XML | Relacao com XML. |
| NfceSimplificadoCancelamentoId | Identificador | Nao informado no material | Nao | FK cancelamento | Preenchido quando cancelada. |
| Ambiente | Numero/enum | Nao informado no material | Sim | Atributo | Ambiente fiscal. |
| Crt | Numero/enum | Nao informado no material | Sim | Atributo | CRT/regime emitente. |
| DocumentoEmitente | Texto/documento | varchar(20) | Sim | Empresa emitente | CPF/CNPJ emitente. |
| DocumentoDestinatario | Texto/documento | varchar(20) | Nao | Pessoa destinataria | CPF/CNPJ quando informado. |
| Uf | Texto | varchar(2) | Sim | UF | UF do documento. |
| Chave | Texto | varchar(50) | Nao | Chave fiscal | Chave NFC-e quando gerada/autorizada. |
| Recibo | Texto | varchar(50) | Nao | Retorno fiscal | Recibo quando informado. |
| Protocolo | Texto | varchar(50) | Nao | Retorno fiscal | Protocolo quando informado. |
| Serie | Numero | Nao informado no material | Sim | Numeracao | Serie fiscal. |
| Numero | Numero | Nao informado no material | Sim | Numeracao | Numero fiscal. |
| Status | Enum | Nao informado no material | Sim | Estado | Status fiscal no Epros. |
| StatusSefaz | Numero | Nao informado no material | Sim | Retorno fiscal | Codigo retornado. |
| MotivoRejeicaoSefaz | Texto | nvarchar(max) | Nao | Retorno fiscal | Motivo da rejeicao. |
| Total | Decimal | decimal(18,2) | Sim | Valor | Total da NFC-e. |
| PdfCaminho | Texto | varchar(500) | Nao | Arquivo | Caminho DANFCE/PDF. |
| XmlCaminho | Texto | varchar(500) | Nao | Arquivo | Caminho XML. |
| JsonRecebido | Texto/JSON | nvarchar(max) | Sim | Payload | Dados recebidos para emissao. |
| DataEmissao | Data/hora | Nao informado no material | Nao | Data fiscal | Data da emissao. |
| CscId | Texto | varchar(6) | Nao | NFC-e | ID CSC usado na NFC-e. |
| Csc | Texto | varchar(40) | Nao | NFC-e | CSC usado na NFC-e. |
| LocalizadorExternoId | Texto | varchar(300) | Nao | Vinculo externo | Relaciona NFC-e a venda/PDV. |
| TipoNFe | Enum | Nao informado no material | Sim | Tipo fiscal | Tipo da NF-e/NFC-e. |

### 11.4 Dicionario de dados implantavel - nfce_simplificado_item

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| Id | Identificador | long | Sim | PK | Identificador do item. |
| TenantId | Texto | varchar(200) | Sim | Tenant | Isolamento do item. |
| NfceSimplificadoId | Identificador | long | Sim | FK NFC-e | Vincula item ao documento. |
| CodigoProduto | Texto | varchar(60) | Sim | Produto | Codigo do produto. |
| NomeProduto | Texto | varchar(120) | Sim | Produto | Nome do produto. |
| CodigoBarras | Texto | varchar(20) | Sim | Produto | Codigo de barras. |
| Ncm | Texto | varchar(50) | Sim | NCM | NCM do item. |
| Cfop | Numero/codigo | Nao informado no material | Sim | CFOP | CFOP do item. |
| Unidade | Texto | varchar(50) | Sim | Unidade | Unidade do item. |
| ValorUnitario | Decimal | decimal(21,10) | Sim | Valor | Valor unitario. |
| Quantidade | Decimal | decimal(15,4) | Sim | Quantidade | Quantidade. |
| Origem | Texto | varchar(5) | Sim | Tributacao | Origem da mercadoria. |
| Csosn | Texto | varchar(5) | Nao | Tributacao | CSOSN quando aplicavel. |
| CstIcms | Texto | varchar(5) | Nao | Tributacao | CST ICMS quando aplicavel. |
| ValorAliquotaIcms | Decimal | decimal(18,3) | Sim | Tributacao | Aliquota ICMS. |
| ValorReducaoIcmsPercentual | Decimal | decimal(18,2) | Sim | Tributacao | Percentual reducao ICMS. |
| TipoReducaoIcms | Enum | Nao informado no material | Sim | Tributacao | Tipo reducao ICMS. |
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
| ValorDesconto | Decimal | decimal(18,2) | Sim | Valor | Desconto. |
| ValorDescontoRateado | Decimal | decimal(18,2) | Sim | Rateio | Desconto rateado. |
| ValorFreteRateado | Decimal | decimal(18,2) | Sim | Rateio | Frete rateado. |
| ValorSeguroRateado | Decimal | decimal(18,2) | Sim | Rateio | Seguro rateado. |
| ValorAcrescimoRateado | Decimal | decimal(18,2) | Sim | Rateio | Acrescimo rateado. |
| ValorOutroRateado | Decimal | decimal(18,2) | Sim | Rateio | Outros valores rateados. |
| Deletado | Data/hora | Nao informado no material | Nao | Exclusao logica | Data de exclusao logica quando houver. |

### 11.5 Dicionario de dados implantavel - nfce_simplificado_xml

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| Id | Identificador | Nao informado no material | Nao informado no material | PK | Nao informado no material. |
| TenantId | Texto | varchar(200) | Sim | Tenant | Isolamento do XML. |
| XmlEnvio | Texto/XML | nvarchar(max) | Sim | XML | XML enviado. |
| XmlRetorno | Texto/XML | nvarchar(max) | Nao | XML | XML retornado/autorizado quando houver. |
| NfceSimplificadoId | Identificador | Nao informado no material | Sim | FK NFC-e | Relacao com NFC-e. |

### 11.6 Dicionario de dados implantavel - configuracao_impressao_nfce

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| Id | Identificador | Nao informado no material | Sim quando aplicavel | PK | Material informa validacao de Id no contrato. |
| EmpresaId | Identificador | Nao informado no material | Sim | FK empresa | Uma configuracao por empresa. |
| TenantId | Texto | varchar(200) | Nao informado no material | Tenant | Tenant da configuracao. |
| DetalheVendaNormal | Nao informado no material | Nao informado no material | Nao informado no material | Atributo | Detalhe de venda normal. |
| DetalheVendaContingencia | Nao informado no material | Nao informado no material | Nao informado no material | Atributo | Detalhe de contingencia. |
| ImprimeDescontoItem | Booleano | true/false | Nao informado no material | Atributo | Imprime desconto do item. |
| ImprimeFoneEmitente | Booleano | true/false | Nao informado no material | Atributo | Imprime telefone emitente. |
| MargemEsquerda | Numero real | real | Nao informado no material | Atributo | Margem esquerda. |
| MargemDireita | Numero real | real | Nao informado no material | Atributo | Margem direita. |
| ModoImpressao | Nao informado no material | Nao informado no material | Nao informado no material | Atributo | Modo de impressao. |
| NfceLayoutQrCode | Nao informado no material | Nao informado no material | Nao informado no material | Atributo | Layout QR Code. |
| VersaoQrCode | Nao informado no material | Nao informado no material | Nao informado no material | Atributo | Versao QR Code. |
| SegundaViaContingencia | Booleano | true/false | Nao informado no material | Atributo | Segunda via em contingencia. |

## 12. Estados

| Estado | Significado | Acoes permitidas |
|---|---|---|
| Novo | Venda/documento ainda nao transmitido. | Transmitir e gerar preview quando aplicavel. |
| Rejeitado | Transmissao recusada. | Corrigir e retransmitir. |
| Autorizado | NFC-e autorizada/aprovada. | Imprimir, baixar XML/PDF, cancelar quando permitido e listar. |
| Cancelado | NFC-e cancelada. | Consultar XML/PDF de cancelamento. |
| Bloqueado para edicao | Venda POS possui numero NFC-e maior que zero. | Consultar/imprimir, sem editar/excluir venda. |

## 13. Fluxos funcionais

### 13.1 Emissao PDV

| Passo | Ator | Acao | Validacao | Resultado |
|---:|---|---|---|---|
| 1 | Operador de caixa | Finaliza venda POS. | Venda valida e tenant/empresa identificados. | Venda pronta para transmissao. |
| 2 | Epros | Carrega parametros NFC-e. | Ambiente, serie, numero, CSC, ID CSC e certificado. | Parametros prontos ou bloqueio. |
| 3 | Epros | Valida itens. | CFOP, CST/CSOSN, NCM, valores e matriz fiscal. | Itens validos ou rejeicao funcional. |
| 4 | Epros | Transmite NFC-e. | Documento novo/rejeitado. | Retorno fiscal. |
| 5 | Epros | Registra autorizacao ou rejeicao. | Retorno da autoridade fiscal. | Documento atualizado. |
| 6 | Epros | Bloqueia edicao/exclusao da venda. | Numero NFC-e maior que zero. | Venda fiscal protegida. |

### 13.2 Impressao

| Passo | Ator | Acao | Validacao | Resultado |
|---:|---|---|---|---|
| 1 | Operador | Solicita impressao ou preview. | NFC-e existente. | Documento localizado. |
| 2 | Epros | Valida XML de retorno/autorizacao. | XML disponivel. | Impressao liberada. |
| 3 | Epros | Aplica configuracao de impressao. | EmpresaId e layout. | DANFCE impresso/renderizado. |

## 14. Telas e operacoes esperadas

| Tela/operacao | Campos principais | Acoes | Observacao |
|---|---|---|---|
| Emissao NFC-e PDV | Venda, total, itens, status, numero, chave. | Transmitir, imprimir, consultar. | Acionada pela finalizacao do PDV. |
| Geracao/preview NFC-e | Documento, itens, total, destinatario quando houver. | Gerar, renderizar DANFCE. | Material cita geracao e renderizacao. |
| Detalhe NFC-e | Chave, protocolo, XML, PDF, status, motivo rejeicao. | Ver, imprimir, cancelar, baixar. | Cancelamento em EF propria. |
| Listagem NFC-e | Periodo, estado, numero fiscal. | Consultar aprovadas/canceladas. | Filtro por numero maior que zero. |
| Impressao nao fiscal | Venda/documento. | Imprimir documento nao fiscal. | Material cita operacao; regra final na MC. |

## 15. Integracoes

| Integracao | Direcao | Dados | Regra |
|---|---|---|---|
| PDV/Vendas | Entrada/Saida | Venda, itens, pagamentos, numero NFC-e e bloqueio pos-emissao. | Regras comerciais ficam no fluxo dono. |
| Parametros fiscais | Entrada | Ambiente, serie, numero, CSC, ID CSC, certificado. | Sem parametros validos nao ha transmissao. |
| Motor tributario | Entrada/Saida | CFOP, CST, CSOSN, NCM, tributos e rateios. | Detalhe em EF propria. |
| Configuracao de impressao | Entrada | Layout, margens, QR Code, segunda via. | Necessaria para DANFCE. |
| Relatorios/contador | Saida | XML/PDF e consulta por periodo. | Detalhe em XML contador/downloads. |

## 16. Mensagens e erros funcionais

| Codigo | Mensagem funcional | Quando ocorre |
|---|---|---|
| MSG-NFCE-001 | NFC-e nao localizada. | Consulta por id/chave sem documento. |
| MSG-NFCE-002 | Certificado nao encontrado. | Transmissao exige certificado inexistente. |
| MSG-NFCE-003 | CSC ou ID CSC ausente. | NFC-e sem credenciais de emissao. |
| MSG-NFCE-004 | Documento ja autorizado. | Tentativa de transmissao indevida. |
| MSG-NFCE-005 | Documento rejeitado. | Autoridade fiscal rejeita transmissao. |
| MSG-NFCE-006 | XML de retorno ausente. | Impressao exige XML autorizado. |
| MSG-NFCE-007 | Arquivo nao encontrado. | XML/PDF requerido nao existe. |
| MSG-NFCE-008 | Venda bloqueada para edicao. | Venda possui numero NFC-e maior que zero. |
| MSG-NFCE-009 | CFOP invalido para NFC-e. | CFOP fora do dominio permitido. |
| MSG-NFCE-010 | CSOSN invalido para NFC-e. | CSOSN fora do dominio permitido. |
| MSG-NFCE-011 | CST ICMS invalido para NFC-e. | CST fora do dominio permitido. |
| MSG-NFCE-012 | Configuracao de impressao NFC-e ausente ou duplicada. | Impressao/configuracao invalida. |

## 17. Requisitos nao funcionais

| Categoria | Requisito |
|---|---|
| Seguranca | Emissao, cancelamento, download e impressao devem exigir tenant, empresa e permissao. |
| Auditoria | Emissao, rejeicao, autorizacao, impressao, download e bloqueio de edicao devem ser auditados. |
| Integridade | XML de envio/retorno e status fiscal nao devem ser perdidos. |
| Disponibilidade | PDV deve receber resposta clara de autorizacao, rejeicao ou erro. |
| Idempotencia | Estrategia final de idempotencia da transmissao PDV fica na MC. |
| Retencao | Politica legal de guarda de XML/PDF fica na MC. |

## 18. Criterios de aceite

| Criterio | Resultado esperado |
|---|---|
| Modelo 65 | Toda NFC-e emitida por este fluxo usa modelo 65. |
| Parametros NFC-e | Emissao bloqueia sem ambiente, serie/numero e CSC/ID CSC quando exigidos. |
| Autorizacao | Retorno autorizado grava chave, numero, XML, PDF/caminho e status. |
| Rejeicao | Retorno rejeitado grava motivo e permite nova transmissao quando estado permitir. |
| Bloqueio POS | Venda POS com numero NFC-e maior que zero nao permite editar/excluir. |
| Impressao | DANFCE usa XML de retorno e configuracao de impressao quando disponivel. |
| Itens | Todos os campos comprovados de item aparecem no dicionario. |
| Sem invencao | Campo sem detalhe no material aparece como `Nao informado no material` ou item de MC. |

## 19. Itens para MC

| Item | Motivo |
|---|---|
| Contingencia NFC-e | Material cita configuracao, mas nao detalha fluxo completo. |
| Impressao nao fiscal | Material cita operacao, mas nao detalha regras. |
| Idempotencia no PDV | Material nao define chave idempotente final para evitar dupla emissao. |
| Sincronismo offline | Material nao detalha operacao offline de NFC-e neste recorte. |
| Matriz fiscal completa | Material traz dominios e classes de validacao, mas matriz completa deve ser consolidada no motor. |
| Politica de retencao XML/PDF | Caminhos e downloads existem, politica final nao informada. |
| Permissoes finais | Material evidencia acoes, mas matriz RBAC final deve ser definida. |
