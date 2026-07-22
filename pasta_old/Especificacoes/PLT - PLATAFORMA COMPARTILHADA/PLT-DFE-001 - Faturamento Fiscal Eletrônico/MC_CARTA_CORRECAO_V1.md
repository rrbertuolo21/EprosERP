# Matriz de Completude - Epros

**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Capacidade:** CARTA_CORRECAO  
**Versao:** V1  
**Empresa:** Siser  

## 1. Objetivo

Registrar lacunas da carta de correcao de NF-e para evoluir a especificacao sem inventar regras legais, protocolos, permissoes ou efeitos nao comprovados no material.

## 2. Resumo de completude

| Item | Status |
|---|---|
| CC-e de NF-e | Parcial |
| Relacao com NF-e autorizada | Parcial |
| Sequencia de evento | Parcial |
| Texto de correcao | Completo no material para tamanho extraido |
| Chave da NF-e | Completo no material |
| XML da CC-e | Completo no material para campos extraidos |
| PDF da CC-e | Parcial |
| Downloads por chave | Parcial |
| Rejeicao fiscal | Parcial |
| Protocolo | Incompleto |
| Regras legais do texto | Incompleto |
| Permissoes finais | Incompleto |

## 3. Matriz

| ID | Capacidade esperada | Status | Conteudo comprovado | Falta para implantacao | Prioridade |
|---|---|---|---|---|---|
| MC-CCE-001 | Criar CC-e de NF-e | Parcial | Acao de carta de correcao, texto/motivo e NF-e autorizada com CC-e. | Definir contrato final de entrada, status permitido e mensagens. | P0 |
| MC-CCE-002 | Relacao 1:N com NF-e | Parcial | NF-e possui 1:N cartas de correcao. | Definir FK final, exclusao, auditoria e consulta historica. | P0 |
| MC-CCE-003 | Sequencia de evento | Parcial | `sequencia_cce` e `SequenciaEvento` comprovados. | Definir concorrencia, incremento transacional e tratamento de falha. | P0 |
| MC-CCE-004 | Chave da NF-e | Completo no material | `Chave` varchar(50) NOT NULL. | Confirmar validacao de formato e unicidade com sequencia. | P0 |
| MC-CCE-005 | Texto de correcao | Parcial | `TextoCorrecao` varchar(1000). | Definir conteudo permitido/proibido, tamanho minimo e mensagens. | P0 |
| MC-CCE-006 | Status fiscal | Parcial | `StatusSefaz` comprovado. | Definir dominios, transicoes e cStat aceitos/rejeitados. | P0 |
| MC-CCE-007 | Motivo de rejeicao | Completo no material para campo extraido | `MotivoRejeicaoSefaz` nvarchar(max). | Definir exibicao, historico e reenvio. | P1 |
| MC-CCE-008 | XML da CC-e | Completo no material para campo extraido | `Xml` nvarchar(max), XML em repositorio logico de correcao. | Definir retencao, imutabilidade, backup e assinatura. | P0 |
| MC-CCE-009 | Caminho XML | Parcial | `XmlCaminho` varchar(500). | Definir padrao de path, nome de arquivo e arquivo ausente. | P1 |
| MC-CCE-010 | PDF da CC-e | Parcial | `PdfCaminho` varchar(500), impressao de evento e download PDF. | Definir template, geracao, regeneracao, logo e falhas. | P1 |
| MC-CCE-011 | Downloads por chave | Parcial | Download XML/PDF CC-e por chave. | Definir permissao, auditoria, nome de arquivo e mime type. | P1 |
| MC-CCE-012 | Protocolo de evento | Incompleto | Protocolo aparece em contexto fiscal geral, mas nao no mapeamento da CC-e. | Definir campo, obrigatoriedade e origem no XML. | P0 |
| MC-CCE-013 | Permissoes | Incompleto | Acoes de criar, imprimir e baixar aparecem. | Definir RBAC por ator: criar, consultar, baixar, imprimir e reprocessar. | P0 |
| MC-CCE-014 | Reprocessamento | Incompleto | Rejeicao e motivo aparecem. | Definir quando pode reenviar, se reutiliza sequencia e historico. | P1 |
| MC-CCE-015 | Efeitos em vendas/operacao | Incompleto | CC-e aparece em acoes da NF-e, mas sem contrato operacional. | Definir notificacao e impacto em relatatorios/comercial. | P2 |
| MC-CCE-016 | Testes | Parcial | Ha elementos de criacao, sequencia, XML/PDF e rejeicao. | Completar testes de concorrencia, texto invalido, documento nao autorizado e arquivo ausente. | P1 |

## 4. Decisoes necessarias

| ID | Decisao | Impacto |
|---|---|---|
| D-CCE-001 | Definir regras legais de conteudo permitido/proibido na CC-e. | Necessario para validacao funcional. |
| D-CCE-002 | Definir campo e regra de protocolo da CC-e. | Necessario para auditoria fiscal completa. |
| D-CCE-003 | Definir incremento transacional da sequencia. | Evita colisao de eventos. |
| D-CCE-004 | Definir retencao e imutabilidade de XML/PDF. | Necessario para compliance. |
| D-CCE-005 | Definir matriz de permissoes. | Necessario para seguranca operacional. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_INUTILIZACAO_NUMERACAO`, mantendo separado o evento de inutilizacao das cartas de correcao e cancelamentos.
