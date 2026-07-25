using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Commands;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Application.Handlers
{
    public class CriarNcrCommandHandler : ICommandHandler<CriarNcrCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarNcrCommandHandler(ContextQualidade context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarNcrCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // RN: codigo unico por tenant.
            var duplicado = await _context.NcrRegistros
                .AnyAsync(n => n.Codigo == request.Codigo, cancellationToken);
            if (duplicado)
                return CommandResult.Falha($"Ja existe uma NCR com o codigo '{request.Codigo}' neste tenant.", block: true);

            var ncr = new NcrRegistro(
                request.Codigo, request.Titulo, request.Descricao, request.OrigemPrincipal,
                request.Prioridade, request.ResponsavelId, request.Severidade, request.DataOcorrencia,
                tenantId, usuario);

            if (!ncr.IsValid)
                return CommandResult.Falha(ncr.Notifications.Select(n => n.Message));

            _context.NcrRegistros.Add(ncr);

            var historico = new NcrHistorico(ncr.Id, EAcaoHistoricoQualidade.Criado,
                Guid.TryParse(usuario, out var uid) ? uid : Guid.Empty, "{}", null,
                EStatusRegistroQualidade.Rascunho.ToString(), null, null, tenantId, usuario);
            if (historico.IsValid)
                _context.NcrHistoricos.Add(historico);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("NCR registrada com sucesso!", new { ncr.Id, ncr.Codigo, Status = ncr.StatusRegistro.ToString() });
        }
    }
}
