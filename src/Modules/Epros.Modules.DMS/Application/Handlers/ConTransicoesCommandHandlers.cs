using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Domain.Entities;
using Epros.Modules.DMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.DMS.Application.Handlers
{
    /// <summary>
    /// Handlers das transições de estado dos submódulos de CONCESSIONÁRIAS (D-02: liga os métodos de
    /// domínio ricos — antes órfãos — a commands/endpoints). Cada transição valida o estado na
    /// entidade, persiste e, quando é fato relevante de integração, emite evento Outbox pós-commit
    /// (catálogo con.*). Filtro por (TenantId, Id) — isolamento multi-tenant.
    /// </summary>
    internal static class ConTransicaoBase
    {
        public static (string tenantId, string usuario) Contexto(ITenantProvider t, ICurrentUser u) =>
            (t.GetTenantId(), u.GetUserId() ?? "system");
    }

    // ==================== CON-CRM ====================

    public class AvancarEtapaOportunidadeCommandHandler : ICommandHandler<AvancarEtapaOportunidadeCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public AvancarEtapaOportunidadeCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }

        public async Task<CommandResult> Handle(AvancarEtapaOportunidadeCommand request, CancellationToken ct)
        {
            var (tenantId, usuario) = ConTransicaoBase.Contexto(_t, _u);
            var op = await _ctx.OportunidadesConcessionaria.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.OportunidadeId, ct);
            if (op == null) return CommandResult.Falha("Oportunidade não encontrada.");

            op.AvancarEtapa(request.NovaEtapa, usuario);
            if (!op.IsValid) return CommandResult.Falha(op.Notifications.Select(n => n.Message));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Etapa avançada com sucesso!", new { op.Id, op.Etapa });
        }
    }

    public class ConverterOportunidadeCommandHandler : ICommandHandler<ConverterOportunidadeCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public ConverterOportunidadeCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }

        public async Task<CommandResult> Handle(ConverterOportunidadeCommand request, CancellationToken ct)
        {
            var (tenantId, usuario) = ConTransicaoBase.Contexto(_t, _u);
            var op = await _ctx.OportunidadesConcessionaria.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.OportunidadeId, ct);
            if (op == null) return CommandResult.Falha("Oportunidade não encontrada.");

            op.Converter(request.VendaId, usuario);
            if (!op.IsValid) return CommandResult.Falha(op.Notifications.Select(n => n.Message));

            _ctx.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Concessionarias.CrmOportunidadeConvertida,
                JsonSerializer.Serialize(new { oportunidadeId = op.Id, vendaId = request.VendaId, tenantId })));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Oportunidade convertida com sucesso!", new { op.Id, op.Etapa, op.VendaId });
        }
    }

    public class RegistrarPerdaOportunidadeCommandHandler : ICommandHandler<RegistrarPerdaOportunidadeCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public RegistrarPerdaOportunidadeCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }

        public async Task<CommandResult> Handle(RegistrarPerdaOportunidadeCommand request, CancellationToken ct)
        {
            var (tenantId, usuario) = ConTransicaoBase.Contexto(_t, _u);
            var op = await _ctx.OportunidadesConcessionaria.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.OportunidadeId, ct);
            if (op == null) return CommandResult.Falha("Oportunidade não encontrada.");

            op.RegistrarPerda(request.MotivoPerdaId, usuario);
            if (!op.IsValid) return CommandResult.Falha(op.Notifications.Select(n => n.Message));

            _ctx.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Concessionarias.CrmOportunidadePerdida,
                JsonSerializer.Serialize(new { oportunidadeId = op.Id, motivoPerdaId = request.MotivoPerdaId, tenantId })));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Perda registrada com sucesso!", new { op.Id, op.Etapa });
        }
    }

    // ==================== CON-VEN ====================

    public class AceitarPropostaVendaCommandHandler : ICommandHandler<AceitarPropostaVendaCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public AceitarPropostaVendaCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }

        public async Task<CommandResult> Handle(AceitarPropostaVendaCommand request, CancellationToken ct)
        {
            var (tenantId, usuario) = ConTransicaoBase.Contexto(_t, _u);
            var p = await _ctx.PropostasVenda.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.PropostaId, ct);
            if (p == null) return CommandResult.Falha("Proposta não encontrada.");

            p.Aceitar(usuario);
            if (!p.IsValid) return CommandResult.Falha(p.Notifications.Select(n => n.Message));

            _ctx.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Concessionarias.VenPropostaAceita,
                JsonSerializer.Serialize(new { propostaId = p.Id, oportunidadeId = p.OportunidadeId, valorFinal = p.ValorFinal, tenantId })));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Proposta aceita com sucesso!", new { p.Id, p.Status, p.ValorFinal });
        }
    }

    public class ExpirarPropostaVendaCommandHandler : ICommandHandler<ExpirarPropostaVendaCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public ExpirarPropostaVendaCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }

        public async Task<CommandResult> Handle(ExpirarPropostaVendaCommand request, CancellationToken ct)
        {
            var (tenantId, usuario) = ConTransicaoBase.Contexto(_t, _u);
            var p = await _ctx.PropostasVenda.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.PropostaId, ct);
            if (p == null) return CommandResult.Falha("Proposta não encontrada.");

            p.Expirar(usuario);
            if (!p.IsValid) return CommandResult.Falha(p.Notifications.Select(n => n.Message));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Proposta expirada.", new { p.Id, p.Status });
        }
    }

    public class ReservarEstoqueVeiculoCommandHandler : ICommandHandler<ReservarEstoqueVeiculoCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public ReservarEstoqueVeiculoCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }

        public async Task<CommandResult> Handle(ReservarEstoqueVeiculoCommand request, CancellationToken ct)
        {
            var (tenantId, usuario) = ConTransicaoBase.Contexto(_t, _u);
            var v = await _ctx.EstoqueVeiculos.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.EstoqueVeiculoId, ct);
            if (v == null) return CommandResult.Falha("Unidade em estoque não encontrada.");

            v.Reservar(usuario);
            if (!v.IsValid) return CommandResult.Falha(v.Notifications.Select(n => n.Message));

            _ctx.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Concessionarias.VenVeiculoReservado,
                JsonSerializer.Serialize(new { estoqueVeiculoId = v.Id, chassiVin = v.ChassiVin, tenantId })));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Unidade reservada com sucesso!", new { v.Id, v.Status });
        }
    }

    public class LiberarEstoqueVeiculoCommandHandler : ICommandHandler<LiberarEstoqueVeiculoCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public LiberarEstoqueVeiculoCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }

        public async Task<CommandResult> Handle(LiberarEstoqueVeiculoCommand request, CancellationToken ct)
        {
            var (tenantId, usuario) = ConTransicaoBase.Contexto(_t, _u);
            var v = await _ctx.EstoqueVeiculos.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.EstoqueVeiculoId, ct);
            if (v == null) return CommandResult.Falha("Unidade em estoque não encontrada.");

            v.Liberar(usuario);
            if (!v.IsValid) return CommandResult.Falha(v.Notifications.Select(n => n.Message));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Unidade liberada.", new { v.Id, v.Status });
        }
    }

    public class CancelarReservaVeiculoCommandHandler : ICommandHandler<CancelarReservaVeiculoCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public CancelarReservaVeiculoCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }

        public async Task<CommandResult> Handle(CancelarReservaVeiculoCommand request, CancellationToken ct)
        {
            var (tenantId, usuario) = ConTransicaoBase.Contexto(_t, _u);
            var r = await _ctx.ReservasVeiculo.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.ReservaId, ct);
            if (r == null) return CommandResult.Falha("Reserva não encontrada.");

            r.Cancelar(request.Motivo, usuario);
            if (!r.IsValid) return CommandResult.Falha(r.Notifications.Select(n => n.Message));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Reserva cancelada.", new { r.Id, r.Status });
        }
    }

    // ==================== CON-FIN ====================

    public class EncerrarJornadaFinCommandHandler : ICommandHandler<EncerrarJornadaFinCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public EncerrarJornadaFinCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }

        public async Task<CommandResult> Handle(EncerrarJornadaFinCommand request, CancellationToken ct)
        {
            var (tenantId, usuario) = ConTransicaoBase.Contexto(_t, _u);
            var j = await _ctx.JornadasFin.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.JornadaId, ct);
            if (j == null) return CommandResult.Falha("Jornada não encontrada.");

            j.Encerrar(usuario);
            if (!j.IsValid) return CommandResult.Falha(j.Notifications.Select(n => n.Message));

            _ctx.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Concessionarias.FinJornadaEncerrada,
                JsonSerializer.Serialize(new { jornadaId = j.Id, tenantId })));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Jornada encerrada.", new { j.Id, j.Status });
        }
    }

    public class LiquidarContratoFinCommandHandler : ICommandHandler<LiquidarContratoFinCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public LiquidarContratoFinCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }

        public async Task<CommandResult> Handle(LiquidarContratoFinCommand request, CancellationToken ct)
        {
            var (tenantId, usuario) = ConTransicaoBase.Contexto(_t, _u);
            var c = await _ctx.ContratosFin.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.ContratoId, ct);
            if (c == null) return CommandResult.Falha("Contrato não encontrado.");

            c.Liquidar(usuario);
            if (!c.IsValid) return CommandResult.Falha(c.Notifications.Select(n => n.Message));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Contrato liquidado.", new { c.Id, c.Status });
        }
    }

    public class CancelarContratoFinCommandHandler : ICommandHandler<CancelarContratoFinCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public CancelarContratoFinCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }

        public async Task<CommandResult> Handle(CancelarContratoFinCommand request, CancellationToken ct)
        {
            var (tenantId, usuario) = ConTransicaoBase.Contexto(_t, _u);
            var c = await _ctx.ContratosFin.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.ContratoId, ct);
            if (c == null) return CommandResult.Falha("Contrato não encontrado.");

            c.Cancelar(usuario);
            if (!c.IsValid) return CommandResult.Falha(c.Notifications.Select(n => n.Message));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Contrato cancelado.", new { c.Id, c.Status });
        }
    }

    // ==================== CON-GAR ====================

    public class JulgarSolicitacaoGarantiaCommandHandler : ICommandHandler<JulgarSolicitacaoGarantiaCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public JulgarSolicitacaoGarantiaCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }

        public async Task<CommandResult> Handle(JulgarSolicitacaoGarantiaCommand request, CancellationToken ct)
        {
            var (tenantId, usuario) = ConTransicaoBase.Contexto(_t, _u);
            var s = await _ctx.SolicitacoesGarantia.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.SolicitacaoId, ct);
            if (s == null) return CommandResult.Falha("Solicitação de garantia não encontrada.");

            if (request.Aprovar) s.Aprovar(usuario); else s.Rejeitar(usuario);
            if (!s.IsValid) return CommandResult.Falha(s.Notifications.Select(n => n.Message));

            _ctx.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Concessionarias.GarSolicitacaoJulgada,
                JsonSerializer.Serialize(new { solicitacaoId = s.Id, protocolo = s.Protocolo, status = s.Status, tenantId })));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Solicitação julgada.", new { s.Id, s.Status });
        }
    }

    // ==================== CON-PES ====================

    public class CancelarReservaPecaCommandHandler : ICommandHandler<CancelarReservaPecaCommand>
    {
        private readonly ContextDMS _ctx; private readonly ITenantProvider _t; private readonly ICurrentUser _u;
        public CancelarReservaPecaCommandHandler(ContextDMS ctx, ITenantProvider t, ICurrentUser u) { _ctx = ctx; _t = t; _u = u; }

        public async Task<CommandResult> Handle(CancelarReservaPecaCommand request, CancellationToken ct)
        {
            var (tenantId, usuario) = ConTransicaoBase.Contexto(_t, _u);
            var r = await _ctx.ReservasPeca.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.ReservaId, ct);
            if (r == null) return CommandResult.Falha("Reserva de peça não encontrada.");

            r.Cancelar(usuario);
            if (!r.IsValid) return CommandResult.Falha(r.Notifications.Select(n => n.Message));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Reserva de peça cancelada.", new { r.Id });
        }
    }
}
