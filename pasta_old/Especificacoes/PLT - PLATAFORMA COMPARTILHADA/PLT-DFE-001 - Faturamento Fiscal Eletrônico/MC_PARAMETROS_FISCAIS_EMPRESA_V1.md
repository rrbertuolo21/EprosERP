# Matriz de Completude - Epros

**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Capacidade:** PARAMETROS_FISCAIS_EMPRESA  
**Versao:** V1  
**Empresa:** Siser  

## 1. Objetivo

Registrar o que falta para que Parametros Fiscais por Empresa sejam implantaveis em padrao completo, sem inventar campos ou regras ausentes no material.

## 2. Resumo de completude

| Item | Status |
|---|---|
| Parametros gerais de empresa | Parcial |
| Parametros NFC-e homologacao | Completo no material para campos extraidos |
| Parametros NFC-e producao | Completo no material para campos extraidos |
| Parametros NF-e producao | Incompleto |
| Certificado/tenant fiscal | Parcial |
| Configuracao de impressao NFC-e | Parcial |
| Parametros de servico fiscal | Incompleto |

## 3. Matriz

| ID | Capacidade esperada | Status | Conteudo comprovado | Falta para implantacao | Prioridade |
|---|---|---|---|---|---|
| MC-PARAM-001 | Descricao dos parametros fiscais | Parcial | Obrigatoria e max 100 caracteres. | Confirmar campo persistido, unicidade e exibicao por empresa. | P1 |
| MC-PARAM-002 | Ambiente NF-e e NFC-e | Parcial | TipoAmbienteNfe e TipoAmbienteNfce obrigatorios; Producao=1, Homologacao=2. | Confirmar valor padrao, permissao de troca apos emissao e historico. | P0 |
| MC-PARAM-003 | NFC-e homologacao | Completo no material | CSC max 36, ID CSC max 6, serie, proximo numero e contingencia. | Confirmar se serie/proximo numero homologacao podem ser zero. | P1 |
| MC-PARAM-004 | NFC-e producao | Parcial | CSC/ID CSC obrigatorios, tamanhos, serie e proximo numero nao podem iniciar zero. | Fechar concorrencia de numeracao e troca de CSC em producao. | P0 |
| MC-PARAM-005 | NF-e producao | Incompleto | Material cita NfeSerieProducao e NfeProximoNrProducao obrigatorios e nao-zero. | Levantar estrutura completa da entidade NF-e de parametros, homologacao/producao e demais campos. | P0 |
| MC-PARAM-006 | Destacar ICMS ST | Parcial | Campo booleano obrigatorio. | Definir onde o parametro e consumido e efeito exato no calculo/documento. | P1 |
| MC-PARAM-007 | Tag de codigo do produto | Parcial | CodigoInterno=1 e CodigoProduto=2. | Definir campo persistido, tela e impacto por documento. | P1 |
| MC-PARAM-008 | Tenant fiscal | Parcial | TenantId varchar(200) obrigatorio; Nome varchar(150) obrigatorio. | Confirmar relacionamento com empresa, unicidade e ciclo de vida. | P0 |
| MC-PARAM-009 | Certificado digital | Parcial | Caminho, senha, serial, validade inicial/final, tipo e ultima transmissao. | Definir criptografia, mascaramento, rotacao, revogacao, alerta de vencimento e trilha de acesso. | P0 |
| MC-PARAM-010 | Transmissao do certificado | Parcial | Certificado e transmitido para contexto fiscal; falha gera erro. | Definir idempotencia, reenvio, status de transmissao e conciliacao. | P0 |
| MC-PARAM-011 | Storage certificado | Parcial | Operacoes leem certificado em `Certificados/{documento}`. | Confirmar estrutura de diretorios por tenant/empresa/documento e politica de retencao. | P0 |
| MC-PARAM-012 | Configuracao impressao NFC-e | Parcial | EmpresaId obrigatorio, Id obrigatorio quando aplicavel, indice por EmpresaId, margens e QR Code. | Completar dominios de modo de impressao, layout QR Code, versao QR Code e preview. | P1 |
| MC-PARAM-013 | Nome da impressora | Parcial | NomeImpressora max 250 citado no material. | Definir se pertence a parametros gerais ou configuracao de impressao NFC-e. | P2 |
| MC-PARAM-014 | TimeOut/protocolo | Incompleto | Material cita TimeOut, Protocolo e tipo de emissao. | Definir tabela, campos, defaults, limites e quem altera. | P1 |
| MC-PARAM-015 | Versoes de servico fiscal | Incompleto | Material cita 15 campos de versao. | Levantar nomes finais, obrigatoriedade e impacto por operacao. | P1 |
| MC-PARAM-016 | Codigo IBGE do municipio do emitente | Parcial | Material exige codigo diferente de zero para emissao. | Confirmar se validacao fica nesta EF ou em Cadastros Base. | P0 |
| MC-PARAM-017 | Regime tributario/CRT | Parcial | Material cita CRT/regime tributario do emitente. | Confirmar dominio final, origem cadastral e efeito em totais. | P0 |
| MC-PARAM-018 | Historico/versionamento | Incompleto | EF consolidada indica parametros versionados/auditados. | Definir tabela/evento de historico e motivo obrigatorio para campos criticos. | P1 |
| MC-PARAM-019 | Concorrencia de numeracao | Incompleto | Material exige serie/proximo numero nao-zero. | Definir bloqueio transacional, reserva de numero, reversao e idempotencia. | P0 |
| MC-PARAM-020 | CF-e/SAT nos parametros | Incompleto | Material cita modelo/status CF-e/SAT, mas nao detalha parametros neste recorte. | Levantar parametros especificos na EF CF-e/SAT. | P1 |

## 4. Decisoes necessarias

| ID | Decisao | Impacto |
|---|---|---|
| D-PARAM-001 | Confirmar entidade final para parametros NF-e producao. | Necessario para completar modelo e dicionario. |
| D-PARAM-002 | Definir politica de seguranca para senha e arquivo de certificado. | Bloqueia implantacao segura. |
| D-PARAM-003 | Definir regra de concorrencia/reserva de numeracao fiscal. | Evita duplicidade fiscal em producao. |
| D-PARAM-004 | Definir se versoes de servico fiscal serao parametros editaveis por empresa ou administrados globalmente pela Siser. | Afeta tela, permissao e suporte. |
| D-PARAM-005 | Definir se diagnostico de aptidao fiscal vira tela/relatorio operacional. | Facilita implantacao e suporte. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_NFE_SAIDA`, porque a parametrizacao fiscal por empresa ja estabelece os pre-requisitos de ambiente, certificado e numeracao para a emissao.
