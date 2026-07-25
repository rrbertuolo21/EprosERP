using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>Cria Produto Grupo.</summary>
    public class CriarProdutoGrupoCommandHandler : ICommandHandler<CriarProdutoGrupoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarProdutoGrupoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarProdutoGrupoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var grupo = new ProdutoGrupo(request.Descricao, tenantId, criadoPor);
            if (!grupo.IsValid)
                return CommandResult.Falha(grupo.Notifications.Select(n => n.Message), "Falha na validação do grupo de produtos.");

            _context.ProdutoGrupos.Add(grupo);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Grupo de produtos criado com sucesso!", new { grupo.Id });
        }
    }

    /// <summary>Atualiza Produto Grupo.</summary>
    public class AtualizarProdutoGrupoCommandHandler : ICommandHandler<AtualizarProdutoGrupoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarProdutoGrupoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarProdutoGrupoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var grupo = await _context.ProdutoGrupos.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
            if (grupo == null)
                return CommandResult.Falha("Grupo de produtos não encontrado.");

            grupo.Alterar(request.Descricao, usuario);
            if (!grupo.IsValid)
                return CommandResult.Falha(grupo.Notifications.Select(n => n.Message), "Falha na validação do grupo de produtos.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Grupo de produtos atualizado com sucesso!");
        }
    }

    /// <summary>Exclui Produto Grupo.</summary>
    public class DeletarProdutoGrupoCommandHandler : ICommandHandler<DeletarProdutoGrupoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DeletarProdutoGrupoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarProdutoGrupoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var grupo = await _context.ProdutoGrupos.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
            if (grupo == null)
                return CommandResult.Falha("Grupo de produtos não encontrado.");

            grupo.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Grupo de produtos excluído com sucesso!");
        }
    }
}
