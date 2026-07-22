# MC_SPED_EFD_V1

## 1. Identificacao

| Item | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | Plataforma Compartilhada |
| Submodulo | Faturamento Fiscal Eletronico |
| Documento | Matriz de completude - SPED/EFD |
| Versao | V1 |
| Status | Concluido |

## 2. Cobertura do material

| Capacidade | Status | Evidencia funcional consolidada |
|---|---|---|
| EFD ICMS/IPI | Parcial | Geracao fiscal citada para EFD ICMS/IPI. |
| EFD Contribuicoes | Parcial | Codigo/estrutura citada e 41 registros informados, sem dicionario completo. |
| Preview de arquivo | Parcial | Preview de arquivo comprovado. |
| Estruturas de dados | Incompleto | Material cita 40 estruturas/visoes, mas nao informa campos completos. |
| Regras | Incompleto | Material cita 50 regras, mas nao informa regras completas neste recorte. |
| Telas/operacao | Parcial | Material cita 8 telas/visoes operacionais, sem contrato completo. |
| Apuracao ICMS | Parcial | Material cita apuracao ICMS manual. |
| Livros/termos fiscais | Parcial | Material cita livros e termos fiscais. |
| ECD | Incompleto | Citado como codigo presente, mas sem especificacao funcional suficiente. |
| Assinatura/transmissao | Pendente | Nao informado no material. |

## 3. Itens de completude

| Codigo | Area | Status | O que existe | O que falta | Prioridade |
|---|---|---|---|---|---|
| MC-SPED-001 | EFD ICMS/IPI | Parcial | Escopo de geracao fiscal e preview. | Layout completo, blocos, registros, campos, validacoes e regras por registro. | P0 |
| MC-SPED-002 | EFD Contribuicoes | Parcial | 41 registros citados. | Lista dos registros, dicionario, obrigatoriedade, origem dos dados e regras. | P0 |
| MC-SPED-003 | Preview | Parcial | Preview de arquivo comprovado. | Definir formato, validacoes exibidas, download e permissoes. | P1 |
| MC-SPED-004 | Fontes de dados | Parcial | Entradas, saidas, documentos, apuracao, cadastros, livros e termos. | Definir campos, filtros, periodo, consolidacao e regras de corte. | P0 |
| MC-SPED-005 | Apuracao ICMS | Parcial | Apuracao manual citada. | Definir modelo, formulas, ajustes, saldos e relacao com registros. | P0 |
| MC-SPED-006 | Simples Nacional | Parcial | Tabela citada. | Definir uso no arquivo, vigencia e campos. | P1 |
| MC-SPED-007 | Estruturas funcionais | Incompleto | 40 estruturas/visoes citadas. | Levantar campos, chaves, filtros e relacionamentos. | P0 |
| MC-SPED-008 | Regras funcionais | Incompleto | 50 regras citadas. | Levantar conteudo de cada regra e testes. | P0 |
| MC-SPED-009 | Operacao/telas | Parcial | 8 telas/visoes citadas. | Definir experiencia final, filtros, acoes, erros e auditoria. | P1 |
| MC-SPED-010 | Arquivo definitivo | Incompleto | Geracao citada. | Definir nomeacao, armazenamento, codificacao, assinatura, download e retencao. | P0 |
| MC-SPED-011 | Validacao oficial | Pendente | Nao informado no material. | Definir validacao externa/interna, mensagens e bloqueios. | P0 |
| MC-SPED-012 | Transmissao/entrega | Pendente | Nao informado no material. | Definir se Epros apenas gera arquivo ou tambem controla entrega/protocolo. | P1 |
| MC-SPED-013 | Fronteira de modulo | Parcial | Macro indica possivel fronteira com Relatorios/Obrigacoes. | Decidir se fica em Faturamento Fiscal, Relatorios ou Contabilidade/Obrigacoes. | P0 |
| MC-SPED-014 | ECD | Incompleto | Citado como presente, sem detalhe funcional suficiente. | Decidir se fica fora desta EF ou recebe EF propria apos fonte completa. | P2 |

## 4. Decisoes pendentes

| Codigo | Decisao | Motivo |
|---|---|---|
| D-SPED-001 | Confirmar se SPED/EFD pertence a Faturamento Fiscal, Relatorios ou modulo de Obrigacoes/Contabilidade. | Define fronteira de construcao. |
| D-SPED-002 | Levantar layout completo EFD ICMS/IPI. | Necessario para arquivo implantavel. |
| D-SPED-003 | Levantar os 41 registros EFD Contribuicoes com campos e regras. | Material informa quantidade, nao dicionario. |
| D-SPED-004 | Definir se Epros fara apenas geracao/preview ou tambem controle de entrega. | Define assinatura, protocolo e auditoria. |
| D-SPED-005 | Definir armazenamento e retencao de arquivos SPED/EFD. | Necessario para compliance fiscal. |
| D-SPED-006 | Definir massa de testes por arquivo, bloco, registro e periodo. | Necessario para homologacao. |

## 5. Proximo passo operacional

O documento especifico seguinte da fila macro, `EF_SINTEGRA`, foi concluido. O proximo item da matriz principal e `PLATAFORMA_COMPARTILHADA/IA_ML`.
