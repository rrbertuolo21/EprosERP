using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Application.Queries;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Handlers
{
    /// <summary>
    /// GRC — parametrizacao por tenant (D-TEC-04). Upsert por (TenantId, Chave) roteando para a
    /// tabela do submodulo. Cada submodulo tem a sua tabela propria (grc_*_parametro).
    /// </summary>
    public class DefinirParametroGrcCommandHandler : ICommandHandler<DefinirParametroGrcCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DefinirParametroGrcCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirParametroGrcCommand request, CancellationToken cancellationToken)
        {
            if (!SubmoduloGrc.EhValido(request.Submodulo))
                return CommandResult.Falha("Submodulo invalido para parametrizacao GRC.");

            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            switch (request.Submodulo)
            {
                case SubmoduloGrc.Politicas:
                    return await UpsertAsync(_context.PoliticaParametros,
                        (c, v) => new PoliticaParametro(c, v, tenantId, usuario), request, usuario, cancellationToken);
                case SubmoduloGrc.Compliance:
                    return await UpsertAsync(_context.ComplianceParametros,
                        (c, v) => new ComplianceParametro(c, v, tenantId, usuario), request, usuario, cancellationToken);
                case SubmoduloGrc.Riscos:
                    return await UpsertAsync(_context.RiscoParametros,
                        (c, v) => new RiscoParametro(c, v, tenantId, usuario), request, usuario, cancellationToken);
                case SubmoduloGrc.ControlesAuditoria:
                    return await UpsertAsync(_context.ControleParametros,
                        (c, v) => new ControleParametro(c, v, tenantId, usuario), request, usuario, cancellationToken);
                case SubmoduloGrc.SegregacaoFuncoes:
                    return await UpsertAsync(_context.SegregacaoParametros,
                        (c, v) => new SegregacaoParametro(c, v, tenantId, usuario), request, usuario, cancellationToken);
                default:
                    return CommandResult.Falha("Submodulo invalido para parametrizacao GRC.");
            }
        }

        private async Task<CommandResult> UpsertAsync<T>(
            DbSet<T> set,
            System.Func<string, string, T> factory,
            DefinirParametroGrcCommand request,
            string usuario,
            CancellationToken cancellationToken) where T : ParametroGrcBase
        {
            var existente = await set.FirstOrDefaultAsync(p => p.Chave == request.Chave, cancellationToken);
            if (existente != null)
            {
                existente.Alterar(request.ValorJson, request.Ativo, usuario);
                await _context.SaveChangesAsync(cancellationToken);
                return CommandResult.Ok("Parametro GRC atualizado.", new { existente.Id, request.Submodulo, request.Chave });
            }

            var novo = factory(request.Chave, request.ValorJson);
            if (!novo.IsValid)
                return CommandResult.Falha(novo.Notifications.Select(n => n.Message));

            set.Add(novo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Parametro GRC definido.", new { novo.Id, request.Submodulo, request.Chave });
        }
    }

    public class ObterParametrosGrcQueryHandler : IRequestHandler<ObterParametrosGrcQuery, IReadOnlyList<ParametroGrcDto>>
    {
        private readonly ContextGRC _context;
        public ObterParametrosGrcQueryHandler(ContextGRC context) => _context = context;

        public async Task<IReadOnlyList<ParametroGrcDto>> Handle(ObterParametrosGrcQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ParametroGrcBase> q = request.Submodulo switch
            {
                SubmoduloGrc.Politicas => _context.PoliticaParametros,
                SubmoduloGrc.Compliance => _context.ComplianceParametros,
                SubmoduloGrc.Riscos => _context.RiscoParametros,
                SubmoduloGrc.ControlesAuditoria => _context.ControleParametros,
                SubmoduloGrc.SegregacaoFuncoes => _context.SegregacaoParametros,
                _ => System.Linq.Enumerable.Empty<ParametroGrcBase>().AsQueryable()
            };

            return await q
                .OrderBy(p => p.Chave)
                .Select(p => new ParametroGrcDto(p.Chave, p.ValorJson, p.Ativo))
                .ToListAsync(cancellationToken);
        }
    }
}
