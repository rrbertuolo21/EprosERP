# MC_SINTEGRA_V1

## 1. Identificacao

| Item | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | Plataforma Compartilhada |
| Submodulo | Faturamento Fiscal Eletronico |
| Documento | Matriz de completude - Sintegra |
| Versao | V1 |
| Status | Concluido |

## 2. Cobertura do material

| Capacidade | Status | Evidencia funcional consolidada |
|---|---|---|
| Geracao mensal | Parcial | Material informa geracao por periodo mensal. |
| Empresa ativa | Parcial | Material informa dependencia da empresa ativa. |
| Prerequisitos cadastrais | Parcial | Material informa validacao de prerequisitos cadastrais da empresa. |
| Registros fiscais obrigatorios | Incompleto | Material informa geracao de registros fiscais obrigatorios, mas nao informa dicionario completo. |
| Linha fixa | Parcial | Material informa linha Sintegra com 126 caracteres. |
| Arquivo ANSI | Parcial | Material informa saida em arquivo texto ANSI 1252. |
| Registro 70 | Incompleto | Material informa registro 70 reservado para transporte. |
| Inventario opcional | Parcial | Material informa inventario opcional no Sintegra. |
| Registros citados | Incompleto | Material cita Reg10 a Reg90, 60/61 e 51 a 61, sem campos completos. |
| Tela/operacao | Parcial | Material cita operacao especifica de Sintegra. |

## 3. Itens de completude

| Codigo | Area | Status | O que existe | O que falta | Prioridade |
|---|---|---|---|---|---|
| MC-SIN-001 | Periodicidade | Parcial | Geracao mensal. | Definir fechamento, reabertura, competencia, timezone e regra de corte. | P1 |
| MC-SIN-002 | Empresa ativa | Parcial | Geracao depende da empresa ativa. | Definir campos cadastrais obrigatorios e mensagens por ausencia. | P0 |
| MC-SIN-003 | Prerequisitos cadastrais | Parcial | Validacao cadastral da empresa. | Listar todos os prerequisitos e formatos exigidos. | P0 |
| MC-SIN-004 | Registros obrigatorios | Incompleto | Material informa geracao de registros fiscais obrigatorios. | Definir lista completa, obrigatoriedade por operacao, origem dos dados e regras. | P0 |
| MC-SIN-005 | Reg10 a Reg90 | Incompleto | Reg10 a Reg90 citados. | Levantar campos, ordem, tamanho, preenchimento e validacoes. | P0 |
| MC-SIN-006 | Registros 60/61 | Incompleto | Registros 60/61 citados. | Levantar escopo, campos, relacao com documentos e regras. | P0 |
| MC-SIN-007 | Registros 51 a 61 | Incompleto | Citados como pendentes de detalhamento. | Definir se entram no Epros, campos e regras. | P0 |
| MC-SIN-008 | Registro 70 | Incompleto | Registro 70 reservado para transporte. | Levantar campos, origem, obrigatoriedade e validacoes de transporte. | P1 |
| MC-SIN-009 | Inventario | Parcial | Inventario opcional. | Definir origem, data de posicao, produtos, quantidades, custos e regra de inclusao. | P1 |
| MC-SIN-010 | Tamanho de linha | Parcial | Linhas com 126 caracteres. | Definir validacao por registro, preenchimento, truncamento e tratamento de caracteres. | P0 |
| MC-SIN-011 | Codificacao | Parcial | Arquivo texto ANSI 1252. | Definir conversao, caracteres invalidos e teste de compatibilidade. | P1 |
| MC-SIN-012 | Nome e armazenamento | Pendente | Nao informado no material. | Definir nome do arquivo, pasta, retencao, download e auditoria. | P1 |
| MC-SIN-013 | Validacao oficial | Pendente | Nao informado no material. | Definir validador, mensagens, bloqueios e evidencias. | P0 |
| MC-SIN-014 | Entrega/protocolo | Pendente | Nao informado no material. | Definir se Epros apenas gera arquivo ou tambem controla entrega. | P2 |
| MC-SIN-015 | Permissoes | Pendente | Nao informado no material. | Definir papeis, acoes e segregacao de acesso. | P1 |
| MC-SIN-016 | Relatorios | Pendente | Nao informado no material. | Definir relatorios, filtros, colunas e exportacoes. | P2 |
| MC-SIN-017 | Fronteira de modulo | Parcial | Macro indica possivel relacao com Relatorios/Contabilidade. | Confirmar se Sintegra permanece em Faturamento Fiscal ou passa a obrigacoes/relatorios fiscais. | P0 |

## 4. Decisoes pendentes

| Codigo | Decisao | Motivo |
|---|---|---|
| D-SIN-001 | Confirmar fronteira final de Sintegra no Epros. | Define se a construcao fica em Faturamento Fiscal, Relatorios ou Contabilidade/Obrigacoes. |
| D-SIN-002 | Levantar layout completo dos registros citados. | Necessario para arquivo implantavel. |
| D-SIN-003 | Definir todos os prerequisitos cadastrais da empresa. | Necessario para validacao de geracao. |
| D-SIN-004 | Definir tratamento de inventario opcional. | Necessario para inclusao correta no arquivo. |
| D-SIN-005 | Definir armazenamento, retencao, permissao e auditoria do arquivo. | Necessario para operacao e compliance. |
| D-SIN-006 | Definir se havera validacao oficial e controle de entrega. | Necessario para fechar escopo operacional. |

## 5. Proximo passo operacional

O refinamento granular fiscal planejado na macro esta concluido em 19 de 19 documentos especificos. O proximo item da matriz principal e `PLATAFORMA_COMPARTILHADA/IA_ML`.
