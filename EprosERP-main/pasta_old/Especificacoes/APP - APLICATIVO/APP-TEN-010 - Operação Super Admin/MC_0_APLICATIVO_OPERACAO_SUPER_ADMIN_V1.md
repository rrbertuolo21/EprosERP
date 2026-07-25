# MC 0_APLICATIVO OPERACAO_SUPER_ADMIN V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** APLICATIVO  
**Submodulo:** OPERACAO_SUPER_ADMIN  
**ID funcional:** APP-TEN-010  
**Versao:** V1  
**Status:** Pronto para validacao humana  
**Data:** 2026-06-06

## 1. Objetivo

Esta matriz mede a completude funcional da operacao super admin do Epros, cobrindo dashboard global, clientes/tenants, equipe Siser, configuracoes globais, instalacao, atualizacao, CMS/landing, comunicador, rotinas e execucao em massa.

## 2. Legenda de status

| Status | Significado |
|---|---|
| Coberto | Capacidade possui regra, fluxo, entidade ou contrato suficiente para construcao inicial. |
| Parcial | Capacidade existe, mas precisa decisao, complemento ou validacao. |
| Lacuna | Capacidade citada ou esperada sem especificacao suficiente. |
| Decisao | Exige validacao humana antes de construcao. |

## 3. Matriz de completude

| Capacidade | Status | Evidencia funcional consolidada | Lacuna / risco | Acao recomendada | Prioridade | Dependencias |
|---|---|---|---|---|---|---|
| Menu super admin | Parcial | Menu global fixo e separado do menu tenant. | Modelo final de permissoes por item nao detalhado. | Criar matriz de permissoes interna. | P0 | Permissoes |
| Dashboard global | Parcial | Estatisticas, assinaturas, pagamentos e grafico anual identificados. | Fonte oficial e formulas dos indicadores nao fechadas. | Definir catalogo de indicadores. | P0 | Assinatura/Cobranca |
| Gestao de tenants | Parcial | Operacao sobre clientes/owners e assinaturas identificada. | Campos completos do tenant/business nao informados. | Consolidar com Onboarding e Limites. | P0 | Onboarding |
| Assinatura manual/offline | Decisao | Super admin pode aprovar assinatura offline e alterar status/datas. | Bug/ambiguidade de precedencia de datas. | Definir regra oficial de aprovacao manual. | P0 | Limites/Pedidos |
| Configuracoes globais | Parcial | Empresa, dominios, banco, formatos, gateways, trial, offline, captcha e logs identificados. | Modelo seguro de settings nao detalhado. | Criar entidade governada de configuracao. | P0 | Configuracao |
| Segredos de gateway | Lacuna | Chaves de provedores identificadas. | Modelo de criptografia/mascaramento/rotacao nao informado. | Definir cofre/segredo funcional. | P0 | Seguranca/Cobranca |
| Instalador | Coberto | Fluxo requirements, banco, settings, admin e finish. | Reentrada e reprocessamento precisam regra formal. | Definir estado de instalacao e autorizacao especial. | P0 | Plataforma |
| Atualizador | Parcial | Check, pendencias, update, cache clear e logs identificados. | Janela, rollback e permissao elevada nao detalhados. | Especificar governanca de update. | P0 | Plataforma |
| Modo demo | Parcial | Bloqueio de mutacoes em registros protegidos identificado. | Lista definitiva de entidades protegidas nao fechada. | Definir matriz demo. | P1 | Seguranca |
| Comunicador | Coberto | Envio para owners/clientes e log com destinatarios/assunto/mensagem. | Retry, status de envio e templates nao detalhados. | Detalhar comunicacoes. | P1 | Comunicacoes |
| Notificacoes | Parcial | Expiracao de assinatura, boas-vindas e comunicacoes identificadas. | Canais, templates e agenda nao detalhados. | Definir catalogo de notificacoes. | P1 | Workflow |
| CMS/landing | Parcial | Landing, marketplace, paginas customizadas e newsletter identificados. | Campos completos nao informados. | Detalhar dicionario CMS. | P1 | Area Publica |
| Newsletter | Parcial | Listagem/export identificadas. | Consentimento, opt-out e retencao nao informados. | Definir privacidade. | P1 | Compliance |
| Equipe Siser | Parcial | Usuario interno, email unico, senha minima e primary_admin. | Papeis internos e protecoes finais nao detalhados. | Criar RBAC interno. | P0 | Permissoes |
| Execucao em massa | Parcial | Status draft/active/completed; logs passed/failed; idempotencia por tenant. | Aprovacao, sandbox e rollback nao definidos. | Criar fluxo de aprovacao e validacao. | P0 | Plataforma |
| Rotinas administrativas | Parcial | Cancelamento, update-plan, gateways, delete-database, update-email-domain. | Agenda, logs e responsabilidade por rotina nao detalhadas. | Especificar no Workflow. | P1 | Workflow |
| Testes automatizados | Lacuna | Nao identificados no material. | Alto risco operacional. | Criar suite para super admin. | P0 | QA |

## 4. Itens criticos para validacao humana

1. Definir matriz de permissoes interna do super admin.
2. Definir fonte e formula oficial dos indicadores do dashboard global.
3. Consolidar o modelo de tenant/business usado pela operacao Siser.
4. Definir regra de aprovacao manual/offline de assinatura e tratamento de datas.
5. Criar modelo seguro de configuracoes globais, com segredo, auditoria e cache.
6. Definir criptografia, mascaramento e rotacao de chaves de provedores.
7. Definir governanca de instalador e reentrada autorizada.
8. Definir governanca de atualizador: permissao, janela, rollback e logs.
9. Definir lista de mutacoes bloqueadas no modo demo.
10. Definir fluxo de aprovacao para execucao em massa.
11. Definir privacidade da newsletter e exportacoes.
12. Definir fronteira de CMS/landing versus area publica.

## 5. Backlog refinado

| Prioridade | Item | Justificativa |
|---|---|---|
| P0 | Criar RBAC interno do super admin. | Evita acesso global amplo sem segregacao. |
| P0 | Criar modelo governado de configuracao global. | Substitui settings soltos e escrita insegura. |
| P0 | Criar cofre/segredo funcional para gateways e licencas. | Protege dados sensiveis. |
| P0 | Especificar aprovacao manual de assinatura. | Evita datas/status inconsistentes. |
| P0 | Criar suite de testes para instalador, update, settings e execucao em massa. | Alto risco operacional. |
| P0 | Criar aprovacao dupla para execucao em massa. | Reduz risco de impacto em tenants. |
| P1 | Detalhar CMS/landing/newsletter. | Campos incompletos. |
| P1 | Criar catalogo de notificacoes. | Padroniza comunicacao. |
| P1 | Definir matriz de modo demo. | Evita mutacoes indevidas. |
| P1 | Definir catalogo de indicadores globais. | Dashboard confiavel. |

## 6. Controle de cobertura funcional

| Bloco funcional | Situacao | Conteudo incorporado | Pendencia de conferencia |
|---|---|---|---|
| Identificacao funcional | Incorporado | APP-TEN-010. | Nenhuma. |
| Menu super admin | Incorporado | Dashboard, users, plans, coupons, features, payment type, currency, country, website. | Permissoes internas. |
| Dashboard | Parcial | Stats e chart. | Formulas/fonte. |
| Tenants/assinaturas | Parcial | Clientes, owners, assinatura, status e offline approval. | Regra final em Limites/Pedidos. |
| Settings | Parcial | Dominios, banco, formatos, gateways, trial, offline, captcha, cron, logo. | Modelo seguro. |
| Instalador/update | Parcial | Wizard, requisitos, banco, admin, finish, updates e logs. | Governanca e rollback. |
| Comunicador | Incorporado | Destinatarios, assunto, mensagem e log. | Retry/status/template. |
| CMS/landing | Parcial | Landing, marketplace, custom pages, newsletter. | Dicionario completo. |
| Execucao em massa | Parcial | draft/active/completed, passed/failed. | Aprovacao e rollback. |
| Testes | Lacuna | Cenarios sugeridos. | Suite automatizada. |

## 7. Notas de rodape

[^agente-001]: A proposta de RBAC interno, cofre funcional de segredos, aprovacao dupla para execucao em massa e catalogo de indicadores foi criada pelo agente como encaminhamento de lacunas reais identificadas no material. Nao foi tratada como regra definitiva sem validacao humana.
