using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.API.Controllers;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Jobs;
using Epros.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Xunit;

namespace Epros.Tests
{
    public class NewsletterPrivacyTests
    {
        private ContextAplicativo CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("operador-test");

            return new ContextAplicativo(options, tenantProvider, currentUser);
        }

        [Fact]
        public void NewsletterSubscriber_Criacao_DeveFalhar_Se_ConsentimentoLGPDFalhar()
        {
            // Act
            var subscriber = new NewsletterSubscriber(
                email: "cliente@teste.com",
                consentimentoLGPD: false, // Sem consentimento explícito
                termosVersao: "v1.0",
                ipRegistro: "192.168.0.1",
                tenantId: "system",
                criadoPor: "system"
            );

            // Assert
            Assert.False(subscriber.IsValid);
            Assert.Contains(subscriber.Notifications, n => n.Key == "ConsentimentoLGPD");
        }

        [Fact]
        public void NewsletterSubscriber_Criacao_DeveSucesso_E_InicializarCamposLGPD()
        {
            // Act
            var subscriber = new NewsletterSubscriber(
                email: "cliente@teste.com",
                consentimentoLGPD: true,
                termosVersao: "v1.0",
                ipRegistro: "192.168.0.1",
                tenantId: "system",
                criadoPor: "system"
            );

            // Assert
            Assert.True(subscriber.IsValid);
            Assert.Equal("cliente@teste.com", subscriber.Email);
            Assert.True(subscriber.ConsentimentoLGPD);
            Assert.Equal("v1.0", subscriber.TermosVersao);
            Assert.Equal("192.168.0.1", subscriber.IpRegistro);
            Assert.NotEqual(Guid.Empty, subscriber.TokenDescadastro);
            Assert.True(subscriber.Ativo);
            Assert.Null(subscriber.DesativadoEm);
            Assert.True((DateTime.UtcNow - subscriber.DataConsentimento).TotalMinutes < 1);
        }

        [Fact]
        public void NewsletterSubscriber_CancelarInscricao_DevePreencherDesativadoEm()
        {
            // Arrange
            var subscriber = new NewsletterSubscriber("cliente@teste.com", "system", "system");

            // Act
            subscriber.CancelarInscricao("system");

            // Assert
            Assert.False(subscriber.Ativo);
            Assert.NotNull(subscriber.DesativadoEm);
            Assert.True((DateTime.UtcNow - subscriber.DesativadoEm.Value).TotalMinutes < 1);

            // Act: Reativar
            subscriber.ReativarInscricao("system");

            // Assert
            Assert.True(subscriber.Ativo);
            Assert.Null(subscriber.DesativadoEm);
        }

        [Fact]
        public async Task InscreverNewsletterCommandHandler_DeveValidarConsentimento()
        {
            // Arrange
            var context = CreateInMemoryContext("db_news_consent_handler");
            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("operador-test");
            var handler = new InscreverNewsletterCommandHandler(context, tenantProvider, currentUser);

            // Act (Sem consentimento - validado pelo FluentValidation / CommandValidator)
            var commandInvalido = new InscreverNewsletterCommand("cliente@teste.com", ConsentimentoLGPD: false);
            var validator = new InscreverNewsletterCommandValidator();
            var valResult = await validator.ValidateAsync(commandInvalido);

            // Assert
            Assert.False(valResult.IsValid);
            Assert.Contains(valResult.Errors, e => e.PropertyName == "ConsentimentoLGPD");
        }

        [Fact]
        public async Task CancelarNewsletterPorTokenCommandHandler_DeveFuncionarComTokenValido()
        {
            // Arrange
            var context = CreateInMemoryContext("db_news_cancel_token");
            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("operador-test");

            // Criar um assinante ativo no banco
            var subscriber = new NewsletterSubscriber(
                email: "optout@teste.com",
                consentimentoLGPD: true,
                termosVersao: "v1.0",
                ipRegistro: "127.0.0.1",
                tenantId: "system",
                criadoPor: "system"
            );
            context.NewsletterSubscribers.Add(subscriber);
            await context.SaveChangesAsync();

            var handler = new CancelarNewsletterPorTokenCommandHandler(context, tenantProvider);

            // Act
            var command = new CancelarNewsletterPorTokenCommand(subscriber.TokenDescadastro);
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            var subSalvo = await context.NewsletterSubscribers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == subscriber.Id);
            Assert.False(subSalvo!.Ativo);
            Assert.NotNull(subSalvo.DesativadoEm);
        }

        [Fact]
        public async Task ExpurgarNewsletterInativaJob_DeveRemoverFisicamenteInativosHaMaisDe30Dias()
        {
            // Arrange
            var context = CreateInMemoryContext("db_news_purge_job");

            // 1. Assinante inativo há 31 dias (deve ser expurgado)
            var subExpurgar = new NewsletterSubscriber("expurgar@teste.com", "system", "system");
            subExpurgar.CancelarInscricao("system");
            // Usamos reflection para setar DesativadoEm no passado
            var propDesativado = typeof(NewsletterSubscriber).GetProperty(nameof(NewsletterSubscriber.DesativadoEm));
            Assert.NotNull(propDesativado);
            propDesativado.SetValue(subExpurgar, DateTime.UtcNow.AddDays(-31));
            Assert.True(subExpurgar.DesativadoEm < DateTime.UtcNow.AddDays(-30));
            context.NewsletterSubscribers.Add(subExpurgar);

            // 2. Assinante inativo há 15 dias (NÃO deve ser expurgado)
            var subManterInativo = new NewsletterSubscriber("manterinativo@teste.com", "system", "system");
            subManterInativo.CancelarInscricao("system");
            typeof(NewsletterSubscriber).GetProperty(nameof(NewsletterSubscriber.DesativadoEm))
                ?.SetValue(subManterInativo, DateTime.UtcNow.AddDays(-15));
            context.NewsletterSubscribers.Add(subManterInativo);

            // 3. Assinante ativo (NÃO deve ser expurgado)
            var subAtivo = new NewsletterSubscriber("ativo@teste.com", "system", "system");
            context.NewsletterSubscribers.Add(subAtivo);

            await context.SaveChangesAsync();

            var listBefore = await context.NewsletterSubscribers.IgnoreQueryFilters().ToListAsync();
            foreach (var s in listBefore)
            {
                Console.WriteLine($"[TEST-DEBUG] Email: {s.Email}, Ativo: {s.Ativo}, DesativadoEm: {s.DesativadoEm}");
            }

            var job = new ExpurgarNewsletterInativaJob(context);

            // Act
            await job.Execute(null!);

            // Assert
            var subscribers = await context.NewsletterSubscribers.IgnoreQueryFilters().ToListAsync();

            Assert.Equal(2, subscribers.Count);
            Assert.Contains(subscribers, s => s.Email == "manterinativo@teste.com");
            Assert.Contains(subscribers, s => s.Email == "ativo@teste.com");
            Assert.DoesNotContain(subscribers, s => s.Email == "expurgar@teste.com");
        }

        #region Helper Classes
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
            public string? GetUserEmail() => "test@test.com";
        }
        #endregion
    }
}
