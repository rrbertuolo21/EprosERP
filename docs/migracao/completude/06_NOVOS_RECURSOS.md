# 06 — Novos Recursos (Novo tem, Legado não tem)

> Auditoria READ-ONLY inversa. Lista tudo que o sistema **NOVO** (`EprosERP` — backend `src/` + frontend `EprosApp`) possui e o **LEGADO** (`Epros/epros_erp-main` + `epros_erp_front-main`) não possui.
> Data: 2026-07. Escopo: módulos de negócio novos, recursos técnicos/cross-cutting, telas e endpoints sem correspondente no legado.
> **Foco de ação:** a coluna "Proposta de alocação na tela" indica onde cada recurso ainda-não-exposto deveria aparecer, como evolução natural do menu atual (`EprosApp/components/menu.ts`), sem quebrar a familiaridade do cliente.

---

## Contexto: o que o legado tinha

O legado era um **ERP fiscal-transacional single-tenant** com foco em NF-e/NFC-e, estoque, compras, financeiro e cadastros. Domínios legados (`Epros.ERP.Domain/Entities`): Cadastros, Compras, Configuracoes, Contabeis, Estoque, Financeiros, Fiscais, Importacoes, Permissoes, Tributarios, Usuarios, Vendas.

Confirmado por busca no legado (0 resultados): **sem** multi-tenant/TenantId, **sem** hash de senha (PBKDF2/BCrypt), **sem** Outbox, **sem** soft-delete. Tinha permissões básicas (`PerfilUsuario`, `Menu`, `MenuItemNivel1/2`) — porém sem o motor de cache/impersonação do novo. O legado tinha `TenantsController` (Finbuckle) e `Gc*` (billing) apenas como esqueleto, não como plataforma SaaS self-service.

O novo transforma isso numa **suíte ERP multi-módulo + plataforma SaaS multi-tenant**.

---

## 1. Módulos de negócio totalmente NOVOS (sem correspondente no legado)

Todos têm backend completo (CQRS: Controllers em `src/API/Epros.API/Controllers/`, entidades em `src/Modules/*/Domain/Entities/`, jobs de Outbox próprios). **Nenhum tem tela nem entrada no menu lateral hoje.**

| Recurso novo | O que faz | Exposto onde hoje | Proposta de alocação na tela | Prioridade |
|---|---|---|---|---|
| **Produção (PCP/Manufatura)** — `ProducaoController` (`api/v1/producao`); entidades ListaMateriais(BOM), BomItem, OrdemProducao, ApontamentoProducao | Lista de materiais (BOM) e ordens de produção com apontamentos (iniciar/apontar/encerrar) | NÃO exposto na UI (só API) | Novo grupo de menu **"Produção"** (ícone 🏭), com itens "Ordens de Produção" e "Lista de Materiais (BOM)"; posicionar após Estoque | Alta |
| **Qualidade** — `InspecoesController` + `NaoConformidadesController` (`api/v1/qualidade/*`); entidades InspecaoLote, NaoConformidade | Inspeção de lotes e tratamento de não conformidades | NÃO exposto na UI (só API) | Novo grupo **"Qualidade"** (ícone ✅): "Inspeções" e "Não Conformidades"; junto de Produção/Estoque | Alta |
| **RH / Folha** — `RHController` (`api/v1/rh`); entidades Colaborador, FolhaPagamento, FolhaPagamentoVerba, Timesheet | Admissão/desligamento, timesheet e processamento de folha de pagamento | NÃO exposto na UI (só API) | Novo grupo **"RH"** (ícone 👥): "Colaboradores", "Timesheet", "Folha de Pagamento"; no fim do menu, antes de Configurações | Alta |
| **Manutenção de Ativos** — `ManutencaoController` (`api/v1/manutencao`); entidades Equipamento, OrdemManutencao, OrdemManutencaoPeca | Cadastro de equipamentos e ordens de manutenção com apontamento de peças | NÃO exposto na UI (só API) | Novo grupo **"Manutenção"** (ícone 🔧): "Equipamentos" e "Ordens de Manutenção"; próximo de Produção | Média |
| **Projetos** — `ProjetosController` (`api/v1/projetos`); entidades Projeto, WbsItem, AlocacaoRecurso | Gestão de projetos com WBS, alocação de recursos e progresso de tarefas | NÃO exposto na UI (só API) | Novo grupo **"Projetos"** (ícone 📋): "Projetos", "WBS/Cronograma", "Alocação de Recursos" | Média |
| **DMS (Concessionária/Oficina)** — `DMSController` (`api/v1/dms`); entidades VendaVeiculo, OrdemServicoDms, GarantiaMontadora | Venda de veículos, ordens de serviço e garantias de montadora | NÃO exposto na UI (só API) | Grupo **"Automotivo / DMS"** (ícone 🚗) — módulo vertical opcional, exibir só p/ tenants do segmento: "Veículos", "Ordens de Serviço", "Garantias" | Baixa |
| **GRC (Governança, Risco, Compliance)** — `GRCController` (`api/v1/grc`); entidades RiscoCorporativo, ControleInterno, Denuncia, IncidenteCompliance | Matriz de riscos, controles internos, canal de denúncias e incidentes de compliance | NÃO exposto na UI (só API) | Grupo **"Compliance / GRC"** (ícone 🛡️) — vertical: "Matriz de Riscos", "Controles", "Canal de Denúncias", "Incidentes" | Baixa |
| **ESG (Sustentabilidade)** — `ESGController` (`api/v1/esg`); entidades EmissaoCarbono, RelatorioESG | Registro de emissões de carbono e relatórios ESG consolidados | NÃO exposto na UI (só API) | Grupo **"ESG"** (ícone 🌱) — vertical: "Emissões" e "Relatórios ESG"; ou sub-item em Relatórios | Baixa |

---

## 2. Novos documentos fiscais (evolução do módulo Fiscal/Vendas)

O legado só emitia NF-e / NFC-e. O novo adiciona backend (controllers + composables `useCte`, `useMdfe`, `useNfse`, `useCstIbsCbs`) para:

| Recurso novo | O que faz | Exposto onde hoje | Proposta de alocação na tela | Prioridade |
|---|---|---|---|---|
| **CT-e** — `CteController` + `useCte.ts` | Conhecimento de Transporte eletrônico | Composable existe; sem página | Menu **Vendas/Emissão** → item "CT-e" (ao lado de NF-e/NFC-e) | Média |
| **MDF-e** — `MdfeController` + `useMdfe.ts` | Manifesto de Documentos Fiscais eletrônico | Composable existe; sem página | Menu **Vendas/Emissão** → item "MDF-e" | Média |
| **NFS-e** — `NfseController` + `useNfse.ts` | Nota Fiscal de Serviço eletrônica | Composable existe; sem página | Menu **Vendas/Emissão** → item "NFS-e" (integra com Cadastro de Serviços já existente) | Média |
| **CST IBS/CBS (Reforma Tributária)** — `CstsIbsCbsController` + `useCstIbsCbs.ts` | Códigos de Situação Tributária de IBS/CBS (reforma 2026+) | Composable existe; usado no cálculo; sem tela de manutenção | Menu **Fiscal** → item "CST IBS/CBS" (ao lado de CFOP/NCM Tributação) | Média |
| **Documentos Fiscais (central)** — `DocumentosFiscaisController` | Consulta unificada de documentos fiscais emitidos | NÃO exposto na UI | Reaproveitar tela **Vendas → Transmissões** ou nova aba "Documentos Fiscais" | Baixa |
| **Pedidos de Venda** — `PedidosController` (`api/v1/pedidos`) | Pedido de venda (pré-nota) | NÃO exposto na UI | Menu **Vendas** → "Pedidos" (antes da emissão da NF-e) | Média |
| **Cupons de desconto** — `CuponsController` | Cupons/descontos promocionais | NÃO exposto na UI | Sub-item em **Vendas** ou **Configurações**; e no fluxo do PDV | Baixa |

---

## 3. Plataforma SaaS / Backoffice (Landlord) — NOVO

Camada inteira inexistente no legado. Parte já tem tela sob `pages/plataforma/*` (rotas fora do menu do ERP, acessadas pelo super-admin/revenda).

| Recurso novo | O que faz | Exposto onde hoje | Proposta de alocação na tela | Prioridade |
|---|---|---|---|---|
| **Painel Landlord Backoffice** | Gerência de assinaturas, tenants, equipe interna, config global | `pages/plataforma/admin.vue` (rota `/plataforma/admin`) | Já exposto; manter fora do menu ERP (área de plataforma). Adicionar link no header só p/ perfil super-admin | Alta |
| **Gestão de Clientes (Tenants)** — `ClientesController` | Carteira de inquilinos SaaS, status de faturamento, limites | `pages/plataforma/admin/clientes/*` | Já exposto (backoffice) | Alta |
| **Revendas** — `RevendasController` | Revendas parceiras, comissões recorrentes | `pages/plataforma/admin/revendas.vue` | Já exposto (backoffice) | Média |
| **Vendedores** — `VendedoresController` | Equipe interna de vendas da plataforma | `pages/plataforma/admin/vendedores.vue` | Já exposto (backoffice) | Média |
| **Geografia** — `GeografiaController` | Países, estados, municípios, histórico CEP, zonas de entrega | `pages/plataforma/geografia.vue` | Já exposto (backoffice) | Média |
| **Assinaturas do tenant** — `AssinaturasController` (`api/v1/aplicativo/assinaturas`) | Assinatura vigente, contratar plano, faturas, PIX | Backend + `pages/area-cliente/*` parcial; sem tela de contratação completa | **Área do Cliente** (`/area-cliente`) → aba "Minha Assinatura / Planos" (já há `planos.vue`, `minhas-faturas.vue`) | Alta |
| **Contratos** — `ContratosController` | Contratos e itens de contrato SaaS | NÃO exposto na UI | Backoffice → aba em cliente/tenant; ou Área do Cliente | Média |
| **Pedidos SaaS** — `PedidosController` (aplicativo) | Pedido de contratação (checkout) | NÃO exposto na UI | Fluxo de contratação na Área do Cliente / landing | Média |
| **Onboarding** — `OnboardingController` | Fluxo de primeiro acesso / setup do tenant | NÃO exposto na UI | Wizard pós-cadastro (após `cadastro.vue`) | Média |
| **Installation / Setup** — `InstallationController` | Instalação/bootstrap inicial do sistema | NÃO exposto na UI | Tela de setup inicial (self-hosted) ou uso interno | Baixa |
| **Super Admin** — `SuperAdminController` | Ações privilegiadas cross-tenant, execução em massa, impersonação | Parcial no painel Landlord | Backoffice → aba "Super Admin" | Alta |
| **Área Pública / Landing / Marketplace** — `AreaPublicaController`, entidades LandingPageSettings, MarketplaceSettings | Config de landing page e marketplace | NÃO exposto na UI | Backoffice → "Configurações Globais / Landing" | Baixa |
| **Newsletter** — NewsletterSubscriber + `ExpurgarNewsletterInativaJob` | Captação de newsletter na landing | NÃO exposto na UI | Landing pública + backoffice | Baixa |

---

## 4. RBAC / Permissões avançadas — NOVO (evolução das permissões legadas)

O legado tinha PerfilUsuario/Menu básicos. O novo reescreve com granularidade e cache.

| Recurso novo | O que faz | Exposto onde hoje | Proposta de alocação na tela | Prioridade |
|---|---|---|---|---|
| **Perfis de Acesso granulares** — `PerfisAcessoController`; entidades PerfilAcesso, PerfilAcessoMenu (flags Ver/Editar/Excluir por item de menu) | Perfis com permissão por item de menu (níveis 1/2) e ação | **Exposto**: `pages/erp/configuracoes/permissoes/perfis/*` + componente `PermissaoMenuTree.vue` | Já exposto (menu Configurações → Perfis de Acesso) | — |
| **Catálogo de Menu** — `MenuCatalogoController` / `MenusController` | Fonte de verdade dos itens de menu p/ montar permissões | Consumido pela tela de perfis | Já exposto indiretamente | — |
| **Cache de permissões** — `PermissaoCacheManager` (30min, invalidação agrupada por usuário/empresa/perfil) | Performance do RBAC | Transparente (infra) | N/A (infra) | — |
| **Impersonação** — `SessaoImpersonacao` + `ImpersonacaoIniciadaEvent` | Super-admin assume um tenant p/ suporte | Backend; parcial no Landlord | Backoffice → botão "Acessar como" no cliente | Média |
| **API Keys / Personal Access Tokens** — `PersonalAccessToken` + `ApiKeyMiddleware` (rotação + rate limit por chave) | Autenticação máquina-a-máquina com expiração e limite/min | NÃO exposto na UI | **Configurações** → nova tela "Tokens de API / Integrações" | Média |

---

## 5. Recursos técnicos / Cross-cutting — NOVO (infra, sem tela própria)

Nenhum existia no legado (confirmado). São transparentes ao usuário, mas alguns merecem indicadores/telas de status.

| Recurso novo | O que faz | Exposto onde hoje | Proposta de alocação na tela | Prioridade |
|---|---|---|---|---|
| **Multi-tenant (SaaS) completo** — `EntidadeSaaSBase` (TenantId em toda entidade), `ITenantProvider`, `InquilinoSaaSMiddleware`, filtro global por tenant no `ContextBase` | Isolamento de dados por inquilino | Infra (JWT Keycloak `tenantId` / header `X-Tenant-Id`) | N/A visual; indicador de tenant/empresa ativa no header | — |
| **Row-Level Security no Postgres** — `TenantRlsInterceptor` (`SET app.current_tenant_id`), `EprosMigrationsSqlGenerator` (cria POLICY tenant_isolation automática) | Defesa em profundidade no banco | Infra | N/A | — |
| **Modo Demo com bloqueio de escrita** — `OperacaoBloqueadaModoDemoException`, flag em `TenantProvider.EhTenantDemo()` | Tenant demo só-leitura | Infra; poderia ter aviso na UI | Banner "Ambiente Demonstração" no topo quando tenant demo | Média |
| **Limites de plano / Entitlements** — `ValidadorLimitesSaaS` (limite usuários/empresas), `ModuloTenantMiddleware` (403 módulo não habilitado) | Faz valer o plano contratado | Infra (HTTP 403) | Mensagem de upgrade ao atingir limite (ligar em Assinatura/Planos) | Média |
| **Bloqueio por inadimplência** — `BloqueioInadimplenciaMiddleware` (HTTP 402 se fatura vencida >15d) | Suspende tenant inadimplente | Infra (HTTP 402) | Tela/banner "Pagamento pendente" + link p/ faturas | Média |
| **Outbox Pattern (por módulo)** — `OutboxMessage`, `DomainEvent`, 9 `*OutboxProcessorJob` (Quartz, 10s) | Publicação transacional de eventos de integração (evita dual-write) | Infra | N/A (talvez monitor no backoffice) | — |
| **CQRS + MediatR + Behaviors** — bus de Command/Query; `MakerCheckerPipelineBehavior` (dry-run com rollback + aprovação super-admin p/ `IComandoRisco`) | Governança de comandos de risco | Infra | Backoffice → fila de aprovações (ExecucaoMassaGlobal) | Baixa |
| **Soft-delete + auditoria automática** — `ContextBase.ProcessarEntidadesSaaS` (DeletadoEm, CriadoPor/AlteradoPor/AlteradoEm, SyncVersion), `AuditMiddleware` (AUDIT_TRAIL, LGPD/PCI) | Nada é apagado fisicamente; trilha de auditoria | Infra | Backoffice → visualizador de trilha de auditoria; "lixeira" por tela | Baixa |
| **Hash de senha PBKDF2** — `Pbkdf2PasswordHasher` (HMAC-SHA256, 100k iterações, sem libs externas) | Armazenamento seguro de senha | Infra (login) | N/A | — |
| **Cofre de segredos (Vault)** — `VaultEncryptionService` (HashiCorp Vault Transit + fallback AES-256-GCM) | Criptografia de segredos (ex.: certificado, chaves) | Infra | N/A | — |
| **Mascaramento de dados sensíveis** — `DataMaskingMiddleware` (CPF, PAN de cartão em logs, PCI DSS) | Proteção de PII nos logs | Infra | N/A | — |
| **Sincronização offline / delta** — `ISyncable` (SyncId, SyncVersion), sync delta nos controllers de plataforma | Base p/ replicação/offline (mobile/PDV) | Infra; ligado ao app mobile | N/A | — |
| **Tratamento global de erros RFC 7807** — `ExcecaoGlobalMiddleware` (ProblemDetails + traceId) | Erros padronizados p/ correlação (Loki/Tempo) | Infra | N/A | — |
| **Observabilidade (Serilog estruturado)** — enrich com TenantId, AUDIT_TRAIL p/ Grafana Loki | Logs estruturados | Infra | N/A | — |
| **Jobs agendados (Quartz)** — VerificarFaturasVencidas, FaturamentoRecorrente, ReguaCobranca, ReajusteContrato, SincronizarGeografia, ExpiracaoSessoes | Automação de faturamento/cobrança/manutenção | Infra | Backoffice → "Jobs / Agendamentos" (status) | Baixa |
| **OpenAPI / Swagger + codegen** — Swashbuckle + `EprosApp/openapi` (tipos gerados) | Contrato de API e tipos TS gerados | Swagger em Dev; tipos no front | N/A | — |

**Ausentes no novo (não implementados):** HealthChecks/`MapHealthChecks`, OpenTelemetry instrumentado, `AddRateLimiter` nativo (há rate limit próprio só no `ApiKeyMiddleware`), versionamento de API formal (`Asp.Versioning`).

---

## 6. Frontend — recursos e telas novas

| Recurso novo | O que faz | Exposto onde hoje | Proposta de alocação na tela | Prioridade |
|---|---|---|---|---|
| **Tema claro/escuro** — `useTheme.ts` (light/dark, persistência localStorage, respeita prefers-color-scheme; default dark) | Alternância de tema (inexistente no legado) | Componível pronto; precisa do botão no header | Toggle de tema no **AppHeader** (ícone sol/lua) | Média |
| **Dashboard real do ERP** — `/erp/acesso-rapido` (dados reais via useApi); `/dashboard` vira redirect | Painel inicial com dados reais (legado só tinha mock ~7k linhas) | Exposto (`acesso-rapido.vue`) | Já exposto; adicionar item "Início/Dashboard" no topo do menu | Média |
| **PDV redesenhado** — `pages/erp/pdv/index.vue` + `components/pdv/*` + layout `pos.vue` + `useRealtime` | Frente de caixa NFC-e com transmissão em tempo real e overlay de progresso | **Exposto** (menu PDV → Caixa) | Já exposto | — |
| **Realtime / SignalR** — `useRealtime.ts` + `Epros.Erp.RealTime.API` | Progresso de transmissão SEFAZ em tempo real | Usado no PDV/transmissões | Já exposto indiretamente | — |
| **Reorganização de rotas sob `/erp/*` e `/plataforma/*`** — separa ERP do backoffice de plataforma | Navegação por contexto (inquilino vs landlord) | Estrutura de rotas nova | Mantida | — |

Telas do legado **removidas** no novo (não são recursos novos, mas notar): showcase de design system `dev/ui/*` e `cadastros/produto/item/*`.

---

## Resumo executivo (o que precisa de alocação na UI)

**Módulos novos com backend pronto e ZERO tela (maior lacuna de UI):**
1. Produção (PCP/BOM) — **Alta**
2. Qualidade (Inspeções, Não Conformidades) — **Alta**
3. RH / Folha de Pagamento — **Alta**
4. Manutenção de Ativos — **Média**
5. Projetos (WBS) — **Média**
6. DMS, GRC, ESG — verticais opcionais — **Baixa**

**Documentos fiscais novos sem tela (composable pronto):** CT-e, MDF-e, NFS-e, CST IBS/CBS, Pedidos de Venda, Cupons — encaixar em Vendas/Emissão e Fiscal.

**SaaS / Backoffice:** Assinaturas/Planos e Super Admin já parcialmente expostos; falta finalizar contratação/onboarding e telas de Contratos, API Tokens, fila de aprovações (Maker-Checker) e trilha de auditoria.

**Frontend rápido de ganhar:** botão de **tema claro/escuro** no header (composable já existe), banner de **modo Demo** e banner de **inadimplência/limite de plano** (backends já retornam 402/403).

**Infra transparente (sem tela, apenas registrar):** multi-tenant + RLS Postgres, Outbox, CQRS/Maker-Checker, soft-delete+auditoria, PBKDF2, Vault, data masking, Serilog/ProblemDetails, Quartz jobs, OpenAPI codegen — nenhum existia no legado.
