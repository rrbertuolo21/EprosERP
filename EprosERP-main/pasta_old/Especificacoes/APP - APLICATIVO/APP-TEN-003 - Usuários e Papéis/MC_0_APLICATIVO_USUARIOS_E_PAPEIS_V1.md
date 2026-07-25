# MC 0 Aplicativo — Usuarios e Papeis V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Aplicativo |
| Submodulo | Usuarios e Papeis |
| Versao | V1 |
| Data | 2026-06-06 |

## 2. Matriz de completude

| Area | Status | Evidencia funcional consolidada | Lacuna | Acao recomendada | Prioridade | Dono sugerido |
|---|---|---|---|---|---|---|
| Cadastro de usuario | Parcial | Login, senha, e-mail, ativo, tenant e vinculos. | Escopo final de unicidade de login/e-mail nao esta fechado. | Definir unicidade global, por tenant ou por empresa. | P0 | Plataforma |
| Tamanho de e-mail | Conflito | Modelo principal informa 120; complementar informa 150. | Tamanho final nao definido. | Padronizar em todo Epros. | P0 | Plataforma |
| Usuario multiempresa | Parcial | UsuarioEmpresa com EmpresaId, PerfilUsuarioId e IsAdmin. | Regras de reativacao/remocao de vinculo nao detalhadas. | Definir ciclo de vida do vinculo. | P0 | Plataforma |
| Admin da empresa | Parcial | IsAdmin dispensa PerfilUsuarioId. | Auditoria e limite do bypass nao detalhados. | Definir trilha e escopo do admin. | P0 | Seguranca |
| Papel direto | Parcial | Material traz roles/papeis e pivots. | Relacao final entre papel, perfil e menu nao fechada. | Decidir se papel direto coexistira com perfil. | P0 | Arquitetura |
| Permissao direta do usuario | Parcial | Material traz grant/deny por usuario. | Precedencia sobre perfil/papel nao definida no Epros. | Definir regra de composicao. | P1 | Arquitetura |
| Senha | Incompleto | Senha armazenada e troca identificadas. | Hash, complexidade, expiracao, historico e reset seguro nao definidos. | Criar politica de senha. | P0 | Seguranca |
| Recuperacao de senha | Parcial | Nova senha por e-mail identificada. | Token, validade, uso unico e auditoria nao detalhados. | Fechar fluxo seguro. | P0 | Seguranca |
| Historico de login | Parcial | user_id, ip, date, details e created_by identificados. | Retencao, falhas e filtros finais nao definidos. | Definir tabela e politica de retencao. | P1 | Seguranca |
| Impersonacao | Parcial | Fluxo de impersonacao e retorno identificado. | Tabela, motivo obrigatorio, limites e auditoria final nao definidos. | Criar governanca de impersonacao. | P0 | Seguranca |
| Preferencias | Parcial | Avatar, notificacoes, idioma, tema e preferencias identificados. | Modelo final de preferencias nao definido. | Definir estrutura normalizada ou JSON governado. | P2 | Produto |
| Usuario grupo/portal/cliente | Parcial | Material informa grupo, portal, contato e cliente. | Epros precisa decidir quais tipos serao suportados no core. | Separar usuario humano, contato e grupo. | P1 | Produto |
| Ultimo administrador | Incompleto | Material aponta protecao contra rebaixar ultimo admin. | Regra final no Epros nao informada. | Implementar trava de ultimo admin. | P0 | Seguranca |
| Hierarquia de gestor | Parcial | reports_to e prevencao de ciclo identificados. | Uso no Epros nao definido. | Definir se hierarquia sera usada em RH/CRM. | P2 | Produto/RH |
| Nivel de usuario e quotas | Parcial | Tabelas completas de nivel e preco identificadas. | Pertencimento a Usuarios ou Limites de Plano nao definido. | Mover regra comercial para Limites quando aplicavel. | P1 | Produto |
| Chave de API | Parcial | ApiKey identificada. | Escopo, rotacao, revogacao e permissao admin/usuario nao definidos. | Especificar governanca de API key. | P0 | Seguranca |
| Testes automatizados | Incompleto | Cenarios manuais identificados. | Suite automatizada nao informada. | Criar testes dos CTs da EF. | P0 | QA |

## 3. Pendencias criticas P0

1. Definir se login/e-mail sao unicos globalmente, por tenant ou por empresa.
2. Padronizar tamanho de e-mail em todos os contratos.
3. Definir politica de senha, reset, troca, historico e complexidade.
4. Criar auditoria obrigatoria para admin e impersonacao.
5. Definir composicao entre perfil, papel, permissao direta e matriz de menu.
6. Impedir remocao/rebaixamento do ultimo administrador valido.
7. Definir regra de invalidacao de sessao/cache apos alteracao de usuario, papel ou vinculo.
8. Definir governanca de chave de API.

## 4. Perguntas para validacao humana

| Pergunta | Impacto |
|---|---|
| O Epros tera papel direto alem de perfil de usuario? | Define tabelas `usuario_papel` e composicao de permissoes. |
| A negacao direta no usuario deve prevalecer sobre papel/perfil? | Define regra de autorizacao efetiva. |
| Usuario cliente/contato acessara o mesmo Epros operacional? | Define tipos de usuario e fronteira com CRM/portal. |
| Preferencias serao armazenadas em tabela normalizada ou JSON governado? | Define modelo de dados e privacidade. |
| Impersonacao exigira motivo obrigatorio? | Define auditoria e conformidade. |
| Chave de API sera permitida para usuario comum? | Define risco e escopo de integracao. |
| Nivel de usuario com quotas sera mantido aqui ou absorvido por Limites de Plano? | Define fronteira de implantacao. |

## 5. Criterios de aceite

| ID | Criterio |
|---|---|
| CA-001 | Usuario sem empresa nao pode ser criado como usuario operacional. |
| CA-002 | E-mail duplicado e bloqueado conforme escopo definido. |
| CA-003 | Usuario comum sem perfil nao acessa empresa. |
| CA-004 | Usuario administrador da empresa pode operar sem PerfilUsuarioId. |
| CA-005 | Usuario nao pode ter duas linhas para a mesma empresa. |
| CA-006 | Alteracao de usuario nao altera senha fora do fluxo de senha. |
| CA-007 | Nova senha igual a atual e bloqueada. |
| CA-008 | Usuario inativo, suspenso, bloqueado ou excluido nao autentica. |
| CA-009 | Impersonacao sem permissao e bloqueada. |
| CA-010 | Impersonacao registra origem, alvo, empresa, inicio e fim. |
| CA-011 | Historico de login respeita escopo do usuario consultante. |
| CA-012 | Ultimo administrador nao pode ser removido sem substituto. |

## 6. Itens de construcao

| Item | Entrega esperada | Prioridade |
|---|---|---|
| Modelo final de usuario | Campos, tamanhos, status, unicidade e exclusao logica. | P0 |
| Modelo de vinculo empresa | Constraint usuario+empresa e regras de admin/perfil. | P0 |
| Modelo de papeis | Decisao perfil x papel x capacidade. | P0 |
| Politica de senha | Hash, reset, expiracao, historico e complexidade. | P0 |
| Auditoria | Login, falha, admin, alteracao critica e impersonacao. | P0 |
| Governanca de API key | Criar, rotacionar, revogar, escopar e auditar. | P0 |
| Preferencias | Estrutura persistente e protecao de privacidade. | P2 |
| Suite QA | Testes automatizados CT-001 a CT-015. | P0 |
