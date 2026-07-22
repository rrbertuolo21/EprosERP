# EF 3 Plataforma Compartilhada - Assinatura Eletronica V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Plataforma Compartilhada |
| Submodulo | Assinatura Eletronica |
| Versao | V1 |
| Status | Especificacao funcional para validacao humana |
| Data | 2026-06-06 |

## 2. Objetivo funcional

O submodulo Assinatura Eletronica controla o registro, validacao, trilha e consulta de assinaturas eletronicas aplicadas a documentos, contratos, politicas, ofertas, anexos formais e outros artefatos digitais do Epros.

O material analisado comprova assinatura eletronica simples vinculada a contrato, com signatario, tipo de assinatura, dados da assinatura, data/hora da assinatura, controle de permissao, sucesso/falha operacional e ativacao do contrato quando a quantidade minima de assinaturas e atingida. As capacidades de certificado digital, carimbo de tempo, hash documental, provedor externo, assinatura qualificada e preservacao criptografica devem ser tratadas como lacunas de completude nesta versao.

## 3. Escopo

### 3.1 Dentro do escopo

| Capacidade | Descricao |
|---|---|
| Solicitar assinatura | Registrar uma solicitacao de assinatura para documento ou contrato elegivel. |
| Registrar assinatura | Gravar signatario, tipo de assinatura, dados da assinatura e data/hora da conclusao. |
| Controlar permissao | Permitir assinatura apenas a usuario autorizado para a acao e para o documento. |
| Consolidar signatarios | Controlar quantidade de assinaturas necessarias para considerar o documento concluido. |
| Ativar documento assinado | Atualizar o estado do documento quando os criterios de assinatura forem cumpridos. |
| Auditar tentativa | Registrar sucesso, falha e eventos relevantes do processo de assinatura. |
| Consultar assinaturas | Exibir assinaturas por documento, usuario, status, periodo e responsavel. |
| Anexar evidencia | Vincular assinatura ao documento armazenado no repositorio documental do Epros. |
| Integrar workflow | Permitir etapa de assinatura dentro de fluxos de aprovacao e aceite. |
| Relatorios | Disponibilizar posicao geral e trilha de auditoria por periodo. |

### 3.2 Fora do escopo

| Tema | Tratamento |
|---|---|
| Cadastro de pessoas, usuarios ou empresas | Pertence aos cadastros e identidade do Epros. |
| Criacao do contrato ou documento de negocio | Pertence ao modulo dono do documento. |
| Armazenamento documental completo | Pertence a Gestao Eletronica de Documentos. |
| Regras comerciais do contrato | Pertencem ao modulo dono do contrato. |
| Cobranca, planos ou pagamento | Pertencem aos submodulos de assinatura comercial, limites e cobranca SaaS. |
| Certificacao qualificada e autoridade certificadora | Lacuna de completude, dependente de decisao de produto e compliance. |

## 4. Dependencias e consumidores

### 4.1 Dependencias

| Dependencia | Uso |
|---|---|
| Identidade e Contexto Tenant | Identificar usuario, tenant, empresa, perfil e permissao de assinatura. |
| Usuarios e Papeis | Controlar quem pode solicitar, assinar, cancelar, excluir evidencia ou consultar auditoria. |
| Permissoes de Menu | Expor menus e acoes conforme perfil autorizado. |
| Gestao Eletronica de Documentos | Armazenar documento, versao assinada, anexos e evidencias. |
| Workflow | Orquestrar aprovacao antes ou depois da assinatura quando aplicavel. |
| Compliance e Privacidade | Controlar auditoria, retencao, mascaramento, consentimento e protecao de dados pessoais. |
| API Gateway e OpenAPI | Publicar contratos de API, seguranca, logs e padrao de erro. |
| SOA e Colaboracao | Notificar signatarios, solicitantes e gestores. |

### 4.2 Consumidores

| Consumidor | Uso |
|---|---|
| Gestao de Contratos de Venda | Assinar contratos comerciais. |
| Gestao de Contratos de Compra | Assinar contratos de fornecimento. |
| Governanca, Riscos e Conformidade | Registrar aceite de politicas, normas e evidencias formais. |
| RH | Assinar ofertas, termos, documentos admissionais, ferias, desligamentos e ciencia de politicas. |
| Projetos | Assinar termos de aceite, entregaveis e marcos contratuais. |
| Qualidade | Assinar planos, nao conformidades, aprovacoes e liberacoes. |
| Operacao Siser | Monitorar uso, auditoria e falhas de assinatura. |

## 5. Principios funcionais

| Codigo | Regra |
|---|---|
| REG-ASS-001 | Toda assinatura deve estar vinculada a um documento ou contrato identificavel. |
| REG-ASS-002 | Toda assinatura deve estar vinculada a um signatario identificavel. |
| REG-ASS-003 | Toda assinatura deve registrar tipo de assinatura, dados da assinatura e data/hora de conclusao. |
| REG-ASS-004 | Usuario sem permissao de assinatura nao pode concluir assinatura. |
| REG-ASS-005 | Documento ou contrato somente deve mudar para concluido/ativo quando atingir o criterio minimo de assinaturas. |
| REG-ASS-006 | Falha de assinatura deve retornar mensagem operacional segura e preservar a trilha da tentativa quando aplicavel. |
| REG-ASS-007 | Sucesso de assinatura deve confirmar conclusao ao usuario e atualizar o historico do documento. |
| REG-ASS-008 | Assinatura nao deve alterar conteudo do documento sem gerar nova versao documental. |
| REG-ASS-009 | Assinatura deve respeitar tenant, empresa e permissao do contexto autenticado. |
| REG-ASS-010 | Evidencias de assinatura devem ser consultaveis por documento, signatario, periodo e status. |
| REG-ASS-011 | Dados de assinatura devem ser protegidos em repouso e em transito conforme politica de privacidade do Epros. |
| REG-ASS-012 | Regras nao comprovadas no material devem permanecer na MC como lacuna, nao como requisito fechado. |

## 6. Regras funcionais detalhadas

### 6.1 Solicitacao de assinatura

| Codigo | Regra |
|---|---|
| REG-ASS-013 | O Epros deve permitir criar solicitacao de assinatura para documento elegivel. |
| REG-ASS-014 | A solicitacao deve conter documento, tenant, empresa quando aplicavel, solicitante, signatario e status inicial. |
| REG-ASS-015 | Documento inexistente, inacessivel ou fora do contexto autorizado deve bloquear a solicitacao. |
| REG-ASS-016 | Solicitacao deve indicar se a assinatura e obrigatoria para concluir o documento. |
| REG-ASS-017 | Solicitacao deve permitir mais de um signatario quando o documento exigir assinatura multipla. |
| REG-ASS-018 | O criterio minimo de assinaturas deve ser parametrizavel por tipo de documento. |
| REG-ASS-019 | Quando o criterio minimo nao estiver parametrizado no material, a MC deve registrar a pendencia. |

### 6.2 Assinatura simples comprovada

| Codigo | Regra |
|---|---|
| REG-ASS-020 | A assinatura simples deve registrar documento/contrato, usuario, tipo de assinatura, dados de assinatura e data/hora. |
| REG-ASS-021 | O tipo de assinatura deve aceitar classificacao funcional, como digital simples ou desenhada, quando habilitada para o documento. |
| REG-ASS-022 | Os dados da assinatura devem armazenar o payload necessario para reconstituir ou validar a evidencia da assinatura. |
| REG-ASS-023 | A data/hora de assinatura deve ser gravada no momento da conclusao. |
| REG-ASS-024 | A assinatura deve registrar o responsavel pela criacao do registro quando essa informacao estiver disponivel. |
| REG-ASS-025 | Uma assinatura deve pertencer a um unico documento/contrato. |
| REG-ASS-026 | Uma assinatura deve pertencer a um unico signatario. |
| REG-ASS-027 | O Epros deve impedir conclusao de assinatura sem dados de assinatura. |
| REG-ASS-028 | O Epros deve impedir conclusao de assinatura sem signatario identificado. |

### 6.3 Permissao e seguranca

| Codigo | Regra |
|---|---|
| REG-ASS-029 | A assinatura deve exigir permissao especifica para assinar o tipo de documento. |
| REG-ASS-030 | A permissao de assinatura deve ser diferente da permissao de visualizar ou editar o documento. |
| REG-ASS-031 | Usuario autorizado para consultar documento nao assina automaticamente sem permissao de assinatura. |
| REG-ASS-032 | Usuario nao autenticado so pode assinar quando houver fluxo publico formal aprovado para esse documento. |
| REG-ASS-033 | Fluxo publico, se existir, deve usar token seguro, expiracao, escopo unico e auditoria reforcada. |
| REG-ASS-034 | A assinatura deve ser bloqueada quando o documento estiver encerrado, cancelado, expirado ou inelegivel. |
| REG-ASS-035 | A assinatura deve ser bloqueada quando o signatario ja tiver assinado e a politica nao permitir substituicao. |
| REG-ASS-036 | Exclusao ou substituicao de assinatura deve exigir permissao especial e trilha de auditoria. |

### 6.4 Criterio de conclusao

| Codigo | Regra |
|---|---|
| REG-ASS-037 | O documento pode exigir uma ou mais assinaturas para conclusao. |
| REG-ASS-038 | O material comprova regra de conclusao com duas assinaturas para ativar contrato. |
| REG-ASS-039 | Quando o documento atingir o numero minimo de assinaturas, o Epros deve atualizar seu estado conforme regra do modulo dono. |
| REG-ASS-040 | A mudanca de estado deve ocorrer dentro de operacao transacional ou com compensacao auditavel. |
| REG-ASS-041 | Falha na atualizacao do documento nao deve deixar assinatura concluida sem trilha de inconsistencia. |
| REG-ASS-042 | Se o modulo dono rejeitar a atualizacao, a assinatura deve registrar falha operacional ou pendencia de integracao. |
| REG-ASS-043 | Assinaturas parciais devem manter o documento aguardando assinaturas restantes. |

### 6.5 Ciclo de vida

| Codigo | Regra |
|---|---|
| REG-ASS-044 | Solicitacao nasce em rascunho ou pendente conforme origem funcional. |
| REG-ASS-045 | Solicitacao pode ser submetida para analise quando depender de aprovacao previa. |
| REG-ASS-046 | Aprovador pode aprovar ou rejeitar solicitacao quando o fluxo exigir aprovacao. |
| REG-ASS-047 | Assinatura concluida deve alterar status da assinatura para assinada. |
| REG-ASS-048 | Assinatura pode ser inativada quando houver regra de cancelamento formal. |
| REG-ASS-049 | Documento assinado pode ser encerrado sem apagar a evidencia da assinatura. |
| REG-ASS-050 | Reativacao de assinatura inativa deve exigir permissao de gestor e motivo. |
| REG-ASS-051 | Toda transicao deve registrar usuario, data/hora e IP quando disponivel. |

### 6.6 Historico e auditoria

| Codigo | Regra |
|---|---|
| REG-ASS-052 | O Epros deve manter historico por solicitacao, assinatura e documento. |
| REG-ASS-053 | Historico deve registrar criacao, submissao, aprovacao, rejeicao, assinatura, falha, cancelamento, inativacao e reativacao. |
| REG-ASS-054 | Auditoria deve registrar usuario, timestamp, acao, estado anterior, estado posterior, IP e observacao quando disponivel. |
| REG-ASS-055 | Eventos de dominio devem ser publicados apos commit da transacao. |
| REG-ASS-056 | Evento de assinatura concluida deve informar documento, signatario, status e data/hora. |
| REG-ASS-057 | Evento de falha de assinatura deve informar documento, usuario, codigo de erro e correlationId quando disponivel. |
| REG-ASS-058 | Dados sensiveis do payload de assinatura nao devem ser exibidos em logs operacionais. |

### 6.7 Integracao documental

| Codigo | Regra |
|---|---|
| REG-ASS-059 | Documento assinado deve ser preservado em repositorio documental. |
| REG-ASS-060 | A assinatura deve referenciar a versao do documento que foi assinada. |
| REG-ASS-061 | Alteracao de documento apos assinatura deve gerar nova versao e exigir nova assinatura quando a regra do documento determinar. |
| REG-ASS-062 | Anexos de evidencia devem ser associados ao documento e a assinatura correspondente. |
| REG-ASS-063 | Documento final deve permitir consulta da trilha de assinaturas. |
| REG-ASS-064 | A preservacao de hash, carimbo de tempo e certificado deve ser validada como lacuna de completude para padrao corporativo. |

### 6.8 Telas e relatorios

| Codigo | Regra |
|---|---|
| REG-ASS-065 | A lista de assinaturas deve permitir filtro por status, periodo e responsavel. |
| REG-ASS-066 | O detalhe deve exibir dados, historico, anexos e aprovacao quando aplicavel. |
| REG-ASS-067 | Painel gestor deve exibir fila de aprovacao, pendencias e indicadores. |
| REG-ASS-068 | Relatorio de posicao geral deve apresentar quantidade por status. |
| REG-ASS-069 | Relatorio de auditoria deve permitir consulta por periodo, usuario, documento e acao. |
| REG-ASS-070 | Exportacoes devem respeitar permissao e mascaramento. |

### 6.9 Mensagens operacionais

| Codigo | Regra |
|---|---|
| REG-ASS-071 | Falha de assinatura deve retornar mensagem amigavel e orientada a nova tentativa. |
| REG-ASS-072 | Sucesso de assinatura deve informar que o documento foi assinado com sucesso. |
| REG-ASS-073 | Permissao negada deve retornar mensagem padronizada de autorizacao. |
| REG-ASS-074 | Mensagens nao devem expor detalhes tecnicos sensiveis. |
| REG-ASS-075 | Erros de integracao devem registrar correlationId quando houver API envolvida. |

## 7. Estados

### 7.1 Estados da solicitacao de assinatura

| Estado | Descricao | Entrada | Saida |
|---|---|---|---|
| Rascunho | Solicitacao criada, ainda nao enviada. | Criacao manual ou automatica. | Submeter, cancelar. |
| Em analise | Solicitacao aguarda aprovacao. | Submissao. | Aprovar, rejeitar. |
| Pendente assinatura | Signatario precisa assinar. | Aprovacao ou criacao direta. | Assinar, cancelar, expirar. |
| Assinada parcialmente | Parte dos signatarios assinou, mas criterio minimo nao foi atingido. | Assinatura de um signatario. | Assinar restante, cancelar, expirar. |
| Concluida | Criterio minimo de assinatura atingido. | Assinatura final valida. | Encerrar, inativar. |
| Rejeitada | Aprovador rejeitou a solicitacao. | Rejeicao. | Retornar para rascunho quando permitido. |
| Cancelada | Solicitacao encerrada antes da assinatura final. | Cancelamento. | Nao informado no material. |
| Expirada | Prazo de assinatura vencido. | Vencimento. | Reabrir quando permitido. |
| Inativa | Registro desativado por gestor. | Inativacao. | Reativar. |
| Encerrada | Registro mantido historicamente sem operacao ativa. | Encerramento. | Nao informado no material. |

### 7.2 Transicoes comprovadas e projetadas

| Estado atual | Evento | Proximo estado | Permissao | Observacao |
|---|---|---|---|---|
| Rascunho | Submeter | Em analise | Operador | Presente no fluxo do material. |
| Em analise | Aprovar | Pendente assinatura | Aprovador | O material informa aprovacao para ativacao do fluxo. |
| Em analise | Rejeitar | Rascunho | Aprovador | Deve registrar motivo. |
| Pendente assinatura | Assinar | Assinada parcialmente ou Concluida | Signatario autorizado | Depende do criterio minimo. |
| Assinada parcialmente | Assinar restante | Concluida | Signatario autorizado | Para documentos com multiplos signatarios. |
| Concluida | Ativar documento | Documento ativo | Sistema | Material comprova ativacao de contrato com duas assinaturas. |
| Concluida | Inativar | Inativa | Gestor | Presente na maquina de estados do material. |
| Concluida | Encerrar | Encerrada | Gestor | Presente na maquina de estados do material. |
| Inativa | Reativar | Concluida | Gestor | Presente na maquina de estados do material. |

## 8. Fluxos operacionais

### 8.1 Fluxo principal de assinatura simples

| Passo | Ator | Acao | Entrada | Validacao | Resultado |
|---|---|---|---|---|---|
| 1 | Modulo dono | Solicita assinatura | Documento e signatario | Documento elegivel | Solicitacao criada |
| 2 | Epros | Valida contexto | Tenant, empresa, usuario | Acesso permitido | Assinatura pendente |
| 3 | Signatario | Informa assinatura | Tipo e dados da assinatura | Dados obrigatorios | Assinatura registrada |
| 4 | Epros | Calcula completude | Assinaturas do documento | Criterio minimo | Parcial ou concluida |
| 5 | Epros | Atualiza documento | Documento assinado | Modulo dono aceita estado | Documento ativo/concluido |
| 6 | Epros | Audita e notifica | Evento de assinatura | Commit concluido | Historico e notificacao |

### 8.2 Fluxo com aprovacao previa

| Passo | Ator | Acao | Resultado |
|---|---|---|---|
| 1 | Operador | Cria solicitacao em rascunho | Solicitacao editavel |
| 2 | Operador | Submete para analise | Solicitacao em analise |
| 3 | Aprovador | Aprova ou rejeita | Solicitacao pendente de assinatura ou retornada |
| 4 | Signatario | Assina | Assinatura registrada |
| 5 | Epros | Conclui e integra | Documento atualizado |

### 8.3 Fluxo de falha de assinatura

| Passo | Ator | Acao | Resultado |
|---|---|---|---|
| 1 | Signatario | Tenta assinar | Validacao iniciada |
| 2 | Epros | Identifica falha | Operacao rejeitada |
| 3 | Epros | Retorna mensagem segura | Usuario orientado a tentar novamente |
| 4 | Epros | Registra evento quando aplicavel | Trilha disponivel para suporte |

## 9. Permissoes funcionais

| Permissao | Descricao | Observacao |
|---|---|---|
| assinatura.solicitar | Criar solicitacao de assinatura. | Para operador ou modulo dono. |
| assinatura.visualizar | Consultar assinatura e documento permitido. | Nao permite assinar. |
| assinatura.assinar | Concluir assinatura como signatario autorizado. | Permissao comprovada no material como necessaria. |
| assinatura.aprovar | Aprovar solicitacao antes de assinatura. | Usada quando houver workflow. |
| assinatura.rejeitar | Rejeitar solicitacao com motivo. | Usada quando houver workflow. |
| assinatura.cancelar | Cancelar solicitacao pendente. | Exige auditoria. |
| assinatura.inativar | Inativar assinatura concluida. | Gestor. |
| assinatura.reativar | Reativar assinatura inativa. | Gestor. |
| assinatura.excluir-evidencia | Remover ou substituir evidencia. | Deve ser restrita e auditada. |
| assinatura.auditoria | Consultar trilha completa. | Perfil de controle/compliance. |

## 10. Modelo de dados funcional e implantavel

### 10.1 Visao geral

| Entidade | Tipo | Finalidade | Retencao | Observacao |
|---|---|---|---|---|
| assinatura_solicitacao | Movimento | Controlar pedido de assinatura para documento. | Historica | Entidade necessaria para orquestrar ciclo de vida. |
| assinatura_registro | Movimento | Registrar assinatura feita por signatario. | Historica | Baseada nos campos comprovados no material. |
| assinatura_signatario | Movimento | Controlar signatarios esperados por documento. | Historica | Necessaria para assinatura multipla. |
| assinatura_documento_ref | Referencia | Vincular assinatura ao documento e sua versao. | Historica | A versao documental deve vir do repositorio documental. |
| assinatura_historico | Auditoria | Registrar transicoes, eventos e falhas. | Conforme politica de auditoria | Transicoes informadas no material. |
| assinatura_evidencia | Movimento | Guardar metadados de evidencia da assinatura. | Historica | Hash/certificado sao lacunas quando nao informados. |
| assinatura_politica | Configuracao | Definir regra por tipo de documento. | Vigente com historico | Parametros nao comprovados devem ser validados. |
| assinatura_notificacao | Movimento | Registrar notificacoes a signatarios e gestores. | Conforme politica de comunicacao | Integra SOA e Colaboracao. |

### 10.2 Entidade assinatura_solicitacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador unico da solicitacao. |
| tenant_id | uuid | 36 | Sim | FK tenant | Isolamento por tenant. |
| empresa_id | uuid | 36 | Nao | FK empresa | Obrigatorio quando documento operar por empresa. |
| documento_ref_id | uuid | 36 | Sim | FK assinatura_documento_ref | Documento a assinar. |
| modulo_dono | string | Nao informado no material | Sim | Referencia funcional | Modulo responsavel pelo documento. |
| tipo_documento | string | Nao informado no material | Sim | Politica | Define regra de assinatura. |
| solicitante_usuario_id | uuid | 36 | Sim | FK usuario | Usuario que solicitou. |
| status | enum | Rascunho; Em analise; Pendente assinatura; Assinada parcialmente; Concluida; Rejeitada; Cancelada; Expirada; Inativa; Encerrada | Sim | Estado | Estado funcional da solicitacao. |
| criterio_minimo_assinaturas | inteiro | >= 1 | Sim | Politica | Material comprova criterio de duas assinaturas para contrato. |
| prazo_assinatura | datetime | ISO 8601 | Nao | Campo simples | Nao informado no material. |
| motivo_rejeicao | texto | Nao informado no material | Nao | Campo simples | Obrigatorio quando rejeitada. |
| criado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora de criacao. |
| atualizado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora da ultima atualizacao. |
| concluido_em | datetime | ISO 8601 | Nao | Auditoria | Preenchido quando concluida. |

### 10.3 Entidade assinatura_registro

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador unico da assinatura. |
| solicitacao_id | uuid | 36 | Sim | FK assinatura_solicitacao | Solicitacao associada. |
| documento_ref_id | uuid | 36 | Sim | FK assinatura_documento_ref | Documento assinado. |
| contrato_id | uuid | 36 | Nao | FK documento/contrato | Campo comprovado como referencia ao contrato pai. |
| usuario_id | uuid | 36 | Sim | FK usuario/signatario | Signatario. |
| tipo_assinatura | string | Nao informado no material | Sim | Campo simples | Material comprova tipo de assinatura. |
| dados_assinatura | texto | Nao informado no material | Sim | Campo protegido | Payload da assinatura; nao deve ser exibido em logs. |
| assinado_em | datetime | ISO 8601 | Sim | Auditoria | Timestamp da assinatura. |
| criador_id | uuid | 36 | Nao | FK usuario | Campo comprovado no material. |
| criado_por | uuid/string | Nao informado no material | Nao | Referencia usuario | Campo comprovado no material; tipo final nao informado. |
| status | enum | Pendente; Assinada; Cancelada; Inativa; Substituida; Falha | Sim | Estado | Controla o registro individual. |
| mensagem_falha | texto | Nao informado no material | Nao | Campo simples | Usado quando assinatura falha. |
| ip_origem | string | Nao informado no material | Nao | Auditoria | Material informa IP no historico, sem formato. |
| user_agent | string | Nao informado no material | Nao | Auditoria | Nao informado no material. |
| criado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora de criacao. |
| atualizado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora de atualizacao. |

### 10.4 Entidade assinatura_signatario

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador do signatario esperado. |
| solicitacao_id | uuid | 36 | Sim | FK assinatura_solicitacao | Solicitacao associada. |
| usuario_id | uuid | 36 | Nao | FK usuario | Quando signatario for usuario interno. |
| pessoa_id | uuid | 36 | Nao | FK pessoa | Quando signatario for pessoa externa cadastrada. |
| nome_exibicao | string | Nao informado no material | Nao | Campo simples | Nome para exibicao quando aplicavel. |
| email | string | Nao informado no material | Nao | Campo pessoal | Necessario para convite, mas formato nao informado. |
| papel_assinatura | string | Nao informado no material | Nao | Campo simples | Exemplo: cliente, fornecedor, responsavel interno. |
| ordem | inteiro | >= 1 | Nao | Campo simples | Usado quando houver assinatura sequencial. |
| obrigatorio | boolean | true/false | Sim | Campo simples | Define se conta para completude. |
| status | enum | Pendente; Assinado; Recusado; Expirado; Cancelado | Sim | Estado | Estado individual do signatario. |
| assinatura_registro_id | uuid | 36 | Nao | FK assinatura_registro | Preenchido apos assinatura. |
| assinado_em | datetime | ISO 8601 | Nao | Auditoria | Data/hora de assinatura. |

### 10.5 Entidade assinatura_documento_ref

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador da referencia documental. |
| tenant_id | uuid | 36 | Sim | FK tenant | Isolamento por tenant. |
| documento_id | uuid | 36 | Sim | FK documento | Documento armazenado no Epros. |
| documento_versao_id | uuid | 36 | Nao | FK versao documental | Necessario para garantir qual versao foi assinada. |
| modulo_dono | string | Nao informado no material | Sim | Referencia funcional | Modulo proprietario. |
| tipo_documento | string | Nao informado no material | Sim | Politica | Tipo funcional. |
| titulo_documento | string | Nao informado no material | Nao | Campo simples | Para exibicao. |
| status_documento_assinado | string | Nao informado no material | Nao | Campo simples | Estado retornado pelo modulo dono. |
| criado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora de criacao. |

### 10.6 Entidade assinatura_historico

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador do historico. |
| solicitacao_id | uuid | 36 | Sim | FK assinatura_solicitacao | Solicitacao associada. |
| assinatura_registro_id | uuid | 36 | Nao | FK assinatura_registro | Assinatura associada quando houver. |
| usuario_id | uuid | 36 | Nao | FK usuario | Usuario executor. |
| acao | enum | Criar; Submeter; Aprovar; Rejeitar; Assinar; Falhar; Cancelar; Inativar; Reativar; Encerrar | Sim | Auditoria | Acoes consolidadas do material. |
| status_anterior | string | Nao informado no material | Nao | Campo simples | Estado anterior. |
| status_posterior | string | Nao informado no material | Nao | Campo simples | Estado posterior. |
| motivo | texto | Nao informado no material | Nao | Campo simples | Motivo de rejeicao, cancelamento ou falha. |
| ip | string | Nao informado no material | Nao | Auditoria | Material informa IP em historico. |
| correlation_id | string | Nao informado no material | Nao | Observabilidade | Usado quando houver API. |
| criado_em | datetime | ISO 8601 | Sim | Auditoria | Timestamp do evento. |

### 10.7 Entidade assinatura_evidencia

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador da evidencia. |
| assinatura_registro_id | uuid | 36 | Sim | FK assinatura_registro | Assinatura associada. |
| tipo_evidencia | enum | Payload; Documento assinado; Log; Certificado; Carimbo de tempo; Hash | Sim | Campo simples | Parte dos tipos e lacuna de completude quando nao comprovados. |
| valor_evidencia | texto | Nao informado no material | Nao | Campo protegido | Conteudo ou referencia segura. |
| arquivo_documental_id | uuid | 36 | Nao | FK documento/anexo | Arquivo armazenado no repositorio documental. |
| hash_documento | string | Nao informado no material | Nao | Campo tecnico | Nao informado no material. |
| algoritmo_hash | string | Nao informado no material | Nao | Campo tecnico | Nao informado no material. |
| provedor_assinatura | string | Nao informado no material | Nao | Campo simples | Nao informado no material. |
| criado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora de criacao. |

### 10.8 Entidade assinatura_politica

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador da politica. |
| tenant_id | uuid | 36 | Sim | FK tenant | Politica por tenant. |
| tipo_documento | string | Nao informado no material | Sim | Chave funcional | Define o tipo de documento. |
| criterio_minimo_assinaturas | inteiro | >= 1 | Sim | Campo simples | Material comprova dois em contrato; demais tipos exigem validacao. |
| permite_assinatura_publica | boolean | true/false | Sim | Campo simples | Default deve ser falso ate validacao. |
| exige_aprovacao_previa | boolean | true/false | Sim | Campo simples | Fluxo do material contempla aprovacao. |
| exige_ordem | boolean | true/false | Nao | Campo simples | Nao informado no material. |
| prazo_padrao_horas | inteiro | >= 0 | Nao | Campo simples | Nao informado no material. |
| status | enum | Ativa; Inativa | Sim | Estado | Controla uso da politica. |
| criado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora de criacao. |
| atualizado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora de atualizacao. |

### 10.9 Entidade assinatura_notificacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador da notificacao. |
| solicitacao_id | uuid | 36 | Sim | FK assinatura_solicitacao | Solicitacao associada. |
| signatario_id | uuid | 36 | Nao | FK assinatura_signatario | Destinatario quando aplicavel. |
| canal | enum | Email; Sistema; Outro | Sim | Campo simples | Canais finais nao informados no material. |
| destinatario | string | Nao informado no material | Sim | Campo pessoal | E-mail, usuario ou canal. |
| tipo | enum | Convite; Lembrete; Sucesso; Falha; Cancelamento; Expiracao | Sim | Campo simples | Tipos funcionais. |
| status | enum | Pendente; Enviada; Falha; Cancelada | Sim | Estado | Status da notificacao. |
| enviado_em | datetime | ISO 8601 | Nao | Auditoria | Data/hora de envio. |
| erro | texto | Nao informado no material | Nao | Campo simples | Detalhe seguro da falha. |

## 11. Dicionario de dados implantavel

### 11.1 Campos comprovados no material

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| contrato_id | uuid | 36 | Nao informado no material | FK contrato/documento | Identifica o contrato pai da assinatura. |
| usuario_id | uuid | 36 | Nao informado no material | FK usuario | Identifica o signatario. |
| tipo_assinatura | string | Nao informado no material | Nao informado no material | Campo simples | Classificacao da assinatura. |
| dados_assinatura | texto | Nao informado no material | Nao informado no material | Campo protegido | Payload da assinatura. |
| assinado_em | datetime | ISO 8601 | Nao informado no material | Auditoria | Data/hora da assinatura. |
| criador_id | uuid | 36 | Nao informado no material | FK usuario | Responsavel por criar o registro. |
| criado_por | uuid/string | Nao informado no material | Nao informado no material | Referencia usuario | Campo presente no material; tipo final nao informado. |

### 11.2 Campos funcionais necessarios para implantacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador tecnico das entidades. |
| tenant_id | uuid | 36 | Sim | FK tenant | Obrigatorio para isolamento. |
| empresa_id | uuid | 36 | Nao | FK empresa | Obrigatorio quando documento for por empresa. |
| solicitacao_id | uuid | 36 | Sim | FK assinatura_solicitacao | Relaciona registros do fluxo. |
| documento_ref_id | uuid | 36 | Sim | FK assinatura_documento_ref | Referencia documento. |
| documento_id | uuid | 36 | Sim | FK documento | Documento armazenado no Epros. |
| documento_versao_id | uuid | 36 | Nao | FK versao documental | Versao especifica assinada. |
| modulo_dono | string | Nao informado no material | Sim | Referencia funcional | Modulo responsavel. |
| tipo_documento | string | Nao informado no material | Sim | Politica | Tipo funcional do documento. |
| titulo_documento | string | Nao informado no material | Nao | Campo simples | Exibicao. |
| solicitante_usuario_id | uuid | 36 | Sim | FK usuario | Usuario solicitante. |
| signatario_id | uuid | 36 | Nao | FK assinatura_signatario | Signatario esperado. |
| pessoa_id | uuid | 36 | Nao | FK pessoa | Signatario externo cadastrado. |
| nome_exibicao | string | Nao informado no material | Nao | Campo simples | Nome do signatario. |
| email | string | Nao informado no material | Nao | Campo pessoal | Contato de convite. |
| papel_assinatura | string | Nao informado no material | Nao | Campo simples | Papel do signatario. |
| ordem | inteiro | >= 1 | Nao | Campo simples | Ordem de assinatura. |
| obrigatorio | boolean | true/false | Sim | Campo simples | Conta para completude. |
| criterio_minimo_assinaturas | inteiro | >= 1 | Sim | Politica | Quantidade minima. |
| status | enum | Conforme entidade | Sim | Estado | Estado funcional. |
| status_anterior | string | Nao informado no material | Nao | Auditoria | Estado anterior. |
| status_posterior | string | Nao informado no material | Nao | Auditoria | Estado posterior. |
| motivo | texto | Nao informado no material | Nao | Campo simples | Motivo de acao. |
| motivo_rejeicao | texto | Nao informado no material | Nao | Campo simples | Obrigatorio quando rejeitada. |
| mensagem_falha | texto | Nao informado no material | Nao | Campo simples | Erro seguro. |
| ip | string | Nao informado no material | Nao | Auditoria | IP do evento quando disponivel. |
| ip_origem | string | Nao informado no material | Nao | Auditoria | IP de assinatura quando disponivel. |
| user_agent | string | Nao informado no material | Nao | Auditoria | Navegador/dispositivo quando disponivel. |
| correlation_id | string | Nao informado no material | Nao | Observabilidade | Rastreio de API. |
| tipo_evidencia | enum | Payload; Documento assinado; Log; Certificado; Carimbo de tempo; Hash | Sim | Campo simples | Classificacao da evidencia. |
| valor_evidencia | texto | Nao informado no material | Nao | Campo protegido | Conteudo ou referencia segura. |
| arquivo_documental_id | uuid | 36 | Nao | FK documento/anexo | Evidencia documental. |
| hash_documento | string | Nao informado no material | Nao | Campo tecnico | Lacuna no material. |
| algoritmo_hash | string | Nao informado no material | Nao | Campo tecnico | Lacuna no material. |
| provedor_assinatura | string | Nao informado no material | Nao | Campo simples | Lacuna no material. |
| permite_assinatura_publica | boolean | true/false | Sim | Politica | Default deve ser falso ate validacao. |
| exige_aprovacao_previa | boolean | true/false | Sim | Politica | Fluxo de aprovacao. |
| exige_ordem | boolean | true/false | Nao | Politica | Nao informado no material. |
| prazo_padrao_horas | inteiro | >= 0 | Nao | Politica | Nao informado no material. |
| prazo_assinatura | datetime | ISO 8601 | Nao | Campo simples | Nao informado no material. |
| canal | enum | Email; Sistema; Outro | Sim | Notificacao | Canais finais nao informados. |
| destinatario | string | Nao informado no material | Sim | Notificacao | Destinatario da mensagem. |
| tipo | enum | Convite; Lembrete; Sucesso; Falha; Cancelamento; Expiracao | Sim | Notificacao | Tipo da notificacao. |
| enviado_em | datetime | ISO 8601 | Nao | Auditoria | Data/hora de envio. |
| erro | texto | Nao informado no material | Nao | Campo simples | Erro de notificacao. |
| criado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora de criacao. |
| atualizado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora de atualizacao. |
| concluido_em | datetime | ISO 8601 | Nao | Auditoria | Data/hora de conclusao. |

## 12. APIs funcionais

| API | Metodo | Finalidade | Entrada | Saida | Observacao |
|---|---|---|---|---|---|
| /assinaturas | POST | Criar solicitacao de assinatura. | Documento, signatarios, criterio. | Solicitacao criada. | Endpoint final nao informado no material. |
| /assinaturas | GET | Listar solicitacoes. | Status, periodo, responsavel, documento. | Lista paginada. | Filtros informados: status, periodo, responsavel. |
| /assinaturas/{id} | GET | Consultar detalhe. | Identificador. | Dados, historico, anexos, aprovacao. | Tela de detalhe informada. |
| /assinaturas/{id}/submeter | POST | Submeter para analise. | Identificador. | Status em analise. | Fluxo informado. |
| /assinaturas/{id}/aprovar | POST | Aprovar solicitacao. | Identificador e decisao. | Pendente assinatura. | Fluxo informado. |
| /assinaturas/{id}/rejeitar | POST | Rejeitar solicitacao. | Motivo. | Rascunho ou rejeitada. | Motivo deve ser registrado. |
| /assinaturas/{id}/assinar | POST | Registrar assinatura. | Tipo e dados da assinatura. | Assinatura concluida/parcial. | Baseado em assinatura simples comprovada. |
| /assinaturas/{id}/cancelar | POST | Cancelar solicitacao. | Motivo. | Cancelada. | Politica final nao informada. |
| /assinaturas/{id}/inativar | POST | Inativar registro. | Motivo. | Inativa. | Fluxo informado. |
| /assinaturas/{id}/reativar | POST | Reativar registro. | Motivo. | Concluida/ativa. | Fluxo informado. |
| /assinaturas/{id}/historico | GET | Consultar auditoria. | Identificador. | Historico. | Deve respeitar permissao de auditoria. |
| /assinaturas/relatorios/posicao | GET | Relatorio de posicao geral. | Periodo e status. | Indicadores. | Relatorio informado. |
| /assinaturas/relatorios/auditoria | GET | Relatorio de auditoria. | Periodo, usuario, documento. | Trilha. | Relatorio informado. |

## 13. Telas e experiencia operacional

### 13.1 Lista de assinaturas

| Elemento | Comportamento |
|---|---|
| Filtros | Status, periodo e responsavel. |
| Acoes | Novo, exportar, visualizar detalhe. |
| Indicadores | Quantidade pendente, parcial, concluida, falha e expirada. |
| Permissao | Exibe apenas documentos acessiveis ao usuario. |

### 13.2 Detalhe da assinatura

| Aba | Conteudo |
|---|---|
| Dados | Documento, tipo, solicitante, signatarios, status e criterio minimo. |
| Historico | Transicoes, usuario, data/hora, IP e motivo. |
| Anexos | Documento assinado e evidencias. |
| Aprovacao | Decisao, motivo, aprovador e data/hora quando aplicavel. |

### 13.3 Painel gestor

| Indicador | Descricao |
|---|---|
| Pendentes | Assinaturas aguardando acao. |
| Em atraso | Solicitacoes vencidas ou perto do vencimento. |
| Concluidas | Assinaturas finalizadas no periodo. |
| Falhas | Tentativas com erro. |
| Auditoria | Acoes criticas por usuario e documento. |

## 14. Relatorios

| ID | Nome | Descricao | Filtros | Saida |
|---|---|---|---|---|
| REL-ASS-001 | Posicao geral | Snapshot por status. | Periodo, tipo de documento, responsavel. | Totais por status e fila. |
| REL-ASS-002 | Auditoria de alteracoes | Trilha por periodo. | Periodo, usuario, documento, acao. | Eventos auditaveis. |

## 15. Integracoes

| Integracao | Direcao | Evento/Acao | Dados minimos | Regra |
|---|---|---|---|---|
| Gestao Eletronica de Documentos | Saida/Entrada | Vincular documento e evidencia. | Documento, versao, assinatura. | Documento assinado deve preservar versao. |
| Workflow | Entrada/Saida | Aprovar, rejeitar, continuar fluxo. | Solicitacao, status, responsavel. | Assinatura pode depender de aprovacao. |
| Identidade | Entrada | Validar usuario e permissao. | Usuario, perfil, tenant, empresa. | Assinatura exige contexto autorizado. |
| SOA e Colaboracao | Saida | Notificar signatario/gestor. | Destinatario, tipo, link seguro. | Notificacao deve respeitar privacidade. |
| Modulo dono do documento | Saida | Atualizar estado do documento. | Documento, status, assinaturas. | Ativacao quando criterio minimo for cumprido. |
| API Gateway e OpenAPI | Entrada/Saida | Expor endpoints. | Token, rota, correlationId. | APIs devem seguir padrao Epros. |

## 16. Criterios de aceite

| ID | Criterio |
|---|---|
| CA-ASS-001 | Usuario sem permissao de assinatura nao consegue assinar documento. |
| CA-ASS-002 | Assinatura valida registra documento, usuario, tipo, dados e data/hora. |
| CA-ASS-003 | Documento com criterio de duas assinaturas so muda para concluido/ativo apos a segunda assinatura valida. |
| CA-ASS-004 | Falha de assinatura retorna mensagem segura e registra trilha quando aplicavel. |
| CA-ASS-005 | Historico exibe transicoes com usuario, timestamp e IP quando disponivel. |
| CA-ASS-006 | Lista permite filtrar por status, periodo e responsavel. |
| CA-ASS-007 | Detalhe exibe dados, historico, anexos e aprovacao quando aplicavel. |
| CA-ASS-008 | Relatorio de posicao geral apresenta totais por status. |
| CA-ASS-009 | Relatorio de auditoria permite consultar a trilha por periodo. |
| CA-ASS-010 | Campos sem informacao de tipo/tamanho no material estao marcados como Nao informado no material. |

## 17. Testes funcionais

| ID | Cenario | Resultado esperado |
|---|---|---|
| CT-ASS-001 | Criar solicitacao valida. | Status inicial registrado. |
| CT-ASS-002 | Submeter sem dados obrigatorios. | Erro de validacao. |
| CT-ASS-003 | Assinar sem permissao. | Assinatura bloqueada. |
| CT-ASS-004 | Assinar com dados validos. | Assinatura registrada. |
| CT-ASS-005 | Assinar primeiro signatario em documento com dois obrigatorios. | Documento permanece pendente/parcial. |
| CT-ASS-006 | Assinar segundo signatario obrigatorio. | Documento muda para concluido/ativo. |
| CT-ASS-007 | Falha operacional de assinatura. | Mensagem segura e trilha. |
| CT-ASS-008 | Inativar assinatura concluida. | Status inativo e historico. |
| CT-ASS-009 | Reativar assinatura inativa. | Status reativado e historico. |
| CT-ASS-010 | Exportar relatorio de auditoria sem permissao. | Acesso negado. |

## 18. Notas de rodape

1. Campos e entidades de politica, evidencia, notificacao, versao documental, hash, provedor, prazo, assinatura publica e criterios por tipo de documento foram estruturados como necessidades de implantacao e/ou padrao corporativo quando o material nao trouxe detalhamento suficiente; por isso, as decisoes finais correspondentes estao registradas como lacunas na MC.
