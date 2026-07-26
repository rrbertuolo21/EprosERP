using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    public class CriarServicoCatalogoCommandHandler : ICommandHandler<CriarServicoCatalogoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarServicoCatalogoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarServicoCatalogoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var servico = new ServicoCatalogo(
                request.Nome, request.Descricao, request.Valor, request.Taxa,
                request.PermiteCamposCustomizados, request.GrupoPrecoLocalidadeJson,
                request.TaxaEmbalagemEntrega, request.TipoTaxaEmbalagemEntrega,
                tenantId, usuario);

            if (!servico.IsValid)
                return CommandResult.Falha(servico.Notifications.Select(n => n.Message), "Dados do serviço inválidos.");

            _context.ServicoCatalogos.Add(servico);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Serviço cadastrado com sucesso!", new { servico.Id });
        }
    }

    public class AtualizarServicoCatalogoCommandHandler : ICommandHandler<AtualizarServicoCatalogoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarServicoCatalogoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarServicoCatalogoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var servico = await _context.ServicoCatalogos.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == request.Id, cancellationToken);
            if (servico == null) return CommandResult.Falha("Serviço não encontrado.");

            servico.Alterar(
                request.Nome, request.Descricao, request.Valor, request.Taxa,
                request.PermiteCamposCustomizados, request.GrupoPrecoLocalidadeJson,
                request.TaxaEmbalagemEntrega, request.TipoTaxaEmbalagemEntrega, usuario);

            if (!servico.IsValid)
                return CommandResult.Falha(servico.Notifications.Select(n => n.Message), "Dados do serviço inválidos.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Serviço atualizado com sucesso!", new { servico.Id });
        }
    }

    public class ExcluirServicoCatalogoCommandHandler : ICommandHandler<ExcluirServicoCatalogoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ExcluirServicoCatalogoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirServicoCatalogoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var servico = await _context.ServicoCatalogos.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == request.Id, cancellationToken);
            if (servico == null) return CommandResult.Falha("Serviço não encontrado.");

            // EF §8.10/§13.1: exclusão lógica via flag Ativo (permite reativação, EF §8.11),
            // preservando auditoria. A base EntidadeSaaSBase não expõe reversão de DeletadoEm.
            servico.Desativar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Serviço excluído logicamente.", new { servico.Id });
        }
    }

    public class ReativarServicoCatalogoCommandHandler : ICommandHandler<ReativarServicoCatalogoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ReativarServicoCatalogoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ReativarServicoCatalogoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // Exclusão lógica usa a flag Ativo (não DeletadoEm), então o registro segue consultável.
            var servico = await _context.ServicoCatalogos
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == request.Id, cancellationToken);
            if (servico == null) return CommandResult.Falha("Serviço não encontrado.");

            servico.Reativar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Serviço reativado com sucesso!", new { servico.Id });
        }
    }
}
