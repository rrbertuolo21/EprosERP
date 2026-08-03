# Manuais de Validação de Negócio — EprosERP (para o time de negócio)

**Para que serve:** cada manual descreve, em linguagem de negócio, **o que o módulo faz hoje** e traz um
**roteiro de validação (seção 8)** — "abra a tela X, faça Y, confira Z". Você usa para conferir, módulo a
módulo, se o sistema corresponde ao processo real da empresa e **anotar o que estiver diferente**.

## Como ler (honestidade — importante)
- **✅ / texto normal** = está construído (confirmado no código as-built).
- **⚠️ pendente / a validar** = a especificação prevê, mas o código ainda não entrega — **não valide como pronto**.
- **🟠 valida-contador / valida-jurídico** = regra fiscal/tributária/trabalhista que **depende de homologação do
  contador/jurídico** antes do go-live. O sistema hoje registra o valor, mas **não calcula a regra** — isso é por
  design (não inventamos regra fiscal).
- **[API] / [backend-only]** = a função existe no motor, mas **ainda não tem tela** — nesta V1 depende de apoio técnico.

## Comece por aqui
1. **0_APLICATIVO** — é o mais maduro (usuários, papéis, planos, cobrança SaaS, permissões). 12 manuais.
2. Espinha operacional: **1_CADASTROS_BASE → ESTOQUE → COMPRAS → FINANCEIRO → VENDAS**.
3. Demais módulos de domínio.

## Índice (18 módulos)
| Pasta | Módulo | Nº manuais |
|---|---|---|
| `0_APLICATIVO/` | Aplicativo / Control-plane SaaS | 12 |
| `1_CADASTROS_BASE/` | Cadastros Base (pessoas, empresas, produtos, geografia) | 1 |
| `ESTOQUE/` | Estoque / WMS / Rastreabilidade | 1 |
| `COMPRAS/` | Compras / Suprimentos | 1 |
| `FINANCEIRO/` | Financeiro / Contabilidade / Tesouraria | 1 |
| `VENDAS/` | Vendas / CRM / Garantias / PDV | 1 |
| `PRODUCAO/` | Produção / PCP / MES | 1 |
| `QUALIDADE/` | Qualidade / AQL / Não-conformidade | 1 |
| `RH/` | RH / Folha / Ponto | 1 |
| `MANUTENCAO/` | Manutenção / Ativos | 1 |
| `PROJETOS/` | Projetos / WBS / EVM | 1 |
| `GRC/` | Governança, Risco e Compliance | 1 |
| `ESG/` | ESG / Emissões | 1 |
| `IMOBILIARIA/` | Imobiliária / Locações | 1 |
| `CONCESSIONARIAS/` | Concessionárias / DMS / F&I | 1 |
| `AGRICULTOR/` | Agricultor / LCDPR | 1 |
| `RELATORIOS/` | Relatórios / BI | 1 |
| `PLATAFORMA_COMPARTILHADA/` | Transversais (Workflow, GED, ICP, conectores) | 1 |

> **Nota de versão:** os 12 de Aplicativo são as-built verificados (módulo fechado). Os 17 demais são **V1 para
> validação, derivados da EF + código as-built em 2026-08-03** — o as-built detalhado e 100% verificado rodando
> sai no fechamento de cada módulo (marcha módulo-a-módulo em andamento). Divergências que você anotar entram
> direto nesse fechamento.
</content>
