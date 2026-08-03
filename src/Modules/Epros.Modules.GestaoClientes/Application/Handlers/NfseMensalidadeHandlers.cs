using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Application.Security;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>
    /// 1.08J — Handler da listagem LANDLORD de NFS-e da mensalidade (visibilidade do operador interno). É uma
    /// consulta consolidada por TODOS os tenants (IgnoreQueryFilters), portanto exige operador interno
    /// (tenant "system") — fail-closed, mesmo que o AbacFilter do controller seja contornado. NÃO emite NFS-e.
    /// </summary>
    public class ListarNfseMensalidadesQueryHandler
        : IQueryHandler<ListarNfseMensalidadesQuery, PagedQueryResult<NfseMensalidadeListaDto>>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarNfseMensalidadesQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<PagedQueryResult<NfseMensalidadeListaDto>> Handle(
            ListarNfseMensalidadesQuery request, CancellationToken cancellationToken)
        {
            if (!GuardaOperadorInterno.EhOperadorInterno(_tenantProvider))
            {
                throw new UnauthorizedAccessException(
                    "Acesso Proibido: NFS-e da mensalidade restrita ao operador interno da Siser (landlord).");
            }

            var query = _context.NfseMensalidades.IgnoreQueryFilters().Where(n => n.DeletadoEm == null);

            if (request.ClienteId.HasValue && request.ClienteId.Value != Guid.Empty)
                query = query.Where(n => n.ClienteId == request.ClienteId.Value);

            if (!string.IsNullOrWhiteSpace(request.Status)
                && Enum.TryParse<NfseMensalidadeStatus>(request.Status, ignoreCase: true, out var status))
                query = query.Where(n => n.Status == status);

            var totalRegistros = await query.CountAsync(cancellationToken);
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)request.TamanhoPagina);

            var items = await query
                .OrderBy(n => n.Competencia).ThenByDescending(n => n.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(n => new NfseMensalidadeListaDto
                {
                    Id = n.Id,
                    FaturaId = n.FaturaId,
                    ClienteId = n.ClienteId,
                    ClienteRazaoSocial = _context.Clientes.IgnoreQueryFilters()
                        .Where(c => c.Id == n.ClienteId)
                        .Select(c => c.RazaoSocial)
                        .FirstOrDefault() ?? string.Empty,
                    Competencia = n.Competencia,
                    ValorBase = n.ValorBase,
                    Status = n.Status.ToString(),
                    Ambiente = n.Ambiente.ToString(),
                    Motivo = n.Motivo,
                    NumeroNfse = n.NumeroNfse,
                    EmitidaEm = n.EmitidaEm,
                    CriadoEm = n.CriadoEm
                })
                .ToListAsync(cancellationToken);

            return new PagedQueryResult<NfseMensalidadeListaDto>(items, totalRegistros, totalPaginas);
        }
    }
}
