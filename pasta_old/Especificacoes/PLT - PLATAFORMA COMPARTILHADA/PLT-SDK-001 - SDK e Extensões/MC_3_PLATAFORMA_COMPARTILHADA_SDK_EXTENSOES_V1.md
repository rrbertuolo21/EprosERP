# MC_3_PLATAFORMA_COMPARTILHADA_SDK_EXTENSOES_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** SDK_EXTENSOES  
**Documento:** Matriz de completude  
**Versao:** V1  
**Status:** Concluido  
**Ultima atualizacao:** 2026-06-09

## 1. Objetivo

Registrar o nivel de completude do submodulo SDK e Extensoes, separando capacidades comprovadas no material, estruturas funcionais criadas para implantacao e lacunas que dependem de decisao da Siser.

## 2. Resumo de cobertura

| Area | Status | Evidencia funcional consolidada |
|---|---|---|
| Registry de extensoes | Parcial | Material informa registry versionado, modulo, alias, uniqueid, descricao, autor, versao e status. |
| Manifesto | Parcial | Material informa manifestos, installdefs, arquivos, providers, stubs e dependencias. |
| Instalacao por tenant | Parcial | Material informa sincronizacao tenant, migracao por modulo e tabela tenant de modulos. |
| Ativacao | Parcial | module_status enabled/disabled informado. |
| Permissoes | Parcial | JSON com module_name, module_alias e module_permission informado. |
| Menus | Parcial | Placements, tipos e visibilidade por user_type/permissao informados. |
| Eventos | Parcial | Eventos com module, type e extractor informados. |
| Callbacks seguros | Parcial | HMAC e replay protection informados como decisao. |
| Metadados dinamicos | Parcial | Labels, layouts, campos dinamicos, cache e pipeline controlado informados. |
| Utilitarios | Parcial | Parametros, validacoes, calculos, XML, mensagens, impressao e criptografia citados. |
| APIs | Pendente | Endpoints finais nao informados. |
| Sandbox | Pendente | Sandbox tenant citado, sem detalhamento. |

## 3. Itens de completude

| Codigo | Area | Status | O que existe | O que falta | Prioridade |
|---|---|---|---|---|---|
| MC-SDK-001 | Escopo | Parcial | Extensoes, hooks, instalacao por tenant e utilitarios. | Validar escopo do MVP. | P0 |
| MC-SDK-002 | Registry | Parcial | module_id, module_name, module_alias, module_uniqueid, description, author, version e status. | Definir registry global final, ownership e publicacao. | P0 |
| MC-SDK-003 | Manifesto | Parcial | Manifesto, arquivos, providers, dependencias e instalacao citados. | Definir schema oficial, validadores e campos obrigatorios. | P0 |
| MC-SDK-004 | Assinatura de pacote | Parcial | Pacotes assinados citados. | Definir algoritmo, cadeia de confianca, revogacao e verificacao. | P0 |
| MC-SDK-005 | Instalacao tenant | Parcial | Sincronizacao, migracao e tabela tenant informadas. | Definir rollback, ordem de etapas, logs e falha parcial. | P0 |
| MC-SDK-006 | Status | Parcial | enabled/disabled informado. | Definir estados adicionais de erro, atualizacao, revogado e pendente. | P1 |
| MC-SDK-007 | Permissoes | Parcial | module_permission com none/view/manage/admin/yes/no informado. | Unificar dominio final e matriz com Aplicativo. | P0 |
| MC-SDK-008 | Sincronizacao de papeis | Parcial | Preserva permissao existente, admin default elevado e demais restritos. | Definir gatilhos, frequencia e auditoria final. | P1 |
| MC-SDK-009 | Menus | Parcial | Placements, tipos, parent, title, user_type e role informados. | Definir schema final, ordem, icone, traducao e validacao. | P1 |
| MC-SDK-010 | Eventos | Parcial | module, type e extractor informados. | Definir catalogo oficial, versionamento de payload e retries. | P0 |
| MC-SDK-011 | Callbacks | Parcial | HMAC e replay protection informados. | Definir janela de replay, headers, rotacao de segredo e erro padrao. | P0 |
| MC-SDK-012 | Metadados | Parcial | Labels, layouts, campos, dropdowns e cache informados. | Definir pipeline, aprovacao, rollback, diff e cache final. | P0 |
| MC-SDK-013 | Utilitarios | Parcial | Validacoes, calculos, mensagens, XML, impressao e criptografia citados. | Definir catalogo oficial, interfaces e ownership por modulo. | P1 |
| MC-SDK-014 | Parametros | Parcial | Parametros globais e de empresa/venda/financeiro/fiscal citados. | Definir fronteira com Configuracao e Cadastros Base. | P0 |
| MC-SDK-015 | APIs | Pendente | Contratos funcionais inferidos; endpoint final ausente. | Definir rotas, metodos, payloads e versionamento. | P0 |
| MC-SDK-016 | Sandbox | Pendente | Sandbox tenant citado. | Definir ambiente de teste, dados, isolamento e reset. | P1 |
| MC-SDK-017 | Telas | Parcial | Lista, detalhe, painel e telas de customizacao citadas. | Detalhar UX final de registry, instalacao, permissao, eventos e metadados. | P1 |
| MC-SDK-018 | Auditoria | Parcial | Historico com Acao, UsuarioId e PayloadJson informado. | Definir payload mascarado, antes/depois, retencao e exportacao. | P0 |
| MC-SDK-019 | Testes | Parcial | Cenarios basicos e smoke transversal citados; EF ampliou. | Criar testes de pacote, permissao, menu, evento, callback, metadado e utilitario. | P0 |
| MC-SDK-020 | Compliance | Incompleto | LGPD citada genericamente. | Definir dados sensiveis em extensoes, logs, segredos e payloads. | P0 |

## 4. Decisoes pendentes

| Codigo | Decisao | Motivo |
|---|---|---|
| D-SDK-001 | Definir schema oficial do manifesto. | Necessario para homologacao. |
| D-SDK-002 | Definir politica de assinatura de pacote. | Necessario para seguranca. |
| D-SDK-003 | Definir status finais de extensao por tenant. | Necessario para operacao e suporte. |
| D-SDK-004 | Unificar dominio de permissoes de extensao. | Evita mistura de none/view/manage/admin/yes/no sem regra. |
| D-SDK-005 | Definir catalogo de pontos de menu e extensao visual. | Necessario para UI consistente. |
| D-SDK-006 | Definir catalogo e versionamento de eventos. | Necessario para compatibilidade. |
| D-SDK-007 | Definir HMAC, janela e protecao contra replay. | Necessario para callbacks externos. |
| D-SDK-008 | Definir pipeline de metadados dinamicos. | Necessario para customizacao auditavel. |
| D-SDK-009 | Definir fronteira entre utilitarios SDK e modulos donos. | Evita duplicidade de regra funcional. |
| D-SDK-010 | Definir APIs finais e sandbox tenant. | Necessario para desenvolvimento seguro. |

## 5. Riscos funcionais

| Risco | Impacto | Mitigacao proposta |
|---|---|---|
| Extensao sem manifesto rigido. | Instalacao imprevisivel. | Schema oficial e validacao bloqueante. |
| Pacote sem assinatura. | Risco de codigo ou metadado nao confiavel. | Exigir assinatura e revogacao. |
| Permissao inconsistente. | Usuario ve menu ou acao indevida. | Unificar matriz e testar por papel. |
| Callback sem replay protection. | Evento externo pode ser repetido indevidamente. | HMAC, timestamp/nonce e janela. |
| Metadado alterado sem trilha. | Dificil reverter e auditar. | Pipeline, historico e rollback. |
| Utilitario assumindo regra de dominio. | Duplicidade de regra e resultado divergente. | Catalogo com fronteira por modulo dono. |

## 6. Proximo passo operacional

O submodulo `PLATAFORMA_COMPARTILHADA/SDK_EXTENSOES` foi processado e esta concluido como conteudo parcial-controlado. O proximo item da matriz principal e `PLATAFORMA_COMPARTILHADA/SOA_COLABORACAO`.
