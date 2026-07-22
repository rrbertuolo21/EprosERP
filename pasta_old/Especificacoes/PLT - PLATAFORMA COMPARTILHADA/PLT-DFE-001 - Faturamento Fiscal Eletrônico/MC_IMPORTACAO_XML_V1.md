# MC_IMPORTACAO_XML_V1

## 1. Identificacao

| Item | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | Plataforma Compartilhada |
| Submodulo | Faturamento Fiscal Eletronico |
| Documento | Matriz de completude - Importacao XML |
| Versao | V1 |
| Status | Concluido |

## 2. Cobertura do material

| Capacidade | Status | Evidencia funcional consolidada |
|---|---|---|
| Upload XML/ZIP | Completo no material | Arquivo XML ou ZIP como entrada. |
| Empresa na importacao | Parcial | Empresa deve ser identificada; obrigatoriedade fisica final nao esta completa. |
| Validacao de XML | Parcial | XML invalido e emitente divergente aparecem como excecoes. |
| Duplicidade | Parcial | Bloqueio por chave/NfeId/documento aparece como regra funcional; indice final nao informado. |
| Registro `importacao_xml` | Completo no material para campos extraidos | Campos, tamanhos principais, status e mensagens preservados. |
| Registro `importacao_arquivo_xml_saida` | Completo no material para campos extraidos | Campos, contadores, status e mensagem preservados. |
| Status de importacao/cadastro/PDF | Parcial | Dominio das etapas existe; transicoes detalhadas e reprocessamento nao informados. |
| Cadastro relacionado | Parcial | Existe etapa e mensagens; regras completas de criacao/relacao nao informadas. |
| PDF | Parcial | Existe status e mensagem; regra de geracao/salvamento e repositorio final nao informados. |
| Compra/fatura | Parcial | Efeito operacional aparece condicionado a dados completos. |
| Contas a pagar | Parcial | Depende de plano de contas e tipo de pagamento mapeado. |
| Estoque | Parcial | Efeito esperado quando aplicavel; regras finais nao informadas neste recorte. |
| Consulta | Parcial | Retorno com dados e total de registros; filtros finais nao informados. |

## 3. Itens de completude

| Codigo | Area | Status | O que existe | O que falta | Prioridade |
|---|---|---|---|---|---|
| MC-IMP-001 | Upload XML/ZIP | Completo no material | Aceite de XML ou ZIP. | Nao informado no material. | P0 |
| MC-IMP-002 | Validacao de empresa | Parcial | Empresa deve estar identificada para processar. | Confirmar obrigatoriedade fisica de `EmpresaId` e regra para XML sem empresa coerente. | P0 |
| MC-IMP-003 | Validacao do XML | Parcial | XML invalido gera erro. | Definir validacoes estruturais, assinatura, schema e mensagens padronizadas. | P0 |
| MC-IMP-004 | Duplicidade | Parcial | Duplicidade por chave/NfeId/documento deve ser bloqueada. | Definir indice unico final, comportamento em reenvio e criterio por tipo de XML. | P0 |
| MC-IMP-005 | `importacao_xml` | Completo no material para campos extraidos | `TenantId`, `EmpresaId`, `Xml`, `TipoDeXml`, `NfeId`, status, mensagens, codigo fiscal, tipo evento e data. | Confirmar chave fisica, indices e obrigatoriedade final dos campos condicionais. | P0 |
| MC-IMP-006 | `importacao_arquivo_xml_saida` | Completo no material para campos extraidos | `TenantId`, `NomeArquivo`, contadores, status e mensagem. | Confirmar relacionamento fisico com XMLs individuais e dominio do status. | P0 |
| MC-IMP-007 | Status por etapa | Parcial | NaoProcessado, Processando, Finalizado e Erro. | Definir transicoes permitidas, reprocessamento, cancelamento de fila e exibicao. | P0 |
| MC-IMP-008 | Mensagens de erro | Completo no material para tamanho | Mensagens de etapa e lote com varchar(500). | Definir catalogo funcional de mensagens e prioridade de exibicao. | P1 |
| MC-IMP-009 | Cadastro relacionado | Parcial | Status e erro de cadastro existem. | Definir quando cadastrar, quando apenas relacionar e quando exigir validacao humana. | P0 |
| MC-IMP-010 | Produtos e clientes/pessoas | Parcial | Contadores de localizados e importados existem. | Definir matching, chaves, tolerancias, conflitos e revisao humana. | P0 |
| MC-IMP-011 | Unidades e tributacao | Parcial | Erro de cadastro pode envolver unidades e tributacao. | Definir tabelas afetadas, equivalencias e validacoes obrigatorias. | P0 |
| MC-IMP-012 | PDF | Parcial | Status e mensagem de salvamento existem. | Definir repositorio, nomeacao, retentativa e regra quando PDF falhar. | P1 |
| MC-IMP-013 | Compra/fatura | Parcial | Geracao condicionada a XML processado e dados completos. | Definir modelo completo de criacao, campos obrigatorios e idempotencia. | P0 |
| MC-IMP-014 | Financeiro | Parcial | Contas a pagar depende de plano e tipo de pagamento. | Definir contas, parcelas, vencimentos, centro de custo e estorno. | P0 |
| MC-IMP-015 | Estoque | Parcial | Estoque pode ser alimentado pela entrada processada. | Definir operacao, deposito, lote/serie, custo e rollback. | P0 |
| MC-IMP-016 | Consulta de importacoes | Parcial | Retorno com dados e total de registros. | Definir filtros, ordenacao, paginacao e campos exibidos. | P1 |
| MC-IMP-017 | Auditoria | Parcial | Data e mensagens aparecem; usuario/processo aparece no macro. | Definir usuario, IP, origem, antes/depois e historico de reprocessamento. | P1 |
| MC-IMP-018 | Permissoes | Pendente | Usuario autorizado aparece como pre-condicao. | Definir permissoes por upload, consulta, reprocessamento e descarte. | P0 |
| MC-IMP-019 | Retencao | Pendente | Nao informado no material. | Definir prazo de guarda de XML, ZIP, PDF e logs. | P1 |
| MC-IMP-020 | Monitoramento operacional | Pendente | Nao informado no material. | Definir alertas de lote parado, erro recorrente e divergencia de contadores. | P2 |

## 4. Decisoes pendentes

| Codigo | Decisao | Motivo |
|---|---|---|
| D-IMP-001 | Definir indice unico da importacao XML por empresa, tipo e identificador fiscal. | Evita duplicidade e efeitos repetidos. |
| D-IMP-002 | Definir vinculo fisico entre lote e XML individual. | Necessario para rastrear ZIP com multiplos documentos. |
| D-IMP-003 | Definir regra de reprocessamento por etapa. | Permite corrigir cadastro ou PDF sem reler XML indevidamente. |
| D-IMP-004 | Definir politica de matching de pessoas/produtos. | Evita cadastros duplicados e relacoes erradas. |
| D-IMP-005 | Definir regra de geracao financeira completa. | Necessario para implantacao integrada com financeiro. |
| D-IMP-006 | Definir regra de impacto no estoque. | Necessario para implantacao integrada com estoque. |
| D-IMP-007 | Definir filtros finais de consulta. | Necessario para operacao diaria. |

## 5. Proximo passo operacional

O proximo documento especifico da fila macro e `EF_CADASTROS_FISCAIS`, detalhando tabelas fiscais, classificacoes, regras tributarias e dicionarios conforme material disponivel.
