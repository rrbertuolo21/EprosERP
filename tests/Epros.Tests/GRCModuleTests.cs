using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Application.Handlers;
using Epros.Modules.GRC.Application.EventHandlers;
using Epros.Modules.GRC.Application.Queries;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class GRCModuleTests
    {
        [Fact]
        public async Task Deve_Registrar_Risco_E_Calcular_Criticidade()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextGRC>()
                .UseInMemoryDatabase("db_grc_riscos")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextGRC(options, tenantProvider, currentUser);

            var handler = new RegistrarRiscoCommandHandler(context, tenantProvider, currentUser);
            // Probabilidade 4 x Impacto 5 = NivelRisco 20 (Status: Inaceitavel)
            var command = new RegistrarRiscoCommand("Vazamento de dados LGPD", "Possível vazamento por falta de DLP", "Seguranca", 4, 5);

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var riscos = await context.RiscosCorporativos.ToListAsync();
            Assert.Single(riscos);
            Assert.Equal("Vazamento de dados LGPD", riscos[0].Titulo);
            Assert.Equal(20, riscos[0].NivelRisco);
            Assert.Equal("Inaceitavel", riscos[0].Status);
        }

        [Fact]
        public async Task Deve_Criar_Controle_Interno()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextGRC>()
                .UseInMemoryDatabase("db_grc_controles")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextGRC(options, tenantProvider, currentUser);

            var handler = new CriarControleCommandHandler(context, tenantProvider, currentUser);
            var command = new CriarControleCommand("CTRL-001", "Revisão Bimestral de Acessos", "Auditoria de acessos Keycloak", "Mensal");

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var controles = await context.ControlesInternos.ToListAsync();
            Assert.Single(controles);
            Assert.Equal("CTRL-001", controles[0].Codigo);
            Assert.Equal("Ativo", controles[0].Status);
        }

        [Fact]
        public async Task Deve_Registrar_Denuncia_Anonima_E_Gerar_Codigo()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextGRC>()
                .UseInMemoryDatabase("db_grc_denuncias")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("anonymous");
            using var context = new ContextGRC(options, tenantProvider, currentUser);

            var handler = new RegistrarDenunciaCommandHandler(context, tenantProvider);
            var command = new RegistrarDenunciaCommand("Fraude no lançamento de notas fiscais de frete");

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var denuncias = await context.Denuncias.ToListAsync();
            Assert.Single(denuncias);
            Assert.Equal("Recebida", denuncias[0].Status);
            Assert.StartsWith("DEN-", denuncias[0].CodigoAcompanhamento);
            Assert.Equal("anonymous", denuncias[0].CriadoPor);
        }

        [Fact]
        public async Task Deve_Julgar_Denuncia_E_Gerar_Outbox_Se_Procedente()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextGRC>()
                .UseInMemoryDatabase("db_grc_julgamento")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextGRC(options, tenantProvider, currentUser);

            var denuncia = new Denuncia("Relato de infração operacional", "tenant-1", "anonymous");
            context.Denuncias.Add(denuncia);
            await context.SaveChangesAsync();

            var handler = new JulgarDenunciaCommandHandler(context, currentUser, tenantProvider);
            var command = new JulgarDenunciaCommand(denuncia.Id, "Procedente", "Confirmado desvio de conduta pelo comitê");

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var denunciaAtualizada = await context.Denuncias.FindAsync(denuncia.Id);
            Assert.Equal("Procedente", denunciaAtualizada!.Status);
            Assert.Equal("Confirmado desvio de conduta pelo comitê", denunciaAtualizada.ParecerFinal);

            var outboxMsg = await context.OutboxMessages.FirstOrDefaultAsync();
            Assert.NotNull(outboxMsg);
            Assert.Equal("DenunciaProcedente", outboxMsg!.EventType);
            Assert.Contains("Confirmado desvio de conduta", outboxMsg.Payload);
        }

        [Fact]
        public async Task Deve_Abrir_Incidente_Compliance_Ao_Processar_DenunciaProcedente()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextGRC>()
                .UseInMemoryDatabase("db_grc_integracao_incidente")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextGRC(options, tenantProvider, currentUser);

            var handler = new DenunciaProcedenteComplianceHandler(context);

            var notification = new DenunciaProcedenteEventNotification(
                DenunciaId: Guid.NewGuid(),
                Relato: "Relato de infração grave",
                ParecerFinal: "Fraude confirmada",
                TenantId: "tenant-1"
            );

            // Agir
            await handler.Handle(notification, CancellationToken.None);

            // Assertiva
            var incidentes = await context.IncidentesCompliance.ToListAsync();
            Assert.Single(incidentes);
            Assert.Equal("Incidente GRC — Investigação de Denúncia Procedente", incidentes[0].Titulo);
            Assert.Equal("Denuncia", incidentes[0].Origem);
            Assert.Equal("Critica", incidentes[0].Gravidade);
            Assert.Contains(notification.DenunciaId.ToString(), incidentes[0].Descricao);
            Assert.Contains("Fraude confirmada", incidentes[0].Descricao);
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
            public string? GetUserName() => "test_user";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
