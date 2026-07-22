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
    /// <summary>Cria Adicionais.</summary>
    public class CriarAdicionaisCommandHandler : ICommandHandler<CriarAdicionaisCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarAdicionaisCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarAdicionaisCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var adicional = new Adicionais(request.Descricao, request.ValorPreco, tenantId, criadoPor);
            if (!adicional.IsValid)
                return CommandResult.Falha(adicional.Notifications.Select(n => n.Message), "Falha na validação do adicional.");

            _context.Adicionais.Add(adicional);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Adicional criado com sucesso!", new { adicional.Id });
        }
    }

    /// <summary>Atualiza Adicionais.</summary>
    public class AtualizarAdicionaisCommandHandler : ICommandHandler<AtualizarAdicionaisCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarAdicionaisCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarAdicionaisCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var adicional = await _context.Adicionais.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
            if (adicional == null)
                return CommandResult.Falha("Adicional não encontrado.");

            adicional.Alterar(request.Descricao, request.ValorPreco, usuario);
            if (!adicional.IsValid)
                return CommandResult.Falha(adicional.Notifications.Select(n => n.Message), "Falha na validação do adicional.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Adicional atualizado com sucesso!");
        }
    }

    /// <summary>Exclui Adicionais.</summary>
    public class DeletarAdicionaisCommandHandler : ICommandHandler<DeletarAdicionaisCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DeletarAdicionaisCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarAdicionaisCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var adicional = await _context.Adicionais.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
            if (adicional == null)
                return CommandResult.Falha("Adicional não encontrado.");

            adicional.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Adicional excluído com sucesso!");
        }
    }
}
