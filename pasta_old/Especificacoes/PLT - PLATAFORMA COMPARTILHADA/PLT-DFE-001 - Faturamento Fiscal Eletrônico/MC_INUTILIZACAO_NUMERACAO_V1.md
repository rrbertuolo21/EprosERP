# Matriz de Completude - Epros

**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Capacidade:** INUTILIZACAO_NUMERACAO  
**Versao:** V1  
**Empresa:** Siser  

## 1. Objetivo

Registrar lacunas da inutilizacao de numeracao fiscal para evoluir a especificacao sem inventar regras fiscais, validacoes de faixa ou politicas de retencao nao comprovadas no material.

## 2. Resumo de completude

| Item | Status |
|---|---|
| Solicitacao de inutilizacao | Parcial |
| Lista de inutilizacoes | Parcial |
| Tabela `inutilizacao_simplificado` | Completo no material para campos extraidos |
| Documento e UF | Completo no material |
| Ambiente, ano, serie e faixa | Parcial |
| Modelo fiscal 55/65 | Parcial |
| cStat 102 autorizado | Parcial |
| XML e protocolo | Completo no material para campos extraidos |
| Justificativa | Parcial |
| Rejeicao | Parcial |
| Sobreposicao/concorrencia | Incompleto |
| Retencao XML | Incompleto |

## 3. Matriz

| ID | Capacidade esperada | Status | Conteudo comprovado | Falta para implantacao | Prioridade |
|---|---|---|---|---|---|
| MC-INUT-001 | Solicitar inutilizacao | Parcial | Envio de faixa com parametros da empresa. | Fechar contrato de entrada e mensagens. | P0 |
| MC-INUT-002 | Listar inutilizacoes | Parcial | Consulta por documento e ambiente. | Definir filtros, paginacao, ordenacao e colunas. | P1 |
| MC-INUT-003 | Modelo `inutilizacao_simplificado` | Completo no material para campos extraidos | TenantId, UF, documento, ambiente, ano, serie, faixa, modelo, status, justificativa, rejeicao, XML, protocolo e caminho. | Definir PK/FK e indices finais. | P0 |
| MC-INUT-004 | Documento da empresa | Completo no material | `Documento` varchar(20) NOT NULL. | Confirmar validacao de CPF/CNPJ e origem cadastral. | P0 |
| MC-INUT-005 | UF | Completo no material | `Uf` varchar(2) NOT NULL. | Confirmar lista de UFs e origem cadastral. | P0 |
| MC-INUT-006 | Ambiente | Parcial | Producao=1, Homologacao=2. | Definir dominio tecnico final e comportamento por ambiente. | P0 |
| MC-INUT-007 | Modelo fiscal | Parcial | Modelos 55 e 65 comprovados. | Confirmar se outros modelos entram na rotina. | P1 |
| MC-INUT-008 | Serie e faixa | Parcial | Serie, NrNfInicial e NrNfFinal comprovados. | Definir formato, limites, zeros, sobreposicao, concorrencia e faixa ja emitida. | P0 |
| MC-INUT-009 | Justificativa | Parcial | Campo `Justificativa` nvarchar(max). | Definir tamanho minimo/maximo, conteudo e mensagens. | P0 |
| MC-INUT-010 | Retorno autorizado | Parcial | cStat 102 persiste XML; protocolo obrigatorio. | Definir dominios de status e tratamento de retorno sem protocolo. | P0 |
| MC-INUT-011 | XML da inutilizacao | Completo no material para campo extraido | `Xml` nvarchar(max) e `XmlCaminho` varchar(500). | Definir retencao, imutabilidade, download e backup. | P0 |
| MC-INUT-012 | Motivo de rejeicao | Completo no material para campo extraido | `MotivoRejeicaoSefaz` nvarchar(max). | Definir exibicao, historico e reenvio. | P1 |
| MC-INUT-013 | Certificado | Parcial | Operacao usa certificado fiscal quando exigido. | Definir validade, erro, reenvio e responsavel por manutencao. | P0 |
| MC-INUT-014 | Unicidade/sobreposicao | Incompleto | Chaves funcionais sugerem faixa por documento/ambiente/modelo/serie/ano. | Definir constraint fisica e regra de faixa sobreposta. | P0 |
| MC-INUT-015 | Integracao com emissao | Incompleto | Faixa inutilizada nao deve ser usada operacionalmente. | Definir bloqueio nos motores de emissao e rollback. | P0 |
| MC-INUT-016 | Permissoes | Incompleto | Rotina de lista e envio comprovada. | Definir RBAC de consultar, inutilizar e reprocessar. | P0 |
| MC-INUT-017 | Auditoria | Parcial | Campos permitem rastrear faixa e retorno. | Definir usuario/processo, tentativas e antes/depois. | P1 |
| MC-INUT-018 | Testes | Parcial | Cenario cStat 102 persiste XML comprovado. | Adicionar testes de faixa invertida, sobreposta, rejeitada, certificado ausente e consulta. | P1 |

## 4. Decisoes necessarias

| ID | Decisao | Impacto |
|---|---|---|
| D-INUT-001 | Definir regra completa de faixa: limites, sobreposicao e numeros ja emitidos. | Evita inutilizar faixa indevida. |
| D-INUT-002 | Definir tamanho e conteudo permitido da justificativa. | Necessario para validacao fiscal. |
| D-INUT-003 | Definir constraint fisica de unicidade. | Evita duplicidade de inutilizacao. |
| D-INUT-004 | Definir retencao e download do XML. | Necessario para compliance. |
| D-INUT-005 | Definir matriz de permissoes. | Necessario para seguranca operacional. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_NFSE`, detalhando lote, RPS, prestador, tomador, servico, valores, consulta e cancelamento municipal conforme material disponivel.
