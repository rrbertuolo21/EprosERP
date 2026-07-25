# MC 0_APLICATIVO IDENTIDADE_E_CONTEXTO_TENANT V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** APLICATIVO  
**Submodulo:** IDENTIDADE_E_CONTEXTO_TENANT  
**ID funcional:** APP-TEN-001  
**Versao:** V1  
**Status:** Pronto para validacao humana  
**Data:** 2026-06-06

## 1. Objetivo

Esta matriz mede a completude funcional de identidade, autenticacao, sessao, token, tenant, empresa ativa, auditoria e seguranca do Epros, separando o que esta pronto para construcao inicial do que exige decisao da Siser.

## 2. Legenda de status

| Status | Significado |
|---|---|
| Coberto | Capacidade possui regra, fluxo, tela, entidade ou contrato suficiente para construcao inicial. |
| Parcial | Capacidade existe, mas precisa decisao, complemento ou validacao. |
| Lacuna | Capacidade citada ou esperada sem especificacao suficiente. |
| Decisao | Exige validacao humana antes de construcao. |

## 3. Matriz de completude

| Capacidade | Status | Evidencia funcional consolidada | Lacuna / risco | Acao recomendada | Prioridade | Dependencias |
|---|---|---|---|---|---|---|
| Login web | Coberto | Email/login, senha, validacao, sucesso, falha e auditoria. | Mensagens e identificador final precisam padronizacao. | Definir credencial primaria oficial. | P0 | USUARIOS_E_PAPEIS |
| Contexto tenant | Coberto | TenantId em usuario, token/claim/sessao e query filter. | Nome fisico do mecanismo nao deve virar dependencia de produto. | Padronizar contrato funcional de tenant. | P0 | ISOLAMENTO_DE_DADOS |
| Selecao de empresa | Coberto | Uma empresa segue direto; multiplas exigem selecao; sem empresa bloqueia. | Colunas visuais finais da selecao nao informadas. | Definir UX e colunas da selecao. | P1 | ONBOARDING_E_EMPRESA |
| Token basico e completo | Parcial | Token inicial e token com empresa/acessos identificados. | Conteudo, expiracao e renovacao finais precisam seguranca. | Fechar politica de token. | P0 | API_GATEWAY_E_OPENAPI |
| Sessao web | Parcial | Sessao, cookie, expiracao e limpeza identificados. | Duracoes divergentes: 30 min, 120 min, 10h token. | Definir matriz de expiracao por canal. | P0 | CONFIGURACAO |
| Logout | Coberto | Logout invalida sessao/token e limpa contexto. | Logout em impersonacao precisa regra final. | Detalhar retorno seguro. | P1 | OPERACAO_SUPER_ADMIN |
| 401/sessao expirada | Coberto | Limpa sessao e retorna ao login. | Mensagem final precisa padronizacao. | Definir texto e comportamento global. | P1 | DASHBOARD_E_LAYOUT |
| Politica de senha | Lacuna | Material traz varias politicas e algumas fracas. | Nao ha politica corporativa definitiva. | Definir tamanho, complexidade, historico, expiracao e bloqueio. | P0 | COMPLIANCE_LGPD_SOX_IFRS |
| Hash de senha | Decisao | Materiais citam algoritmos diferentes. | Nao deve copiar algoritmo fraco como padrao. | Escolher algoritmo moderno e plano de migracao de hashes. | P0 | Segurança |
| Recuperacao de senha | Parcial | Email, token, reset e limpeza identificados. | Janela de validade conflita e enumeracao de email precisa decisao. | Definir janela unica e mensagem segura. | P0 | Email/Notificacoes |
| Troca de senha autenticada | Coberto | Senha atual, nova senha, confirmacao e bloqueio de senha igual. | Permissao especifica precisa validacao. | Definir se todo usuario pode trocar propria senha. | P1 | USUARIOS_E_PAPEIS |
| Verificacao de email | Parcial | Verificacao e reenvio com limite identificados. | Regra de obrigatoriedade por tenant/produto nao fechada. | Definir quando email verificado e exigido. | P1 | ONBOARDING_E_EMPRESA |
| Registro self-service | Parcial | Cadastro tenant com empresa, admin e seeds. | Registro habilitado, termos, captcha e email precisam governanca. | Definir politica de cadastro publico. | P0 | ONBOARDING_E_EMPRESA; ASSINATURA_E_PLANOS |
| Duplicidade CNPJ/CPF/email | Coberto | Bloqueios identificados. | Escopo cross-tenant/soft delete precisa confirmacao. | Definir escopo de unicidade. | P0 | CADASTROS_BASE |
| Auditoria de login | Parcial | Historico, IP, data, detalhes, owner/contexto. | Retencao e privacidade de geolocalizacao nao definidas. | Definir politica de retencao e LGPD. | P0 | COMPLIANCE_LGPD_SOX_IFRS |
| Tentativas e rate limit | Parcial | Limites 5, 50/30min e parametros foram identificados. | Numeros divergem. | Definir politica central de rate limit. | P0 | Segurança |
| Banimento de IP | Parcial | Falhas podem gerar banimento temporario. | Banimento de login versus site inteiro precisa politica. | Definir escopo, prazo e desbloqueio. | P0 | Segurança |
| Login API/mobile | Parcial | Token API, refresh, logout, modulo ativo e canal mobile identificados. | Alguns canais tinham credencial insegura; contrato final pendente. | Criar contrato unico de canal. | P0 | API_GATEWAY_E_OPENAPI |
| Login PDV/local | Lacuna | Login por email/CPF e PIN supervisor identificado. | Sem politica segura definitiva para canal local. | Encaminhar para PDV com padrao de autenticacao forte. | P0 | VENDAS/PONTO_DE_VENDA_PDV |
| Provedor externo/SSO | Decisao | Provedores externos, diretorio e 2FA citados. | Nao ha politica de ativacao, auto-provisionamento e mapeamento. | Definir roadmap IAM. | P1 | INTEGRACOES_E_CONECTORES |
| Login social | Decisao | Capacidade social identificada. | Credenciais, tokens e mapeamento local nao fechados. | Decidir se o Epros tera social login. | P2 | COMPLIANCE_LGPD_SOX_IFRS |
| Impersonacao | Decisao | Capacidade identificada com retorno ao usuario original. | Alto risco; precisa autorizacao, motivo, trilha e limites. | Definir politica ou remover. | P0 | OPERACAO_SUPER_ADMIN |
| Permissoes ver/editar/excluir | Coberto | Perfil carrega arvore de menu e flags. | Cache e invalidacao precisam detalhe. | Fechar com Permissoes de Menu. | P0 | PERMISSOES_DE_MENU |
| Menu sem RBAC na leitura | Lacuna | Leitura autenticada aberta foi identificada. | Pode expor catalogo de menu indevidamente. | Definir se retorna catalogo controlado ou somente autorizado. | P1 | PERMISSOES_DE_MENU |
| Cache de permissoes | Parcial | Cache de 30 min identificado. | Invalidacao ao alterar perfil nao detalhada. | Definir invalidacao imediata ou TTL. | P0 | PERMISSOES_DE_MENU |
| Modelo de dados | Parcial | EF contem entidades, relacoes, constraints e diagrama. | Tamanhos divergentes e campos nao informados. | Validar modelo com arquitetura de dados. | P0 | Dados/Arquitetura |
| Dicionario de dados | Parcial | Campos e tamanhos preservados quando informados. | Obrigatoriedade completa nao esta em todos os campos. | Completar campos fisicos finais. | P0 | Dados/Arquitetura |
| Testes automatizados | Parcial | Cenarios derivados existem. | Automacao nao comprovada para todos os fluxos. | Criar suite minima de seguranca. | P0 | QA |

## 4. Itens criticos para validacao humana

1. Definir politica corporativa de senha e algoritmo de hash do Epros.
2. Definir expiracao de token basico, token completo, sessao web, API e mobile.
3. Definir janela unica de reset de senha e comportamento de token consumido.
4. Definir se email nao encontrado deve ser informado ou mascarado por seguranca.
5. Definir escopo de unicidade de email, login, CNPJ e CPF.
6. Definir politica de rate limit, lockout e banimento de IP.
7. Definir retencao de historico de login, falhas, sucessos, sessoes e tokens.
8. Definir se impersonacao sera permitida e sob quais controles.
9. Definir se login social, SSO, diretorio e 2FA entram na V1.
10. Corrigir campo de login em claims/contratos para transportar login real, nao identificador indevido.
11. Definir se consulta de menu retorna catalogo completo autenticado ou apenas itens autorizados.
12. Definir contrato seguro para PDV/mobile, sem credencial em claro ou login fraco.

## 5. Backlog refinado

| Prioridade | Item | Justificativa |
|---|---|---|
| P0 | Aprovar politica de senha e hash moderno. | Sem isso identidade nasce insegura. |
| P0 | Unificar expiracao de sessao/token por canal. | Evita comportamento contraditorio. |
| P0 | Fechar reset de senha seguro. | Tokens conflitantes geram risco operacional. |
| P0 | Corrigir claim/campo de login. | Evita contexto incorreto e auditoria ruim. |
| P0 | Criar contrato unico de token e contexto. | Base para todos os modulos. |
| P0 | Criar suite de testes de login, reset, empresa, permissao e sessao expirada. | Reduz risco em area critica. |
| P0 | Definir politica de impersonacao. | Capacidade de alto risco. |
| P0 | Definir contrato seguro para mobile/PDV. | Elimina login fraco. |
| P1 | Definir retencao de auditoria. | Necessario para compliance. |
| P1 | Definir invalidacao de cache de permissao. | Evita permissao antiga apos mudanca. |
| P1 | Padronizar mensagens. | Melhora UX sem reduzir seguranca. |
| P2 | Decidir login social. | Capacidade nao essencial para ERP core. |

## 6. Controle de cobertura funcional

| Bloco funcional | Situacao | Conteudo incorporado | Pendencia de conferencia |
|---|---|---|---|
| Identificacao funcional | Incorporado | ID APP-TEN-001. | Nenhuma. |
| Login e sessao | Incorporado | Login, logout, sessao, token e 401. | Expiracoes e politicas. |
| Tenant e empresa | Incorporado | TenantId, empresa ativa, obter acessos. | UX e escopo de unicidade. |
| Senha | Incorporado como parcial | Login, reset, troca, confirmacao e tokens. | Politica de senha/hash. |
| Auditoria | Incorporado | Historico, tentativas, sucessos, IPs, tokens. | Retencao e privacidade. |
| RBAC | Incorporado | Perfil, acesso, menu e flags. | Cache e leitura de menu. |
| Cadastro tenant | Incorporado | Dados de empresa, usuario admin e seeds. | Governanca de registro publico. |
| API/mobile/PDV | Incorporado como parcial | Token API, refresh, logout e canais. | Contrato seguro final. |
| Provedores externos | Incorporado como decisao | SSO/social/diretorio/2FA. | Roadmap IAM. |
| Modelo de dados | Incorporado | Entidades, relacoes, constraints e diagrama. | Tamanhos e obrigatoriedades finais. |
| Dicionario de dados | Incorporado | Campos de usuario, empresa, sessao, tokens, auditoria, menu e contratos. | Completar campos nao informados. |
| Testes | Incorporado | Cenarios derivados. | Automacao. |

## 7. Notas de rodape

[^agente-001]: Itens de maturidade, seguranca, auditoria, retencao, backlog refinado e controles de impersonacao/provedores externos foram organizados pelo agente a partir do material disponivel. O que nao estava explicitamente informado foi marcado como lacuna, decisao ou `Nao informado no material`.

