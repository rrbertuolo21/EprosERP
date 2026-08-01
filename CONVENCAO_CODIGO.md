# Convenção de Código — EprosERP

> **Documento CANÔNICO e obrigatório para humanos e agentes de IA** que alteram este repositório.
> Leia **antes** de escrever qualquer linha. Em caso de dúvida, **copie um arquivo existente do mesmo módulo** — não invente padrão novo.
> Este é a **fonte única de convenção**. O [`docs/migracao/PADRAO_PORTE_LEGADO.md`](docs/migracao/PADRAO_PORTE_LEGADO.md) contém apenas o *molde campo a campo* do porte e se subordina a este documento. Em qualquer divergência, **este arquivo prevalece**.

**Documentos complementares (ordem de leitura):**
1. Este arquivo (`CONVENCAO_CODIGO.md`) — regras gerais, anti-alucinação e fonte única
2. [`docs/migracao/PADRAO_PORTE_LEGADO.md`](docs/migracao/PADRAO_PORTE_LEGADO.md) — molde campo a campo do legado → novo (só o template)
3. Legado fonte da verdade: `../Epros/epros_erp-main/src/Epros.ERP.Domain/`

---

## 1. Regra de ouro

```
PORTE FIEL. NÃO INVENTAR. NÃO SIMPLIFICAR. NÃO REMOVER CAMPO.
```

- Toda entidade/campo do legado **precisa existir** no EprosERP (mesmo que em fase posterior).
- Traduz-se **plataforma** (SQL Server / `long` / `Entity` / Controller monolítico) para **plataforma nova** (PostgreSQL / `Guid` / `EntidadeSaaSBase` / CQRS MediatR).
- **Proibido** criar biblioteca, namespace, módulo ou padrão que não exista neste repositório sem aprovação explícita.
- **Proibido** reescrever `Epros.ERP.DfeCalculos` ou `Epros.ERP.Dfe.API` — reutilizar via `IHerculesFiscalService`.

---

## 1.1. Dois modos de trabalho (LEIA — define o que é obrigatório em cada fase)

O projeto opera em **dois modos**. A fase atual é sempre indicada na tarefa. Não misture os gates.

| | **Modo Porte** (fase atual — velocidade) | **Modo Consolidação** (depois — qualidade) |
|---|---|---|
| Objetivo | Volume de código FIEL em massa, em paralelo | Compilar, migrar e testar tudo |
| Entidade + mapping + CQRS | ✅ obrigatório | ✅ (revisar) |
| Migration EF | ❌ **NÃO criar** (evita snapshot corrompido em paralelo) | ✅ num passe **serializado**, 1 Context por vez |
| `dotnet build` / `dotnet test` verde | ❌ não é gate (o dono compila/ajusta depois) | ✅ **gate obrigatório** |
| Testes xUnit | opcional | ✅ obrigatório |
| Fidelidade campo a campo | ✅ **sempre** (nunca relaxa) | ✅ auditar De→Para |

Regra prática: **no Modo Porte, o único gate que nunca relaxa é a fidelidade** (nenhum campo do legado pode sumir). Migration e testes são do Modo Consolidação.

---

## 1.2. Decisões arquiteturais FIXADAS (não reabrir sem ADR) — jul/2026

Definidas após auditoria das ~834 mudanças. **Nenhum agente pode contrariar:**

| Tema | Decisão | O que fazer |
|---|---|---|
| **Modelo financeiro** | Canônico = `ContasAPagar` / `ContasAReceber` (fiel ao legado, com itens/juros/multa/desconto/FatoGerador). | Migrar API/handlers/eventos/OFX/fluxo de caixa para o fiel. **Remover** `ContaPagar`/`ContaReceber` simplificados e suas tabelas. Um modelo só. |
| **RBAC / Menu** | Dono = **GestaoClientes** (`PerfilAcesso` + menu dinâmico, já usado em runtime; `UsuarioEmpresa.PerfilUsuarioId` resolve nele). | Deprecar `PerfilUsuario`/`menus` do **Aplicativo**. Aplicativo não é dono de RBAC de tenant. |
| **Catálogos nacionais** | `Ncm`, `Cest`, `CodigoAnp`, `EnquadramentoIpi`, `IcmsAliquotaInterestadual`, `FcpAliquotaUf`, CFOP etc. são **`IGlobalEntity`** (sem tenant), como `Banco`/`CodigoServicoSefaz`. Nunca `EntidadeSaaSBase` com tenant. | Trocar base + migration removendo `tenant_id`. |
| **IeSt** | Dono único = **GestaoClientes** (pertence à Empresa). | Fiscal remove DbSet/handlers/controller de escrita; leitura via Lookup se precisar. |
| **FatoGeradorFinanceiro** | Dono = **Financeiro**. | Remover do módulo Vendas; integração Venda→Financeiro via evento/Outbox. |
| **Agregado raiz** | Se as sub-entidades existem (ex.: `CompraEmitente`, `VendaNfe`), o agregado raiz (`Compra`/`Venda`) DEVE ter as navegações filhas e persisti-las. | Expandir `Compra`/handlers para o agregado fiscal completo. |
| **Status de documento** | `Venda.Status` e `Compra.Status` = enum (`EVendaStatus`), nunca string. | Alinhar Venda ao enum. |

### Regras reforçadas para agentes (P0 da auditoria)
- **CONGELAR migrations**: agentes de porte NÃO criam migration. Migrations só num **passe de consolidação serializado**, feito pelo lead, um Context por vez.
- **Controller fino de verdade**: ZERO `DbContext` injetado em controller (violação vista em `EnderecosController`/`VeiculosController`). Só `_mediator.Send()`.
- **Sem duplicar entidade entre módulos**: antes de criar, `grep` o nome em TODOS os módulos. Uma tabela = um Context dono.
- **Catálogo nacional = `IGlobalEntity`** sempre.
- **Backfill antes de NOT NULL**: nova FK obrigatória (ex.: `servicos.empresa_id`) entra nullable + backfill, nunca `defaultValue: Guid.Empty`.

---

## 2. Stack fixa (não substituir)

| Camada | Tecnologia |
|--------|------------|
| Backend | .NET 8, ASP.NET Core, C# |
| ORM | Entity Framework Core 8 |
| Banco | PostgreSQL (snake_case automático via `ContextBase`) |
| CQRS | MediatR |
| Validação domínio | Flunt (`Contract<T>`, `Notifiable<Notification>`) |
| Validação command (opcional) | FluentValidation (`AbstractValidator<T>`) |
| Frontend | Nuxt 3, Vue 3, TypeScript |
| Testes | xUnit (`tests/Epros.Tests/`) |

**Não usar:** AutoMapper em commands, Repository genérico, UnitOfWork custom, Dapper (salvo exceção aprovada), SQL Server, `long` como PK de entidade SaaS.

---

## 3. Estrutura do repositório

```
EprosERP/
├── src/
│   ├── API/Epros.API/              ← Controllers finos (só MediatR)
│   ├── Infrastructure/Epros.Infrastructure/
│   ├── Shared/Epros.Shared/        ← EntidadeSaaSBase, CommandResult, enums, IGlobalEntity, IHardDeletable
│   └── Modules/
│       ├── Epros.Modules.Aplicativo/
│       ├── Epros.Modules.GestaoClientes/
│       ├── Epros.Modules.Estoque/
│       ├── Epros.Modules.Vendas/
│       ├── Epros.Modules.Financeiro/
│       ├── Epros.Modules.Fiscal/
│       └── ... (Qualidade, Producao, RH, Projetos, Manutencao, GRC, ESG, DMS)
├── tests/Epros.Tests/
├── Epros.App/                      ← Frontend Nuxt (plataforma/admin)
└── docs/migracao/PADRAO_PORTE_LEGADO.md
```

### Layout interno de cada módulo

```
Epros.Modules.<Nome>/
├── Domain/
│   ├── Entities/           ← Agregados e entidades
│   ├── ValueObjects/       ← VOs imutáveis (quando necessário)
│   └── Enums/              ← Enums exclusivos do módulo (preferir Shared se compartilhado)
├── Application/
│   ├── Commands/           ← Records IRequest<CommandResult> ou ICommand
│   ├── Queries/            ← Records + Handlers (podem ficar no mesmo arquivo)
│   └── Handlers/           ← IRequestHandler ou ICommandHandler
├── Infrastructure/
│   ├── Data/               ← Context<Modulo>.cs + Lookups
│   ├── Jobs/               ← Quartz (se aplicável)
│   └── Services/           ← Integrações externas do módulo
└── Migrations/             ← EF Core migrations DESTE módulo apenas
```

---

## 4. Mapa legado → módulo → schema PostgreSQL

| Origem legado (`Epros.ERP.Domain/Entities/...`) | Módulo EprosERP | DbContext | Schema EF |
|--------------------------------------------------|-----------------|-----------|-----------|
| Cadastros/Pessoas, Empresas, Enderecos | GestaoClientes | ContextGestaoClientes | `plataforma` |
| Cadastros/Produtos, Estoque, Compras | Estoque | ContextEstoque | `estoque` |
| Cadastros/Bancos, Financeiros, Importacoes | Financeiro | ContextFinanceiro | `financas` |
| Vendas | Vendas | ContextVendas | `vendas` |
| Fiscais, Tributarios, Servicos, Contador, Configuracoes | Fiscal | ContextFiscal | `plataforma` * |
| Permissoes, Usuarios | Aplicativo **+** GestaoClientes | ContextAplicativo / ContextGestaoClientes | `aplicativo` / `plataforma` \*\* |
| RH, Producao, Qualidade, etc. | Módulos respectivos | Context* | ver `HasDefaultSchema` no Context |

> \* **Confirmado no código:** `ContextFiscal` usa `HasDefaultSchema("plataforma")`. **Não** criar schema `fiscal`. Seguir sempre o `HasDefaultSchema` que já está no Context do módulo.
> \*\* **Permissões/Menu já existem parcialmente em `GestaoClientes` (schema `plataforma`, migration `AddPermissoesMenu`).** Antes de portar Menu/Perfil, verifique lá para não duplicar.

**Antes de criar entidade:** abrir o `Context*.cs` do módulo destino e verificar se já existe DbSet ou entidade similar. **Nunca duplicar** (ex.: `Servico` já está em `Epros.Modules.Fiscal`).

---

## 5. Entidades de domínio

### 5.1 Base obrigatória

Toda entidade multi-tenant herda `Epros.Shared.Domain.Entities.EntidadeSaaSBase`:

| Campo base | Tipo | Regra |
|------------|------|-------|
| Id | Guid | Gerado no ctor base |
| SyncId | Guid | Offline sync |
| TenantId | string | Injetado via ContextBase |
| SyncVersion | int | Incrementado em `MarcarAlterado` |
| CriadoEm, AlteradoEm, DeletadoEm | DateTime | UTC |
| CriadoPor, AlteradoPor | string | UserId |

**Não recriar** Id, TenantId ou auditoria manualmente.

### 5.2 Entidades globais (sem tenant)

Implementar `IGlobalEntity` quando a entidade for compartilhada entre tenants (ex.: `Banco`):

```csharp
public class Banco : EntidadeSaaSBase, IGlobalEntity
{
    public Banco(string codigo, string descricao, string criadoPor) : base("system", criadoPor) { ... }
}
```

### 5.3 Padrão de classe

```csharp
public class MinhaEntidade : EntidadeSaaSBase
{
    public Guid OutraEntidadeId { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public OutraEntidade OutraEntidade { get; private set; } = null!; // só navegação INTRA-módulo

    protected MinhaEntidade() { } // EF Core — obrigatório

    public MinhaEntidade(/* todos os campos */, string tenantId, string criadoPor)
        : base(tenantId, criadoPor)
    {
        // atribuir campos
        Validar();
    }

    public void Alterar(/* campos */, string alteradoPor)
    {
        // atribuir campos
        MarcarAlterado(alteradoPor);
        Validar();
    }

    public void Validar()
    {
        Clear();
        AddNotifications(new Contract<MinhaEntidade>()
            .Requires()
            .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição é obrigatória [Origem: MinhaEntidade]"));
    }
}
```

### 5.4 Regras de propriedades

| Regra | Detalhe |
|-------|---------|
| `private set` | Sempre, exceto Lookups de leitura cruzada |
| FK legado `long XId` | → `Guid XId` |
| Enum legado | Portar para `Epros.Shared.Domain.Enums` — **não** usar `string` ou `int` solto |
| `SequenciaTenantId` legado | **Exceção oficial aprovada:** portar como `public long? SequenciaExibicao { get; private set; }` — nome fixo, idêntico em todas as entidades, somente exibição/UX. Nunca substitui o `Guid Id`. Não invente outros nomes. |
| Soft delete | `entidade.Deletar(userId)` — **padrão sempre**. Nunca `_context.Remove()` em entidade SaaS |
| Hard delete | **Default é soft delete.** `IHardDeletable` só com aprovação explícita e em lista branca justificada — "o legado apagava fisicamente" **não** é justificativa suficiente por si só |
| DateTime | UTC (`DateTime.UtcNow` na base) — nunca `DateTime.Now` |
| decimal | Precisão 18,2 aplicada automaticamente pelo `ContextBase` |
| Mensagens validação | Português BR, incluir `[Origem: NomeEntidade]` quando portado do legado |

### 5.5 Arquivos modelo (copiar, não reinventar)

| Tipo | Arquivo referência |
|------|-------------------|
| Entidade rica | `src/Modules/Epros.Modules.Financeiro/Domain/Entities/ContaBancaria.cs` |
| Entidade global | `src/Modules/Epros.Modules.Financeiro/Domain/Entities/Banco.cs` |
| Entidade expandida legado | `src/Modules/Epros.Modules.Estoque/Domain/Entities/Produto.cs` |
| Agregado venda (parcial) | `src/Modules/Epros.Modules.Vendas/Domain/Entities/Venda.cs` |

---

## 6. DbContext e EF Core

### 6.1 Herança

Todo Context de módulo herda `Epros.Infrastructure.Data.ContextBase` e recebe `ITenantProvider` + `ICurrentUser`.

### 6.2 Mapping

- Mapping **inline** em `OnModelCreating` do Context do módulo.
- Primeira linha: `modelBuilder.HasDefaultSchema("<schema>");` — usar **exatamente** o schema já definido no Context do módulo.
- Última linha do override: `base.OnModelCreating(modelBuilder);`
- Registrar `DbSet<T>` com nome plural descritivo em português camelCase: `ContasBancarias`, `PlanosDeContas`.
- Índices compostos: `{ TenantId, CampoNegocio }`.
- FK: preferir `DeleteBehavior.Restrict` (cadastros) ou `Cascade` (filhos do agregado).
- `HasMaxLength` em **todas** as strings — copiar limite do legado ou do molde.

### 6.3 Migrations

> **Modo Porte: NÃO criar migrations.** Gerar migration em paralelo corrompe snapshot. As migrations são feitas no **Modo Consolidação**, num passe serializado, **um Context por vez**.

```powershell
# SOMENTE no Modo Consolidação — um agente por vez por Context
dotnet ef migrations add <NomeDescritivo> --project src/Modules/Epros.Modules.<Modulo> --startup-project src/API/Epros.API
```

| Regra | Detalhe |
|-------|---------|
| 1 migration por feature coesa | Ex.: `AddBancoAndContaBancariaAndCartao` |
| Nome | PascalCase descritivo |
| Conflito | 2 agentes no mesmo Context → **serializar** migrations |
| Não editar | Snapshot de outro agente — criar migration corretiva nova |
| Validar | `dotnet build` + `dotnet test` após cada migration |

### 6.4 Leitura cross-module (Lookup)

**Proibido** referência de projeto entre módulos de domínio.

Para ler entidade de outro módulo, criar **Lookup** no `Infrastructure/Data/` do módulo consumidor:

```csharp
// Em ContextFinanceiro.OnModelCreating:
modelBuilder.Entity<PessoaLookup>(e =>
{
    e.ToTable("pessoas", "plataforma");
    e.HasKey(x => x.Id);
    e.HasQueryFilter(x => x.DeletadoEm == null);
});
```

Referência: `src/Modules/Epros.Modules.Financeiro/Infrastructure/Data/PessoaLookup.cs`

---

## 7. Application Layer (CQRS)

### 7.1 Commands

Preferir `record` imutável. Dois estilos **válidos** no projeto (usar o do módulo que está editando):

**Estilo A — `IRequest<CommandResult>` (Financeiro, Estoque, Vendas):**

```csharp
public record CriarBancoCommand(string Codigo, string Descricao) : IRequest<CommandResult>;
```

**Estilo B — `ICommand` + validator FluentValidation (Fiscal):**

```csharp
public record CriarServicoCommand(...) : ICommand;
public class CriarServicoCommandValidator : AbstractValidator<CriarServicoCommand> { ... }
```

Convenção de nomes:

| Operação | Command |
|----------|---------|
| Criar | `Criar<Entidade>Command` |
| Atualizar | `Atualizar<Entidade>Command` |
| Deletar | `Deletar<Entidade>Command` |
| Negócio | Verbo + substantivo: `BaixarContaPagarCommand`, `LancarCompraCommand` |

### 7.2 Handlers

```csharp
// Estilo A
public class CriarBancoCommandHandler : IRequestHandler<CriarBancoCommand, CommandResult>
// Estilo B
public class CriarServicoCommandHandler : ICommandHandler<CriarServicoCommand>
```

**Template obrigatório do Handle:**

```csharp
public async Task<CommandResult> Handle(MeuCommand request, CancellationToken cancellationToken)
{
    var tenantId = _tenantProvider.GetTenantId();
    var userId = _currentUser.GetUserId() ?? "system";

    var entidade = new MinhaEntidade(/* ... */, tenantId, userId);
    if (!entidade.IsValid)
        return CommandResult.Falha(entidade.Notifications.Select(n => n.Message));

    _context.MinhasEntidades.Add(entidade);
    await _context.SaveChangesAsync(cancellationToken);

    return CommandResult.Ok("Mensagem de sucesso em português.", new { entidade.Id });
}
```

### 7.3 Queries

- Records: `Listar<Entidade>Query`, `Obter<Entidade>PorIdQuery`
- Handler pode ficar no mesmo arquivo `*Queries.cs` (padrão Financeiro)
- Retorno: `CommandResult` com `Dados` = lista ou DTO anônimo
- Paginação padrão: `pagina = 1`, `tamanhoPagina = 20`
- Sync offline: `Obter<Entidade>SyncQuery(DateTime since)` quando módulo suporta

### 7.4 CommandResult

Usar **somente** `Epros.Shared.Application.Models.CommandResult`:

```csharp
CommandResult.Ok("mensagem", dadosOpcional);
CommandResult.Falha("erro único");
CommandResult.Falha(listaDeErros, "mensagem opcional");
```

**Não** criar `Result<T>`, `ApiResponse`, `ServiceResponse` paralelos.

### 7.5 DI e registro

- Handlers registrados via **assembly scan** em `Program.cs` — **não editar Program.cs** para registrar handler individual.
- Injetar sempre: `Context<Modulo>`, `ITenantProvider`, `ICurrentUser`.
- Serviços cross-module: injetar **interface** de `Epros.Shared.Application.Contracts` ou do módulo publicador.

### 7.6 Arquivos modelo

| Tipo | Referência |
|------|------------|
| Commands + Handlers CRUD | `Financeiro/Application/Commands/BancoAndContaBancariaAndCartaoCommands.cs` |
| Handlers separados + ICommand | `Fiscal/Application/Handlers/CriarServicoCommandHandler.cs` |
| Queries | `Financeiro/Application/Queries/BancoAndContaBancariaAndCartaoQueries.cs` |

---

## 8. API (Controllers)

- Controllers **finos**: só `_mediator.Send()` — zero lógica de negócio.
- Rota base: `api/v1/<recurso>` consistente com existente.
- Namespace: `Epros.API.Controllers`. Atributos: `[ApiController]`, `[Route("api/v1/...")]`.

| HTTP | Quando |
|------|--------|
| `Ok(result)` | Sucesso GET/PUT |
| `Created(string.Empty, result)` | POST criado |
| `NotFound(result)` | GET id inexistente |
| `BadRequest(result)` | Validação rota/corpo |
| `UnprocessableEntity(result)` | Domínio inválido |

- Compatibilizar rota com legado quando possível: `api/v1/cadastros/pessoas`, `api/v1/financeiro/contas-pagar`.
- IDs na rota: `{id:guid}`. Validar `id rota == id body` em PUT.
- Referência: `src/API/Epros.API/Controllers/EstoqueController.cs`

---

## 9. Eventos entre módulos

- Publicar via `OutboxMessage` no DbContext do módulo origem.
- Domain events tipados em `Epros.Shared.Domain.Events` ou pasta Events do módulo.
- Handler de integração em `Application/Handlers/*EventHandler.cs` no módulo **consumidor**.
- **Não** chamar DbContext de outro módulo diretamente.
- Eventos existentes (exemplos): `VendaFaturada`, `CompraLancada`, `FolhaProcessada`.

---

## 10. Testes

> **Aplica-se ao Modo Consolidação.** No Modo Porte, testes são opcionais (o dono compila/ajusta depois).

### 10.1 Obrigatório para toda entidade/command nova (Consolidação)

| Tipo | O quê testar |
|------|--------------|
| Domínio | `Validar()` — caso válido + cada regra inválida |
| Handler | Happy path + entidade não encontrada + validação falha |
| Nomenclatura | `[Fact(DisplayName = "Entidade \| Cenário \| Resultado esperado")]` |

### 10.2 Padrão

```csharp
public class MinhaEntidadeTests
{
    private const string TenantId = "tenant-test-001";
    private const string UserId = "user-test-001";

    [Fact(DisplayName = "MinhaEntidade | Dados válidos deve ser válida")]
    public void Criar_DadosValidos_DeveSerValida()
    {
        var entidade = new MinhaEntidade(/* ... */, TenantId, UserId);
        Assert.True(entidade.IsValid);
    }
}
```

Referência: `tests/Epros.Tests/BancoAndContaAndCartaoTests.cs`

### 10.3 Gate (somente Modo Consolidação)

```powershell
dotnet build EprosERP/Epros.sln
dotnet test EprosERP/tests/Epros.Tests/Epros.Tests.csproj
```

**Zero testes falhando.** Não commitar com build quebrado.

---

## 11. Frontend (Epros.App / Nuxt 3)

| Regra | Detalhe |
|-------|---------|
| Linguagem | TypeScript, `<script setup lang="ts">` |
| Textos UI | Português BR |
| API | Prefixo `/api/v1/` — usar composable existente se houver |
| Estilo | Seguir páginas existentes (`pages/plataforma/`, `components/`) |
| Não criar | Nova lib UI sem alinhamento — manter visual das páginas atuais |
| Legado front | `../Epros/epros_erp_front-main` é referência de UX — portar comportamento, não copiar cegamente |

---

## 12. Idioma e nomenclatura

| Item | Idioma / formato |
|------|------------------|
| Código (classes, métodos, propriedades) | Português BR ou inglês técnico **consistente com o módulo** |
| Mensagens usuário/validação | Português BR |
| Comentários | Português BR, só quando lógica não óbvia |
| Migrations | Nome descritivo |
| Commits | Português BR, imperativo: "Adiciona entidade ProdutoGrupo ao Estoque" |

Prefixos enum (legado): manter `E` quando portado — `ETipoPessoa`, `EVendaStatus`, `EModeloDocumento`.

---

## 13. Anti-alucinação — lista de proibições

| ❌ Proibido | ✅ Fazer |
|------------|---------|
| Criar `Epros.Modules.Tributario` separado | Usar `Epros.Modules.Fiscal` (já tem Cfop, Ncm, Servico, etc.) |
| Criar `Repository<T>` / `IUnitOfWork` | DbContext do módulo direto no handler |
| PK `long` ou `int` identity | `Guid` via `EntidadeSaaSBase` |
| Navegação EF cross-module | Lookup + Guid FK |
| Reescrever DfeCalculos | `IHerculesFiscalService` |
| Simplificar Venda/Compra para 2 entidades | Portar agregado completo do legado |
| String onde legado tem enum | Portar enum para Shared |
| Editar `Program.cs` por handler | Assembly scan já registra |
| Criar migration no Modo Porte | Migrations só no Modo Consolidação, serializadas |
| Criar schema `fiscal` | Fiscal usa `plataforma` (seguir o Context) |
| Inventar campo "útil" | Só campos do legado ou já no ERP |
| Inventar nome p/ sequência legada | Usar exatamente `SequenciaExibicao` (long?) |
| Hard delete "porque o legado apagava" | Soft delete; hard só em lista branca aprovada |
| `DateTime.Now` | UTC via base |
| Comentários óbvios | Código autoexplicativo |

### Checklist — Modo Porte (durante o porte em massa)

- [ ] Li o arquivo legado equivalente em `Epros/epros_erp-main`?
- [ ] **Todos** os campos do legado estão na entidade nova?
- [ ] Entidade no módulo correto (tabela seção 4) e schema correto?
- [ ] DbSet + mapping inline no Context?
- [ ] Commands/Queries/Handlers espelhando o controller legado?
- [ ] Não dupliquei entidade que já existia?

### Checklist — Modo Consolidação (antes de fechar/entregar)

- [ ] Migration serializada criada (1 Context por vez)?
- [ ] Testes xUnit adicionados?
- [ ] `dotnet build` + `dotnet test` verde?
- [ ] De→Para do módulo conferido (nenhum campo perdido)?

---

## 14. Trabalho paralelo (múltiplos agentes)

### 14.1 Donos por módulo (não invadir)

Cada agente escreve **somente dentro do seu módulo**. Referência cross-module só por Guid FK / Lookup.

| Frente | Módulos permitidos |
|--------|-------------------|
| Operacional | GestaoClientes, Estoque, Vendas, Financeiro |
| Fiscal | Fiscal (+ adaptadores DFe em Infrastructure) |
| Plataforma/Front/ETL | Aplicativo, Epros.App, tests, scripts |

> No porte automatizado atual, cada **módulo** tem 1 agente dono do seu `Context*.cs`, evitando conflito de arquivo.

### 14.2 Recursos exclusivos (1 agente por vez)

- Migrations do **mesmo** `Context*.cs`
- `Epros.Shared` (enums/base) — coordenar antes
- `Program.cs`
- `ContextBase.cs`

### 14.3 Integração entre agentes

- Contrato: interfaces em `Epros.Shared.Application.Contracts`
- DTO compartilhado: `Epros.Shared.Application.Models` ou DTO no módulo publicador
- Conflito de enum: **um** agente adiciona em Shared, outros só consomem

---

## 15. Porte do legado — fluxo obrigatório

Para cada entidade/controller legado (**Modo Porte**):

1. Abrir classe legada em `Epros/epros_erp-main/src/Epros.ERP.Domain/Entities/...`
2. Abrir controller legado em `Epros/epros_erp-main/src/Epros.ERP.API/Controllers/V1/...`
3. Identificar módulo destino (seção 4)
4. Verificar se entidade **já existe** no ERP (grep pelo nome) — não duplicar
5. Criar/expandir entidade com **todos** os campos
6. Mapping inline no Context (**sem** migration no Modo Porte)
7. Commands espelhando **cada** action do controller legado
8. Controller fino
9. Conferir com `docs/migracao/PADRAO_PORTE_LEGADO.md`

No **Modo Consolidação**, retomar cada módulo para: migrations serializadas → testes → build/test verde → auditoria De→Para.

---

## 16. Referência rápida de tipos compartilhados

```csharp
using Epros.Shared.Domain.Entities;      // EntidadeSaaSBase, IGlobalEntity, IHardDeletable
using Epros.Shared.Application.Models;    // CommandResult
using Epros.Shared.Application.Contracts; // ITenantProvider, ICurrentUser, ICommand, ICommandHandler
using Epros.Shared.Domain.Enums;          // enums compartilhados
using Epros.Infrastructure.Data;          // ContextBase
using Epros.Shared.Domain.Events;         // OutboxMessage
using MediatR;
using Flunt.Validations;
```

---

## 17. Prompt mínimo para colar em agentes externos

```
Você está no repositório EprosERP. OBRIGATÓRIO:
1. Ler CONVENCAO_CODIGO.md (canônico) e docs/migracao/PADRAO_PORTE_LEGADO.md (molde) antes de codar.
2. Fonte da verdade funcional: ../Epros/epros_erp-main (legado).
3. PORTE FIEL — não inventar campos, módulos ou padrões. Nenhum campo do legado pode sumir.
4. Entidades SaaS: EntidadeSaaSBase + Guid + Flunt Validar().
5. CQRS MediatR + CommandResult. Controllers finos.
6. Um DbContext por módulo — não referenciar projetos entre módulos (usar Lookup + Guid FK).
7. Copiar arquivo existente do mesmo módulo como molde. Seguir o HasDefaultSchema do Context.
8. MODO PORTE: sem migration, sem gate de teste. MODO CONSOLIDAÇÃO: migration serializada + dotnet build/test verde.
9. Escopo: [MÓDULO] — tarefas: [IDs]. Escrever SÓ dentro de [MÓDULO]. NÃO alterar: [pastas fora do escopo].
```

---

*Versão: 1.1 — jul/2026. Ajustes: dois modos de trabalho (Porte/Consolidação); migrations serializadas só na Consolidação; schema do Fiscal corrigido para `plataforma`; `SequenciaExibicao` como exceção oficial; hard delete restrito a lista branca. Atualizar quando uma convenção nova for adotada explicitamente.*
