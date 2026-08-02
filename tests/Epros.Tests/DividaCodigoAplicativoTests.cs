using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Application.Services;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Epros.Tests
{
    // ---------- Helpers compartilhados ----------
    internal sealed class DcTenantProvider : ITenantProvider
    {
        private readonly string _t;
        public DcTenantProvider(string t) => _t = t;
        public string GetTenantId() => _t;
        public bool EhTenantDemo() => false;
    }

    internal sealed class DcCurrentUser : ICurrentUser
    {
        private readonly string _u;
        public DcCurrentUser(string u) => _u = u;
        public string? GetUserId() => _u;
        public string? GetUserName() => "Dc Test";
        public string? GetUserEmail() => "dc@epros.com";
    }

    /// <summary>
    /// REG-017 — LOCK OTIMISTA (xmin como concurrency token). Contra um Postgres real (Testcontainers),
    /// prova que duas leituras concorrentes das entidades-chave, ao salvar, geram conflito no 2º save
    /// (DbUpdateConcurrencyException = o 409 do middleware) em vez de last-write-wins silencioso.
    /// </summary>
    public class LockOtimisticoReg017Tests
    {
        private const string Tenant = "tenant-lock-017";

        private static ContextGestaoClientes Ctx(string conn)
        {
            var tp = new DcTenantProvider(Tenant);
            var options = PostgresTestDb.BuildOptions<ContextGestaoClientes>(conn, tp);
            return new ContextGestaoClientes(options, tp, new DcCurrentUser("lock-user"));
        }

        [Fact]
        public async Task Atualizacao_Concorrente_De_Plano_Faz_Segundo_Save_Conflitar()
        {
            var conn = PostgresTestDb.CreateDatabase("db_lock_plano_017");

            Guid planoId;
            using (var seed = Ctx(conn))
            {
                var plano = new Plano("Plano Lock", 100m, Tenant, "seed");
                seed.Planos.Add(plano);
                await seed.SaveChangesAsync();
                planoId = plano.Id;
            }

            using var ctx1 = Ctx(conn);
            using var ctx2 = Ctx(conn);

            // Duas leituras independentes da MESMA linha: cada contexto captura o xmin corrente.
            var p1 = await ctx1.Planos.FirstAsync(p => p.Id == planoId);
            var p2 = await ctx2.Planos.FirstAsync(p => p.Id == planoId);

            // 1º save vence e muda o xmin da linha.
            p1.MarcarAlterado("u1");
            await ctx1.SaveChangesAsync();

            // 2º save carrega xmin obsoleto → UPDATE afeta 0 linhas → conflito de concorrência.
            p2.MarcarAlterado("u2");
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => ctx2.SaveChangesAsync());
        }

        [Fact]
        public async Task Atualizacao_Concorrente_De_Cliente_Faz_Segundo_Save_Conflitar()
        {
            var conn = PostgresTestDb.CreateDatabase("db_lock_cliente_017");

            Guid clienteId;
            using (var seed = Ctx(conn))
            {
                var plano = new Plano("Plano Lock C", 100m, Tenant, "seed");
                seed.Planos.Add(plano);
                var cliente = new Cliente("Cliente Lock", "22222222000122", "c@lock.com", plano.Id, Tenant, "seed");
                seed.Clientes.Add(cliente);
                await seed.SaveChangesAsync();
                clienteId = cliente.Id;
            }

            using var ctx1 = Ctx(conn);
            using var ctx2 = Ctx(conn);

            var c1 = await ctx1.Clientes.FirstAsync(c => c.Id == clienteId);
            var c2 = await ctx2.Clientes.FirstAsync(c => c.Id == clienteId);

            c1.MarcarAlterado("u1");
            await ctx1.SaveChangesAsync();

            c2.MarcarAlterado("u2");
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => ctx2.SaveChangesAsync());
        }
    }

    /// <summary>
    /// REG-016 — restauração GOVERNADA de soft-delete. Prova que o handler restaura uma entidade-chave
    /// soft-deleted (Cliente no control-plane; Usuario no identity), é IDEMPOTENTE (restaurar de novo ou
    /// restaurar algo não-deletado é no-op) e respeita o tenant.
    /// </summary>
    public class RestauracaoSoftDeleteReg016Tests
    {
        private const string Tenant = "tenant-restore-016";

        private (ContextGestaoClientes G, ContextAplicativo A) Contexts(string db)
        {
            var tp = new DcTenantProvider(Tenant);
            var cu = new DcCurrentUser("restore-op");
            var optG = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(db)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            var optA = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(db)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return (new ContextGestaoClientes(optG, tp, cu), new ContextAplicativo(optA, tp, cu));
        }

        private RestaurarEntidadeCommandHandler Handler(ContextGestaoClientes g, ContextAplicativo a)
            => new RestaurarEntidadeCommandHandler(g, a, new DcTenantProvider(Tenant), new DcCurrentUser("restore-op"));

        [Fact]
        public async Task Restaura_Cliente_SoftDeleted_E_Eh_Idempotente()
        {
            var db = Guid.NewGuid().ToString();
            Guid clienteId;

            // Cria e soft-deleta um Cliente.
            {
                var (g, _) = Contexts(db);
                var plano = new Plano("Plano R", 100m, Tenant, "seed");
                g.Planos.Add(plano);
                var cliente = new Cliente("Cliente R", "33333333000133", "r@cli.com", plano.Id, Tenant, "seed");
                g.Clientes.Add(cliente);
                await g.SaveChangesAsync();
                clienteId = cliente.Id;

                g.Clientes.Remove(cliente); // vira soft-delete (não é IHardDeletable)
                await g.SaveChangesAsync();

                // Confirma que sumiu do filtro padrão, mas existe soft-deleted.
                Assert.Null(await g.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId));
                var morto = await g.Clientes.IgnoreQueryFilters().FirstAsync(c => c.Id == clienteId);
                Assert.NotNull(morto.DeletadoEm);
            }

            // Restaura via handler.
            {
                var (g, a) = Contexts(db);
                var r = await Handler(g, a).Handle(new RestaurarEntidadeCommand(EntidadeRestauravel.Cliente, clienteId), CancellationToken.None);
                Assert.True(r.Sucesso);

                var vivo = await g.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId);
                Assert.NotNull(vivo);
                Assert.True(vivo!.EstaAtivo());
            }

            // Idempotência: restaurar de novo (já ativo) é no-op de sucesso.
            {
                var (g, a) = Contexts(db);
                var r2 = await Handler(g, a).Handle(new RestaurarEntidadeCommand(EntidadeRestauravel.Cliente, clienteId), CancellationToken.None);
                Assert.True(r2.Sucesso); // no-op, mas sucesso
            }
        }

        [Fact]
        public async Task Restaura_Usuario_SoftDeleted()
        {
            var db = Guid.NewGuid().ToString();
            Guid usuarioId;

            {
                var (_, a) = Contexts(db);
                var u = new Usuario(Tenant, "User R", "userr@teste.com", "Senha@123", UsuarioTipo.Company, "seed");
                a.Usuarios.Add(u);
                await a.SaveChangesAsync();
                usuarioId = u.Id;

                a.Usuarios.Remove(u);
                await a.SaveChangesAsync();
                Assert.Null(await a.Usuarios.FirstOrDefaultAsync(x => x.Id == usuarioId));
            }

            {
                var (g, a) = Contexts(db);
                var r = await Handler(g, a).Handle(new RestaurarEntidadeCommand(EntidadeRestauravel.Usuario, usuarioId), CancellationToken.None);
                Assert.True(r.Sucesso);
                var vivo = await a.Usuarios.FirstOrDefaultAsync(x => x.Id == usuarioId);
                Assert.NotNull(vivo);
                Assert.True(vivo!.EstaAtivo());
            }
        }

        [Fact]
        public async Task Restaurar_Entidade_Nao_Deletada_Eh_NoOp()
        {
            var db = Guid.NewGuid().ToString();
            var (g, a) = Contexts(db);
            var plano = new Plano("Plano Vivo", 100m, Tenant, "seed");
            g.Planos.Add(plano);
            await g.SaveChangesAsync();

            var r = await Handler(g, a).Handle(new RestaurarEntidadeCommand(EntidadeRestauravel.Plano, plano.Id), CancellationToken.None);
            Assert.True(r.Sucesso); // no-op de sucesso; nada a restaurar
        }
    }

    /// <summary>
    /// 1.10 — o MenuCatalogoSeeder popula o catálogo com CapacidadeRequerida, de modo que GET /menu projete
    /// o menu dinâmico real (não o fallback estático). Prova: o seed é idempotente, cria itens COM capacidade,
    /// e um usuário com a capacidade real do item passa a vê-lo no menu projetado.
    /// </summary>
    public class MenuCatalogoSeedTests
    {
        private const string Tenant = "tenant-menu-seed";

        private ServiceProvider CreateProvider(string db)
        {
            var services = new ServiceCollection();
            var optApp = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(db)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            var optGestao = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(db)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var tp = new DcTenantProvider(Tenant);
            var cu = new DcCurrentUser(Guid.NewGuid().ToString());

            services.AddSingleton(new ContextAplicativo(optApp, tp, cu));
            services.AddSingleton(new ContextGestaoClientes(optGestao, tp, cu));
            services.AddSingleton<ITenantProvider>(tp);
            services.AddSingleton<ICurrentUser>(cu);
            services.AddMemoryCache();
            services.AddSingleton<IPermissaoCacheManager, PermissaoCacheManager>();
            services.AddScoped<ICapacidadesEfetivasService, CapacidadesEfetivasService>();
            services.AddSingleton<Epros.Shared.Security.IEprosTokenService>(
                new Epros.Shared.Security.EprosTokenService("epros-test-signing-key-0123456789-abcdef"));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ObterMenuDoUsuarioQuery).Assembly));

            return services.BuildServiceProvider();
        }

        // Assembly da API onde vivem os controllers + o seeder (para descobrir as capacidades reais).
        private static System.Reflection.Assembly ApiAssembly => typeof(Epros.API.Seed.MenuCatalogoSeeder).Assembly;

        [Fact]
        public async Task Seed_Cria_Itens_Com_Capacidade_E_Eh_Idempotente()
        {
            var provider = CreateProvider(Guid.NewGuid().ToString());
            var ctx = provider.GetRequiredService<ContextGestaoClientes>();

            var caps1 = await Epros.API.Seed.MenuCatalogoSeeder.SeedAsync(ctx, ApiAssembly);
            Assert.True(caps1 > 0, "o seed deve resolver ao menos uma capacidade real.");

            var menus = await ctx.Menus.CountAsync();
            var itensComCap = await ctx.MenusItensNivel1.CountAsync(i => i.CapacidadeRequerida != null);
            Assert.True(menus > 0);
            Assert.True(itensComCap > 0, "deve haver itens de menu com CapacidadeRequerida preenchida.");

            // Idempotência: rodar de novo não duplica grupos nem itens.
            await Epros.API.Seed.MenuCatalogoSeeder.SeedAsync(ctx, ApiAssembly);
            Assert.Equal(menus, await ctx.Menus.CountAsync());
            Assert.Equal(itensComCap, await ctx.MenusItensNivel1.CountAsync(i => i.CapacidadeRequerida != null));
        }

        [Fact]
        public async Task Menu_Do_Usuario_Retorna_Item_Real_Apos_Seed()
        {
            var provider = CreateProvider(Guid.NewGuid().ToString());
            var mediator = provider.GetRequiredService<IMediator>();
            var ctx = provider.GetRequiredService<ContextGestaoClientes>();
            var ctxApp = provider.GetRequiredService<ContextAplicativo>();

            // Tenant + empresa + usuário vinculado.
            var plano = new Plano("Plano Menu", 100m, Tenant, "system");
            var cliente = new Cliente("Razao Menu", "12345678000100", "menu@cli.com", plano.Id, Tenant, "system");
            ctx.Planos.Add(plano);
            ctx.Clientes.Add(cliente);
            var endereco = new Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco("Rua", "1", "", "Centro", "00000000", "Cidade", "SP");
            var empresa = new Empresa("Empresa Menu LTDA", "Fantasia", "12345678000100", null, null, null, null,
                RegimeTributario.SimplesNacional, RegimeApuracao.Cumulativo,
                null, null, null, null, null, null, null, null, null, null, endereco, Tenant, "system", false);
            ctx.Empresas.Add(empresa);
            var usuario = new Usuario(Tenant, "User", $"u{Guid.NewGuid():N}@teste.com", "Senha@123", UsuarioTipo.Company, "system");
            ctxApp.Usuarios.Add(usuario);
            ctxApp.UsuariosEmpresas.Add(new UsuarioEmpresa(Tenant, usuario.Id, empresa.Id, null, false, "system"));
            await ctx.SaveChangesAsync();
            await ctxApp.SaveChangesAsync();

            // Semeia o catálogo real de menu (com capacidades resolvidas).
            await Epros.API.Seed.MenuCatalogoSeeder.SeedAsync(ctx, ApiAssembly);

            // Concede ao usuário a capacidade REAL de um item semeado (Empresas → empresa:ler).
            var nome = Epros.API.Seed.CapacidadeCatalogoSeeder.NomeCapacidade("Empresa", "Ler");
            var cap = new Capacidade(nome, "Empresa Ler", "Empresa", null, nome, Tenant, "seed");
            ctx.Capacidades.Add(cap);
            var papel = new Papel("Papel Empresa", "p", null, true, false, null, null, null, Tenant, "seed");
            ctx.Papeis.Add(papel);
            await ctx.SaveChangesAsync();
            ctx.PapeisCapacidades.Add(new PapelCapacidade(papel.Id, cap.Id, Tenant, "seed"));
            ctx.UsuariosPapeis.Add(new UsuarioPapel(usuario.Id, papel.Id, "Usuario", Tenant, "seed", empresaId: empresa.Id));
            await ctx.SaveChangesAsync();

            var result = await mediator.Send(new ObterMenuDoUsuarioQuery(usuario.Id, empresa.Id));
            Assert.True(result.Sucesso);
            var resp = Assert.IsType<MenuDoUsuarioResponseDto>(result.Dados);

            // O menu dinâmico traz um item com a capacidade real semeada (não o fallback).
            var todasCaps = resp.Menu
                .SelectMany(m => m.Itens)
                .Select(i => i.CapacidadeRequerida)
                .Where(c => c != null)
                .ToList();
            Assert.Contains(nome, todasCaps);
        }
    }
}
