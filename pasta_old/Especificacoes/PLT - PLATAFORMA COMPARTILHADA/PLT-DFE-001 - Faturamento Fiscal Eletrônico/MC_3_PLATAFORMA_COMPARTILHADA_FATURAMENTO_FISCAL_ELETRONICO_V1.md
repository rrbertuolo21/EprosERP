# Matriz de Completude - Epros

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Versao:** V1  
**Status:** Em revisao  
**Ultima atualizacao:** 2026-06-06

## 1. Objetivo da matriz

Registrar o que ainda precisa ser confirmado, construido ou detalhado para que Faturamento Fiscal Eletronico alcance padrao implantavel internacional no Epros, sem depender de retorno aos materiais de origem.

## 2. Resumo executivo

| Indicador | Situacao |
|---|---|
| Conteudo funcional aproveitavel | Com conteudo |
| EF criada | Sim |
| Modelo de dados funcional | Parcialmente completo |
| Dicionario de dados implantavel | Parcialmente completo |
| Principais lacunas | Seguranca NFS-e, retencao legal, tenant fiscal, contratos OpenAPI, matriz fiscal completa, regras municipais NFS-e |

## 3. Matriz de completude

| ID | Capacidade esperada | Status | Evidencia funcional disponivel | O que falta construir/validar | Prioridade |
|---|---|---|---|---|---|
| MC-DFE-001 | Parametros fiscais por empresa para NF-e/NFC-e | Parcial | Campos, obrigatoriedade e limites de CSC/serie/proximo numero estao descritos. | Fechar todos os campos de homologacao/producao NF-e e NFC-e, regras de concorrencia de numeracao e historico de alteracao. | P0 |
| MC-DFE-002 | Certificado digital fiscal seguro | Parcial | Caminho, senha, serial, validade e transmissao do certificado aparecem no material. | Definir criptografia, rotacao, alerta de vencimento, revogacao, mascaramento e trilha de acesso. | P0 |
| MC-DFE-003 | Emissao NF-e completa | Parcial | Ha endpoints/capacidades de emissao simples, completa, DANFE previa e regeneracao. | Homologar contrato final de entrada, campos obrigatorios por modelo e estrategia de idempotencia. | P0 |
| MC-DFE-004 | Emissao NFC-e para PDV e retaguarda | Parcial | Ha emissao NFC-e, regras de CFOP/CST/CSOSN e configuracao de impressao. | Fechar contingencia, impressao termica, bloqueio de edicao pos-emissao e sincronismo offline/PDV. | P0 |
| MC-DFE-005 | NFS-e com seguranca e parametrizacao municipal | Incompleto | Ha emitir lote, consultar lote, consultar por RPS, cancelar e config com municipio/provedor. | Definir autenticacao obrigatoria, matriz municipal, provedores, ISS, retencoes, RPS, lote e cancelamento por municipio. | P0 |
| MC-DFE-006 | Cancelamento NF-e/NFC-e | Parcial | Documento autorizado pode cancelar; retorno autorizado e duplicidade sao tratados. | Fechar prazos, justificativa, regras por modelo, conciliacao de duplicidade e efeitos em vendas/financeiro/estoque. | P0 |
| MC-DFE-007 | Carta de correcao | Parcial | Tabela possui chave, sequencia, texto, status, XML e PDF. | Definir regras de texto permitido, limite legal, sequenciamento final e permissao por papel. | P1 |
| MC-DFE-008 | Inutilizacao de numeracao | Parcial | Tabela e fluxo possuem UF, documento, ambiente, ano, serie, faixa, modelo, protocolo e XML. | Fechar regra de concorrencia com numeracao de emissao, bloqueios de faixa e consulta previa obrigatoria. | P0 |
| MC-DFE-009 | XML contador mensal | Parcial | Ha listagem por mes/ano, paginacao e ZIP com ou sem PDF. | Definir filtros finais, politica de acesso contador, retencao, nomeacao de arquivo e reprocessamento. | P1 |
| MC-DFE-010 | Downloads fiscais por chave/venda/compra | Parcial | Ha download XML/PDF autorizados, cancelamento, CC-e e XML de envio. | Consolidar contrato unico de download, autorizacao por empresa e tratamento de arquivo ausente. | P1 |
| MC-DFE-011 | Importacao XML/ZIP | Parcial | Ha upload XML/ZIP, status por etapa, mensagens de erro e validacao de duplicidade. | Definir efeitos finais em compra, estoque, financeiro e cadastro de produtos/clientes. | P0 |
| MC-DFE-012 | CFOP e CFOP padrao | Parcial | Campos, indicadores, tamanhos e ativacao a partir de base padrao estao descritos. | Definir vigencia final, unicidade por tenant, inativacao segura e governanca de carga. | P1 |
| MC-DFE-013 | NCM e regra tributaria por grupo | Parcial | NCM, grupo, CodRegra, CFOPs, CST/CSOSN, PIS/COFINS, IPI, ICMS, IBS/CBS e textos estao mapeados. | Homologar matriz completa por produto/empresa, obrigatoriedade por regime e fallback quando nao houver regra. | P0 |
| MC-DFE-014 | Matriz CFOP x CST x CSOSN NFC-e | Parcial | Dominios permitidos e existencia de validacoes foram mapeados. | Levantar e aprovar matriz completa, incluindo excecoes por UF/regime. | P0 |
| MC-DFE-015 | Beneficio fiscal por UF | Parcial | Codigo, descricao, UF, CSOSN/CST e unicidade funcional estao mapeados. | Definir vigencia, aplicabilidade por produto/NCM e controles de desativacao. | P1 |
| MC-DFE-016 | Classificacao IBS/CBS | Parcial | CST, classificacao, anexos, vigencia e indicadores por modelo foram mapeados. | Fechar atualizacao oficial, versionamento, RBAC e validacao com NCM. | P0 |
| MC-DFE-017 | Aliquotas FCP e ICMS interestadual | Parcial | UF, aliquotas, observacao e invalidacao de cache estao descritos. | Definir carga oficial, vigencia, unicidade e historico. | P1 |
| MC-DFE-018 | IBPT | Parcial | NCM, UF, aliquotas e calculo por base/origem estao mapeados. | Consolidar fonte de atualizacao, versionamento, calendario e reconciliacao com demais instancias fiscais. | P1 |
| MC-DFE-019 | Configuracao de impressao NFC-e | Parcial | Campos de layout, margens, QR Code e unicidade por empresa foram mapeados. | Validar preview, impressoras suportadas, segunda via e contingencia. | P2 |
| MC-DFE-020 | Catalogos e enums fiscais | Parcial | Modelos, ambiente, regime, pagamento, frete, CST, CSOSN, UF e outros dominios foram mapeados. | Fechar catalogo oficial, traducao de labels, vigencia e descontinuacao. | P1 |
| MC-DFE-021 | Contrato OpenAPI fiscal unico | Incompleto | Rotas funcionais foram identificadas para emissao, download, inutilizacao, IBPT, enums e cadastros fiscais. | Desenhar contrato final versionado, autenticado, com erros padronizados e compatibilidade interna. | P0 |
| MC-DFE-022 | Tenant fiscal e tenant corporativo | Incompleto | Material traz tenant fiscal, cliente tenant, usuario e TenantId nos cadastros. | Definir se o tenant fiscal e extensao do tenant corporativo ou servico separado, evitando duplicidade de usuarios. | P0 |
| MC-DFE-023 | Retencao legal e armazenamento | Incompleto | Caminhos funcionais de certificados e XML/PDF estao descritos. | Definir prazo legal, storage, versionamento, backup, criptografia, imutabilidade e descarte. | P0 |
| MC-DFE-024 | Auditoria fiscal completa | Parcial | EF define trilha de status, XML, download, usuario, tenant e alteracoes fiscais. | Definir esquema tecnico-funcional de auditoria e relatorios obrigatorios. | P1 |
| MC-DFE-025 | Permissoes e segregacao fiscal | Parcial | Papeis foram organizados funcionalmente; material traz menus em alguns cadastros. | Fechar matriz Ver/Criar/Editar/Excluir/Emitir/Cancelar/Baixar por papel e empresa. | P0 |
| MC-DFE-026 | Rejeicoes fiscais e mensagens | Parcial | Ha tratamento de certificado, chave, arquivo, rejeicao, documento nao localizado e validacoes. | Criar catalogo final de erros por codigo, severidade, acao do usuario e reprocessamento. | P1 |
| MC-DFE-027 | Testes regressivos fiscais | Parcial | Cenarios de emissao, cancelamento, inutilizacao, XML contador, certificado, IBPT, cadastros e calculo foram mapeados. | Automatizar matriz de testes por modelo, regime, CST/CSOSN, UF e ambiente. | P0 |
| MC-DFE-028 | CT-e, MDF-e, manifesto e obrigacoes fiscais | Incompleto | Existem referencias funcionais, mas sem modelo final completo nesta EF. | Decidir escopo no Epros: incluir neste submodulo, criar submodulos proprios ou deixar para fase posterior. | P1 |
| MC-DFE-029 | Efeitos integrados em vendas/compras/estoque/financeiro | Parcial | EF registra fronteiras e eventos esperados. | Fechar eventos transacionais: autorizacao gera contas/estoque? cancelamento estorna? importacao compra cria CP/estoque? | P0 |
| MC-DFE-030 | Modelo fisico completo de chaves | Incompleto | Campos e tamanhos foram extraidos, mas PKs fisicas nem sempre aparecem. | Definir PK, FK, indices, unicidades e constraints fisicas finais. | P0 |

## 4. Itens P0 para validacao humana

| ID | Decisao necessaria | Impacto se nao decidir |
|---|---|---|
| D-DFE-001 | Modelo de seguranca obrigatorio para NFS-e. | Risco de exposicao fiscal e emissao indevida. |
| D-DFE-002 | Estrategia de tenant fiscal unificada com identidade corporativa. | Duplicidade de usuarios, tokens e permissoes. |
| D-DFE-003 | Contrato final OpenAPI fiscal. | Desenvolvimento pode criar endpoints divergentes. |
| D-DFE-004 | Concorrencia de numeracao e inutilizacao. | Risco de duplicidade ou buraco fiscal nao controlado. |
| D-DFE-005 | Retencao e armazenamento legal de XML/PDF/certificado. | Risco legal, fiscal e operacional. |
| D-DFE-006 | Matriz completa de validacao fiscal por CFOP/CST/CSOSN/NCM/regime. | Emissao com rejeicoes recorrentes. |
| D-DFE-007 | Efeitos integrados pos-autorizacao e pos-cancelamento. | Inconsistencia entre fiscal, estoque e financeiro. |

## 5. Checklist de implantacao

| Item | Status |
|---|---|
| EF criada no formato novo | Concluido |
| MC criada | Concluido |
| Modelo de dados incluido antes do dicionario | Concluido |
| Dicionario com tipo/tamanho/obrigatoriedade quando informado | Concluido |
| Lacunas sem informacao marcadas como `Nao informado no material` | Concluido |
| Nomes de sistemas anteriores removidos dos entregaveis finais | Concluido |
| Copia espelhada em `04_ENTREGAVEIS_REFINADOS` | Pendente ate sincronizacao do ciclo |
| Matriz de execucao atualizada | Pendente ate fechamento do ciclo |

## 6. Nota de controle

Esta MC nao substitui a EF. Ela registra pendencias para validacao, desenho final e implantacao, mantendo a EF como fonte funcional principal do que ja esta consolidado.
