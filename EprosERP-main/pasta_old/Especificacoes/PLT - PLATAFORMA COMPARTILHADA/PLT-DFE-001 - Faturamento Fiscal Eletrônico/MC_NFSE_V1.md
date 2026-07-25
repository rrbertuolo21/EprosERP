# MC_NFSE_V1

## 1. Identificacao

| Campo | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | FATURAMENTO_FISCAL_ELETRONICO |
| Documento | Matriz de completude - NFS-e |
| Versao | V1 |
| Status | Concluido |
| Data | 2026-06-07 |

## 2. Resumo de completude

| Area | Status |
|---|---|
| Emissao de lote | Parcial |
| Consulta de lote | Parcial |
| Consulta por RPS | Parcial |
| Cancelamento | Parcial |
| Prestador/tomador/servico/valores | Parcial |
| Modelo de dados operacional | Parcial |
| Persistencia fiscal final | Incompleto |
| Parametrizacao municipal | Incompleto |
| Seguranca/autorizacao | Incompleto |
| Integracao financeira/comercial | Incompleto |

## 3. Matriz de completude

| Item | Capacidade esperada | Status | Evidencia disponivel | O que falta construir/definir | Prioridade |
|---|---|---|---|---|---|
| MC-NFSE-001 | Configuracao municipal | Parcial | Material informa configuracao com municipio IBGE e provedor. | Definir tabela/parametros finais por municipio, ambiente, provedor, credenciais e vigencia. | P0 |
| MC-NFSE-002 | Emissao por lote | Parcial | Ha estrutura com NumeroLote, Sincrono, Ambiente, NaturezaOperacao, RegimeEspecialTributacao, OptanteSimplesNacional, IncentivoFiscal, Competencia, RPS, Prestador, Tomador e Servico. | Definir persistencia, retorno, status, protocolo, XML/PDF e idempotencia. | P0 |
| MC-NFSE-003 | Consulta de lote | Parcial | Ha NumeroLote, Protocolo, Ambiente e Prestador. | Definir retorno padronizado, status, erros municipais e conciliacao com emissao. | P0 |
| MC-NFSE-004 | Consulta por RPS | Parcial | Ha NumeroRps, Serie, Tipo, MesCompetencia, AnoCompetencia, Ambiente e Prestador. | Definir retorno padronizado e vinculo com NFS-e autorizada. | P0 |
| MC-NFSE-005 | Cancelamento NFS-e | Parcial | Ha NumeroNfse, CodigoCancelamento, Motivo, Ambiente e Prestador. | Definir regras municipais, prazos, protocolos, efeitos e armazenamento do evento. | P0 |
| MC-NFSE-006 | Prestador | Parcial | Documento, CRT e CodigoMunicipioIbge obrigatorios; demais campos opcionais. | Definir obrigatoriedade municipal de inscricao, razao, UF, endereco, contato e certificado. | P0 |
| MC-NFSE-007 | Tomador | Parcial | Documento e CRT obrigatorios; demais campos opcionais. | Definir obrigatoriedade por tipo de tomador, municipio e regra fiscal. | P0 |
| MC-NFSE-008 | RPS | Parcial | Numero, Serie e Tipo obrigatorios; DataEmissao opcional. | Definir dominios de tipo, sequenciamento, unicidade e relacao com lote. | P0 |
| MC-NFSE-009 | Servico | Parcial | ItemListaServico, CodigoMunicipioIbge, CodigoPais, ExigibilidadeIss, MunicipioIncidencia e Valores obrigatorios. | Definir dominios municipais, CNAE/NBS, tributacao municipal, discriminacao minima e processo. | P0 |
| MC-NFSE-010 | Valores e retencoes | Parcial | Ha valores de servico, deducoes, PIS, COFINS, INSS, IR, CSLL, retencoes, ISS, descontos, total tributos e indicadores. | Definir formulas completas, arredondamento, base de calculo, sinais e validacoes. | P0 |
| MC-NFSE-011 | ISS Simples Nacional | Parcial | Material indica ISS 3.9% para Simples. | Confirmar escopo municipal, vigencia e excecoes antes de regra definitiva. | P0 |
| MC-NFSE-012 | Retencoes regime normal | Parcial | Material indica PIS/COFINS/IR e limites 215 e 666.80. | Definir formulas, bases, vigencia, responsavel pela retencao e regra por municipio. | P0 |
| MC-NFSE-013 | IBS/CBS | Parcial | Ha finalidade, indicadores, valores, redutor, tributos e classificacao. | Definir dominios, obrigatoriedade, formulas e convivencia com ISS. | P1 |
| MC-NFSE-014 | Total de tributos | Parcial | Ha percentuais federal, estadual, municipal e Simples opcional. | Definir origem dos percentuais, exibicao, armazenamento e arredondamento. | P1 |
| MC-NFSE-015 | Endereco | Parcial | CodigoMunicipioIbge e CodigoPais obrigatorios dentro da estrutura; demais campos opcionais. | Definir padrao de CEP, UF, bairro, logradouro e validacao por pais/municipio. | P1 |
| MC-NFSE-016 | Contato | Parcial | Telefone e e-mail opcionais. | Definir validacao e obrigatoriedade por municipio/provedor. | P2 |
| MC-NFSE-017 | Persistencia fiscal final | Incompleto | Material comprova estruturas operacionais, nao tabela de historico NFS-e. | Definir tabelas finais, chaves, status, retorno, protocolo, XML, PDF, auditoria e retencao. | P0 |
| MC-NFSE-018 | Armazenamento XML/PDF | Incompleto | Material nao informa armazenamento final NFS-e. | Definir XML envio, XML retorno, PDF, evento de cancelamento, caminho e politica de retencao. | P0 |
| MC-NFSE-019 | Seguranca/autenticacao | Incompleto | Material indica operacoes sem autenticacao explicita. | Definir autenticacao obrigatoria, autorizacao por papel, trilha de auditoria e segregacao por tenant. | P0 |
| MC-NFSE-020 | Certificado do prestador | Parcial | Prestador possui CertificadoPath e CertificadoSenha opcionais. | Definir armazenamento seguro, obrigatoriedade, validade, substituicao e permissao. | P0 |
| MC-NFSE-021 | Integracao com Vendas/Servicos | Incompleto | Material nao define contrato final de origem comercial. | Definir evento de origem, campos comerciais, status e reemissao. | P1 |
| MC-NFSE-022 | Integracao financeira | Incompleto | Material nao define contas a receber/receita para NFS-e. | Definir quando gera financeiro, impostos retidos, cancelamento e reconciliacao. | P1 |
| MC-NFSE-023 | Permissoes por operacao | Incompleto | Material nao define papeis para emitir, consultar e cancelar. | Definir matriz de permissao por empresa, filial, ambiente e operacao. | P0 |
| MC-NFSE-024 | Mensagens e erros municipais | Incompleto | Material nao lista codigos de erro/retorno. | Definir catalogo de mensagens, severidade e orientacao de correcao. | P1 |
| MC-NFSE-025 | Homologacao/producao | Parcial | Ambiente existe como campo opcional. | Definir dominio, padrao, segregacao de numeracao/RPS e permissao de troca. | P0 |
| MC-NFSE-026 | Auditoria | Incompleto | Necessaria para operacao fiscal, mas estrutura nao informada. | Definir usuario, data/hora, payload, retorno, IP/origem e alteracoes. | P0 |
| MC-NFSE-027 | Idempotencia e duplicidade | Incompleto | Material nao informa regra. | Definir unicidade de lote/RPS/NFS-e por prestador, ambiente e competencia. | P0 |
| MC-NFSE-028 | Reprocessamento | Incompleto | Material possui consultas, mas nao regra de reprocessamento. | Definir quando consultar, reenviar, bloquear ou reconciliar. | P1 |

## 4. Decisoes pendentes

| Decisao | Pergunta | Impacto |
|---|---|---|
| D-NFSE-001 | Qual sera o modelo fisico de persistencia da NFS-e no Epros? | Define tabelas, historico, relatorios e suporte. |
| D-NFSE-002 | Quais municipios/provedores entram na primeira implantacao? | Define parametrizacao, testes e homologacao. |
| D-NFSE-003 | Qual autenticacao/autorizacao sera obrigatoria para operacoes NFS-e? | Resolve risco fiscal P0. |
| D-NFSE-004 | Como NFS-e se integra com vendas/servicos e financeiro? | Define faturamento ponta a ponta. |
| D-NFSE-005 | Como aplicar ISS 3.9% e limites 215/666.80 sem regra municipal completa? | Evita calculo incorreto. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_CTE`, detalhando CT-e conforme material disponivel.
