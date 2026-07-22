# Mapa Mestre — Módulo 0_APLICATIVO (Reconciliação EF × Código)

**Data:** 2026-07-22
**Fontes:**
- Specs: `EspecificaçõesPlataforma/0_APLICATIVO/` (11 submódulos)
- Código: `src/Modules/Epros.Modules.Aplicativo/`, `src/Modules/Epros.Modules.GestaoClientes/`, `src/API/Epros.API/Controllers/`, `src/API/Epros.API/Security/`, `src/Shared/`, `src/Infrastructure/`

## Observação arquitetural central

As specs do módulo APLICATIVO cruzam **dois** módulos de código:
- **`Epros.Modules.Aplicativo`** — identidade/tenant, usuários, sessões, super-admin, onboarding-config, instalação, área pública, execução em massa.
- **`Epros.Modules.GestaoClientes`** — todo o catálogo SaaS comercial: `Plano`, `GrupoPlano`, `ModuloPlano`, `AssinaturaCliente`, `PedidoSaaS`, `Cupom`, `Fatura`, `Menu`/`MenuItemNivel1`/`MenuItemNivel2`, `PerfilAcesso`/`PerfilAcessoMenu`, `Empresa`, `Pais`/`Moeda`/`Municipio`.

O isolamento de dados é implementado em `src/Infrastructure/Epros.Infrastructure/Data/ContextBase.cs` (query filters por `TenantId` + soft-delete) sobre a base `src/Shared/Epros.Shared/Domain/Entities/EntidadeSaaSBase.cs`.

## Tabela resumo

| Submódulo | Status | Entidades faltantes (nº) | Tier | Gap #1 |
|---|---|---|---|---|
| ASSINATURA_E_PLANOS | PARCIAL | ~6 | G | Sem catálogo global de plano (flags CRM/Projetos/RH/Contas/PDV, duração, limites clientes/fornecedores/produtos/faturas); snapshot de assinatura mínimo |
| CATALOGOS_GLOBAIS_SAAS | PARCIAL | ~4 | M | `IGlobalEntity` é interface vazia sem enforcement; faltam `Funcionalidade`/add-on, `ConfiguracaoPublica`, resolução de módulos ativos |
| DASHBOARD_E_LAYOUT | AUSENTE | ~8 | G | Nenhuma entidade/query de widget/BI; só existe dashboard super-admin agregado |
| IDENTIDADE_E_CONTEXTO_TENANT | PARCIAL | ~2 | M | SSO/login social e MFA declarados mas não implementados; lockout/banimento parcial |
| ISOLAMENTO_DE_DADOS | DONE | 0 | P | Falta enforcement de owner/tenant em Settings e rotina banco-por-tenant (arquitetura shared-db) |
| LIMITES_DE_PLANO | PARCIAL | ~2 | M | `ValidadorLimitesSaaS` cobre só usuários e empresas; faltam produtos/faturas/clientes/fornecedores |
| ONBOARDING_E_EMPRESA | PARCIAL | ~2 | M | Registro do tenant no domínio de gestão de assinatura (RF-6.6) não encadeia criação de Cliente SaaS |
| OPERACAO_SUPER_ADMIN | PARCIAL | ~1 | M | `UpdateLog`/atualização existe como entidade, mas fluxo de upgrade/governança incompleto |
| PEDIDOS_E_COBRANCA_SAAS | PARCIAL | ~2 | G | Webhook/gateway são stubs; conciliação, sessão de pagamento e reembolso incompletos |
| PERMISSOES_DE_MENU | PARCIAL | 0 | M | Modelo menu/perfil/acesso completo; falta endpoint de "acessos do usuário" (AcessosResponse) e catálogo de menu completo |
| USUARIOS_E_PAPEIS | PARCIAL | ~5 | G | Modelo é `PerfilAcesso`+`PerfilAcessoMenu` (RBAC por menu); faltam `papel`, `capacidade`, `usuario_papel`, `nivel_usuario`, `preco_nivel_usuario` |

Legenda Tier: P (<1 dia-agente) · M · G.

---

## 1. ASSINATURA_E_PLANOS — PARCIAL — Tier G

**EF:** `EspecificaçõesPlataforma/0_APLICATIVO/ASSINATURA_E_PLANOS/EF_0_APLICATIVO_ASSINATURA_E_PLANOS_V1.md`

### Entidades exigidas × presentes
| EF exige | Código | Local |
|---|---|---|
| Plano | Plano (parcial) | `GestaoClientes/Domain/Entities/Plano.cs` |
| Catálogo global de plano (5.1.1: duração, flags CRM/Projetos/RH/Contas/PDV, limites clientes/fornecedores/produtos/faturas) | **AUSENTE** | — |
| Grupo de plano | GrupoPlano | `GestaoClientes/Domain/Entities/GrupoPlano.cs` |
| Módulo geral / Módulo do plano | ModuloPlano (só nome+plano) | `GestaoClientes/Domain/Entities/ModuloPlano.cs` |
| Assinatura do cliente + snapshot | AssinaturaCliente (`DetalhesPacoteJson`) | `GestaoClientes/Domain/Entities/AssinaturaCliente.cs` |
| Cliente assinante | Cliente | `GestaoClientes/Domain/Entities/Cliente.cs` |
| Fatura / Pagamento de fatura | Fatura / PagamentoFatura | `GestaoClientes/Domain/Entities/` |
| Composição de faturamento / Histórico reajuste | ComposicaoFaturamento / HistoricoReajuste | idem |
| Quantidade de permissão (tipo 0=empresas,1=usuários) | **AUSENTE como entidade dedicada** | validação em `ValidadorLimitesSaaS` |
| Revenda / Vendedor | Revenda / Vendedor | idem |
| Empresa operadora | Empresa | `GestaoClientes/Domain/Entities/Empresa.cs` |
| UpgradePlano (encadeamento) | UpgradePlano | `GestaoClientes/Domain/Entities/UpgradePlano.cs` |

### GAPS principais
- `Plano` só tem `Nome, Preco, Ativo, GrupoPlanoId, LimiteUsuarios, LimiteEmpresas, RecursosInclusos, Modulos`. Faltam: descrição curta/completa, valor mensal/anual, opção gratuito/pago, IDs em gateways, limites de clientes/equipes/projetos, destaque, status de sincronização, data início/fim — todos exigidos em 5.1.
- **Catálogo global de plano (5.1.1)** inexistente: flags CRM/Projetos/RH/Contas/PDV, duração (vitalícia/mensal/anual), limites de fornecedores/produtos/faturas.
- Snapshot de assinatura é `DetalhesPacoteJson` livre — sem garantia dos campos preservados de 5.2.2.
- Endpoints existentes (`AssinaturasController.cs`): `GET vigente`, `POST contratar`, `GET faturas`, `POST faturas/{id}/pix`, `GET public/planos` (`AreaPublicaController.cs` → `ListarPlanosPublicosQuery`). Faltam: troca de plano, arquivar/restaurar, sincronização de limites/módulos, alertas de expiração, encadeamento de vigência.
- Regras 6.5 (bloqueio por inadimplência 15 dias), 6.11 (estados) e 6.12 (encadeamento) não têm evidência de implementação completa.

### Contratos cross-module
- **Publica:** eventos Outbox de ciclo do cliente (RF-014) — no código só há `FaturaAlertaCobrancaEvent`, `ComissaoApuradaEvent` (`GestaoClientes`). Faltam Cliente/Assinatura criado/sincronizado.
- **Consome:** configurações de gateway (`SystemSetting` escopo `gateway`), `Pais`/`Municipio`/`Moeda`.

---

## 2. CATALOGOS_GLOBAIS_SAAS — PARCIAL — Tier M

**EF:** `.../CATALOGOS_GLOBAIS_SAAS/EF_0_APLICATIVO_CATALOGOS_GLOBAIS_SAAS_V1.md` (seção 11: Pais, Moeda, Funcionalidade, Cupom, Configuração pública, Tipo de pagamento, Add-on/módulo, Módulo ativo por contexto, Módulos do plano).

### Entidades exigidas × presentes
| EF exige | Código |
|---|---|
| Pais | `GestaoClientes/Domain/Entities/Pais.cs` (+ `ListarPaisesQuery`) |
| Moeda | `GestaoClientes/Domain/Entities/Moeda.cs` |
| Cupom / Uso de cupom | `Cupom.cs`, `UsoCupom.cs` |
| Módulos do plano | `ModuloPlano.cs` |
| Funcionalidade | **AUSENTE** |
| Configuração pública (site) | Parcial: `LandingPageSettings.cs`, `MarketplaceSettings.cs`, `CustomPage.cs` (Aplicativo) |
| Add-on / módulo comercializável | **AUSENTE** |
| Módulo ativo por usuário/contexto (resolução) | **AUSENTE** |
| Tipo de pagamento | implícito em enums de `PedidoSaaS`/`PagamentoFatura` |

### GAPS principais
- `IGlobalEntity` (`src/Shared/Epros.Shared/Domain/Entities/IGlobalEntity.cs`) é **interface vazia (marker)** sem qualquer enforcement — o único implementador é `CodigoPostalCache`. Catálogos globais (Pais, Moeda) não implementam a marker e continuam sob query filter de tenant em `ContextBase.cs` a menos que registrados como não-tenantizados.
- Falta modelo de **add-on/Funcionalidade** e **resolução de módulos ativos** (seções 7.3/7.4/13.3/13.4).
- Configuração pública do site dispersa em 3 entidades no módulo Aplicativo, sem o modelo unificado da EF.

### Contratos cross-module
- **Publica:** catálogos globais (Pais/Moeda) consumidos por todos os módulos; deveriam ser `IGlobalEntity`.
- **Consome:** —

---

## 3. DASHBOARD_E_LAYOUT — AUSENTE — Tier G

**EF:** `.../DASHBOARD_E_LAYOUT/EF_0_APLICATIVO_DASHBOARD_E_LAYOUT_V1.md` (seção 11: Widget Siser, Lançamento p/ dashboard, Transação recente, Atalho operacional, Total por período, Série gráfica, Estoque BI, Meta de vendedor, Serviço vendido, Relatórios, Consulta paginada, Home/feed/conectores).

### Entidades exigidas × presentes
Nenhuma das entidades de widget/BI/atalho/série existe no código.

### GAPS principais
- Não há entidades, queries ou controllers de dashboard operacional/BI no módulo Aplicativo.
- Único ponto relacionado: `SuperAdminController.cs` `GET dashboard` → `SuperAdminQueries.cs`/`SuperAdminDtos.cs` — dashboard **agregado do super-admin**, não os dashboards de domínio (vendas/compras/financeiro/estoque/fiscal) da EF (7.6–7.10).
- Layout autenticado, validação de licença no layout (7.2), painel operacional da empresa, atalhos, home/busca/feed — todos ausentes.

### Contratos cross-module
- **Consome (previsto):** Lookups de Vendas/Compras/Financeiro/Estoque (`VendasLookups`, `PessoaLookup`, `FinanceiroLookups`, `ServicoLookup`) para alimentar BI — hoje sem consumidor.

---

## 4. IDENTIDADE_E_CONTEXTO_TENANT — PARCIAL — Tier M

**EF:** `.../IDENTIDADE_E_CONTEXTO_TENANT/EF_0_APLICATIVO_IDENTIDADE_E_CONTEXTO_TENANT_V1.md` (11.1–11.12: Usuario, UsuarioEmpresa, PerfilUsuario, PerfilUsuarioAcesso, Menu, HistoricoLogin, Token API, Sessao, banimento, Empresa/Acesso no contexto de auth).

### Entidades exigidas × presentes
| EF exige | Código |
|---|---|
| Usuario | `Aplicativo/Domain/Entities/Usuario.cs` (rico: status, tipo, MFA flag, lockout, forgot-pwd token, ApiKey+RateLimit) |
| UsuarioEmpresa | `UsuarioEmpresa.cs` (Usuario, Empresa, PerfilAcesso, EhAdmin) |
| PerfilUsuario / PerfilUsuarioAcesso | `GestaoClientes/.../PerfilAcesso.cs`, `PerfilAcessoMenu.cs` |
| Menu + itens | `GestaoClientes/.../Menu.cs`, `MenuItemNivel1.cs`, `MenuItemNivel2.cs` |
| HistoricoLogin | `Aplicativo/.../HistoricoLogin.cs` |
| Token de acesso API | `PersonalAccessToken.cs` + `Usuario.ApiKey*` |
| Sessão | `SessaoUsuario.cs` (token, IP, UA, expiração, revogado) |
| Banimento de login | `BannedIp.cs` |
| Impersonação | `SessaoImpersonacao.cs` + `ImpersonacaoEventHandler.cs` |

### GAPS principais
- **SSO / login social / provedores externos (7.7)** — sem implementação.
- **MFA** — só flag `MfaHabilitado` em `Usuario`, sem fluxo.
- Fluxo de login implementado (`AuthController.cs`): login público, selecionar-empresa, recuperar/resetar/alterar senha, registrar-tenant, empresas-disponíveis. Contexto completo emitido pós-seleção de empresa (`OnboardingController.cs` `GET onboarding/sessao/contexto`).
- Lockout/`AccessFailedCount` presente em `Usuario`, mas política de banimento por tentativas (7.6) parcial.

### Contratos cross-module
- **Publica:** `UsuarioCriado`, `UsuarioAtualizado`, `UsuarioDeletado`, `ImpersonacaoIniciada` (Outbox, `UsuarioHandlers.cs`).
- **Consome:** `PerfilAcesso`/`Menu` de GestaoClientes (via `PermissaoMenuFilter.cs`).

---

## 5. ISOLAMENTO_DE_DADOS — DONE — Tier P

**EF:** `.../ISOLAMENTO_DE_DADOS/EF_0_APLICATIVO_ISOLAMENTO_DE_DADOS_V1.md` (11.1 ModeloBaseAuditavel, 11.2 EntidadeTenantizada, 11.3 Setting por owner/tenant, 11.4 Tenant físico, 11.5 soft-delete).

### Entidades/mecanismos exigidos × presentes
| EF exige | Código |
|---|---|
| ModeloBaseAuditável (Id, criado/alterado/deletado por+em) | `EntidadeSaaSBase.cs` (Id, SyncId, TenantId, CriadoEm/Por, AlteradoEm/Por, DeletadoEm) |
| Entidade tenantizada + atribuição automática de contexto | `ContextBase.cs`: `SaveChanges` seta `TenantId` automaticamente; query filter `TenantId == provider.GetTenantId() && DeletadoEm == null` |
| Filtro de consulta / proteção de leitura | `HasQueryFilter` em `ContextBase.cs:131-138` |
| Soft delete + auditoria | `EntidadeSaaSBase.Deletar()`, `IHardDeletable` para exceções (Sessao, Impersonacao, Newsletter) |
| Setting por owner/tenant | `SystemSetting.cs` (Escopo: global/landlord/tenant/gateway/install) |
| Tenant provider | `src/Shared/.../Application/Contracts/ITenantProvider.cs` |

### GAPS principais
- Arquitetura é **shared-database + discriminador TenantId** (não banco-por-tenant); a EF 7.6 menciona "banco por tenant e rotinas tenant-aware" — decisão a validar (o modelo atual é aceitável mas diverge da opção físico-por-tenant).
- Entidades não-tenantizadas (catálogos globais) ainda dependem de registro explícito para escapar do query filter — vide item CATALOGOS.

**Avaliação:** mecanismo central implementado e fiel. Marcado DONE com ressalva de decisão arquitetural.

---

## 6. LIMITES_DE_PLANO — PARCIAL — Tier M

**EF:** `.../LIMITES_DE_PLANO/EF_0_APLICATIVO_LIMITES_DE_PLANO_V1.md` (11.1–11.13: Tenant, Cliente, Plano, Módulo, Quantidade de Permissão, Fatura, Item, Composição, Reajuste, Revenda, Vendedor).

### Presente
- `Aplicativo/Application/Services/ValidadorLimitesSaaS.cs` (`IValidadorLimitesSaaS`): `PossuiFolgaUsuariosAsync`, `PossuiFolgaEmpresasAsync`, `ValidarLimiteUsuariosAsync`, `ValidarLimiteEmpresasAsync` — cruza `ContextAplicativo` × `ContextGestaoClientes`.

### GAPS principais
- Cobre **apenas usuários e empresas**. A EF (e o catálogo global) exige limites de **clientes, fornecedores, produtos, faturas, equipes, projetos** — sem enforcement.
- Não há entidade dedicada `QuantidadeDePermissao` (tipo 0/1); limites vêm de `Plano.LimiteUsuarios`/`LimiteEmpresas` diretamente.
- Comportamento "limite ausente = lacuna de config (bloqueia silêncio)" (6.2.5) não evidenciado.
- Convenção `-1 = ilimitado` (5.1) não tratada.

### Contratos cross-module
- **Consome:** `Plano` (limites), contagem de `Usuario`/`Empresa`. Ponto de integração para módulos de cadastro chamarem antes de exceder cota.

---

## 7. ONBOARDING_E_EMPRESA — PARCIAL — Tier M

**EF:** `.../ONBOARDING_E_EMPRESA/EF_0_APLICATIVO_ONBOARDING_E_EMPRESA_V1.md` (11.1 Registro de Tenant, 11.2 Empresa Operacional, 11.3 Endereço, 11.4 Parâmetros Fiscais, 11.5 Company/Config, 11.6 Ano Financeiro, 11.7 Usuário+Empresa, 11.8 Perfil/Acesso/Menu, 11.9 Config chave-valor, 11.10 Moeda/Geografia, 11.11 Armazém/Transportadora, 11.12 Idioma/Dicionário).

### Entidades exigidas × presentes
| EF exige | Código |
|---|---|
| Registro de Tenant | `RegistrarNovoTenantCommand` (`AuthCommands.cs`/`AuthHandlers.cs`) |
| Empresa Operacional + Parâmetros Fiscais | `GestaoClientes/.../Empresa.cs`, `EmpresaParametrosDfe.cs`, `EmpresaCertificado.cs` |
| Company/Config (chave-valor de empresa) | `Aplicativo/.../ConfiguracaoEmpresa.cs` (TimeZone, DateFormat, Currency, VAT, Logo, Favicon...) |
| Ano Financeiro | `AnoFinanceiro.cs` / `ExercicioFinanceiro.cs` |
| Config chave-valor | `SystemSetting.cs` |
| Idioma/Dicionário | `Idioma.cs` (Code, Name, CountryCode, Enabled) |
| Moeda/Geografia | `Moeda`, `Pais`, `Municipio` (GestaoClientes) |
| Instalação | `InstalacaoState.cs` + `InstallationController.cs` |

### GAPS principais
- **RF 6.6 (registro do cliente no domínio de gestão de assinatura)**: o `RegistrarNovoTenantCommand` cria tenant/usuário, mas não há evidência de encadeamento que crie/registre `Cliente` SaaS com plano/revenda/vendedor/empresa operadora parametrizáveis.
- Endpoints onboarding (`OnboardingController.cs`): salvar/consultar config de empresa, habilitar/desabilitar idiomas, contexto de sessão. Falta wizard multi-etapa completo e dicionário de tradução.
- `ConfiguracaoEmpresa` (Aplicativo) e `Empresa` (GestaoClientes) coexistem — duplicação a reconciliar.

### Contratos cross-module
- **Publica:** `UsuarioCriado`. **Consome:** catálogos (Pais/Moeda/Idioma), `Plano`.

---

## 8. OPERACAO_SUPER_ADMIN — PARCIAL — Tier M

**EF:** `.../OPERACAO_SUPER_ADMIN/EF_0_APLICATIVO_OPERACAO_SUPER_ADMIN_V1.md` (11.1 Usuario Interno Siser, 11.2 Config Global, 11.3 Comunicação Super Admin, 11.4 Área Pública Admin, 11.5 Execução em Massa, 11.6 Log, 11.7 Atualização e Log).

### Entidades exigidas × presentes
| EF exige | Código |
|---|---|
| Usuário Interno Siser | `Aplicativo/.../UsuarioInterno.cs` (PrimaryAdmin, CreatorId) |
| Configuração Global | `SystemSetting.cs` + `ConfiguracaoGlobal.cs` (GestaoClientes) |
| Comunicação Super Admin | `ComunicacaoSuperAdmin.cs` (BusinessIds, Canais, Assunto, Mensagem, Status) |
| Área Pública Admin | `LandingPageSettings.cs`, `MarketplaceSettings.cs`, `CustomPage.cs`, `NewsletterSubscriber.cs` |
| Execução em Massa + Log | `ExecucaoMassaGlobal.cs`, `LogExecucaoMassa.cs` |
| Atualização e Log | `UpdateLog.cs`, `InstalacaoState.cs` |

### GAPS principais
- Cobertura de endpoints ampla (`SuperAdminController.cs`, 362 linhas): aprovar assinatura manual, execução-massa (criar/simular/ativar/concluir/aprovar), config global, páginas (publicar/rascunho), newsletter (cancelar/reativar), settings landing/marketplace, comunicação, dashboard, usuários internos (criar/alterar-senha/tornar-admin).
- Fluxo de **atualização/upgrade do sistema** (11.7 / governança de versão) presente como entidade (`UpdateLog`) mas fluxo operacional incompleto.
- Execução em massa: pipeline `MakerCheckerPipelineBehavior.cs` (aprovação) + `ExecucaoMassaGlobalCommands`/`Simular` presentes — bom.

### Contratos cross-module
- **Publica:** `ComunicacaoSuperAdminCriada` (Outbox, `ComunicacaoSuperAdminHandlers.cs` tenant `system`).
- **Consome:** `Cliente`/`AssinaturaCliente` (aprovação manual), todos os módulos (execução em massa global).

---

## 9. PEDIDOS_E_COBRANCA_SAAS — PARCIAL — Tier G

**EF:** `.../PEDIDOS_E_COBRANCA_SAAS/EF_0_APLICATIVO_PEDIDOS_E_COBRANCA_SAAS_V1.md` (11.1 Pedido SaaS, 11.2 Cupom/Uso, 11.3 Fatura, 11.4 Pagamento, 11.5 Composição/Reajuste, 11.6 Comprovante/Sessão/Rotina).

### Entidades exigidas × presentes
| EF exige | Código |
|---|---|
| Pedido SaaS | `GestaoClientes/.../PedidoSaaS.cs` (ValorBase/Desconto/Total, Moeda, MetodoPagamento, Status, AssinaturaCriadaId; `Liquidar/MarcarFalha/MarcarReembolsado`) |
| Cupom / Uso de Cupom | `Cupom.cs`, `UsoCupom.cs` |
| Fatura / Pagamento | `Fatura.cs`, `PagamentoFatura.cs` |
| Composição / Reajuste | `ComposicaoFaturamento.cs`, `HistoricoReajuste.cs` |
| Comprovante / Sessão pagamento / Transferência | `ComprovantePagamento.cs`, `SessaoPagamento.cs`, `PagamentoTransferencia.cs`, `PagamentoGlobal.cs` |

### GAPS principais
- Endpoints (`PedidosController.cs`): criar pedido, listar, checkout, transferência (criar/analisar/pendentes), **webhook**. Estrutura presente.
- **Integração real com gateways** (PIX/Mercado Pago/Stripe/etc.) é stub — geração de QR/URL, conciliação por webhook e reembolso não confirmados como funcionais.
- Fluxo pedido→fatura→assinatura parcial (`PedidoSaaS.Liquidar(assinaturaId)` existe, mas orquestração completa por confirmar).

### Contratos cross-module
- **Publica:** `FaturaAlertaCobrancaEvent`, `ComissaoApuradaEvent` (Outbox).
- **Consome:** `Plano`, `Cupom`, config de gateway (`SystemSetting` escopo `gateway`).

---

## 10. PERMISSOES_DE_MENU — PARCIAL — Tier M

**EF:** `.../PERMISSOES_DE_MENU/EF_0_APLICATIVO_PERMISSOES_DE_MENU_V1.md` (11.1 menu, 11.2/11.3 menu_item_nivel1/2, 11.4 perfil_usuario, 11.5 perfil_usuario_acesso, 11.6 usuario, 11.7 usuario_empresa; contratos AcessoItem/Acesso/AcessosResponse/AuthResponse/sessionReturn).

### Entidades exigidas × presentes
| EF exige | Código |
|---|---|
| menu / nivel1 / nivel2 | `Menu.cs`, `MenuItemNivel1.cs`, `MenuItemNivel2.cs` |
| perfil_usuario | `PerfilAcesso.cs` |
| perfil_usuario_acesso (Ver/Editar/Excluir por menu+nível) | `PerfilAcessoMenu.cs` (Ver, Editar, Excluir, MenuId, Nivel1Id?, Nivel2Id?) |
| usuario / usuario_empresa | `Usuario.cs`, `UsuarioEmpresa.cs` (PerfilAcessoId, EhAdmin) |

### GAPS principais
- **Enforcement implementado e fiel**: `src/API/Epros.API/Security/PermissaoMenuFilter.cs` + `PermissaoMenuAttribute.cs` + `AbacFilter.cs`/`AbacAuthorizeAttribute.cs` — resolve perfil→acessos, checa Ver/Editar/Excluir por Menu/Nivel1/Nivel2, bypass para admin.
- CRUD de perfis: `PerfisAcessoController.cs` (list/get/post/put/delete + sincronizar acessos via `PerfilAcesso.SincronizarAcessos`).
- Catálogo de menu: `MenuCatalogoController.cs` só `GET menus/paginado` e `GET menus/{id}` — **falta manutenção (CRUD) do catálogo de menu** e seed completo.
- **Contrato `AcessosResponse`/`Acesso`/`AcessoItem`** (árvore de permissões do usuário logado para o front) — não há endpoint dedicado que devolva a estrutura de menu permitida ao usuário.

### Contratos cross-module
- **Consome:** `Usuario`/`UsuarioEmpresa` (Aplicativo) × `PerfilAcesso`/`Menu` (GestaoClientes) — acoplamento cross-context em `PermissaoMenuFilter`.

---

## 11. USUARIOS_E_PAPEIS — PARCIAL — Tier G

**EF:** `.../USUARIOS_E_PAPEIS/EF_0_APLICATIVO_USUARIOS_E_PAPEIS_V1.md` (11.1 usuario, 11.2 usuario_empresa, 11.3 perfil_usuario, 11.4 papel, 11.5 capacidade, 11.6 usuario_papel, 11.7 papel_capacidade, 11.8 usuario_capacidade, 11.9 historico_login, 11.10 preferencia_usuario, 11.11 nivel_usuario, 11.12 preco_nivel_usuario, 11.13 sessao_impersonacao).

### Entidades exigidas × presentes
| EF exige | Código | Situação |
|---|---|---|
| usuario | `Usuario.cs` | OK |
| usuario_empresa | `UsuarioEmpresa.cs` | OK |
| perfil_usuario | `PerfilAcesso.cs` | OK (modelo por menu) |
| historico_login | `HistoricoLogin.cs` | OK |
| preferencia_usuario | `PreferenciaUsuario.cs` | OK (idioma, tema, avatar, formatos) |
| sessao_impersonacao | `SessaoImpersonacao.cs` | OK |
| **papel** | — | **AUSENTE** |
| **capacidade** | — | **AUSENTE** (existe `UsuarioPermissao` Recurso/Ação, mas ligado a `PerfilColaborador`, não catálogo de capacidade) |
| **usuario_papel** | — | **AUSENTE** |
| **papel_capacidade** | — | **AUSENTE** |
| **usuario_capacidade** | — | **AUSENTE** |
| **nivel_usuario** | — | **AUSENTE** |
| **preco_nivel_usuario** | — | **AUSENTE** |

### GAPS principais
- O código implementa **RBAC baseado em perfil×menu** (`PerfilAcesso`/`PerfilAcessoMenu`), não o modelo **papel/capacidade** da EF. Divergência de modelo, não só de campos.
- `UsuarioPermissao.cs` (Recurso/Ação/Permitido) é o esboço mais próximo de "capacidade", mas atrelado a `PerfilColaboradorId` e sem catálogo de capacidades nem vínculo papel↔capacidade.
- **Níveis de usuário e precificação por nível** (11.11/11.12) — sem qualquer implementação.
- Endpoints de usuário completos (`UsuariosController.cs`): CRUD, nova-senha, histórico-login, impersonar iniciar/encerrar, api-key gerar/revogar, preferências.

### Contratos cross-module
- **Publica:** `UsuarioCriado/Atualizado/Deletado`, `ImpersonacaoIniciada` (Outbox).
- **Consome:** `Empresa`, `PerfilAcesso`.

---

## Síntese de contratos cross-module (Outbox / Lookups / Global)

- **Eventos Outbox publicados (existentes):** `UsuarioCriado`, `UsuarioAtualizado`, `UsuarioDeletado`, `ImpersonacaoIniciada`, `ComunicacaoSuperAdminCriada` (Aplicativo); `FaturaAlertaCobrancaEvent`, `ComissaoApuradaEvent` (GestaoClientes). Processados por `AplicativoOutboxProcessorJob.cs`.
- **Eventos previstos e faltantes:** ciclo do Cliente/Assinatura (criado/atualizado/troca-plano/sincronizado — RF-APP-TEN-004-014), Pedido liquidado, Plano sincronizado.
- **IGlobalEntity:** marker vazia; enforcement de catálogo global inexistente (só `CodigoPostalCache` a implementa).
- **Lookups:** consumidos pela camada de dashboards (ausente); Lookups produzidos vivem em outros módulos (`VendasLookups`, `PessoaLookup`, `FinanceiroLookups`, `ServicoLookup`).
- **Jobs:** `AplicativoOutboxProcessorJob`, `ExpiracaoSessoesJob`, `ExpurgarNewsletterInativaJob`.

