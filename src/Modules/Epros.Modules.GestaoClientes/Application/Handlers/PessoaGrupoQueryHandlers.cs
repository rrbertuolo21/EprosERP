using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Lista Pessoa Grupos.</summary>
    public class ListarPessoaGruposQueryHandler : IQueryHandler<ListarPessoaGruposQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarPessoaGruposQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarPessoaGruposQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var grupos = await _context.PessoaGrupos
                .Where(g => g.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Grupos de Pessoa listados com sucesso.", grupos);
        }
    }

    /// <summary>Obtém Pessoa Grupo Por Id.</summary>
    public class ObterPessoaGrupoPorIdQueryHandler : IQueryHandler<ObterPessoaGrupoPorIdQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterPessoaGrupoPorIdQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterPessoaGrupoPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var grupo = await _context.PessoaGrupos
                .FirstOrDefaultAsync(g => g.Id == request.Id && g.TenantId == tenantId, cancellationToken);

            if (grupo == null)
                return CommandResult.Falha(new[] { "Grupo não encontrado" });

            return CommandResult.Ok("Grupo de Pessoa obtido com sucesso.", grupo);
        }
    }
}
