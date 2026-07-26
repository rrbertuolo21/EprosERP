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
    /// <summary>Cria Unidade Medida Tributavel.</summary>
    public class CriarUnidadeMedidaTributavelCommandHandler : ICommandHandler<CriarUnidadeMedidaTributavelCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarUnidadeMedidaTributavelCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarUnidadeMedidaTributavelCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var unidade = new UnidadeMedidaTributavel(request.CodigoNcm, request.DataInicioVigencia, request.DataFimVigencia, request.UnidadeMedida, request.Descricao, tenantId, criadoPor);
            if (!unidade.IsValid)
                return CommandResult.Falha(unidade.Notifications.Select(n => n.Message), "Falha na validação da unidade de medida tributável.");

            _context.UnidadesMedidaTributavel.Add(unidade);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Unidade de medida tributável criada com sucesso!", new { unidade.Id });
        }
    }

    /// <summary>Atualiza Unidade Medida Tributavel.</summary>
    public class AtualizarUnidadeMedidaTributavelCommandHandler : ICommandHandler<AtualizarUnidadeMedidaTributavelCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarUnidadeMedidaTributavelCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarUnidadeMedidaTributavelCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var unidade = await _context.UnidadesMedidaTributavel.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (unidade == null)
                return CommandResult.Falha("Unidade de medida tributável não encontrada.");

            unidade.Alterar(request.CodigoNcm, request.DataInicioVigencia, request.DataFimVigencia, request.UnidadeMedida, request.Descricao, usuario);
            if (!unidade.IsValid)
                return CommandResult.Falha(unidade.Notifications.Select(n => n.Message), "Falha na validação da unidade de medida tributável.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Unidade de medida tributável atualizada com sucesso!");
        }
    }

    /// <summary>Exclui Unidade Medida Tributavel.</summary>
    public class DeletarUnidadeMedidaTributavelCommandHandler : ICommandHandler<DeletarUnidadeMedidaTributavelCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DeletarUnidadeMedidaTributavelCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarUnidadeMedidaTributavelCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var unidade = await _context.UnidadesMedidaTributavel.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (unidade == null)
                return CommandResult.Falha("Unidade de medida tributável não encontrada.");

            unidade.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Unidade de medida tributável excluída com sucesso!");
        }
    }
}
