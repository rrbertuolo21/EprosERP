# MC_MANIFESTO_DFE_V1

## 1. Identificacao

| Campo | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | FATURAMENTO_FISCAL_ELETRONICO |
| Documento | Matriz de completude - Manifesto DFe |
| Versao | V1 |
| Status | Concluido |
| Data | 2026-06-08 |

## 2. Resumo de completude

| Area | Status |
|---|---|
| Consulta por NSU | Parcial |
| Limite diario | Parcial |
| Tipos de manifestacao | Parcial |
| Ciencia e confirmacao | Parcial |
| Download XML | Parcial |
| Compra/fatura | Parcial |
| Itens manifestados | Parcial |
| Permissoes | Parcial |
| XML/protocolo/retorno completo | Incompleto |
| Integracoes completas | Incompleto |

## 3. Matriz de completude

| Item | Capacidade esperada | Status | Evidencia disponivel | O que falta construir/definir | Prioridade |
|---|---|---|---|---|---|
| MC-MAN-001 | Consulta por NSU | Parcial | Material informa consulta de distribuicao por NSU. | Definir ultimo NSU, retorno, paginacao/lote, retries e conciliacao. | P0 |
| MC-MAN-002 | Limite diario de consultas | Parcial | `manifesto_limites` e limite diario comprovados. | Definir contador, janela diaria, valor do limite, escopo por empresa e excecoes. | P0 |
| MC-MAN-003 | Modelo `manifestos` | Parcial | Campos chave, tipo, NSU, fatura_salva, localizacao, documento e valor comprovados. | Definir tipos fisicos, obrigatoriedade, indices, unicidade e auditoria. | P0 |
| MC-MAN-004 | Tipos de manifestacao | Parcial | Dominio 0 a 4 comprovado. | Confirmar regras fiscais completas, permissoes e transicoes permitidas. | P0 |
| MC-MAN-005 | Ciencia | Parcial | Tipo 1 e evento 210210 comprovados. | Definir retorno, protocolo, XML de evento e repeticao. | P0 |
| MC-MAN-006 | Confirmacao | Parcial | Tipo 2 e evento 210200 comprovados; habilita download/impressao. | Definir retorno, protocolo, XML de evento e impacto em compra. | P0 |
| MC-MAN-007 | Desconhecimento | Parcial | Tipo 3 comprovado. | Definir evento fiscal, retorno, protocolo e efeitos posteriores. | P1 |
| MC-MAN-008 | Operacao nao realizada | Parcial | Tipo 4 comprovado. | Definir evento fiscal, justificativa, retorno, protocolo e efeitos posteriores. | P1 |
| MC-MAN-009 | Download XML DFe | Parcial | Download XML apos ciencia/confirmacao comprovado. | Definir armazenamento, retencao, permissoes, integridade e caminho final. | P0 |
| MC-MAN-010 | Geracao de compra/fatura | Parcial | Material informa salvar fornecedor e salvar fatura. | Definir contrato com Compras, contas a pagar, duplicidade e validacoes. | P0 |
| MC-MAN-011 | Cadastro produto | Parcial | Material informa cadastrar produto. | Definir contrato com Cadastros/Estoque, deduplicacao e campos obrigatorios. | P1 |
| MC-MAN-012 | Atribuicao de estoque | Parcial | Material informa atribuir estoque. | Definir deposito, quantidade, custo, unidade, lote/serie e divergencias. | P0 |
| MC-MAN-013 | Controle `fatura_salva` | Parcial | Flag comprovada para impedir duplicidade. | Definir momento de atualizacao, rollback e conciliacao. | P0 |
| MC-MAN-014 | Itens manifestados | Parcial | `item_dves` relaciona produto e NF manifestada. | Definir campos, cardinalidade, chaves, produto e documento fiscal. | P1 |
| MC-MAN-015 | Permissoes | Parcial | Acesso por permissao de visualizacao ou criacao comprovado. | Definir permissoes finais por consultar, manifestar, baixar XML, gerar compra e atribuir estoque. | P0 |
| MC-MAN-016 | Schema XML distribuido | Incompleto | XML e download comprovados, schema nao detalhado. | Definir estrutura, validacao, assinatura, armazenamento e parser. | P0 |
| MC-MAN-017 | Retornos e mensagens | Incompleto | Material nao informa codigos completos. | Definir mensagens, codigos, severidade e reprocessamento. | P1 |
| MC-MAN-018 | Auditoria | Incompleto | Necessaria para consulta/manifestacao/download/compra, nao informada. | Definir usuario, data/hora, origem, payload, retorno e alteracoes. | P0 |
| MC-MAN-019 | Tenant/empresa | Incompleto | Regra macro exige contexto fiscal, material especifico nao detalha. | Definir isolamento, certificado, documento e localizacao. | P0 |
| MC-MAN-020 | Deduplicacao | Parcial | Chave, NSU e fatura_salva comprovados. | Definir unicidade por chave/NSU/documento e tratamento de duplicidade. | P0 |

## 4. Decisoes pendentes

| Decisao | Pergunta | Impacto |
|---|---|---|
| D-MAN-001 | Qual sera o limite diario final de consultas por empresa/contexto fiscal? | Define bloqueio operacional. |
| D-MAN-002 | Quais permissoes finais governam consulta, manifestacao, download XML e geracao de compra? | Define seguranca. |
| D-MAN-003 | Como compra/fatura/estoque serao criados a partir do manifesto? | Define integracao operacional. |
| D-MAN-004 | Qual sera a politica de armazenamento e retencao dos XMLs baixados? | Define evidencia fiscal. |
| D-MAN-005 | Quais eventos e retornos completos serao suportados para tipos 3 e 4? | Define completude fiscal. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_CFE_SAT`, detalhando CF-e/SAT conforme material disponivel.
