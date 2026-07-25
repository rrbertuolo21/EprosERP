# MC_3_PLATAFORMA_COMPARTILHADA_IMPRESSAO_TERMICA_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** IMPRESSAO_TERMICA  
**Documento:** Matriz de completude  
**Versao:** V1  
**Status:** Concluido  
**Ultima atualizacao:** 2026-06-09

## 1. Objetivo

Registrar o nivel de completude do submodulo Impressao Termica, destacando capacidades comprovadas, lacunas de implantacao e decisoes que a Siser precisa validar para transformar os fluxos existentes em componente padronizado do Epros.

## 2. Resumo de cobertura

| Area | Status | Evidencia funcional consolidada |
|---|---|---|
| Configuracao de impressora | Parcial | Nome de impressora, carregamento local, salvamento e teste informados. |
| Cupom de venda | Amplo | Cabecalho, itens, totais, pagamentos, troco, consumidor e rodape informados. |
| Cupom de compra | Parcial | Estrutura equivalente a venda e itens de compra informados. |
| Cozinha | Parcial | Impressao de item preparado, comanda, data, responsavel e dados adicionais informados. |
| Caixa | Parcial | Abertura, sangria/suprimento e fechamento informados. |
| Fiscal NFC-e/SAT | Parcial | Impressao pos-autorizacao por XML, tipo NFC-e/SAT/perguntar e papel 80mm informados. |
| Mobile/desktop | Parcial | Canais Windows, Android Bluetooth, tamanho 80/60 e reimpressao informados. |
| Relatorios de fechamento | Parcial | 10 relatorios/variantes citados, sem dicionario completo. |
| Etiqueta | Parcial | Nome, preco e codigo de barras informados. |
| API REST | Nao informado no material | Material indica ausencia de endpoint web dedicado. |

## 3. Itens de completude

| Codigo | Area | Status | O que existe | O que falta | Prioridade |
|---|---|---|---|---|---|
| MC-IMP-001 | Configuracao | Parcial | Nome da impressora, tamanho de papel e teste. | Definir entidade fisica final, escopo por empresa/dispositivo/usuario e sincronismo. | P0 |
| MC-IMP-002 | Persistencia local | Parcial | Material informa arquivo/configuracao local. | Definir armazenamento final do Epros e migracao para configuracao governada. | P0 |
| MC-IMP-003 | Cupom venda | Amplo | 26+ regras de layout e conteudo. | Validar formato final, quebras de linha, fontes, truncamentos e privacidade. | P0 |
| MC-IMP-004 | Desconto item | Corrigido na EF | Material indicava comportamento inconsistente. | Validar regra correta com Siser e QA. | P0 |
| MC-IMP-005 | Documento consumidor | Parcial | Prioridade CPF/documento e consumidor nao identificado. | Definir mascara, documento padrao e LGPD. | P0 |
| MC-IMP-006 | Cupom compra | Parcial | Estrutura e itens informados. | Validar titulo correto, totais, pagamentos e campos especificos de compra. | P1 |
| MC-IMP-007 | Cozinha | Parcial | Item preparado, comanda, responsavel e dados adicionais. | Definir multiplas impressoras, setor de preparo, reimpressao e cancelamento. | P1 |
| MC-IMP-008 | Abertura caixa | Parcial | Numero, data/hora, operador, valor e assinatura. | Definir layout final, permissao e copia/reimpressao. | P1 |
| MC-IMP-009 | Sangria/suprimento | Parcial | Tipo e motivo informados. | Definir campos financeiros completos e aprovacao quando aplicavel. | P1 |
| MC-IMP-010 | Fiscal NFC-e/SAT | Parcial | XML autorizado, decisao de canal, cancelado, logo e papel. | Definir contrato final com fiscal, status, reimpressao, erros e contingencia. | P0 |
| MC-IMP-011 | UF 23/35 | Parcial | Regra de decisao SAT/NFC-e informada. | Validar se permanece como regra do Epros ou parametrizacao fiscal. | P0 |
| MC-IMP-012 | Papel 80/58/60 | Parcial | 80mm, 60mm e mencao a 58mm. | Definir dominios oficiais e comportamento por canal. | P1 |
| MC-IMP-013 | Erros e monitoramento | Parcial | Mensagem operacional e monitoramento citados. | Definir tabela, severidade, retry, telemetria e suporte. | P0 |
| MC-IMP-014 | Bluetooth Android | Parcial | Pareados e permissoes citados. | Definir fluxo final, compatibilidade, pareamento, falhas e seguranca. | P1 |
| MC-IMP-015 | Desktop/mobile | Parcial | Canais distintos informados. | Definir se a configuracao sera unificada ou separada por canal. | P1 |
| MC-IMP-016 | Fechamento caixa | Parcial | 10 relatorios/variantes citados. | Definir nomes finais, campos, filtros, exportacao, PDF e permissao. | P1 |
| MC-IMP-017 | Etiqueta | Parcial | Nome, preco e codigo de barras. | Definir tamanhos, impressora, formatos, lote e layout. | P1 |
| MC-IMP-018 | Reimpressao | Parcial | Pedido e NFC-e citados. | Definir permissao, marca de reimpressao, limite e auditoria. | P0 |
| MC-IMP-019 | API | Pendente | Nenhum endpoint dedicado informado. | Decidir se IMP tera API interna, job local ou apenas conector de dispositivo. | P1 |
| MC-IMP-020 | Retencao | Pendente | Nao informado no material. | Definir retencao de solicitacoes, resultados, erros e payloads. | P1 |
| MC-IMP-021 | Permissoes | Pendente | Nao informado no material. | Definir perfis para configurar, imprimir, reimprimir e ver falhas. | P0 |
| MC-IMP-022 | Homologacao de dispositivos | Pendente | Nao informado no material. | Definir lista suportada, testes e politica de suporte. | P1 |

## 4. Decisoes pendentes

| Codigo | Decisao | Motivo |
|---|---|---|
| D-IMP-001 | Definir modelo final de configuracao por empresa, usuario, caixa ou dispositivo. | Evita configuracoes conflitantes. |
| D-IMP-002 | Confirmar canais oficiais suportados na primeira versao. | Define escopo de desenvolvimento e homologacao. |
| D-IMP-003 | Confirmar se regra UF 23/35 sera fixa ou parametrizavel. | Impacta fiscal e PDV. |
| D-IMP-004 | Definir padrao visual final dos cupons e etiquetas. | Necessario para validacao humana e QA. |
| D-IMP-005 | Definir permissao de reimpressao fiscal. | Necessario para seguranca e auditoria. |
| D-IMP-006 | Definir armazenamento de tentativas e erros de impressao. | Necessario para suporte. |
| D-IMP-007 | Definir se relatorios de fechamento ficam em IMP ou Relatorios. | Define fronteira de construcao. |
| D-IMP-008 | Definir politica LGPD para documento de consumidor em cupom. | Necessario para privacidade. |

## 5. Proximo passo operacional

O submodulo `PLATAFORMA_COMPARTILHADA/IMPRESSAO_TERMICA` foi processado e esta concluido. O proximo item da matriz principal e `PLATAFORMA_COMPARTILHADA/INTEGRACAO_IOT`.
