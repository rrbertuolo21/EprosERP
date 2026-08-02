using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.DMS.Application.Handlers
{
    /// <summary>
    /// D-02 (2ª leva) — mais transições de estado antes órfãs, agora expostas por CQRS/endpoint:
    /// CON-MNT (orçamento aprovar/rejeitar, OS status/encerrar), CON-PES (demanda atender/cancelar),
    /// CON-CRM (test drive realizar/cancelar), CON-VEN (venda faturar/entregar), CON-DEV (contrato de
    /// rede encerrar/cancelar), CON-GAR (garantia do veículo encerrar). Filtro por (TenantId, Id).
    /// </summary>

    // ===== CON-MNT =====
    public class AprovarOrcamentoManutencaoCommandHandler : ICommandHandler<AprovarOrcamentoManutencaoCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public AprovarOrcamentoManutencaoCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }
        public async Task<CommandResult> Handle(AprovarOrcamentoManutencaoCommand r, CancellationToken ct)
        {
            var (tid, usr) = ConTransicaoBase.Contexto(_t, _u);
            var o = await _ctx.OrcamentosManutencao.FirstOrDefaultAsync(x => x.TenantId == tid && x.Id == r.OrcamentoId, ct);
            if (o == null) return CommandResult.Falha("Orçamento não encontrado.");
            o.Aprovar(usr);
            if (!o.IsValid) return CommandResult.Falha(o.Notifications.Select(n => n.Message));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Orçamento aprovado.", new { o.Id, o.Status });
        }
    }

    public class RejeitarOrcamentoManutencaoCommandHandler : ICommandHandler<RejeitarOrcamentoManutencaoCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public RejeitarOrcamentoManutencaoCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }
        public async Task<CommandResult> Handle(RejeitarOrcamentoManutencaoCommand r, CancellationToken ct)
        {
            var (tid, usr) = ConTransicaoBase.Contexto(_t, _u);
            var o = await _ctx.OrcamentosManutencao.FirstOrDefaultAsync(x => x.TenantId == tid && x.Id == r.OrcamentoId, ct);
            if (o == null) return CommandResult.Falha("Orçamento não encontrado.");
            o.Rejeitar(usr);
            if (!o.IsValid) return CommandResult.Falha(o.Notifications.Select(n => n.Message));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Orçamento rejeitado.", new { o.Id, o.Status });
        }
    }

    public class AtualizarStatusOsManutencaoCommandHandler : ICommandHandler<AtualizarStatusOsManutencaoCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public AtualizarStatusOsManutencaoCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }
        public async Task<CommandResult> Handle(AtualizarStatusOsManutencaoCommand r, CancellationToken ct)
        {
            var (tid, usr) = ConTransicaoBase.Contexto(_t, _u);
            var o = await _ctx.OrdensServicoManutencao.FirstOrDefaultAsync(x => x.TenantId == tid && x.Id == r.OrdemServicoId, ct);
            if (o == null) return CommandResult.Falha("Ordem de serviço não encontrada.");
            o.AtualizarStatus(r.Status, usr);
            if (!o.IsValid) return CommandResult.Falha(o.Notifications.Select(n => n.Message));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Status atualizado.", new { o.Id, o.StatusOficina });
        }
    }

    public class EncerrarOsManutencaoCommandHandler : ICommandHandler<EncerrarOsManutencaoCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public EncerrarOsManutencaoCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }
        public async Task<CommandResult> Handle(EncerrarOsManutencaoCommand r, CancellationToken ct)
        {
            var (tid, usr) = ConTransicaoBase.Contexto(_t, _u);
            var o = await _ctx.OrdensServicoManutencao.FirstOrDefaultAsync(x => x.TenantId == tid && x.Id == r.OrdemServicoId, ct);
            if (o == null) return CommandResult.Falha("Ordem de serviço não encontrada.");
            o.Encerrar(usr);
            if (!o.IsValid) return CommandResult.Falha(o.Notifications.Select(n => n.Message));
            _ctx.OutboxMessages.Add(new OutboxMessage(tid,
                CatalogoEventosIntegracao.Concessionarias.MntOrdemServicoFechada,
                JsonSerializer.Serialize(new { osId = o.Id, veiculoId = o.VeiculoId, tenantId = tid })));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("OS da oficina encerrada.", new { o.Id, o.StatusOficina });
        }
    }

    // ===== CON-PES =====
    public class AtenderDemandaPecaCommandHandler : ICommandHandler<AtenderDemandaPecaCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public AtenderDemandaPecaCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }
        public async Task<CommandResult> Handle(AtenderDemandaPecaCommand r, CancellationToken ct)
        {
            var (tid, usr) = ConTransicaoBase.Contexto(_t, _u);
            var d = await _ctx.DemandasPeca.FirstOrDefaultAsync(x => x.TenantId == tid && x.Id == r.DemandaId, ct);
            if (d == null) return CommandResult.Falha("Demanda não encontrada.");
            d.Atender(usr);
            if (!d.IsValid) return CommandResult.Falha(d.Notifications.Select(n => n.Message));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Demanda atendida.", new { d.Id, d.Status });
        }
    }

    public class CancelarDemandaPecaCommandHandler : ICommandHandler<CancelarDemandaPecaCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public CancelarDemandaPecaCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }
        public async Task<CommandResult> Handle(CancelarDemandaPecaCommand r, CancellationToken ct)
        {
            var (tid, usr) = ConTransicaoBase.Contexto(_t, _u);
            var d = await _ctx.DemandasPeca.FirstOrDefaultAsync(x => x.TenantId == tid && x.Id == r.DemandaId, ct);
            if (d == null) return CommandResult.Falha("Demanda não encontrada.");
            d.Cancelar(usr);
            if (!d.IsValid) return CommandResult.Falha(d.Notifications.Select(n => n.Message));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Demanda cancelada.", new { d.Id, d.Status });
        }
    }

    // ===== CON-CRM =====
    public class RealizarTestDriveCommandHandler : ICommandHandler<RealizarTestDriveCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public RealizarTestDriveCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }
        public async Task<CommandResult> Handle(RealizarTestDriveCommand r, CancellationToken ct)
        {
            var (tid, usr) = ConTransicaoBase.Contexto(_t, _u);
            var td = await _ctx.TestDrives.FirstOrDefaultAsync(x => x.TenantId == tid && x.Id == r.TestDriveId, ct);
            if (td == null) return CommandResult.Falha("Test drive não encontrado.");
            td.Realizar(r.Resultado, usr);
            if (!td.IsValid) return CommandResult.Falha(td.Notifications.Select(n => n.Message));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Test drive realizado.", new { td.Id, td.Status });
        }
    }

    public class CancelarTestDriveCommandHandler : ICommandHandler<CancelarTestDriveCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public CancelarTestDriveCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }
        public async Task<CommandResult> Handle(CancelarTestDriveCommand r, CancellationToken ct)
        {
            var (tid, usr) = ConTransicaoBase.Contexto(_t, _u);
            var td = await _ctx.TestDrives.FirstOrDefaultAsync(x => x.TenantId == tid && x.Id == r.TestDriveId, ct);
            if (td == null) return CommandResult.Falha("Test drive não encontrado.");
            td.Cancelar(usr);
            if (!td.IsValid) return CommandResult.Falha(td.Notifications.Select(n => n.Message));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Test drive cancelado.", new { td.Id, td.Status });
        }
    }

    // ===== CON-VEN (venda mestre) =====
    public class FaturarVendaVeiculoCommandHandler : ICommandHandler<FaturarVendaVeiculoCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public FaturarVendaVeiculoCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }
        public async Task<CommandResult> Handle(FaturarVendaVeiculoCommand r, CancellationToken ct)
        {
            var (tid, usr) = ConTransicaoBase.Contexto(_t, _u);
            var v = await _ctx.VendasVeiculos.FirstOrDefaultAsync(x => x.TenantId == tid && x.Id == r.VendaId, ct);
            if (v == null) return CommandResult.Falha("Venda não encontrada.");
            v.Faturar(usr);
            if (!v.IsValid) return CommandResult.Falha(v.Notifications.Select(n => n.Message));
            // NF-03: emissão fiscal (NF-e/CFOP) do veículo é pendente/valida-contador; aqui é só a
            // transição de estado da venda mestre.
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Venda faturada.", new { v.Id, v.Status });
        }
    }

    public class EntregarVendaVeiculoCommandHandler : ICommandHandler<EntregarVendaVeiculoCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public EntregarVendaVeiculoCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }
        public async Task<CommandResult> Handle(EntregarVendaVeiculoCommand r, CancellationToken ct)
        {
            var (tid, usr) = ConTransicaoBase.Contexto(_t, _u);
            var v = await _ctx.VendasVeiculos.FirstOrDefaultAsync(x => x.TenantId == tid && x.Id == r.VendaId, ct);
            if (v == null) return CommandResult.Falha("Venda não encontrada.");
            v.Entregar(usr);
            if (!v.IsValid) return CommandResult.Falha(v.Notifications.Select(n => n.Message));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Veículo entregue ao cliente.", new { v.Id, v.Status });
        }
    }

    // ===== CON-DEV =====
    public class EncerrarContratoRedeCommandHandler : ICommandHandler<EncerrarContratoRedeCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public EncerrarContratoRedeCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }
        public async Task<CommandResult> Handle(EncerrarContratoRedeCommand r, CancellationToken ct)
        {
            var (tid, usr) = ConTransicaoBase.Contexto(_t, _u);
            var c = await _ctx.ContratosRede.FirstOrDefaultAsync(x => x.TenantId == tid && x.Id == r.ContratoId, ct);
            if (c == null) return CommandResult.Falha("Contrato de rede não encontrado.");
            c.Encerrar(usr);
            if (!c.IsValid) return CommandResult.Falha(c.Notifications.Select(n => n.Message));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Contrato de rede encerrado.", new { c.Id, c.Status });
        }
    }

    public class CancelarContratoRedeCommandHandler : ICommandHandler<CancelarContratoRedeCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public CancelarContratoRedeCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }
        public async Task<CommandResult> Handle(CancelarContratoRedeCommand r, CancellationToken ct)
        {
            var (tid, usr) = ConTransicaoBase.Contexto(_t, _u);
            var c = await _ctx.ContratosRede.FirstOrDefaultAsync(x => x.TenantId == tid && x.Id == r.ContratoId, ct);
            if (c == null) return CommandResult.Falha("Contrato de rede não encontrado.");
            c.Cancelar(usr);
            if (!c.IsValid) return CommandResult.Falha(c.Notifications.Select(n => n.Message));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Contrato de rede cancelado.", new { c.Id, c.Status });
        }
    }

    // ===== CON-GAR =====
    public class EncerrarVeiculoGarantiaCommandHandler : ICommandHandler<EncerrarVeiculoGarantiaCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public EncerrarVeiculoGarantiaCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }
        public async Task<CommandResult> Handle(EncerrarVeiculoGarantiaCommand r, CancellationToken ct)
        {
            var (tid, usr) = ConTransicaoBase.Contexto(_t, _u);
            var v = await _ctx.VeiculosGarantia.FirstOrDefaultAsync(x => x.TenantId == tid && x.Id == r.VeiculoGarantiaId, ct);
            if (v == null) return CommandResult.Falha("Garantia do veículo não encontrada.");
            v.Encerrar(usr);
            if (!v.IsValid) return CommandResult.Falha(v.Notifications.Select(n => n.Message));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Garantia do veículo encerrada.", new { v.Id, v.Status });
        }
    }
}
