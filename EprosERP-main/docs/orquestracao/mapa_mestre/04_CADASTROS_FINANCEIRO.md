# Mapa Mestre — CADASTROS_BASE + FINANCEIRO

> Reconciliação spec (EF_*) × código real. Agente de mapa 04. Data 2026-07-22.

## Constatação central
- **CADASTROS_BASE** no `Modules.GestaoClientes` (68 entidades). Geografia e Parâmetros DONE; Pessoa/Organização com CRUD-núcleo pronto mas SEM governança (LGPD/dedup/importação/eventos `pessoa.*`).
- **FINANCEIRO** só 15 entidades. 4/12 submódulos com código: CP e CR (gold standard), CGL (só plano de contas — SEM partida dobrada), Tesouraria (banco/conta/cartão/OFX — sem movimento/caixa/cheque). 8 submódulos AUSENTES.
- Financeiro é **consumidor puro** (não publica outbox); `FatoGeradorFinanceiro` é dono da origem dos títulos. Consome VendaFaturada, CompraLancada, ProjetoFaturado, FolhaProcessada.
- **Sem motor contábil de partida dobrada em NENHUM módulo** (grep LancamentoContabil/Voucher/PeriodoContabil/CentroCusto/AtivoFixo/Depreciacao/Consolidacao/Hedge = 0).

## Tabela resumo

| Submódulo | Status | Faltantes (nº) | Tier | Gap #1 |
|---|---|---|---|---|
| CAD GEOGRAFIA_E_LOCALIZACAO | DONE | 0/8 | P | — |
| CAD PARAMETROS_OPERACIONAIS | DONE | 0/14 | P | — |
| CAD PESSOA_E_ORGANIZACAO (N0) | PARCIAL | ~14/32 | G | governança LGPD/dedup + eventos pessoa.* (REG-PEM-161) |
| FIN CONTAS_PAGAR (gold) | DONE-core | 6/12 | G | cp_despesa/categoria/recorrência/alocação/anexo |
| FIN CONTAS_RECEBER (gold) | DONE-core | 13/17 | G | boleto/fatura próprios (cr_boleto, cr_fatura*, cr_assinatura) |
| FIN CONTABILIDADE_GERAL | PARCIAL | 13/17 | G | partida dobrada ausente (cgl_lancamento/linha/voucher/periodo) |
| FIN TESOURARIA | PARCIAL | 15/21 | G | movimento/transferência/caixa/cheque |
| FIN SERVICOS_FINANCEIROS | AUSENTE | ~35/35 | G | boleto/CNAB 240/400/gateway |
| FIN ATIVOS_FIXOS | AUSENTE | 13/13 | M | zero afx_* |
| FIN CAMBIO_E_RISCO | AUSENTE | 9/10 | M | só Moeda; sem taxa/exposição/hedge |
| FIN CONSOLIDACAO_E_RELATORIOS | AUSENTE | 12/12 | M | zero con_* |
| FIN CONTABILIDADE_GERENCIAL | AUSENTE | 6/6 | M | CentroCusto inexistente |
| FIN PLANEJAMENTO_E_ORCAMENTO | AUSENTE | 15/15 | M | orçamento só em Projetos |
| FIN SUBSIDIOS_E_FUNDOS | AUSENTE | 3/3 | P | zero sbf_* |
| FIN GESTAO_DE_CONTRATOS_FINANCEIROS | AUSENTE | 5/5 | M | Contrato existente é SaaS, não financeiro |

## Direcionamento
- **CAD-PEM é gargalo N0** → completar governança + publicar eventos `pessoa.*` ANTES dos consumidores.
- **FIN-CGL partida dobrada** é o maior bloqueio contábil → construir do zero (F2).
- CP/CR: completar (boleto/CNAB/despesas) — F2/F3.
- Demais FIN: F3/F4.
