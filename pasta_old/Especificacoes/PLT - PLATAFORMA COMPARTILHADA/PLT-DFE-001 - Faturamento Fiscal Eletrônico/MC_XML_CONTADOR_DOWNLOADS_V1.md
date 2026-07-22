# MC_XML_CONTADOR_DOWNLOADS_V1

## 1. Identificacao

| Campo | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | FATURAMENTO_FISCAL_ELETRONICO |
| Documento | Matriz de completude - XML contador e downloads fiscais |
| Versao | V1 |
| Status | Concluido |
| Data | 2026-06-08 |

## 2. Resumo de completude

| Area | Status |
|---|---|
| Listagem mensal | Parcial |
| ZIP contador | Parcial |
| Download por chave | Parcial |
| XML envio venda/compra | Parcial |
| Regeracao de PDF | Parcial |
| Armazenamento fiscal | Parcial |
| Permissoes | Incompleto |
| Retencao | Incompleto |
| Auditoria | Incompleto |

## 3. Matriz de completude

| Item | Capacidade esperada | Status | Evidencia disponivel | O que falta construir/definir | Prioridade |
|---|---|---|---|---|---|
| MC-XML-001 | Listagem mensal por referencia | Parcial | Mes, ano, pagina e tamanho de pagina comprovados. | Definir filtros adicionais, totalizadores, ordenacao e contrato final. | P0 |
| MC-XML-002 | Colunas da listagem | Parcial | CRT, emitente, destinatario, UF, chave, protocolo, serie, numero, status e emissao comprovados. | Definir tipos, obrigatoriedade, mascaramento e exportacao. | P1 |
| MC-XML-003 | ZIP contador com PDF | Parcial | Nome `XMLS-com-pdfs-{mes}-{ano}.zip` comprovado. | Definir composicao, falhas parciais, tamanho maximo e auditoria. | P0 |
| MC-XML-004 | ZIP contador sem PDF | Parcial | Nome `XMLS-sem-pdfs-{mes}-{ano}.zip` comprovado. | Definir composicao, falhas parciais, tamanho maximo e auditoria. | P0 |
| MC-XML-005 | Download XML por chave | Parcial | Operacao comprovada. | Definir permissao, retencao, MIME, erro e auditoria. | P0 |
| MC-XML-006 | Download PDF por chave | Parcial | Operacao comprovada. | Definir permissao, retencao, geracao/regeneracao e erro. | P0 |
| MC-XML-007 | Download cancelamento | Parcial | XML/PDF cancelamento por chave comprovados. | Definir eventos suportados, permissao e retencao. | P0 |
| MC-XML-008 | Download CC-e | Parcial | XML/PDF CC-e por chave comprovados. | Definir multiplas CC-e, sequencia, permissao e retencao. | P0 |
| MC-XML-009 | XML envio venda | Parcial | Download por VendaId comprovado. | Definir contrato com Vendas, autorizacao e erro. | P1 |
| MC-XML-010 | XML envio compra | Parcial | Download por CompraId comprovado. | Definir contrato com Compras, autorizacao e erro. | P1 |
| MC-XML-011 | Regeracao de PDF | Parcial | Regeracao por chave comprovada. | Definir dados necessarios, layout, logo, falhas e auditoria. | P1 |
| MC-XML-012 | Armazenamento por documento/ano/mes | Parcial | Caminho funcional comprovado. | Definir storage final, particionamento, criptografia e backup. | P0 |
| MC-XML-013 | Permissoes finais | Incompleto | Material indica lacuna/controle insuficiente. | Definir matriz por usuario fiscal, contador, venda, compra e suporte. | P0 |
| MC-XML-014 | Auditoria | Incompleto | Necessaria, nao detalhada. | Definir usuario, data/hora, IP/origem, arquivo, chave e resultado. | P0 |
| MC-XML-015 | Retencao legal | Incompleto | Material nao informa politica final. | Definir tempo, descarte, bloqueio legal e recuperacao. | P0 |
| MC-XML-016 | Acesso do contador | Incompleto | ZIP contador comprovado, modelo de acesso nao informado. | Definir se contador acessa portal, link, envio manual ou integracao. | P1 |
| MC-XML-017 | Falhas parciais de ZIP | Incompleto | ZIP comprovado, erro detalhado nao informado. | Definir se ZIP falha inteiro ou inclui manifesto de pendencias. | P1 |
| MC-XML-018 | Dominio fiscal/principal | Parcial | Consultas de dominio comprovadas. | Definir uso final, cache, seguranca e exibicao. | P2 |

## 4. Decisoes pendentes

| Decisao | Pergunta | Impacto |
|---|---|---|
| D-XML-001 | Qual matriz de permissoes governa download fiscal e ZIP contador? | Define seguranca. |
| D-XML-002 | Qual politica de retencao XML/PDF/ZIP sera adotada? | Define conformidade. |
| D-XML-003 | Como contador acessara os pacotes mensais? | Define experiencia e seguranca. |
| D-XML-004 | Como tratar arquivo ausente em ZIP mensal? | Define suporte e confiabilidade. |
| D-XML-005 | Qual sera o contrato final de auditoria de downloads? | Define rastreabilidade. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_IMPORTACAO_XML`, detalhando importacao XML conforme material disponivel.
