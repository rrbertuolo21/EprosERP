# MC 3 Plataforma Compartilhada - Assinatura Eletronica V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Plataforma Compartilhada |
| Submodulo | Assinatura Eletronica |
| Versao | V1 |
| Data | 2026-06-06 |

## 2. Matriz de completude

| Area | Status | Evidencia funcional consolidada | Lacuna | Acao recomendada | Prioridade | Dono sugerido |
|---|---|---|---|---|---|---|
| Assinatura simples | Parcial | Material comprova assinatura vinculada a contrato, usuario, tipo, dados e data/hora. | Tipos finais, validacoes e formato do payload nao estao detalhados. | Definir contrato funcional do payload e tipos permitidos. | P0 | Plataforma/Produto |
| Signatarios | Parcial | Material comprova usuario signatario e criterio de duas assinaturas para contrato. | Nao ha modelo completo de signatarios, ordem, obrigatoriedade e externos. | Validar modelo de signatarios internos/externos. | P0 | Plataforma/Cadastros |
| Permissao | Parcial | Material comprova permissao especifica para assinar contrato. | Matriz completa de permissoes por tipo de documento nao existe. | Criar permissoes por acao e tipo documental. | P0 | Seguranca |
| Criterio de conclusao | Parcial | Contrato pode mudar para ativo quando tiver duas assinaturas. | Criterio por tipo de documento nao esta definido. | Parametrizar criterio minimo por tipo documental. | P0 | Produto |
| Estados | Parcial | Fluxo rascunho, analise, aprovacao, ativo, inativo, encerrado e reativacao aparece no material. | Estados especificos de assinatura parcial, expirada e cancelada precisam validacao. | Validar maquina de estados final. | P0 | Produto/Workflow |
| Auditoria | Parcial | Material informa historico com usuario, timestamp e IP. | Retencao, mascaramento, correlationId e consulta de auditoria nao estao detalhados. | Definir politica de auditoria e privacidade. | P0 | Compliance |
| Documento assinado | Incompleto | Material indica integracao com repositorio documental. | Versao assinada, imutabilidade, anexo final e alteracao pos-assinatura nao estao definidos. | Fechar contrato com Gestao Eletronica de Documentos. | P0 | Plataforma/GED |
| Hash documental | Incompleto | Lacuna identificada no material. | Hash, algoritmo e verificacao de integridade nao estao definidos. | Definir modelo de integridade documental. | P0 | Seguranca/Compliance |
| Carimbo de tempo | Incompleto | Lacuna identificada no material. | Provedor, tipo de timestamp e evidencias nao estao definidos. | Decidir exigencia e provedor. | P1 | Compliance |
| Certificado/provedor | Incompleto | Material nao comprova assinatura qualificada. | Provedor, certificado, validade, revogacao e nivel de assinatura ausentes. | Definir se MVP tera assinatura simples, avancada ou qualificada. | P0 | Produto/Juridico |
| APIs | Incompleto | Material nao informa endpoints finais. | Rotas, metodos, contratos, erros e autorizacao precisam especificacao tecnica. | Publicar APIs no padrao Epros. | P0 | Plataforma/API |
| Telas | Parcial | Lista, detalhe, painel gestor e relatorios estao descritos em alto nivel. | Campos, acoes, estados visuais e permissao por tela precisam detalhamento. | Desenhar telas finais. | P1 | Produto/UX |
| Relatorios | Parcial | Posicao geral e auditoria de alteracoes constam no material. | Layout, filtros, colunas e exportacao nao estao detalhados. | Definir relatorios finais. | P1 | Produto/BI |
| Notificacoes | Incompleto | Necessidade funcional inferida do processo de assinatura. | Canais, templates, lembretes, expiracao e SLA nao estao definidos. | Criar politica de notificacao. | P1 | SOA e Colaboracao |
| Fluxo publico | Incompleto | Material nao define assinatura por convidado externo. | Token, expiracao, identidade externa e seguranca ausentes. | Validar se assinatura externa entra no MVP. | P0 | Produto/Seguranca |
| Testes | Parcial | Cenarios basicos de criacao, aprovacao, falha e auditoria aparecem no material. | Faltam testes de assinatura multipla, permissao, documento versionado e privacidade. | Criar suite automatizada completa. | P0 | QA |

## 3. Pendencias criticas P0

1. Definir se o MVP aceita apenas assinatura eletronica simples ou exige assinatura com certificado/provedor.
2. Fechar o modelo de signatarios: usuario interno, pessoa externa, ordem, obrigatoriedade e substituicao.
3. Parametrizar criterio minimo de assinaturas por tipo de documento.
4. Definir se assinatura publica por link existira e quais controles de seguranca serao obrigatorios.
5. Fechar contrato com Gestao Eletronica de Documentos para versao assinada, anexos e imutabilidade.
6. Definir hash documental, algoritmo, armazenamento e verificacao.
7. Padronizar permissao de solicitar, assinar, aprovar, cancelar, inativar, reativar e consultar auditoria.
8. Publicar endpoints finais no padrao do Epros.
9. Definir retencao, mascaramento e acesso a trilha de auditoria.
10. Criar testes automatizados para assinatura multipla, permissao, falha, auditoria e alteracao de documento assinado.

## 4. Perguntas para validacao humana

| Pergunta | Impacto |
|---|---|
| O MVP deve ter assinatura simples ou ja precisa de certificado/provedor externo? | Define arquitetura, custo e validade juridica. |
| Assinatura externa por link sera permitida? | Define token, expiracao, identidade e riscos. |
| Quais tipos de documento entram no primeiro ciclo? | Define politicas e consumidores. |
| Dois signatarios e regra fixa ou apenas regra de contrato? | Define parametrizacao por tipo documental. |
| Assinatura pode ser substituida ou excluida? Em quais casos? | Define auditoria e controle juridico. |
| Documento alterado depois da assinatura exige nova assinatura automaticamente? | Define integracao documental. |
| Qual prazo de validade de solicitacao de assinatura? | Define expiracao e notificacoes. |
| Quais relatorios sao obrigatorios no MVP? | Define escopo de BI e auditoria. |

## 5. Itens de construcao

| Item | Entrega esperada | Prioridade |
|---|---|---|
| Modelo de solicitacao | Entidade, estados, signatarios e criterio minimo. | P0 |
| Registro de assinatura | Campos comprovados, validacao, payload e timestamp. | P0 |
| Permissoes | Matriz por acao e tipo de documento. | P0 |
| Integracao documental | Documento, versao, evidencia e anexo assinado. | P0 |
| Auditoria | Historico, IP, usuario, timestamp, motivo, correlationId e retencao. | P0 |
| APIs | Endpoints, contratos, erros e seguranca. | P0 |
| Hash/integridade | Campos, algoritmo, geracao e verificacao. | P0 |
| Provedor/certificado | Decisao de nivel de assinatura e contrato de integracao. | P0 |
| Telas | Lista, detalhe, painel gestor e acoes por permissao. | P1 |
| Relatorios | Posicao geral e auditoria com filtros e exportacao. | P1 |
| Notificacoes | Convite, lembrete, sucesso, falha, cancelamento e expiracao. | P1 |
| Testes automatizados | Suite para fluxo completo e falhas. | P0 |

## 6. Criterios de aceite de completude

| ID | Criterio |
|---|---|
| MC-ASS-001 | EF possui modelo de dados antes do dicionario. |
| MC-ASS-002 | Todos os campos do dicionario possuem tipo, tamanho/dominio, obrigatoriedade, relacao e regra/observacao. |
| MC-ASS-003 | Campos sem informacao no material estao marcados como Nao informado no material. |
| MC-ASS-004 | MC separa evidencias comprovadas de lacunas de implantacao. |
| MC-ASS-005 | Nenhuma regra de certificado/provedor e tratada como comprovada quando o material nao confirma. |
| MC-ASS-006 | Permissao de assinatura e tratada como obrigatoria. |
| MC-ASS-007 | Criterio de duas assinaturas para contrato esta preservado. |
| MC-ASS-008 | Lacunas de documento assinado, hash, carimbo de tempo e provedor estao explicitadas. |
