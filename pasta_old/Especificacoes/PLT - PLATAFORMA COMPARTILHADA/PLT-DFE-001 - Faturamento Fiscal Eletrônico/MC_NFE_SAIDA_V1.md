# Matriz de Completude - Epros

**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Capacidade:** NFE_SAIDA  
**Versao:** V1  
**Empresa:** Siser  

## 1. Objetivo

Registrar lacunas da emissao de NF-e de saida para que a especificacao seja validada e evolua para implantacao sem inventar regras nao comprovadas no material.

## 2. Resumo de completude

| Item | Status |
|---|---|
| Documento NF-e de saida | Parcial |
| Itens da NF-e | Completo no material para campos extraidos |
| XML de envio/retorno | Completo no material para campos extraidos |
| Fluxo de autorizacao/rejeicao | Parcial |
| Downloads XML/PDF | Parcial |
| Previa/DANFE | Parcial |
| Integracao com venda/faturamento | Parcial |

## 3. Matriz

| ID | Capacidade esperada | Status | Conteudo comprovado | Falta para implantacao | Prioridade |
|---|---|---|---|---|---|
| MC-NFE-001 | Emissao NF-e modelo 55 | Parcial | Modelo 55, emissao simples/completa, transmissao e retorno. | Fechar contrato final de entrada e validacoes por campo. | P0 |
| MC-NFE-002 | Autorizacao fiscal | Parcial | Documento autorizado grava chave, numero, XML, status e protocolo quando retornado. | Definir idempotencia, tentativas, timeout e conciliacao de recibo. | P0 |
| MC-NFE-003 | Rejeicao fiscal | Parcial | Rejeicao grava status e motivo; documento rejeitado pode retransmitir. | Definir fluxo de correcao, historico de tentativas e mensagens padronizadas. | P0 |
| MC-NFE-004 | Numeracao NF-e | Incompleto | Serie e numero/proximo/ultimo numero aparecem no material. | Definir reserva transacional, concorrencia, rollback e faixa por ambiente. | P0 |
| MC-NFE-005 | Previa/DANFE sem autorizacao | Parcial | Material cita previa e DANFE sem autorizacao. | Definir se salva arquivo, validade da previa e permissao. | P1 |
| MC-NFE-006 | Regeneracao PDF | Parcial | Material cita regerar PDF por chave. | Definir logo, template, armazenamento, auditoria e falhas. | P1 |
| MC-NFE-007 | Download XML/PDF | Parcial | Download por chave, localizador externo e periodo. | Definir permissao final, retencao, mascaramento e auditoria de download. | P0 |
| MC-NFE-008 | Documento nfe_simplificado | Parcial | Campos persistidos e tamanhos principais extraidos. | Confirmar dominios de Status, TipoNFe, Ambiente, Serie e Numero. | P0 |
| MC-NFE-009 | Itens nfe_simplificado_item | Parcial | Campos, tipos e tamanhos principais extraidos. | Confirmar dominios de CFOP, CST, CSOSN, origem, unidade e indicadores. | P0 |
| MC-NFE-010 | XML nfe_simplificado_xml | Completo no material | XmlEnvio obrigatorio, XmlRetorno opcional, TenantId obrigatorio. | Definir compactacao/assinatura/imutabilidade e retencao. | P1 |
| MC-NFE-011 | Validacoes de emitente | Parcial | CPF/CNPJ, regime/CRT, UF, municipio e telefone aparecem. | Completar obrigatoriedade por tipo de emissao. | P0 |
| MC-NFE-012 | Validacoes de destinatario | Parcial | CPF/CNPJ, IE, endereco, CEP, e-mail e exterior aparecem. | Completar matriz por contribuinte/isento/exterior. | P0 |
| MC-NFE-013 | Tipo de operacao fiscal | Parcial | Finalidade, tipo, CFOP e informacoes complementares aparecem. | Fechar modelo final de consumo por venda/faturamento. | P0 |
| MC-NFE-014 | Pagamentos/cobranca | Parcial | Forma de pagamento, fatura e duplicatas aparecem. | Detalhar campos, parcelas, obrigatoriedade e integracao financeira. | P1 |
| MC-NFE-015 | Transporte | Parcial | Volumes, peso, marca e especie aparecem. | Detalhar campos, transportadora, frete e obrigatoriedade. | P1 |
| MC-NFE-016 | E-mail da NF-e | Incompleto | Material cita e-mail como acao permitida. | Definir destinatarios, anexos, template, auditoria e reenvio. | P2 |
| MC-NFE-017 | Cancelamento e CC-e | Parcial | Relacionamentos e acoes aparecem. | Detalhar em EFs proprias de eventos. | P0 |
| MC-NFE-018 | Retencao legal XML/PDF | Incompleto | Caminhos e downloads existem. | Definir politica legal de guarda, expurgo, backup e imutabilidade. | P0 |
| MC-NFE-019 | Permissoes | Parcial | Material indica necessidade de permissao para download/transmissao. | Definir matriz RBAC final por ator e acao. | P0 |
| MC-NFE-020 | Integracao com venda/faturamento | Parcial | Faturamento aciona NF-e, autorizacao atualiza chave/status, falha pode gerar rollback. | Fechar eventos integrados e responsabilidade por rollback. | P0 |

## 4. Decisoes necessarias

| ID | Decisao | Impacto |
|---|---|---|
| D-NFE-001 | Definir contrato final de emissao simples e completa. | Necessario para desenvolvimento de API/tela. |
| D-NFE-002 | Definir mecanismo de reserva/idempotencia de numeracao. | Evita duplicidade fiscal. |
| D-NFE-003 | Definir matriz de permissoes de emissao, retransmissao e download. | Necessario para seguranca operacional. |
| D-NFE-004 | Definir politica de retencao e imutabilidade XML/PDF. | Necessario para compliance fiscal. |
| D-NFE-005 | Definir se e-mail de NF-e entra nesta fase. | Afeta escopo de comunicacao fiscal. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_NFCE_PDV`, mantendo separacao entre NF-e modelo 55 e NFC-e modelo 65.
