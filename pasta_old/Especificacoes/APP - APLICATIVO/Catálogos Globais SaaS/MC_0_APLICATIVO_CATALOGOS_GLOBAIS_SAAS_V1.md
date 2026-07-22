# MC 0_APLICATIVO CATALOGOS_GLOBAIS_SAAS V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** APLICATIVO  
**Submodulo:** CATALOGOS_GLOBAIS_SAAS  
**ID funcional:** APP-TEN-007  
**Versao:** V1  
**Status:** Pronto para validacao humana  
**Data:** 2026-06-06

## 1. Objetivo

Esta matriz mede a completude funcional do submodulo de Catalogos Globais SaaS do Epros, separando o que esta suficientemente especificado do que precisa de decisao, complemento de dados ou validacao antes de implantacao.

## 2. Legenda de status

| Status | Significado |
|---|---|
| Coberto | Capacidade possui regra, entidade, fluxo ou tela suficiente para construcao inicial. |
| Parcial | Capacidade existe, mas possui lacuna de detalhe, decisao ou integracao. |
| Lacuna | Capacidade esperada ou citada sem especificacao suficiente. |
| Decisao | Exige validacao humana antes de construcao. |

## 3. Matriz de completude

| Capacidade | Status | Evidencia funcional consolidada | Lacuna / risco | Acao recomendada | Prioridade | Dependencias |
|---|---|---|---|---|---|---|
| Catalogos globais sem tenant | Coberto | Material indica paises, moedas, funcionalidades, cupons, configuracao publica e tipo de pagamento como catalogos globais. | Validar se algum catalogo devera ser regionalizado ou tenantizado no futuro. | Confirmar politica global dos catalogos. | P1 | IDENTIDADE_E_CONTEXTO_TENANT |
| CRUD padrao de catalogos | Coberto | Listagem, verificacao de duplicidade, salvamento, atualizacao e falha invalida foram identificados. | Mensagens finais precisam padronizacao em portugues. | Criar catalogo de mensagens. | P2 | DASHBOARD_E_LAYOUT |
| Permissao administrativa | Coberto | Telas administrativas exigem perfil administrativo. | Matriz detalhada por acao nao informada. | Integrar com usuarios e permissoes. | P0 | USUARIOS_E_PAPEIS; PERMISSOES_DE_MENU |
| Pais | Coberto | Campo `CountryId`, `Name` 50 caracteres, exclusao bloqueada quando em uso. | Politica de inativacao nao informada. | Definir exclusao/inativacao para pais em uso. | P1 | PESSOA_E_ORGANIZACAO |
| Moeda | Parcial | Campos `CurrencyId`, `CurrencySymbol`, `CurrencyName`; duplicidade por nome. | Seed inicial comentado; nao ha lista oficial de moedas. | Siser deve fornecer moedas iniciais e politica de manutencao. | P0 | FINANCEIRO; CONFIGURACAO |
| Funcionalidades | Coberto | `FeaturesId`, `Title` e `Description`; titulo e descricao obrigatorios; exibicao publica. | Faltam regras de ordenacao, destaque e publicacao. | Definir governanca de exibicao publica. | P2 | DASHBOARD_E_LAYOUT |
| Cupons | Parcial | Cadastro possui nome, desconto, limite e codigo. | Material indica que nao ha aplicacao comprovada no checkout. | Decidir se cupons serao aplicados em contratacao/cobranca e detalhar regra. | P0 | PEDIDOS_E_COBRANCA_SAAS |
| Configuracao publica | Parcial | Registro unico com nome, titulo, descricao, rodape, email, copyright, decimais, redes sociais e logo. | Criacao inicial, obrigatoriedade e campos de redes sociais nao detalhados. | Fechar dicionario completo da configuracao publica. | P1 | CONFIGURACAO; DASHBOARD_E_LAYOUT |
| Tipo de pagamento | Lacuna | Rota/cadastro global identificado. | Campos e regras da entidade nao foram informados. | Detalhar modelo de dados e uso funcional. | P0 | PEDIDOS_E_COBRANCA_SAAS; FINANCEIRO |
| Modelo de dados funcional | Parcial | EF contem entidades, tabelas, relacionamentos, cardinalidades, constraints, persistencia e diagrama logico. | Algumas chaves, cardinalidades e politicas de exclusao nao estao informadas no material. | Validar lacunas de modelo antes do desenho fisico. | P0 | API_GATEWAY_E_OPENAPI |
| Dicionario de dados implantavel | Parcial | EF inclui campos por entidade com tipo/tamanho/obrigatoriedade quando informados. | Varios campos permanecem como nao informados. | Completar com time de dados/arquitetura. | P0 | API_GATEWAY_E_OPENAPI |
| Catalogo modular e add-ons | Parcial | Material traz add-ons, modulos habilitados, preco, alias, cache, exibicao e dependencias. | Modelo fisico completo e cardinalidade nao estao fechados. | Definir entidade canonica de modulo/add-on. | P0 | ASSINATURA_E_PLANOS; LIMITES_DE_PLANO |
| Modulos ativos por usuario/contexto | Parcial | Regras de owner, subusuario, baseline, plano, avulso, override e diff sem duplicidade. | Modelo final de usuario/contexto e auditoria de override precisam validacao. | Consolidar com usuarios, permissoes e limites. | P0 | USUARIOS_E_PAPEIS; LIMITES_DE_PLANO |
| Baseline de modulos | Parcial | Baseline deve ser mesclado ao conjunto final sem duplicidade. | Estrutura fisica e governanca do baseline nao informadas. | Definir cadastro ou parametro do baseline. | P0 | CONFIGURACAO |
| Dependencias parent/child de add-ons | Parcial | Habilitacao/desabilitacao respeita dependencias. | Regra exata de cascata ou bloqueio nao informada. | Definir matriz de dependencia e comportamento por acao. | P0 | SDK_EXTENSOES; OPERACAO_SUPER_ADMIN |
| Cache de preco/metadados | Parcial | Alteracao de add-on limpa cache de modulo. | Tecnologia e escopo do cache nao informados; efeito funcional foi mapeado. | Definir politica de invalidacao e observabilidade. | P1 | PLATAFORMA_COMPARTILHADA/CONFIGURACAO |
| Upload de pacote add-on | Lacuna | Tela de upload citada. | Formato, validacoes, seguranca e efeito operacional nao detalhados. | Especificar fluxo completo ou mover para extensoes. | P1 | SDK_EXTENSOES; OPERACAO_SUPER_ADMIN |
| Experiencia publica | Parcial | Funcionalidades e pricing podem ser exibidos publicamente. | Ordenacao, publicacao, destaque, idioma e conteudo nao informados. | Definir UX e governanca de publicacao. | P2 | DASHBOARD_E_LAYOUT |
| API externa/publica | Lacuna | Material nao informa API publica especifica para o modulo. | Contratos OpenAPI nao podem ser fechados. | Confirmar se havera API ou apenas consumo interno. | P1 | API_GATEWAY_E_OPENAPI |
| Testes automatizados | Lacuna | Material informa ausencia de testes automatizados identificados. | Risco de regressao nos catalogos e resolucao modular. | Criar suite minima de testes para CRUD, duplicidade e modulos ativos. | P1 | QA |
| Homologacao manual | Parcial | Material sugere replay manual dos fluxos. | Roteiro detalhado e massa de dados nao informados. | Criar roteiro de homologacao. | P1 | Implantacao |

## 4. Itens criticos para validacao humana

1. Confirmar se todos os catalogos deste submodulo permanecem globais e sem tenant.
2. Fornecer lista inicial oficial de moedas ou confirmar que nao havera seed inicial.
3. Decidir se cupons serao aplicados ao checkout/cobranca.
4. Detalhar tipo de pagamento: campos, status, uso e integracao.
5. Definir modelo canonico de add-ons, modulos do plano e modulos ativos por usuario/contexto.
6. Definir comportamento de dependencia parent/child: bloqueio ou cascata.
7. Definir politica de exclusao/inativacao dos catalogos em uso.
8. Validar seguranca do fallback de resolucao modular quando nao houver usuario autenticado.
9. Definir se upload de add-on pertence a este submodulo ou a extensoes/operacao.

## 5. Backlog refinado

| Prioridade | Item | Justificativa |
|---|---|---|
| P0 | Definir aplicacao funcional de cupons no checkout/cobranca. | Evita cadastro sem efeito de negocio. |
| P0 | Detalhar modelo de tipo de pagamento. | Necessario para banco, APIs e integracao financeira. |
| P0 | Fechar modelo de add-ons/modulos ativos. | Necessario para autorizacao correta de recursos. |
| P0 | Definir dependencias parent/child de modulos. | Evita habilitacao inconsistente. |
| P0 | Completar chaves e campos nao informados no dicionario. | Necessario para desenho fisico de banco. |
| P1 | Definir politica de exclusao/inativacao de todos os catalogos. | Evita perda de historico e quebra referencial. |
| P1 | Criar roteiro de homologacao de catalogos globais. | Necessario para validacao humana. |
| P1 | Especificar upload de add-on ou mover formalmente para extensoes. | Reduz risco de seguranca e escopo. |
| P2 | Padronizar mensagens e UX dos CRUDs. | Melhora consistencia operacional. |
| P2 | Definir governanca de exibicao publica de funcionalidades. | Melhora controle comercial. |

## 6. Controle de cobertura funcional

| Bloco funcional | Situacao | Conteudo incorporado | Pendencia de conferencia |
|---|---|---|---|
| Identificacao funcional | Incorporado | ID APP-TEN-007. | Nenhuma. |
| Catalogos globais | Incorporado | Pais, moeda, funcionalidades, cupons, configuracao publica e tipo de pagamento. | Completar campos ausentes. |
| Regras de negocio | Incorporado | 24 regras consolidadas em comportamento funcional do Epros. | Validar lacunas de cupom, tipo pagamento e add-ons. |
| Modelo de dados | Incorporado | Entidades/tabelas, relacionamentos, cardinalidades, constraints, persistencia e diagrama. | Fechar chaves e cardinalidades nao informadas. |
| Fluxos | Incorporado | CRUD global, exclusao de pais, resolucao modular e governanca de add-ons. | Detalhar upload se permanecer neste submodulo. |
| Telas | Incorporado | Listas administrativas, configuracao publica, add-ons e experiencia publica. | UX final e permissoes por acao. |
| Integracoes | Incorporado | Integracoes internas com assinatura, limites, cobranca, configuracao, permissoes e onboarding. | Confirmar API externa/publica. |
| Testes | Incorporado | Cenarios de aceite e estrategia manual. | Criar automacao minima. |

## 7. Notas de rodape

[^agente-001]: Itens de maturidade, auditoria, mensagens padronizadas e backlog refinado foram organizados pelo agente a partir do material disponivel. O que nao estava explicitamente informado foi marcado como lacuna ou decisao.

