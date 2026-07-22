# Matriz de Completude - Epros

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** GESTAO_ELETRONICA_DE_DOCUMENTOS_GED  
**Versao:** V1  
**Status:** Em revisao  
**Ultima atualizacao:** 2026-06-07

## 1. Objetivo da matriz

Registrar lacunas, decisoes e capacidades pendentes para que GED seja implantado como repositorio documental e de arquivos corporativo do Epros.

## 2. Resumo executivo

| Indicador | Situacao |
|---|---|
| Conteudo funcional aproveitavel | Com conteudo |
| EF criada | Sim |
| Modelo de dados funcional | Parcialmente completo |
| Dicionario de dados implantavel | Parcialmente completo |
| Principais lacunas | Modelo unico documento/arquivo/midia/anexo, seguranca de hash/senha, API final, retencao, OCR, denuncia publica, storage governado |

## 3. Matriz de completude

| ID | Capacidade esperada | Status | Evidencia funcional disponivel | O que falta construir/validar | Prioridade |
|---|---|---|---|---|---|
| MC-GED-001 | Modelo unico de documento, arquivo, midia e anexo | Incompleto | Existem documents, document_revisions, media, file e attachments/referencias. | Definir modelo canonico unico, evitando duplicidade e mantendo compatibilidade com modulos consumidores. | P0 |
| MC-GED-002 | Documento versionado | Parcial | Documento e revisao separados, revisao atual, revisao obrigatoria e log de mudanca. | Definir numeracao de revisao, imutabilidade pos-aprovacao e relacao com assinatura. | P0 |
| MC-GED-003 | Controle documental mensal | Parcial | Tipo, pessoa, periodo, entregue e data entrada; filtro de pendentes e baixa. | Fechar cadastros de tipos, recorrencia, notificacoes e relatorio final. | P1 |
| MC-GED-004 | Upload governado | Parcial | Permissao, tipo, MIME, tamanho, quota, lote, URL remota, API, partes, hash bloqueado. | Consolidar contrato API, mensagens, limites por plano e politicas de seguranca. | P0 |
| MC-GED-005 | Download seguro | Parcial | Token, senha, nivel minimo, tracker, concorrencia, range, captcha e paginas intermediarias. | Definir politica final de token, IP, captcha, limite, download publico e logs. | P0 |
| MC-GED-006 | Hash e senha seguros | Incompleto | Material informa campos de hash e senha de 32/64 caracteres. | Definir algoritmo seguro atual, migracao e armazenamento de segredo. | P0 |
| MC-GED-007 | Deduplicacao fisica e registro logico | Parcial | Reuso de bytes por hash/tamanho e unique_hash por registro. | Definir criterios finais de dedupe, colisao, integridade e auditoria. | P0 |
| MC-GED-008 | Pastas e compartilhamento | Parcial | Arvore, share key, permissoes view/upload_download/all, senha, privacidade em cascata. | Definir UX final, heranca, links anonimos multiplos e expiracao de compartilhamento. | P0 |
| MC-GED-009 | Biblioteca de midia reutilizavel | Parcial | Midia, diretorios, picker/modal, upload, download, exclusao e mover. | Fechar contrato para consumo por todos os modulos e padrao de URL. | P1 |
| MC-GED-010 | Storage dinamico e multi-servidor | Parcial | Storage ativo, discos dinamicos, servidores, status, capacidade, fila move/delete/restore. | Definir ownership com Configuracao, credenciais, teste, health check e failover. | P0 |
| MC-GED-011 | Retencao e descarte | Incompleto | Purge e fila existem, mas sem matriz legal por tipo documental. | Definir retencao por tipo, hold juridico, descarte auditado e backup. | P0 |
| MC-GED-012 | OCR e busca full-text | Incompleto | Material registra ausencia. | Definir motor OCR, indexacao, idioma, permissao e custo. | P1 |
| MC-GED-013 | Denuncia publica de arquivo | Incompleto | file_report, admin e i18n existem; submissao publica nao esta confirmada. | Criar ou confirmar formulario publico, captcha, evidencias e fluxo completo. | P0 |
| MC-GED-014 | Moderacao administrativa | Parcial | Reports, filtros, accepted/cancelled, status de arquivo e bloqueio por hash. | Definir workflow, SLA, notificacoes, motivo obrigatorio e dupla aprovacao para acao destrutiva. | P1 |
| MC-GED-015 | Preview e watermark | Parcial | Miniaturas, cache, metadados, watermark e tokens de embed aparecem. | Fechar formatos suportados, politicas de watermark, cache e seguranca de preview. | P1 |
| MC-GED-016 | Tags e classificacao | Parcial | Tags publicas e por recurso, anti-duplicidade e edicao por arquivo. | Definir taxonomia corporativa, permissoes e uso por modulos. | P2 |
| MC-GED-017 | Eventos e notificacoes | Parcial | Upload visivel, rename, copy, bulk download, store/delete e report possuem eventos. | Padronizar eventos do Epros, payloads e assinantes. | P1 |
| MC-GED-018 | API final | Incompleto | Material tem muitas operacoes, mas endpoints finais nao estao consolidados. | Criar OpenAPI para documentos, revisoes, arquivos, pastas, upload, download, report, admin e storage. | P0 |
| MC-GED-019 | Permissoes e segregacao | Parcial | Existem permissoes own/any, project files, admin/moderador, download e manage. | Consolidar matriz Ver/Criar/Editar/Excluir/Download/Share/Admin por papel e contexto. | P0 |
| MC-GED-020 | Integracao com modulos | Parcial | Projetos, contratos, RH, fiscal, assinatura, compliance e anexos sao referenciados. | Fechar contratos de integracao e ownership de arquivos por modulo. | P0 |
| MC-GED-021 | Carga de arquivos historicos | Incompleto | Material traz estruturas e caminhos diversos. | Definir inventario, saneamento, dedupe, mapeamento de owners, hashes e storage. | P1 |
| MC-GED-022 | Relatorios e auditoria | Parcial | Posicao geral, auditoria, checklist, storage, downloads e moderacao previstos. | Definir KPIs, filtros, retencao de logs e exportacao auditada. | P1 |

## 4. Itens P0 para validacao humana

| ID | Decisao necessaria | Impacto se nao decidir |
|---|---|---|
| D-GED-001 | Modelo canonico unico para documento, arquivo, midia e anexo. | Risco de construir repositorios duplicados. |
| D-GED-002 | Politica de hash/senha segura. | Risco de seguranca e nao conformidade. |
| D-GED-003 | API final de upload/download/documentos/pastas. | Risco de contratos inconsistentes entre modulos. |
| D-GED-004 | Retencao legal e descarte por tipo documental. | Risco juridico e LGPD. |
| D-GED-005 | Estrategia de storage dinamico e multi-servidor. | Risco de indisponibilidade e perda de arquivo. |
| D-GED-006 | Fluxo completo de denuncia publica e moderacao. | Risco de abuso sem tratamento rastreavel. |
| D-GED-007 | Matriz de permissoes por contexto e papel. | Risco de vazamento ou bloqueio indevido. |

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

Esta MC olha para frente: valida o que falta construir, decidir ou homologar para GED virar a camada documental definitiva do Epros.
