using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>Obtém Produtos Sync.</summary>
    public class ObterProdutosSyncQueryHandler : IQueryHandler<ObterProdutosSyncQuery, IEnumerable<ProdutoSyncDto>>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterProdutosSyncQueryHandler(ContextEstoque context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<IEnumerable<ProdutoSyncDto>> Handle(ObterProdutosSyncQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            var produtos = await _context.Produtos
                .IgnoreQueryFilters()
                .Where(p => p.TenantId == tenantId)
                .Where(p => p.CriadoEm > request.Since 
                            || (p.AlteradoEm != null && p.AlteradoEm > request.Since) 
                            || (p.DeletadoEm != null && p.DeletadoEm > request.Since))
                .Select(p => new ProdutoSyncDto
                {
                    Id = p.Id,
                    SyncId = p.SyncId,
                    Sku = p.Sku,
                    Nome = p.Nome,
                    PrecoVenda = p.PrecoVenda,
                    SaldoEstoque = p.SaldoEstoque,
                    SyncVersion = p.SyncVersion,
                    Deletado = p.DeletadoEm != null,
                    CriadoEm = p.CriadoEm,
                    AlteradoEm = p.AlteradoEm,
                    DeletadoEm = p.DeletadoEm
                })
                .ToListAsync(cancellationToken);

            return produtos;
        }
    }
}
