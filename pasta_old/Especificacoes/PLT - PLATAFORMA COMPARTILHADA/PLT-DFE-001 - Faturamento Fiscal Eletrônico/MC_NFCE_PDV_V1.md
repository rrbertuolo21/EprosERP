# Matriz de Completude - Epros

**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Capacidade:** NFCE_PDV  
**Versao:** V1  
**Empresa:** Siser  

## 1. Objetivo

Registrar lacunas da NFC-e/PDV para que a especificacao seja validada e evolua para implantacao sem inventar regras nao comprovadas no material.

## 2. Resumo de completude

| Item | Status |
|---|---|
| Documento NFC-e | Parcial |
| Itens da NFC-e | Completo no material para campos extraidos |
| XML envio/retorno | Completo no material para campos extraidos |
| Emissao PDV | Parcial |
| Impressao DANFCE | Parcial |
| Bloqueio pos-emissao | Completo no material para regra extraida |
| Configuracao de impressao | Parcial |

## 3. Matriz

| ID | Capacidade esperada | Status | Conteudo comprovado | Falta para implantacao | Prioridade |
|---|---|---|---|---|---|
| MC-NFCE-001 | Emissao NFC-e modelo 65 | Parcial | Modelo 65, transmissao PDV/retaguarda, autorizacao e rejeicao. | Fechar contrato final de emissao e retorno. | P0 |
| MC-NFCE-002 | CSC e ID CSC | Parcial | CSC e ID CSC usados na emissao/impressao; obrigatorios em producao pela EF de parametros. | Definir rotacao, troca em producao, mascaramento e auditoria. | P0 |
| MC-NFCE-003 | Numeracao NFC-e | Incompleto | Serie e numero/proximo numero aparecem no material. | Definir reserva transacional, idempotencia e concorrencia PDV. | P0 |
| MC-NFCE-004 | Bloqueio pos-emissao | Completo no material | Numero NFC-e maior que zero impede editar/excluir venda POS. | Confirmar mensagens finais e excecoes administrativas. | P0 |
| MC-NFCE-005 | Impressao DANFCE | Parcial | Usa XML retorno, CSC/ID CSC, impressora e configuracao. | Definir layout final, homologacao de impressoras, contingencia e reimpressao. | P1 |
| MC-NFCE-006 | Impressao nao fiscal | Incompleto | Material cita operacao de impressao nao fiscal. | Definir regra, permissao, uso e diferenca para DANFCE. | P2 |
| MC-NFCE-007 | Configuracao impressao NFC-e | Parcial | EmpresaId obrigatorio, uma por empresa, margens, modo, QR Code e segunda via. | Completar dominios de modo, layout QR Code, versao QR Code e preview. | P1 |
| MC-NFCE-008 | Documento nfce_simplificado | Parcial | Campos persistidos e tamanhos principais extraidos. | Confirmar dominios de Status, TipoNFe, Ambiente, Serie e Numero. | P0 |
| MC-NFCE-009 | Itens nfce_simplificado_item | Parcial | Campos, tipos e tamanhos principais extraidos. | Confirmar dominios de CFOP, CST, CSOSN, origem, unidade e indicadores. | P0 |
| MC-NFCE-010 | Matriz CFOP/CST/CSOSN NFC-e | Parcial | Dominios permitidos e existencia de matriz aparecem no material. | Consolidar matriz completa e excecoes por UF/regime. | P0 |
| MC-NFCE-011 | Contingencia NFC-e | Incompleto | Material cita detalhe de venda em contingencia e segunda via. | Definir fluxo de contingencia, numeracao, impressao, envio posterior e bloqueios. | P0 |
| MC-NFCE-012 | Sincronismo offline PDV | Nao informado no material | Nao ha regra completa neste recorte. | Localizar material de PDV/offline antes de especificar. | P1 |
| MC-NFCE-013 | Download XML/PDF | Parcial | Arquivos e caminhos aparecem no modelo; downloads tratados no fluxo fiscal geral. | Definir permissao, auditoria e retencao especifica. | P1 |
| MC-NFCE-014 | Cancelamento NFC-e | Parcial | Relacao e status cancelado aparecem. | Detalhar em EF de cancelamento, incluindo prazos, justificativa e duplicidade. | P0 |
| MC-NFCE-015 | Permissoes | Incompleto | Acoes aparecem no material, mas matriz final nao. | Definir RBAC por caixa, supervisor, fiscal e suporte. | P0 |
| MC-NFCE-016 | Retencao legal XML/PDF | Incompleto | XML/PDF/caminhos existem. | Definir guarda legal, backup, expurgo e imutabilidade. | P0 |

## 4. Decisoes necessarias

| ID | Decisao | Impacto |
|---|---|---|
| D-NFCE-001 | Definir contrato final de emissao PDV e retaguarda. | Necessario para desenvolvimento da API/tela. |
| D-NFCE-002 | Definir reserva/idempotencia de numeracao em contexto PDV. | Evita dupla emissao. |
| D-NFCE-003 | Definir fluxo de contingencia NFC-e. | Necessario para operacao fiscal em indisponibilidade. |
| D-NFCE-004 | Definir matriz RBAC de emissao, impressao, cancelamento e download. | Necessario para seguranca. |
| D-NFCE-005 | Definir politica de retencao e imutabilidade XML/PDF. | Necessario para compliance fiscal. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_NFE_ENTRADA`, mantendo separado o processo de entrada/compra do processo de venda/PDV.
