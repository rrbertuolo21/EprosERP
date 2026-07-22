# MC_CADASTROS_FISCAIS_V1

## 1. Identificacao

| Item | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | Plataforma Compartilhada |
| Submodulo | Faturamento Fiscal Eletronico |
| Documento | Matriz de completude - Cadastros fiscais |
| Versao | V1 |
| Status | Concluido |

## 2. Cobertura do material

| Capacidade | Status | Evidencia funcional consolidada |
|---|---|---|
| CFOP | Completo no material para campos extraidos | Codigo, descricao, natureza, correlacao, indicadores, incidencia simples e devolucao. |
| CFOP padrao | Parcial | Existe carga por tabela, vigencia e ativacao; governanca final incompleta. |
| NCM | Completo no material para campos extraidos | Codigo, descricao, vigencia e ato inicial. |
| NCM configuracao | Parcial | Existe vinculo por NCM; regra final de unicidade e uso incompleta. |
| Tributacao NCM | Completo no material para campos extraidos | Grupo, beneficio, regra, CFOPs, CST/CSOSN, PIS, COFINS, IPI, ICMS, IBS/CBS e textos. |
| ST por NCM | Completo no material para campos extraidos | UF, tipo de calculo, aliquotas, MVA, reducao, valor unitario e FCP ST. |
| FCP por regra NCM | Completo no material para campos extraidos | UF e percentual por regra. |
| Grupo tributario | Completo no material para campos extraidos | Tenant e descricao obrigatorios. |
| Tipo de operacao fiscal | Completo no material para campos extraidos | Grupo, CFOPs, descricao, sobrescrita, finalidade, atendimento, frete e movimento. |
| Beneficio fiscal | Completo no material para campos extraidos | Codigo, descricao, UF, CSOSN e CST. |
| Observacao NF-e | Completo no material para campos extraidos | Descricao e consulta paginada. |
| CEST | Parcial | Codigo e descricao; uso final por produto/regra nao completo. |
| Codigo ANP | Parcial | Codigo, descricao e vigencia; uso final por produto/regra nao completo. |
| Enquadramento IPI | Parcial | Codigo, descricao e tipo de operacao; matriz de uso final incompleta. |
| FCP UF | Parcial | UF, aliquota, observacao e invalidacao de cache; vigencia nao informada. |
| ICMS interestadual | Parcial | UF origem, UF destino, aliquota e invalidacao de cache; vigencia nao informada. |
| IBS/CBS | Parcial | CST, classificacoes, anexos e indicadores por modelo; detalhamento de atualizacao e calculo fica pendente. |

## 3. Itens de completude

| Codigo | Area | Status | O que existe | O que falta | Prioridade |
|---|---|---|---|---|---|
| MC-CADFIS-001 | CFOP | Completo no material para campos extraidos | Campos, indicadores, correlacao, devolucao e filtro MEI. | Confirmar regra final de ativo/inativo e vigencia operacional. | P0 |
| MC-CADFIS-002 | CFOP padrao | Parcial | Carga por tabela, vigencia e ativacao em lote. | Definir governanca de atualizacao, fonte, versao, rollback e auditoria completa. | P1 |
| MC-CADFIS-003 | NCM | Completo no material para campos extraidos | Codigo, descricao, vigencia e ato inicial. | Definir fonte oficial, versionamento e reconciliacao com produtos. | P1 |
| MC-CADFIS-004 | NCM configuracao | Parcial | `NcmId`, `NcmTributacaoId` e indice por NCM. | Confirmar cardinalidade, unicidade e regra de selecao quando houver mais de uma configuracao. | P0 |
| MC-CADFIS-005 | Tributacao NCM | Completo no material para campos extraidos | Campos fiscais completos e validacoes principais. | Homologar matriz completa CFOP x CST x CSOSN x regime x modelo. | P0 |
| MC-CADFIS-006 | IBS/CBS na regra NCM | Parcial | Campos `CstIbsCbsNfe`, `CClassTribNfe`, `CstIbsCbsNfce`, `CClassTribNfce`. | Definir obrigatoriedade por vigencia, modelo fiscal e regra de fallback. | P0 |
| MC-CADFIS-007 | ST por NCM | Completo no material para campos extraidos | UF, tipo calculo, aliquotas, MVA, reducao, valor unitario e FCP ST. | Definir vigencia, prioridade por UF e conflitos com regra principal. | P0 |
| MC-CADFIS-008 | FCP por regra NCM | Completo no material para campos extraidos | UF e percentual. | Definir vigencia, unicidade por regra/UF e auditoria de alteracao. | P1 |
| MC-CADFIS-009 | Grupo tributario | Completo no material para campos extraidos | Tenant e descricao obrigatorios. | Definir vinculacao obrigatoria por empresa/produto e regra de troca. | P0 |
| MC-CADFIS-010 | Tipo de operacao fiscal | Completo no material para campos extraidos | Campos obrigatorios, CFOPs e duplicidade de descricao. | Definir dominios finais de finalidade, atendimento, frete e movimento. | P0 |
| MC-CADFIS-011 | Beneficio fiscal | Completo no material para campos extraidos | Codigo, descricao, UF, CSOSN/CST e unicidade codigo+UF. | Definir vigencia, aplicabilidade por NCM/produto e regra de desativacao. | P1 |
| MC-CADFIS-012 | Observacao NF-e | Completo no material para campos extraidos | Descricao e retorno paginado. | Definir uso automatico por tipo de operacao, produto ou regra fiscal. | P1 |
| MC-CADFIS-013 | CEST | Parcial | Codigo e descricao. | Definir relacionamento com produto/NCM e obrigatoriedade por operacao. | P1 |
| MC-CADFIS-014 | Codigo ANP | Parcial | Codigo, descricao e vigencia ativa na consulta. | Definir relacionamento com produto, combustivel, CFOP e obrigatoriedade. | P1 |
| MC-CADFIS-015 | Enquadramento IPI | Parcial | Codigo, descricao e tipo de operacao. | Definir aplicabilidade por CST IPI, NCM e tipo de documento. | P1 |
| MC-CADFIS-016 | FCP UF | Parcial | UF, aliquota, observacao e cache. | Definir vigencia, origem da carga e historico de alteracoes. | P1 |
| MC-CADFIS-017 | ICMS interestadual | Parcial | UF origem, UF destino, aliquota e cache. | Definir vigencia, origem da carga e conflito com regra NCM. | P1 |
| MC-CADFIS-018 | Classificacao IBS/CBS | Parcial | CST, classificacoes, anexos e indicadores por modelo. | Definir atualizacao oficial, versionamento, vigencia e validacao com NCM. | P0 |
| MC-CADFIS-019 | Permissoes | Pendente | Menus/operacoes sao citados no material de origem, mas modelo final nao esta fechado. | Definir permissoes por cadastro, carga, edicao, exclusao e consulta. | P0 |
| MC-CADFIS-020 | Auditoria | Parcial | Alteracoes fiscais devem registrar trilha. | Definir campos de auditoria, historico de valor anterior/novo, usuario e motivo. | P1 |
| MC-CADFIS-021 | Retencao historica | Pendente | Nao informado no material. | Definir guarda de versoes e consulta retroativa por data de emissao. | P1 |

## 4. Decisoes pendentes

| Codigo | Decisao | Motivo |
|---|---|---|
| D-CADFIS-001 | Definir matriz fiscal homologada CFOP x CST x CSOSN x regime x modelo. | Evita rejeicoes em NF-e/NFC-e. |
| D-CADFIS-002 | Definir fonte oficial e rotina de atualizacao de CFOP, NCM, FCP, ICMS e IBS/CBS. | Necessario para governanca fiscal. |
| D-CADFIS-003 | Definir regra de vigencia operacional para aliquotas e catalogos. | Necessario para emissao retroativa e auditoria. |
| D-CADFIS-004 | Definir permissoes por cadastro e por carga. | Necessario para implantacao segura. |
| D-CADFIS-005 | Definir impacto de troca de grupo tributario em empresa/produto. | Evita calculo com regra incorreta. |
| D-CADFIS-006 | Definir relacao final de CEST, ANP e enquadramento IPI com produto/NCM. | Necessario para campos obrigatorios por segmento. |
| D-CADFIS-007 | Definir regras finais de cache e invalidacao para todas as tabelas fiscais. | Evita uso de aliquota antiga. |

## 5. Proximo passo operacional

O proximo documento especifico da fila macro e `EF_MOTOR_CALCULO_TRIBUTARIO`, detalhando validacoes, matrizes, impostos, rateios e calculos conforme material disponivel.
