using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Application.Handlers;
using Epros.Modules.DMS.Application.Queries;
using Epros.Modules.DMS.Domain.Entities;
using Epros.Modules.DMS.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class DMSModuleTests
    {
        [Fact]
        public async Task Deve_Registrar_Venda_De_Veiculo()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextDMS>()
                .UseInMemoryDatabase("db_dms_venda_veiculos")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextDMS(options, tenantProvider, currentUser);

            var handler = new RegistrarVendaVeiculoCommandHandler(context, tenantProvider, currentUser);
            var command = new RegistrarVendaVeiculoCommand("9BWZZZ372HP123456", "Golf Comfortline 1.4 TSI", "Volkswagen", 2017, 85000m, "Carlos Roberto");

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var vendas = await context.VendasVeiculos.ToListAsync();
            Assert.Single(vendas);
            Assert.Equal("Golf Comfortline 1.4 TSI", vendas[0].Modelo);
            Assert.Equal("9BWZZZ372HP123456", vendas[0].Chassi);
            Assert.Equal("Reservado", vendas[0].Status);
        }

        [Fact]
        public async Task Deve_Abrir_Ordem_Servico_Oficina()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextDMS>()
                .UseInMemoryDatabase("db_dms_abertura_os")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextDMS(options, tenantProvider, currentUser);

            var handler = new AbrirOrdemServicoDmsCommandHandler(context, tenantProvider, currentUser);
            var command = new AbrirOrdemServicoDmsCommand("OS-88712", "9BWZZZ372HP123456", "Barulho suspensão dianteira", 450m, 200m, false);

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var ordens = await context.OrdensServicoDms.ToListAsync();
            Assert.Single(ordens);
            Assert.Equal("OS-88712", ordens[0].NumeroOs);
            Assert.Equal(650m, ordens[0].ValorTotal);
            Assert.Equal("Aberta", ordens[0].Status);
        }

        [Fact]
        public async Task Deve_Fechar_Ordem_Servico_E_Gerar_Garantia_Se_Reclamado()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextDMS>()
                .UseInMemoryDatabase("db_dms_fechamento_os_garantia")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextDMS(options, tenantProvider, currentUser);

            var os = new OrdemServicoDms("OS-771", "9BWZZZ372HP123456", "Vazamento amortecedor", 1200m, 350m, true, "tenant-1", "user-1");
            context.OrdensServicoDms.Add(os);
            await context.SaveChangesAsync();

            var handler = new FecharOrdemServicoDmsCommandHandler(context, tenantProvider, currentUser);

            // Agir
            var result = await handler.Handle(new FecharOrdemServicoDmsCommand(os.Id), CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var osAtualizada = await context.OrdensServicoDms.FindAsync(os.Id);
            Assert.Equal("Fechada", osAtualizada!.Status);

            var garantias = await context.GarantiasMontadora.ToListAsync();
            Assert.Single(garantias);
            Assert.Equal(os.Id, garantias[0].OrdemServicoDmsId);
            Assert.Equal("Solicitada", garantias[0].Status);
            Assert.Equal(1200m, garantias[0].ValorReclamado);
        }

        [Fact]
        public async Task Deve_Julgar_Garantia_E_Atualizar_Status_Na_Ordem_Servico()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextDMS>()
                .UseInMemoryDatabase("db_dms_julgamento_garantia")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextDMS(options, tenantProvider, currentUser);

            var os = new OrdemServicoDms("OS-772", "9BWZZZ372HP123456", "Defeito no alternador", 800m, 150m, true, "tenant-1", "user-1");
            context.OrdensServicoDms.Add(os);

            var garantia = new GarantiaMontadora(os.Id, "9BWZZZ372HP123456", "Alternador Bosch", 800m, "tenant-1", "user-1");
            context.GarantiasMontadora.Add(garantia);

            await context.SaveChangesAsync();

            var handler = new JulgarGarantiaMontadoraCommandHandler(context, currentUser);
            var command = new JulgarGarantiaMontadoraCommand(garantia.Id, "Aprovada", "Troca autorizada por recall técnico");

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var garantiaAtualizada = await context.GarantiasMontadora.FindAsync(garantia.Id);
            Assert.Equal("Aprovada", garantiaAtualizada!.Status);
            Assert.Equal("Troca autorizada por recall técnico", garantiaAtualizada.ParecerMontadora);

            var osAtualizada = await context.OrdensServicoDms.FindAsync(os.Id);
            Assert.Equal("Aprovada", osAtualizada!.StatusGarantia);
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
