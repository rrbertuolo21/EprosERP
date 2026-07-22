# Matriz de Completude Macro - Epros

**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Documento:** MC macro do submodulo  
**Versao:** V2  
**Empresa:** Siser  

## 1. Objetivo

Registrar as lacunas macro do refinamento granular de Faturamento Fiscal Eletronico, separando o que possui conteudo suficiente, o que possui conteudo parcial e o que nao foi localizado no material.

## 2. Resumo de completude

| Item | Quantidade |
|---|---:|
| Capacidades com conteudo suficiente para EF especifica | 13 |
| Capacidades com conteudo parcial | 6 |
| Capacidades sem conteudo localizado | 2 |
| EFs especificas planejadas nesta revisao | 19 |

## 3. Matriz macro

| ID | Capacidade esperada | Status | Evidencia funcional disponivel | Falta para padrao implantavel | Prioridade |
|---|---|---|---|---|---|
| MC-DFE-MACRO-001 | EF macro do submodulo fiscal eletronico | Concluido | Escopo amplo, mapa de documentos/eventos/obrigacoes, grupos de dados, regras macro e ordem de EFs. | Validacao da Siser sobre a granularidade proposta. | P0 |
| MC-DFE-MACRO-002 | Parametros fiscais por empresa | Parcial | Ambientes NF-e/NFC-e, serie, proximo numero, CSC, certificado e impressao. | Completar dicionario com todos os campos, obrigatoriedade por ambiente e regras de concorrencia. | P0 |
| MC-DFE-MACRO-003 | NF-e saida | Parcial | Emissao, DANFE, XML, autorizacao, rejeicao, cancelamento, CC-e e downloads aparecem no material. | Gerar EF especifica com modelo de dados, dicionario completo, fluxos, regras, mensagens e testes. | P0 |
| MC-DFE-MACRO-004 | NFC-e / PDV | Parcial | Modelo 65, CSC, transmissao, cancelamento, lista de aprovados/cancelados, impressao e bloqueio de edicao. | Gerar EF especifica, incluindo PDV, impressao, contingencia e sincronismo quando comprovado. | P0 |
| MC-DFE-MACRO-005 | NF-e entrada | Parcial | Ha referencias a entrada, numeracao, XML e compra. | Fechar campos, relacao com compras, certificados, eventos e efeitos financeiros/estoque. | P1 |
| MC-DFE-MACRO-006 | Devolucao fiscal | Parcial | Upload XML, estados, transmissao, cancelamento, correcao e numeracao compartilhada. | Gerar EF especifica com dados de entrada, dicionario, efeitos e restricoes. | P1 |
| MC-DFE-MACRO-007 | Cancelamento fiscal | Parcial | Documento autorizado, retorno autorizado, duplicidade, XML/PDF e status cancelado. | Detalhar prazos, justificativa, efeitos integrados e reconciliacao. | P0 |
| MC-DFE-MACRO-008 | Carta de correcao | Parcial | Sequencia, motivo/texto, XML e impressao de evento. | Detalhar limites de texto, permissao, status permitido e efeitos no documento. | P0 |
| MC-DFE-MACRO-009 | Inutilizacao de numeracao | Parcial | Ambiente, serie, numero inicial/final, protocolo, XML e status fiscal. | Completar dicionario, validacoes de faixa, concorrencia e consulta. | P0 |
| MC-DFE-MACRO-010 | NFS-e | Incompleto | Ha lote, consulta lote, consulta RPS, cancelamento e DTOs de prestador/tomador/servico/valores. | Definir matriz municipal, autenticacao, provedores, ISS, retencoes, RPS e armazenamento. | P0 |
| MC-DFE-MACRO-011 | CT-e | Incompleto | Ha habilitacao, permissoes, estados, referencia a NF-e e importacao XML. | Levantar modelo de dados completo, campos, eventos, XML, autorizacao, cancelamento e encerramentos aplicaveis. | P1 |
| MC-DFE-MACRO-012 | MDF-e | Incompleto | Ha permissoes, consulta de nao encerrados, encerramento e flag encerrado. | Levantar modelo completo, campos, eventos, XML, protocolo, veiculos/carga e relacao com CT-e/NF-e. | P1 |
| MC-DFE-MACRO-013 | Manifesto DFe | Parcial | Consulta NSU, limite diario, ciencia, confirmacao, desconhecimento, operacao nao realizada, XML e compra. | Gerar EF especifica com modelo, estados, limites, efeitos em compras/estoque e auditoria. | P1 |
| MC-DFE-MACRO-014 | CF-e/SAT | Incompleto | Ha status, modelo 59, parametros e processamento dedicado. | Levantar emissao, cancelamento, XML, equipamento, parametrizacao e relacao com PDV. | P1 |
| MC-DFE-MACRO-015 | XML contador e downloads fiscais | Parcial | Download por chave/periodo, XML com ou sem PDF, ZIP e erros funcionais. | Detalhar filtros, permissoes, retencao, auditoria, armazenamento e geracao de PDF. | P0 |
| MC-DFE-MACRO-016 | Importacao XML | Parcial | XML/ZIP, duplicidade, validacao de empresa, status de importacao/cadastro/PDF, lote, consulta e efeitos operacionais. | EF especifica criada; ainda faltam idempotencia final, reprocessamento, vinculo fisico lote/XML e regras completas de efeitos em compra, financeiro e estoque. | P0 |
| MC-DFE-MACRO-017 | Cadastros fiscais | Parcial | CFOP, NCM, grupo tributario, tipo operacao, beneficios, CEST, ANP, IPI, FCP, ICMS, classificacoes e observacoes. | EF especifica criada; ainda faltam matriz homologada CFOP/CST/CSOSN, vigencias finais, permissoes, fontes oficiais e regras completas de carga/cache. | P0 |
| MC-DFE-MACRO-018 | Motor de calculo tributario | Parcial | Validacoes CFOP/CST/CSOSN, ICMS, PIS, COFINS, IPI, IBS/CBS, ISS, IBPT e rateios. | EF especifica criada; ainda faltam formulas completas, matriz homologada, arredondamento, residuo de rateio e persistencia fisica do resultado. | P0 |
| MC-DFE-MACRO-019 | SPED/EFD | Incompleto | Ha referencias a EFD ICMS/IPI, EFD Contribuicoes, registros e preview. | EF/MC especificas criadas como parcial-controlada; ainda faltam layout completo, registros, campos, regras, assinatura, validacao oficial, entrega e fronteira final. | P1 |
| MC-DFE-MACRO-020 | Sintegra | Incompleto | EF/MC especificas criadas como parcial-controladas; ha geracao mensal, prerequisitos cadastrais, tamanho fixo de linha, arquivo texto ANSI 1252, registro 70 reservado e inventario opcional. | Levantar layout completo, campos por registro, validacoes oficiais, relatorios, permissoes, armazenamento e dependencia de dados. | P1 |
| MC-DFE-MACRO-021 | eSocial | Nao informado no material | Nenhuma ocorrencia funcional localizada neste submodulo. | Nao criar EF sem fonte; localizar outro material ou registrar como escopo futuro. | P2 |
| MC-DFE-MACRO-022 | Reinf | Nao informado no material | Nenhuma ocorrencia funcional localizada neste submodulo. | Nao criar EF sem fonte; localizar outro material ou registrar como escopo futuro. | P2 |

## 4. Decisoes necessarias

| ID | Decisao | Impacto |
|---|---|---|
| D-DFE-MACRO-001 | Confirmar que o refinamento fiscal deve gerar EFs especificas por documento/evento/obrigacao. | Define o tamanho final do pacote fiscal. |
| D-DFE-MACRO-002 | Confirmar se SPED/EFD e Sintegra permanecem neste submodulo ou serao deslocados para obrigacoes/relatorios fiscais. | Define fronteira de construcao e validacao. |
| D-DFE-MACRO-003 | Confirmar se CT-e, MDF-e e CF-e/SAT entram como escopo completo do Epros nesta fase. | Define prioridade de detalhamento e desenvolvimento. |
| D-DFE-MACRO-004 | Confirmar onde eSocial/Reinf devem ser buscados, ja que nao foram localizados neste pacote. | Evita inventar conteudo fiscal/trabalhista. |

## 5. Proximo passo operacional

O refinamento granular fiscal planejado na macro esta concluido em 19 de 19 documentos especificos. O proximo item da matriz principal e `PLATAFORMA_COMPARTILHADA/IA_ML`.
