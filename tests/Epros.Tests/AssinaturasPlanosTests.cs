using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.API.Middlewares;

namespace Epros.Tests
{
    public class AssinaturasPlanosTests
    {
        #region Testes de Contratação e Trial Único

        [Fact]
        public async Task Deve_Contratar_Plano_Trial_Com_Sucesso_E_Bloquear_Segundo_Contrato_Trial()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-trial-test";
            var userId = "user-trial-test";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            // Cria cliente
            var cliente = new Cliente("Cliente Teste Trial", "00.000.000/0001-00", "cliente@trial.com", Guid.NewGuid(), tenantId, userId);
            context.Clientes.Add(cliente);

            // Cria plano Trial (Preço 0)
            var planoTrial = new Plano(
                nome: "Plano Trial",
                preco: 0.00m,
                grupoPlanoId: null,
                limiteUsuarios: 5,
                limiteEmpresas: 1,
                recursosInclusos: "Fiscal,Estoque",
                tenantId: tenantId,
                criadoPor: userId
            );
            context.Planos.Add(planoTrial);
            await context.SaveChangesAsync();

            var handler = new ContratarPlanoCommandHandler(context, tenantProvider, currentUser);

            // Act 1: Contrata o plano trial pela primeira vez
            var command1 = new ContratarPlanoCommand(planoTrial.Id, "Manual");
            var result1 = await handler.Handle(command1, CancellationToken.None);

            // Assert 1
            Assert.True(result1.Sucesso);
            var assinaturas = await context.AssinaturasClientes.Where(a => a.ClienteId == cliente.Id).ToListAsync();
            Assert.Single(assinaturas);
            Assert.Equal(AssinaturaStatus.Ativa, assinaturas[0].Status);
            Assert.NotNull(assinaturas[0].TrialAte);
            Assert.Equal(planoTrial.Id, cliente.PlanoId); // O cliente teve o plano atualizado para o plano trial contratado

            // Act 2: Tenta contratar o mesmo plano trial novamente
            var result2 = await handler.Handle(command1, CancellationToken.None);

            // Assert 2
            Assert.False(result2.Sucesso);
            Assert.Contains(result2.Erros, e => e.Contains("limitado a uma utilização por cliente"));
            
            // Verifica que não foi criada uma nova assinatura
            var assinaturasAposTentativa = await context.AssinaturasClientes.Where(a => a.ClienteId == cliente.Id).ToListAsync();
            Assert.Single(assinaturasAposTentativa);
        }

        #endregion

        #region Testes de Encadeamento de Vigência

        [Fact]
        public async Task Deve_Contratar_Plano_Pago_E_Encadear_Corretamente_Se_Houver_Assinatura_Ativa()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-encadeamento";
            var userId = "user-encadeamento";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            // Cria dois planos pagos
            var planoAtivo = new Plano("Plano Básico", 199.90m, null, 10, 2, "Estoque", tenantId, userId);
            var planoFuturo = new Plano("Plano Premium", 499.90m, null, 50, 5, "Estoque,Fiscal,Financeiro", tenantId, userId);
            context.Planos.AddRange(planoAtivo, planoFuturo);

            // Cria cliente
            var cliente = new Cliente("Cliente Teste Encadeamento", "00.000.000/0001-00", "cliente@encadeamento.com", planoAtivo.Id, tenantId, userId);
            context.Clientes.Add(cliente);

            // Cria uma assinatura ativa existente (com vigência de 10 dias atrás até 20 dias no futuro)
            var dataInicioAtiva = DateTime.UtcNow.AddDays(-10);
            var dataFimAtiva = DateTime.UtcNow.AddDays(20);
            var assinaturaAtiva = new AssinaturaCliente(
                clienteId: cliente.Id,
                planoId: planoAtivo.Id,
                status: AssinaturaStatus.Ativa,
                dataInicio: dataInicioAtiva,
                dataFim: dataFimAtiva,
                trialAte: null,
                metodoPagamento: "PIX",
                transacaoId: "tx-ativa",
                detalhesPacoteJson: "{}",
                tenantId: tenantId,
                criadoPor: userId
            );
            context.AssinaturasClientes.Add(assinaturaAtiva);
            await context.SaveChangesAsync();

            var handler = new ContratarPlanoCommandHandler(context, tenantProvider, currentUser);

            // Act: Contrata o plano futuro
            var command = new ContratarPlanoCommand(planoFuturo.Id, "PIX");
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            // Deve obter a assinatura futura
            var assinaturaFutura = await context.AssinaturasClientes
                .Where(a => a.ClienteId == cliente.Id && a.Status == AssinaturaStatus.Futura)
                .FirstOrDefaultAsync();

            Assert.NotNull(assinaturaFutura);
            // A data de início da assinatura futura deve ser no dia seguinte ao término da assinatura ativa (D+1)
            Assert.Equal(dataFimAtiva.AddDays(1).Date, assinaturaFutura.DataInicio!.Value.Date);
            Assert.Equal(assinaturaFutura.DataInicio.Value.AddDays(30).Date, assinaturaFutura.DataFim!.Value.Date);

            // Como a assinatura criada é futura (status inicial Futura), o plano contratado não deve ser ativado imediatamente no cliente
            var clienteDB = await context.Clientes.FindAsync(cliente.Id);
            Assert.Equal(planoAtivo.Id, clienteDB!.PlanoId); // Continua com o plano ativo anterior
        }

        #endregion

        #region Testes de Faturamento e PIX

        [Fact]
        public async Task Deve_Gerar_Pix_Para_Fatura_Pendente_Com_Sucesso()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-pix";
            var userId = "user-pix";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            // Cria cliente e fatura pendente
            var cliente = new Cliente("Cliente Pix", "00.000.000/0001-00", "cliente@pix.com", Guid.NewGuid(), tenantId, userId);
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            var fatura = new Fatura(cliente.Id, 299.90m, DateTime.UtcNow.AddDays(5), tenantId, userId);
            context.Faturas.Add(fatura);
            await context.SaveChangesAsync();

            var handler = new GerarPixFaturaCommandHandler(context, tenantProvider, currentUser);

            // Act
            var command = new GerarPixFaturaCommand(fatura.Id);
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            Assert.NotNull(result.Dados);

            var pixResponse = (PixResponseDto)result.Dados;
            Assert.Equal(fatura.Id, pixResponse.FaturaId);
            Assert.Equal(299.90m, pixResponse.Valor);
            Assert.Contains("base64-string-simulated-qrcode-data-for-epros-billing", pixResponse.QrCodeBase64);
            Assert.Contains("https://checkout.epros.com/pay/", pixResponse.CheckoutUrl);

            // Verifica se o PagamentoFatura foi registrado no banco de dados como pendente
            var pagamento = await context.PagamentosFaturas.FirstOrDefaultAsync(p => p.FaturaId == fatura.Id);
            Assert.NotNull(pagamento);
            Assert.Equal("PIX", pagamento.TipoPagamento);
            Assert.Equal(PagamentoFaturaStatus.Pending, pagamento.Status);
            Assert.Equal(299.90m, pagamento.ValorPago);
        }

        #endregion

        #region Testes de Middleware de Bloqueio por Inadimplência

        [Fact]
        public async Task Middleware_Deve_Bloquear_Tenant_Inadimplente_Em_Rotas_Operacionais()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-devedor-operacional";
            var userId = "user-devedor";

            using var contextGestao = CreateInMemoryContext(dbName, tenantId, userId);

            // Cria cliente ativo
            var cliente = new Cliente("Empresa Inadimplente", "00.000.000/0001-00", "financeiro@devedor.com", Guid.NewGuid(), tenantId, userId);
            contextGestao.Clientes.Add(cliente);

            // Cria fatura vencida há mais de 15 dias (ex: 20 dias atrás)
            var faturaVencida = new Fatura(cliente.Id, 299.90m, DateTime.UtcNow.AddDays(-20), tenantId, userId);
            contextGestao.Faturas.Add(faturaVencida);
            await contextGestao.SaveChangesAsync();

            // Configura o HttpContext simulado
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/v1/estoque/produtos"; // Rota operacional comercial

            // Configura o ServiceProvider e escopos
            var services = new ServiceCollection();
            var tenantProvider = new TestTenantProvider(tenantId);
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton(contextGestao);
            httpContext.RequestServices = services.BuildServiceProvider();

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new BloqueioInadimplenciaMiddleware(next);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            Assert.False(nextCalled); // O request pipeline deve ser interrompido
            Assert.Equal(StatusCodes.Status402PaymentRequired, httpContext.Response.StatusCode); // Deve retornar 402
        }

        [Fact]
        public async Task Middleware_Deve_Permitir_Acesso_Em_Rotas_Liberadas_Mesmo_Inadimplente()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-devedor-liberado";
            var userId = "user-devedor";

            using var contextGestao = CreateInMemoryContext(dbName, tenantId, userId);

            // Cria cliente ativo
            var cliente = new Cliente("Empresa Inadimplente", "00.000.000/0001-00", "financeiro@devedor.com", Guid.NewGuid(), tenantId, userId);
            contextGestao.Clientes.Add(cliente);

            // Cria fatura vencida há mais de 15 dias (ex: 20 dias atrás)
            var faturaVencida = new Fatura(cliente.Id, 299.90m, DateTime.UtcNow.AddDays(-20), tenantId, userId);
            contextGestao.Faturas.Add(faturaVencida);
            await contextGestao.SaveChangesAsync();

            // Configura o HttpContext simulado
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/v1/aplicativo/assinaturas/faturas"; // Rota liberada para pagamento/faturas

            // Configura o ServiceProvider
            var services = new ServiceCollection();
            var tenantProvider = new TestTenantProvider(tenantId);
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton(contextGestao);
            httpContext.RequestServices = services.BuildServiceProvider();

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new BloqueioInadimplenciaMiddleware(next);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            Assert.True(nextCalled); // Deve chamar o próximo middleware no pipeline
            Assert.NotEqual(StatusCodes.Status402PaymentRequired, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task Middleware_Deve_Permitir_Acesso_Se_A_Fatura_Estiver_Atrasada_Ha_Menos_De_15_Dias()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-devedor-recente";
            var userId = "user-devedor";

            using var contextGestao = CreateInMemoryContext(dbName, tenantId, userId);

            // Cria cliente ativo
            var cliente = new Cliente("Empresa Debito Recente", "00.000.000/0001-00", "financeiro@recente.com", Guid.NewGuid(), tenantId, userId);
            contextGestao.Clientes.Add(cliente);

            // Cria fatura vencida há 5 dias (tolerância é até 15 dias)
            var faturaVencida = new Fatura(cliente.Id, 299.90m, DateTime.UtcNow.AddDays(-5), tenantId, userId);
            contextGestao.Faturas.Add(faturaVencida);
            await contextGestao.SaveChangesAsync();

            // Configura o HttpContext simulado
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/v1/estoque/produtos"; // Rota operacional

            // Configura o ServiceProvider
            var services = new ServiceCollection();
            var tenantProvider = new TestTenantProvider(tenantId);
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton(contextGestao);
            httpContext.RequestServices = services.BuildServiceProvider();

            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new BloqueioInadimplenciaMiddleware(next);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            Assert.True(nextCalled); // Deve permitir acesso (tolerância)
            Assert.NotEqual(StatusCodes.Status402PaymentRequired, httpContext.Response.StatusCode);
        }

        #endregion

        #region Helpers e Doubles de Teste

        private ContextGestaoClientes CreateInMemoryContext(string databaseName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            return new ContextGestaoClientes(options, tenantProvider, currentUser);
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
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
