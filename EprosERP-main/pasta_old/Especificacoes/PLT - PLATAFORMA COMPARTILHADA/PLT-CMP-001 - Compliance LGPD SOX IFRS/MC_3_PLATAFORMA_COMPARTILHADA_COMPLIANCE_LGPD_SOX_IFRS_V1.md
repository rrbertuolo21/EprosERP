# MC 3 Plataforma Compartilhada - Compliance LGPD SOX IFRS V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Plataforma Compartilhada |
| Submodulo | Compliance LGPD SOX IFRS |
| Versao | V1 |
| Data | 2026-06-06 |

## 2. Matriz de completude

| Area | Status | Evidencia funcional consolidada | Lacuna | Acao recomendada | Prioridade | Dono sugerido |
|---|---|---|---|---|---|---|
| Conteudo operacional detalhado | Incompleto | Material declara ausencia de entidades operacionais extraidas. | Tabelas finais, endpoints e regras detalhadas precisam validacao. | Validar desenho funcional com juridico, seguranca, produto e financeiro. | P0 | Produto/Compliance |
| Agregado base | Parcial | Campos minimos: Id, TenantId, Codigo, Status, ResponsavelId. | Tipos, tamanhos, indices e relacionamentos completos nao informados. | Confirmar modelo de registro generico ou entidades especificas. | P0 | Plataforma |
| Historico | Parcial | Campos minimos: Acao, UsuarioId, PayloadJson; fluxo informa timestamp e IP. | Imutabilidade, retencao, protecao de payload e consulta nao detalhadas. | Definir padrao de trilha auditavel. | P0 | Seguranca/Compliance |
| Anexos | Parcial | Campo ArquivoId associado ao repositorio documental. | Tipos de documento, versao, retencao e acesso nao detalhados. | Fechar contrato com Gestao Eletronica de Documentos. | P1 | Plataforma/GED |
| Base legal | Incompleto | Requisito de registro por finalidade. | Catalogo final de bases legais nao definido. | Validar catalogo juridico. | P0 | Juridico/Compliance |
| Consentimento | Incompleto | Requisito de evidencia com timestamp, IP e versao do termo. | Revogacao, reconsentimento, canal e validade nao detalhados. | Especificar ciclo completo de consentimento. | P0 | Juridico/Produto |
| Direitos do titular | Incompleto | Requisitos de exportacao, retificacao, eliminacao e oposicao. | SLA, protocolo, aprovacao, bloqueios e execucao por modulo nao detalhados. | Criar processo e APIs por tipo de solicitacao. | P0 | Compliance/Produto |
| Mascaramento | Incompleto | Requisito de mascaramento por perfil. | Catalogo de campos sensiveis e regras por modulo nao existem. | Mapear campos sensiveis por modulo. | P0 | Seguranca/Dados |
| Auditoria de acesso a dado pessoal | Incompleto | Requisito de trilha imutavel de acesso. | Nivel de detalhe, volume, retencao e consulta nao definidos. | Definir politica de auditoria e armazenamento. | P0 | Seguranca/Arquitetura |
| Retencao e anonimizacao | Incompleto | Requisito de retencao e anonimizacao automatica pos-prazo. | Politicas por dado, bloqueios legais e execucao automatica nao definidos. | Criar motor de retencao. | P0 | Compliance/Arquitetura |
| Controles financeiros | Incompleto | Requisito de segregacao e log de alteracoes financeiras. | Perfis incompativeis, acoes criticas e excecoes nao definidos. | Mapear controles por modulo financeiro. | P0 | Financeiro/Controles |
| IFRS | Incompleto | Objetivo menciona configuracao IFRS por tenant. | Parametros, vigencia e consumidores nao definidos. | Validar escopo contabil/regulatorio. | P1 | Contabil/Produto |
| Telas | Parcial | Lista, detalhe, painel gestor e relatorios basicos. | Campos, acoes, permissoes e experiencia final nao detalhados. | Desenhar telas finais. | P1 | Produto/UX |
| APIs | Incompleto | Material nao informa endpoints finais. | Rotas, payloads, erros e autorizacao precisam definicao. | Publicar APIs no padrao Epros. | P0 | Plataforma/API |
| Testes | Parcial | Cenarios basicos de CRUD/workflow e mascaramento. | Faltam testes de consentimento, DSR, retencao, auditoria e controles financeiros. | Criar suite automatizada completa. | P0 | QA |

## 3. Pendencias criticas P0

1. Validar se o Epros usara modelo generico de registro de compliance, entidades especificas ou ambos.
2. Definir catalogo juridico de bases legais e finalidades.
3. Definir ciclo de consentimento: coleta, evidencia, versao, revogacao e reconsentimento.
4. Definir processo de direitos do titular com protocolo, SLA, aprovacao e execucao por modulo.
5. Mapear campos pessoais e sensiveis por modulo para mascaramento.
6. Definir trilha imutavel de acesso a dado pessoal e dado sensivel.
7. Definir motor de retencao, anonimizacao, eliminacao e bloqueios legais.
8. Mapear controles financeiros, segregacao de funcoes e eventos criticos.
9. Fechar escopo de configuracao IFRS por tenant.
10. Publicar endpoints finais e politica de autorizacao.

## 4. Perguntas para validacao humana

| Pergunta | Impacto |
|---|---|
| O MVP precisa cobrir todas as bases legais ou apenas consentimento e obrigacao legal? | Define catalogo inicial e complexidade juridica. |
| Direitos do titular serao atendidos por portal, backoffice Siser ou ambos? | Define UX, API e seguranca. |
| Qual SLA por tipo de solicitacao do titular? | Define prazos, alertas e relatorios. |
| Quais campos pessoais/sensiveis entram no primeiro ciclo de mascaramento? | Define varredura por modulo. |
| Auditoria de acesso sera registrada para toda visualizacao ou apenas revelacao/exportacao? | Define volume e custo. |
| Retencao automatica pode eliminar dados ou apenas anonimizar/bloquear no MVP? | Define risco operacional e juridico. |
| Quais eventos financeiros exigem segregacao de funcoes? | Define controles SOX. |
| Quais parametros IFRS realmente precisam existir por tenant? | Define escopo contabil. |

## 5. Itens de construcao

| Item | Entrega esperada | Prioridade |
|---|---|---|
| Modelo de compliance | Registro, historico, anexo e entidades especificas. | P0 |
| Catalogo juridico | Bases legais e finalidades. | P0 |
| Consentimento | Evidencia, termo, versao, revogacao e consulta. | P0 |
| Direitos do titular | Protocolo, fluxo, SLA, evidencias e execucao. | P0 |
| Mascaramento | Politica por campo, perfil, modulo e contexto. | P0 |
| Auditoria imutavel | Acesso a dados, alteracoes e exportacoes. | P0 |
| Retencao | Politicas, lotes, bloqueios, anonimizacao e eliminacao. | P0 |
| Controles financeiros | Segregacao, aprovacao, excecao e trilha. | P0 |
| IFRS | Parametros, vigencia e consumidores. | P1 |
| APIs | Rotas, contratos, erros e autorizacao. | P0 |
| Telas e relatorios | Lista, detalhe, painel, auditoria, DSR e retencao. | P1 |
| Testes | Suite de compliance transversal. | P0 |

## 6. Criterios de aceite de completude

| ID | Criterio |
|---|---|
| MC-CMP-001 | EF possui modelo de dados antes do dicionario. |
| MC-CMP-002 | Todos os campos do dicionario possuem tipo, tamanho/dominio, obrigatoriedade, relacao e regra/observacao. |
| MC-CMP-003 | Campos sem informacao no material estao marcados como Nao informado no material. |
| MC-CMP-004 | MC explicita que nao houve entidade operacional detalhada no material. |
| MC-CMP-005 | Base legal, consentimento, DSR, mascaramento, auditoria, retencao, controles financeiros e IFRS possuem lacunas registradas. |
| MC-CMP-006 | EF nao trata como comprovada nenhuma API final que o material nao informou. |
