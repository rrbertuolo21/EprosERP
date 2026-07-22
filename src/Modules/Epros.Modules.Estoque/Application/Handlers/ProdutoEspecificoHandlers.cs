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
    /// <summary>Cria Produto Especifico.</summary>
    public class CriarProdutoEspecificoCommandHandler : ICommandHandler<CriarProdutoEspecificoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarProdutoEspecificoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarProdutoEspecificoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var especifico = new ProdutoEspecifico(
                request.ProdutoId,
                request.ValorPercentualGlpDerivadoPetroleo,
                request.ValorPercentualGasNaturalNacional,
                request.ValorPercentualGasNaturalImportado,
                request.ValorPartida,
                request.UfConsumo,
                tenantId,
                criadoPor);

            if (request.Origens != null)
            {
                foreach (var origem in request.Origens)
                    especifico.AdicionarOrigem(origem.IndicadorImportacao, origem.UfOrigem, origem.ValorPercentualUf, criadoPor);
            }

            if (!especifico.IsValid)
                return CommandResult.Falha(especifico.Notifications.Select(n => n.Message), "Falha na validação do produto específico.");

            foreach (var origem in especifico.Origens)
            {
                if (!origem.IsValid)
                    especifico.AddNotifications(origem.Notifications);
            }

            if (!especifico.IsValid)
                return CommandResult.Falha(especifico.Notifications.Select(n => n.Message), "Falha na validação das origens de combustível.");

            _context.ProdutosEspecificos.Add(especifico);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Produto específico criado com sucesso!", new { especifico.Id });
        }
    }

    /// <summary>Atualiza Produto Especifico.</summary>
    public class AtualizarProdutoEspecificoCommandHandler : ICommandHandler<AtualizarProdutoEspecificoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarProdutoEspecificoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarProdutoEspecificoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var especifico = await _context.ProdutosEspecificos
                .Include(p => p.Origens)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (especifico == null)
                return CommandResult.Falha("Produto específico não encontrado.");

            especifico.Alterar(
                request.ValorPercentualGlpDerivadoPetroleo,
                request.ValorPercentualGasNaturalNacional,
                request.ValorPercentualGasNaturalImportado,
                request.ValorPartida,
                request.UfConsumo,
                usuario);

            if (request.Origens != null)
            {
                foreach (var origem in especifico.Origens.ToList())
                    especifico.DeletarOrigem(origem.Id, usuario);

                foreach (var origem in request.Origens)
                    especifico.AdicionarOrigem(origem.IndicadorImportacao, origem.UfOrigem, origem.ValorPercentualUf, usuario);
            }

            if (!especifico.IsValid)
                return CommandResult.Falha(especifico.Notifications.Select(n => n.Message), "Falha na validação do produto específico.");

            foreach (var origem in especifico.Origens)
            {
                if (!origem.IsValid)
                    especifico.AddNotifications(origem.Notifications);
            }

            if (!especifico.IsValid)
                return CommandResult.Falha(especifico.Notifications.Select(n => n.Message), "Falha na validação das origens de combustível.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Produto específico atualizado com sucesso!");
        }
    }

    /// <summary>Exclui Produto Especifico.</summary>
    public class DeletarProdutoEspecificoCommandHandler : ICommandHandler<DeletarProdutoEspecificoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DeletarProdutoEspecificoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarProdutoEspecificoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var especifico = await _context.ProdutosEspecificos
                .Include(p => p.Origens)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (especifico == null)
                return CommandResult.Falha("Produto específico não encontrado.");

            especifico.DeletarComOrigens(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Produto específico excluído com sucesso!");
        }
    }
}
