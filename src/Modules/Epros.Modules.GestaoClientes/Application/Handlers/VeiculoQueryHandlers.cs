using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Lista Veiculos.</summary>
    public class ListarVeiculosQueryHandler : IQueryHandler<ListarVeiculosQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarVeiculosQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarVeiculosQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.PessoasVeiculos
                .AsNoTracking()
                .Where(v => v.TenantId == tenantId);

            if (request.PessoaId.HasValue)
            {
                query = query.Where(v => v.PessoaId == request.PessoaId.Value);
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Veículos listados com sucesso.", new { Total = total, Itens = itens });
        }
    }

    /// <summary>Obtém Veiculo Por Id.</summary>
    public class ObterVeiculoPorIdQueryHandler : IQueryHandler<ObterVeiculoPorIdQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterVeiculoPorIdQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterVeiculoPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var veiculo = await _context.PessoasVeiculos
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == request.Id && v.TenantId == tenantId, cancellationToken);

            if (veiculo == null)
            {
                return CommandResult.Falha("Veículo não encontrado");
            }

            return CommandResult.Ok("Veículo obtido com sucesso.", veiculo);
        }
    }
}
