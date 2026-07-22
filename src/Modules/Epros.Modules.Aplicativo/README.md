# Epros.Modules.Aplicativo

Módulo **Aplicativo / Plataforma SaaS** do EprosERP. Cobre autenticação e sessão, usuários (internos e de tenant), assinaturas/planos e pedidos, área **Super Admin** (operações globais, execução em massa maker-checker, configurações de sistema, comunicação), landing page/marketplace, newsletter, onboarding, menus/perfis de acesso e governança de instalação/atualização.

## Arquitetura

Clean Architecture + CQRS (MediatR), multi-tenant por `TenantId`. Operações globais são restritas ao tenant do sistema (`"system"` / Siser).

```
Application/
  Commands/       Contratos de escrita
  Queries/        Contratos de leitura
  Handlers/       Handlers de comando/consulta e de eventos
Domain/
  Entities/       Usuario, UsuarioInterno, Assinatura*, SystemSetting, CustomPage, LandingPageSettings,
                  ExecucaoMassaGlobal, ComunicacaoSuperAdmin, SessaoUsuario, ...
Infrastructure/
  Data/           ContextAplicativo (EF Core / PostgreSQL)
Migrations/
```

## Fluxos principais

- **Autenticação / Sessão** — `AuthHandlers`, `AuthQueryHandlers`, `SessaoQueryHandlers` (login, empresas disponíveis, sessão ativa, histórico de login).
- **Usuários** — `UsuarioHandlers` (usuários de tenant), `UsuarioInternoHandlers` (usuários internos/Super Admin).
- **Assinaturas & Pedidos** — `AssinaturaCommandHandlers` / `AssinaturaQueryHandlers` / `PedidosHandlers` (contratação de plano, ajuste de planos em lote).
- **Super Admin** — `SuperAdminQueryHandlers` (dashboard/usuários/settings globais), `ExecucaoMassaGlobalHandlers` + `SimularExecucaoMassaGlobalCommandHandler` (fluxo **maker-checker** com simulação e aprovação), `SystemSettingHandlers`, `ComunicacaoSuperAdminHandlers` (enfileira no Outbox e processa via job).
- **Conteúdo público** — `LandingPageSettingsHandlers` (landing page e marketplace, registro único por escopo), `CustomPageHandlers`, `NewsletterSubscriberHandlers`, `CuponsHandlers`.
- **Menus / Acesso** — `MenusHandlers` / `MenusQueryHandlers` (perfis de acesso).
- **Instalação / Onboarding** — `InstallationHandlers`, `OnboardingHandlers`.

## Restrição de tenant do sistema

Handlers de escopo global (Super Admin, System Settings, Landing/Marketplace, Execução em Massa) validam `GetTenantId() == "system"` e retornam/lançam acesso proibido caso contrário.

## Testes

`tests/Epros.Tests/SuperAdminCqrsTests.cs`, `SuperAdminDomainTests.cs`, `SuperAdminControllerTests.cs`, `SuperAdminTests.cs` — cobrem restrição de tenant, criação/alteração de usuário interno, promoção a admin principal, System Settings, maker-checker de execução em massa, CustomPage/Newsletter, queries de leitura e o fluxo de comunicação Super Admin via Outbox.
