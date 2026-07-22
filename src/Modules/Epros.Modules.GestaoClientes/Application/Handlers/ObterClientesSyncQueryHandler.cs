using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Obtém Clientes Sync.</summary>
    public class ObterClientesSyncQueryHandler : IQueryHandler<ObterClientesSyncQuery, IEnumerable<ClienteSyncDto>>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterClientesSyncQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<IEnumerable<ClienteSyncDto>> Handle(ObterClientesSyncQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Usamos IgnoreQueryFilters para trazer itens com soft-delete.
            // Para segurança contra Tenant Leak, aplicamos o filtro de tenantId de forma manual.
            var clientes = await _context.Clientes
                .IgnoreQueryFilters()
                .Where(c => c.TenantId == tenantId)
                .Where(c => c.CriadoEm > request.Since 
                            || (c.AlteradoEm != null && c.AlteradoEm > request.Since) 
                            || (c.DeletadoEm != null && c.DeletadoEm > request.Since))
                .Select(c => new ClienteSyncDto
                {
                    Id = c.Id,
                    SyncId = c.SyncId,
                    RazaoSocial = c.RazaoSocial,
                    Cnpj = c.Cnpj,
                    Email = c.Email,
                    PlanoId = c.PlanoId,
                    SyncVersion = c.SyncVersion,
                    Ativo = c.Ativo,
                    Deletado = c.DeletadoEm != null,
                    CriadoEm = c.CriadoEm,
                    AlteradoEm = c.AlteradoEm,
                    DeletadoEm = c.DeletadoEm
                })
                .ToListAsync(cancellationToken);

            return clientes;
        }
    }
}
