# MC 1 Cadastros Base — Parametros Operacionais V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Cadastros Base |
| Submodulo | Parametros Operacionais |
| Versao | V1 |
| Data | 2026-06-06 |

## 2. Matriz de completude

| Area | Status | Evidencia funcional consolidada | Lacuna | Acao recomendada | Prioridade | Dono sugerido |
|---|---|---|---|---|---|---|
| Organizacao | Parcial | Company com tenant, logo, fuso, data e moeda. | Campos cadastrais completos da empresa ficam em Pessoa/Organizacao. | Fechar contrato com empresa emitente. | P0 | Cadastros |
| Criacao inicial transacional | Parcial | Organizacao, plano ativo inicial e exercicio financeiro inicial. | PlanId fixo inicial precisa governanca com planos comerciais. | Integrar com assinatura/limites. | P0 | Aplicativo/Cadastros |
| Tenant em listagens | Incompleto | Material identifica listagens sem filtro. | Todas as listagens precisam filtro tenant. | Implementar filtro global/obrigatorio. | P0 | Plataforma |
| Categorias | Parcial | Nome unico e bloqueio quando em produto. | Tamanho de nome e status ativo nao informados. | Completar dicionario. | P1 | Cadastros/Estoque |
| Unidades | Parcial | Nome unico e bloqueio quando em produto. | Codigo internacional e conversoes ausentes. | Criar codigo padrao e conversao. | P1 | Cadastros/Estoque |
| Armazens | Parcial | Nome, pais, cidade, contato e e-mail. | Vinculos reais de uso operacional nao detalhados. | Definir bloqueio por estoque/movimento. | P0 | Estoque |
| Projetos | Parcial | Nome unico e bloqueio por lancamento contabil. | Status, responsavel e vigencia nao informados. | Integrar com Projetos/Financeiro. | P1 | Projetos/Financeiro |
| Preferencias gerais | Parcial | Flags principais mapeadas. | Tipo final de StockCalculationMode aparece ambiguo. | Definir enum final. | P0 | Estoque/Cadastros |
| Auditoria de parametros | Incompleto | Gap documentado. | Log de alteracao nao implementado no material. | Implementar auditoria imutavel. | P0 | Plataforma/Seguranca |
| E-mail | Parcial | Registro unico com campos SMTP basicos. | Teste de conexao, autenticacao moderna, segredo e rotacao nao detalhados. | Criar governanca de e-mail. | P1 | Plataforma |
| Impostos | Parcial | UI administra nome, aliquota e ativo. | Vigencia, jurisdicao, calculo e uso transacional fora daqui. | Integrar com fiscal/tributario. | P0 | Fiscal |
| Fusos horarios | Parcial | Lista global e vinculo em organizacao. | Codigo IANA nao informado. | Adicionar identificador IANA. | P1 | Plataforma |
| Multi-moeda | Parcial | CurrencyId default existe. | Moeda ISO, multi-moeda e moeda funcional nao implementadas. | Criar catalogo de moeda. | P1 | Financeiro/Cadastros |
| Matriz/filiais | Incompleto | Gap documentado. | EmpresaPaiId, heranca e override ausentes. | Definir hierarquia de organizacao. | P1 | Cadastros |
| Efetividade temporal | Incompleto | Gap documentado. | Vigencia de preferencias e impostos ausente. | Criar vigencia. | P2 | Plataforma/Fiscal |
| Permissionamento fino | Incompleto | Gap documentado. | Permissao por painel nao detalhada. | Mapear menus/acessos por painel. | P1 | Plataforma |
| Testes automatizados | Parcial | Cenarios identificados. | Suite automatizada nao comprovada. | Criar testes CT-001 a CT-012. | P0 | QA |

## 3. Pendencias criticas P0

1. Garantir filtro por TenantId em todas as listagens e consultas tenantizadas.
2. Corrigir exclusao de armazem para verificar vinculos do proprio armazem.
3. Implementar auditoria de alteracao de parametros, principalmente flags financeiras e de estoque.
4. Definir enum final de modo de calculo de estoque.
5. Integrar PlanId inicial com governanca real de assinatura e limites.
6. Definir bloqueios corretos para exclusao de armazem em uso.
7. Separar definitivamente interface de imposto de calculo tributario.

## 4. Perguntas para validacao humana

| Pergunta | Impacto |
|---|---|
| O tenant pode ter mais de uma organizacao/filial no MVP? | Define `company` 1:1 ou 1:N com tenant. |
| PlanId inicial 1 continua valido ou deve vir do plano contratado? | Define integracao com assinatura. |
| Alteracao de flags financeiras exige aprovacao ou apenas justificativa? | Define governanca e workflow. |
| Armazem pode ser excluido se nao houver saldo, mas houver historico? | Define regra de integridade de estoque. |
| Imposto administrativo precisa vigencia por data? | Define modelo fiscal. |
| Configuracao de e-mail sera por tenant, filial ou usuario? | Define escopo da entidade. |
| Moeda padrao pode variar por filial? | Define multi-moeda e estrutura organizacional. |

## 5. Criterios de aceite

| ID | Criterio |
|---|---|
| CA-001 | Criacao inicial grava organizacao, plano e exercicio em transacao unica. |
| CA-002 | Falha parcial desfaz toda a criacao inicial. |
| CA-003 | Listagens nao retornam dados de outro tenant. |
| CA-004 | Categoria duplicada no tenant e bloqueada. |
| CA-005 | Unidade duplicada no tenant e bloqueada. |
| CA-006 | Armazem duplicado no tenant e bloqueado. |
| CA-007 | Projeto duplicado no tenant e bloqueado com mensagem correta. |
| CA-008 | Categoria/unidade/projeto/armazem em uso nao sao excluidos. |
| CA-009 | Fuso em uso por organizacao nao e excluido. |
| CA-010 | Imposto em uso transacional nao e excluido. |
| CA-011 | Logo e redimensionado para 300x300. |
| CA-012 | Alteracao de flags criticas gera auditoria. |

## 6. Itens de construcao

| Item | Entrega esperada | Prioridade |
|---|---|---|
| Modelo final de parametros | Company, preferencias, email, auxiliares, fuso e impostos com constraints. | P0 |
| Filtro tenant | Garantia de isolamento em todas as consultas. | P0 |
| Auditoria de parametros | Log imutavel com antes/depois, usuario e justificativa. | P0 |
| Exclusao protegida | Regras por entidade e vinculo correto. | P0 |
| Testes de regressao | Cenarios de tenant, exclusao e transacao. | P0 |
| E-mail seguro | Segredo, teste de conexao e autenticacao moderna. | P1 |
| Multi-moeda | Catalogo ISO, moeda funcional e casas decimais. | P1 |
| Filiais/heranca | EmpresaPaiId, heranca e override. | P1 |
| Unidades padronizadas | Codigo padrao e conversoes. | P1 |
| Vigencia de parametros | Inicio/fim para preferencias e impostos. | P2 |
