# EF_SINTEGRA_V1

## 1. Identificacao

| Item | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | Plataforma Compartilhada |
| Submodulo | Faturamento Fiscal Eletronico |
| Documento | Especificacao funcional - Sintegra |
| Versao | V1 |
| Status | Concluido |
| Nivel de completude | Parcial, conforme material disponivel |

## 2. Objetivo funcional

O recurso Sintegra do Epros deve permitir a geracao de arquivo fiscal mensal por empresa, usando dados fiscais, cadastrais e operacionais disponiveis no periodo. A geracao deve validar a empresa ativa, validar prerequisitos cadastrais, montar registros fiscais obrigatorios informados no material e produzir arquivo texto em codificacao ANSI 1252, com linhas de tamanho fixo de 126 caracteres quando geradas.

Esta EF organiza o conteudo comprovado sem completar layout oficial, campos de todos os registros, formulas, validacoes legais ou relatorios que nao estejam informados no material. O documento deve ser validado como base parcial-controlada para implantacao.

## 3. Escopo comprovado

| Area | Conteudo comprovado |
|---|---|
| Periodicidade | Geracao por periodo mensal. |
| Empresa | Sintegra depende da empresa ativa. |
| Prerequisitos | Sintegra valida prerequisitos cadastrais da empresa. |
| Registros fiscais | Material informa geracao de registros fiscais obrigatorios. |
| Tamanho de linha | Cada linha do Sintegra possui tamanho fixo de 126 caracteres. |
| Arquivo | Saida em arquivo texto ANSI 1252. |
| Registro 70 | Registro 70 reservado para transporte. |
| Inventario | Inventario opcional no Sintegra. |
| Registros citados | Material cita Reg10 a Reg90, registros 60/61, registros 51 a 61 como pendentes de detalhamento, e NFe como conteudo relacionado. |
| Operacao | Material cita tela/operacao de Sintegra e geracao como exportacao. |

## 4. Fora de escopo por falta de material suficiente

| Item | Motivo |
|---|---|
| Layout completo de todos os registros | Nao informado no material. |
| Dicionario campo a campo dos registros | Nao informado no material. |
| Regras oficiais de preenchimento por registro | Nao informado no material. |
| Formula de totalizacao por registro | Nao informado no material. |
| Nome final do arquivo | Nao informado no material. |
| Validacao externa/oficial | Nao informado no material. |
| Entrega, protocolo ou recibo | Nao informado no material. |
| Permissoes finais | Nao informado no material. |
| Retencao e armazenamento final | Nao informado no material. |

## 5. Papeis

| Papel | Responsabilidade |
|---|---|
| Usuario fiscal | Selecionar empresa ativa e periodo mensal para gerar o arquivo Sintegra. |
| Usuario contabil | Validar o arquivo gerado, pendencias e conteudo fiscal antes do uso externo. |
| Gestor fiscal | Manter dados cadastrais e fiscais necessarios para a geracao. |
| Epros | Validar prerequisitos, montar registros disponiveis, gerar arquivo texto e registrar pendencias quando faltar dado ou regra. |

## 6. Entradas

| Entrada | Obrigatorio | Regra |
|---|---|---|
| Empresa ativa | Sim | A geracao depende de empresa ativa. |
| Periodo mensal | Sim | A geracao e mensal. |
| Dados cadastrais da empresa | Sim | Devem ser validados antes da geracao. |
| Documentos fiscais do periodo | Condicional | Fonte para registros fiscais quando aplicavel. |
| Pessoas vinculadas aos documentos | Condicional | Usadas nos registros quando exigidas. |
| Itens fiscais | Condicional | Usados nos registros quando exigidos. |
| Transporte | Condicional | Registro 70 e reservado para transporte. |
| Inventario | Nao | Inventario e opcional no Sintegra. |

## 7. Saidas

| Saida | Conteudo |
|---|---|
| Arquivo Sintegra | Arquivo texto ANSI 1252 com registros e linhas de 126 caracteres quando geradas. |
| Registros gerados | Registros fiscais obrigatorios conforme material e dados disponiveis. |
| Pendencias | Falta de prerequisito cadastral, dado fiscal, regra de registro ou linha fora do tamanho esperado. |
| Resultado de geracao | Status funcional da solicitacao, mensagens e referencia do arquivo quando houver geracao. |

## 8. Regras funcionais

| Codigo | Regra |
|---|---|
| SIN-001 | A geracao Sintegra deve ser solicitada por empresa ativa. |
| SIN-002 | A geracao Sintegra deve ser por periodo mensal. |
| SIN-003 | Antes de gerar o arquivo, o Epros deve validar os prerequisitos cadastrais da empresa. |
| SIN-004 | Se faltar prerequisito cadastral obrigatorio da empresa, a geracao deve retornar pendencia funcional. |
| SIN-005 | O Epros deve gerar registros fiscais obrigatorios quando houver layout, regra e dados suficientes. |
| SIN-006 | Cada linha gerada para o Sintegra deve possuir 126 caracteres. |
| SIN-007 | Linha com tamanho diferente de 126 caracteres deve bloquear arquivo definitivo ou gerar pendencia funcional. |
| SIN-008 | O arquivo Sintegra deve ser texto em codificacao ANSI 1252. |
| SIN-009 | O Registro 70 deve ser tratado como registro reservado para transporte. |
| SIN-010 | O inventario deve ser tratado como opcional no Sintegra. |
| SIN-011 | O material cita Reg10 a Reg90 e NFe como conteudo relacionado; o Epros nao deve completar campos desses registros sem layout validado. |
| SIN-012 | O material cita registros 60/61 e registros 51 a 61 como pendentes de detalhamento; o Epros deve registra-los como lacuna enquanto nao houver dicionario. |
| SIN-013 | A geracao Sintegra deve ser tratada como exportacao fiscal periodica, separada da autorizacao de documentos fiscais eletronicos. |
| SIN-014 | Se faltar dado de origem ou regra de registro, a geracao deve informar pendencia em vez de criar linha presumida. |
| SIN-015 | O material nao informa entrega, protocolo, assinatura, validacao oficial, retencao ou permissao final; essas capacidades ficam pendentes na MC. |

## 9. Fluxo funcional

| Passo | Ator | Acao | Entrada | Validacao | Saida |
|---|---|---|---|---|---|
| 1 | Usuario fiscal | Seleciona empresa ativa e mes/ano. | Empresa, mes, ano. | Empresa ativa e periodo mensal informados. | Solicitacao criada. |
| 2 | Epros | Valida prerequisitos cadastrais da empresa. | Dados cadastrais. | Prerequisitos presentes. | Geracao liberada ou pendencia. |
| 3 | Epros | Levanta dados fiscais do periodo. | Documentos, pessoas, itens, transporte e inventario quando aplicavel. | Existencia de dados. | Base de registros preparada. |
| 4 | Epros | Monta registros disponiveis. | Base de dados e regras existentes. | Layout/regra informado no material. | Registros gerados ou pendentes. |
| 5 | Epros | Valida tamanho de linha. | Linhas montadas. | 126 caracteres por linha. | Arquivo valido ou pendencia. |
| 6 | Epros | Gera arquivo texto. | Registros validados. | Codificacao ANSI 1252. | Arquivo Sintegra gerado. |
| 7 | Usuario fiscal/contabil | Valida resultado. | Arquivo e pendencias. | Validacao humana. | Arquivo aceito ou ajustes pendentes. |

## 10. Modelo de dados funcional e implantavel

O material nao informa tabelas finais do Epros para Sintegra. As entidades abaixo sao estruturas funcionais minimas para controlar a geracao, o arquivo, os registros e as pendencias, sem afirmar persistencia fisica definitiva.[^nota1]

### 10.1 Entidades funcionais

| Entidade funcional | Finalidade | Cardinalidade | Observacao |
|---|---|---|---|
| `sintegra_geracao` | Controlar uma solicitacao mensal por empresa. | 1 por solicitacao | Estrutura funcional criada para organizar a geracao. |
| `sintegra_fonte_dados` | Registrar dados usados na geracao. | 0..N por geracao | Documentos fiscais, pessoas, itens, transporte e inventario. |
| `sintegra_registro` | Representar registros fiscais montados ou pendentes. | 0..N por geracao | Layout completo nao informado no material. |
| `sintegra_arquivo` | Representar arquivo texto gerado. | 0..N por geracao | Codificacao ANSI 1252 e linhas de 126 caracteres quando geradas. |
| `sintegra_pendencia` | Registrar ausencia de dado, prerequisito, layout ou regra. | 0..N por geracao | Usado para impedir geracao presumida. |

### 10.2 Relacionamentos

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| Empresa | possui | `sintegra_geracao` | Cada geracao pertence a uma empresa ativa. |
| `sintegra_geracao` | possui | `sintegra_fonte_dados` | Fontes usadas na montagem do arquivo. |
| `sintegra_geracao` | possui | `sintegra_registro` | Registros gerados ou pendentes. |
| `sintegra_geracao` | possui | `sintegra_arquivo` | Arquivo texto resultante. |
| `sintegra_geracao` | possui | `sintegra_pendencia` | Pendencias funcionais e de completude. |

## 11. Dicionario de dados implantavel

### 11.1 `sintegra_geracao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da geracao. |
| EmpresaId | Identificador | Nao informado no material | Sim | Empresa | Empresa ativa da geracao. |
| Mes | Numero | 1-12 | Sim | Periodo | Mes da geracao. |
| Ano | Numero | 4 digitos | Sim | Periodo | Ano da geracao. |
| StatusGeracao | Enum/texto | Nao informado no material | Sim | Status | Status final nao informado. |
| DataSolicitacao | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data da solicitacao. |
| UsuarioSolicitanteId | Identificador | Nao informado no material | Nao informado no material | Usuario | Usuario que solicitou. |
| InventarioIncluido | Booleano | Sim/Nao | Nao | Parametro | Inventario e opcional no Sintegra. |

### 11.2 `sintegra_fonte_dados`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da fonte. |
| GeracaoId | Identificador | Nao informado no material | Sim | Geracao | Geracao Sintegra. |
| TipoFonte | Enum/texto | DocumentoFiscal, Pessoa, ItemFiscal, Transporte, Inventario, Empresa | Sim | Tipo | Fontes comprovadas ou condicionais no material. |
| ReferenciaId | Identificador | Nao informado no material | Nao informado no material | Registro de origem | Identificador da origem quando existir. |
| QuantidadeRegistros | Numero | Nao informado no material | Nao informado no material | Controle | Quantidade usada quando disponivel. |
| Observacao | Texto | Nao informado no material | Nao | Observacao | Detalhe da fonte. |

### 11.3 `sintegra_registro`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do registro. |
| GeracaoId | Identificador | Nao informado no material | Sim | Geracao | Geracao Sintegra. |
| CodigoRegistro | Texto | Reg10 a Reg90; registros 60/61; registros 51 a 61 citados | Sim | Layout | Campo consolidado a partir dos registros citados; dicionario completo nao informado. |
| Sequencia | Numero | Nao informado no material | Nao informado no material | Ordem | Ordem no arquivo. |
| ConteudoLinha | Texto | 126 caracteres quando gerado | Condicional | Arquivo | Linha deve ter tamanho fixo de 126 caracteres. |
| TamanhoLinha | Numero | 126 | Sim quando houver linha | Validacao | Usado para validar regra de tamanho fixo. |
| StatusRegistro | Enum/texto | Gerado, Pendente ou Nao informado no material | Sim | Status | Consolidado funcional para controle.[^nota1] |
| Mensagem | Texto | Nao informado no material | Nao | Mensagem | Erro/pendencia do registro. |

### 11.4 `sintegra_arquivo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do arquivo. |
| GeracaoId | Identificador | Nao informado no material | Sim | Geracao | Geracao Sintegra. |
| NomeArquivo | Texto | Nao informado no material | Nao informado no material | Arquivo | Nome final nao informado. |
| CaminhoArquivo | Texto | Nao informado no material | Nao informado no material | Arquivo | Caminho final nao informado. |
| Codificacao | Texto | ANSI 1252 | Sim | Arquivo | Codificacao comprovada no material. |
| QuantidadeLinhas | Numero | Nao informado no material | Nao informado no material | Controle | Quantidade de linhas do arquivo. |
| StatusArquivo | Enum/texto | Nao informado no material | Sim | Status | Status final nao informado. |
| DataGeracao | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data da geracao. |

### 11.5 `sintegra_pendencia`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da pendencia. |
| GeracaoId | Identificador | Nao informado no material | Sim | Geracao | Geracao Sintegra. |
| TipoPendencia | Enum/texto | CadastroEmpresa, DadoFiscal, LayoutRegistro, RegraRegistro, TamanhoLinha, Arquivo | Sim | Tipo | Categorias funcionais criadas para controle.[^nota1] |
| CodigoRegistro | Texto | Nao informado no material | Nao | Registro | Registro relacionado quando houver. |
| Mensagem | Texto | Nao informado no material | Sim | Mensagem | Descreve a pendencia sem presumir regra. |
| BloqueiaArquivoDefinitivo | Booleano | Sim/Nao | Sim | Controle | Indica se impede arquivo definitivo. |

## 12. Integracoes

| Origem/Destino | Tipo | Dados trocados | Regra |
|---|---|---|---|
| Cadastros Base | Entrada | Empresa, pessoas, dados cadastrais e fiscais. | Prerequisitos cadastrais da empresa devem ser validados. |
| Faturamento Fiscal Eletronico | Entrada | Documentos fiscais, itens, NFe e dados fiscais do periodo. | Registros dependem de dados fiscais disponiveis. |
| Estoque | Entrada condicional | Inventario. | Inventario e opcional no Sintegra. |
| Transporte | Entrada condicional | Dados de transporte. | Registro 70 e reservado para transporte. |
| Relatorios/Contabilidade | Saida | Arquivo Sintegra e pendencias. | Fronteira final ainda deve ser confirmada na MC. |

## 13. Telas, comandos e relatorios

| Item | Conteudo |
|---|---|
| Tela/operacao Sintegra | Material cita operacao especifica de Sintegra. |
| Filtros minimos | Empresa ativa, mes e ano. |
| Acoes minimas | Gerar arquivo, consultar resultado e visualizar pendencias. |
| Relatorios | Nao informado no material. |
| Permissoes | Nao informado no material. |

## 14. Cenarios de validacao

| ID | Cenario | Resultado esperado |
|---|---|---|
| SIN-TST-001 | Gerar Sintegra sem empresa ativa. | Geracao bloqueada por ausencia de empresa ativa. |
| SIN-TST-002 | Gerar Sintegra para empresa sem prerequisito cadastral. | Pendencia funcional de cadastro. |
| SIN-TST-003 | Gerar linha com tamanho diferente de 126 caracteres. | Arquivo definitivo bloqueado ou pendencia registrada. |
| SIN-TST-004 | Gerar arquivo com dados suficientes e linhas validas. | Arquivo texto ANSI 1252 gerado. |
| SIN-TST-005 | Gerar Sintegra com inventario opcional marcado. | Inventario considerado quando houver dados e regra suficiente. |

## 15. Indicadores e controles

| Indicador | Descricao |
|---|---|
| Geracoes por periodo | Quantidade de solicitacoes por mes/ano e empresa. |
| Arquivos gerados | Quantidade de arquivos Sintegra gerados. |
| Pendencias por tipo | Pendencias de cadastro, dado fiscal, layout, regra, tamanho de linha e arquivo. |
| Registros gerados | Quantidade de registros por codigo quando disponivel. |

## 16. Matriz de rastreabilidade funcional

| Capacidade | Regra | Dados | Teste |
|---|---|---|---|
| Geracao mensal | SIN-001, SIN-002 | `sintegra_geracao` | SIN-TST-001 |
| Prerequisitos cadastrais | SIN-003, SIN-004 | `sintegra_pendencia` | SIN-TST-002 |
| Linha fixa | SIN-006, SIN-007 | `sintegra_registro` | SIN-TST-003 |
| Arquivo ANSI 1252 | SIN-008 | `sintegra_arquivo` | SIN-TST-004 |
| Inventario opcional | SIN-010 | `sintegra_fonte_dados` | SIN-TST-005 |

## 17. Criterios de aceite

| Criterio | Resultado esperado |
|---|---|
| Empresa ativa | Geracao nao ocorre sem empresa ativa. |
| Periodo mensal | Geracao sempre exige mes e ano. |
| Cadastro | Ausencia de prerequisito cadastral gera pendencia. |
| Tamanho fixo | Toda linha gerada deve ter 126 caracteres. |
| Codificacao | Arquivo gerado deve ser texto ANSI 1252. |
| Ausencia de invencao | Registros sem layout/campo/regra ficam pendentes na MC. |

## 18. Notas de rodape

[^nota1]: As entidades `sintegra_geracao`, `sintegra_fonte_dados`, `sintegra_registro`, `sintegra_arquivo` e `sintegra_pendencia`, assim como alguns dominios de status e pendencia, foram criados nesta especificacao para permitir controle implantavel da geracao, porque o material comprova a capacidade, mas nao informa tabelas finais nem dicionario completo de layout.
