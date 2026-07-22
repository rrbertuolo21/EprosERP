# MC 3 Plataforma Compartilhada - Configuracao V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Plataforma Compartilhada |
| Submodulo | Configuracao |
| Versao | V1 |
| Data | 2026-06-06 |

## 2. Matriz de completude

| Area | Status | Evidencia funcional consolidada | Lacuna | Acao recomendada | Prioridade | Dono sugerido |
|---|---|---|---|---|---|---|
| Hub de configuracao | Coberto | Menu, wrapper, hub, secoes e links diretos. | UX final e permissao visual precisam validacao. | Desenhar hub final. | P1 | Produto/UX |
| Parametros tenant | Parcial | settings, settings2 e site_config com muitas chaves. | Modelo final chave/valor versus entidades especificas precisa decisao. | Validar arquitetura de parametros. | P0 | Plataforma |
| Modulos habilitados | Parcial | Flags enabled/disabled e leitura runtime. | Origem contratual versus edicao tenant precisa regra final. | Integrar com assinatura/limites. | P0 | Aplicativo/Plataforma |
| Status auxiliares | Coberto | Status de fatura, lead, tarefa, ticket, taxrates, fontes, marcos, categorias e tags. | Fronteira com modulos donos precisa governanca. | Definir ownership por lista. | P1 | Produto |
| E-mail/SMTP | Parcial | Configuracao SMTP, templates, fila, teste e reprocessamento. | Politica de segredo, limites e template final nao completos. | Padronizar envio e seguranca. | P0 | SOA/Seguranca |
| Captcha/seguranca | Parcial | Captcha, senha, bloqueio, manutencao e IP banidos. | Politica final de seguranca central precisa alinhar com identidade. | Integrar com Identidade. | P0 | Seguranca |
| Campos personalizados | Parcial | Custom fields por entidade. | Tipos finais, validacoes, indexacao e relatorios nao definidos. | Criar padrao unico. | P1 | Plataforma |
| Arquivos/storage | Parcial | Upload temporario, download, servidores, tokens, fila e politicas de arquivo. | Parte pertence a documentos/storage; contrato final precisa separacao. | Fechar fronteira com GED. | P0 | Plataforma/GED |
| Internacionalizacao | Parcial | Idiomas, chaves, conteudos, import/export e reload. | Cobertura de strings hardcoded e workflow de traducao incompletos. | Definir processo i18n final. | P1 | Plataforma |
| Tarefas background | Parcial | Tabelas task/log e tarefas de poda, fila, notificacao e plugin. | Agendamento externo e SLA nao informados. | Definir scheduler Epros. | P0 | Plataforma/Ops |
| Instalacao/upgrades | Parcial | Pre-check, setup DB, import, config e upgrades. | Fluxo final cloud/Siser nao definido. | Adaptar a operacao SaaS. | P1 | DevOps/Siser |
| Concorrencia otimista | Parcial | RowVersion em operacoes selecionadas. | Quais entidades do Epros exigem versao nao definido. | Criar politica de concorrencia. | P0 | Arquitetura |
| Auditoria/excecoes | Parcial | Exceptions, LoggingRow, IsActive e historico. | Retencao, mascaramento e padrao unico de log precisam definicao. | Integrar com Compliance. | P0 | Seguranca/Compliance |
| Plugins/temas | Parcial | Plugins, temas, configuracoes, ordem e instalacao. | Modelo final de extensibilidade precisa decisao. | Definir se Epros tera plugins tenant. | P2 | Produto/Arquitetura |
| APIs | Parcial | Rotas e endpoints de material existem, mas dispersos. | Contrato final do Epros nao informado. | Publicar API padrao no gateway. | P0 | Plataforma/API |
| Testes | Parcial | Cenarios de hub, modulo, SMTP, upload, versao, i18n e API. | Faltam testes de segredo, cache, auditoria, scheduler e fronteiras. | Expandir suite automatizada. | P0 | QA |

## 3. Pendencias criticas P0

1. Definir modelo final de parametros: chave/valor puro, entidades especificas ou hibrido.
2. Separar parametro global Siser, parametro tenant, parametro empresa e preferencia de usuario.
3. Definir invalidacao de cache para alteracoes de configuracao.
4. Integrar flags de modulos com assinatura, limites e permissoes.
5. Definir politica de segredos para SMTP, storage, APIs e demais credenciais.
6. Fechar fronteira com Gestao Eletronica de Documentos para upload, servidores, tokens e politicas de arquivo.
7. Definir scheduler oficial para tarefas em segundo plano e logs.
8. Definir politica de concorrencia otimista por entidade.
9. Criar contrato final de APIs no API Gateway.
10. Normalizar auditoria, exclusao logica e retencao com Compliance.

## 4. Perguntas para validacao humana

| Pergunta | Impacto |
|---|---|
| O Epros tera editor generico de chave/valor visivel ao cliente ou apenas telas especificas? | Define UX e risco operacional. |
| Quais configuracoes poderao ser alteradas pelo tenant e quais somente pela Siser? | Define permissao e suporte. |
| Flags de modulo sao editaveis ou apenas resultado da assinatura? | Define integracao comercial. |
| Campos personalizados entram no MVP? Em quais entidades? | Define modelo de dados extensivel. |
| Storage sera configuravel por tenant ou centralizado pela Siser? | Define seguranca, custo e GED. |
| O Epros tera plugins/temas instalaveis por tenant? | Define extensibilidade. |
| Agendamento de tarefas sera interno ao Epros ou externo via infraestrutura? | Define operacao. |
| Quais parametros exigem historico antes/depois obrigatorio? | Define auditoria. |

## 5. Itens de construcao

| Item | Entrega esperada | Prioridade |
|---|---|---|
| Catalogo de parametros | Grupo, chave, tipo, escopo, segredo, valor e historico. | P0 |
| Hub de configuracao | Menu, secoes, permissao e links diretos. | P1 |
| Modulos habilitados | Flags, leitura runtime, cache e origem contratual. | P0 |
| E-mail/SMTP | Configuracao segura, teste, templates e fila. | P0 |
| i18n | Idiomas, chaves, conteudos, import/export e reload. | P1 |
| Campos personalizados | Modelo por entidade, tipo, ordem e exibicao. | P1 |
| Upload/storage | Politica, servidor, token, fila e fronteira GED. | P0 |
| Tarefas background | Scheduler, task, log, status e alertas. | P0 |
| Auditoria/excecoes | Log padrao, historico antes/depois e retencao. | P0 |
| Concorrencia | Versao por entidade e erro funcional. | P0 |
| APIs | Contratos no padrao Epros. | P0 |
| Testes | Suite de configuracao transversal. | P0 |

## 6. Criterios de aceite de completude

| ID | Criterio |
|---|---|
| MC-CFG-001 | EF possui modelo de dados antes do dicionario. |
| MC-CFG-002 | Todos os campos do dicionario possuem tipo, tamanho/dominio, obrigatoriedade, relacao e regra/observacao. |
| MC-CFG-003 | Campos sem informacao no material estao marcados como Nao informado no material. |
| MC-CFG-004 | EF preserva inventario de tabelas e estruturas do material sem nomes de plataformas anteriores. |
| MC-CFG-005 | MC explicita fronteiras com cobranca, usuarios, documentos, tickets, formularios, produtos e analytics. |
| MC-CFG-006 | Segredos, cache, scheduler, storage, auditoria e concorrencia aparecem como pendencias P0 quando nao fechados. |
