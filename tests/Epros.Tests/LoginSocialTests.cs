using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Services;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// 1.04 PASS 3 — Login social (OAuth/OIDC, Google + Microsoft). Cobre as três resoluções do callback:
    /// vínculo (provedor, sub) existente loga; e-mail verificado casa com usuário → vincula e loga;
    /// identidade nova / e-mail não verificado → encaminha ao onboarding (não cria tenant fiscal).
    /// Também: provedor não configurado e state inválido. O provedor OIDC é substituído por um fake
    /// (não há rede nos testes); o state real é obtido chamando o /start de verdade.
    /// </summary>
    public class LoginSocialTests
    {
        private static readonly IPasswordHasher _hasher = new Epros.Infrastructure.Services.Pbkdf2PasswordHasher();

        private sealed class FakeOidcSocialClient : IOidcSocialClient
        {
            public PerfilSocial? PerfilRetorno { get; set; }

            public Task<string> MontarUrlAutorizacaoAsync(ProvedorSocialOptions opcoes, string state, string nonce, CancellationToken ct)
                => Task.FromResult($"https://fake.provider/authorize?state={state}&nonce={nonce}");

            public Task<PerfilSocial> TrocarCodigoPorPerfilAsync(ProvedorSocialOptions opcoes, ProvedorSocial provedor, string code, string nonceEsperado, CancellationToken ct)
            {
                if (PerfilRetorno == null) throw new OidcSocialException("perfil não configurado no fake.");
                return Task.FromResult(PerfilRetorno);
            }
        }

        private (ServiceProvider Provider, FakeOidcSocialClient Oidc) CreateServiceProvider(string databaseName, bool googleConfigurado = true)
        {
            var services = new ServiceCollection();
            var connectionString = PostgresTestDb.CreateDatabase(databaseName);

            var tenantProvider = new FixedTenantProvider("system");
            var currentUser = new FixedCurrentUser("system-test");

            var optAplicativo = PostgresTestDb.BuildOptions<ContextAplicativo>(connectionString, tenantProvider);
            var optGestaoClientes = PostgresTestDb.BuildOptions<ContextGestaoClientes>(connectionString, tenantProvider);

            services.AddSingleton(new ContextAplicativo(optAplicativo, tenantProvider, currentUser));
            services.AddSingleton(new ContextGestaoClientes(optGestaoClientes, tenantProvider, currentUser));
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton<ICurrentUser>(currentUser);
            services.AddSingleton<IPasswordHasher, Epros.Infrastructure.Services.Pbkdf2PasswordHasher>();
            services.AddSingleton<Epros.Shared.Security.IEprosTokenService>(new Epros.Shared.Security.EprosTokenService("epros-test-signing-key-0123456789-abcdef"));
            services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();
            services.AddMemoryCache();

            var opcoes = new AutenticacaoSocialOptions
            {
                Google = googleConfigurado
                    ? new ProvedorSocialOptions { ClientId = "cid", ClientSecret = "secret", RedirectUri = "http://localhost/cb", Authority = "https://accounts.google.com" }
                    : new ProvedorSocialOptions()
            };
            services.AddSingleton<IOptions<AutenticacaoSocialOptions>>(Options.Create(opcoes));

            var oidc = new FakeOidcSocialClient();
            services.AddSingleton<IOidcSocialClient>(oidc);

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CallbackLoginSocialCommandHandler).Assembly));

            return (services.BuildServiceProvider(), oidc);
        }

        private static async Task<Usuario> SeedUsuarioAsync(ContextAplicativo ctx, string tenant, string email)
        {
            var usuario = new Usuario(tenant, "Fulano Social", email, _hasher.Hash("SenhaForte@123"), UsuarioTipo.Company, "system");
            ctx.Usuarios.Add(usuario);
            ctx.AcessosUsuarioTenant.Add(new AcessoUsuarioTenant(tenant, usuario.Id, PapelAcessoTenant.Proprietario, "system"));
            await ctx.SaveChangesAsync();
            return usuario;
        }

        /// <summary>Chama /start de verdade e devolve o state gerado (extraído da URL do fake).</summary>
        private static async Task<string> ObterStateViaStartAsync(IMediator mediator, string provedor = "google")
        {
            var result = await mediator.Send(new IniciarLoginSocialCommand(provedor));
            Assert.True(result.Sucesso, string.Join(";", result.Erros));
            var dto = Assert.IsType<IniciarLoginSocialDto>(result.Dados);
            var marcador = "state=";
            var i = dto.UrlAutorizacao.IndexOf(marcador, StringComparison.Ordinal);
            Assert.True(i >= 0);
            var resto = dto.UrlAutorizacao.Substring(i + marcador.Length);
            var fim = resto.IndexOf('&');
            return fim >= 0 ? resto.Substring(0, fim) : resto;
        }

        [Fact]
        public async Task Start_Provedor_Nao_Configurado_Deve_Falhar()
        {
            var (sp, _) = CreateServiceProvider("db_social_start_naoconfig", googleConfigurado: false);
            var mediator = sp.GetRequiredService<IMediator>();

            var result = await mediator.Send(new IniciarLoginSocialCommand("google"));

            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("não está configurado"));
        }

        [Fact]
        public async Task Callback_State_Invalido_Deve_Falhar()
        {
            var (sp, oidc) = CreateServiceProvider("db_social_state_invalido");
            var mediator = sp.GetRequiredService<IMediator>();
            oidc.PerfilRetorno = new PerfilSocial("sub-x", "x@y.com", true, "X");

            var result = await mediator.Send(new CallbackLoginSocialCommand("google", "code", "state-que-nao-existe", null, "127.0.0.1", "Chrome"));

            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("State inválido"));
        }

        [Fact]
        public async Task Callback_Com_Vinculo_Existente_Deve_Logar()
        {
            var (sp, oidc) = CreateServiceProvider("db_social_vinculo_existente");
            var ctx = sp.GetRequiredService<ContextAplicativo>();
            var mediator = sp.GetRequiredService<IMediator>();

            var usuario = await SeedUsuarioAsync(ctx, "tenant-soc-a", "vinculado@social.com");
            ctx.IdentidadesExternas.Add(new IdentidadeExterna("tenant-soc-a", usuario.Id, ProvedorSocial.Google, "google-sub-abc", "vinculado@social.com", "system"));
            await ctx.SaveChangesAsync();

            oidc.PerfilRetorno = new PerfilSocial("google-sub-abc", "vinculado@social.com", true, "Vinculado");
            var state = await ObterStateViaStartAsync(mediator);

            var result = await mediator.Send(new CallbackLoginSocialCommand("google", "code", state, null, "127.0.0.1", "Chrome"));

            Assert.True(result.Sucesso, string.Join(";", result.Erros));
            var dto = Assert.IsType<AuthResponseDto>(result.Dados);
            Assert.Equal(usuario.Id, dto.UsuarioId);
            Assert.Equal("tenant-soc-a", dto.TenantId);
        }

        [Fact]
        public async Task Callback_Email_Verificado_Casa_Com_Usuario_Deve_Vincular_E_Logar()
        {
            var (sp, oidc) = CreateServiceProvider("db_social_vincula_por_email");
            var ctx = sp.GetRequiredService<ContextAplicativo>();
            var mediator = sp.GetRequiredService<IMediator>();

            var usuario = await SeedUsuarioAsync(ctx, "tenant-soc-b", "casa@social.com");

            oidc.PerfilRetorno = new PerfilSocial("google-sub-novo", "casa@social.com", true, "Casa");
            var state = await ObterStateViaStartAsync(mediator);

            var result = await mediator.Send(new CallbackLoginSocialCommand("google", "code", state, null, "127.0.0.1", "Chrome"));

            Assert.True(result.Sucesso, string.Join(";", result.Erros));
            Assert.IsType<AuthResponseDto>(result.Dados);

            // Vínculo foi criado.
            var vinculo = await ctx.IdentidadesExternas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(i => i.Provedor == ProvedorSocial.Google && i.SubjectId == "google-sub-novo");
            Assert.NotNull(vinculo);
            Assert.Equal(usuario.Id, vinculo!.UsuarioId);
            Assert.True(vinculo.Ativo);
        }

        [Fact]
        public async Task Callback_Identidade_Nova_Deve_Encaminhar_Ao_Onboarding()
        {
            var (sp, oidc) = CreateServiceProvider("db_social_identidade_nova");
            var mediator = sp.GetRequiredService<IMediator>();

            // E-mail verificado, mas nenhum usuário existente com esse e-mail.
            oidc.PerfilRetorno = new PerfilSocial("google-sub-sozinho", "ninguem@social.com", true, "Ninguém");
            var state = await ObterStateViaStartAsync(mediator);

            var result = await mediator.Send(new CallbackLoginSocialCommand("google", "code", state, null, "127.0.0.1", "Chrome"));

            Assert.True(result.Sucesso, string.Join(";", result.Erros));
            var dto = Assert.IsType<LoginSocialPendenteDto>(result.Dados);
            Assert.True(dto.PrecisaCompletarCadastro);
            Assert.Equal("ninguem@social.com", dto.Email);
            Assert.Equal("google-sub-sozinho", dto.SubjectId);
        }

        [Fact]
        public async Task Callback_Email_Nao_Verificado_Nao_Vincula_A_Conta_Existente()
        {
            var (sp, oidc) = CreateServiceProvider("db_social_email_nao_verificado");
            var ctx = sp.GetRequiredService<ContextAplicativo>();
            var mediator = sp.GetRequiredService<IMediator>();

            await SeedUsuarioAsync(ctx, "tenant-soc-c", "existente@social.com");

            // Mesmo e-mail de um usuário existente, porém NÃO verificado pelo provedor: não pode assumir a conta.
            oidc.PerfilRetorno = new PerfilSocial("google-sub-naover", "existente@social.com", false, "NaoVerificado");
            var state = await ObterStateViaStartAsync(mediator);

            var result = await mediator.Send(new CallbackLoginSocialCommand("google", "code", state, null, "127.0.0.1", "Chrome"));

            Assert.True(result.Sucesso, string.Join(";", result.Erros));
            var dto = Assert.IsType<LoginSocialPendenteDto>(result.Dados);
            Assert.True(dto.PrecisaCompletarCadastro);

            // Não criou vínculo à conta existente.
            var existeVinculo = await ctx.IdentidadesExternas
                .IgnoreQueryFilters()
                .AnyAsync(i => i.SubjectId == "google-sub-naover");
            Assert.False(existeVinculo);
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
