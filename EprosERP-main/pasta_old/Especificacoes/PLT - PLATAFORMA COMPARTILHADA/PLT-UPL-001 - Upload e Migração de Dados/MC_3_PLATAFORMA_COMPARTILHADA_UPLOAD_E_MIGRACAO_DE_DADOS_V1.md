# MC 3 - PLATAFORMA COMPARTILHADA / UPLOAD E MIGRACAO DE DADOS V1

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

## 2. Resumo de completude

| Area | Status | Evidencia funcional consolidada | Pendencia |
|---|---|---|---|
| Upload direto | Concluido | Validacoes de extensao, tamanho, bloqueios, nome, storage e resposta foram especificadas. | Definir limites finais por tenant/plano. |
| Upload em partes | Concluido | Faixa de bytes, montagem, primeira parte, partes antigas e consolidacao foram especificadas. | Confirmar tamanho maximo de parte. |
| Storage e deduplicacao | Concluido | Dono, usuario upload, hash, nome unico, storage e estatisticas foram especificados. | Validar modelo final no GED. |
| Upload remoto | Concluido | URL, fila, progresso, status, cancelamento e arquivo gerado foram especificados. | Confirmar politicas de seguranca para URL externa. |
| Upload por API | Concluido | Chave de acesso, usuario associado, origem API e atualizacao de conteudo foram especificados. | Confirmar limites e escopos da chave. |
| Importacao CSV/XLSX | Concluido | Upload temporario, cabecalho, linha 2, erros por linha, resultado e log foram especificados. | Confirmar entidades da V1. |
| Importacao de oportunidades/clientes/projetos/itens | Concluido | Campos, duplicidade, atribuicoes, eventos e mapeamentos foram especificados. | Validar campos obrigatorios finais por modulo dono. |
| Exportacao | Concluido | POST, permissao, filtros atuais, campos padrao/customizados e download posterior foram especificados. | Confirmar retencao de arquivo exportado. |
| Wizard de importacao | Concluido | Etapas, mapeamento salvo, deduplicacao, adaptadores, checkpoints e undo foram especificados. | Definir alcance do undo. |
| Importacao fiscal XML | Concluido | Tipos, status, empresa, XML, cadastros, venda/compra, financeiro, PDF e erros foram especificados. | Validar fronteira com fiscal granular. |
| Atualizacao incremental | Concluido | Versao, blocos, idempotencia, jobs, logs, lote por empresa e falhas foram especificados. | Definir governanca tecnica e aprovacao. |
| Migracao offline | Concluido | Pasta controlada, conta destino, copia, processamento e desativacao pos-uso foram especificados. | Confirmar ferramenta operacional final. |
| Modelo de dados | Concluido | 18 entidades funcionais e dicionario implantavel foram definidos. | Validar nomes fisicos antes da modelagem tecnica. |

## 3. Matriz de lacunas funcionais

| ID | Capacidade esperada | Status | O que falta construir ou validar | Impacto se nao resolver |
|---|---|---|---|---|
| MC-UPL-001 | Limite de tamanho por canal | Pendente | Definir tamanho maximo para upload direto, remoto, API e partes. | Risco de rejeicao indevida ou consumo excessivo de storage. |
| MC-UPL-002 | Lista final de extensoes permitidas | Pendente | Separar extensoes por importacao tabular, XML, anexos e migracao offline. | Entrada de arquivo indevido ou bloqueio de arquivo valido. |
| MC-UPL-003 | Tipos bloqueados e palavras bloqueadas | Pendente | Definir catalogo corporativo e administracao por tenant. | Risco de seguranca e inconsistencia entre empresas. |
| MC-UPL-004 | Hash bloqueado | Pendente | Confirmar origem, manutencao e tela da lista de hashes proibidos. | Arquivos proibidos podem entrar no Epros. |
| MC-UPL-005 | Arquivos banidos por hash e tamanho | Pendente | Confirmar estrutura funcional da lista citada no material. | Validacao pode ficar incompleta. |
| MC-UPL-006 | Quota diaria de upload | Pendente | Definir se existe limite diario por usuario, empresa ou plano. | Uso abusivo ou bloqueio nao padronizado. |
| MC-UPL-007 | Quota de disco | Pendente | Integrar limite de storage ao plano e ao GED. | Falha tardia de upload por falta de espaco. |
| MC-UPL-008 | Limpeza de temporarios de importacao | Pendente | Definir prazo para arquivos CSV/XLSX temporarios apos processamento. | Acumulo de arquivos temporarios. |
| MC-UPL-009 | Limpeza de exports temporarios | Pendente | Definir prazo de expiracao e remocao de arquivos exportados. | Exposicao prolongada de dados exportados. |
| MC-UPL-010 | Tamanho de parte | Pendente | Definir tamanho padrao e limite de upload em partes. | Upload grande pode falhar ou consumir recursos. |
| MC-UPL-011 | Seguranca de URL remota | Pendente | Definir bloqueio de redes internas, autenticacao, redirecionamentos e protocolo permitido. | Risco de chamada remota indevida. |
| MC-UPL-012 | Fila remota duplicada | Pendente | Definir criterio exato de duplicidade de URL pendente. | Downloads duplicados podem ocupar fila e storage. |
| MC-UPL-013 | Chave de API de upload | Pendente | Definir escopos, validade, revogacao e auditoria da chave. | Integracoes podem operar com privilegio excessivo. |
| MC-UPL-014 | Entidades de importacao da V1 | Pendente | Confirmar oportunidades, clientes, projetos, itens e XML na primeira entrega. | Time pode construir escopo maior que o necessario. |
| MC-UPL-015 | Campos obrigatorios por entidade importada | Pendente | Validar campos finais com os modulos donos. | Importacao pode aceitar dado incompleto ou rejeitar dado valido. |
| MC-UPL-016 | Duplicidade de oportunidades | Pendente | Confirmar combinacoes finais de nome, email, telefone e empresa. | Duplicatas podem ser criadas ou linhas corretas ignoradas. |
| MC-UPL-017 | Duplicidade de clientes | Pendente | Confirmar combinacoes finais de email, telefone e empresa. | Base cadastral pode ficar duplicada. |
| MC-UPL-018 | Projetos com usuarios atribuidos | Pendente | Definir regra de parsing, validacao e falha para usuarios nao encontrados. | Projetos podem ser importados sem equipe correta. |
| MC-UPL-019 | Campos customizados de oportunidades | Pendente | Validar se o limite 1 a 150 permanece no Epros. | Perda de dados customizados ou excesso de colunas. |
| MC-UPL-020 | Formato do log de erro | Pendente | Confirmar se a exibicao tabular com conteudo estruturado sera mantida. | Usuarios podem perder detalhe de validacao por linha. |
| MC-UPL-021 | Undo de importacao | Pendente | Definir entidades que suportam desfazer e prazo para acao. | Registros errados podem exigir correcao manual. |
| MC-UPL-022 | Checkpoint idempotente | Pendente | Definir chave de checkpoint por execucao, linha e entidade. | Reprocessamento pode duplicar dados. |
| MC-UPL-023 | Adaptadores de fonte externa | Pendente | Definir conectores e trilha de auditoria por fonte. | Importacoes externas podem ficar sem rastreabilidade. |
| MC-UPL-024 | Exportacoes da V1 | Pendente | Confirmar quais entidades exportaveis entram na primeira entrega. | Exportacao pode ficar incompleta ou ampla demais. |
| MC-UPL-025 | Campos exportaveis | Pendente | Validar catalogo de campos padrao e customizados por entidade. | Exportacao pode gerar planilhas fora do esperado. |
| MC-UPL-026 | Fronteira XML x fiscal | Pendente | Confirmar o que fica no staging de upload e o que fica no modulo fiscal. | Duplicidade de regras fiscais. |
| MC-UPL-027 | Mensagem de erro obrigatoria em lote XML | Pendente | O material marca `MensagemErro` como obrigatoria; validar se pode ser vazia em sucesso. | Modelo fisico pode exigir campo sem valor funcional. |
| MC-UPL-028 | Empresa obrigatoria em XML | Pendente | O material traz `EmpresaId` opcional, mas regra exige empresa para processar. | Divergencia entre modelo e processamento. |
| MC-UPL-029 | Atualizacao incremental de schema | Pendente | Definir aprovacao, ambiente, rollback e segregacao de responsabilidades. | Alteracoes tecnicas podem ocorrer sem governanca. |
| MC-UPL-030 | Lote de empresas por atualizacao | Pendente | Confirmar batch 5 ou parametrizar por ambiente. | Atualizacao pode ser lenta ou sobrecarregar o ambiente. |
| MC-UPL-031 | Patch apos falha parcial | Pendente | Definir regra segura para nao remover patch antes de todas as empresas concluirem. | Empresas podem ficar em versoes diferentes sem reprocessamento. |
| MC-UPL-032 | Job tecnico com funcao dinamica | Pendente | Definir catalogo permitido de funcoes e validacao previa. | Risco de execucao indevida. |
| MC-UPL-033 | Migracao offline de arquivos | Pendente | Definir ferramenta final, operador, evidencias e desativacao pos-uso. | Carga massiva pode ficar sem controle. |
| MC-UPL-034 | Retencao de logs | Pendente | Definir prazo para logs de erro, historico, payload e arquivos temporarios. | Risco de perda de auditoria ou excesso de dados. |
| MC-UPL-035 | Modelo fisico final | Pendente | Validar nomes de tabelas, indices, chaves e tipos. | Retrabalho tecnico na implementacao. |

## 4. Matriz de aderencia ao padrao de especificacao

| Requisito do padrao | Status | Observacao |
|---|---|---|
| Documento descreve o Epros no presente | Concluido | A EF foi escrita como fonte funcional do Epros. |
| Sem nomes de plataformas anteriores | Concluido | Nao ha referencia a sistemas externos de origem do levantamento. |
| Modelo de dados antes do dicionario | Concluido | A EF contem modelo funcional antes do dicionario. |
| Dicionario com campo, formato, tamanho, obrigatoriedade, chave e regra | Concluido | Estrutura aplicada nas entidades e campos transversais. |
| Campos desconhecidos marcados de forma explicita | Concluido | Foi usado `Nao informado no material`. |
| Lacunas separadas da especificacao | Concluido | Pendencias foram encaminhadas para esta MC. |
| Sem invencao de regra obrigatoria | Concluido | Itens criados para padronizacao foram indicados em nota da EF. |

## 5. Itens prontos para validacao humana

| Item | Status | Observacao |
|---|---|---|
| Fluxo de upload direto | Concluido | Inclui validacao, storage, nome unico, hash e resposta. |
| Fluxo de upload remoto | Concluido | Inclui URL, fila, progresso, status e arquivo gerado. |
| Fluxo de upload em partes | Concluido | Inclui faixa de bytes, montagem e limpeza de partes antigas. |
| Fluxo de importacao tabular | Concluido | Inclui temporario, cabecalho, linha 2, log e resultado. |
| Fluxo de exportacao | Concluido | Inclui permissao, filtros, campos selecionados e download posterior. |
| Fluxo de XML fiscal | Concluido | Inclui tipos, status, empresa, cadastros, venda/compra e financeiro. |
| Fluxo de atualizacao incremental | Concluido | Inclui versao, blocos, idempotencia, jobs e logs. |
| Modelo de dados | Concluido | Entidades funcionais suficientes para modelagem inicial. |
| Criterios de aceite | Concluido | Cenarios principais documentados. |

## 6. Status final do submodulo

| Indicador | Valor |
|---|---|
| Status do submodulo | Concluido |
| Classificacao de conteudo | Com conteudo |
| Arquivos canonicos processados | 12 |
| EF criada | Sim |
| MC criada | Sim |
| Requer retorno ao material canonico para validacao normal | Nao |
| Requer decisao humana antes de construir tudo | Sim, para lacunas listadas |
