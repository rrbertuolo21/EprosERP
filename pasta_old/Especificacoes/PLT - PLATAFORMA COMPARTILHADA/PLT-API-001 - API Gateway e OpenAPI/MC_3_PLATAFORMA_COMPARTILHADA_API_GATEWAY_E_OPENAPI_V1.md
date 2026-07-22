# MC 3 Plataforma Compartilhada — API Gateway e OpenAPI V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Plataforma Compartilhada |
| Submodulo | API Gateway e OpenAPI |
| Versao | V1 |
| Data | 2026-06-06 |

## 2. Matriz de completude

| Area | Status | Evidencia funcional consolidada | Lacuna | Acao recomendada | Prioridade | Dono sugerido |
|---|---|---|---|---|---|---|
| Autenticacao | Parcial | Token, empresa, usuario, credencial, recuperacao de senha e erros principais. | Refresh token, MFA e politica de sessao nao detalhados. | Definir politica de autenticacao completa. | P0 | Plataforma/Seguranca |
| Tenant e empresa | Parcial | EmpresaId no login, tenant create/update e empresa ativa. | Regra final de troca de empresa ativa nao detalhada. | Definir contexto e troca segura. | P0 | Plataforma |
| Catalogo de rotas | Incompleto | Endpoints e dominios foram mapeados, mas dispersos. | Modelo de catalogo unificado nao implementado no material. | Construir catalogo de rotas. | P0 | Plataforma |
| OpenAPI | Parcial | Necessidade de catalogo unico e versionamento. | Pipeline de publicacao e OpenAPI 3.1 nao comprovados. | Criar geracao/publicacao por ambiente. | P0 | Plataforma |
| Autorizacao | Incompleto | Ha regras de acesso e varios riscos de endpoints sem autorizacao. | Autorizacao precisa ser obrigatoria no gateway. | Bloquear rotas sem politica explicita. | P0 | Seguranca |
| Rate limit | Incompleto | Gap identificado. | Nao existe politica detalhada no material. | Definir limite por tenant, cliente, usuario e rota. | P0 | Plataforma |
| Chaves de API | Incompleto | Gap identificado para parceiros/revenda. | Modelo de chave/segredo/rotacao ausente. | Implementar cliente de API e chave protegida. | P0 | Plataforma |
| Batch/sincronizacao | Parcial | Lotes mobile/PDV, clientes, fornecedores, vendas, inventario e retorno por item. | Politica de rollback total/parcial precisa decisao. | Definir contrato por rota. | P1 | Plataforma/Mobile |
| Download/PDF | Parcial | Download de documentos por API identificado. | Permissao e log precisam ser obrigatorios. | Padronizar download seguro. | P0 | Plataforma/Fiscal |
| Erros padronizados | Parcial | Mensagens principais e HTTP 403 identificados. | Catalogo final de erros nao definido. | Criar catalogo Epros de erros. | P0 | Plataforma |
| Logs/auditoria | Incompleto | Necessidade de audit log API registrada. | Campos, retencao e mascaramento precisam politica. | Implementar log tecnico e auditoria. | P0 | Compliance/Plataforma |
| Compatibilidade | Parcial | Versoes antigas e contratos dinamicos identificados. | Adaptadores precisam whitelist e prazo. | Criar camada de compatibilidade controlada. | P1 | Plataforma |
| Eventos assincronos | Parcial | Eventos de DFe, compra e venda identificados. | Schema e barramento nao detalhados. | Criar catalogo de eventos. | P1 | Plataforma |
| Testes automatizados | Parcial | Cenarios basicos existem. | Faltam testes de tenant, rate limit, lote, OpenAPI e seguranca. | Criar suite completa. | P0 | QA |

## 3. Pendencias criticas P0

1. Bloquear toda rota sem autenticacao/autorizacao explicitamente aprovada.
2. Construir catalogo unico de rotas com dono funcional, versao, permissao e status.
3. Publicar OpenAPI por ambiente somente com rotas aprovadas.
4. Definir politica de tenant e empresa ativa para web, mobile, PDV e cliente externo.
5. Implantar rate limit e chaves de API.
6. Padronizar catalogo de erros e correlationId.
7. Implementar log tecnico com mascaramento e retencao.
8. Garantir que payload nao possa substituir tenant/empresa autorizados.
9. Definir politica de lote: rollback total, parcial e retorno por item.
10. Cobrir autenticacao, autorizacao, tenant, OpenAPI, rate limit e lote com testes.

## 4. Perguntas para validacao humana

| Pergunta | Impacto |
|---|---|
| O Epros tera clientes externos de API no MVP ou apenas canais internos/mobile/PDV? | Define chave de API e portal de desenvolvedor. |
| O header de versao sera obrigatorio no MVP? | Define roteamento de versoes. |
| Rate limit sera por plano contratado, por cliente de API ou ambos? | Define modelo de limites. |
| Lotes mobile devem ter rollback total ou parcial por item? | Define contratos de sincronizacao. |
| Rotas publicas permitidas existem? Quais? | Define excecoes de seguranca. |
| Qual prazo padrao de suporte para rota depreciada? | Define governanca de versao. |
| Logs de requisicao devem ser retidos por quanto tempo? | Define compliance e custo. |

## 5. Itens de construcao

| Item | Entrega esperada | Prioridade |
|---|---|---|
| Catalogo de rotas | Rota, versao, dono, permissao, status e OpenAPI. | P0 |
| Autenticacao e tenant | Token, empresa ativa, escopos e troca segura. | P0 |
| Autorizacao obrigatoria | Politica por rota, modulo, perfil, usuario e cliente. | P0 |
| OpenAPI | Geracao, publicacao, ambiente, historico e arquivo. | P0 |
| Chaves e rate limit | Cliente de API, escopos, segredo, rotacao e limite. | P0 |
| Logs e auditoria | CorrelationId, mascaramento, retencao e eventos criticos. | P0 |
| Erros padronizados | Catalogo de codigos e formato unico. | P0 |
| Batch | Lote, item, retorno por item e rollback. | P1 |
| Compatibilidade | Adaptadores com whitelist e deprecacao. | P1 |
| Eventos | Catalogo de eventos e schemas. | P1 |

## 6. Criterios de aceite de completude

| ID | Criterio |
|---|---|
| MC-API-001 | EF possui modelo de dados antes do dicionario. |
| MC-API-002 | Todos os campos do dicionario possuem tipo, tamanho/dominio, obrigatoriedade, relacao e regra/observacao. |
| MC-API-003 | Campos sem tamanho conhecido estao marcados como Nao informado no material. |
| MC-API-004 | Toda rota ativa exige politica de acesso. |
| MC-API-005 | Toda rota externa publicada aparece no OpenAPI. |
| MC-API-006 | A MC explicita lacunas de rate limit, chaves, lote, logs e compatibilidade. |

