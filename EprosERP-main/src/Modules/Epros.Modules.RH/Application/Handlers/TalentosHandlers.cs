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
    public class CriarMetaColaboradorCommandHandler : ICommandHandler<CriarMetaColaboradorCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public CriarMetaColaboradorCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarMetaColaboradorCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var meta = new TltMetaColaborador(
                request.ColaboradorId, request.TipoMetaId, request.Titulo, request.Descricao,
                request.DataInicio, request.DataFim, request.Alvo, request.Progresso,
                TltMetaColaborador.StNaoIniciada, request.CriadoPorId, request.OwnerId, tenantId, usuario);

            meta.ValidarRegras();
            if (!meta.IsValid)
                return CommandResult.Falha(meta.Notifications.Select(n => n.Message));

            _context.TltMetaColaboradors.Add(meta);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Meta criada.", new { MetaId = meta.Id, meta.Status });
        }
    }

    public class RegistrarSolicitacaoLicencaCommandHandler : ICommandHandler<RegistrarSolicitacaoLicencaCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RegistrarSolicitacaoLicencaCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarSolicitacaoLicencaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var solicitacao = new TltSolicitacaoLicenca(
                request.ColaboradorId, request.TipoLicencaId, request.DataInicio, request.DataFim, request.TotalDias,
                request.Motivo, request.Anexo, TltSolicitacaoLicenca.StPendente, null, null, null,
                request.CriadoPorId, request.OwnerId, tenantId, usuario);

            if (!solicitacao.PeriodoValido())
                return CommandResult.Falha("A data final da licenca deve ser maior ou igual a data inicial (TLT secao 20).");
            if (!solicitacao.IsValid)
                return CommandResult.Falha(solicitacao.Notifications.Select(n => n.Message));

            _context.TltSolicitacaoLicencas.Add(solicitacao);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Solicitacao de licenca registrada.", new { SolicitacaoId = solicitacao.Id, solicitacao.Status });
        }
    }

    public class AprovarSolicitacaoLicencaCommandHandler : ICommandHandler<AprovarSolicitacaoLicencaCommand>
    {
        private readonly ContextRH _context;
        private readonly ICurrentUser _user;

        public AprovarSolicitacaoLicencaCommandHandler(ContextRH context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AprovarSolicitacaoLicencaCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var s = await _context.TltSolicitacaoLicencas.FirstOrDefaultAsync(x => x.Id == request.SolicitacaoId, ct);
            if (s == null) return CommandResult.Falha("Solicitacao nao encontrada.");

            s.Aprovar(request.AprovadoPorId, request.Comentario, usuario);
            if (!s.IsValid)
                return CommandResult.Falha(s.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Licenca aprovada.", new { s.Id, s.Status });
        }
    }

    public class RejeitarSolicitacaoLicencaCommandHandler : ICommandHandler<RejeitarSolicitacaoLicencaCommand>
    {
        private readonly ContextRH _context;
        private readonly ICurrentUser _user;

        public RejeitarSolicitacaoLicencaCommandHandler(ContextRH context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(RejeitarSolicitacaoLicencaCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var s = await _context.TltSolicitacaoLicencas.FirstOrDefaultAsync(x => x.Id == request.SolicitacaoId, ct);
            if (s == null) return CommandResult.Falha("Solicitacao nao encontrada.");

            s.Rejeitar(request.AprovadoPorId, request.Comentario, usuario);
            if (!s.IsValid)
                return CommandResult.Falha(s.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Licenca rejeitada.", new { s.Id, s.Status });
        }
    }

    public class ListarMetasColaboradorQueryHandler : IQueryHandler<ListarMetasColaboradorQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarMetasColaboradorQueryHandler(ContextRH context) => _context = context;
        public async Task<CommandResult> Handle(ListarMetasColaboradorQuery request, CancellationToken ct)
            => CommandResult.Ok("Metas listadas.", await _context.TltMetaColaboradors.OrderByDescending(m => m.CriadoEm).ToListAsync(ct));
    }

    public class ListarSolicitacoesLicencaQueryHandler : IQueryHandler<ListarSolicitacoesLicencaQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarSolicitacoesLicencaQueryHandler(ContextRH context) => _context = context;
        public async Task<CommandResult> Handle(ListarSolicitacoesLicencaQuery request, CancellationToken ct)
            => CommandResult.Ok("Solicitacoes listadas.", await _context.TltSolicitacaoLicencas.OrderByDescending(s => s.CriadoEm).ToListAsync(ct));
    }
}
