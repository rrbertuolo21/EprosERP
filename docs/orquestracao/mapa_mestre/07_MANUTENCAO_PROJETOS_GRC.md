# Mapa Mestre — MANUTENCAO · PROJETOS · GRC (módulos em quarentena)

> Reconciliação spec × código. Agente de mapa 07. Data 2026-07-22.
> 10 entidades reais (3 MAN + 3 PRJ + 4 GRC) contra ~200 exigidas. 0 DONE.

## Entidades reais
- **MANUTENCAO** (`Modules.Manutencao/Domain/Entities`): `Equipamento`, `OrdemManutencao`, `OrdemManutencaoPeca`. Controller `ManutencaoController.cs`.
- **PROJETOS** (`Modules.Projetos/Domain/Entities`): `Projeto`, `WbsItem`, `AlocacaoRecurso`. Controller `ProjetosController.cs`.
- **GRC** (`Modules.GRC/Domain/Entities`): `RiscoCorporativo`, `ControleInterno`, `Denuncia`, `IncidenteCompliance`. Controller `GRCController.cs` + `DenunciaProcedenteComplianceHandler`.

## Contratos/dependências
- **SoD**: NÃO existe entidade/motor SoD nem evento `ViolacaoSoDDetectada` (só string no enum Origem). Dep. RH+Keycloak inexistente.
- MAN→EST+FIN e PRJ→FIN+RH: só IDs soltos, sem contrato real.

## Tabela resumo

| Submódulo | Módulo | Status | Presentes/Exigidas | Tier |
|---|---|---|---|---|
| CONFIABILIDADE_E_REVISAO | MAN | AUSENTE | 0/8 | G |
| GESTAO_DE_PARADAS | MAN | AUSENTE | 0/9 | M |
| GESTAO_DE_PECAS_DE_REPOSICAO | MAN | SCAFFOLD | 1/10 | M |
| GESTAO_DE_TRABALHO | MAN | PARCIAL | 1/10 | G |
| INDUCAO_E_CONFIGURACAO_DE_EQUIPAMENTOS | MAN | PARCIAL | 1/10 | M |
| MANUTENCAO_PREDITIVA | MAN | AUSENTE | 0/11 | G |
| MANUTENCAO_PREVENTIVA | MAN | AUSENTE | 0/11 | G |
| DEFINICAO_DE_PROJETO | PRJ | PARCIAL | 1/9 | M |
| ENCERRAMENTO_DE_PROJETO | PRJ | AUSENTE | 0/5 | M |
| FATURAMENTO_DE_PROJETO | PRJ | AUSENTE | 0/6 | G |
| GESTAO_DE_RECURSOS | PRJ | PARCIAL | 1/5 | M |
| GESTAO_DE_RISCOS_DE_PROJETO | PRJ | AUSENTE | 0/5 | M |
| PLANEJAMENTO_E_ORCAMENTO | PRJ | SCAFFOLD | 0/6 | G |
| PLANEJAMENTO_E_RASTREAMENTO | PRJ | PARCIAL | 1/8 | G |
| PORTFOLIO_E_PRIORIZACAO | PRJ | AUSENTE | 0/5 | G |
| COMPLIANCE_REGULATORIO | GRC | SCAFFOLD | 1/9 | G |
| CONTROLES_INTERNOS_E_AUDITORIA | GRC | PARCIAL | 1/13 | G |
| GESTAO_DE_POLITICAS | GRC | AUSENTE | 0/11 | M |
| GESTAO_DE_RISCOS_CORPORATIVOS | GRC | PARCIAL | 1/11 | G |
| INVESTIGACOES_E_DENUNCIAS | GRC | PARCIAL | 1/9 | M |
| SEGREGACAO_DE_FUNCOES_SOD | GRC | AUSENTE | 0/12 | G |

Consolidado: 8 AUSENTE · 3 SCAFFOLD · 10 PARCIAL · 0 DONE.
