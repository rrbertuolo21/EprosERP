# Configuração de campos Jira — projeto EP

Runbook para o **administrador Jira**. Execute na ordem abaixo.

## 1. Criar campo customizado (cascading select)

Em **Configurações → Issues → Campos customizados → Criar campo**:

| Campo | Tipo | Contexto | Obrigatório |
|---|---|---|---|
| **Domínio** | **Seleção em cascata** (Cascading Select) | Projeto EP, todos os tipos | Sim |

- **Nível 1 (pai):** macromódulo — ex.: `FIN — Financeiro`
- **Nível 2 (filho):** submódulo — ex.: `FIN-CP-001 — Contas a Pagar`

A validação Módulo ↔ Submódulo fica **nativa** — combinações inválidas não aparecem na UI.

### Importar opções

Usar [opcoes-campo-cascata.csv](opcoes-campo-cascata.csv) como referência (`parent,child`):

- 132 pares módulo → submódulo
- 1 par transversal: `TRV — Trabalho transversal,TRV — Trabalho transversal`

Regenerar após alteração no mapa de produto:

```bash
python docs/dev-framework/jira/generate-taxonomy.py
```

> **Nota:** o Jira Cloud não importa cascading via CSV nativamente em todos os planos. Se não houver import, cadastrar manualmente a hierarquia usando o CSV como checklist — ou usar app de marketplace (ex.: CSV Custom Field Import) uma única vez.

> Após criar o campo, anote o ID (`customfield_XXXXX`) — usado em JQL, automação e MCP.

### Por que cascading em vez de dois campos?

| Cascading | Dois selects separados |
|---|---|
| Validação nativa pai/filho | Precisa automation/ScriptRunner |
| Um campo obrigatório | Dois campos obrigatórios |
| JQL: `"Domínio" = Pai` ou `Pai > Filho` | JQL em dois campos |

## 2. Telas (screens)

Adicionar **Domínio** em:

- Criar issue (Epic, Novos Módulos, Melhoria/Ajuste, Bug, Hotfix)
- Editar issue
- Transição para *Em andamento* (DoR gate)

Posição sugerida: após **Tipo de item**, antes de **Team**.

### Layout do backlog

Em **Board settings → Card layout**, exibir:

- Domínio (mostra pai e filho)
- Team
- Epic Link (ou parent)

## 3. Validador Módulo ↔ Submódulo

**Não necessário** com cascading select — o Jira só permite filhos válidos do pai selecionado.

Manter validação manual apenas se o time usar **dois campos legados** em paralelo durante transição.

## 4. Consolidar Componentes (técnicos)

Manter **somente** componentes transversais. Remover da tela de criação (não deletar do histórico) os componentes funcionais de menu.

### Componentes a manter

| Componente | Uso |
|---|---|
| Backend - API Transversal | Endpoints cross-cutting |
| Backend - Banco de Dados | Migrations, índices, performance |
| Backend - DFe | Motor fiscal Hercules |
| Frontend - Componentes Compartilhados | Design system / UI lib |
| Frontend - Infraestrutura Nuxt | Nuxt, build, SSR |
| Engenharia - CI/CD | Pipelines, deploy |
| Engenharia - Testes e Qualidade | Testcontainers, cobertura |
| Engenharia - Refatoração | Dívida técnica estrutural |
| Engenharia - Dependências e Tooling | NuGet, npm, upgrades |
| Engenharia - Documentação Técnica | OpenAPI, runbooks |

### Componentes funcionais (legado)

Não usar em issues novas. De/para em [de-para-componentes-jira.md](de-para-componentes-jira.md).

## 5. Quick Filters do board EP

| Nome | JQL |
|---|---|
| FIN | `"Domínio" = "FIN — Financeiro"` |
| VEN | `"Domínio" = "VEN — Vendas"` |
| EST | `"Domínio" = "EST — Estoque"` |
| PLT | `"Domínio" = "PLT — Plataforma Compartilhada"` |
| Transversal | `"Domínio" = "TRV — Trabalho transversal"` |
| FIN-CP | `"Domínio" = "FIN — Financeiro > FIN-CP-001 — Contas a Pagar"` |

Substituir `"Domínio"` pelo nome exato do campo se diferente.

## 6. Automação — copiar Domínio do Epic

```
Trigger: Issue created
Condition: parent is Epic AND parent has Domínio set
Action: Copy field value from parent to child
```

Filhos cross-domain podem sobrescrever manualmente após criação.

## 7. Checklist pós-configuração

- [ ] Campo **Domínio** visível na criação de Epic e tasks
- [ ] Hierarquia 17 módulos + 132 submódulos + TRV cadastrada
- [ ] Card layout exibe Domínio (pai e filho)
- [ ] Quick Filters FIN/VEN/EST funcionam
- [ ] JQL `"Domínio" = "FIN — Financeiro > FIN-CP-001 — Contas a Pagar"` retorna issues corretas
- [ ] ID do custom field documentado abaixo

### ID do campo (preencher após criação)

| Campo | customfield_id |
|---|---|
| Domínio (cascading) | `customfield_10343` |

Atualizar [publish-jira-ep.md](../../.cursor/skills/S20-planning-breakdown/checklists/publish-jira-ep.md) quando o ID for conhecido.
