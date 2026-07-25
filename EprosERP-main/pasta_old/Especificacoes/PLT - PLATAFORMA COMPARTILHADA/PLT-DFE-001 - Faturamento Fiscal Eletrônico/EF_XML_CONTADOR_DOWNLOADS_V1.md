# EF_XML_CONTADOR_DOWNLOADS_V1

## 1. Identificacao

| Campo | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | FATURAMENTO_FISCAL_ELETRONICO |
| Documento | Especificacao funcional - XML contador e downloads fiscais |
| Versao | V1 |
| Status | Concluido |
| Data | 2026-06-08 |

## 2. Objetivo funcional

XML contador e downloads fiscais permitem ao Epros listar documentos fiscais por mes e ano, baixar XML/PDF por chave, baixar XML de envio de venda e compra, baixar XML/PDF de cancelamento e CC-e, regerar PDF quando comprovado e gerar pacote ZIP mensal para contador com XMLs e, opcionalmente, PDFs.

Esta EF consolida os contratos funcionais comprovados no material canonico para consulta mensal, downloads por chave, downloads por origem comercial e geracao de ZIP, sem inventar politica de retencao, permissoes finais, layout de arquivo ou armazenamento definitivo alem do que esta informado.

## 3. Escopo

| Area | Incluso | Status |
|---|---|---|
| Listagem mensal | Consulta por mes, ano, pagina e tamanho da pagina | Com conteudo |
| Colunas da listagem | CRT, emitente, destinatario, UF, chave, protocolo, serie, numero, status e emissao | Com conteudo |
| ZIP contador | Download mensal com XMLs e opcionalmente PDFs | Com conteudo |
| Nome do ZIP | `XMLS-com-pdfs-{mes}-{ano}.zip` ou `XMLS-sem-pdfs-{mes}-{ano}.zip` | Com conteudo |
| Download XML autorizado | Download por chave | Com conteudo |
| Download PDF autorizado | Download por chave | Com conteudo |
| Download XML/PDF de cancelamento | Download por chave | Com conteudo |
| Download XML/PDF de CC-e | Download por chave | Com conteudo |
| XML de envio de venda | Download por identificador da venda | Com conteudo |
| XML de envio de compra | Download por identificador da compra | Com conteudo |
| Regeracao de PDF | Gerar e baixar novo PDF de NF-e por chave | Parcial |
| Dominio de servico fiscal | Consulta de dominio fiscal e dominio principal | Parcial |
| Armazenamento | Caminho por documento, ano e mes | Parcial |
| Permissoes finais | Matriz final de acesso para contador/download | Incompleto |
| Retencao legal | Politica de retencao XML/PDF/ZIP | Incompleto |

## 4. Fora de escopo

| Item | Motivo |
|---|---|
| Emissao de documentos fiscais | Possui EFs especificas por documento. |
| Importacao XML | Possui EF especifica na fila macro. |
| Manifesto DFe | Possui EF especifica concluida. |
| Armazenamento documental corporativo completo | Politica final nao informada no material. |
| Geracao completa de DANFE/eventos | Esta EF cobre download/regeracao comprovada, nao layout fiscal completo. |

## 5. Atores e responsabilidades

| Ator | Responsabilidade | Observacao |
|---|---|---|
| Usuario fiscal | Consultar documentos, baixar XML/PDF e gerar ZIP mensal. | Permissoes finais nao informadas no material. |
| Contador | Receber pacote mensal de documentos fiscais. | Acesso direto ou mediado nao informado no material. |
| Usuario de vendas | Baixar XML de envio relacionado a venda quando permitido. | Contrato com Vendas fica no modulo dono. |
| Usuario de compras | Baixar XML de envio relacionado a compra quando permitido. | Contrato com Compras fica no modulo dono. |
| Epros | Localizar arquivos, validar vinculo fiscal, gerar ZIP e retornar arquivos. | Retencao e auditoria final ficam na MC. |

## 6. Conceitos funcionais

| Conceito | Definicao |
|---|---|
| XML contador | Pacote mensal de XMLs fiscais para uso contabil. |
| Download por chave | Recuperacao de XML ou PDF usando a chave fiscal do documento/evento. |
| XML de envio | XML gerado antes/para transmissao de venda ou compra. |
| ZIP contador | Arquivo compactado com XMLs e opcionalmente PDFs. |
| ComPdf | Indicador de inclusao de PDFs no pacote mensal. |
| Periodo mensal | Mes e ano usados para listar e empacotar documentos. |
| Documento fiscal baixavel | Documento ou evento com XML/PDF disponivel no armazenamento fiscal. |

## 7. Capacidades funcionais

| Capacidade | Descricao | Entrada principal | Saida esperada |
|---|---|---|---|
| Listar documentos por referencia | Lista documentos fiscais por mes, ano, pagina e tamanho da pagina. | Mes, Ano, Pagina, TamanhoPagina | Lista paginada. |
| Baixar PDF autorizado | Baixa PDF fiscal por chave. | Chave | PDF entregue ou erro. |
| Baixar XML autorizado | Baixa XML fiscal por chave. | Chave | XML entregue ou erro. |
| Baixar PDF de cancelamento | Baixa PDF de cancelamento por chave. | Chave | PDF entregue ou erro. |
| Baixar XML de cancelamento | Baixa XML de cancelamento por chave. | Chave | XML entregue ou erro. |
| Baixar PDF de CC-e | Baixa PDF de carta de correcao por chave. | Chave | PDF entregue ou erro. |
| Baixar XML de CC-e | Baixa XML de carta de correcao por chave. | Chave | XML entregue ou erro. |
| Baixar XML de envio de venda | Baixa XML de envio por identificador de venda. | VendaId | XML entregue ou erro. |
| Baixar XML de envio de compra | Baixa XML de envio por identificador de compra. | CompraId | XML entregue ou erro. |
| Regerar PDF NF-e | Gera e baixa novo PDF por chave. | Chave e dados de apresentacao quando disponiveis | PDF regerado. |
| Gerar ZIP contador | Gera pacote mensal por mes, ano, destinatario e indicador de PDF. | Mes, Ano, Destinatario, IncluiPdfs | ZIP entregue. |
| Consultar dominio fiscal | Retorna dominio fiscal usado em downloads. | Nao informado no material | Dominio fiscal. |
| Consultar dominio principal | Retorna dominio principal usado em downloads. | Nao informado no material | Dominio principal. |

## 8. Regras funcionais

| Regra | Descricao | Contexto | Resultado esperado | Severidade | Fonte funcional |
|---|---|---|---|---|---|
| REG-XML-001 | Listagem mensal deve receber mes e ano. | Consulta mensal | Buscar documentos do periodo. | Bloqueante | Material comprova consulta por mes/ano. |
| REG-XML-002 | Listagem mensal deve suportar pagina e tamanho de pagina. | Consulta mensal | Retornar resultado paginado. | Alta | Material comprova pagina/tamanho. |
| REG-XML-003 | A listagem mensal deve exibir CRT, emitente, destinatario, UF, chave, protocolo, serie, numero, status e emissao quando disponiveis. | Tela/consulta | Apresentar colunas fiscais. | Alta | Colunas comprovadas. |
| REG-XML-004 | Download XML autorizado deve usar chave fiscal. | Download XML | Entregar XML ou erro funcional. | Bloqueante | Operacao comprovada. |
| REG-XML-005 | Download PDF autorizado deve usar chave fiscal. | Download PDF | Entregar PDF ou erro funcional. | Bloqueante | Operacao comprovada. |
| REG-XML-006 | Download XML de cancelamento deve usar chave fiscal. | Evento cancelamento | Entregar XML de cancelamento ou erro. | Alta | Operacao comprovada. |
| REG-XML-007 | Download PDF de cancelamento deve usar chave fiscal. | Evento cancelamento | Entregar PDF de cancelamento ou erro. | Alta | Operacao comprovada. |
| REG-XML-008 | Download XML de CC-e deve usar chave fiscal. | Evento CC-e | Entregar XML de CC-e ou erro. | Alta | Operacao comprovada. |
| REG-XML-009 | Download PDF de CC-e deve usar chave fiscal. | Evento CC-e | Entregar PDF de CC-e ou erro. | Alta | Operacao comprovada. |
| REG-XML-010 | XML de envio de venda deve ser baixado por identificador de venda. | Venda | Entregar XML de envio ou erro. | Alta | Operacao comprovada. |
| REG-XML-011 | XML de envio de compra deve ser baixado por identificador de compra. | Compra | Entregar XML de envio ou erro. | Alta | Operacao comprovada. |
| REG-XML-012 | ZIP contador deve receber mes, ano, destinatario e indicador de inclusao de PDF. | ZIP contador | Gerar pacote mensal. | Bloqueante | Operacao comprovada. |
| REG-XML-013 | ZIP contador deve permitir geracao com PDFs. | ZIP contador | Arquivo nomeado como com PDFs. | Alta | Nome comprovado. |
| REG-XML-014 | ZIP contador deve permitir geracao sem PDFs. | ZIP contador | Arquivo nomeado como sem PDFs. | Alta | Nome comprovado. |
| REG-XML-015 | Nome do ZIP com PDFs deve seguir `XMLS-com-pdfs-{mes}-{ano}.zip`. | ZIP contador | Arquivo nomeado corretamente. | Media | Nome comprovado. |
| REG-XML-016 | Nome do ZIP sem PDFs deve seguir `XMLS-sem-pdfs-{mes}-{ano}.zip`. | ZIP contador | Arquivo nomeado corretamente. | Media | Nome comprovado. |
| REG-XML-017 | Arquivos fiscais devem ser localizaveis por documento, ano e mes quando armazenados no repositorio fiscal comprovado. | Armazenamento | Localizar XML/PDF por periodo. | Alta | Caminho comprovado por documento/ano/mes. |
| REG-XML-018 | Regeracao de PDF NF-e deve ocorrer por chave quando solicitada. | Regeracao PDF | PDF regerado/baixado. | Media | Operacao comprovada. |
| REG-XML-019 | Downloads fiscais devem validar existencia do arquivo. | Download | Entregar arquivo ou erro funcional claro. | Bloqueante | Regra macro comprovada. |
| REG-XML-020 | Downloads fiscais devem validar integridade do vinculo fiscal. | Download | Impedir arquivo de outro contexto fiscal. | Bloqueante | Regra macro comprovada. |
| REG-XML-021 | Permissao final de download/contador deve ser tratada como lacuna ate definicao formal. | Seguranca | Registrar na MC. | Bloqueante | Material indica lacuna de permissao. |
| REG-XML-022 | Politica de retencao de XML/PDF/ZIP deve ser tratada como lacuna ate definicao formal. | Retencao | Registrar na MC. | Alta | Material nao informa retencao final. |

## 9. Estados e situacoes

| Situacao | Descricao | Observacao |
|---|---|---|
| Disponivel | Arquivo fiscal existe e pode ser baixado. | Dominio final nao informado no material. |
| Ausente | Arquivo fiscal nao localizado. | Deve retornar erro funcional claro. |
| Gerando ZIP | Pacote mensal em montagem. | Processamento comprovado como blob/arquivo. |
| ZIP gerado | Pacote mensal pronto para download. | Nome depende de inclusao de PDF. |
| Erro | Falha ao gerar/baixar arquivo. | Mensagens finais ficam na MC. |

## 10. Modelo de dados funcional e implantavel

O material comprova contratos de consulta e download, armazenamento por documento/ano/mes, XMLs/PDFs por chave, XML de envio por venda/compra e ZIP mensal para contador. Como nao ha tabela final propria de downloads no material, a EF organiza entidades funcionais de consulta, arquivo fiscal, pacote contador e auditoria de download como consolidacao operacional.[^1]

| Entidade funcional | Finalidade | Cardinalidade | Persistencia indicada |
|---|---|---|---|
| xml_contador_consulta | Registrar consulta mensal/paginada. | 0..N por consulta | Consolidacao funcional.[^1] |
| arquivo_fiscal_download | Representar XML/PDF baixavel por chave/origem. | 0..N por documento/evento | Consolidacao funcional.[^1] |
| xml_contador_pacote | Representar ZIP mensal gerado para contador. | 0..N por mes/ano/destinatario | Consolidacao funcional.[^1] |
| xml_contador_item | Relacionar arquivos incluidos no ZIP. | 0..N por pacote | Consolidacao funcional.[^1] |
| dominio_download_fiscal | Guardar dominios funcionais retornados para download. | 0..N por consulta | Consolidacao funcional.[^1] |
| auditoria_download_fiscal | Registrar solicitacoes de download/ZIP. | 0..N por operacao | Necessaria; estrutura final nao informada.[^1] |

### 10.1 Relacionamentos funcionais

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| xml_contador_consulta | lista | arquivo_fiscal_download | Consulta mensal retorna documentos baixaveis. |
| xml_contador_pacote | possui | xml_contador_item | ZIP contem arquivos fiscais. |
| xml_contador_item | referencia | arquivo_fiscal_download | Item aponta para XML/PDF incluido. |
| arquivo_fiscal_download | registra | auditoria_download_fiscal | Cada download deve ser auditavel quando politica final existir. |
| xml_contador_pacote | registra | auditoria_download_fiscal | Cada ZIP deve ser auditavel quando politica final existir. |

## 11. Dicionario de dados implantavel

### 11.1 xml_contador_consulta

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| Mes | Numero | 1-12 | Sim | Periodo | Mes da consulta. |
| Ano | Numero | 4 digitos | Sim | Periodo | Ano da consulta. |
| Pagina | Numero | Nao informado no material | Sim | Paginacao | Pagina solicitada. |
| TamanhoPagina | Numero | Nao informado no material | Sim | Paginacao | Tamanho da pagina. |
| TotalRegistros | Numero | Nao informado no material | Nao informado no material | Resultado | Total final nao detalhado para esta consulta.[^1] |
| DataConsulta | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Estrutura final nao informada.[^1] |

### 11.2 arquivo_fiscal_download

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| Chave | Texto | Nao informado no material | Condicional | Chave fiscal | Usada nos downloads por chave. |
| TipoArquivo | Enum/texto | XML, PDF, ZIP | Sim | Tipo | Tipo funcional do arquivo. |
| TipoDocumento | Enum/texto | Autorizado, Cancelamento, CC-e, EnvioVenda, EnvioCompra | Sim | Classificacao | Consolidado a partir das operacoes comprovadas.[^1] |
| VendaId | Identificador | Nao informado no material | Condicional | Venda | Obrigatorio para XML de envio de venda. |
| CompraId | Identificador | Nao informado no material | Condicional | Compra | Obrigatorio para XML de envio de compra. |
| Documento | Texto | Nao informado no material | Condicional | Documento fiscal | Usado no armazenamento por documento. |
| Mes | Numero | 1-12 | Condicional | Periodo | Usado no armazenamento/ZIP. |
| Ano | Numero | 4 digitos | Condicional | Periodo | Usado no armazenamento/ZIP. |
| CaminhoArquivo | Texto | Nao informado no material | Nao informado no material | Armazenamento | Caminho final nao informado. |
| Conteudo | Arquivo/binario | XML/PDF/ZIP | Nao informado no material | Arquivo | Pode ser retornado diretamente no download. |

### 11.3 xml_contador_pacote

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| Mes | Numero | 1-12 | Sim | Periodo | Mes do pacote. |
| Ano | Numero | 4 digitos | Sim | Periodo | Ano do pacote. |
| Destinatario | Texto | Nao informado no material | Sim | Contador/destino | Campo comprovado no envio de pacote. |
| IncluiPdfs | Booleano | Sim/Nao | Sim | Composicao | Define ZIP com ou sem PDF. |
| NomeArquivo | Texto | `XMLS-com-pdfs-{mes}-{ano}.zip` ou `XMLS-sem-pdfs-{mes}-{ano}.zip` | Sim | Arquivo | Nome comprovado. |
| StatusGeracao | Enum/texto | Nao informado no material | Nao informado no material | Status | Dominio final nao informado. |
| DataGeracao | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Estrutura final nao informada.[^1] |

### 11.4 xml_contador_item

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| PacoteId | Identificador | Nao informado no material | Sim | Relacao com pacote | Vinculo com ZIP.[^1] |
| ArquivoFiscalId | Identificador | Nao informado no material | Sim | Relacao com arquivo | Arquivo incluido no ZIP.[^1] |
| TipoArquivo | Enum/texto | XML ou PDF | Sim | Tipo | ZIP pode incluir XML e, opcionalmente, PDF. |

### 11.5 dominio_download_fiscal

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| TipoDominio | Enum/texto | Fiscal, Principal | Sim | Dominio | Material comprova os dois dominios. |
| ValorDominio | Texto | Nao informado no material | Nao informado no material | URL/dominio | Valor final nao informado no material. |

### 11.6 auditoria_download_fiscal

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| TipoOperacao | Enum/texto | Consulta, Download, ZIP, RegeracaoPdf | Sim | Operacao | Consolidado a partir das operacoes comprovadas.[^1] |
| Chave | Texto | Nao informado no material | Condicional | Chave fiscal | Quando operacao for por chave. |
| Mes | Numero | 1-12 | Condicional | Periodo | Quando operacao for mensal. |
| Ano | Numero | 4 digitos | Condicional | Periodo | Quando operacao for mensal. |
| UsuarioId | Identificador | Nao informado no material | Nao informado no material | Usuario | Auditoria final nao informada. |
| DataHora | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Estrutura final nao informada.[^1] |
| Resultado | Texto/enum | Nao informado no material | Nao informado no material | Resultado | Sucesso/erro final nao informado. |

## 12. Fluxos funcionais

### 12.1 Listar documentos do mes

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Solicita listagem mensal. | Mes, ano, pagina e tamanho da pagina | Consulta iniciada. |
| 2 | Epros | Localiza documentos do periodo. | Documento fiscal da empresa | Lista paginada. |
| 3 | Epros | Exibe colunas comprovadas. | CRT, emitente, destinatario, UF, chave, protocolo, serie, numero, status, emissao | Listagem fiscal. |

### 12.2 Baixar arquivo por chave

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Solicita XML/PDF por chave. | Chave e tipo de arquivo/evento | Pedido recebido. |
| 2 | Epros | Valida existencia e vinculo fiscal. | Chave | Arquivo localizado ou erro. |
| 3 | Epros | Entrega arquivo. | XML/PDF | Download concluido. |

### 12.3 Baixar XML de envio por venda ou compra

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario autorizado | Solicita XML de envio. | VendaId ou CompraId | Pedido recebido. |
| 2 | Epros | Localiza XML de envio. | Origem comercial | XML localizado ou erro. |
| 3 | Epros | Entrega XML. | XML | Download concluido. |

### 12.4 Gerar ZIP contador

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Solicita pacote mensal. | Mes, ano, destinatario, incluiPdfs | Geracao iniciada. |
| 2 | Epros | Seleciona documentos fiscais do periodo. | Mes e ano | Arquivos elegiveis. |
| 3 | Epros | Inclui XMLs e, quando indicado, PDFs. | IncluiPdfs | Conteudo do ZIP preparado. |
| 4 | Epros | Nomeia o pacote. | Mes, ano, incluiPdfs | Nome `XMLS-com-pdfs...` ou `XMLS-sem-pdfs...`. |
| 5 | Epros | Entrega ZIP. | Pacote | Download concluido. |

### 12.5 Regerar PDF

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Solicita regeracao de PDF. | Chave | Pedido recebido. |
| 2 | Epros | Regera PDF quando dados existirem. | Documento fiscal e apresentacao disponivel | PDF gerado. |
| 3 | Epros | Entrega PDF. | PDF | Download concluido. |

## 13. Validacoes e mensagens

| Codigo | Mensagem | Condicao |
|---|---|---|
| MSG-XML-001 | Mes e ano sao obrigatorios. | Consulta ou ZIP sem periodo. |
| MSG-XML-002 | Chave fiscal e obrigatoria. | Download por chave sem chave. |
| MSG-XML-003 | Venda e obrigatoria para XML de envio de venda. | Download de envio de venda sem identificador. |
| MSG-XML-004 | Compra e obrigatoria para XML de envio de compra. | Download de envio de compra sem identificador. |
| MSG-XML-005 | Destinatario e obrigatorio para ZIP contador. | ZIP sem destinatario. |
| MSG-XML-006 | Arquivo fiscal nao localizado. | Caminho/conteudo ausente. |
| MSG-XML-007 | Documento fiscal nao pertence ao contexto informado. | Vinculo fiscal invalido. |
| MSG-XML-008 | Falha ao gerar ZIP contador. | Erro de empacotamento. |
| MSG-XML-009 | PDF nao pode ser regerado com os dados disponiveis. | Regeracao sem dados suficientes. |

## 14. Integracoes

| Integracao | Direcao | Dados | Regra | Lacuna |
|---|---|---|---|---|
| NF-e/NFC-e | Entrada | Chave, protocolo, serie, numero, status, XML, PDF | Fonte principal dos downloads. | Retencao e eventos finais. |
| Cancelamento | Entrada | XML/PDF de cancelamento por chave | Download por chave. | Permissao e retencao. |
| CC-e | Entrada | XML/PDF de CC-e por chave | Download por chave. | Permissao e retencao. |
| Vendas | Entrada | VendaId e XML de envio | Download XML de envio de venda. | Contrato final com Vendas. |
| Compras | Entrada | CompraId e XML de envio | Download XML de envio de compra. | Contrato final com Compras. |
| Contabilidade | Saida | ZIP mensal com XML/PDF opcional | Pacote para contador. | Modelo de acesso direto/indireto. |
| Armazenamento fiscal | Entrada/Saida | Documento, ano, mes, arquivos | Localizacao de arquivos fiscais. | Politica final de retencao. |

## 15. Permissoes e seguranca

| Controle | Regra |
|---|---|
| Download por chave | Deve validar contexto fiscal e existencia do arquivo. |
| ZIP contador | Deve validar periodo, destinatario e contexto fiscal. |
| XML de venda/compra | Deve validar origem e permissao do usuario. |
| Permissoes finais | Nao informadas no material; registradas na MC. |
| Auditoria | Necessaria para downloads fiscais; estrutura final nao informada no material. |

## 16. Relatorios e consultas

| Consulta | Filtros comprovados | Resultado |
|---|---|---|
| XML contador mensal | Mes, ano, pagina, tamanho da pagina | CRT, emitente, destinatario, UF, chave, protocolo, serie, numero, status, emissao. |
| Download por chave | Chave | XML/PDF autorizado, cancelamento ou CC-e. |
| XML envio venda | VendaId | XML de envio da venda. |
| XML envio compra | CompraId | XML de envio da compra. |
| ZIP contador | Mes, ano, destinatario, incluiPdfs | ZIP mensal. |

## 17. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-XML-001 | Listagem mensal deve exigir mes e ano. |
| CA-XML-002 | Listagem mensal deve suportar pagina e tamanho de pagina. |
| CA-XML-003 | Listagem mensal deve apresentar as colunas comprovadas quando os dados existirem. |
| CA-XML-004 | Download XML/PDF por chave deve entregar arquivo ou erro claro. |
| CA-XML-005 | Download de cancelamento e CC-e deve usar chave. |
| CA-XML-006 | XML de envio de venda deve exigir VendaId. |
| CA-XML-007 | XML de envio de compra deve exigir CompraId. |
| CA-XML-008 | ZIP contador com PDFs deve usar nome `XMLS-com-pdfs-{mes}-{ano}.zip`. |
| CA-XML-009 | ZIP contador sem PDFs deve usar nome `XMLS-sem-pdfs-{mes}-{ano}.zip`. |
| CA-XML-010 | Downloads devem validar existencia do arquivo e vinculo fiscal. |

## 18. Lacunas encaminhadas para MC

| Lacuna | Impacto |
|---|---|
| Permissoes finais para downloads e contador | Necessario para seguranca. |
| Politica de retencao XML/PDF/ZIP | Necessario para conformidade fiscal. |
| Auditoria detalhada | Necessario para rastrear acessos a documentos fiscais. |
| Formato final da resposta paginada | Necessario para contrato de API/tela. |
| Regras de arquivo ausente/regeracao | Necessario para suporte operacional. |
| Modelo de acesso do contador | Necessario para liberar pacote com seguranca. |

## 19. Proximo passo

O proximo documento especifico da fila macro e `EF_IMPORTACAO_XML`, detalhando importacao XML conforme material disponivel.

[^1]: Consolidacao funcional criada para tornar implantavel a especificacao, pois o material comprova operacoes de consulta/download/ZIP e armazenamento por documento/ano/mes, mas nao informa tabela final completa de consulta, arquivo, pacote, dominio ou auditoria.
