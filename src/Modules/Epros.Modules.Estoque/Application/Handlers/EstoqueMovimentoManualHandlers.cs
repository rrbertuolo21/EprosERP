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
    /// <summary>Cria Estoque Movimento Manual.</summary>
    public class CriarEstoqueMovimentoManualCommandHandler : ICommandHandler<CriarEstoqueMovimentoManualCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEstoqueMovimentoManualCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEstoqueMovimentoManualCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var movimento = new EstoqueMovimentoManual(request.ProdutoId, request.TipoEstoque, request.TipoMovimento, request.QuantidadeMovimentada, request.ValorUnitario, tenantId, criadoPor);

            _context.EstoqueMovimentosManuais.Add(movimento);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Movimento manual de estoque criado com sucesso!", new { movimento.Id });
        }
    }

    /// <summary>Atualiza Estoque Movimento Manual.</summary>
    public class AtualizarEstoqueMovimentoManualCommandHandler : ICommandHandler<AtualizarEstoqueMovimentoManualCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarEstoqueMovimentoManualCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarEstoqueMovimentoManualCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var movimento = await _context.EstoqueMovimentosManuais.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (movimento == null)
                return CommandResult.Falha("Movimento manual de estoque não encontrado.");

            movimento.Alterar(request.ProdutoId, request.TipoEstoque, request.TipoMovimento, request.QuantidadeMovimentada, request.ValorUnitario, usuario);

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Movimento manual de estoque atualizado com sucesso!");
        }
    }

    /// <summary>Exclui Estoque Movimento Manual.</summary>
    public class DeletarEstoqueMovimentoManualCommandHandler : ICommandHandler<DeletarEstoqueMovimentoManualCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DeletarEstoqueMovimentoManualCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarEstoqueMovimentoManualCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var movimento = await _context.EstoqueMovimentosManuais.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (movimento == null)
                return CommandResult.Falha("Movimento manual de estoque não encontrado.");

            movimento.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Movimento manual de estoque excluído com sucesso!");
        }
    }
}
