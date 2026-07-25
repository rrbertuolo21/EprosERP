using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Queries
{
    // ===================== Portal do Cliente (VEN-PCL) =====================

    public record ListarPortalUsuariosClienteQuery(Guid? ClienteId = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarPortalUsuariosClienteQueryHandler : IRequestHandler<ListarPortalUsuariosClienteQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarPortalUsuariosClienteQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarPortalUsuariosClienteQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.PortalUsuariosCliente.AsNoTracking().Where(u => u.TenantId == tenantId);
            if (request.ClienteId.HasValue) query = query.Where(u => u.ClienteId == request.ClienteId.Value);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(u => u.Nome)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(u => new { u.Id, u.ClienteId, u.Nome, u.Email, Status = u.Status.ToString(), u.AdministradorCliente })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Usuários do portal listados.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    /// <summary>
    /// §13/§18: solicitações consultadas sempre com filtro de cliente (nunca consulta sem critério de cliente/tenant).
    /// </summary>
    public record ListarPortalSolicitacoesQuery(Guid ClienteId, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarPortalSolicitacoesQueryHandler : IRequestHandler<ListarPortalSolicitacoesQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarPortalSolicitacoesQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarPortalSolicitacoesQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            // §13.5: consulta sempre com critério de cliente.
            if (request.ClienteId == Guid.Empty) return CommandResult.Falha("Consulta do portal exige cliente vinculado.");
            var query = _context.PortalSolicitacoes.AsNoTracking().Where(s => s.TenantId == tenantId && s.ClienteId == request.ClienteId);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(s => s.AbertaEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(s => new { s.Id, s.Assunto, Status = s.Status.ToString(), s.AbertaEm, s.RespondidaEm })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Solicitações listadas.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }
}
