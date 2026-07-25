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
    /// <summary>Cria Balanca.</summary>
    public class CriarBalancaCommandHandler : ICommandHandler<CriarBalancaCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarBalancaCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarBalancaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var balanca = new Balanca(request.Nome, request.QntDigitoIdentificador, request.QntDigitoCodigoProduto, request.QntDigitoValorProduto, request.QntCasaDecimal, request.TipoValor, tenantId, criadoPor);
            if (!balanca.IsValid)
                return CommandResult.Falha(balanca.Notifications.Select(n => n.Message), "Falha na validação da balança.");

            _context.Balancas.Add(balanca);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Balança criada com sucesso!", new { balanca.Id });
        }
    }

    /// <summary>Atualiza Balanca.</summary>
    public class AtualizarBalancaCommandHandler : ICommandHandler<AtualizarBalancaCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarBalancaCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarBalancaCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var balanca = await _context.Balancas.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
            if (balanca == null)
                return CommandResult.Falha("Balança não encontrada.");

            balanca.Alterar(request.Nome, request.QntDigitoIdentificador, request.QntDigitoCodigoProduto, request.QntDigitoValorProduto, request.QntCasaDecimal, request.TipoValor, usuario);
            if (!balanca.IsValid)
                return CommandResult.Falha(balanca.Notifications.Select(n => n.Message), "Falha na validação da balança.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Balança atualizada com sucesso!");
        }
    }

    /// <summary>Exclui Balanca.</summary>
    public class DeletarBalancaCommandHandler : ICommandHandler<DeletarBalancaCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DeletarBalancaCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarBalancaCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            // Carrega os produtos vinculados para aplicar a regra de deleção condicionada do legado.
            var balanca = await _context.Balancas.Include(b => b.Produtos).FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
            if (balanca == null)
                return CommandResult.Falha("Balança não encontrada.");

            balanca.DeletarSeNaoVinculada(usuario);
            if (!balanca.IsValid)
                return CommandResult.Falha(balanca.Notifications.Select(n => n.Message), "Não foi possível excluir a balança.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Balança excluída com sucesso!");
        }
    }
}
