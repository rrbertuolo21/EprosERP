# MC 0_APLICATIVO ISOLAMENTO_DE_DADOS V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** APLICATIVO  
**Submodulo:** ISOLAMENTO_DE_DADOS  
**ID funcional:** APP-TEN-009  
**Versao:** V1  
**Status:** Pronto para validacao humana  
**Data:** 2026-06-06

## 1. Objetivo

Esta matriz mede a completude funcional do isolamento de dados do Epros, incluindo contexto, classificacao de entidades, filtros de leitura, auditoria, soft delete, owner, banco por tenant, jobs e configuracoes.

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
| Classificacao obrigatoria de entidades | Coberto | Entidade sem classificacao deve falhar. | Processo de governanca ainda nao detalhado. | Criar checklist de criacao de entidade. | P0 | Dados/Arquitetura |
| Atribuicao de contexto na criacao | Coberto | Tenant/empresa/owner preenchidos na inclusao. | Modelo final de campo depende de estrategia. | Definir contrato de contexto. | P0 | IDENTIDADE_E_CONTEXTO_TENANT |
| Filtro automatico de leitura | Parcial | Query filter por tenant e filtros explicitos por owner/empresa identificados. | Nao ha uma unica estrategia oficial. | Escolher padrao por deployment. | P0 | API_GATEWAY_E_OPENAPI |
| Entidades comuns/globais | Parcial | Catalogos globais identificados. | Lista final precisa aprovacao. | Criar cadastro governado de excecoes. | P0 | CATALOGOS_GLOBAIS_SAAS |
| Auditoria transversal | Parcial | Datas, usuario, owner e campos de auditoria identificados. | Bypass de auditoria em operacao especial. | Eliminar bypass ou registrar excecao formal. | P0 | COMPLIANCE_LGPD_SOX_IFRS |
| Empresa/grupo/usuario em registros | Parcial | EmpresaID, GrupoID e UsuarioID identificados. | Relacao final entre tenant, empresa e grupo precisa definicao. | Fechar modelo multiempresa. | P0 | ONBOARDING_E_EMPRESA |
| Owner logico | Parcial | created_by/creator identificado para isolamento e settings. | Mapeamento com tenant/empresa nao fechado. | Definir equivalencia owner x tenant. | P0 | IDENTIDADE_E_CONTEXTO_TENANT |
| Banco por tenant | Decisao | Tenant com dominio/base/status identificado. | Estrategia fisica nao definida para o Epros. | Decidir se sera opcao de deployment. | P1 | Infraestrutura |
| Jobs tenant-aware | Parcial | Filas/jobs por tenant identificados. | Contrato de contexto para jobs nao fechado. | Definir padrao obrigatorio de execucao. | P0 | WORKFLOW |
| Soft delete | Parcial | deleted=0/1 e restauracao identificados. | Entidades e retencao nao definidas. | Criar politica de exclusao/restauracao. | P0 | COMPLIANCE_LGPD_SOX_IFRS |
| Lock otimista | Parcial | Conflito de alteracao identificado. | Entidades obrigatorias nao definidas. | Definir dominios com lock. | P1 | Dados/Arquitetura |
| Settings por owner | Coberto | key+owner, is_public e invalidacao de cache identificados. | Cache indefinido/remember forever precisa politica. | Definir TTL e invalidacao. | P0 | CONFIGURACAO |
| Cache de entidades | Parcial | Cache LRU e cache settings identificados. | Escopo final nao definido. | Padronizar cache de metadados. | P1 | PLATAFORMA_COMPARTILHADA/CONFIGURACAO |
| Entidades de dominio protegidas | Parcial | 46 entidades tenantizadas e 55 entidades transversais inventariadas. | Campo-a-campo fica para modulos donos. | Garantir que cada modulo detalhe suas entidades. | P0 | Todos os modulos |
| Testes automatizados | Lacuna | Nao identificados no material. | Alto risco de vazamento sem testes. | Criar suite obrigatoria de isolamento. | P0 | QA |
| Homologacao manual | Parcial | Replay manual sugerido. | Massa multi-tenant nao definida. | Criar massa com tenants/empresas distintos. | P1 | Implantacao |

## 4. Itens criticos para validacao humana

1. Definir estrategia oficial de isolamento: coluna tenant, empresa/grupo, owner logico, banco por tenant ou combinacao por deployment.
2. Aprovar lista final de entidades comuns/globais.
3. Definir contrato unico de contexto entregue por identidade.
4. Definir se operacoes especiais podem ignorar auditoria e sob quais controles.
5. Definir politica de soft delete, restauracao e retencao.
6. Definir entidades que exigem lock otimista.
7. Definir padrao obrigatorio para jobs e filas tenant-aware.
8. Definir cache de settings e invalidacao.
9. Criar testes automatizados contra vazamento cross-tenant.

## 5. Backlog refinado

| Prioridade | Item | Justificativa |
|---|---|---|
| P0 | Criar validador de entidade sem classificacao. | Impede dado sem isolamento. |
| P0 | Criar testes de consulta cross-tenant. | Evita vazamento. |
| P0 | Criar testes de criacao sem contexto. | Garante bloqueio. |
| P0 | Definir entidades comuns globais. | Evita excecoes indevidas. |
| P0 | Unificar auditoria e remover bypass. | Evita perda de rastreabilidade. |
| P0 | Definir contexto para jobs. | Evita processamento em tenant errado. |
| P1 | Definir banco por tenant como opcao ou nao. | Impacto arquitetural grande. |
| P1 | Definir lock otimista por dominio. | Evita sobrescrita concorrente. |
| P1 | Criar relatorio de integridade de contexto. | Ajuda implantacao e suporte. |

## 6. Controle de cobertura funcional

| Bloco funcional | Situacao | Conteudo incorporado | Pendencia de conferencia |
|---|---|---|---|
| Identificacao funcional | Incorporado | ID APP-TEN-009. | Nenhuma. |
| Regras de isolamento | Incorporado | Contexto, filtro, classificacao, auditoria e entidades comuns. | Estrategia final. |
| Modelo de dados | Incorporado | ModeloBaseAuditavel, EntidadeTenantizadaizada, Setting, Tenant fisico, auditoria e soft delete. | Modelo fisico final. |
| Dicionario de dados | Incorporado | Campos transversais preservados. | Campos de dominio nos modulos donos. |
| Fluxos | Incorporado | Criacao, consulta, settings, banco por tenant e excecoes. | Jobs finais. |
| Telas | Nao se aplica | Pacote transversal sem tela propria. | Telas consumidoras herdam regras. |
| Auditoria | Parcial | Campos e eventos definidos. | Retencao e bypass. |
| Testes | Lacuna | Ausencia identificada. | Suite obrigatoria. |

## 7. Notas de rodape

[^agente-001]: Itens de estrategia unificada, relatorios de integridade, mensagens, requisitos nao funcionais e backlog refinado foram organizados pelo agente a partir do material disponivel. O que nao estava explicitamente informado foi marcado como lacuna ou decisao.

