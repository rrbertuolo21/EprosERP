using System;
using System.Linq;
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
    /// <summary>
    /// D-02 — transições de estado antes órfãas agora expostas por command/handler/endpoint.
    /// Cobre CON-CRM, CON-VEN, CON-FIN, CON-GAR, CON-PES e a emissão de eventos con.* no Outbox.
    /// </summary>
    public class ConcessionariasTransicoesTests
    {
        private const string Tenant = "tenant-1";

        private static ContextDMS Novo(string db) =>
            new ContextDMS(
                new DbContextOptionsBuilder<ContextDMS>().UseInMemoryDatabase(db).Options,
                new FakeTenant(Tenant), new FakeUser("user-1"));

        // ---------- CON-CRM ----------

        [Fact(DisplayName = "Transição | CON-CRM | Converter oportunidade emite evento e é idempotente")]
        public async Task Converter_Oportunidade_Emite_Evento()
        {
            using var ctx = Novo("db_tr_crm_conv");
            var op = new OportunidadeConcessionaria(Guid.NewGuid(), 50000m, 0.5m, Tenant, "user-1");
            ctx.OportunidadesConcessionaria.Add(op);
            await ctx.SaveChangesAsync();

            var handler = new ConverterOportunidadeCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            var ok = await handler.Handle(new ConverterOportunidadeCommand(op.Id, Guid.NewGuid()), CancellationToken.None);
            Assert.True(ok.Sucesso);

            var recarregada = await ctx.OportunidadesConcessionaria.SingleAsync();
            Assert.Equal("Convertida", recarregada.Etapa);

            var evt = await ctx.OutboxMessages.SingleAsync();
            Assert.Equal(CatalogoEventosIntegracao.Concessionarias.CrmOportunidadeConvertida, evt.EventType);

            // Segunda conversão é bloqueada pela entidade
            var duplicada = await handler.Handle(new ConverterOportunidadeCommand(op.Id, Guid.NewGuid()), CancellationToken.None);
            Assert.False(duplicada.Sucesso);
        }

        [Fact(DisplayName = "Transição | CON-CRM | Handler retorna falha quando oportunidade não existe")]
        public async Task Avancar_Etapa_Inexistente_Falha()
        {
            using var ctx = Novo("db_tr_crm_404");
            var handler = new AvancarEtapaOportunidadeCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            var r = await handler.Handle(new AvancarEtapaOportunidadeCommand(Guid.NewGuid(), "Negociação"), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        // ---------- CON-VEN ----------

        [Fact(DisplayName = "Transição | CON-VEN | Aceitar proposta emitida emite evento")]
        public async Task Aceitar_Proposta_Emite_Evento()
        {
            using var ctx = Novo("db_tr_ven_aceite");
            var p = new PropostaVenda(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow.AddDays(5), 100000m, 5000m, Tenant, "user-1");
            ctx.PropostasVenda.Add(p);
            await ctx.SaveChangesAsync();

            var handler = new AceitarPropostaVendaCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            var r = await handler.Handle(new AceitarPropostaVendaCommand(p.Id), CancellationToken.None);

            Assert.True(r.Sucesso);
            Assert.Equal("Aceita", (await ctx.PropostasVenda.SingleAsync()).Status);
            var evt = await ctx.OutboxMessages.SingleAsync();
            Assert.Equal(CatalogoEventosIntegracao.Concessionarias.VenPropostaAceita, evt.EventType);
        }

        [Fact(DisplayName = "Transição | CON-VEN | Reservar unidade livre; segunda reserva falha")]
        public async Task Reservar_Estoque_Controla_Estado()
        {
            using var ctx = Novo("db_tr_ven_reserva");
            var v = new EstoqueVeiculo(Guid.NewGuid(), "9BWZZZ372HP123456", Guid.NewGuid(), null, null, null, Tenant, "user-1");
            ctx.EstoqueVeiculos.Add(v);
            await ctx.SaveChangesAsync();

            var handler = new ReservarEstoqueVeiculoCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            Assert.True((await handler.Handle(new ReservarEstoqueVeiculoCommand(v.Id), CancellationToken.None)).Sucesso);
            // A entidade recarregada já está "Reservado" → segunda reserva é rejeitada
            Assert.False((await handler.Handle(new ReservarEstoqueVeiculoCommand(v.Id), CancellationToken.None)).Sucesso);
        }

        // ---------- CON-FIN ----------

        [Fact(DisplayName = "Transição | CON-FIN | Encerrar jornada e liquidar contrato")]
        public async Task Encerrar_Jornada_E_Liquidar_Contrato()
        {
            using var ctx = Novo("db_tr_fin");
            var j = new JornadaFin(Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), Tenant, "user-1");
            var c = new ContratoFin(null, Guid.NewGuid(), "CT-1", null, Tenant, "user-1");
            ctx.JornadasFin.Add(j);
            ctx.ContratosFin.Add(c);
            await ctx.SaveChangesAsync();

            var jh = new EncerrarJornadaFinCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            Assert.True((await jh.Handle(new EncerrarJornadaFinCommand(j.Id), CancellationToken.None)).Sucesso);
            Assert.Equal("Encerrada", (await ctx.JornadasFin.SingleAsync()).Status);

            var lh = new LiquidarContratoFinCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            Assert.True((await lh.Handle(new LiquidarContratoFinCommand(c.Id), CancellationToken.None)).Sucesso);
            Assert.Equal("Liquidado", (await ctx.ContratosFin.SingleAsync()).Status);
        }

        // ---------- CON-GAR ----------

        [Fact(DisplayName = "Transição | CON-GAR | Julgar solicitação (aprovar) emite evento; rejulgar falha")]
        public async Task Julgar_Solicitacao_Garantia()
        {
            using var ctx = Novo("db_tr_gar");
            var s = new SolicitacaoGarantia(Guid.NewGuid(), "PROTO-9", DateTime.UtcNow, 10000m, "Ruído", "Relato", null, Tenant, "user-1");
            ctx.SolicitacoesGarantia.Add(s);
            await ctx.SaveChangesAsync();

            var handler = new JulgarSolicitacaoGarantiaCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            Assert.True((await handler.Handle(new JulgarSolicitacaoGarantiaCommand(s.Id, true), CancellationToken.None)).Sucesso);
            Assert.Equal("Aprovada", (await ctx.SolicitacoesGarantia.SingleAsync()).Status);
            Assert.Equal(CatalogoEventosIntegracao.Concessionarias.GarSolicitacaoJulgada, (await ctx.OutboxMessages.SingleAsync()).EventType);

            // já julgada → rejeitar falha
            Assert.False((await handler.Handle(new JulgarSolicitacaoGarantiaCommand(s.Id, false), CancellationToken.None)).Sucesso);
        }

        // ---------- Eventos em handlers de criação (D-08) ----------

        [Fact(DisplayName = "Evento | CON-FIN | Criar contrato emite con.fin.contrato_emitido")]
        public async Task Criar_Contrato_Emite_Evento()
        {
            using var ctx = Novo("db_evt_contrato");
            var handler = new CriarContratoFinCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            var r = await handler.Handle(new CriarContratoFinCommand(null, Guid.NewGuid(), "CT-EVT-1", null), CancellationToken.None);

            Assert.True(r.Sucesso);
            var evt = await ctx.OutboxMessages.SingleAsync();
            Assert.Equal(CatalogoEventosIntegracao.Concessionarias.FinContratoEmitido, evt.EventType);
            Assert.True(CatalogoEventosIntegracao.EhEventoConhecido(evt.EventType));
        }

        [Fact(DisplayName = "Evento | CON-MNT | Abrir OS da oficina emite con.mnt.ordem_servico_aberta")]
        public async Task Abrir_Os_Emite_Evento()
        {
            using var ctx = Novo("db_evt_os");
            var handler = new AbrirOrdemServicoDmsCommandHandler(ctx, new FakeTenant(Tenant), new FakeUser("user-1"));
            var r = await handler.Handle(
                new AbrirOrdemServicoDmsCommand("OS-EVT-1", "9BWZZZ372HP123456", "Revisão", 100m, 200m, false),
                CancellationToken.None);

            Assert.True(r.Sucesso);
            var evt = await ctx.OutboxMessages.SingleAsync();
            Assert.Equal(CatalogoEventosIntegracao.Concessionarias.MntOrdemServicoAberta, evt.EventType);
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
