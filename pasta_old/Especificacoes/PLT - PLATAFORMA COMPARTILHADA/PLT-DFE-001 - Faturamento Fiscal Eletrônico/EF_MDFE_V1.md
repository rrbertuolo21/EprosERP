# EF_MDFE_V1

## 1. Identificacao

| Campo | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | FATURAMENTO_FISCAL_ELETRONICO |
| Documento | Especificacao funcional - MDF-e |
| Versao | V1 |
| Status | Concluido |
| Data | 2026-06-07 |

## 2. Objetivo funcional

O MDF-e permite ao Epros controlar manifesto eletronico de transporte, consultar documentos nao encerrados, executar encerramento por chave, protocolo e localizacao, manter flag de encerramento e organizar os dados logisticos comprovados: municipios de carregamento, percursos, CIOTs, vale pedagio, informacoes de descarga, NF-e de descarga e CT-e de descarga.

Esta EF descreve somente o que esta comprovado no material canonico. Modelo completo de emissao, autorizacao, veiculos, condutores, carga, XML de retorno, PDF, eventos e regras fiscais ficam registrados como lacunas na MC.

## 3. Escopo

| Area | Incluso | Status |
|---|---|---|
| Permissoes MDF-e | Permissoes analogas ao CT-e, com prefixo `mdfe.*` | Parcial |
| Consulta de nao encerrados | Consulta fiscal de MDF-e nao encerrado | Com conteudo |
| Encerramento | Encerramento por chave, protocolo e localizacao | Com conteudo |
| Flag de encerramento | Campo `encerrado` em `mdves` | Com conteudo |
| Identificacao | Estado, chave, numero MDF-e e protocolo | Parcial |
| Municipios de carregamento | Filhos de municipios de carregamento | Parcial |
| Percursos | Filhos de percursos | Parcial |
| CIOTs | Filhos de CIOTs | Parcial |
| Vale pedagio | Filhos de vale pedagio | Parcial |
| Descargas | Informacoes de descarga, NF-e de descarga e CT-e de descarga | Parcial |
| Modelo fiscal completo | Emissao, autorizacao, veiculo, condutor, carga, seguradora, XML, PDF e eventos | Incompleto |

## 4. Fora de escopo

| Item | Motivo |
|---|---|
| CT-e detalhado | Possui EF especifica concluida. |
| Manifesto DFe de destinatario | Possui EF especifica na fila macro. |
| NF-e de entrada/saida | Possuem EF especificas. |
| Regras completas de emissao MDF-e | Nao informado no material. |
| Encerramentos e eventos alem da flag comprovada | Nao informado no material. |

## 5. Atores e responsabilidades

| Ator | Responsabilidade | Observacao |
|---|---|---|
| Usuario fiscal/logistico | Visualizar, criar, atualizar, excluir, consultar nao encerrados e encerrar MDF-e conforme permissao. | Permissoes especificas finais precisam ser detalhadas. |
| Administrador Siser | Configurar modulo, permissao e parametros fiscais. | Parametros MDF-e finais nao informados. |
| Epros | Controlar estado, chave, numero, protocolo, flag de encerramento e filhos logisticos comprovados. | Modelo fiscal completo esta na MC. |

## 6. Conceitos funcionais

| Conceito | Definicao |
|---|---|
| MDF-e | Manifesto Eletronico de Documentos Fiscais. |
| Nao encerrado | MDF-e que ainda exige encerramento fiscal. |
| Encerrado | MDF-e com flag de encerramento marcada. |
| Chave MDF-e | Chave fiscal do manifesto. |
| Numero MDF-e | Numero operacional/fiscal do manifesto. |
| Protocolo | Identificador fiscal usado no encerramento. |
| Localizacao | Identificador de localizacao usado no encerramento. |
| Municipio de carregamento | Municipio relacionado ao carregamento do manifesto. |
| Percurso | UF/trecho de percurso do manifesto. |
| CIOT | Codigo identificador relacionado a operacao de transporte. |
| Vale pedagio | Informacao de pedagio vinculada ao manifesto. |
| Descarga | Informacao de descarregamento com documentos fiscais relacionados. |

## 7. Capacidades funcionais

| Capacidade | Descricao | Entrada principal | Saida esperada |
|---|---|---|---|
| Controlar permissoes MDF-e | Aplica permissoes com prefixo `mdfe.*`. | Perfil do usuario | Operacao permitida ou bloqueada. |
| Listar MDF-e | Lista MDF-e existentes. | Filtros nao informados | Lista de MDF-e. |
| Consultar nao encerrados | Consulta documentos MDF-e nao encerrados. | Empresa/localizacao/parametros fiscais | Lista de nao encerrados. |
| Encerrar MDF-e | Encerrar por chave, protocolo e localizacao. | Chave, Protocolo, LocalizacaoId | Flag encerrado atualizada quando aceito. |
| Registrar identificacao | Preserva estado, chave, numero e protocolo. | Dados MDF-e | Manifesto identificado. |
| Registrar filhos logisticos | Preserva municipios, percursos, CIOTs, vale pedagio e descargas. | Estruturas filhas | Dados logisticos vinculados. |
| Relacionar NF-e/CT-e em descarga | Preserva NF-e e CT-e de descarga. | Documentos de descarga | Descarga documentada. |

## 8. Regras funcionais

| Regra | Descricao | Contexto | Resultado esperado | Severidade | Fonte funcional |
|---|---|---|---|---|---|
| REG-MDFE-001 | Operacoes MDF-e devem possuir permissoes com prefixo `mdfe.*`. | Permissoes | Bloquear usuario sem permissao aplicavel. | Bloqueante | Material informa permissoes mdfe.* analogas ao CT-e. |
| REG-MDFE-002 | Visualizacao de MDF-e deve exigir permissao funcional de visualizacao. | Consulta/listagem | Permitir ou bloquear visualizacao. | Bloqueante | Inferencia direta do prefixo mdfe.* e analogia CT-e.[^1] |
| REG-MDFE-003 | Criacao de MDF-e deve exigir permissao funcional de criacao. | Criacao | Permitir ou bloquear criacao. | Bloqueante | Inferencia direta do prefixo mdfe.* e analogia CT-e.[^1] |
| REG-MDFE-004 | Atualizacao de MDF-e deve exigir permissao funcional de atualizacao. | Atualizacao | Permitir ou bloquear edicao. | Bloqueante | Inferencia direta do prefixo mdfe.* e analogia CT-e.[^1] |
| REG-MDFE-005 | Exclusao de MDF-e deve exigir permissao funcional de exclusao. | Exclusao | Permitir ou bloquear exclusao. | Bloqueante | Inferencia direta do prefixo mdfe.* e analogia CT-e.[^1] |
| REG-MDFE-006 | Epros deve permitir consulta de MDF-e nao encerrados. | Consulta fiscal | Retornar documentos nao encerrados. | Alta | Operacao comprovada. |
| REG-MDFE-007 | Encerramento de MDF-e deve receber chave, protocolo e localizacao. | Encerramento | Bloquear encerramento incompleto. | Bloqueante | Operacao comprovada. |
| REG-MDFE-008 | Encerramento aceito deve atualizar flag `encerrado`. | Encerramento | Marcar MDF-e como encerrado. | Bloqueante | Flag comprovada. |
| REG-MDFE-009 | MDF-e deve preservar estado. | Identificacao | Registrar estado. | Alta | Campo comprovado. |
| REG-MDFE-010 | MDF-e deve preservar chave fiscal. | Identificacao | Registrar chave. | Alta | Campo comprovado. |
| REG-MDFE-011 | MDF-e deve preservar numero do manifesto. | Identificacao | Registrar MdfeNumero. | Alta | Campo comprovado. |
| REG-MDFE-012 | MDF-e deve preservar protocolo quando informado. | Identificacao/encerramento | Registrar protocolo. | Alta | Campo comprovado e usado no encerramento. |
| REG-MDFE-013 | MDF-e deve possuir indicador booleano de encerramento. | Ciclo de vida | Controlar encerrado sim/nao. | Alta | Campo comprovado. |
| REG-MDFE-014 | MDF-e deve suportar municipios de carregamento. | Logistica | Manter filhos de carregamento. | Media | Filhos comprovados. |
| REG-MDFE-015 | MDF-e deve suportar percursos. | Logistica | Manter filhos de percurso. | Media | Filhos comprovados. |
| REG-MDFE-016 | MDF-e deve suportar CIOTs. | Logistica | Manter filhos de CIOT. | Media | Filhos comprovados. |
| REG-MDFE-017 | MDF-e deve suportar vale pedagio. | Logistica | Manter filhos de vale pedagio. | Media | Filhos comprovados. |
| REG-MDFE-018 | MDF-e deve suportar informacoes de descarga. | Descarga | Manter filhos de descarga. | Media | Filhos comprovados. |
| REG-MDFE-019 | MDF-e deve suportar NF-e de descarga. | Descarga | Relacionar NF-e descarregada. | Media | Filhos comprovados. |
| REG-MDFE-020 | MDF-e deve suportar CT-e de descarga. | Descarga | Relacionar CT-e descarregado. | Media | Filhos comprovados. |
| REG-MDFE-021 | A EF nao deve assumir campos completos de emissao, autorizacao, veiculo, condutor, carga ou eventos quando nao informados. | Especificacao | Encaminhar para MC. | Bloqueante | Material parcial. |

## 9. Estados e situacoes

| Situacao | Descricao | Regra |
|---|---|---|
| Nao encerrado | Documento ainda aparece na consulta de nao encerrados. | Consulta comprovada. |
| Encerrado | Flag `encerrado` marcada apos encerramento aceito. | Campo comprovado. |
| Estado fiscal | Estado textual/funcional do MDF-e. | Dominio final nao informado no material. |

## 10. Modelo de dados funcional e implantavel

O material comprova a entidade funcional `mdves` com estado, chave, numero MDF-e, encerrado e protocolo, alem de filhos de municipios de carregamento, percursos, CIOTs, vale pedagio, informacoes de descarga, NF-e de descarga e CT-e de descarga. O material nao informa colunas completas, chaves fisicas, tipos, obrigatoriedades finais, XML/PDF, protocolo de autorizacao completo ou payload de emissao.[^1]

| Entidade funcional | Finalidade | Cardinalidade | Persistencia indicada |
|---|---|---|---|
| mdves | Controlar MDF-e, chave, numero, protocolo e encerramento. | 1 por MDF-e | Comprovada parcialmente. |
| mdve_municipios_carregamento | Municipios de carregamento do MDF-e. | 0..N por MDF-e | Comprovada parcialmente. |
| mdve_percursos | Percursos do MDF-e. | 0..N por MDF-e | Comprovada parcialmente. |
| mdve_ciots | CIOTs do MDF-e. | 0..N por MDF-e | Comprovada parcialmente. |
| mdve_vale_pedagios | Vale pedagio do MDF-e. | 0..N por MDF-e | Comprovada parcialmente. |
| mdve_info_descargas | Informacoes de descarga. | 0..N por MDF-e | Comprovada parcialmente. |
| mdve_nfe_descargas | NF-e vinculada a descarga. | 0..N por descarga | Comprovada parcialmente. |
| mdve_cte_descargas | CT-e vinculado a descarga. | 0..N por descarga | Comprovada parcialmente. |
| mdfe_consulta_nao_encerrados | Registro funcional de consulta de nao encerrados. | 0..N por consulta | Consolidacao funcional da operacao comprovada.[^1] |
| mdfe_encerramento | Registro funcional de encerramento. | 0..N por encerramento | Consolidacao funcional da operacao comprovada.[^1] |

### 10.1 Relacionamentos funcionais

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| mdves | possui | mdve_municipios_carregamento | MDF-e pode possuir municipios de carregamento. |
| mdves | possui | mdve_percursos | MDF-e pode possuir percursos. |
| mdves | possui | mdve_ciots | MDF-e pode possuir CIOTs. |
| mdves | possui | mdve_vale_pedagios | MDF-e pode possuir vales pedagio. |
| mdves | possui | mdve_info_descargas | MDF-e pode possuir descargas. |
| mdve_info_descargas | possui | mdve_nfe_descargas | Descarga pode possuir NF-e. |
| mdve_info_descargas | possui | mdve_cte_descargas | Descarga pode possuir CT-e. |
| mdves | pode gerar | mdfe_encerramento | Encerramento atualiza flag encerrado. |

## 11. Dicionario de dados implantavel

### 11.1 mdves

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| Estado | Enum/texto | Nao informado no material | Nao informado no material | Estado fiscal | Dominio final nao informado. |
| Chave | Texto | Nao informado no material | Nao informado no material | Chave MDF-e | Chave fiscal do manifesto. |
| MdfeNumero | Texto/numero | Nao informado no material | Nao informado no material | Numero MDF-e | Numero do manifesto. |
| Encerrado | Booleano | Sim/Nao | Sim | Encerramento | Flag comprovada. |
| Protocolo | Texto | Nao informado no material | Nao informado no material | Protocolo fiscal | Usado no encerramento. |
| LocalizacaoId | Identificador | Nao informado no material | Nao informado no material | Localizacao | Usado na operacao de encerramento; campo final nao informado.[^1] |
| XmlCaminho | Texto | Nao informado no material | Nao informado no material | XML | Nao informado no material. |
| PdfCaminho | Texto | Nao informado no material | Nao informado no material | Documento auxiliar | Nao informado no material. |

### 11.2 mdve_municipios_carregamento

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| MdfeId | Identificador | Nao informado no material | Sim | Relacao com mdves | Vinculo com MDF-e.[^1] |
| Municipio | Texto/identificador | Nao informado no material | Nao informado no material | Municipio | Campo final nao informado. |

### 11.3 mdve_percursos

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| MdfeId | Identificador | Nao informado no material | Sim | Relacao com mdves | Vinculo com MDF-e.[^1] |
| Percurso | Texto/identificador | Nao informado no material | Nao informado no material | Percurso | Campo final nao informado. |

### 11.4 mdve_ciots

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| MdfeId | Identificador | Nao informado no material | Sim | Relacao com mdves | Vinculo com MDF-e.[^1] |
| Ciot | Texto | Nao informado no material | Nao informado no material | CIOT | Campo final nao informado. |

### 11.5 mdve_vale_pedagios

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| MdfeId | Identificador | Nao informado no material | Sim | Relacao com mdves | Vinculo com MDF-e.[^1] |
| ValePedagio | Texto/estrutura | Nao informado no material | Nao informado no material | Vale pedagio | Campo final nao informado. |

### 11.6 mdve_info_descargas

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| MdfeId | Identificador | Nao informado no material | Sim | Relacao com mdves | Vinculo com MDF-e.[^1] |
| InfoDescarga | Texto/estrutura | Nao informado no material | Nao informado no material | Descarga | Campo final nao informado. |

### 11.7 mdve_nfe_descargas

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| InfoDescargaId | Identificador | Nao informado no material | Sim | Relacao com descarga | Vinculo com descarga.[^1] |
| ChaveNfe | Texto | Nao informado no material | Nao informado no material | NF-e | Campo final nao informado. |

### 11.8 mdve_cte_descargas

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| InfoDescargaId | Identificador | Nao informado no material | Sim | Relacao com descarga | Vinculo com descarga.[^1] |
| ChaveCte | Texto | Nao informado no material | Nao informado no material | CT-e | Campo final nao informado. |

### 11.9 mdfe_consulta_nao_encerrados

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| DataConsulta | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Necessario para rastreio; estrutura final nao informada.[^1] |
| ResultadoConsulta | Texto/estrutura | Nao informado no material | Nao informado no material | Resultado | Lista de nao encerrados; formato final nao informado.[^1] |

### 11.10 mdfe_encerramento

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| MdfeId | Identificador | Nao informado no material | Nao informado no material | Relacao com mdves | Vinculo final nao informado.[^1] |
| Chave | Texto | Nao informado no material | Sim | Chave MDF-e | Obrigatoria para encerramento. |
| Protocolo | Texto | Nao informado no material | Sim | Protocolo fiscal | Obrigatorio para encerramento. |
| LocalizacaoId | Identificador | Nao informado no material | Sim | Localizacao | Obrigatoria para encerramento. |
| ResultadoEncerramento | Texto/estrutura | Nao informado no material | Nao informado no material | Retorno | Retorno final nao informado.[^1] |
| DataEncerramento | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Estrutura final nao informada.[^1] |

## 12. Fluxos funcionais

### 12.1 Consultar MDF-e nao encerrados

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal/logistico | Solicita consulta de nao encerrados. | Empresa/localizacao/parametros fiscais | Consulta iniciada. |
| 2 | Epros | Verifica permissao MDF-e aplicavel. | Perfil do usuario | Consulta permitida ou bloqueada. |
| 3 | Epros | Consulta documentos nao encerrados. | Parametros fiscais | Lista de nao encerrados. |
| 4 | Epros | Registra resultado funcional. | Resultado | Historico de consulta quando definido.[^1] |

### 12.2 Encerrar MDF-e

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal/logistico | Solicita encerramento. | Chave, Protocolo, LocalizacaoId | Encerramento em preparacao. |
| 2 | Epros | Valida campos obrigatorios. | Chave, Protocolo, LocalizacaoId | Bloqueio ou envio. |
| 3 | Epros | Executa encerramento. | Dados validados | Retorno fiscal. |
| 4 | Epros | Atualiza flag quando aceito. | Retorno aceito | `Encerrado` marcado como Sim. |

### 12.3 Registrar dados logisticos

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario/Epros | Informa ou importa dados do manifesto. | Dados disponiveis | MDF-e em edicao. |
| 2 | Epros | Registra municipios de carregamento e percursos. | Filhos logisticos | Dados vinculados. |
| 3 | Epros | Registra CIOTs e vale pedagio. | Filhos logisticos | Dados vinculados. |
| 4 | Epros | Registra descargas, NF-e e CT-e de descarga. | Informacoes de descarga | Descargas vinculadas. |

## 13. Validacoes e mensagens

| Codigo | Mensagem | Condicao |
|---|---|---|
| MSG-MDFE-001 | Usuario sem permissao MDF-e. | Operacao sem permissao aplicavel. |
| MSG-MDFE-002 | Chave do MDF-e e obrigatoria para encerramento. | Encerramento sem chave. |
| MSG-MDFE-003 | Protocolo e obrigatorio para encerramento. | Encerramento sem protocolo. |
| MSG-MDFE-004 | Localizacao e obrigatoria para encerramento. | Encerramento sem localizacao. |
| MSG-MDFE-005 | MDF-e ja encerrado. | Nova tentativa de encerramento quando flag ja estiver marcada. |
| MSG-MDFE-006 | Consulta de nao encerrados sem configuracao fiscal suficiente. | Parametros fiscais ausentes. |

## 14. Integracoes

| Integracao | Direcao | Dados | Regra | Lacuna |
|---|---|---|---|---|
| CT-e | Entrada | CT-e de descarga | MDF-e pode conter CT-e em descarga. | Relacao final com EF CT-e. |
| NF-e | Entrada | NF-e de descarga | MDF-e pode conter NF-e em descarga. | Validacao de chave/documento. |
| Cadastros Base | Entrada | Municipios, localizacao, participantes | Dados mestres nao devem ser duplicados. | Campos finais nao informados. |
| Parametros fiscais | Entrada | Ambiente, certificado, comunicacao fiscal | Necessario para consulta/encerramento. | Parametros MDF-e nao informados. |
| Logistica/Estoque | Entrada/Saida | Percursos, carga, descarga | Material cita filhos logisticos. | Contrato operacional nao informado. |

## 15. Permissoes e seguranca

| Controle | Regra |
|---|---|
| Permissoes MDF-e | O material informa prefixo `mdfe.*` analogo ao CT-e; a matriz final de permissao fica na MC. |
| Encerramento | Deve exigir usuario autorizado e dados obrigatorios. |
| Auditoria | Necessaria para consulta e encerramento; estrutura final nao informada. |
| Tenant/empresa | Nao informado no material para MDF-e. |

## 16. Relatorios e consultas

| Consulta | Filtros comprovados | Resultado |
|---|---|---|
| Lista MDF-e | Nao informado no material | MDF-e com estado, chave, numero, protocolo e encerrado quando disponiveis. |
| Nao encerrados | Nao informado no material | MDF-e pendentes de encerramento. |
| Encerramento | Chave, protocolo e localizacao | Resultado do encerramento e flag atualizada. |

## 17. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-MDFE-001 | Deve existir controle de permissao para operacoes MDF-e. |
| CA-MDFE-002 | Deve ser possivel consultar MDF-e nao encerrados. |
| CA-MDFE-003 | Encerramento sem chave deve ser bloqueado. |
| CA-MDFE-004 | Encerramento sem protocolo deve ser bloqueado. |
| CA-MDFE-005 | Encerramento sem localizacao deve ser bloqueado. |
| CA-MDFE-006 | Encerramento aceito deve marcar `Encerrado` como Sim. |
| CA-MDFE-007 | MDF-e deve preservar estado, chave, numero, protocolo e flag encerrado quando informados. |
| CA-MDFE-008 | MDF-e deve suportar municipios de carregamento, percursos, CIOTs, vale pedagio e descargas como estruturas filhas. |
| CA-MDFE-009 | Descargas devem suportar NF-e e CT-e relacionados quando informados. |
| CA-MDFE-010 | Campos nao informados no material nao devem ser preenchidos por suposicao na EF. |

## 18. Lacunas encaminhadas para MC

| Lacuna | Impacto |
|---|---|
| Modelo completo MDF-e | Necessario para emissao/autorizacao completa. |
| Dominio de estados | Necessario para ciclo de vida. |
| Campos completos de veiculo, condutor, carga, seguradora, vale pedagio, CIOT e percurso | Necessario para documento fiscal completo. |
| XML de envio, XML autorizado, PDF/DAMDFE, protocolo completo e armazenamento | Necessario para evidencia fiscal. |
| Eventos alem do encerramento | Necessario para operacao fiscal completa. |
| Integracao com CT-e/NF-e/logistica | Necessario para cadeia de transporte. |
| Seguranca, auditoria e segregacao por tenant/empresa | Necessario para operacao segura. |

## 19. Proximo passo

O proximo documento especifico da fila macro e `EF_MANIFESTO_DFE`, detalhando Manifesto DFe conforme material disponivel.

[^1]: Consolidacao funcional criada para tornar implantavel a especificacao, pois o material comprova `mdves`, consulta de nao encerrados, encerramento e estruturas filhas, mas nao informa tabela final completa, colunas dos filhos, chaves fisicas, auditoria, XML/PDF, retorno fiscal completo ou protocolo de autorizacao para MDF-e.
