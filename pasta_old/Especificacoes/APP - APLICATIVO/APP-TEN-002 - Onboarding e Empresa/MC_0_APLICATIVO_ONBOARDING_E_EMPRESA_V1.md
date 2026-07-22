# MC 0_APLICATIVO ONBOARDING_E_EMPRESA V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** APLICATIVO  
**Submodulo:** ONBOARDING_E_EMPRESA  
**ID funcional:** APP-TEN-002  
**Versao:** V1  
**Status:** Pronto para validacao humana  
**Data:** 2026-06-06

## 1. Objetivo

Esta matriz mede a completude funcional do onboarding de tenant e empresa do Epros, incluindo registro publico, cadastro transacional, primeira empresa, usuario administrador, seeds, configuracoes, idiomas, geografia, armazem, sessao, contexto, limites e fronteiras com os modulos donos.

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
| Cadastro transacional de tenant | Coberto | Fluxo cria tenant, empresa, grupos, plano financeiro, naturezas, CFOP e usuario admin. | Politica final para falha parcial precisa definicao. | Definir reversao ou status pendente controlado. | P0 | Isolamento de Dados |
| Primeira empresa | Coberto | Contrato Empresa preserva razao social, documentos, regime, endereco, parametros fiscais e grupos. | Manutencao completa pertence a outro modulo. | Garantir fronteira com Cadastros Base. | P0 | Cadastros Base |
| Documento unico | Coberto | CNPJ/CPF duplicado bloqueia cadastro. | Escopo da busca cross-tenant precisa confirmacao. | Definir regra de unicidade por base/tenant. | P0 | Identidade/Isolamento |
| Plano escolhido no registro | Decisao | Tela/contrato possui PlanoId, mas seed fixa plano em trecho do material. | Cliente pode receber plano errado. | Usar plano validado ou parametro aprovado. | P0 | Limites de Plano |
| Nome obrigatorio | Lacuna | Estrutura exige Name, mas uma tela traz apenas email/senha/confirmacao. | Cadastro pode falhar ou gravar nome ausente. | Alinhar UI e contrato. | P0 | UX |
| Registro comercial Siser | Parcial | Cadastro chama registro em controle comercial e login consulta limites/bloqueio. | IDs comerciais fixos precisam parametrizacao. | Criar parametros governados. | P0 | Limites de Plano |
| Seed de grupos | Coberto | PessoaGrupo, ProdutoGrupo e TributarioGrupo criados com base na razao social. | Nomes/campos completos dos grupos ficam nos modulos donos. | Referenciar cadastros donos. | P1 | Cadastros Base |
| Plano financeiro inicial | Parcial | Modelo de plano financeiro importado/criado no onboarding. | Modelo, versao e rollback nao detalhados. | Governar modelo oficial e versionamento. | P0 | Financeiro |
| Naturezas financeiras | Parcial | Modelo de configuracao de natureza financeira importado/criado. | Versao e mapeamento final nao detalhados. | Governar modelo oficial. | P0 | Financeiro |
| CFOPs padrao | Coberto | Lista 1102, 1202, 1411, 5101, 5102, 5405, 6101, 6102, 6108, 6404 preservada. | Dono final e atualizacao futura precisam regra. | Vincular a cadastros/fiscal. | P1 | Cadastros/Fiscal |
| Usuario administrador | Coberto | Usuario admin criado e vinculado a empresa com IsAdmin=true. | Politica de senha final pendente. | Usar politica moderna definida em identidade. | P0 | Identidade |
| UsuarioEmpresa | Coberto | EmpresaId e PerfilUsuarioId possuem validacoes; IsAdmin permitido. | Regra admin sem perfil precisa confirmacao final. | Validar com Identidade. | P0 | Identidade |
| Perfil e menu | Parcial | Perfil, acessos, menu em tres niveis, Ver/Editar/Excluir preservados. | Duplicidade entre perfil_usuario_acesso e perfil_acesso. | Consolidar modelo de permissao. | P0 | Permissoes |
| Cache de permissoes | Parcial | Cache de permissoes identificado. | Invalidacao apos alteracao nao detalhada. | Definir invalidacao obrigatoria. | P0 | Permissoes |
| Configuracao de empresa | Parcial | Company/Companies preserva nome, endereco, moeda, fuso, data, logos, VAT e rodape. | Ha estruturas de empresa sobrepostas. | Consolidar empresa operacional x configuracao. | P0 | Configuracao/Cadastros |
| Moeda | Coberto | CurrencyName 250 e CurrencySymbol 50 obrigatorios. | CurrencyId nullable em uma estrutura. | Definir obrigatoriedade final da moeda. | P1 | Catalogos |
| Percentual/tipo de imposto | Parcial | VatPercentage, VatType e CurrencyPosition obrigatorios em uma estrutura. | Semantica fiscal final nao definida para Epros. | Mover regra tributaria ao modulo dono. | P1 | Fiscal/Cadastros |
| Campos sem mapeamento | Lacuna | ShowVatOnPDF, ShowVatOnPurchase, AllowNegativeInventory aparecem sem uso funcional. | Risco de inventar comportamento. | Validar ou descartar formalmente. | P1 | Produto |
| CompanyId fixo | Lacuna | Consumo de moeda com CompanyId fixo identificado. | Risco multiempresa. | Remover fixo e usar empresa ativa. | P0 | Configuracao |
| Geografia | Parcial | Pais, estado, cidade, municipio, regiao, territorio preservados. | Dono final e duplicidade cidade/municipio precisam consolidacao. | Consolidar com Cadastros Base. | P1 | Cadastros Base |
| Armazem | Parcial | Armazem possui nome, descricao, geografia, telefone e endereco; consumido por estoque. | Dono final e obrigatoriedade de endereco pendentes. | Enviar manutencao a Estoque/Cadastros. | P1 | Estoque |
| Transportadora | Parcial | Transportadora com nome e telefone. | Sem consumidor identificado no material. | Validar se fica em Cadastros Base. | P2 | Cadastros Base |
| Idiomas | Parcial | Codigo, nome, countryCode, enabled, idioma base protegido, seletor global. | Persistencia final de dicionario nao definida. | Definir plataforma de traducao/localizacao. | P1 | Plataforma |
| Configuracoes chave-valor | Parcial | key, value, is_public, created_by e cache identificados. | Modelo final, auditoria e invalidacao precisam fechar. | Padronizar settings por owner/tenant. | P0 | Configuracao |
| Consentimento de cookies | Lacuna | Persistencia identificada fora de estrutura transacional. | Baixa rastreabilidade. | Criar entidade auditavel de consentimento. | P1 | Compliance |
| Area publica/signup | Parcial | Home, planos, captcha, newsletter, conteudo e fluxo de aquisicao identificados. | CMS completo e dono final nao definidos. | Definir fronteira com site/configuracao. | P2 | Relatorios/Site |
| Notificacao de boas-vindas | Parcial | Envio ao cliente e alerta interno identificados. | Templates, canais e retry nao detalhados. | Especificar notificacoes. | P1 | Comunicacoes |
| Bloqueio comercial no login | Coberto | block retornado na sessao e direcionamento de regularizacao. | Regra detalhada fica em Limites de Plano. | Manter dependencia formal. | P0 | Limites de Plano |
| Testes automatizados | Lacuna | Testes automatizados nao identificados; cenarios manuais listados. | Alto risco de ambiente parcial. | Criar suite de onboarding. | P0 | QA |

## 4. Itens criticos para validacao humana

1. Confirmar se o plano do cadastro deve vir sempre do plano escolhido ou de parametro Siser quando ausente.
2. Corrigir contrato/tela para que todo campo obrigatorio de registro esteja presente.
3. Definir politica transacional: reverter tudo, pendenciar reparo ou permitir retry por etapa.
4. Parametrizar empresa comercial Siser, revenda, vendedor e plano usados em registro comercial.
5. Consolidar empresa operacional, empresa de configuracao e empresa comercial Siser.
6. Definir modelo oficial e versionado do plano financeiro inicial.
7. Definir modelo oficial e versionado de natureza financeira inicial.
8. Confirmar lista oficial de CFOPs iniciais e seu dono futuro.
9. Definir politica moderna de senha inicial/recuperacao.
10. Consolidar permissao entre perfil_usuario_acesso e perfil_acesso.
11. Decidir persistencia final de idiomas e traducoes.
12. Criar entidade auditavel para consentimento de cookies.
13. Remover qualquer dependencia de CompanyId fixo e usar empresa ativa.
14. Definir dono final de geografia, armazem e transportadora.

## 5. Backlog refinado

| Prioridade | Item | Justificativa |
|---|---|---|
| P0 | Implementar orquestrador transacional de onboarding com etapas e rollback/pendencia controlada. | Evita ambiente parcial. |
| P0 | Alinhar formulario publico e contrato de registro. | Remove campo obrigatorio ausente. |
| P0 | Parametrizar plano, empresa comercial, revenda e vendedor do registro comercial. | Remove identificadores fixos. |
| P0 | Criar suite automatizada de cadastro tenant completo. | Valida seeds essenciais. |
| P0 | Definir politica de senha inicial e recuperacao. | Evita regra insegura. |
| P0 | Consolidar modelo de empresa. | Evita duplicidade de entidades. |
| P0 | Consolidar modelo de permissoes. | Evita duplicidade entre estruturas de acesso. |
| P1 | Versionar modelos de plano financeiro/natureza. | Permite auditoria de seed. |
| P1 | Definir localizacao/idiomas como plataforma ou submodulo proprio. | Evita acoplamento ao onboarding. |
| P1 | Criar consentimento de cookies transacional. | Compliance e auditoria. |
| P1 | Definir dono de geografia/armazem/transportadora. | Evita duplicidade de cadastros. |
| P2 | Refinar area publica/CMS/newsletter. | Fronteira secundaria. |

## 6. Controle de cobertura funcional

| Bloco funcional | Situacao | Conteudo incorporado | Pendencia de conferencia |
|---|---|---|---|
| Identificacao funcional | Incorporado | APP-TEN-002. | Nenhuma. |
| Cadastro tenant | Incorporado | Registro, TenantId, transacao, duplicidade, plano e usuario. | Plano escolhido x plano parametrizado. |
| Empresa inicial | Incorporado | Empresa, endereco, parametros fiscais, grupos e contratos. | Consolidacao com cadastro de empresas. |
| Seeds | Incorporado | Grupos, plano financeiro, natureza e CFOPs. | Modelos oficiais e versionamento. |
| Usuario/admin | Incorporado | Usuario, usuario_empresa, IsAdmin, perfil. | Politica de senha. |
| Permissoes/menu | Parcial | Perfil, acesso, menu tres niveis, Ver/Editar/Excluir. | Modelo duplicado e invalidacao de cache. |
| Configuracoes | Parcial | Company/Companies, settings, moeda, fuso, data, logos e tema. | Modelo final e campos sem mapeamento. |
| Idiomas | Parcial | Catalogo, status, idioma base, seletor, dicionario. | Persistencia final. |
| Geografia/armazem | Parcial | Pais, estado, cidade, municipio, regiao, territorio, armazem e transportadora. | Dono final. |
| Integracoes | Parcial | Sessao, login, acessos, cadastro, municipios, enums, registro comercial. | Contratos versionados e seguranca. |
| Testes | Lacuna | Cenarios manuais identificados. | Suite automatizada. |

## 7. Notas de rodape

[^agente-001]: A recomendacao de orquestrador transacional com etapas, entidade auditavel de consentimento, relatorio de status de onboarding e consolidacao de modelos foi criada pelo agente para fechar lacunas reais do material. Esses pontos permanecem como backlog/decisao ate validacao humana.
