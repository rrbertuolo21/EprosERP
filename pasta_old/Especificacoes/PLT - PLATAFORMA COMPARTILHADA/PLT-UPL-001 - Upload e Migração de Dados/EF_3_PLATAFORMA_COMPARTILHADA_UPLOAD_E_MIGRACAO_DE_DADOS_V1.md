# EF 3 - PLATAFORMA COMPARTILHADA / UPLOAD E MIGRACAO DE DADOS V1

## 1. Controle do documento

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | UPLOAD_E_MIGRACAO_DE_DADOS |
| Versao | V1 |
| Data | 2026-06-11 |
| Status | Concluido |
| Conteudo analisado | 12 documentos canonicos do submodulo |

## 2. Objetivo funcional

O submodulo Upload e Migracao de Dados define a camada do Epros para receber arquivos, validar conteudo, processar importacoes, exportar dados, acompanhar jobs assincronos, registrar erros, permitir desfazer importacoes quando suportado, executar cargas por URL remota, controlar arquivos temporarios e orquestrar atualizacoes incrementais de estrutura de dados.

O submodulo tambem padroniza a importacao fiscal por XML, separando staging, processamento, cadastro automatico, status de PDF, vinculacao com empresa e resultados de processamento.

## 3. Escopo funcional

| Area | Descricao |
|---|---|
| Upload web direto | Recebimento de arquivo por formulario, validacao, storage temporario ou definitivo e resposta estruturada. |
| Upload por URL remota | Registro de URL, fila em segundo plano, progresso, cancelamento e arquivo gerado ao concluir. |
| Upload por API | Recebimento de arquivo por chave de acesso funcional e execucao em nome do usuario autorizado. |
| Upload em partes | Recebimento por faixa de bytes, montagem progressiva, descarte de partes antigas e validacao de tamanho. |
| Importacao CSV/XLSX | Upload temporario, validacao, mapeamento, importacao por linha, resultado parcial e log de erros. |
| Importacao fiscal XML | Recebimento de XML fiscal, classificacao de tipo, processamento, cadastro e salvamento de PDF. |
| Exportacao | Geracao de arquivo XLSX com filtros atuais, campos selecionados e download posterior. |
| Wizard de importacao | Etapas de upload, mapeamento, validacao, duplicidade, confirmacao, importacao, resultado e undo quando disponivel. |
| Migracao offline de arquivos | Carga pontual de arquivos a partir de pasta controlada para repositorio do Epros. |
| Atualizacao incremental | Controle de versoes, blocos de atualizacao, idempotencia por arquivo/bloco e log por empresa. |
| Auditoria e erros | Trilha por execucao, logs por linha, payload de falha, status e usuario. |
| Integracoes | Cadastros, fiscal, estoque, vendas, compras, financeiro, GED, configuracao, seguranca e workflow. |

## 4. Fora de escopo

| Item | Tratamento |
|---|---|
| CRUD completo das entidades importadas | Permanece no modulo dono da entidade. |
| OFX bancario | Deve permanecer em Tesouraria. |
| Regras completas de documento fiscal eletronico | Permanecem em Faturamento Fiscal Eletronico; este submodulo controla importacao e staging. |
| Tabela final de arquivos do GED | O submodulo referencia anexos/arquivos; detalhe documental definitivo fica no GED. |
| Motor final de seguranca de hashes bloqueados | Deve ser definido em seguranca/compliance. |
| Editor tecnico de scripts | O Epros deve controlar execucao e historico; definicao de governanca tecnica fica pendente na MC. |

## 5. Atores e responsabilidades

| Ator | Responsabilidades |
|---|---|
| Usuario importador | Enviar arquivo, escolher tipo de importacao, preencher parametros e acompanhar resultado. |
| Usuario aprovador | Aprovar registros ou execucoes quando o workflow exigir. |
| Administrador | Parametrizar limites, extensoes, regras de duplicidade, jobs e atualizacoes. |
| Processo de fila | Executar importacoes, uploads remotos, exports e atualizacoes em segundo plano. |
| Usuario API | Enviar arquivo por integracao autenticada. |
| Auditor | Consultar logs, erros, status, arquivos e historico. |

## 6. Conceitos funcionais

| Conceito | Definicao |
|---|---|
| Execucao de upload | Registro que controla o recebimento de arquivo, origem, status e destino. |
| Execucao de importacao | Registro que controla validacao, leitura, linhas aprovadas, linhas ignoradas e erros. |
| Arquivo temporario | Arquivo recebido e ainda nao consolidado no repositorio definitivo. |
| Import ref | Identificador unico de uma transacao de importacao. |
| Linha invalida | Linha que falha na validacao e pode ser ignorada sem abortar o lote quando a regra permitir. |
| Log de importacao | Registro de erros por execucao, atributo, mensagem e referencia. |
| Resultado parcial | Resultado com parte das linhas importadas e parte rejeitada ou ignorada. |
| Upload remoto | Carga iniciada por URL externa, com progresso e fila. |
| Bloco de atualizacao | Unidade idempotente de alteracao estrutural ou operacional aplicada uma vez. |
| Undo de importacao | Desfazimento dos registros criados em uma execucao quando suportado. |

## 7. Capacidades funcionais

### 7.1 Upload direto

1. O Epros deve receber arquivos por formulario com parametro multipart padrao.
2. O upload deve validar extensoes permitidas.
3. O upload deve validar tipos bloqueados.
4. O upload deve validar palavras bloqueadas no nome do arquivo.
5. O upload deve validar tamanho maximo permitido.
6. O upload deve validar tamanho minimo permitido.
7. O upload deve validar quantidade maxima de arquivos quando configurada.
8. O upload deve bloquear arquivos por hash quando a lista de bloqueio estiver disponivel.
9. O upload deve impedir arquivo vazio quando o canal exigir.
10. O usuario com autorizacao administrativa pode ignorar bloqueio global de uploads quando parametrizado.
11. Ao receber arquivo sem nome valido, o Epros deve atribuir nome funcional baseado em data/hora.
12. Nomes iniciados por ponto devem ser saneados para evitar arquivo oculto.
13. Se o arquivo final apresentar divergencia de tamanho, o Epros deve descartar o arquivo quando configurado.
14. A resposta deve informar sucesso ou erro de forma estruturada.

### 7.2 Upload em partes

1. O Epros deve aceitar cabecalho de faixa de bytes para upload em partes.
2. A primeira parte deve limpar parte temporaria anterior do mesmo arquivo.
3. Partes parciais devem aguardar as proximas partes sem consolidar o arquivo.
4. Partes antigas com mais de 3 dias devem ser removidas.
5. O diretorio temporario de partes deve ser criado quando necessario.
6. Ao concluir o tamanho esperado, o Epros deve consolidar o arquivo e seguir para storage.

### 7.3 Storage e deduplicacao

1. O Epros deve registrar arquivo com dono, usuario que enviou, origem, nome, tamanho, hash e data.
2. O usuario dono do arquivo pode ser diferente do usuario que realizou upload em compartilhamento.
3. O hash do arquivo deve permitir reaproveitamento quando existir arquivo ativo com mesmo hash e tamanho.
4. O nome original deve ser tornado unico quando ja existir arquivo ativo com mesmo nome no destino.
5. O arquivo deve receber identificador tecnico opaco para armazenamento.
6. O Epros deve atualizar estatisticas de storage apos inserir o arquivo.
7. O tamanho da pasta deve ser atualizado quando o arquivo tiver pasta de destino.
8. O destino pode ser armazenamento local, servidor disponivel ou provedor externo quando configurado.
9. Falha no storage deve impedir a consolidacao do upload.

### 7.4 Upload por URL remota

1. O usuario pode informar URL remota valida.
2. O Epros deve obter metadados da URL antes do download quando possivel.
3. O download remoto deve controlar tamanho total, tamanho baixado e percentual.
4. O progresso deve ser atualizado em fila propria.
5. O job remoto pode assumir status pendente, processando, baixando, completo, falho ou cancelado.
6. Ao concluir, o job deve criar referencia ao arquivo gerado.
7. O usuario pode consultar filas remotas existentes.
8. O usuario pode remover item de fila remota quando permitido.
9. URL remota em segundo plano deve evitar duplicidade de item pendente equivalente.

### 7.5 Upload por API

1. O upload por API exige chave de acesso valida.
2. O Epros deve identificar o usuario associado a chave antes de processar o arquivo.
3. O canal API pode aceitar limite de arquivo distinto conforme ambiente.
4. O canal API pode atualizar conteudo de arquivo existente quando a acao autorizada for enviada.
5. A origem do upload deve ser registrada como API.

### 7.6 Importacao CSV/XLSX

1. O Epros deve aceitar arquivos CSV e XLSX para importacao tabular quando o fluxo estiver habilitado.
2. O upload inicial deve armazenar o arquivo em area temporaria identificada por chave unica.
3. A resposta do upload deve retornar chave unica, nome, extensao e tamanho humanizado.
4. Arquivo com tipo nao permitido deve ser rejeitado com status de conflito funcional.
5. A validacao da importacao exige nome do arquivo e chave unica.
6. A validacao deve verificar existencia do arquivo temporario.
7. Ao iniciar importacao, o Epros deve criar identificador unico da transacao.
8. O identificador da transacao deve ser propagado para as linhas importadas.
9. O processamento deve considerar cabecalho na primeira linha.
10. A primeira linha de dados deve iniciar na linha 2.
11. Colunas podem ser normalizadas para cabecalhos simples e minusculos.
12. Linhas invalidas podem ser ignoradas sem abortar o lote quando o tipo de importacao suportar falha por linha.
13. Falhas por linha devem alimentar contagem de erros.
14. O retorno deve informar quantidade com sucesso, quantidade ignorada, quantidade de erros e referencia do erro.
15. O log de erro deve ser consultavel pela referencia da importacao.
16. O log de erro deve apresentar linha, atributo e mensagem.
17. O resultado da importacao deve indicar passed, partial, failed ou nothing.
18. O arquivo temporario nao deve ser apagado automaticamente sem politica definida.

### 7.7 Importacao de leads, clientes, projetos e itens

1. Importacao de oportunidades exige status informado e existente no catalogo do modulo.
2. O titulo da oportunidade e obrigatorio.
3. Campos customizados de oportunidade devem aceitar mapeamento de 1 a 150.
4. A oportunidade deve receber identificador da importacao.
5. O status da oportunidade deve vir do parametro informado.
6. A criacao da oportunidade deve registrar usuario criador e data de criacao.
7. A regra de duplicidade de oportunidade pode considerar nome, email, telefone e empresa conforme parametros.
8. Linha duplicada deve ser ignorada e contabilizada como ignorada.
9. Apos importar oportunidades, o Epros deve processar atribuicoes e eventos.
10. Se o usuario nao puder atribuir livremente, a atribuicao deve incluir o proprio usuario.
11. Importacao de clientes deve aplicar regras de duplicidade por email, telefone e empresa conforme parametros.
12. Apos importar clientes, o Epros pode criar usuarios/contatos relacionados.
13. Importacao de projetos deve processar usuarios atribuidos informados em CSV.
14. O projeto deve receber identificador da importacao.
15. Importacao de itens deve validar colunas obrigatorias do item.
16. A importacao deve emitir eventos antes e depois da gravacao quando o fluxo suportar.

### 7.8 Exportacao

1. Exportacao deve exigir autenticacao e permissao de visualizacao do modulo.
2. Exportacao deve ser acionada por POST.
3. Exportacao nao deve gravar dados de negocio, apenas arquivo temporario.
4. Exportacao deve usar os filtros atuais da listagem.
5. Exportacao deve permitir selecao de campos padrao.
6. Exportacao deve permitir selecao de campos customizados quando existirem.
7. A lista exportada deve ser gerada sem paginacao.
8. A resposta deve retornar URL de download posterior.
9. Falha de exportacao deve retornar erro funcional.
10. Exportacoes previstas incluem clientes, oportunidades, projetos, faturas, estimativas, pagamentos, despesas, apontamentos de horas, itens, tarefas e atendimentos.
11. O nome do arquivo exportado deve refletir a entidade exportada.
12. Colunas exportadas nao devem ser inventadas alem das selecoes padrao e customizadas existentes.

### 7.9 Wizard de importacao

1. O Epros deve suportar fluxo por etapas para importacao.
2. As etapas esperadas incluem upload, mapeamento, validacao, deduplicacao, confirmacao, importacao, resultado e erro.
3. O mapeamento salvo deve poder ser reaproveitado.
4. Fontes externas devem ser tratadas por adaptador separado.
5. A execucao deve possuir checkpoint idempotente.
6. Erros devem ser tratados de forma centralizada.
7. O undo deve desfazer registros criados pela execucao quando a entidade e a origem suportarem a acao.
8. O usuario deve poder visualizar ultimo resultado quando disponivel.

### 7.10 Importacao fiscal XML

1. O Epros deve registrar XML importado com empresa, conteudo, tipo, chave, status de importacao, status de cadastro, status de PDF, codigo fiscal e tipo de evento.
2. Tipos previstos de XML: nao aplicavel, nota fiscal de entrada, nota fiscal de saida, nota fiscal de entrada propria e nota fiscal de cancelamento.
3. Status de processamento previstos: nao processado, processando, finalizado e erro.
4. A lista de XMLs deve rejeitar resultado vazio com mensagem funcional.
5. A empresa deve existir para que a importacao seja processada.
6. XML vazio ou invalido deve ser rejeitado.
7. Documento do emitente deve ser compativel com a empresa quando a regra do tipo exigir.
8. Documento do destinatario deve ser compativel com a empresa quando a regra do tipo exigir.
9. O processamento deve executar cadastro de pessoas e veiculos quando necessario.
10. O processamento deve cadastrar produtos quando necessario.
11. O processamento deve cadastrar unidades de medida quando necessario.
12. O processamento deve cadastrar tributacao por NCM quando necessario.
13. XML de saida deve gerar venda, contas a receber e documento fiscal quando aplicavel.
14. XML de entrada deve gerar compra, contas a pagar e documento fiscal quando aplicavel.
15. Falha transacional deve desfazer entidades geradas no lote.
16. Nenhuma venda processada com sucesso deve gerar erro de lote de saida.
17. Nenhuma compra processada com sucesso deve gerar erro de lote de entrada.
18. Falha de comunicacao fiscal por tempo esgotado deve ser registrada como erro funcional.
19. Atualizacao do status de PDF deve registrar erro proprio quando falhar.
20. Atualizacao do status de cadastro deve registrar erro proprio quando falhar.

### 7.11 Arquivo de importacao XML de saida

1. O Epros deve registrar arquivo de lote de XML de saida.
2. O arquivo deve possuir nome, quantidade de XMLs, invalidos, produtos localizados, clientes localizados, produtos importados, clientes importados, mensagem de erro e status.
3. Status previstos: verificando, processando, finalizado e erro.
4. Todos os contadores informados no material sao obrigatorios.
5. Mensagem de erro e obrigatoria no material e deve ser validada com negocio se pode ser vazia em sucesso.

### 7.12 Atualizacao incremental de schema e dados tecnicos

1. O Epros deve manter versao atual incremental.
2. Cada atualizacao deve possuir versao alvo.
3. Cada bloco de atualizacao deve ser isolado.
4. O Epros deve registrar arquivo ou bloco aplicado para garantir idempotencia.
5. Atualizacao ja registrada nao deve ser reaplicada.
6. Atualizacao pode ser executada por empresa ou por ambiente central, conforme escopo.
7. Atualizacao em lote de empresas deve controlar status atual, versao atual, versao alvo e log.
8. O processamento em lote deve limitar quantidade de empresas por ciclo quando parametrizado.
9. O status da atualizacao deve assumir novo, processando, falho ou concluido.
10. Falha deve gravar log e manter status falho.
11. Sucesso deve gravar log e atualizar versao.
12. Arquivo de patch deve ser aplicado a todas as empresas elegiveis quando utilizado.
13. Remocao de patch apos execucao parcial deve ser tratada como lacuna de seguranca operacional.
14. Funcao tecnica declarada em job deve existir; se nao existir, o job deve falhar.
15. Job sem funcao informada deve falhar.
16. Cache tecnico pode ser limpo apos atualizacao quando aplicavel.

### 7.13 Migracao offline de arquivos

1. O Epros deve permitir migracao pontual de arquivos a partir de pasta controlada.
2. A migracao offline nao deve usar o mesmo fluxo de upload web quando o objetivo for carga massiva.
3. A migracao offline deve copiar arquivos para o repositorio do Epros sem mover a origem, quando esse for o modo configurado.
4. A conta destino deve ser definida antes da execucao.
5. A pasta inicial destino deve ser definida antes da execucao.
6. A ferramenta de migracao offline deve ser removida ou desativada apos uso quando for recurso temporario.

## 8. Fluxos funcionais

### 8.1 Upload direto

| Passo | Acao | Resultado |
|---|---|---|
| 1 | Usuario seleciona arquivo | Epros recebe arquivo e metadados. |
| 2 | Epros valida extensao, tamanho, bloqueios e quota | Arquivo aprovado segue para storage; rejeitado retorna erro. |
| 3 | Epros trata nome e destino | Nome unico, hash e caminho opaco sao definidos. |
| 4 | Epros grava arquivo | Registro de arquivo e estatisticas sao atualizados. |
| 5 | Epros responde | Usuario recebe sucesso ou erro estruturado. |

### 8.2 Importacao tabular

| Passo | Acao | Resultado |
|---|---|---|
| 1 | Usuario envia CSV/XLSX | Arquivo e salvo em area temporaria. |
| 2 | Usuario informa parametros | Epros valida arquivo, entidade e dados obrigatorios. |
| 3 | Epros cria import ref | Execucao passa a ter identificador unico. |
| 4 | Epros le linhas a partir da linha 2 | Linhas validas sao importadas; invalidas podem ser ignoradas. |
| 5 | Epros gera log | Falhas ficam consultaveis por referencia. |
| 6 | Epros apresenta resultado | Status final: passed, partial, failed ou nothing. |

### 8.3 Importacao fiscal XML

| Passo | Acao | Resultado |
|---|---|---|
| 1 | Usuario envia XML | Epros grava staging do XML. |
| 2 | Epros valida empresa e XML | XML invalido ou empresa ausente gera erro. |
| 3 | Epros classifica tipo | Entrada, saida, entrada propria, cancelamento ou nao aplicavel. |
| 4 | Epros processa cadastros | Pessoas, veiculos, produtos, unidades e NCM quando necessario. |
| 5 | Epros gera entidades de destino | Venda/CR ou compra/CP e documento fiscal conforme tipo. |
| 6 | Epros atualiza status | Importacao, cadastro e PDF sao atualizados separadamente. |

### 8.4 Upload remoto

| Passo | Acao | Resultado |
|---|---|---|
| 1 | Usuario informa URL | Epros valida URL. |
| 2 | Epros cria fila | Job inicia como pendente. |
| 3 | Processo baixa arquivo | Progresso atualiza tamanho e percentual. |
| 4 | Download conclui | Arquivo e registrado e job fica completo. |
| 5 | Falha/cancelamento | Job registra status e motivo. |

### 8.5 Atualizacao incremental

| Passo | Acao | Resultado |
|---|---|---|
| 1 | Processo detecta versao alvo | Epros identifica blocos pendentes. |
| 2 | Epros verifica idempotencia | Blocos ja aplicados sao ignorados. |
| 3 | Epros aplica bloco | Sucesso registra bloco; falha registra log e status. |
| 4 | Epros atualiza versao | Empresa ou ambiente recebe versao final. |
| 5 | Epros limpa cache quando aplicavel | Ambiente fica alinhado ao novo estado. |

## 9. Estados funcionais

| Entidade | Estados |
|---|---|
| Registro controlado | Rascunho, EmAnalise, Ativo, Inativo, Encerrado |
| Upload | Recebido, Validando, Armazenando, Concluido, Erro, Cancelado |
| Upload remoto | Pendente, Processando, Baixando, Completo, Falho, Cancelado |
| Importacao | Criada, Validando, Processando, Parcial, Finalizada, Erro, Desfeita |
| Importacao XML | Nao processado, Processando, Finalizado, Erro |
| Arquivo XML de saida | Verificando, Processando, Finalizado, Erro |
| Exportacao | Solicitada, Gerando, Disponivel, Erro, Expirada |
| Atualizacao incremental | Novo, Processando, Concluido, Falho |
| Bloco de atualizacao | Pendente, Aplicado, Ignorado, Falho |

## 10. Regras de permissao

| Codigo | Regra |
|---|---|
| UPL-PERM-001 | Upload exige usuario autenticado ou chave de API valida. |
| UPL-PERM-002 | Importacao exige permissao de criacao no modulo da entidade importada. |
| UPL-PERM-003 | Exportacao exige permissao de visualizacao no modulo exportado. |
| UPL-PERM-004 | Consulta de erro de importacao exige acesso a execucao ou permissao administrativa. |
| UPL-PERM-005 | Atualizacao incremental exige papel administrativo tecnico. |
| UPL-PERM-006 | Cancelamento de upload remoto exige ser dono ou administrador. |
| UPL-PERM-007 | Undo de importacao exige permissao sobre a entidade e sobre a execucao. |
| UPL-PERM-008 | Usuario com nivel administrativo pode ignorar bloqueio global de upload quando parametrizado. |

## 11. Parametros funcionais

| Parametro | Valor encontrado | Observacao |
|---|---|---|
| Extensoes tabulares permitidas | csv, xlsx | Usadas para importacao tabular. |
| Linha de cabecalho | 1 | Dados iniciam na linha 2. |
| Campos customizados de oportunidades | 1 a 150 | Mapeamento preservado. |
| Status de upload remoto | pending, processing, downloading, complete, failed, cancelled | Traduzidos para estados funcionais no Epros. |
| Limpeza de partes antigas | mais de 3 dias | Upload em partes. |
| Feedback de URL remota | 200000 bytes | Intervalo de progresso informado no material. |
| Batch de atualizacao por ciclo | 5 empresas | Parametro encontrado em fluxo de atualizacao. |
| Porta/limite de upload API | Nao informado no material | O material cita limites distintos por arquitetura, sem valor unico. |
| Tamanho maximo de upload | Nao informado no material | Parametrizado por usuario/ambiente. |
| Tamanho minimo de upload | 1 byte como padrao de referencia | Confirmar na implantacao. |

## 12. Modelo de dados funcional e implantavel

### 12.1 Visao geral das entidades

| Entidade | Finalidade |
|---|---|
| upl_configuracao | Parametros de upload, extensoes, limites, fila, importacao e exportacao. |
| upl_execucao_upload | Controle de recebimento de arquivo por origem direta, remota ou API. |
| upl_upload_parte | Partes recebidas de upload fracionado. |
| upl_arquivo | Registro funcional do arquivo recebido ou migrado. |
| upl_fila_url_remota | Fila de download remoto e progresso. |
| upl_execucao_importacao | Execucao de importacao tabular ou XML. |
| upl_importacao_linha | Resultado por linha processada. |
| upl_importacao_erro | Log de erro por execucao, linha e atributo. |
| upl_mapeamento_importacao | Mapeamentos salvos e reaproveitaveis. |
| upl_importacao_xml | Staging e controle de XML fiscal. |
| upl_arquivo_xml_saida | Resumo de lote de XML de saida. |
| upl_execucao_exportacao | Geracao de arquivos de exportacao. |
| upl_exportacao_campo | Campos padrao/customizados escolhidos. |
| upl_atualizacao_versao | Versao aplicada por empresa ou ambiente. |
| upl_atualizacao_bloco | Blocos/arquivos idempotentes aplicados. |
| upl_atualizacao_job | Jobs tecnicos de atualizacao. |
| upl_migracao_offline | Execucao de carga offline de arquivos. |
| upl_historico | Auditoria funcional. |

### 12.2 Relacionamentos principais

| Origem | Relacao | Destino |
|---|---|---|
| upl_execucao_upload | gera | upl_arquivo |
| upl_execucao_upload | possui | upl_upload_parte |
| upl_fila_url_remota | gera | upl_execucao_upload |
| upl_execucao_importacao | possui | upl_importacao_linha |
| upl_execucao_importacao | possui | upl_importacao_erro |
| upl_execucao_importacao | pode usar | upl_mapeamento_importacao |
| upl_execucao_importacao | pode gerar | entidades dos modulos donos |
| upl_importacao_xml | pertence a | empresa |
| upl_arquivo_xml_saida | resume | lote de XML de saida |
| upl_execucao_exportacao | possui | upl_exportacao_campo |
| upl_atualizacao_versao | possui | upl_atualizacao_bloco |
| upl_atualizacao_job | atualiza | upl_atualizacao_versao |
| upl_migracao_offline | gera | upl_arquivo |

### 12.3 Entidade upl_configuracao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa da configuracao. |
| chave | texto | Nao informado no material | Sim | UK por tenant | Nome funcional do parametro. |
| valor | texto/json | Nao informado no material | Nao |  | Valor parametrizado. |
| ativo | booleano | true/false | Sim |  | Indica uso. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |

### 12.4 Entidade upl_execucao_upload

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Dono da execucao. |
| usuario_upload_id | inteiro | Nao informado no material | Nao | FK usuario | Usuario que enviou quando diferente do dono. |
| origem | enum | direct, remote, api, offline | Sim | indice | Origem do upload. |
| nome_original | texto | Nao informado no material | Sim |  | Nome recebido. |
| extensao | texto | csv, xlsx, xml ou Nao informado no material | Nao | indice | Extensao do arquivo. |
| tamanho_bytes | inteiro | Nao informado no material | Nao |  | Tamanho recebido. |
| mime_type | texto | Nao informado no material | Nao |  | Tipo detectado. |
| status | enum | recebido, validando, armazenando, concluido, erro, cancelado | Sim | indice | Estado da execucao. |
| mensagem_erro | texto | Nao informado no material | Nao |  | Erro funcional. |
| pasta_destino_id | inteiro/texto | Nao informado no material | Nao |  | Pasta alvo. |
| arquivo_id | inteiro | Nao informado no material | Nao | FK upl_arquivo | Arquivo criado. |
| criado_em | data-hora | Nao informado no material | Sim | indice | Data do recebimento. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Ultima alteracao. |

### 12.5 Entidade upl_upload_parte

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| execucao_upload_id | inteiro | Nao informado no material | Sim | FK upl_execucao_upload | Upload de origem. |
| byte_inicio | inteiro | Nao informado no material | Sim |  | Inicio da faixa. |
| byte_fim | inteiro | Nao informado no material | Sim |  | Fim da faixa. |
| total_bytes | inteiro | Nao informado no material | Sim |  | Total esperado. |
| caminho_temporario | texto | Nao informado no material | Sim |  | Local da parte. |
| completa | booleano | true/false | Sim |  | Parte consolidada. |
| criado_em | data-hora | Nao informado no material | Sim | indice | Usado na limpeza acima de 3 dias. |

### 12.6 Entidade upl_arquivo

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| owner_usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Dono do arquivo. |
| uploaded_usuario_id | inteiro | Nao informado no material | Nao | FK usuario | Quem enviou. |
| nome_original | texto | Nao informado no material | Sim |  | Nome informado. |
| nome_armazenado | texto | Nao informado no material | Sim | UK por storage | Nome opaco. |
| extensao | texto | Nao informado no material | Nao | indice | Extensao. |
| tamanho_bytes | inteiro | Nao informado no material | Sim | indice | Tamanho. |
| hash_arquivo | texto | Nao informado no material | Nao | indice | Usado para deduplicacao. |
| pasta_id | inteiro/texto | Nao informado no material | Nao |  | Pasta destino. |
| servidor_storage_id | inteiro/texto | Nao informado no material | Nao |  | Storage usado. |
| origem_upload | enum | direct, remote, api, offline | Sim | indice | Origem. |
| status | enum | ativo, removido, erro | Sim | indice | Situacao. |
| criado_em | data-hora | Nao informado no material | Sim | indice | Data de criacao. |

### 12.7 Entidade upl_fila_url_remota

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador da fila. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Dono da fila. |
| url | texto | URL valida | Sim | indice | Origem remota. |
| servidor_processamento_id | inteiro/texto | Nao informado no material | Nao |  | Servidor do download. |
| status_job | enum | pending, processing, downloading, complete, failed, cancelled | Sim | indice | Status preservado do material. |
| tamanho_total | inteiro | Nao informado no material | Nao |  | Total remoto. |
| tamanho_baixado | inteiro | Nao informado no material | Nao |  | Baixado ate o momento. |
| percentual_download | decimal | 0..100 | Nao |  | Progresso. |
| pasta_destino_id | inteiro/texto | Nao informado no material | Nao |  | Destino opcional. |
| novo_arquivo_id | inteiro | Nao informado no material | Nao | FK upl_arquivo | Arquivo gerado ao concluir. |
| mensagem_erro | texto | Nao informado no material | Nao |  | Motivo de falha. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Atualizacao de progresso. |

### 12.8 Entidade upl_execucao_importacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Usuario importador. |
| import_ref | texto | Nao informado no material | Sim | UK | Identificador unico da transacao. |
| tipo_importacao | texto | leads, clients, projects, items, xml, outros previstos | Sim | indice | Tipo funcional. |
| arquivo_id | inteiro | Nao informado no material | Nao | FK upl_arquivo | Arquivo consolidado. |
| arquivo_temporario_chave | texto | Nao informado no material | Nao | indice | Chave de upload temporario. |
| arquivo_temporario_nome | texto | Nao informado no material | Nao |  | Nome temporario. |
| status | enum | criada, validando, processando, parcial, finalizada, erro, desfeita | Sim | indice | Estado. |
| total_linhas | inteiro | Nao informado no material | Nao |  | Total lido. |
| linhas_sucesso | inteiro | Nao informado no material | Nao |  | Count passed. |
| linhas_ignoradas | inteiro | Nao informado no material | Nao |  | Skipped. |
| quantidade_erros | inteiro | Nao informado no material | Nao |  | Error count. |
| referencia_erro | texto | Nao informado no material | Nao | indice | Consulta de log. |
| resultado | enum | passed, partial, failed, nothing | Nao |  | Resultado de tela. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |
| finalizado_em | data-hora | Nao informado no material | Nao |  | Conclusao. |

### 12.9 Entidade upl_importacao_linha

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| execucao_importacao_id | inteiro | Nao informado no material | Sim | FK upl_execucao_importacao | Execucao. |
| numero_linha | inteiro | >= 2 para dados | Sim | indice | Linha processada. |
| status | enum | importada, ignorada, erro | Sim | indice | Resultado da linha. |
| entidade_destino | texto | Nao informado no material | Nao |  | Entidade gerada. |
| entidade_destino_id | inteiro/texto | Nao informado no material | Nao |  | Registro gerado. |
| payload_linha | texto/json | Nao informado no material | Nao |  | Dados normalizados. |

### 12.10 Entidade upl_importacao_erro

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| execucao_importacao_id | inteiro | Nao informado no material | Sim | FK upl_execucao_importacao | Execucao. |
| referencia_erro | texto | Nao informado no material | Sim | indice | Referencia de consulta. |
| numero_linha | inteiro | Nao informado no material | Nao | indice | Linha com erro. |
| atributo | texto | Nao informado no material | Nao |  | Campo com erro. |
| mensagem | texto | Nao informado no material | Sim |  | Mensagem funcional. |
| formato_exibicao | texto | tabela/html ou Nao informado no material | Nao |  | Material preserva log tabular. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |

### 12.11 Entidade upl_mapeamento_importacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Dono do mapeamento. |
| tipo_importacao | texto | Nao informado no material | Sim | indice | Tipo. |
| nome | texto | Nao informado no material | Sim |  | Nome do mapeamento. |
| mapa_colunas | texto/json | Nao informado no material | Sim |  | Associacao coluna -> campo. |
| ativo | booleano | true/false | Sim |  | Permite reaproveitamento. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |

### 12.12 Entidade upl_importacao_xml

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| empresa_id | inteiro | Nao informado no material | Nao | FK empresa fiscal | Campo opcional no material, mas processamento exige empresa. |
| xml | texto | XML | Sim |  | Conteudo do XML. |
| tipo_de_xml | enum | NaoAplicavel, NotaFiscalEntrada, NotaFiscalSaida, NotaFiscalEntradaPropria, NotaFiscalCancelamento | Sim | indice | Tipo fiscal. |
| nfe_id | texto | Nao informado no material | Sim | indice | Chave/identificador fiscal. |
| status_importacao_xml | enum | NaoProcessado, Processando, Finalizado, Erro | Sim | indice | Status de importacao. |
| mensagem_erro_importacao_xml | texto | Nao informado no material | Nao |  | Erro de importacao. |
| status_cadastro | enum | NaoProcessado, Processando, Finalizado, Erro | Sim | indice | Status de cadastros. |
| mensagem_erro_cadastro | texto | Nao informado no material | Nao |  | Erro de cadastro. |
| status_salvar_pdf | enum | NaoProcessado, Processando, Finalizado, Erro | Sim | indice | Status de PDF. |
| mensagem_erro_salvar_pdf | texto | Nao informado no material | Nao |  | Erro de PDF. |
| codigo_sefaz | inteiro | Nao informado no material | Sim |  | Codigo fiscal. |
| tipo_evento | texto | Nao informado no material | Sim |  | Tipo de evento. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |

### 12.13 Entidade upl_arquivo_xml_saida

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| nome_arquivo | texto | Nao informado no material | Sim |  | Nome do arquivo. |
| qtd_xmls | inteiro | Nao informado no material | Sim |  | Total XMLs. |
| qtd_xmls_invalidos | inteiro | Nao informado no material | Sim |  | XMLs invalidos. |
| qtd_produtos_localizados | inteiro | Nao informado no material | Sim |  | Produtos existentes. |
| qtd_clientes_localizados | inteiro | Nao informado no material | Sim |  | Clientes existentes. |
| qtd_produtos_importados | inteiro | Nao informado no material | Sim |  | Produtos criados. |
| qtd_clientes_importados | inteiro | Nao informado no material | Sim |  | Clientes criados. |
| mensagem_erro | texto | Nao informado no material | Sim |  | Obrigatorio no material. |
| status | enum | Verificando, Processando, Finalizado, Erro | Sim | indice | Estado do lote. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |

### 12.14 Entidade upl_execucao_exportacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Solicitante. |
| entidade | texto | clientes, oportunidades, projetos, faturas, estimativas, pagamentos, despesas, horas, itens, tarefas, atendimentos | Sim | indice | Entidade exportada. |
| filtros_json | texto/json | Nao informado no material | Nao |  | Filtros da lista atual. |
| status | enum | solicitada, gerando, disponivel, erro, expirada | Sim | indice | Estado. |
| arquivo_id | inteiro | Nao informado no material | Nao | FK upl_arquivo | Arquivo XLSX gerado. |
| url_download | texto | Nao informado no material | Nao |  | Download posterior. |
| mensagem_erro | texto | Nao informado no material | Nao |  | Falha. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |

### 12.15 Entidade upl_exportacao_campo

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| execucao_exportacao_id | inteiro | Nao informado no material | Sim | FK upl_execucao_exportacao | Exportacao. |
| origem_campo | enum | standard_field, custom_field | Sim |  | Origem da selecao. |
| chave_campo | texto | Nao informado no material | Sim |  | Campo escolhido. |
| rotulo | texto | Nao informado no material | Nao |  | Cabecalho. |
| ordem | inteiro | Nao informado no material | Nao |  | Ordem na planilha. |

### 12.16 Entidade upl_atualizacao_versao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Nao | FK empresa | Nulo quando escopo central. |
| versao_atual | texto/inteiro | Nao informado no material | Sim | indice | Versao antes da atualizacao. |
| versao_alvo | texto/inteiro | Nao informado no material | Sim | indice | Versao esperada. |
| status | enum | novo, processando, concluido, falho | Sim | indice | Estado. |
| log | texto | Nao informado no material | Nao |  | Log funcional/tecnico. |
| iniciado_em | data-hora | Nao informado no material | Nao |  | Inicio. |
| finalizado_em | data-hora | Nao informado no material | Nao |  | Fim. |

### 12.17 Entidade upl_atualizacao_bloco

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| atualizacao_versao_id | inteiro | Nao informado no material | Sim | FK upl_atualizacao_versao | Atualizacao. |
| nome_arquivo | texto | Nao informado no material | Sim | UK por escopo | Garante idempotencia. |
| identificador_bloco | texto | Nao informado no material | Nao | UK por escopo | Bloco isolado. |
| status | enum | pendente, aplicado, ignorado, falho | Sim | indice | Estado. |
| log | texto | Nao informado no material | Nao |  | Resultado. |
| aplicado_em | data-hora | Nao informado no material | Nao |  | Data de aplicacao. |

### 12.18 Entidade upl_atualizacao_job

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Nao | FK empresa | Empresa quando job for por tenant. |
| tipo | texto | modal, cronjob, url ou Nao informado no material | Sim | indice | Tipo de disparo. |
| nome | texto | Nao informado no material | Sim |  | Nome do job. |
| funcao_nome | texto | Nao informado no material | Nao |  | Se vazia, job falha. |
| status | enum | new, processing, failed, completed | Sim | indice | Status preservado do material. |
| payload_json | texto/json | Nao informado no material | Nao |  | Parametros. |
| log | texto | Nao informado no material | Nao |  | Resultado. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |
| finalizado_em | data-hora | Nao informado no material | Nao |  | Conclusao. |

### 12.19 Entidade upl_migracao_offline

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa destino. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Responsavel. |
| conta_destino | texto | Nao informado no material | Sim |  | Conta de destino. |
| caminho_origem | texto | Nao informado no material | Sim |  | Pasta offline. |
| pasta_inicial_destino | texto | Nao informado no material | Sim |  | Pasta inicial no Epros. |
| modo | texto | copiar | Sim |  | Material indica copiar, nao mover origem. |
| status | enum | criada, processando, concluida, erro, cancelada | Sim | indice | Estado. |
| arquivos_processados | inteiro | Nao informado no material | Nao |  | Contador. |
| mensagem_erro | texto | Nao informado no material | Nao |  | Falha. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |

### 12.20 Entidade upl_historico

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| entidade | texto | Nao informado no material | Sim | indice | Entidade auditada. |
| entidade_id | inteiro/texto | Nao informado no material | Sim | indice | Registro auditado. |
| acao | texto | Nao informado no material | Sim |  | Acao executada. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Executor. |
| ip_origem | texto | Nao informado no material | Nao |  | Origem. |
| payload_json | texto/json | Nao informado no material | Nao |  | Dados complementares. |
| criado_em | data-hora | Nao informado no material | Sim |  | Momento. |

## 13. Dicionario de dados implantavel

### 13.1 Campos transversais

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador unico. |
| tenant_id | inteiro | Nao informado no material | Sim quando escopo empresa | FK empresa | Isolamento por empresa. |
| usuario_id | inteiro | Nao informado no material | Sim quando houver ator | FK usuario | Executor/dono. |
| status | texto/enum | Conforme entidade | Sim | indice | Estado funcional. |
| mensagem_erro | texto | Nao informado no material | Nao |  | Erro funcional. |
| criado_em | data-hora | Nao informado no material | Sim |  | Data de criacao. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Data de alteracao. |

### 13.2 Campos de upload

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| origem | enum | direct, remote, api, offline | Sim | indice | Origem do arquivo. |
| nome_original | texto | Nao informado no material | Sim |  | Nome recebido. |
| nome_armazenado | texto | Nao informado no material | Sim | UK | Nome opaco. |
| tamanho_bytes | inteiro | Nao informado no material | Sim quando arquivo consolidado | indice | Tamanho. |
| hash_arquivo | texto | Nao informado no material | Nao | indice | Deduplicacao e bloqueio. |
| owner_usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Dono. |
| uploaded_usuario_id | inteiro | Nao informado no material | Nao | FK usuario | Quem enviou. |

### 13.3 Campos de importacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| import_ref | texto | Nao informado no material | Sim | UK | Identificador de transacao. |
| tipo_importacao | texto | leads, clients, projects, items, xml, outros previstos | Sim | indice | Tipo funcional. |
| total_linhas | inteiro | Nao informado no material | Nao |  | Total processado. |
| linhas_sucesso | inteiro | Nao informado no material | Nao |  | Count passed. |
| linhas_ignoradas | inteiro | Nao informado no material | Nao |  | Skipped. |
| quantidade_erros | inteiro | Nao informado no material | Nao |  | Error count. |
| referencia_erro | texto | Nao informado no material | Nao | indice | Consulta de log. |
| resultado | enum | passed, partial, failed, nothing | Nao |  | Resultado apresentado. |

### 13.4 Campos de XML fiscal

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| xml | texto | XML | Sim |  | Conteudo importado. |
| tipo_de_xml | enum | NaoAplicavel, NotaFiscalEntrada, NotaFiscalSaida, NotaFiscalEntradaPropria, NotaFiscalCancelamento | Sim | indice | Tipo do XML. |
| nfe_id | texto | Nao informado no material | Sim | indice | Identificador fiscal. |
| status_importacao_xml | enum | NaoProcessado, Processando, Finalizado, Erro | Sim | indice | Processamento do XML. |
| status_cadastro | enum | NaoProcessado, Processando, Finalizado, Erro | Sim | indice | Cadastros gerados. |
| status_salvar_pdf | enum | NaoProcessado, Processando, Finalizado, Erro | Sim | indice | PDF fiscal. |
| codigo_sefaz | inteiro | Nao informado no material | Sim |  | Codigo fiscal. |
| tipo_evento | texto | Nao informado no material | Sim |  | Evento fiscal. |

### 13.5 Campos de exportacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| entidade | texto | entidades exportaveis | Sim | indice | Entidade alvo. |
| filtros_json | texto/json | Nao informado no material | Nao |  | Filtros atuais. |
| origem_campo | enum | standard_field, custom_field | Sim |  | Tipo de campo selecionado. |
| chave_campo | texto | Nao informado no material | Sim |  | Campo escolhido. |
| url_download | texto | Nao informado no material | Nao |  | Link posterior. |

### 13.6 Campos de atualizacao incremental

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| versao_atual | texto/inteiro | Nao informado no material | Sim | indice | Versao de partida. |
| versao_alvo | texto/inteiro | Nao informado no material | Sim | indice | Versao desejada. |
| nome_arquivo | texto | Nao informado no material | Sim | UK | Idempotencia por arquivo. |
| identificador_bloco | texto | Nao informado no material | Nao | UK | Idempotencia por bloco. |
| funcao_nome | texto | Nao informado no material | Nao |  | Obrigatoria quando job exigir funcao. |
| log | texto | Nao informado no material | Nao |  | Resultado ou falha. |

## 14. Integracoes funcionais

| Modulo | Integracao |
|---|---|
| GED | Armazenamento e referencia de arquivos consolidados. |
| Fiscal | Processamento de XML, codigo fiscal, PDF e eventos fiscais. |
| Cadastros Base | Cadastro automatico de pessoas, veiculos, produtos, unidades e NCM quando aplicavel. |
| Vendas | XML de saida pode gerar venda. |
| Compras/Estoque | XML de entrada pode gerar compra e atualizar fluxo de entrada. |
| Financeiro | XML de saida pode gerar contas a receber; XML de entrada pode gerar contas a pagar. |
| Configuracao | Parametros de extensao, duplicidade, limites, jobs e versoes. |
| Seguranca | Permissoes, chaves de API, bloqueios por hash e auditoria. |
| Workflow | Aprovacao de registros controlados e execucoes sensiveis. |

## 15. Telas, relatorios e respostas

| Tela/visao | Conteudo esperado |
|---|---|
| Lista de upload e migracao | Filtros por status, periodo e responsavel; acoes novo e exportar. |
| Detalhe da execucao | Dados, historico, anexos, erros, aprovacao e resultado. |
| Painel gestor | KPIs, fila de aprovacao, execucoes em andamento e falhas. |
| Wizard de importacao | Upload, requisitos, amostras, mapeamento, duplicidade, confirmacao, resultado e erro. |
| Log de erro | Consulta por referencia, linha, atributo e mensagem. |
| Upload remoto | URL, status, progresso, cancelamento e arquivo gerado. |
| Exportacao | Selecionar campos padrao/customizados e receber download posterior. |
| Atualizacao incremental | Versao atual, versao alvo, blocos aplicados, falhas e logs. |

| Relatorio | Descricao |
|---|---|
| Posicao geral | Snapshot por status e tipo de execucao. |
| Auditoria de alteracoes | Trilha por periodo, usuario, acao e entidade. |
| Resultado de importacoes | Totais de sucesso, ignorados, erros e resultados parciais. |
| Atualizacoes por empresa | Versao atual, alvo, status e log. |

## 16. Criterios de aceite

| Codigo | Criterio |
|---|---|
| UPL-CA-001 | Upload deve rejeitar extensao nao permitida. |
| UPL-CA-002 | Upload deve rejeitar arquivo abaixo do tamanho minimo quando o canal exigir. |
| UPL-CA-003 | Upload em partes deve aguardar conclusao antes de consolidar. |
| UPL-CA-004 | Partes temporarias com mais de 3 dias devem ser removidas. |
| UPL-CA-005 | URL remota deve registrar progresso e status final. |
| UPL-CA-006 | Importacao tabular sem arquivo temporario existente deve falhar. |
| UPL-CA-007 | Importacao deve iniciar dados na linha 2 e usar cabecalho na linha 1. |
| UPL-CA-008 | Falhas por linha devem alimentar contagem e log consultavel. |
| UPL-CA-009 | Resultado parcial deve diferenciar sucesso, ignorados e erros. |
| UPL-CA-010 | Exportacao deve aplicar filtros atuais e campos selecionados. |
| UPL-CA-011 | Exportacao deve exigir permissao de visualizacao do modulo. |
| UPL-CA-012 | XML vazio ou invalido deve ser rejeitado. |
| UPL-CA-013 | XML sem empresa valida nao deve ser processado. |
| UPL-CA-014 | XML de saida sem venda processada deve gerar erro funcional de lote. |
| UPL-CA-015 | XML de entrada sem compra processada deve gerar erro funcional de lote. |
| UPL-CA-016 | Atualizacao incremental nao pode reaplicar bloco ja registrado. |
| UPL-CA-017 | Job de atualizacao sem funcao ou com funcao inexistente deve falhar. |
| UPL-CA-018 | Migracao offline deve registrar arquivos processados e nao mover origem quando configurada para copia. |

## 17. Pontos de decisao encaminhados para MC

1. Definir tamanhos maximos por canal de upload.
2. Definir lista final de extensoes aceitas por tipo de importacao.
3. Confirmar tabela/lista de arquivos banidos e hash bloqueado.
4. Definir politica de limpeza de temporarios de importacao e exportacao.
5. Definir entidades finais com wizard de importacao na V1.
6. Definir se undo sera exigido para todas as importacoes ou apenas para algumas entidades.
7. Definir governanca de atualizacao incremental e execucao tecnica.
8. Definir retencao de XML, logs de importacao, arquivos temporarios e exports.

## 18. Notas de elaboracao

[^1]: Foram criados nomes funcionais padronizados para tabelas do Epros com prefixo `upl_`, pois o material informa campos, regras e relacionamentos, mas nao apresenta nomenclatura fisica definitiva para a nova base.
[^2]: A expressao "oportunidades" foi usada para representar a entidade comercial citada no material como importacao de leads, mantendo a especificacao no vocabulario funcional do Epros.
