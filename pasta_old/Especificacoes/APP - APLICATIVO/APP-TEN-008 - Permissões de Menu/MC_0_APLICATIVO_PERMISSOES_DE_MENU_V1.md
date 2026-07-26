# MC 0 Aplicativo — Permissoes de Menu V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Aplicativo |
| Submodulo | Permissoes de Menu |
| Versao | V1 |
| Data | 2026-06-06 |

## 2. Matriz de completude

| Area | Status | Evidencia funcional consolidada | Lacuna | Acao recomendada | Prioridade | Dono sugerido |
|---|---|---|---|---|---|---|
| Catalogo de menu 3 niveis | Parcial | `menu`, `menu_item_nivel1`, `menu_item_nivel2`, descricao, icone, rota e ordem. | Tenant/global do catalogo nao esta definido; obrigatoriedade de campos nao esta completa. | Definir governanca do catalogo e constraints finais. | P0 | Plataforma |
| Perfil de usuario | Parcial | `perfil_usuario` com TenantId e Descricao. | Divergencia entre limite de descricao 20 e persistencia 100. | Definir tamanho final de descricao e mensagem correta. | P0 | Plataforma |
| Matriz Ver/Editar/Excluir | Parcial | `perfil_usuario_acesso` com MenuId, MenuItemNivel1Id, MenuItemNivel2Id, Ver, Editar e Excluir. | Obrigatoriedade final de MenuItemNivel1Id/MenuItemNivel2Id e constraint unica nao informadas. | Formalizar regras por nivel e constraint de unicidade. | P0 | Plataforma |
| Usuario multiempresa | Parcial | Usuario com login, senha, e-mail, ativo; vinculo empresa, perfil e IsAdmin. | Tamanho final de e-mail diverge entre 120 e 150; regra de usuario sem perfil quando admin precisa teste. | Unificar contrato de usuario. | P0 | Plataforma |
| Autorizacao transversal | Parcial | PodeLer, PodeIncluirAlterar e PodeDeletar por menu e flags, com admin bypass. | Inventario completo dos identificadores de todos os controllers/rotas de negocio fica nos modulos donos. | Exigir matriz de identificadores em cada EF funcional. | P0 | Todos os modulos |
| Cache de permissoes | Parcial | Cache absoluto de 30 minutos. | Chave por usuario/empresa/tenant e invalidação apos alteracao de perfil nao detalhadas. | Definir chave e politica de invalidacao. | P0 | Plataforma |
| Menu visivel x API protegida | Parcial | Menu e autorizacao usam mesmos identificadores. | Leitura de catalogo de menu sem regra clara de permissao final. | Decidir exposicao do catalogo e logs de acesso. | P1 | Seguranca/Plataforma |
| Capacidade granular | Parcial | Material informa acoes, niveis todos/proprio e papel-capacidade. | Modelo final complementar ainda nao esta fechado. | Decidir se o Epros tera camada de capacidade alem de menu. | P1 | Arquitetura |
| Escopo por propriedade | Parcial | Material informa manage-any/manage-own e propriedade do registro. | Falta padrao Epros para owner do registro em cada dominio. | Criar regra transversal de ownership. | P1 | Arquitetura |
| Recuperacao de senha | Parcial | Endpoint de nova senha por e-mail identificado. | Validade, token, auditoria e politica de senha nao detalhados. | Definir fluxo seguro completo. | P0 | Seguranca |
| Algoritmo e politica de senha | Incompleto | Senha armazenada e comparada. | Algoritmo moderno, requisitos de complexidade, historico e expiracao nao definidos. | Definir politica de senha Epros. | P0 | Seguranca |
| Auditoria de bypass admin | Incompleto | Admin possui bypass funcional. | Auditoria de uso do bypass nao informada. | Registrar eventos de acesso administrativo. | P0 | Seguranca |
| Exclusao logica | Parcial | Usuario, perfil e acessos sao removidos logicamente. | Retencao, reativacao e impacto em historico nao detalhados. | Definir politica de ciclo de vida. | P1 | Plataforma |
| Bloqueio SaaS no login | Parcial | Block direciona para faturas vencidas. | Regras detalhadas pertencem a cobranca SaaS; este submodulo so consome status. | Garantir contrato entre cobranca e login. | P0 | Aplicativo/Cobranca |
| Menu especifico MEI | Parcial | Menu id funcional 16 oculto quando regime nao e MEI. | Catalogo final do menu id 16 e regra por outros regimes nao detalhados. | Fechar matriz fiscal de menus condicionais. | P1 | Fiscal/Plataforma |
| Mensagens | Parcial | Mensagens principais foram preservadas. | Padrao internacional de codigos, idiomas e internacionalizacao nao informado. | Definir catalogo de mensagens. | P2 | Plataforma |
| Testes automatizados | Incompleto | Cenarios manuais identificados. | Suite automatizada nao informada. | Criar testes de login, perfil, usuario, cache e autorizacao. | P0 | QA |

## 3. Pendencias criticas P0

1. Definir constraint unica de `perfil_usuario_acesso` para impedir duplicidade por perfil e item de menu.
2. Definir tamanho final de `perfil_usuario.Descricao`: 20 ou 100 caracteres.
3. Definir tamanho final de `usuario.Email`: 120 ou 150 caracteres.
4. Definir invalidacao de cache quando perfil, acesso, usuario ou vinculo empresa forem alterados.
5. Definir politica segura de senha e recuperacao de senha.
6. Auditar bypass administrativo por usuario, empresa, acao, menu, data/hora e resultado.
7. Exigir que cada modulo operacional entregue sua matriz de identificadores de menu para leitura, edicao e exclusao.
8. Fechar contrato do bloqueio SaaS entre login, cobranca e area de faturas vencidas.

## 4. Perguntas para validacao humana

| Pergunta | Impacto |
|---|---|
| O catalogo de menu e global da Siser ou pode variar por tenant/plano? | Define TenantId em menu e seed. |
| Perfil de usuario e unico por tenant ou por empresa? | Define unicidade, telas e reaproveitamento entre empresas. |
| Usuario admin da empresa deve enxergar todos os menus contratados ou todos os menus do Epros? | Define intersecao com limites de plano. |
| O Epros adotara camada de capacidades granulares alem de Ver/Editar/Excluir? | Define modelo `capacidade` e matriz adicional. |
| A permissao Ver deve ser obrigatoria para permitir Editar/Excluir? | Define consistencia da matriz. |
| Alterar perfil invalida sessoes ativas imediatamente? | Define cache e seguranca operacional. |
| O catalogo de menu pode ser consultado por qualquer autenticado? | Define seguranca da rota de menus. |

## 5. Criterios de aceite

| ID | Criterio |
|---|---|
| CA-001 | Usuario comum sem perfil na empresa nao acessa telas protegidas. |
| CA-002 | Usuario administrador da empresa acessa telas da empresa sem PerfilUsuarioId. |
| CA-003 | Perfil com Ver=false nao permite listar ou consultar dados da rota protegida. |
| CA-004 | Perfil com Editar=false nao permite criar ou alterar dados da rota protegida. |
| CA-005 | Perfil com Excluir=false nao permite excluir dados da rota protegida. |
| CA-006 | Alteracao de perfil reflete no menu e nas APIs apos politica de cache definida. |
| CA-007 | Menu exibido ao usuario contem apenas itens permitidos e aplicaveis ao regime/plano. |
| CA-008 | Usuario nao consegue selecionar empresa fora de seu vinculo. |
| CA-009 | E-mail duplicado e bloqueado conforme regra final de unicidade. |
| CA-010 | Usuario nao pode ter dois perfis na mesma empresa. |
| CA-011 | Bloqueio SaaS redireciona para faturas vencidas. |
| CA-012 | Erro 401 limpa sessao e direciona para login. |

## 6. Itens de construcao

| Item | Entrega esperada | Prioridade |
|---|---|---|
| Modelo final de menu | Tabelas, constraints, seed e governanca global/tenant. | P0 |
| Modelo final de perfil | Descricao, unicidade, exclusao logica e auditoria. | P0 |
| Matriz de acesso | Constraints, validacoes por nivel e sincronizacao. | P0 |
| Cache seguro | Chave, expiracao, invalidacao e testes. | P0 |
| Auditoria | Log de acesso permitido/negado e bypass admin. | P0 |
| Politica de senha | Hash, complexidade, troca, recuperacao e historico. | P0 |
| Catalogo de mensagens | Mensagens normalizadas e prontas para internacionalizacao. | P2 |
| Suite QA | Testes automatizados dos cenarios CT-001 a CT-016. | P0 |
