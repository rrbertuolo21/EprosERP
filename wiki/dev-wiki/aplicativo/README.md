# Aplicativo — Dev

> **O que é:** o **control-plane SaaS** (Gestão de Clientes) com que a Siser opera o negócio de vender o Epros:
> tenants, planos, assinatura, cobrança, acesso/RBAC, isolamento e operação Landlord. **Não** é o ERP do cliente — é o
> painel que **gerencia os clientes** do ERP.
>
> **Fonte canônica (fábrica):** `projetos/siser/iniciativas/plataforma/especificacoes/0_APLICATIVO/`
> — [`MANUAL_CENTRAL.md`](../../../../..) (visão 100%) e [`MODELO_DADOS.md`](../../../../..) (ERD). Os 11 MANUAIS de submódulo estão listados em [Regras / decisões](#regras--decisões-implementadas).
> **Código:** `src/Modules/Epros.Modules.Aplicativo/` e `src/Modules/Epros.Modules.GestaoClientes/` · controllers em `src/API/Epros.API/Controllers/`.

## Visão técnica & fronteiras
Módulo transversal do control-plane. Sua fronteira **não** é uma área de negócio do ERP: ele governa **quem é cliente,
o que contratou, se pagou e quem pode entrar**. Os demais módulos (Vendas, Estoque, Fiscal…) rodam **dentro** do tenant
que este módulo cria e isola; a autorização (`AbacFilter`), o isolamento (RLS) e o entitlement de plano que eles herdam
são definidos aqui. É o único módulo com uma superfície **Landlord** (operação interna Siser), servida em host separado.

**Estado (2026-08-01):** 10 de 11 submódulos fechados e testados; 1.11 em finalização. Suíte em Postgres real
(Testcontainers) verde (~1037 testes no último placar do host-guard), branch `pente-fino/aplicativo`.

## Arquitetura
**Dois contextos / dois schemas físicos**, ambos multi-tenant por `TenantId`, soft-delete e snake_case via `ContextBase`:

- **`aplicativo.*`** — `Epros.Modules.Aplicativo` — identidade global, sessão, impersonação, preferências.
  Context: `src/Modules/Epros.Modules.Aplicativo/Infrastructure/Data/ContextAplicativo.cs`.
- **`plataforma.*`** — `Epros.Modules.GestaoClientes` — tenant/cliente, assinatura, cobrança, RBAC, empresa, catálogos, menu.
  Context: `src/Modules/Epros.Modules.GestaoClientes/Infrastructure/Data/ContextGestaoClientes.cs`.

Toda entidade herda `EntidadeSaaSBase` → `{ Id, SyncId, TenantId, SyncVersion, CriadoEm/Por, AlteradoEm/Por, DeletadoEm }`.
Concurrency token global = **`xmin` do Postgres** (aditivo, sem migration; só no provider Npgsql), mapeado a **409** no `ExcecaoGlobalMiddleware`.

**Entidades-chave** (modelo completo e cardinalidades → `especificacoes/0_APLICATIVO/MODELO_DADOS.md`):

| Entidade | Papel | Schema |
|---|---|---|
| `Cliente` | o **tenant**; âncora do isolamento (`Cliente.TenantId`) | `plataforma` |
| `Plano` / `GrupoPlano` / `ModuloPlano` / `AddOn` | oferta (híbrida global+custom), limites, entitlement | `plataforma` |
| `AssinaturaCliente` | vínculo vigente Cliente↔Plano, ciclo (`ProximaCobrancaEm`, `TrialAte`) | `plataforma` |
| `Fatura` → `FaturaItem` → `PagamentoFatura` → `ReciboPagamento` | ciclo de cobrança | `plataforma` |
| `Cupom` / `UsoCupom` | desconto (pedido inicial e recorrência) | `plataforma` |
| `Revenda` / `Vendedor` | canal de venda, percentuais de comissão | `plataforma` |
| `Usuario` | identidade **global** (não pertence a um tenant único) | `aplicativo` |
| `AcessoUsuarioTenant` | membership N:N (índice único `UsuarioId, TenantId`) | `plataforma` |
| `UsuarioInterno` / `SessaoImpersonacao` | operação Landlord + trilha de impersonação | `aplicativo` |
| `IdentidadeExterna` | login social OAuth (Provedor + SubjectId) | `aplicativo` |
| `Papel` / `Capacidade` (+ join) | RBAC unificado | `plataforma` |
| `Menu` / `MenuItemNivel1/2` (`CapacidadeRequerida`) | árvore de navegação = projeção das capacidades | `plataforma` |

## Endpoints
Rotas reais dos controllers em `src/API/Epros.API/Controllers/`. Público = sem gate; `[AbacAuthorize("Recurso","Acao")]` = capacidade exigida; **Landlord** = superfície interna Siser (host-guard, ver abaixo).

| Rota (prefixo) | Ex. de operações | Auth | Controller |
|---|---|---|---|
| `api/v1/public/auth/login`, `.../plataforma/login` | login tenant / login painel | público | `AuthController` |
| `api/v1/public/account/login`, `api/v1/account/session` | login e sessão de conta | público / sessão | `AccountController` |
| `api/v1/onboarding/empresa`, `.../{empresaId}` | self-register PF/PJ, trial automático | público / sessão | `OnboardingController` |
| `api/v1/aplicativo/assinaturas` | `vigente`, `contratar`, `mudar-plano`, `cancelar`, `reativar` | sessão do cliente | `AssinaturasController` |
| `api/v1/aplicativo/cupons` | CRUD de cupom | `SuperAdmin:Configurar` / `SuporteComercial:Configurar` | `CuponsController` |
| `api/v1/aplicativo/usuarios` | usuários do tenant | `Usuario:Ver` | `UsuariosController` |
| `api/v1/plataforma/planos` | catálogo de planos | `SuperAdmin:Configurar` / `SuporteComercial:Configurar` | `PlanosController` |
| `api/v1/plataforma/papeis`, `.../perfis-acesso` | RBAC (papéis, perfis) | `Papel:Ler` / `PerfilAcesso:Ler` | `RbacController`, `PerfisAcessoController` |
| `api/v1/plataforma/menus` | menu projetado das capacidades | sessão | `MenusController` |
| `api/v1/plataforma/faturas`, `.../{id}/pdf`, `.../recibo/pdf`, `.../boleto/pdf`, `.../pagamentos/{id}/estornar` | faturas, PDFs, estorno | **Landlord** — `SuperAdmin:Configurar` + `SuporteComercial:Configurar` | `FaturasController` |
| `api/v1/plataforma/superadmin/...` | aprovar assinatura manual, restaurar soft-delete | **Landlord** — `SuperAdmin:Configurar` | `SuperAdminController` |
| `api/v1/landlord/suporte/acessar-cliente` | impersonação com trilha | **Landlord** — `SuperAdmin:Configurar` | `LandlordSuporteController` |
| `api/v1/plataforma/relatorios-financeiros` | receita/inadimplência/comissão por período | **Landlord** — `SuperAdmin:Configurar` (+ guard no handler) | `RelatoriosFinanceirosLandlordController` |
| `api/v1/plataforma/revendas` | canal/comissão | **Landlord** | `RevendasController` |
| `api/v1/installation/state`, `.../check-requirements` | bootstrap/instalação | — | `InstallationController` |

## Eventos & integrações
**Outbox** (`plataforma`): efeitos gravados na mesma transação e entregues por processador. Eventos publicados:

- `FaturaAlertaCobrancaEvent` — régua de dunning D-3/D-1/D+1/D+5/D+10/D+14 (`ReguaCobrancaJob`). ⚠️ **Enfileira mas ninguém consome no GestaoClientes** — ver [Estado](#estado--pendências).
- `TrialConvertidoEm` / conversão trial→pago (marco 1.08A) · fim de trial gera 1ª fatura.
- `ReciboEmitidoEvent`, `PagamentoEstornadoEvent` — pagamento/estorno (processador 1.08C notifica).
- `PlanoAlteradoEvent`, `AssinaturaCanceladaEvent`, `AssinaturaReativadaEvent` — mudança de plano / ciclo de vida da assinatura.
- `ComissaoApuradaEvent` (factual), `ReconhecimentoReceita` cronograma — apuração de comissão e receita diferida (1.08I).
- `PessoaAnonimizadaEvent` — LGPD.

**Integração externa:** gateway Mercado Pago (`IPaymentGateway` — PIX/cartão/boleto, webhook de conciliação, refund
`POST /v1/payments/{id}/refunds`). PDF via QuestPDF (`IDocumentoFinanceiroRenderer`). E-mail: só boas-vindas + broadcast super-admin entregam hoje.

**Cross-módulo:** este módulo define o **entitlement de plano** (`ModuloTenantMiddleware`/`ValidadorLimitesSaaS`), a
**autorização** (`AbacFilter`) e o **isolamento** (RLS) que todos os módulos do ERP herdam. Módulos sobem **desabilitados** e são liberados por plano.

## Regras / decisões implementadas
Não repetidas aqui — a fonte da verdade são os MANUAIS/EF/DECISOES na fábrica (`especificacoes/0_APLICATIVO/`):

- Visão 100% + roteiro de fechamento: `MANUAL_CENTRAL.md` · ERD: `MODELO_DADOS.md`
- Integração intra-módulo: `ESPECIFICACAO_INTEGRACAO_DEPENDENCIAS_INTRA_MODULO_V1.md`
- **11 MANUAIS de submódulo** (cada um em `especificacoes/0_APLICATIVO/<SUBMODULO>/`):
  1. `ASSINATURA_E_PLANOS/MANUAL_0_APLICATIVO_ASSINATURA_E_PLANOS_V1.md`
  2. `CATALOGOS_GLOBAIS_SAAS/MANUAL_CATALOGOS_GLOBAIS_SAAS_V1.md`
  3. `DASHBOARD_E_LAYOUT/MANUAL_DASHBOARD_E_LAYOUT_V1.md`
  4. `IDENTIDADE_E_CONTEXTO_TENANT/MANUAL_IDENTIDADE_E_CONTEXTO_TENANT_V1.md`
  5. `ISOLAMENTO_DE_DADOS/MANUAL_ISOLAMENTO_DE_DADOS_V1.md`
  6. `LIMITES_DE_PLANO/MANUAL_LIMITES_DE_PLANO_V1.md`
  7. `ONBOARDING_E_EMPRESA/MANUAL_ONBOARDING_E_EMPRESA_V1.md`
  8. `PEDIDOS_E_COBRANCA_SAAS/MANUAL_PEDIDOS_E_COBRANCA_SAAS_V1.md`
  9. `USUARIOS_E_PAPEIS/MANUAL_USUARIOS_E_PAPEIS_V1.md`
  10. `PERMISSOES_DE_MENU/MANUAL_PERMISSOES_DE_MENU_V1.md`
  11. `OPERACAO_SUPER_ADMIN/MANUAL_OPERACAO_SUPER_ADMIN_V1.md`
- Decisões e verificação por submódulo: `<SUBMODULO>/DECISOES_IMPLANTACAO_V1.md`, `EF_...`, `MC_...`, `VERIFICACAO_IMPLEMENTACAO_V1.md`.

Destaques de decisão (o *porquê* mora no MANUAL correspondente):
- **RBAC unificado** (Papel+Capacidade) e **menu = projeção** das capacidades via `AbacFilter` — fonte única, sem tela sem gate (1.09/1.10).
- **Isolamento fail-closed** por RLS Postgres (1.05).
- **Inadimplência**: avisar antes → somente-leitura/export → bloqueio → reativar ao pagar (skill jurídica; 1.06/REG-021).
- **Reconhecimento de receita** (anual = 12 avos + passivo diferido) e **comissão** parametrizável — mecanismo feito (1.08I), **parâmetros = valida contador**.

## Como estender / gotchas
- **Novo endpoint do control-plane:** controller fino em `Controllers/`, rota `api/v1/<area>/...`, `[AbacAuthorize("Recurso","Acao")]`
  com uma capacidade **descoberta** (o `AbacFilter` só cobra o que o catálogo conhece); despache command/query (MediatR) — nada de lógica no controller.
- **Nova tela no menu:** o `MenuCatalogoSeeder` amarra cada folha à capacidade `recurso:acao` do módulo. Se não há capacidade óbvia, a folha fica `CapacidadeRequerida = null` (item sem gate próprio — documentado no seeder). Não semeie capacidade que o gate não conheça.
- **Superfície Landlord:** rotas internas (superadmin, landlord/suporte, relatorios-financeiros, plataforma/faturas, installation)
  são protegidas em profundidade pelo `HostGuardMiddleware` (`src/API/Epros.API/Middlewares/HostGuardMiddleware.cs`) — só respondem no host configurado em `Hosts:Landlord`; fora dele → **404**. É defense-in-depth **sobre** o gate ABAC.
- **Concurrency:** UPDATE/DELETE levam `WHERE ... AND xmin=@original`; conflito vira **409**. Não tente setar versão em inserts/seeds (o Postgres preenche `xmin` sozinho).
- **Restauração de soft-delete:** use `EntidadeSaaSBase.Restaurar(...)` + `RestaurarEntidadeCommand` (idempotente; re-aplica o filtro de tenant à mão sob `IgnoreQueryFilters`).
- **Regra de negócio/fiscal/contábil:** vem **sempre** da skill de negócio (`Negocio-acumulado/<domínio>`), citando a fonte. Skill vazia → pare e peça validação humana; nunca invente (Regra #0).

## Estado & pendências
- **1.11 OPERACAO_SUPER_ADMIN** em finalização (fixes de segurança super-admin, perfis de suporte, wizard).
- 🔴 **Dunning quebrado:** `ReguaCobrancaJob` monta e enfileira `FaturaAlertaCobrancaEvent`, mas **não há Outbox processor no GestaoClientes** consumindo → alertas nunca entregam. Fim de trial não notifica; recibo é registro, não e-mail/PDF. Só boas-vindas + broadcast super-admin enviam de fato.
- **Valida contador/advogado (parâmetro, não código):** contas contábeis e política de reconhecimento de receita (CPC 47), base/%/momento/janela de clawback da comissão, política de proração, alíquota/subitem de ISS/NFS-e da mensalidade, método de LTV. O código calcula citando a norma; **não fixa o parâmetro**.
- **Dependências de ambiente (código existe, falta provar):**
  - Build Nuxt do front — falta `node`/`npm` na máquina (as `.vue` foram entregues).
  - Ponta-a-ponta cartão/boleto/checkout — falta credencial Mercado Pago (testes com gateway mockado).
  - Migrations em banco persistente — só rodadas no container de teste; falta apply em lote + smoke test.
  - QuestPDF depende do nativo SkiaSharp (presente aqui); em host sem a lib, a fatia cai para HTML print-ready sem mudar contratos.
- **Go-live** depende de 3 dependências externas: provedor SMTP, credencial Mercado Pago e overlay fiscal `negocio-siser` (com contador).
- **LGPD** parcial: anonimização + auditoria + janela de export 30d existem; falta export/exclusão completa por tenant (portabilidade).
