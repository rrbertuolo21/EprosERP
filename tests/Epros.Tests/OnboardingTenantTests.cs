using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Epros.Tests
{
    public class OnboardingTenantTests
    {
        private (ServiceProvider Provider, TestTenantProvider TenantProvider) CreateServiceProvider(string databaseName, string tenantInicial = "system")
        {
            var services = new ServiceCollection();

            // O onboarding roda numa transação relacional real (BeginTransaction/UseTransaction) que o
            // provider InMemory não suporta. Usa-se um Postgres real e isolado.
            var connectionString = PostgresTestDb.CreateDatabase(databaseName);

            var tenantProvider = new TestTenantProvider(tenantInicial);
            var currentUser = new TestCurrentUser("system-test");

            var optAplicativo = PostgresTestDb.BuildOptions<ContextAplicativo>(connectionString, tenantProvider);
            var optGestaoClientes = PostgresTestDb.BuildOptions<ContextGestaoClientes>(connectionString, tenantProvider);

            services.AddSingleton(new ContextAplicativo(optAplicativo, tenantProvider, currentUser));
            services.AddSingleton(new ContextGestaoClientes(optGestaoClientes, tenantProvider, currentUser));
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton<ICurrentUser>(currentUser);
            services.AddSingleton<IPasswordHasher, Epros.Infrastructure.Services.Pbkdf2PasswordHasher>();
            services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(AutenticarUsuarioCommandHandler).Assembly);
            });

            return (services.BuildServiceProvider(), tenantProvider);
        }

        [Fact]
        public async Task RegistrarNovoTenant_Deve_Criar_Bases_Operacionais_E_Configuracoes_Com_Sucesso()
        {
            // Arrange
            var (serviceProvider, _) = CreateServiceProvider("db_onboarding_sucesso", "system");
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var contextApp = serviceProvider.GetRequiredService<ContextAplicativo>();
            var contextGestao = serviceProvider.GetRequiredService<ContextGestaoClientes>();

            // REG-036: o self-register exige município IBGE cadastrado. Semeamos o catálogo mínimo
            // (País → Subdivisão/UF → Município) para o código IBGE usado no comando.
            var pais = new Pais("Brasil", "BR", "BRA", "076", "Brasília", "+55", "system");
            contextGestao.Paises.Add(pais);
            await contextGestao.SaveChangesAsync();
            var uf = new Subdivisao(pais.Id, "BR-PR", "Paraná", TipoSubdivisao.Estado, null, "system");
            contextGestao.Subdivisoes.Add(uf);
            await contextGestao.SaveChangesAsync();
            var municipio = new Municipio(pais.Id, uf.Id, "Cianorte", 4105508, null, null, "system", "PR");
            contextGestao.Municipios.Add(municipio);
            await contextGestao.SaveChangesAsync();

            // REG-036: documento fiscal válido (CNPJ), município IBGE válido, telefone + tipo.
            var command = new RegistrarNovoTenantCommand(
                "Nova Empresa Ltda", "12345678000195", "Admin Onboarding", "admin@onboard.com", "SenhaForte@123",
                CodigoIbgeMunicipio: 4105508, Telefone: "44999990000", TipoTelefone: "Celular");

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.True(result.Sucesso);

            var dados = Assert.IsType<Dictionary<string, object>>(result.Dados);
            string tenantId = (string)dados["TenantId"];
            Guid empresaId = (Guid)dados["EmpresaId"];

            // 1. Verificar se PessoaGrupo foi semeado
            var pessoaGrupo = await contextGestao.PessoaGrupos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.TenantId == tenantId);
            Assert.NotNull(pessoaGrupo);
            Assert.Contains("Nova Empresa Ltda", pessoaGrupo.Descricao);

            // 2. Verificar se a Empresa foi persistida com o PessoaGrupo real associado
            var empresa = await contextGestao.Empresas.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == empresaId);
            Assert.NotNull(empresa);
            Assert.Equal(pessoaGrupo.Id, empresa.PessoaGrupoId);
            // REG-036: IDs fiscais/agrupadores NÃO são mais fabricados com Guid.NewGuid() (placeholder
            // para entidade inexistente). Ficam nulos até serem semeados no onboarding real.
            Assert.Null(empresa.ProdutoGrupoId);
            Assert.Null(empresa.PlanoContasFinanceiroId);
            Assert.Null(empresa.TributarioGrupoId);
            // Município IBGE real refletido no endereço (cidade/UF do catálogo).
            Assert.Equal("Cianorte", empresa.Endereco.Cidade);
            Assert.Equal("PR", empresa.Endereco.Estado);

            // 3. Verificar se ConfiguracaoEmpresa foi criada
            var config = await contextApp.ConfiguracoesEmpresas.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.EmpresaId == empresaId);
            Assert.NotNull(config);
            Assert.Equal("Nova Empresa Ltda", config.Nome);
            Assert.Equal("admin@onboard.com", config.Email);

            // 4. Verificar se o AnoFinanceiro foi criado
            var ano = await contextApp.AnosFinanceiros.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.TenantId == tenantId);
            Assert.NotNull(ano);
            Assert.True(ano.IsActive);
            Assert.True(ano.ToDate > ano.FromDate);

            // 5. Verificar se os idiomas padrão foram criados
            var idiomas = await contextApp.Idiomas.IgnoreQueryFilters().Where(i => i.TenantId == tenantId).ToListAsync();
            Assert.Equal(2, idiomas.Count);
            Assert.Contains(idiomas, i => i.Code == "en");
            Assert.Contains(idiomas, i => i.Code == "pt-br");
        }

        [Fact]
        public async Task SalvarConfiguracaoEmpresa_Deve_Atualizar_Preferencias_Com_Sucesso()
        {
            // Arrange
            var (serviceProvider, tenantProvider) = CreateServiceProvider("db_onboarding_config", "tenant-1");
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var contextApp = serviceProvider.GetRequiredService<ContextAplicativo>();

            var empresaId = Guid.NewGuid();
            var configOriginal = new ConfiguracaoEmpresa("tenant-1", empresaId, "Original", "org@org.com", null, null, 1, "DD-MM-YYYY", 1, 0m, 1, 1, null, null, null, "system");
            contextApp.ConfiguracoesEmpresas.Add(configOriginal);
            await contextApp.SaveChangesAsync();

            var command = new SalvarConfiguracaoEmpresaCommand(
                EmpresaId: empresaId,
                Nome: "Empresa Atualizada",
                Email: "atualizada@empresa.com",
                Telefone: "4499999999",
                Endereco: "Rua Teste, 100",
                TimeZoneId: 2,
                DateFormat: "YYYY-MM-DD",
                CurrencyId: 2,
                VatPercentage: 15.5m,
                VatType: 2,
                CurrencyPosition: 2,
                FooterText: "Rodapé Personalizado",
                Logo: "base64logo",
                Favicon: "base64favicon"
            );

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.True(result.Sucesso);

            var configDb = await contextApp.ConfiguracoesEmpresas.FirstOrDefaultAsync(c => c.EmpresaId == empresaId);
            Assert.NotNull(configDb);
            Assert.Equal("Empresa Atualizada", configDb.Nome);
            Assert.Equal("atualizada@empresa.com", configDb.Email);
            Assert.Equal("Rua Teste, 100", configDb.Endereco);
            Assert.Equal(2, configDb.TimeZoneId);
            Assert.Equal("YYYY-MM-DD", configDb.DateFormat);
            Assert.Equal(15.5m, configDb.VatPercentage);
            Assert.Equal(2, configDb.VatType);
        }

        [Fact]
        public async Task DesabilitarIdioma_Ingles_Deve_Retornar_Falha()
        {
            // Arrange
            var (serviceProvider, _) = CreateServiceProvider("db_onboarding_lang", "tenant-1");
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var contextApp = serviceProvider.GetRequiredService<ContextAplicativo>();

            var langEn = new Idioma("tenant-1", "en", "English", "US", true, "system");
            contextApp.Idiomas.Add(langEn);
            await contextApp.SaveChangesAsync();

            var command = new DesabilitarIdiomaCommand("en");

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("idioma inglês (en) é o idioma base e não pode ser desativado"));
        }

        [Fact]
        public async Task Habilitar_E_Desabilitar_Idioma_Permitido_Deve_Funcionar_Corretamente()
        {
            // Arrange
            var (serviceProvider, _) = CreateServiceProvider("db_onboarding_lang_ok", "tenant-1");
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var contextApp = serviceProvider.GetRequiredService<ContextAplicativo>();

            var langEs = new Idioma("tenant-1", "es", "Spanish", "ES", false, "system");
            contextApp.Idiomas.Add(langEs);
            await contextApp.SaveChangesAsync();

            // Act 1: Habilitar
            var resultHab = await mediator.Send(new HabilitarIdiomaCommand("es"));
            Assert.True(resultHab.Sucesso);

            var langDb = await contextApp.Idiomas.FirstAsync(i => i.Code == "es");
            Assert.True(langDb.Enabled);

            // Act 2: Desabilitar
            var resultDes = await mediator.Send(new DesabilitarIdiomaCommand("es"));
            Assert.True(resultDes.Sucesso);

            langDb = await contextApp.Idiomas.FirstAsync(i => i.Code == "es");
            Assert.False(langDb.Enabled);
        }

        [Fact]
        public async Task ObterContextoSessao_Deve_Retornar_Contexto_Completo()
        {
            // Arrange
            var (serviceProvider, _) = CreateServiceProvider("db_onboarding_session", "tenant-1");
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var contextApp = serviceProvider.GetRequiredService<ContextAplicativo>();
            var contextGestao = serviceProvider.GetRequiredService<ContextGestaoClientes>();

            var usuarioId = Guid.NewGuid();
            // Cada Empresa precisa da sua PRÓPRIA instância de Endereco (owned type): compartilhar o mesmo
            // objeto entre dois donos faz o EF Core relacional gravar colunas nulas na segunda empresa
            // (viola NOT NULL em "logradouro"). O InMemory tolerava; o Postgres real não.
            var endereco1 = new Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco("Rua", "12", null, "Centro", "12345678", "Cianorte", "PR");
            var endereco2 = new Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco("Rua", "12", null, "Centro", "12345678", "Cianorte", "PR");
            var empresa1 = new Epros.Modules.GestaoClientes.Domain.Entities.Empresa("Empresa 1 SA", "Emp 1", "12345678901234", null, null, null, null, RegimeTributario.SimplesNacional, RegimeApuracao.Cumulativo, null, null, null, null, null, null, null, null, null, null, endereco1, "tenant-1", "system");
            var empresa2 = new Epros.Modules.GestaoClientes.Domain.Entities.Empresa("Empresa 2 SA", "Emp 2", "98765432109876", null, null, null, null, RegimeTributario.SimplesNacional, RegimeApuracao.Cumulativo, null, null, null, null, null, null, null, null, null, null, endereco2, "tenant-1", "system");

            var plano = new Plano("Plano Teste", 100m, null, 10, 5, null, "tenant-1", "system");
            contextGestao.Planos.Add(plano);

            var cliente = new Cliente("Razao Social", "12345678000195", "cliente@teste.com", plano.Id, null, null, 10, StatusSaaS.Ativo, "tenant-1", "system");
            contextGestao.Clientes.Add(cliente);

            contextGestao.Empresas.Add(empresa1);
            contextGestao.Empresas.Add(empresa2);
            await contextGestao.SaveChangesAsync();

            var empresaId1 = empresa1.Id;
            var empresaId2 = empresa2.Id;

            contextApp.UsuariosEmpresas.Add(new UsuarioEmpresa("tenant-1", usuarioId, empresaId1, null, true, "system"));
            contextApp.UsuariosEmpresas.Add(new UsuarioEmpresa("tenant-1", usuarioId, empresaId2, null, false, "system"));
            await contextApp.SaveChangesAsync();

            var query = new ObterContextoSessaoQuery("tenant-1", usuarioId);

            // Act
            var result = await mediator.Send(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("tenant-1", result.TenantId);
            Assert.Equal(usuarioId, result.UsuarioId);
            Assert.Equal(5, result.QtdeCadastroEmpresa);
            Assert.Equal(10, result.QtdeCadastroUsuario);
            Assert.False(result.Block);

            var lista = result.Empresas.ToList();
            Assert.Equal(2, lista.Count);
            Assert.Contains(lista, e => e.EmpresaId == empresaId1 && e.RazaoSocial == "Empresa 1 SA" && e.EhAdmin);
            Assert.Contains(lista, e => e.EmpresaId == empresaId2 && e.RazaoSocial == "Empresa 2 SA" && !e.EhAdmin);
        }

        #region Provedores de Teste
        public class TestTenantProvider : ITenantProvider
        {
            public string TenantId { get; set; }
            public TestTenantProvider(string tenantId) => TenantId = tenantId;
            public string GetTenantId() => TenantId;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
        #endregion
    }
}
