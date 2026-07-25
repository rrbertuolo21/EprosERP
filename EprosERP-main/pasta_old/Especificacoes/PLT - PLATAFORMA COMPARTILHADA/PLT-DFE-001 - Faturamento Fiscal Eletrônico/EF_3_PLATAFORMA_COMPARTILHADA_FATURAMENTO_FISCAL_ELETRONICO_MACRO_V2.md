# Especificacao Funcional Macro - Epros

**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Documento:** EF macro do submodulo  
**Versao:** V2  
**Empresa:** Siser  
**Status:** Refinamento granular iniciado  

## 1. Controle do documento

| Item | Conteudo |
|---|---|
| Responsavel pela elaboracao | Analise funcional assistida |
| Responsavel pela validacao funcional | Siser |
| Responsavel pela validacao tecnica | Siser |
| Area dona do processo | Fiscal, Vendas, Compras, Estoque, Financeiro, Cadastros, Plataforma, Relatorios |
| Publico-alvo | Produto, negocio, implantacao, desenvolvimento, suporte, operacao fiscal |
| Fonte de verdade | Esta EF macro organiza o escopo fiscal eletronico do Epros e direciona as EFs especificas |

## 2. Objetivo funcional

O submodulo Faturamento Fiscal Eletronico existe para centralizar a operacao fiscal eletronica do Epros, incluindo emissao, consulta, eventos, armazenamento, importacao, downloads, parametrizacao fiscal e suporte ao motor de calculo tributario.

O material de origem comprova que o escopo e maior do que NF-e, NFC-e e NFS-e. Existem tambem referencias ou regras para CT-e, MDF-e, Manifesto DFe, CF-e/SAT, Sintegra, SPED/EFD, NF-e entrada, devolucao, XML de contador, cadastros fiscais e calculo tributario. Esta versao macro estabelece a organizacao correta para que cada documento/evento fiscal seja detalhado em EF propria quando houver conteudo suficiente.

## 3. Principio de refinamento

| Principio | Aplicacao |
|---|---|
| Nao inventar | Documento, evento ou obrigacao fiscal so recebe regra, campo ou fluxo quando houver base no material. |
| Aproveitar o maximo | Toda tabela, campo, regra, tela, API, fluxo, status e lacuna devem ser absorvidos no documento macro ou no documento especifico correspondente. |
| Separar granularidade | A EF macro descreve a plataforma fiscal; as EFs especificas detalham cada documento/evento/obrigacao. |
| Preservar fonte definitiva | Os documentos descrevem o Epros no presente, sem narrativa historica, sem nomes de sistemas anteriores e sem arquivos de codigo. |
| Marcar falta de conteudo | Quando houver apenas citacao, o item fica na MC como incompleto ou nao informado no material. |

## 4. Escopo macro

### 4.1 Dentro do escopo

| Capacidade | Status de conteudo | Documento especifico esperado | Observacao |
|---|---:|---|---|
| Parametros fiscais por empresa | Com conteudo | EF_PARAMETROS_FISCAIS_EMPRESA | Ambientes, series, proximos numeros, CSC, certificado e impressao fiscal. |
| NF-e saida | Com conteudo | EF_NFE_SAIDA | Emissao, DANFE, XML, transmissao, autorizacao, rejeicao, download e vinculo com venda. |
| NFC-e / PDV | Com conteudo | EF_NFCE_PDV | Emissao modelo 65, CSC, DANFCE, PDV, bloqueio de edicao e impressao. |
| NF-e entrada | Com conteudo | EF_NFE_ENTRADA | Material traz emissao sobre compra, XML de entrada, importacao XML e integracao parcial com compras/estoque/financeiro. |
| Devolucao | Com conteudo | EF_DEVOLUCAO_FISCAL | Upload de XML, estados, transmissao, cancelamento, correcao e numeracao compartilhada. |
| Cancelamento fiscal | Com conteudo | EF_CANCELAMENTO_DFE | Documento autorizado, retorno autorizado, XML/PDF de cancelamento e duplicidade. |
| Carta de correcao | Com conteudo | EF_CARTA_CORRECAO | Sequencia, texto de correcao, XML do evento e impressao do evento. |
| Inutilizacao de numeracao | Com conteudo | EF_INUTILIZACAO_NUMERACAO | Faixa numerica, serie, UF, ambiente, protocolo, XML e status fiscal. |
| NFS-e | Com conteudo parcial | EF_NFSE | Emissao por lote, consulta lote, consulta RPS, cancelamento e dados de prestador/tomador/servico. |
| CT-e | Com conteudo parcial | EF_CTE | Existem regras de habilitacao, permissoes, estados, referencia a NF-e e importacao XML. |
| MDF-e | Com conteudo parcial | EF_MDFE | Existem regras de permissao, consulta de nao encerrados, encerramento e flag de encerramento. |
| Manifesto DFe | Com conteudo | EF_MANIFESTO_DFE | Consulta por NSU, limite diario, ciencia, confirmacao, desconhecimento, operacao nao realizada e download XML. |
| CF-e/SAT | Com conteudo parcial | EF_CFE_SAT | Material cita status, modelo fiscal 59, parametros e processamento dedicado. |
| XML contador e downloads fiscais | Com conteudo | EF_XML_CONTADOR_DOWNLOADS | Download por periodo, XML com/sem PDF, chave, mes, ano, venda, compra e ZIP. |
| Importacao XML | Com conteudo | EF_IMPORTACAO_XML | XML/ZIP, validacao, duplicidade, cadastro, status de XML/cadastro/PDF e efeitos operacionais. |
| Cadastros fiscais | Com conteudo | EF_CADASTROS_FISCAIS | CFOP, CFOP padrao, NCM, tributacao, CEST, ANP, IPI, FCP, ICMS interestadual, beneficio e observacao. |
| Motor de calculo tributario | Com conteudo | EF_MOTOR_CALCULO_TRIBUTARIO | Validacoes, matrizes CFOP/CST/CSOSN, ICMS, PIS, COFINS, IPI, IBS/CBS, ISS, IBPT e rateios. |
| IBPT e classificacoes tributarias | Com conteudo | EF_IBPT_CLASSIFICACOES | Aliquotas por NCM/UF, classificacao IBS/CBS, anexos, vigencia e aplicabilidade por modelo. |
| SPED/EFD | Com conteudo parcial | EF_SPED_EFD | Material traz geracao fiscal, registros, EFD ICMS/IPI, EFD Contribuicoes e fronteira com relatorios/obrigacoes. |
| Sintegra | Com conteudo parcial | EF_SINTEGRA | Material traz geracao mensal, dependencias cadastrais, tamanho fixo de linha e registros. |
| eSocial | Sem conteudo localizado | Nao gerar EF nesta etapa | Nao informado no material deste submodulo. |
| Reinf | Sem conteudo localizado | Nao gerar EF nesta etapa | Nao informado no material deste submodulo. |

### 4.2 Fora do escopo macro

| Item | Tratamento |
|---|---|
| Pedido de venda, pedido de compra e recebimento fisico | Permanecem nos modulos donos; este submodulo recebe ou devolve efeitos fiscais. |
| Contas a receber e contas a pagar | Permanecem no Financeiro; efeitos devem ser integrados quando documento fiscal autorizar, cancelar ou importar. |
| Cadastro mestre de pessoa, empresa, produto e endereco | Permanecem em Cadastros Base; este submodulo valida e consome dados fiscais. |
| Estoque operacional | Permanecem em Estoque; manifesto, importacao XML e compras podem disparar efeitos integrados. |

## 5. Arquitetura funcional fiscal

| Camada funcional | Papel no Epros | Conteudo comprovado |
|---|---|---|
| Emissao e protocolo fiscal | Transmitir, consultar e registrar documentos fiscais eletronicos. | NF-e, NFC-e, NFS-e, cancelamento, CC-e, inutilizacao, downloads e status. |
| Parametrizacao fiscal | Manter parametros, regras e cadastros consumidos pela emissao/calculo. | CFOP, NCM, grupo tributario, tipo de operacao, beneficios, FCP, ICMS interestadual, impressao NFC-e. |
| Motor de calculo | Calcular/validar impostos, rateios e matrizes fiscais. | ICMS, CSOSN, PIS, COFINS, IPI, IBS/CBS, ISS, IBPT, rateio de frete/desconto/seguro/outros. |
| Documentos complementares | Operar documentos fiscais alem de NF-e/NFC-e. | CT-e, MDF-e, Manifesto DFe, CF-e/SAT, Sintegra e SPED/EFD aparecem com conteudo parcial. |
| Armazenamento e evidencia fiscal | Guardar XML/PDF, gerar ZIP e permitir download auditado. | XML autorizado, XML de envio, XML cancelado, XML CC-e, ZIP contador, PDF fiscal. |
| Integracao operacional | Notificar vendas, compras, estoque, financeiro, contador e relatorios. | Venda, compra XML, manifesto para compra/estoque, XML contador e efeitos financeiros aparecem no material. |

## 6. Mapa de documentos, eventos e obrigacoes

| Item fiscal | Natureza | Gatilho | Resultado esperado | Status no material |
|---|---|---|---|---|
| NF-e saida | Documento fiscal eletronico | Venda, faturamento ou integracao interna | Documento autorizado, rejeitado ou pendente com XML/PDF/status | Com conteudo |
| NFC-e | Documento fiscal eletronico | PDV, venda presencial ou retaguarda | Documento autorizado/rejeitado/cancelado com DANFCE/XML | Com conteudo |
| NF-e entrada | Documento fiscal de entrada | Compra ou importacao XML | Documento de entrada registrado com XML e efeitos de compra | Parcial |
| NFS-e | Documento fiscal de servico | Lote/RPS de servico | Lote emitido, consultado ou cancelado | Parcial |
| CT-e | Documento fiscal de transporte | Operacao de transporte | Conhecimento transmitido/importado com estado fiscal | Parcial |
| MDF-e | Manifesto fiscal de transporte | Transporte/carga | Documento emitido, consultado, nao encerrado ou encerrado | Parcial |
| Manifesto DFe | Evento/consulta de documento recebido | Consulta distribuicao e manifestacao do destinatario | Ciencia, confirmacao, desconhecimento, operacao nao realizada e XML baixado | Com conteudo |
| CF-e/SAT | Documento fiscal de cupom | Venda fiscal presencial | Documento/status fiscal de cupom | Parcial |
| Cancelamento | Evento fiscal | Documento autorizado | Evento autorizado, XML/PDF de cancelamento e status cancelado | Com conteudo |
| Carta de correcao | Evento fiscal | Documento autorizado com ajuste permitido | Evento de correcao sequenciado com XML/PDF | Com conteudo |
| Inutilizacao | Evento de numeracao | Faixa numerica nao utilizada | Faixa inutilizada com protocolo e XML | Com conteudo |
| Devolucao | Documento/evento operacional fiscal | XML de entrada ou venda devolvida | Documento de devolucao transmitido/cancelado/corrigido | Com conteudo |
| XML contador | Servico fiscal de suporte | Consulta mensal | ZIP com XML e opcionalmente PDF | Com conteudo |
| Importacao XML | Entrada fiscal/documental | Upload XML/ZIP | Cadastro/importacao/PDF processados ou rejeitados | Com conteudo |
| Sintegra | Obrigacao/arquivo fiscal | Periodo mensal | Arquivo gerado com registros e tamanho fixo | Parcial |
| SPED/EFD | Obrigacao/arquivo fiscal | Periodo fiscal | Arquivos fiscais periodicos | Parcial |
| eSocial | Obrigacao trabalhista/fiscal | Nao informado no material | Nao informado no material | Sem conteudo localizado |
| Reinf | Obrigacao fiscal | Nao informado no material | Nao informado no material | Sem conteudo localizado |

## 7. Regras macro

| Regra | Descricao | Aplicacao | Resultado |
|---|---|---|---|
| REG-DFE-MACRO-001 | Todo documento fiscal deve estar vinculado a tenant e empresa fiscalmente identificavel. | Emissao, consulta, importacao e download. | Bloquear operacao sem contexto fiscal. |
| REG-DFE-MACRO-002 | Documento fiscal autorizado deve preservar chave, protocolo, status, XML e, quando aplicavel, PDF. | NF-e, NFC-e, NFS-e, CT-e, MDF-e e eventos. | Garantir evidencia fiscal posterior. |
| REG-DFE-MACRO-003 | Eventos fiscais so podem ocorrer quando o status do documento permitir. | Cancelamento, CC-e, inutilizacao, manifesto e encerramento. | Bloquear evento invalido. |
| REG-DFE-MACRO-004 | A numeracao fiscal deve ser controlada por empresa, ambiente, modelo, serie e documento. | NF-e, NFC-e, NF-e entrada, devolucao e inutilizacao. | Evitar duplicidade e faixa incoerente. |
| REG-DFE-MACRO-005 | Downloads fiscais devem validar permissao, existencia do arquivo e integridade do vinculo fiscal. | XML, PDF, ZIP contador, manifesto e eventos. | Entregar arquivo ou erro funcional claro. |
| REG-DFE-MACRO-006 | Importacao XML deve rejeitar duplicidade e divergencia de empresa quando o material exigir validacao. | Importacao XML, compra XML, devolucao e manifesto. | Impedir entrada fiscal inconsistente. |
| REG-DFE-MACRO-007 | Parametros fiscais de producao devem estar completos antes da emissao. | NF-e, NFC-e e demais documentos que dependam de certificado/ambiente. | Bloquear transmissao incompleta. |
| REG-DFE-MACRO-008 | Regras tributarias aplicadas na emissao devem vir de cadastros fiscais e motor de calculo. | Itens, totais, rateios e tributos aproximados. | Calculo fiscal rastreavel. |
| REG-DFE-MACRO-009 | Quando um item fiscal tiver apenas referencia parcial, ele deve ser documentado como parcial e detalhado na MC. | CT-e, MDF-e, CF-e/SAT, Sintegra, SPED/EFD e obrigacoes nao localizadas. | Evitar inventar campos/regras. |
| REG-DFE-MACRO-010 | A EF especifica de cada item deve conter modelo de dados funcional e dicionario antes de ser considerada implantavel. | Todos os documentos filhos. | Garantir validacao por humano e construcao. |

## 8. Estados macro

| Estado | Significado | Aplicavel a |
|---|---|---|
| Recebido | Documento ou requisicao fiscal recebida pelo Epros. | Emissao/importacao/consulta. |
| NaoProcessado | Registro aguardando processamento. | Importacao XML e processamento assicrono quando houver. |
| Processando | Operacao fiscal em execucao. | Importacao, transmissao, download ou geracao. |
| Autorizado | Autoridade fiscal autorizou o documento/evento. | NF-e, NFC-e, cancelamento, inutilizacao e demais eventos quando comprovado. |
| Rejeitado | Autoridade fiscal ou validacao funcional rejeitou a operacao. | Emissao, evento, importacao. |
| Cancelado | Documento/evento foi cancelado quando permitido. | NF-e, NFC-e, NFS-e e devolucao quando comprovado. |
| Finalizado | Rotina terminou com sucesso operacional. | Importacao, download, ZIP, processamento. |
| Erro | Rotina terminou com falha. | Importacao, transmissao, PDF, XML, servico fiscal. |
| Encerrado | Documento de transporte encerrado. | MDF-e quando detalhado. |
| Nao informado no material | Estado requerido mas sem dominio final no material. | Itens sem conteudo suficiente. |

## 9. Modelo de dados funcional e implantavel

### 9.1 Grupos de dados macro

| Grupo | Entidades/tabelas comprovadas ou esperadas no detalhamento | Papel funcional |
|---|---|---|
| Parametros fiscais | empresa_parametros_dfe, configuracao_impressao_nfce | Habilitar emissao por empresa, ambiente, serie, numero, CSC, certificado e layout. |
| NF-e/NFC-e simplificadas | nfe_simplificado, nfe_simplificado_item, nfe_simplificado_xml, nfce_simplificado, nfce_simplificado_item, nfce_simplificado_xml | Persistir documentos, itens, XML, status, chave, protocolo, retorno e arquivos. |
| Eventos de NF-e/NFC-e | nfe_simplificado_cancelamento, nfce_simplificado_cancelamento, nfe_simplificado_carta_correcao, inutilizacao_simplificado | Registrar cancelamentos, CC-e e inutilizacoes. |
| NFS-e | DTOs de lote, RPS, prestador, tomador, servico, valores, contato, endereco, IBS/CBS e cancelamento | Estruturar emissao, consulta e cancelamento de servico. |
| Manifesto DFe | manifestos, manifesto_limites, item_dves | Consultar distribuicao, manifestar, baixar XML e controlar limite diario. |
| CT-e | ctes | Controlar documento de transporte com referencia a NF-e e importacao XML. |
| MDF-e | mdves | Controlar manifesto de transporte, consulta de nao encerrados e encerramento. |
| CF-e/SAT | Status_CFe, parametros SAT/CFe | Controlar cupom fiscal eletronico quando detalhado. |
| XML e arquivos | caminhos logicos de XML/PDF/ZIP, importacao_xml, importacao_arquivo_xml_saida | Armazenar arquivos fiscais e controlar importacao/download. |
| Cadastros fiscais | cfop, cfop_padrao, ncm, ncm_tributacao, tributario_grupo, tipo_operacao_fiscal, codigo_beneficio_fiscal, cest, codigo_anp, enquadramento_ipi, fcp_aliquota_uf, icms_aliquota_interestadual, classificacao_tributaria, cst_ibs_cbs, observacao_nfe | Parametrizar operacoes fiscais e calculo tributario. |
| Obrigacoes fiscais | estruturas de Sintegra, SPED/EFD e registros fiscais periodicos | Gerar arquivos e evidencias fiscais periodicas quando detalhado. |

### 9.2 Dicionario macro

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---:|---|---|---|
| TenantId | Texto/identificador | Nao informado no material | Sim | Relacao com tenant | Usado para isolamento fiscal. |
| EmpresaId | Identificador | Nao informado no material | Sim | Relacao com empresa | Empresa emitente/destinataria conforme operacao. |
| ModeloDocumento | Enum/codigo | 55, 65, 59 e outros quando detalhados | Sim | Dominio fiscal | NF-e=55, NFC-e=65, CF-e=59 aparecem no material. |
| Ambiente | Enum | Homologacao/Producao ou codigo equivalente | Sim | Parametro fiscal | Direciona transmissao fiscal. |
| Serie | Numero | Nao informado no material | Condicional | Numeracao fiscal | Obrigatoria para emissao quando aplicavel. |
| Numero | Numero | Nao informado no material | Condicional | Numeracao fiscal | Proximo numero nao pode ser zero em producao quando informado. |
| Chave | Texto | 44 digitos quando chave fiscal | Condicional | Identificacao fiscal | Usada em XML, download, eventos e referencias. |
| Protocolo | Texto | Nao informado no material | Condicional | Retorno fiscal | Gravado quando autorizacao/evento retorna protocolo. |
| StatusFiscal | Enum | Recebido, Autorizado, Rejeitado, Cancelado e equivalentes | Sim | Controle de estado | Determina acoes permitidas. |
| StatusProcessamento | Enum | NaoProcessado, Processando, Finalizado, Erro | Condicional | Rotinas | Usado em importacao/processamentos. |
| XmlAutorizado | Arquivo/texto | XML | Condicional | Documento fiscal | Deve ser preservado quando autorizado. |
| XmlEnvio | Arquivo/texto | XML | Condicional | Documento fiscal | Usado para evidencia de transmissao/envio. |
| XmlEvento | Arquivo/texto | XML | Condicional | Evento fiscal | Cancelamento, CC-e, inutilizacao e manifesto quando aplicavel. |
| PdfDocumento | Arquivo | PDF | Condicional | Documento fiscal | DANFE/DANFCE/PDF fiscal quando disponivel. |
| Justificativa | Texto | Nao informado no material | Condicional | Evento fiscal | Usada em cancelamento, inutilizacao e contingencia quando aplicavel. |
| MotivoCorrecao | Texto | Nao informado no material | Condicional | Carta de correcao | Texto da CC-e. |
| SequenciaEvento | Numero | Nao informado no material | Condicional | Evento fiscal | Usada para CC-e quando informado. |
| NumeroInicial | Numero | Nao informado no material | Condicional | Inutilizacao | Inicio da faixa inutilizada. |
| NumeroFinal | Numero | Nao informado no material | Condicional | Inutilizacao | Fim da faixa inutilizada. |
| CNPJEmitente | Documento | CNPJ | Condicional | Empresa/pessoa | Validacao fiscal do emitente. |
| DocumentoDestinatario | Documento | CPF/CNPJ | Condicional | Pessoa | Validacao conforme operacao. |
| CFOP | Codigo fiscal | 4 caracteres quando informado | Condicional | Cadastro fiscal | Usado em itens e tipo de operacao. |
| NCM | Codigo fiscal | 8 caracteres quando informado | Condicional | Cadastro fiscal | Usado em produto, tributacao e IBPT. |
| CST | Codigo fiscal | Nao informado no material | Condicional | Tributo | Validado no motor tributario. |
| CSOSN | Codigo fiscal | Nao informado no material | Condicional | Tributo | Validado no motor tributario. |
| ValorBase | Decimal | Nao informado no material | Condicional | Calculo | Base de calculo de tributos. |
| Aliquota | Decimal | Nao informado no material | Condicional | Calculo | Exigida conforme tributo/CST/CSOSN. |
| ValorTributo | Decimal | Nao informado no material | Condicional | Calculo | Resultado calculado. |
| CodigoIBPT | Texto | Nao informado no material | Condicional | IBPT/NCM | Base para tributos aproximados. |
| ComPdf | Booleano | Sim/Nao | Condicional | XML contador | Define se ZIP mensal inclui PDF. |
| Mes | Numero | 1-12 | Condicional | Periodo fiscal | Usado em downloads/obrigacoes. |
| Ano | Numero | 4 digitos | Condicional | Periodo fiscal | Usado em downloads/obrigacoes. |
| NSU | Texto/numero | Nao informado no material | Condicional | Manifesto DFe | Usado na consulta de documentos distribuidos. |
| TipoManifestacao | Enum/codigo | Ciencia, confirmacao, desconhecimento, operacao nao realizada | Condicional | Manifesto DFe | Eventos comprovados no material. |
| Encerrado | Booleano | Sim/Nao | Condicional | MDF-e | Controla encerramento quando detalhado. |

## 10. Integracoes macro

| Origem/Destino | Tipo | Dados trocados | Observacao |
|---|---|---|---|
| Vendas | Entrada/Saida | Venda, itens, pagamento, destinatario, status fiscal, chave, XML/PDF | NF-e/NFC-e e cancelamento afetam venda. |
| Compras | Entrada/Saida | XML de entrada, fornecedor, itens, compra, duplicidade, status | Importacao XML e manifesto podem gerar compra. |
| Estoque | Saida | Itens importados, atribuicao de estoque, movimentos decorrentes | Efeitos dependem do processo dono. |
| Financeiro | Saida | Titulo a pagar/receber, fatura, cancelamento fiscal | Criacao financeira fica no modulo dono. |
| Cadastros Base | Entrada | Empresa, pessoa, produto, endereco, certificado, grupo tributario | Dados mestres nao sao duplicados. |
| Plataforma | Entrada/Saida | Tenant, autenticacao, autorizacao, auditoria, storage e arquivos | Controle transversal. |
| Relatorios/Contabilidade | Saida | XML mensal, PDF, SPED/EFD, Sintegra, indicadores fiscais | Obrigos periodicas dependem de detalhamento. |

## 11. EFs especificas a produzir nesta revisao

| Ordem | Documento | Status inicial | Criterio de conclusao |
|---:|---|---|---|
| 1 | EF_PARAMETROS_FISCAIS_EMPRESA | Concluido | Campos, regras, obrigatoriedade e dicionario completos. |
| 2 | EF_NFE_SAIDA | Concluido | Fluxo, campos, estados, XML/PDF, regras e testes. |
| 3 | EF_NFCE_PDV | Concluido | CSC, PDV, impressao, bloqueios e dicionario completo. |
| 4 | EF_NFE_ENTRADA | Concluido | Conteudo parcial aproveitado, modelo de dados registrado e lacunas na MC. |
| 5 | EF_DEVOLUCAO_FISCAL | Concluido | Estados, numeracao, XML e efeitos integrados. |
| 6 | EF_CANCELAMENTO_DFE | Concluido | Status, duplicidade, protocolo, XML/PDF e efeitos. |
| 7 | EF_CARTA_CORRECAO | Concluido | Sequencia, texto, XML, PDF e regras de documento autorizado. |
| 8 | EF_INUTILIZACAO_NUMERACAO | Concluido | Faixa, serie, ambiente, UF, XML, protocolo e validacoes. |
| 9 | EF_NFSE | Concluido | Lote/RPS/prestador/tomador/servico/valores e lacunas municipais. |
| 10 | EF_CTE | Concluido | Estados, referencia a NF-e, importacao XML, permissoes e lacunas. |
| 11 | EF_MDFE | Concluido | Consulta nao encerrados, encerramento, protocolo e lacunas. |
| 12 | EF_MANIFESTO_DFE | Concluido | NSU, manifestacoes, limite diario, XML e compra/estoque. |
| 13 | EF_CFE_SAT | Concluido | Modelo, status, parametros e lacunas. |
| 14 | EF_XML_CONTADOR_DOWNLOADS | Concluido | Filtros, ZIP, PDF, auditoria e erros. |
| 15 | EF_IMPORTACAO_XML | Concluido | XML/ZIP, status, cadastro, PDF, duplicidade, lote, consulta e efeitos condicionais. |
| 16 | EF_CADASTROS_FISCAIS | Concluido | CFOP, CFOP padrao, NCM, tributacao NCM, ST, FCP, grupo, tipo operacao, beneficios, catalogos e IBS/CBS. |
| 17 | EF_MOTOR_CALCULO_TRIBUTARIO | Concluido | Validacoes, matrizes NFC-e, ICMS, PIS, COFINS, IPI, IBS/CBS, ISS, IBPT, rateios e lacunas. |
| 18 | EF_SPED_EFD | Concluido | Conteudo parcial: EFD ICMS/IPI, EFD Contribuicoes, preview, fontes, registros citados e lacunas. |
| 19 | EF_SINTEGRA | Concluido | EF/MC especificas criadas como conteudo parcial-controlado; faltas permanecem na MC. |

## 12. Criterios de aceite da EF macro

| Criterio | Resultado esperado |
|---|---|
| Mapa fiscal completo | Todos os documentos/eventos/obrigacoes citados no material estao listados com status de conteudo. |
| Ausencia de invencao | Itens sem conteudo aparecem como `Nao informado no material` ou lacuna de MC. |
| Granularidade de implantacao | Cada item com conteudo suficiente possui EF especifica prevista. |
| Modelo de dados macro | Principais grupos de dados e campos transversais estao organizados. |
| Caminho de continuidade | A ordem das proximas EFs especificas esta definida. |

## 13. Notas de rodape

[^nota1]: A separacao entre EF macro e EFs especificas por documento/evento/obrigacao foi criada nesta revisao para melhorar a validacao humana e a implantacao, porque o material contem volume e variedade fiscal maiores que uma EF unica consegue expressar com seguranca.
