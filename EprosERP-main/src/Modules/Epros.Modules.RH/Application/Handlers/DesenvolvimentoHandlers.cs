using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Application.Queries;
using Epros.Modules.RH.Domain.Entities;
using Epros.Modules.RH.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.RH.Application.Handlers
{
    public class RegistrarPromocaoCommandHandler : ICommandHandler<RegistrarPromocaoCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RegistrarPromocaoCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarPromocaoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            // DEV secao 14.9: promocao nasce Pendente.
            var promocao = new DevPromocao(
                request.ColaboradorId, request.FilialAnteriorId, request.DepartamentoAnteriorId, request.CargoAnteriorId,
                request.FilialAtualId, request.DepartamentoAtualId, request.CargoAtualId, request.DataEfetiva,
                request.Motivo, request.Documento, DevPromocao.StPendente, tenantId, usuario);

            if (!promocao.IsValid)
                return CommandResult.Falha(promocao.Notifications.Select(n => n.Message));

            _context.DevPromocaos.Add(promocao);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Promocao registrada.", new { PromocaoId = promocao.Id, promocao.Status });
        }
    }

    public class AprovarPromocaoCommandHandler : ICommandHandler<AprovarPromocaoCommand>
    {
        private readonly ContextRH _context;
        private readonly ICurrentUser _user;

        public AprovarPromocaoCommandHandler(ContextRH context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AprovarPromocaoCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var promocao = await _context.DevPromocaos.FirstOrDefaultAsync(p => p.Id == request.PromocaoId, ct);
            if (promocao == null) return CommandResult.Falha("Promocao nao encontrada.");

            promocao.Aprovar(usuario);
            if (!promocao.IsValid)
                return CommandResult.Falha(promocao.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Promocao aprovada.", new { promocao.Id, promocao.Status });
        }
    }

    public class RejeitarPromocaoCommandHandler : ICommandHandler<RejeitarPromocaoCommand>
    {
        private readonly ContextRH _context;
        private readonly ICurrentUser _user;

        public RejeitarPromocaoCommandHandler(ContextRH context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(RejeitarPromocaoCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var promocao = await _context.DevPromocaos.FirstOrDefaultAsync(p => p.Id == request.PromocaoId, ct);
            if (promocao == null) return CommandResult.Falha("Promocao nao encontrada.");

            promocao.Rejeitar(usuario);
            if (!promocao.IsValid)
                return CommandResult.Falha(promocao.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Promocao rejeitada.", new { promocao.Id, promocao.Status });
        }
    }

    public class RegistrarAdvertenciaCommandHandler : ICommandHandler<RegistrarAdvertenciaCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RegistrarAdvertenciaCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarAdvertenciaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var advertencia = new DevAdvertencia(
                request.ColaboradorId, request.TipoAdvertenciaId, request.Assunto, request.Severidade,
                request.DataAdvertencia, request.Descricao, request.Documento, request.AdvertidoPor,
                null, null, tenantId, usuario);

            if (!advertencia.IsValid)
                return CommandResult.Falha(advertencia.Notifications.Select(n => n.Message));

            _context.DevAdvertencias.Add(advertencia);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Advertencia registrada.", new { AdvertenciaId = advertencia.Id });
        }
    }

    public class RegistrarDesligamentoCommandHandler : ICommandHandler<RegistrarDesligamentoCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RegistrarDesligamentoCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarDesligamentoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            // DEV secao 16.11: desligamento administrativo nao calcula rescisao financeira.
            var desligamento = new DevDesligamento(
                request.ColaboradorId, request.TipoDesligamentoId, request.DataAviso, request.DataDesligamento,
                request.Motivo, request.Descricao, request.Documento, null, null, tenantId, usuario);

            if (!desligamento.IsValid)
                return CommandResult.Falha(desligamento.Notifications.Select(n => n.Message));

            _context.DevDesligamentos.Add(desligamento);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Desligamento administrativo registrado (sem calculo de rescisao).", new { DesligamentoId = desligamento.Id });
        }
    }

    public class ListarPromocoesQueryHandler : IQueryHandler<ListarPromocoesQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarPromocoesQueryHandler(ContextRH context) => _context = context;
        public async Task<CommandResult> Handle(ListarPromocoesQuery request, CancellationToken ct)
            => CommandResult.Ok("Promocoes listadas.", await _context.DevPromocaos.OrderByDescending(p => p.CriadoEm).ToListAsync(ct));
    }

    public class ListarAdvertenciasQueryHandler : IQueryHandler<ListarAdvertenciasQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarAdvertenciasQueryHandler(ContextRH context) => _context = context;
        public async Task<CommandResult> Handle(ListarAdvertenciasQuery request, CancellationToken ct)
            => CommandResult.Ok("Advertencias listadas.", await _context.DevAdvertencias.OrderByDescending(a => a.CriadoEm).ToListAsync(ct));
    }
}
