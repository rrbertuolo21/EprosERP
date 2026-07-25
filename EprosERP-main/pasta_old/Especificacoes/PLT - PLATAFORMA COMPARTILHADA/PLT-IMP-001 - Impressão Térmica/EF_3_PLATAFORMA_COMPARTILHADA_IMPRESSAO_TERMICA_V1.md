# EF_3_PLATAFORMA_COMPARTILHADA_IMPRESSAO_TERMICA_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Tipo de documento:** Especificacao Funcional definitiva  
**Versao:** V1  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** IMPRESSAO_TERMICA  
**Status:** Concluido  
**Ultima atualizacao:** 2026-06-09

## 1. Controle do documento

| Item | Conteudo |
|---|---|
| Responsavel pela elaboracao | Agente de analise funcional |
| Responsavel pela validacao funcional | Siser |
| Responsavel pela validacao tecnica | Siser |
| Area dona do processo | Plataforma Compartilhada |
| Publico-alvo | Produto, negocio, implantacao, desenvolvimento, suporte, operacao |
| Fonte de verdade | Esta EF e a fonte funcional definitiva do submodulo |

## 2. Objetivo funcional

O submodulo Impressao Termica do Epros centraliza configuracao, validacao, emissao, reimpressao e controle de comprovantes termicos e impressos operacionais relacionados a PDV, NFC-e, SAT, compra, cozinha, caixa, fechamento e etiquetas. O submodulo tambem define fronteiras para que transmissao fiscal, DANFE em PDF, dashboards e processos comerciais continuem pertencendo aos respectivos modulos donos.

| Pergunta | Resposta |
|---|---|
| Para que o submodulo existe? | Para padronizar a impressao local e operacional do Epros em canais termicos, fiscais e de comprovantes. |
| Que problema de negocio resolve? | Evita que cada tela trate impressora, layout, erro e reimpressao de forma diferente. |
| Qual resultado operacional deve produzir? | Comprovantes impressos ou pendencias claras quando faltarem configuracao, permissao, dado, XML autorizado ou impressora valida. |
| Quais areas dependem dele? | Vendas, PDV, Compras, Fiscal Eletronico, Caixa, Cozinha, Estoque, Relatorios e Aplicativo. |

## 3. Escopo funcional

### 3.1 Dentro do escopo

| Capacidade | Descricao | Observacao |
|---|---|---|
| Configuracao de impressora | Manter impressora local, tamanho de papel e canal de impressao. | Material informa nome de impressora, Start.ini e configuracao local. |
| Cupom nao fiscal de venda | Imprimir pedido de venda sem valor fiscal com itens, totais, pagamentos, troco, cliente e rodape. | Material informa layout de venda. |
| Cupom nao fiscal de compra | Imprimir cupom de compra com estrutura equivalente ao cupom de venda. | Material informa cupom de compra. |
| Comanda de cozinha | Imprimir pedido de cozinha para itens preparados. | Material informa filtro por produto preparado. |
| Abertura de caixa | Imprimir comprovante de abertura de caixa. | Material informa numero, data/hora, operador, valor e assinatura. |
| Sangria e suprimento | Imprimir comprovante de fluxo de caixa. | Material informa entrada como suprimento e demais fluxos como sangria. |
| Impressao fiscal pos-autorizacao | Imprimir NFC-e ou SAT a partir de XML autorizado. | Transmissao fiscal fica fora deste submodulo. |
| Impressao por canal desktop/mobile | Suportar canais locais de impressao, incluindo impressora instalada e Bluetooth quando aplicavel. | Material informa canais Windows e Android. |
| Fechamento de caixa | Imprimir relatorios de encerramento de caixa. | Material informa 10 relatorios/variantes. |
| Etiqueta de produto | Imprimir etiqueta com nome, preco e codigo de barras. | Material informa etiqueta de produto. |
| Teste de impressao | Permitir teste de configuracao de impressora. | Material informa cupom de teste. |
| Reimpressao | Permitir reimpressao de pedido e NFC-e quando houver origem disponivel. | Material informa reimpressao. |

### 3.2 Fora do escopo

| Item fora do escopo | Motivo | Destino correto |
|---|---|---|
| Transmissao fiscal | Impressao usa XML/resultado autorizado; transmissao pertence ao fiscal. | Faturamento Fiscal Eletronico |
| DANFE PDF web | Material separa relatorios PDF web da impressao termica local. | Fiscal, Vendas ou Relatorios |
| Fluxo de comanda/restaurante | Este submodulo define layout e impressao, nao o processo operacional completo. | PDV/Vendas |
| Dashboard e analises de caixa | Este submodulo imprime relatorios, nao agrega indicadores. | Analytics/Relatorios |
| Cadastro fiscal SAT ou tipo de cupom do caixa | IMP consome parametrizacao, nao e dono do cadastro fiscal. | Cadastros Base/Fiscal/PDV |

## 4. Glossario e conceitos funcionais

| Termo | Definicao funcional | Observacoes |
|---|---|---|
| Impressora local | Impressora configurada para uso pelo Epros em uma estacao ou dispositivo. | Pode vir de configuracao local. |
| Cupom nao fiscal | Impressao operacional sem valor fiscal. | Deve conter marca explicita de documento nao fiscal. |
| Comanda de cozinha | Impressao interna para preparo de item. | Apenas itens preparados entram nesse layout. |
| SAT | Documento fiscal impresso a partir de XML autorizado. | Transmissao fora do escopo. |
| NFC-e | Documento fiscal de consumidor impresso a partir de XML autorizado. | Transmissao fora do escopo. |
| Canal de impressao | Meio usado para imprimir: impressora instalada, impressora termica, Bluetooth ou canal fiscal. | Contratos finais pendentes na MC. |
| Reimpressao | Nova emissao fisica de documento previamente gerado/autorizado. | Deve respeitar dados de origem. |

## 5. Atores, papeis e responsabilidades

| Ator/Papel | Responsabilidade | Permissoes esperadas | Restricoes |
|---|---|---|---|
| Operador de caixa | Imprimir venda, abertura, sangria, suprimento, fechamento e reimpressao permitida. | Imprimir e reimprimir conforme permissao. | Nao altera parametros fiscais. |
| Operador de cozinha | Receber ou acionar impressao de comanda. | Consultar/imprimir comandas. | Nao altera venda. |
| Operador de compras | Imprimir cupom de compra quando aplicavel. | Imprimir cupom de compra. | Nao altera dados fiscais. |
| Gestor de loja | Configurar impressora, tamanho de papel e acompanhar falhas. | Configurar, testar, inativar configuracao. | Deve respeitar auditoria. |
| Fiscal/Retaguarda | Reimprimir NFC-e/SAT autorizado quando permitido. | Reimprimir documento autorizado. | Nao transmite por este submodulo. |
| Epros | Validar configuracao, montar layout, enviar impressao, registrar resultado e erro. | Automacao sistemica. | Nao imprime sem pre-condicoes. |

## 6. Visao operacional do submodulo

O gestor configura a impressora local e, quando aplicavel, tamanho de papel. O Epros salva a configuracao e a carrega no inicio da operacao. Antes de imprimir venda, compra, cozinha, caixa, fiscal ou etiqueta, o Epros valida se ha impressora configurada, dados obrigatorios e canal compativel.

Na venda, o Epros monta o cupom nao fiscal com cabecalho da empresa, itens, totais, pagamentos, troco, dados de entrega, observacoes, CPF/documento do consumidor e rodape. Na cozinha, imprime apenas itens marcados para preparo. Na abertura e movimentacao de caixa, imprime comprovantes operacionais com assinatura. Na impressao fiscal, o Epros usa XML autorizado de NFC-e ou SAT, aplica decisao de tipo de cupom e envia ao canal fiscal. Na reimpressao, o Epros usa o documento original e registra nova tentativa.

Falhas de impressao, impressora ausente, tamanho de papel ausente, XML indisponivel, canal nao suportado e dados obrigatorios faltantes devem produzir pendencia funcional clara e auditavel.

## 7. Capacidades funcionais

### 7.1 Configuracao da impressora

| Item | Especificacao |
|---|---|
| Objetivo | Definir a impressora e parametros locais usados para impressao. |
| Acionamento | Manual pelo gestor ou carregamento no inicio da operacao. |
| Pre-condicoes | Usuario autorizado e dispositivo/estacao identificavel. |
| Dados de entrada | Nome da impressora, canal, tamanho de papel e status. |
| Processamento | O Epros salva configuracao, carrega no runtime e valida antes de imprimir. |
| Resultado esperado | Impressora disponivel para operacao. |
| Pos-condicoes | Teste de impressao pode ser executado. |
| Excecoes | Nome vazio, tamanho de papel nao informado, impressora indisponivel. |
| Auditoria | Usuario, data, dispositivo e alteracao de configuracao. |

### 7.2 Cupom nao fiscal de venda

| Item | Especificacao |
|---|---|
| Objetivo | Imprimir pedido de venda operacional sem valor fiscal. |
| Acionamento | Finalizacao de venda, pagamento ou pedido, conforme tela consumidora. |
| Pre-condicoes | Venda existente, impressora configurada e itens disponiveis. |
| Dados de entrada | Empresa, venda, itens, pagamentos, totais, cliente, entrega e observacoes. |
| Processamento | O Epros monta cabecalho, itens, totais, pagamentos, troco, dados de consumidor e rodape. |
| Resultado esperado | Cupom nao fiscal impresso. |
| Pos-condicoes | Tentativa registrada. |
| Excecoes | Impressora ausente, venda sem itens, erro de canal. |
| Auditoria | Venda, usuario, impressora, horario e status da impressao. |

### 7.3 Comanda de cozinha

| Item | Especificacao |
|---|---|
| Objetivo | Imprimir comanda de preparo para cozinha. |
| Acionamento | Evento de pedido/comanda. |
| Pre-condicoes | Item de venda marcado para preparo e impressora configurada. |
| Dados de entrada | Venda, item, comanda, garcom/vendedor, dados adicionais. |
| Processamento | O Epros imprime titulo, data, numero da comanda, responsavel e itens preparados. |
| Resultado esperado | Comanda termica impressa. |
| Pos-condicoes | Impressao registrada. |
| Excecoes | Item nao preparado, impressora ausente ou dados insuficientes. |
| Auditoria | Pedido, item, usuario, horario e status. |

### 7.4 Abertura, sangria e suprimento de caixa

| Item | Especificacao |
|---|---|
| Objetivo | Imprimir comprovantes operacionais de caixa. |
| Acionamento | Abertura, sangria ou suprimento. |
| Pre-condicoes | Caixa e operador informados. |
| Dados de entrada | Numero do caixa/movimento, data/hora, operador, valor, tipo e motivo. |
| Processamento | O Epros monta comprovante com titulo, dados do movimento e linha de assinatura. |
| Resultado esperado | Comprovante impresso. |
| Pos-condicoes | Tentativa registrada. |
| Excecoes | Caixa ausente, valor ausente ou impressora ausente. |
| Auditoria | Caixa, operador, tipo, valor, horario e status. |

### 7.5 Impressao fiscal pos-autorizacao

| Item | Especificacao |
|---|---|
| Objetivo | Imprimir NFC-e ou SAT depois de autorizacao fiscal. |
| Acionamento | Pos-autorizacao fiscal ou reimpressao. |
| Pre-condicoes | XML autorizado disponivel, impressora configurada e tipo de cupom definido. |
| Dados de entrada | XML autorizado, modelo fiscal, caixa, UF, indicador de cancelamento e logotipo quando houver. |
| Processamento | O Epros decide entre NFC-e e SAT, aplica parametros de impressao e envia ao canal fiscal. |
| Resultado esperado | Documento fiscal impresso ou pendencia funcional. |
| Pos-condicoes | Tentativa registrada. |
| Excecoes | XML ausente, impressora ausente, tipo indefinido ou erro de canal. |
| Auditoria | Documento fiscal, chave quando disponivel, usuario, horario, impressora e status. |

### 7.6 Fechamento de caixa e etiqueta

| Item | Especificacao |
|---|---|
| Objetivo | Imprimir relatorios de encerramento e etiquetas de produto. |
| Acionamento | Fechamento de caixa ou reposicao/etiquetagem. |
| Pre-condicoes | Dados agregados do fechamento ou produto informados. |
| Dados de entrada | Caixa, produtos, fluxo, conferencia, produto, preco e codigo de barras. |
| Processamento | O Epros monta relatorio/etiqueta e envia para impressao ou PDF quando aplicavel. |
| Resultado esperado | Relatorio ou etiqueta impresso. |
| Pos-condicoes | Tentativa registrada. |
| Excecoes | Dados insuficientes, impressora ausente ou formato nao suportado. |
| Auditoria | Origem, usuario, horario e status. |

## 8. Regras de negocio

| Regra | Descricao | Condicao | Resultado | Severidade | Observacoes |
|---|---|---|---|---|---|
| IMP-001 | O Epros deve carregar a impressora configurada no inicio da operacao. | Login, abertura de PDV ou inicializacao de canal. | Nome da impressora fica disponivel para impressao. | Bloqueante | Material informa carregamento local. |
| IMP-002 | O Epros deve permitir salvar o nome da impressora local. | Configuracao de impressora. | Configuracao persistida. | Bloqueante | Material informa chave local. |
| IMP-003 | Impressao operacional deve ser bloqueada ou alertada quando a impressora estiver vazia. | Venda, pagamento, pedido, compra, fiscal ou mobile. | Impressao nao segue sem configuracao. | Bloqueante | Varias telas validam impressora. |
| IMP-004 | O tamanho de papel deve ser informado quando o canal exigir. | Impressao mobile/local. | Configuracao incompleta bloqueia impressao. | Bloqueante | Material informa tamanho obrigatorio diferente de zero. |
| IMP-005 | Cupom de venda deve conter cabecalho da empresa. | Impressao de venda. | Razao social, nome fantasia, documento, IE e endereco sao impressos quando disponiveis. | Bloqueante | Campos informados no material. |
| IMP-006 | Cupom de venda deve ser marcado como documento nao fiscal. | Impressao de venda nao fiscal. | Texto de documento nao fiscal aparece no cupom. | Bloqueante | Material informa marca explicita. |
| IMP-007 | Itens de venda devem imprimir codigo, descricao, quantidade, unidade, valor unitario, desconto e valor do item. | Cupom de venda. | Linha de item impressa. | Bloqueante | Layout informado. |
| IMP-008 | Acrescimo de item deve ser impresso somente quando maior que zero. | Item com acrescimo. | Acrescimo aparece no cupom. | Informativa | Material informa condicao. |
| IMP-009 | Desconto de item deve usar o valor de desconto do item quando maior que zero. | Item com desconto. | Desconto aparece no cupom. | Bloqueante | Regra correta para o Epros.[^nota1] |
| IMP-010 | Volumes, acrescimo total, desconto total, total de produtos e total final devem aparecer quando disponiveis. | Cupom de venda. | Totais impressos. | Bloqueante | Material informa totais. |
| IMP-011 | Formas de pagamento devem imprimir descricao e valor. | Cupom de venda com pagamentos. | Pagamentos aparecem no cupom. | Bloqueante | Material informa pagamentos. |
| IMP-012 | Troco deve ser impresso somente quando maior que zero. | Venda com troco. | Troco aparece no cupom. | Informativa | Material informa condicao. |
| IMP-013 | Endereco de entrega deve ser impresso quando existir. | Venda com entrega. | Dados de entrega aparecem no cupom. | Informativa | Material informa endereco. |
| IMP-014 | Dados adicionais da venda devem ser impressos quando preenchidos. | Venda com observacao. | Observacoes aparecem no cupom. | Informativa | Material informa dados adicionais. |
| IMP-015 | CPF/documento do consumidor deve priorizar o documento informado para a nota; se ausente, usa documento da pessoa; se ambos ausentes, imprime consumidor nao identificado ou documento padrao conforme politica validada. | Rodape de cupom. | Consumidor identificado ou nao identificado. | Alerta | Documento padrao final fica na MC. |
| IMP-016 | Rodape de venda deve conter numero da venda, data de emissao e aviso sem valor fiscal. | Cupom nao fiscal. | Rodape impresso. | Bloqueante | Material informa rodape. |
| IMP-017 | Comanda de cozinha deve imprimir apenas item preparado. | Impressao de cozinha. | Itens nao preparados nao entram na comanda. | Bloqueante | Material informa filtro. |
| IMP-018 | Comanda deve conter numero do pedido, data/hora, comanda, responsavel e dados adicionais do item quando houver. | Impressao de cozinha. | Comanda completa. | Bloqueante | Material informa campos. |
| IMP-019 | Abertura de caixa deve imprimir comprovante com numero, data/hora, operador, valor e assinatura. | Abertura de caixa. | Comprovante impresso. | Bloqueante | Material informa layout. |
| IMP-020 | Fluxo de caixa de entrada deve ser tratado como suprimento; demais fluxos como sangria. | Impressao de fluxo. | Titulo correto no comprovante. | Bloqueante | Material informa decisao. |
| IMP-021 | Fluxo de caixa deve imprimir motivo quando informado. | Sangria/suprimento. | Motivo aparece no comprovante. | Informativa | Material informa motivo. |
| IMP-022 | Cupom de compra deve usar estrutura equivalente ao cupom de venda, com itens de compra. | Impressao de compra. | Cupom de compra impresso. | Bloqueante | Material informa paridade. |
| IMP-023 | O titulo do cupom de compra no Epros deve identificar compra, nao venda. | Impressao de compra. | Titulo funcional correto. | Bloqueante | Regra correta para o Epros.[^nota1] |
| IMP-024 | Impressao fiscal deve escolher NFC-e quando o modelo fiscal for NFC-e. | XML autorizado. | Canal NFC-e acionado. | Bloqueante | Material informa decisao. |
| IMP-025 | Quando nao houver caixa informado, impressao SAT pode usar XML do movimento fiscal. | Impressao SAT. | SAT impresso a partir do XML. | Bloqueante | Material informa fluxo. |
| IMP-026 | Quando o caixa define tipo de emissao NFC-e, o Epros deve imprimir NFC-e. | Tipo NFC-e. | NFC-e impressa. | Bloqueante | Material informa decisao. |
| IMP-027 | Quando o caixa define tipo de emissao SAT, o Epros deve imprimir SAT. | Tipo SAT. | SAT impresso. | Bloqueante | Material informa decisao. |
| IMP-028 | Quando o tipo for perguntar, UF 23 ou 35 deve direcionar para SAT; demais UFs para NFC-e. | Tipo perguntar. | Canal definido pela UF. | Bloqueante | Material informa regra. |
| IMP-029 | Impressao fiscal deve usar XML de retorno/autorizacao. | NFC-e/SAT. | Documento impresso a partir do XML. | Bloqueante | Material informa XML autorizado. |
| IMP-030 | Impressao fiscal deve cortar papel quando o canal suportar. | NFC-e/SAT. | Papel cortado. | Informativa | Material informa cortar papel true. |
| IMP-031 | Impressao fiscal deve permitir tarja/indicacao de documento cancelado quando informado. | Documento cancelado. | Impressao indica cancelamento. | Bloqueante | Material informa parametro. |
| IMP-032 | Logotipo da empresa deve ser usado quando disponivel. | Cupom ou fiscal com logo. | Logo impresso. | Informativa | Material informa imagem da empresa. |
| IMP-033 | Papel fiscal padrao informado e 80mm. | Impressao fiscal local. | Tipo de papel 80mm. | Alerta | 58mm citado mas nao consolidado. |
| IMP-034 | Erro de impressao deve gerar mensagem operacional e registro de monitoramento. | Falha de canal. | Erro visivel e auditavel. | Bloqueante | Material informa mensagem e monitoramento. |
| IMP-035 | Pos-autorizacao SAT/NFC-e deve acionar impressao quando a configuracao permitir. | Autorizacao fiscal bem-sucedida. | Impressao iniciada. | Bloqueante | Material informa pos-autorizacao. |
| IMP-036 | Canal Android deve usar impressora Bluetooth pareada quando aplicavel. | Mobile Android. | Lista/dispositivo Bluetooth usado. | Bloqueante | Material informa Bluetooth. |
| IMP-037 | Canal Android deve solicitar permissao Bluetooth conforme versao do sistema. | Mobile Android. | Permissao solicitada. | Bloqueante | Material informa permissao. |
| IMP-038 | Canal mobile deve gerar cupom 80mm quando tamanho for 80; caso contrario usar layout alternativo informado. | Mobile. | Layout adequado ao tamanho. | Bloqueante | Material informa 80/60. |
| IMP-039 | Teste de impressao deve emitir cupom de teste com data e documento da empresa quando disponivel. | Teste de configuracao. | Cupom de teste impresso. | Informativa | Material informa teste. |
| IMP-040 | Reimpressao de pedido deve usar dados do pedido original. | Reimpressao. | Pedido reimpresso. | Bloqueante | Material informa reimpressao. |
| IMP-041 | Reimpressao NFC-e deve usar XML original quando disponivel. | Reimpressao NFC-e. | Documento fiscal reimpresso. | Bloqueante | Material informa XML. |
| IMP-042 | Fechamento de caixa deve permitir relatorios de encerramento e variantes operacionais. | Encerramento. | Relatorios impressos/PDF. | Informativa | Material informa 10 relatorios/variantes. |
| IMP-043 | Etiqueta de produto deve conter nome, preco e codigo de barras. | Etiquetagem. | Etiqueta impressa. | Bloqueante | Material informa conteudo. |

## 9. Parametros de configuracao

| Parametro | Finalidade | Tipo/formato | Valor padrao | Obrigatorio | Nivel | Quem pode alterar | Impacto |
|---|---|---|---|---|---|---|---|
| NomeImpressora | Identificar impressora local. | Texto | Nao informado no material | Sim | Dispositivo/Usuario/Empresa | Gestor | Bloqueia impressao se ausente. |
| CanalImpressao | Definir canal: local, fiscal, Bluetooth, relatorio ou etiqueta. | Enum/texto | Nao informado no material | Sim | Dispositivo/Empresa | Gestor | Define mecanismo de impressao. |
| TamanhoPapel | Definir 80mm, 60mm ou outro tamanho suportado. | Numero/enum | 80mm para fiscal local quando aplicavel | Condicional | Dispositivo | Gestor | Altera layout. |
| TipoEmissaoCupom | Definir NFC-e, SAT ou Perguntar. | Enum | Nao informado no material | Condicional | Caixa/Empresa | Gestor/Fiscal | Define impressao fiscal. |
| CortarPapel | Acionar corte automatico quando suportado. | Booleano | Sim para fiscal local | Nao | Dispositivo | Gestor | Impacta finalizacao fisica. |
| ProdutoDuasLinhas | Controlar quebra de produto em duas linhas. | Booleano | Nao | Nao | Dispositivo | Gestor | Impacta layout fiscal. |
| UsarBarrasComoCodigo | Usar codigo de barras como codigo de produto. | Booleano | Nao | Nao | Dispositivo | Gestor | Impacta layout fiscal. |
| ImprimirPedidoAutomatico | Controlar impressao automatica em pagamento/pedido. | Booleano | Nao informado no material | Condicional | Usuario/Dispositivo | Gestor | Aciona impressao automatica. |

## 10. Modelo de dados funcional e implantavel

### 10.1 Visao geral do modelo

O modelo do submodulo organiza configuracao local, canais, templates, solicitacoes de impressao, itens impressos, resultados, historico, reimpressao e erros. O material informa campos operacionais dispersos como nome da impressora, tipo de emissao do cupom, logotipo da empresa, tamanho de papel, XML autorizado, dados de venda, compra, caixa, cozinha e etiqueta. As entidades abaixo consolidam esses dados em estrutura funcional implantavel do Epros.[^nota2]

| Grupo de dados | Entidades/tabelas | Papel funcional | Observacoes |
|---|---|---|---|
| Configuracao | `impressao_configuracao`, `impressao_canal` | Define impressora, canal, tamanho de papel e parametros. | Derivado de campos comprovados. |
| Templates/layouts | `impressao_template` | Controla tipo de documento e layout. | Criado para organizar layouts citados.[^nota2] |
| Movimentos | `impressao_solicitacao`, `impressao_item`, `impressao_resultado` | Registra cada tentativa de impressao. | Necessario para auditoria. |
| Fiscal | `impressao_fiscal_xml` | Referencia XML fiscal autorizado e status de cancelamento. | Transmissao fora do escopo. |
| Auditoria | `impressao_historico`, `impressao_erro` | Registra alteracoes, falhas e reimpressao. | Material informa erro e monitoramento. |

### 10.2 Entidades, finalidade e cardinalidade

| Entidade | Finalidade | Cardinalidade | Observacao |
|---|---|---|---|
| `impressao_configuracao` | Guardar impressora e parametros por contexto. | 0..N por empresa/dispositivo | Inclui nome e tamanho. |
| `impressao_canal` | Classificar canal de impressao. | 1..N por configuracao | Local, fiscal, Bluetooth, relatorio, etiqueta. |
| `impressao_template` | Representar layout de cupom, comanda, caixa, fiscal, fechamento ou etiqueta. | 0..N | Estrutura funcional criada.[^nota2] |
| `impressao_solicitacao` | Registrar uma tentativa de impressao. | 1 por tentativa | Principal movimento do submodulo. |
| `impressao_item` | Registrar linhas ou itens impressos. | 0..N por solicitacao | Usado para venda, compra, cozinha e etiqueta. |
| `impressao_fiscal_xml` | Guardar referencia ao XML usado para NFC-e/SAT. | 0..1 por solicitacao fiscal | XML pertence ao fiscal; IMP referencia. |
| `impressao_resultado` | Registrar sucesso, falha, mensagem e reimpressao. | 1..N por solicitacao | Permite historico de tentativas. |
| `impressao_erro` | Detalhar erro de impressao. | 0..N por resultado | Usado para monitoramento. |
| `impressao_historico` | Auditar mudancas de configuracao e eventos. | 0..N por configuracao/solicitacao | Criado para rastreabilidade.[^nota2] |

### 10.3 Relacionamentos

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| Empresa/Dispositivo | possui | `impressao_configuracao` | Cada contexto pode ter configuracao propria. |
| `impressao_configuracao` | usa | `impressao_canal` | Canal define capacidade. |
| `impressao_solicitacao` | usa | `impressao_configuracao` | Toda impressao deve conhecer configuracao usada. |
| `impressao_solicitacao` | usa | `impressao_template` | Tipo de documento define layout. |
| `impressao_solicitacao` | possui | `impressao_item` | Itens compoem o documento impresso. |
| `impressao_solicitacao` | possui | `impressao_resultado` | Cada tentativa gera resultado. |
| `impressao_resultado` | possui | `impressao_erro` | Erros detalham falhas. |
| `impressao_solicitacao` | referencia | `impressao_fiscal_xml` | Apenas para NFC-e/SAT. |

## 11. Dicionario de dados implantavel

### 11.1 `impressao_configuracao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da configuracao. |
| EmpresaId | Identificador | Nao informado no material | Nao informado no material | Empresa | Empresa dona da configuracao. |
| DispositivoId | Identificador/texto | Nao informado no material | Nao informado no material | Dispositivo | Contexto local. |
| UsuarioId | Identificador | Nao informado no material | Nao informado no material | Usuario | Usuario da configuracao quando aplicavel. |
| NomeImpressora | Texto | Nao informado no material | Sim | Impressora | Campo comprovado. |
| TamanhoPapel | Enum/numero | 80mm, 60mm ou Nao informado no material | Condicional | Layout | Obrigatorio quando canal exige. |
| TipoEmissaoCupom | Enum/texto | NFC-e, SAT, Perguntar | Condicional | Fiscal/Caixa | Campo comprovado. |
| ImprimirPedidoAutomatico | Booleano | Sim/Nao | Nao informado no material | Parametro | Campo comprovado no app. |
| Ativo | Booleano | Sim/Nao | Sim | Status | Criado para controle operacional.[^nota2] |

### 11.2 `impressao_canal`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do canal. |
| Codigo | Texto | Local, FiscalEscPos, Bluetooth, Relatorio, Etiqueta | Sim | Codigo | Dominios consolidados do material.[^nota2] |
| Descricao | Texto | Nao informado no material | Nao | Descritivo | Descricao do canal. |
| SuportaCortePapel | Booleano | Sim/Nao | Nao informado no material | Capacidade | Fiscal local suporta corte quando informado. |
| SuportaLogo | Booleano | Sim/Nao | Nao informado no material | Capacidade | Logo usado quando disponivel. |
| Ativo | Booleano | Sim/Nao | Sim | Status | Controle operacional. |

### 11.3 `impressao_template`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do template. |
| TipoDocumento | Enum/texto | VendaNaoFiscal, CompraNaoFiscal, Cozinha, AberturaCaixa, FluxoCaixa, NFCe, SAT, FechamentoCaixa, Etiqueta, Teste | Sim | Tipo | Tipos comprovados. |
| Nome | Texto | Nao informado no material | Sim | Nome | Nome funcional do layout. |
| ColunasCondensadas | Numero | 46 | Condicional | Layout | Valor informado para cupom nao fiscal. |
| ColunasNormal | Numero | 34 | Condicional | Layout | Valor informado para cupom nao fiscal. |
| Ativo | Booleano | Sim/Nao | Sim | Status | Controla uso. |

### 11.4 `impressao_solicitacao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da solicitacao. |
| ConfiguracaoId | Identificador | Nao informado no material | Sim | `impressao_configuracao` | Configuracao usada. |
| TemplateId | Identificador | Nao informado no material | Sim | `impressao_template` | Layout usado. |
| OrigemModulo | Texto | Vendas, Compras, Caixa, Fiscal, Cozinha, Estoque, Nao informado no material | Sim | Origem | Origem da impressao. |
| OrigemId | Identificador/texto | Nao informado no material | Nao informado no material | Documento origem | Venda, compra, caixa, produto etc. |
| TipoDocumento | Enum/texto | Mesmo dominio do template | Sim | Tipo | Tipo impresso. |
| UsuarioId | Identificador | Nao informado no material | Nao informado no material | Usuario | Usuario solicitante. |
| StatusSolicitacao | Enum/texto | Pendente, Impressa, Falha, Bloqueada, Nao informado no material | Sim | Status | Dominio funcional criado.[^nota2] |
| CriadoEm | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data da solicitacao. |

### 11.5 `impressao_item`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do item. |
| SolicitacaoId | Identificador | Nao informado no material | Sim | `impressao_solicitacao` | Solicitacao relacionada. |
| CodigoProduto | Texto | Nao informado no material | Condicional | Produto | Usado em cupom e etiqueta. |
| Descricao | Texto | Nao informado no material | Sim | Item | Descricao impressa. |
| Quantidade | Decimal | Nao informado no material | Condicional | Item | Usado em venda/compra/cozinha. |
| Unidade | Texto | Nao informado no material | Condicional | Unidade | Unidade impressa. |
| ValorUnitario | Decimal | Nao informado no material | Condicional | Valor | Cupom venda/compra. |
| ValorDesconto | Decimal | Nao informado no material | Nao | Valor | Desconto funcional correto. |
| ValorAcrescimo | Decimal | Nao informado no material | Nao | Valor | Acrescimo quando maior que zero. |
| ValorTotalItem | Decimal | Nao informado no material | Condicional | Valor | Total do item. |
| DadosAdicionais | Texto | Nao informado no material | Nao | Observacao | Cozinha/venda. |
| Preparado | Booleano | Sim/Nao | Condicional | Cozinha | Apenas preparados imprimem na cozinha. |

### 11.6 `impressao_fiscal_xml`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do XML referenciado. |
| SolicitacaoId | Identificador | Nao informado no material | Sim | `impressao_solicitacao` | Solicitacao fiscal. |
| ModeloFiscal | Enum/texto | NFC-e, SAT | Sim | Fiscal | Modelos comprovados. |
| XmlAutorizado | Texto/binario | Nao informado no material | Sim | Fiscal | XML autorizado usado para impressao. |
| DocumentoCancelado | Booleano | Sim/Nao | Nao | Fiscal | Indica tarja/estado de cancelamento. |
| Uf | Codigo UF | 2 digitos quando informado | Condicional | Fiscal | Usada para decisao SAT/NFC-e quando tipo Perguntar. |

### 11.7 `impressao_resultado`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do resultado. |
| SolicitacaoId | Identificador | Nao informado no material | Sim | `impressao_solicitacao` | Solicitacao relacionada. |
| StatusResultado | Enum/texto | Sucesso, Falha, Bloqueada, Nao informado no material | Sim | Status | Resultado da tentativa. |
| Mensagem | Texto | Nao informado no material | Nao | Mensagem | Mensagem operacional. |
| ImpressoraUsada | Texto | Nao informado no material | Nao informado no material | Impressora | Nome efetivamente usado. |
| Reimpressao | Booleano | Sim/Nao | Sim | Controle | Indica reimpressao. |
| CriadoEm | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data da tentativa. |

### 11.8 `impressao_erro`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do erro. |
| ResultadoId | Identificador | Nao informado no material | Sim | `impressao_resultado` | Resultado com falha. |
| TipoErro | Enum/texto | ImpressoraAusente, ConfiguracaoIncompleta, XmlAusente, CanalIndisponivel, FalhaImpressao, Nao informado no material | Sim | Tipo | Dominio criado para controle.[^nota2] |
| Mensagem | Texto | Nao informado no material | Sim | Mensagem | Erro exibido/registrado. |
| Monitorado | Booleano | Sim/Nao | Nao informado no material | Monitoramento | Material informa monitoramento. |

### 11.9 `impressao_historico`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do historico. |
| ConfiguracaoId | Identificador | Nao informado no material | Condicional | `impressao_configuracao` | Historico de configuracao. |
| SolicitacaoId | Identificador | Nao informado no material | Condicional | `impressao_solicitacao` | Historico de impressao. |
| Acao | Texto | Nao informado no material | Sim | Acao | Alteracao, impressao, reimpressao, erro. |
| UsuarioId | Identificador | Nao informado no material | Nao informado no material | Usuario | Usuario responsavel. |
| PayloadJson | JSON | Nao informado no material | Nao informado no material | Auditoria | Dados da acao. |
| CriadoEm | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data da acao. |

## 12. Integracoes e fronteiras

| Origem/Destino | Tipo | Dados trocados | Regra |
|---|---|---|---|
| Vendas/PDV | Entrada | Venda, itens, pagamentos, troco, cliente, entrega e observacoes. | IMP imprime; venda e dona do processo comercial. |
| Compras | Entrada | Compra, itens e totais. | IMP imprime cupom de compra; compra e dona do processo. |
| Caixa | Entrada | Abertura, sangria, suprimento e fechamento. | IMP imprime comprovantes e relatorios. |
| Faturamento Fiscal Eletronico | Entrada | XML autorizado, modelo fiscal, cancelamento, UF. | IMP imprime; transmissao fica no fiscal. |
| Cozinha/Comanda | Entrada | Pedido, item preparado, comanda e responsavel. | IMP imprime layout; fluxo operacional fica no PDV/Vendas. |
| Estoque/Produtos | Entrada | Produto, preco e codigo de barras. | IMP imprime etiqueta. |
| Relatorios | Saida/Entrada | Fechamento em formato imprimivel/PDF. | Agregacao analitica fica em Relatorios/Analytics. |
| Aplicativo | Entrada/Saida | Configuracao local, Bluetooth e impressora instalada. | Sincronismo final fica na MC. |

## 13. Telas, comandos e relatorios

| ID | Tela/Comando | Especificacao |
|---|---|---|
| TEL-IMP-001 | Configuracao de impressora PDV | Informar nome da impressora, salvar configuracao e carregar para runtime. |
| TEL-IMP-002 | Login/carregamento de impressora | Carregar impressora configurada no inicio da operacao. |
| TEL-IMP-003 | Venda PDV cupom nao fiscal | Finalizar venda e imprimir cupom nao fiscal. |
| TEL-IMP-004 | Cozinha comanda termica | Imprimir item preparado para cozinha. |
| TEL-IMP-005 | Abertura de caixa | Imprimir comprovante de abertura. |
| TEL-IMP-006 | Sangria/suprimento | Imprimir comprovante de fluxo de caixa. |
| TEL-IMP-007 | Compra cupom nao fiscal | Imprimir cupom operacional de compra. |
| TEL-IMP-008 | Impressao fiscal SAT/NFC-e | Imprimir documento pos-autorizacao a partir de XML. |
| TEL-IMP-009 | Encerramento de caixa | Imprimir relatorios de encerramento e variantes. |
| TEL-IMP-010 | Etiqueta de produto | Imprimir nome, preco e codigo de barras. |
| TEL-IMP-011 | Configuracao mobile/desktop | Listar impressoras ou dispositivos e testar impressao. |
| TEL-IMP-012 | Pagamento mobile/desktop | Imprimir pedido conforme canal configurado. |
| TEL-IMP-013 | Resumo de caixa | Imprimir fechamento/finalizadora. |
| TEL-IMP-014 | Reimpressao NFC-e | Reimprimir usando XML original quando disponivel. |

## 14. Cenarios de validacao

| ID | Cenario | Resultado esperado |
|---|---|---|
| CT-IMP-001 | Salvar nome da impressora. | Configuracao carregada para operacao. |
| CT-IMP-002 | Operar sem impressora configurada. | Bloqueio ou alerta funcional. |
| CT-IMP-003 | Reiniciar aplicacao depois de configurar impressora. | Configuracao persiste. |
| CT-IMP-010 | Imprimir venda com impressora valida. | Cupom nao fiscal impresso. |
| CT-IMP-011 | Imprimir com canal Bematech. | Canal correto usado. |
| CT-IMP-012 | Imprimir compra. | Cupom de compra impresso com layout proprio. |
| CT-IMP-020 | Imprimir cozinha para item preparado. | Comanda impressa. |
| CT-IMP-021 | Imprimir abertura de caixa. | Comprovante emitido. |
| CT-IMP-022 | Imprimir sangria/suprimento. | Comprovante emitido. |
| CT-IMP-030 | Imprimir NFC-e pos-autorizacao. | Documento fiscal impresso. |
| CT-IMP-031 | Imprimir SAT com papel 80mm. | SAT impresso. |
| CT-IMP-040 | Imprimir via Bluetooth com permissao. | Impressao enviada. |
| CT-IMP-041 | Imprimir pedido em desktop. | Saida fisica emitida. |
| CT-IMP-050 | Imprimir fechamento de caixa. | Relatorio/PDF emitido. |

## 15. Indicadores e controles

| Indicador | Descricao |
|---|---|
| Impressoes por tipo | Quantidade por venda, compra, cozinha, caixa, fiscal, etiqueta e fechamento. |
| Falhas por canal | Falhas agrupadas por impressora/canal. |
| Reimpressoes | Quantidade de reimpressoes por origem. |
| Configuracoes incompletas | Dispositivos sem impressora ou tamanho de papel obrigatorio. |
| Tempo ate impressao | Intervalo entre solicitacao e resultado. |

## 16. Seguranca, auditoria e conformidade

| Area | Regra funcional |
|---|---|
| Permissao | Configurar impressora, reimprimir fiscal e imprimir fechamento devem respeitar permissoes. |
| Auditoria | Configuracao, impressao, reimpressao e falha devem registrar usuario, horario e origem. |
| Fiscal | IMP nao transmite documento; apenas imprime documento autorizado. |
| Dados pessoais | CPF/documento do consumidor deve respeitar politicas de privacidade. |
| Retencao | Resultado de impressao e erro devem seguir retencao definida pela Siser. |

## 17. Matriz de rastreabilidade funcional

| Capacidade | Regras | Dados | Testes |
|---|---|---|---|
| Configuracao | IMP-001 a IMP-004 | `impressao_configuracao`, `impressao_canal` | CT-IMP-001 a CT-IMP-003 |
| Venda | IMP-005 a IMP-016 | `impressao_solicitacao`, `impressao_item` | CT-IMP-010, CT-IMP-011 |
| Cozinha | IMP-017, IMP-018 | `impressao_item` | CT-IMP-020 |
| Caixa | IMP-019 a IMP-021, IMP-042 | `impressao_solicitacao`, `impressao_resultado` | CT-IMP-021, CT-IMP-022, CT-IMP-050 |
| Compra | IMP-022, IMP-023 | `impressao_item` | CT-IMP-012 |
| Fiscal | IMP-024 a IMP-035, IMP-041 | `impressao_fiscal_xml` | CT-IMP-030, CT-IMP-031 |
| Mobile/desktop | IMP-036 a IMP-040 | `impressao_configuracao` | CT-IMP-040, CT-IMP-041 |
| Etiqueta | IMP-043 | `impressao_item` | Nao informado no material |

## 18. Criterios de aceite

| Criterio | Resultado esperado |
|---|---|
| Configuracao | Impressao nao ocorre sem impressora quando o canal exigir. |
| Cupom venda | Layout inclui empresa, itens, totais, pagamentos, troco, consumidor e rodape. |
| Cupom compra | Layout identifica compra corretamente. |
| Cozinha | Somente itens preparados sao impressos. |
| Caixa | Abertura, sangria e suprimento geram comprovante com assinatura. |
| Fiscal | NFC-e/SAT imprimem somente com XML autorizado. |
| Erros | Falhas geram mensagem e registro auditavel. |
| Modelo de dados | Entidades, campos, obrigatoriedade e lacunas estao documentados. |
| Ausencia de invencao | O que nao esta informado no material aparece como `Nao informado no material` ou item da MC. |

## 19. Notas de rodape

[^nota1]: O material registra comportamentos que nao devem ser perpetuados como regra funcional do Epros, como desconto de item usando campo de acrescimo e cupom de compra com titulo de venda. Nesta EF eles foram convertidos para a regra funcional correta do Epros.
[^nota2]: As entidades `impressao_configuracao`, `impressao_canal`, `impressao_template`, `impressao_solicitacao`, `impressao_item`, `impressao_fiscal_xml`, `impressao_resultado`, `impressao_erro` e `impressao_historico` foram criadas nesta especificacao para consolidar em modelo implantavel os campos e processos comprovados no material, que estavam distribuidos entre configuracao local, venda, compra, caixa, fiscal, cozinha, aplicativo e etiquetas.
