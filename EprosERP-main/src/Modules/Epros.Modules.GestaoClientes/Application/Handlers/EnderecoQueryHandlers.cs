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
    /// <summary>Lista Enderecos.</summary>
    public class ListarEnderecosQueryHandler : IQueryHandler<ListarEnderecosQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarEnderecosQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarEnderecosQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.EnderecosPessoas
                .AsNoTracking()
                .Where(e => e.TenantId == tenantId);

            if (request.PessoaId.HasValue)
            {
                query = query.Where(e => e.PessoaId == request.PessoaId.Value);
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Endereços listados com sucesso.", new { Total = total, Itens = itens });
        }
    }

    /// <summary>Obtém Endereco Por Id.</summary>
    public class ObterEnderecoPorIdQueryHandler : IQueryHandler<ObterEnderecoPorIdQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterEnderecoPorIdQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterEnderecoPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var endereco = await _context.EnderecosPessoas
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == request.Id && e.TenantId == tenantId, cancellationToken);

            if (endereco == null)
            {
                return CommandResult.Falha("Endereço não encontrado");
            }

            return CommandResult.Ok("Endereço obtido com sucesso.", endereco);
        }
    }
}
