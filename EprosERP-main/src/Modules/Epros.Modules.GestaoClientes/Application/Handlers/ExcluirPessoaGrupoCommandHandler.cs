using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Exclui Pessoa Grupo.</summary>
    public class ExcluirPessoaGrupoCommandHandler : ICommandHandler<ExcluirPessoaGrupoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ExcluirPessoaGrupoCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirPessoaGrupoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var deletadoPor = _currentUser.GetUserId() ?? "system";

            var grupo = await _context.PessoaGrupos
                .FirstOrDefaultAsync(g => g.Id == request.Id && g.TenantId == tenantId, cancellationToken);
            if (grupo == null)
            {
                return CommandResult.Falha(new[] { "Grupo não encontrado" }, "Erro de validação");
            }

            grupo.Deletar(deletadoPor);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Grupo de Pessoa excluído com sucesso!", new { PessoaGrupoId = grupo.Id });
        }
    }
}
