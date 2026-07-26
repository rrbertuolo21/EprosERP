# MC 1 Cadastros Base — Geografia e Localizacao V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Cadastros Base |
| Submodulo | Geografia e Localizacao |
| Versao | V1 |
| Data | 2026-06-06 |

## 2. Matriz de completude

| Area | Status | Evidencia funcional consolidada | Lacuna | Acao recomendada | Prioridade | Dono sugerido |
|---|---|---|---|---|---|---|
| Municipio Brasil | Completo parcial | Id como codigo IBGE, nome, UF, ativo e validacoes. | Garantir tamanho de 7 digitos e tipo final do Id. | Formalizar constraint de 7 digitos. | P0 | Cadastros |
| Pais | Parcial | Id, Nome e Capital. | ISO alpha-2, alpha-3, numerico, discagem e moeda nao detalhados. | Completar padrao ISO. | P1 | Cadastros |
| UF/Subdivisao | Parcial | UF brasileira como enum. | Modelo internacional de subdivisao nao implementado. | Criar tabela Subdivisao. | P1 | Cadastros |
| Endereco | Parcial | PaisId, MunicipioId, UF, CEP, logradouro, numero, complemento, bairro, referencia e tipo. | Posse do endereco fica em Pessoa/Empresa; historico e normalizacao nao detalhados. | Definir contrato compartilhado de endereco. | P0 | Cadastros |
| CEP Brasil | Parcial | CEP de 8 digitos, cache, provedor e fallback manual. | Multi-provedor, SLA, retencao de cache e reprocessamento nao detalhados. | Criar governanca de consulta postal. | P1 | Plataforma/Cadastros |
| Codigo postal internacional | Incompleto | Gap documentado por pais. | Regex, mascara e validacao por pais nao implementados. | Criar FormatoCodigoPostal. | P1 | Cadastros |
| Sincronizacao geografica | Parcial | Estados e contadores definidos. | Agendamento periodico e alerta de falha nao detalhados. | Implementar job agendado idempotente. | P0 | Plataforma |
| Zona de entrega | Parcial | Entidade com nome e faixa de CEP. | Sobreposicao, prioridade e integracao com frete nao detalhadas. | Definir regra de faixa e conflito. | P1 | Logistica |
| Geocodificacao | Parcial | Latitude/longitude opcionais. | Fluxo de geocoding e provedor nao detalhados. | Definir geocoding, cache e auditoria. | P2 | Logistica |
| Hierarquia territorial | Incompleto | Gap documentado. | Territorio pai e agregacoes regionais nao implementados. | Criar hierarquia territorial. | P2 | Cadastros/BI |
| Vigencia geografica | Incompleto | Gap documentado. | VigenciaInicio/VigenciaFim ausentes. | Modelar vigencia temporal. | P2 | Cadastros |
| Normalizacao/deduplicacao | Incompleto | Gap documentado. | Padronizacao postal e deduplicacao ausentes. | Definir normalizacao e match de endereco. | P1 | Dados |
| Dados tributarios em municipio | Resolvido como fronteira | Campos de aliquota foram identificados fora da natureza geografica. | Garantir que fiscal assuma esses dados. | Referenciar modulo fiscal. | P0 | Fiscal |
| Testes automatizados | Parcial | CTs identificados. | Cobertura automatizada nao comprovada. | Criar testes das validacoes e jobs. | P0 | QA |

## 3. Pendencias criticas P0

1. Formalizar `municipio.Id` como codigo IBGE de 7 digitos no Brasil.
2. Implementar job periodico de sincronizacao geografica idempotente.
3. Garantir que municipio extinto seja inativado e nunca removido fisicamente.
4. Definir contrato compartilhado de endereco consumido por Pessoa, Empresa, Vendas, Compras e Fiscal.
5. Separar definitivamente dados tributarios do cadastro geografico.
6. Automatizar testes das 15 validacoes de dominio.

## 4. Perguntas para validacao humana

| Pergunta | Impacto |
|---|---|
| A base de pais/UF/municipio sera global compartilhada ou tenantizada? | Define TenantId e governanca de atualizacao. |
| O Epros deve permitir municipio customizado por tenant? | Afeta integridade fiscal e sincronizacao oficial. |
| CEP manual pode criar cache definitivo ou apenas pendencia revisavel? | Define qualidade de dados. |
| Zona de entrega pode ter faixas sobrepostas com prioridade? | Define algoritmo de frete/logistica. |
| Subdivisao internacional entra no MVP ou fase posterior? | Define modelo inicial do endereco. |
| Geocodificacao sera automatica ou apenas manual? | Define provedores, custo e privacidade. |

## 5. Criterios de aceite

| ID | Criterio |
|---|---|
| CA-001 | Municipio brasileiro e salvo apenas com codigo IBGE valido. |
| CA-002 | Municipio com nome fora do intervalo permitido e bloqueado. |
| CA-003 | Pais com nome vazio e bloqueado. |
| CA-004 | Endereco sem PaisId ou MunicipioId valido e bloqueado. |
| CA-005 | Endereco brasileiro com CEP invalido e bloqueado. |
| CA-006 | Consulta de CEP usa cache quando existir. |
| CA-007 | Falha de provedor gera registro para reprocessamento. |
| CA-008 | Preenchimento manual de CEP registra usuario, motivo e data/hora. |
| CA-009 | Sincronizacao reexecutada nao duplica municipios. |
| CA-010 | Municipio extinto fica inativo. |
| CA-011 | Dados tributarios nao sao persistidos em municipio. |

## 6. Itens de construcao

| Item | Entrega esperada | Prioridade |
|---|---|---|
| Modelo geografico Brasil | Pais, municipio, UF, endereco e CEP com constraints. | P0 |
| Sincronizacao oficial | Job, estados, contadores, alerta e relatorio. | P0 |
| Consulta CEP | Cache, provedor, fallback manual e reprocessamento. | P0 |
| Zona de entrega | Faixas, validacao de sobreposicao e consumo logistico. | P1 |
| ISO pais | Codigos ISO, discagem e moeda padrao. | P1 |
| Subdivisao internacional | Estados/provincias/condados por pais. | P1 |
| Codigo postal por pais | Regex, mascara e exemplos por pais. | P1 |
| Normalizacao endereco | Padronizacao e deduplicacao. | P1 |
| Geocodificacao | Latitude/longitude automatica e reversa. | P2 |
| Vigencia territorial | Inicio/fim de validade geografica. | P2 |
