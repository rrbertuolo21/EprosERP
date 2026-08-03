using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Application.Handlers;
using Epros.Modules.DMS.Domain.Entities;
using Epros.Modules.DMS.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>D-02 (2ª leva) — MNT, PES, CRM, VEN(venda mestre), DEV, GAR.</summary>
    public class ConcessionariasTransicoes2Tests
    {
        private const string Tenant = "tenant-1";

        private static ContextDMS Novo(string db) =>
            new ContextDMS(
                new DbContextOptionsBuilder<ContextDMS>().UseInMemoryDatabase(db).Options,
                new FakeTenant(Tenant), new FakeUser("user-1"));

        [Fact(DisplayName = "Transição | CON-MNT | Aprovar orçamento em análise; rejeitar depois falha")]
        public async Task Aprovar_Orcamento()
        {
            using var ctx = Novo("db2_mnt_orc");
            var o = new OrcamentoManutencao(Guid.NewGuid(), DateTime.UtcNow.AddDays(3), 1500m, Tenant, "user-1");
            ctx.OrcamentosManutencao.Add(o);
            await ctx.SaveChangesAsync();

            var h = new AprovarOrcamentoManutencaoCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            Assert.True((await h.Handle(new AprovarOrcamentoManutencaoCommand(o.Id), CancellationToken.None)).Sucesso);
            Assert.Equal("Aprovado", (await ctx.OrcamentosManutencao.SingleAsync()).Status);
        }

        [Fact(DisplayName = "Transição | CON-MNT | Encerrar OS emite evento con.mnt.ordem_servico_fechada")]
        public async Task Encerrar_Os_Manutencao_Emite_Evento()
        {
            using var ctx = Novo("db2_mnt_os");
            var os = new OrdemServicoManutencao(Guid.NewGuid(), null, Guid.NewGuid(), "9BWZZZ372HP123456", "ABC1D23", 40000m,
                Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null, Tenant, "user-1");
            ctx.OrdensServicoManutencao.Add(os);
            await ctx.SaveChangesAsync();

            var h = new EncerrarOsManutencaoCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            Assert.True((await h.Handle(new EncerrarOsManutencaoCommand(os.Id), CancellationToken.None)).Sucesso);
            Assert.Equal("Encerrada", (await ctx.OrdensServicoManutencao.SingleAsync()).StatusOficina);
            Assert.Equal(CatalogoEventosIntegracao.Concessionarias.MntOrdemServicoFechada, (await ctx.OutboxMessages.SingleAsync()).EventType);
        }

        [Fact(DisplayName = "Transição | CON-PES | Atender demanda")]
        public async Task Atender_Demanda()
        {
            using var ctx = Novo("db2_pes_dem");
            var d = new DemandaPeca(Guid.NewGuid(), Guid.NewGuid(), "OrdemServico", Guid.NewGuid(), Guid.NewGuid(), 3m, DateTime.UtcNow.AddDays(2), Tenant, "user-1");
            ctx.DemandasPeca.Add(d);
            await ctx.SaveChangesAsync();

            var h = new AtenderDemandaPecaCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            Assert.True((await h.Handle(new AtenderDemandaPecaCommand(d.Id), CancellationToken.None)).Sucesso);
            Assert.Equal("Atendida", (await ctx.DemandasPeca.SingleAsync()).Status);
        }

        [Fact(DisplayName = "Transição | CON-CRM | Realizar test drive grava resultado")]
        public async Task Realizar_TestDrive()
        {
            using var ctx = Novo("db2_crm_td");
            var td = new TestDrive(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddHours(1), Tenant, "user-1");
            ctx.TestDrives.Add(td);
            await ctx.SaveChangesAsync();

            var h = new RealizarTestDriveCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            Assert.True((await h.Handle(new RealizarTestDriveCommand(td.Id, "Cliente gostou"), CancellationToken.None)).Sucesso);
            Assert.Equal("Realizado", (await ctx.TestDrives.SingleAsync()).Status);
        }

        [Fact(DisplayName = "Transição | CON-VEN | Faturar depois Entregar (ciclo feliz)")]
        public async Task Faturar_E_Entregar_Venda()
        {
            using var ctx = Novo("db2_ven_ciclo");
            var v = new VendaVeiculo("9BWZZZ372HP123456", "Gol", "VW", 2024, 80000m, "Cliente X", Tenant, "user-1");
            ctx.VendasVeiculos.Add(v);
            await ctx.SaveChangesAsync();

            var fh = new FaturarVendaVeiculoCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            Assert.True((await fh.Handle(new FaturarVendaVeiculoCommand(v.Id), CancellationToken.None)).Sucesso);
            Assert.Equal("Faturado", (await ctx.VendasVeiculos.SingleAsync()).Status);

            var eh = new EntregarVendaVeiculoCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            Assert.True((await eh.Handle(new EntregarVendaVeiculoCommand(v.Id), CancellationToken.None)).Sucesso);
            Assert.Equal("Entregue", (await ctx.VendasVeiculos.SingleAsync()).Status);
        }

        [Fact(DisplayName = "Transição | CON-VEN | Entregar antes de faturar falha (contexto próprio)")]
        public async Task Entregar_Antes_De_Faturar_Falha()
        {
            using var ctx = Novo("db2_ven_ordem");
            var v = new VendaVeiculo("9BWZZZ372HP654321", "Onix", "GM", 2024, 90000m, "Cliente Y", Tenant, "user-1");
            ctx.VendasVeiculos.Add(v);
            await ctx.SaveChangesAsync();

            var eh = new EntregarVendaVeiculoCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            Assert.False((await eh.Handle(new EntregarVendaVeiculoCommand(v.Id), CancellationToken.None)).Sucesso);
            Assert.Equal("Reservado", (await ctx.VendasVeiculos.SingleAsync()).Status);
        }

        [Fact(DisplayName = "Transição | CON-DEV | Encerrar contrato de rede")]
        public async Task Encerrar_Contrato_Rede()
        {
            using var ctx = Novo("db2_dev_ctr");
            var c = new ContratoRede(Guid.NewGuid(), "Concessão", DateTime.UtcNow, DateTime.UtcNow.AddYears(2), Tenant, "user-1");
            ctx.ContratosRede.Add(c);
            await ctx.SaveChangesAsync();

            var h = new EncerrarContratoRedeCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            Assert.True((await h.Handle(new EncerrarContratoRedeCommand(c.Id), CancellationToken.None)).Sucesso);
            Assert.Equal("Encerrado", (await ctx.ContratosRede.SingleAsync()).Status);
        }

        [Fact(DisplayName = "Transição | CON-GAR | Encerrar garantia do veículo")]
        public async Task Encerrar_Veiculo_Garantia()
        {
            using var ctx = Novo("db2_gar_veic");
            var vg = new VeiculoGarantia(Guid.NewGuid(), Guid.NewGuid(), "9BWZZZ372HP123456", Guid.NewGuid(),
                DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddYears(3), 0m, 100000m, Tenant, "user-1");
            ctx.VeiculosGarantia.Add(vg);
            await ctx.SaveChangesAsync();

            var h = new EncerrarVeiculoGarantiaCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            Assert.True((await h.Handle(new EncerrarVeiculoGarantiaCommand(vg.Id), CancellationToken.None)).Sucesso);
            Assert.Equal("Encerrada", (await ctx.VeiculosGarantia.SingleAsync()).Status);
        }

        private sealed class FakeTenant : ITenantProvider
        {
            private readonly string _t;
            public FakeTenant(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private sealed class FakeUser : ICurrentUser
        {
            private readonly string _u;
            public FakeUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "test";
            public string? GetUserEmail() => "t@epros.com";
        }
    }
}
