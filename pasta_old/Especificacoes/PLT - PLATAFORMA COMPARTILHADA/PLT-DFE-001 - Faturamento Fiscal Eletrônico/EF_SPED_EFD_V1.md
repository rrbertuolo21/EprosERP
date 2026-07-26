# EF_SPED_EFD_V1

## 1. Identificacao

| Item | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | Plataforma Compartilhada |
| Submodulo | Faturamento Fiscal Eletronico |
| Documento | Especificacao funcional - SPED/EFD |
| Versao | V1 |
| Status | Concluido |
| Nivel de completude | Parcial, conforme material disponivel |

## 2. Objetivo funcional

O recurso SPED/EFD do Epros deve apoiar a geracao de arquivos fiscais periodicos a partir de documentos, cadastros fiscais, entradas, saidas, apuracoes e registros fiscais do periodo. O material comprova existencia de escopo para EFD ICMS/IPI, EFD Contribuicoes, preview de arquivo, registros EFD Contribuicoes e fronteira com relatorios/obrigacoes fiscais.

Esta EF organiza o conteudo existente sem completar layouts oficiais, registros, blocos, validacoes ou formulas que nao estejam no material. O documento deve ser validado como base parcial para evolucao funcional.

## 3. Escopo comprovado

| Area | Conteudo comprovado |
|---|---|
| EFD ICMS/IPI | Geracao fiscal citada para EFD ICMS/IPI. |
| EFD Contribuicoes | Estrutura citada com 41 registros no material. |
| Preview | Existe capacidade de preview de arquivo. |
| Dados de origem | Documentos fiscais, entradas, saidas, cadastros fiscais, apuracao ICMS e informacoes fiscais periodicas. |
| Entidades funcionais | Material cita 40 estruturas de dados/visoes funcionais relacionadas ao SPED. |
| Regras | Material cita 50 regras e anexo com 41 registros de EFD Contribuicoes. |
| Telas/operacao | Material cita 8 telas/visoes operacionais para SPED. |
| Fronteira | Emissao/transmissao de documentos fiscais permanece nas EFs de documentos fiscais; SPED/EFD e obrigacao fiscal periodica. |

## 4. Fora de escopo por falta de material suficiente

| Item | Motivo |
|---|---|
| Layout completo de cada arquivo | Nao informado no material. |
| Lista completa de blocos e registros | Apenas quantidade de registros EFD Contribuicoes foi informada, sem dicionario completo. |
| Regras oficiais por registro | Nao informado no material. |
| Validador oficial e mensagens | Nao informado no material. |
| Assinatura, transmissao ou entrega | Nao informado no material para esta EF. |
| ECD | Citado como codigo presente, mas sem especificacao funcional suficiente neste recorte. |
| Apuracao completa | Existem referencias a apuracao ICMS manual, mas campos e formulas nao estao completos. |

## 5. Papeis

| Papel | Responsabilidade |
|---|---|
| Usuario fiscal | Selecionar periodo, empresa e tipo de arquivo fiscal a gerar. |
| Usuario contabil | Validar arquivo, preview e dados fiscais antes do uso externo. |
| Gestor fiscal | Parametrizar dados fiscais necessarios e acompanhar lacunas de completude. |
| Epros | Reunir dados do periodo, montar arquivo quando houver regra suficiente, gerar preview e registrar resultado funcional. |

## 6. Entradas

| Entrada | Obrigatorio | Regra |
|---|---|---|
| Empresa | Sim | Empresa define documentos, cadastros e apuracoes do periodo. |
| Periodo fiscal | Sim | Periodo usado para selecionar documentos e movimentos. |
| Tipo de obrigacao | Sim | EFD ICMS/IPI ou EFD Contribuicoes quando suportado. |
| Documentos fiscais | Condicional | Fonte para entradas, saidas e apuracoes. |
| Cadastros fiscais | Condicional | CFOP, NCM, CST, CSOSN, aliquotas e demais cadastros usados nos registros. |
| Apuracao ICMS | Condicional | Material cita apuracao ICMS manual. |
| Tabela Simples Nacional | Condicional | Material cita tabela Simples Nacional. |
| Livros e termos fiscais | Condicional | Material cita livros/termos fiscais. |

## 7. Saidas

| Saida | Conteudo |
|---|---|
| Arquivo fiscal periodico | Arquivo SPED/EFD gerado quando houver dados e regra suficiente. |
| Preview de arquivo | Visualizacao previa do arquivo fiscal. |
| Resultado de geracao | Status, mensagens e dados do arquivo gerado. |
| Pendencias de geracao | Lista de lacunas de dados ou regras que impedem arquivo final. |

## 8. Regras funcionais

| Codigo | Regra |
|---|---|
| SPED-001 | A geracao SPED/EFD deve ser por empresa e periodo fiscal. |
| SPED-002 | O Epros deve separar EFD ICMS/IPI e EFD Contribuicoes como tipos de obrigacao fiscal. |
| SPED-003 | O Epros deve permitir preview do arquivo fiscal quando houver arquivo montado. |
| SPED-004 | O Epros deve consumir documentos fiscais de entrada e saida do periodo quando usados na geracao. |
| SPED-005 | O Epros deve consumir cadastros fiscais do periodo quando usados na geracao. |
| SPED-006 | O Epros deve tratar apuracao ICMS como entrada funcional quando aplicavel. |
| SPED-007 | O Epros deve tratar livros e termos fiscais como entradas funcionais quando aplicavel. |
| SPED-008 | O Epros deve tratar tabela Simples Nacional como entrada funcional quando aplicavel. |
| SPED-009 | EFD Contribuicoes possui 41 registros citados no material, mas o dicionario desses registros nao esta informado. |
| SPED-010 | O Epros nao deve gerar registro sem layout, campos e regra de preenchimento definidos. |
| SPED-011 | Se faltar dado obrigatorio para o arquivo fiscal, a geracao deve retornar pendencia funcional. |
| SPED-012 | Se faltar regra de registro/bloco, a geracao deve ficar bloqueada para arquivo definitivo. |
| SPED-013 | A emissao e transmissao de NF-e, NFC-e, NFS-e, CT-e, MDF-e e eventos nao pertencem ao SPED/EFD; elas alimentam dados fiscais do periodo. |
| SPED-014 | SPED/EFD deve ser tratado como obrigacao fiscal periodica e nao como evento de autorizacao de documento eletronico. |
| SPED-015 | O material nao informa assinatura, transmissao ou protocolo de entrega para SPED/EFD; essas capacidades ficam pendentes na MC. |

## 9. Fluxo funcional

| Passo | Ator | Acao | Entrada | Validacao | Saida |
|---|---|---|---|---|---|
| 1 | Usuario fiscal | Seleciona empresa, periodo e tipo de obrigacao. | Empresa, periodo, tipo. | Empresa e periodo informados. | Solicitacao criada. |
| 2 | Epros | Levanta dados fiscais do periodo. | Documentos, entradas, saidas, cadastros e apuracoes. | Existencia de dados. | Base de geracao preparada. |
| 3 | Epros | Verifica regras de registros. | Tipo de obrigacao e registros disponiveis. | Layout/regra existente. | Pode gerar ou retorna pendencia. |
| 4 | Epros | Monta arquivo quando houver regra suficiente. | Dados fiscais e regras. | Consistencia minima. | Arquivo fiscal ou erro funcional. |
| 5 | Usuario fiscal | Visualiza preview. | Arquivo montado. | Arquivo disponivel. | Preview apresentado. |
| 6 | Usuario fiscal/contabil | Valida resultado. | Preview, mensagens e pendencias. | Validacao humana. | Arquivo aceito ou ajustes pendentes. |

## 10. Modelo de dados funcional e implantavel

O material nao informa tabelas finais do Epros para SPED/EFD. As entidades abaixo sao estruturas funcionais minimas para organizar a obrigacao e registrar pendencias, sem afirmar persistencia fisica definitiva.[^nota1]

### 10.1 Entidades funcionais

| Entidade funcional | Finalidade | Cardinalidade | Observacao |
|---|---|---|---|
| `sped_efd_geracao` | Controlar uma solicitacao de geracao por empresa, periodo e tipo. | 1 por solicitacao | Estrutura funcional criada para organizar o processo. |
| `sped_efd_fonte_dados` | Registrar fontes usadas na geracao. | 0..N por geracao | Documentos, entradas, saidas, apuracao, cadastros e livros. |
| `sped_efd_registro` | Representar registros/blocos que compoem arquivo fiscal. | 0..N por geracao | Layout completo nao informado no material. |
| `sped_efd_arquivo` | Representar arquivo gerado ou preview. | 0..N por geracao | Armazena status funcional do arquivo. |
| `sped_efd_pendencia` | Registrar ausencia de dado, layout ou regra. | 0..N por geracao | Usado para bloquear geracao definitiva quando faltar material. |

### 10.2 Relacionamentos

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| Empresa | possui | `sped_efd_geracao` | Cada geracao pertence a uma empresa. |
| `sped_efd_geracao` | possui | `sped_efd_fonte_dados` | Fontes de dados usadas na montagem. |
| `sped_efd_geracao` | possui | `sped_efd_registro` | Registros/blocos gerados ou pendentes. |
| `sped_efd_geracao` | possui | `sped_efd_arquivo` | Arquivo final ou preview. |
| `sped_efd_geracao` | possui | `sped_efd_pendencia` | Pendencias de completude e geracao. |

## 11. Dicionario de dados implantavel

### 11.1 `sped_efd_geracao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da geracao. |
| EmpresaId | Identificador | Nao informado no material | Sim | Empresa | Empresa da obrigacao fiscal. |
| PeriodoInicio | Data | Nao informado no material | Sim | Periodo | Inicio do periodo fiscal. |
| PeriodoFim | Data | Nao informado no material | Sim | Periodo | Fim do periodo fiscal. |
| TipoObrigacao | Enum/texto | EFD_ICMS_IPI, EFD_CONTRIBUICOES | Sim | Tipo | Tipos comprovados no material. |
| StatusGeracao | Enum/texto | Nao informado no material | Sim | Status | Status final nao informado. |
| DataSolicitacao | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data da solicitacao. |
| UsuarioSolicitanteId | Identificador | Nao informado no material | Nao informado no material | Usuario | Usuario que solicitou. |

### 11.2 `sped_efd_fonte_dados`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da fonte. |
| GeracaoId | Identificador | Nao informado no material | Sim | Geracao | Geracao SPED/EFD. |
| TipoFonte | Enum/texto | DocumentoFiscal, Entrada, Saida, CadastroFiscal, ApuracaoICMS, LivroFiscal, TermoFiscal, SimplesNacional | Sim | Tipo | Fontes comprovadas no material. |
| QuantidadeRegistros | Numero | Nao informado no material | Nao informado no material | Controle | Quantidade usada quando disponivel. |
| Observacao | Texto | Nao informado no material | Nao | Observacao | Detalhe da fonte. |

### 11.3 `sped_efd_registro`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do registro. |
| GeracaoId | Identificador | Nao informado no material | Sim | Geracao | Geracao SPED/EFD. |
| TipoObrigacao | Enum/texto | EFD_ICMS_IPI, EFD_CONTRIBUICOES | Sim | Tipo | Tipo da obrigacao. |
| Bloco | Texto | Nao informado no material | Nao informado no material | Layout | Bloco nao detalhado no material. |
| CodigoRegistro | Texto | Nao informado no material | Nao informado no material | Layout | Codigo do registro nao detalhado. |
| Sequencia | Numero | Nao informado no material | Nao informado no material | Ordem | Ordem do registro no arquivo. |
| ConteudoLinha | Texto | Nao informado no material | Condicional | Arquivo | Conteudo final nao detalhado. |
| StatusRegistro | Enum/texto | Gerado, Pendente ou Nao informado no material | Sim | Status | Consolidado funcional para controle.[^nota1] |
| Mensagem | Texto | Nao informado no material | Nao | Mensagem | Erro/pendencia do registro. |

### 11.4 `sped_efd_arquivo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do arquivo. |
| GeracaoId | Identificador | Nao informado no material | Sim | Geracao | Geracao SPED/EFD. |
| TipoArquivo | Enum/texto | Arquivo, Preview | Sim | Tipo | Preview comprovado no material. |
| NomeArquivo | Texto | Nao informado no material | Nao informado no material | Arquivo | Nome nao informado. |
| CaminhoArquivo | Texto | Nao informado no material | Nao informado no material | Arquivo | Caminho nao informado. |
| Conteudo | Texto/arquivo | Nao informado no material | Condicional | Arquivo | Conteudo nao detalhado. |
| StatusArquivo | Enum/texto | Nao informado no material | Sim | Status | Status final nao informado. |
| DataGeracao | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data da geracao. |

### 11.5 `sped_efd_pendencia`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da pendencia. |
| GeracaoId | Identificador | Nao informado no material | Sim | Geracao | Geracao SPED/EFD. |
| TipoPendencia | Enum/texto | DadoAusente, LayoutAusente, RegraAusente, ValidacaoPendente | Sim | Tipo | Consolidado funcional para MC.[^nota1] |
| Referencia | Texto | Nao informado no material | Nao | Referencia | Registro, fonte ou campo afetado. |
| Mensagem | Texto | Nao informado no material | Sim | Mensagem | Descricao da pendencia. |
| BloqueiaArquivoDefinitivo | Booleano | Nao informado no material | Sim | Bloqueio | Deve bloquear arquivo definitivo quando faltar layout/regra. |

## 12. Integracoes funcionais

| Integracao | Dados | Regra |
|---|---|---|
| Documentos fiscais | NF-e, NFC-e, eventos e documentos fiscais do periodo | Alimentam registros fiscais quando layouts forem definidos. |
| Compras/entradas | Entradas fiscais do periodo | Alimentam arquivos periodicos quando aplicavel. |
| Vendas/saidas | Saidas fiscais do periodo | Alimentam arquivos periodicos quando aplicavel. |
| Cadastros fiscais | CFOP, NCM, CST, CSOSN, aliquotas e beneficios | Usados na composicao dos registros. |
| Motor tributario | Bases, valores e impostos calculados | Fonte para registros de apuracao quando aplicavel. |
| Relatorios/Contabilidade | Arquivo, preview e pendencias | Consumidores provaveis da obrigacao fiscal. |

## 13. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-SPED-001 | O Epros deve permitir solicitar geracao por empresa, periodo e tipo de obrigacao. |
| CA-SPED-002 | O Epros deve separar EFD ICMS/IPI de EFD Contribuicoes. |
| CA-SPED-003 | O Epros deve permitir preview quando houver arquivo montado. |
| CA-SPED-004 | O Epros deve registrar pendencia quando faltar dado obrigatorio. |
| CA-SPED-005 | O Epros deve registrar pendencia quando faltar layout ou regra de registro. |
| CA-SPED-006 | O Epros nao deve considerar arquivo definitivo quando faltarem registros/blocos nao definidos. |
| CA-SPED-007 | EFD Contribuicoes deve ser tratada como conteudo parcial ate que os 41 registros citados tenham dicionario completo. |

## 14. Pontos pendentes para validacao

| Ponto | Impacto |
|---|---|
| Layout completo EFD ICMS/IPI | Necessario para arquivo implantavel. |
| Layout completo EFD Contribuicoes | Necessario para arquivo implantavel. |
| Lista completa dos 41 registros EFD Contribuicoes | Material informa quantidade, nao o dicionario completo. |
| Campos das 40 estruturas funcionais citadas | Material informa quantidade, nao campos completos. |
| Regras das 50 regras citadas | Material informa quantidade, nao conteudo completo. |
| Assinatura, validacao oficial, transmissao e protocolo | Nao informado no material. |
| Fronteira final com Relatorios/Contabilidade | Precisa decisao de arquitetura do Epros. |

## 15. Notas de autoria

[^nota1]: As entidades funcionais de SPED/EFD foram criadas para organizar o processo e explicitar lacunas, porque o material comprova capacidade, fontes, preview e volume de registros, mas nao informa tabelas finais nem layout completo implantavel.
