using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// 1.04 PASS 2 — Acesso multi-tenant (identidade global + membership N:N usuário↔tenant).
    /// Cobre: login com N tenants exige seleção; seleção de tenant emite token escopado; vínculo
    /// inexistente é barrado; single-tenant (uma membership) entra direto; self-register cria a membership.
    /// </summary>
    public class MultiTenantAcessoTests
    {
        private static readonly IPasswordHasher _hasher = new Epros.Infrastructure.Services.Pbkdf2PasswordHasher();

        private (ServiceProvider Provider, Epros.Shared.Security.IEprosTokenService Token) CreateServiceProvider(string databaseName)
        {
            var services = new ServiceCollection();
            var connectionString = PostgresTestDb.CreateDatabase(databaseName);

            // Semeamos dados cross-tenant: com provider "system", o ContextBase preserva o TenantId de cada
            // entidade (não sobrescreve pelo tenant corrente), permitindo montar memberships em vários tenants.
            var tenantProvider = new FixedTenantProvider("system");
            var currentUser = new FixedCurrentUser("system-test");

            var optAplicativo = PostgresTestDb.BuildOptions<ContextAplicativo>(connectionString, tenantProvider);
            var optGestaoClientes = PostgresTestDb.BuildOptions<ContextGestaoClientes>(connectionString, tenantProvider);

            services.AddSingleton(new ContextAplicativo(optAplicativo, tenantProvider, currentUser));
            services.AddSingleton(new ContextGestaoClientes(optGestaoClientes, tenantProvider, currentUser));
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton<ICurrentUser>(currentUser);
            services.AddSingleton<IPasswordHasher, Epros.Infrastructure.Services.Pbkdf2PasswordHasher>();
            var tokenService = new Epros.Shared.Security.EprosTokenService("epros-test-signing-key-0123456789-abcdef");
            services.AddSingleton<Epros.Shared.Security.IEprosTokenService>(tokenService);
            services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AutenticarUsuarioCommandHandler).Assembly));

            return (services.BuildServiceProvider(), tokenService);
        }

        private static async Task SeedIdentidadeComMembershipsAsync(
            ContextAplicativo ctx, Guid usuarioId, string email, string senha, params (string tenant, PapelAcessoTenant papel)[] tenants)
        {
            var homeTenant = tenants[0].tenant;
            var usuario = new Usuario(homeTenant, "Contador Parceiro", email, _hasher.Hash(senha), UsuarioTipo.Company, "system");
            // Força o Id conhecido para simplificar as asserções.
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                .GetProperty(nameof(Epros.Shared.Domain.Entities.EntidadeSaaSBase.Id))!
                .SetValue(usuario, usuarioId);
            ctx.Usuarios.Add(usuario);

            foreach (var (tenant, papel) in tenants)
            {
                ctx.AcessosUsuarioTenant.Add(new AcessoUsuarioTenant(tenant, usuarioId, papel, "system"));
            }
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task Login_Com_Varios_Tenants_Deve_Exigir_Selecao_De_Tenant()
        {
            var (sp, _) = CreateServiceProvider("db_mt_login_varios");
            var ctx = sp.GetRequiredService<ContextAplicativo>();
            var mediator = sp.GetRequiredService<IMediator>();

            var usuarioId = Guid.NewGuid();
            await SeedIdentidadeComMembershipsAsync(ctx, usuarioId, "contador@parceiro.com", "SenhaForte@123",
                ("tenant-a", PapelAcessoTenant.Proprietario), ("tenant-b", PapelAcessoTenant.ContadorParceiro));

            var result = await mediator.Send(new AutenticarUsuarioCommand("contador@parceiro.com", "SenhaForte@123", "127.0.0.1", "Chrome"));

            Assert.True(result.Sucesso);
            var dto = Assert.IsType<AuthResponseDto>(result.Dados);
            Assert.True(dto.ExigeSelecaoTenant);
            Assert.False(dto.ExigeSelecaoEmpresa);
            Assert.NotNull(dto.Tenants);
            Assert.Equal(2, dto.Tenants!.Count);
            Assert.Contains(dto.Tenants, t => t.TenantId == "tenant-a" && t.Papel == "Proprietario");
            Assert.Contains(dto.Tenants, t => t.TenantId == "tenant-b" && t.Papel == "ContadorParceiro");
        }

        [Fact]
        public async Task SelecionarTenant_Com_Membership_Valida_Deve_Emitir_Token_Escopado()
        {
            var (sp, tokenService) = CreateServiceProvider("db_mt_selecionar_ok");
            var ctx = sp.GetRequiredService<ContextAplicativo>();
            var mediator = sp.GetRequiredService<IMediator>();

            var usuarioId = Guid.NewGuid();
            await SeedIdentidadeComMembershipsAsync(ctx, usuarioId, "contador2@parceiro.com", "SenhaForte@123",
                ("tenant-a", PapelAcessoTenant.Proprietario), ("tenant-b", PapelAcessoTenant.ContadorParceiro));

            var result = await mediator.Send(new SelecionarTenantCommand(usuarioId, "tenant-b"));

            Assert.True(result.Sucesso);
            var dto = Assert.IsType<AuthResponseDto>(result.Dados);
            Assert.Equal("tenant-b", dto.TenantId);

            var principal = tokenService.Validar(dto.Token);
            Assert.NotNull(principal);
            Assert.Equal("tenant-b", principal!.FindFirst("tenantId")?.Value);
            Assert.Equal(usuarioId.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }

        [Fact]
        public async Task SelecionarTenant_Sem_Membership_Deve_Falhar()
        {
            var (sp, _) = CreateServiceProvider("db_mt_selecionar_sem_acesso");
            var ctx = sp.GetRequiredService<ContextAplicativo>();
            var mediator = sp.GetRequiredService<IMediator>();

            var usuarioId = Guid.NewGuid();
            await SeedIdentidadeComMembershipsAsync(ctx, usuarioId, "so-um@parceiro.com", "SenhaForte@123",
                ("tenant-a", PapelAcessoTenant.Proprietario));

            var result = await mediator.Send(new SelecionarTenantCommand(usuarioId, "tenant-b"));

            Assert.False(result.Sucesso);
            Assert.Contains("O usuário não possui acesso ao tenant informado.", result.Erros);
        }

        [Fact]
        public async Task SelecionarTenant_Com_Membership_Inativa_Deve_Falhar()
        {
            var (sp, _) = CreateServiceProvider("db_mt_selecionar_inativa");
            var ctx = sp.GetRequiredService<ContextAplicativo>();
            var mediator = sp.GetRequiredService<IMediator>();

            var usuarioId = Guid.NewGuid();
            var usuario = new Usuario("tenant-a", "Contador", "inativa@parceiro.com", _hasher.Hash("SenhaForte@123"), UsuarioTipo.Company, "system");
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase).GetProperty("Id")!.SetValue(usuario, usuarioId);
            ctx.Usuarios.Add(usuario);
            var acesso = new AcessoUsuarioTenant("tenant-b", usuarioId, PapelAcessoTenant.ContadorParceiro, "system");
            acesso.Desativar("system");
            ctx.AcessosUsuarioTenant.Add(acesso);
            await ctx.SaveChangesAsync();

            var result = await mediator.Send(new SelecionarTenantCommand(usuarioId, "tenant-b"));

            Assert.False(result.Sucesso);
            Assert.Contains("O usuário não possui acesso ao tenant informado.", result.Erros);
        }

        [Fact]
        public async Task Login_Com_Um_Tenant_Via_Membership_Entra_Direto()
        {
            var (sp, _) = CreateServiceProvider("db_mt_login_um");
            var ctx = sp.GetRequiredService<ContextAplicativo>();
            var mediator = sp.GetRequiredService<IMediator>();

            var usuarioId = Guid.NewGuid();
            await SeedIdentidadeComMembershipsAsync(ctx, usuarioId, "unico@tenant.com", "SenhaForte@123",
                ("tenant-solo", PapelAcessoTenant.Proprietario));

            var result = await mediator.Send(new AutenticarUsuarioCommand("unico@tenant.com", "SenhaForte@123", "127.0.0.1", "Chrome"));

            Assert.True(result.Sucesso);
            var dto = Assert.IsType<AuthResponseDto>(result.Dados);
            Assert.False(dto.ExigeSelecaoTenant);
            Assert.Equal("tenant-solo", dto.TenantId);
        }

        [Fact]
        public async Task RegistrarNovoTenant_Deve_Criar_Membership_Proprietario()
        {
            var (sp, _) = CreateServiceProvider("db_mt_registro_membership");
            var mediator = sp.GetRequiredService<IMediator>();
            var contextApp = sp.GetRequiredService<ContextAplicativo>();
            var contextGestao = sp.GetRequiredService<ContextGestaoClientes>();

            // Catálogo mínimo para REG-036 (país → UF → município).
            var pais = new Pais("Brasil", "BR", "BRA", "076", "Brasília", "+55", "system");
            contextGestao.Paises.Add(pais);
            await contextGestao.SaveChangesAsync();
            var uf = new Subdivisao(pais.Id, "BR-PR", "Paraná", TipoSubdivisao.Estado, null, "system");
            contextGestao.Subdivisoes.Add(uf);
            await contextGestao.SaveChangesAsync();
            var municipio = new Municipio(pais.Id, uf.Id, "Cianorte", 4105508, null, null, "system", "PR");
            contextGestao.Municipios.Add(municipio);
            await contextGestao.SaveChangesAsync();

            var command = new RegistrarNovoTenantCommand(
                "Empresa Membership Ltda", "12345678000195", "Admin MT", "admin@mt.com", "SenhaForte@123",
                CodigoIbgeMunicipio: 4105508, Telefone: "44999990000", TipoTelefone: "Celular");

            var result = await mediator.Send(command);
            Assert.True(result.Sucesso);

            var dados = Assert.IsType<Dictionary<string, object>>(result.Dados);
            var tenantId = (string)dados["TenantId"];
            var usuarioId = (Guid)dados["UsuarioAdminId"];

            var membership = await contextApp.AcessosUsuarioTenant
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.UsuarioId == usuarioId && a.TenantId == tenantId);

            Assert.NotNull(membership);
            Assert.True(membership!.Ativo);
            Assert.Equal(PapelAcessoTenant.Proprietario, membership.Papel);
        }

        private sealed class FixedTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public FixedTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
            public bool EhTenantDemo() => false;
        }

        private sealed class FixedCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public FixedCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
