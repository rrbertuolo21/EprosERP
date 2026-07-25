using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Handlers do submódulo Gestão de Armazém WMS (EST-WMS). Gravam em transação única, respeitam
    /// tenant (filtro global do ContextBase) e publicam eventos funcionais via Outbox (WMS-016/017/018).
    /// </summary>
    public class CriarWmsArmazemCommandHandler : ICommandHandler<CriarWmsArmazemCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarWmsArmazemCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarWmsArmazemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // WMS-015: dono/tenant. Fallback: identidade do usuário atual quando não informado.
            var donoId = request.UsuarioDonoId ?? (Guid.TryParse(usuario, out var g) ? g : Guid.Empty);

            var armazem = new WmsArmazem(request.Nome, request.Endereco, request.Cidade, request.Cep, request.Telefone, request.Email, request.Ativo, donoId, tenantId, usuario);
            if (!armazem.IsValid)
                return CommandResult.Falha(armazem.Notifications.Select(n => n.Message), "Dados do armazém são inválidos.");

            _context.WmsArmazens.Add(armazem);

            // WMS-016: evento após salvar.
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "estoque.wms.armazem_criado",
                JsonSerializer.Serialize(new { armazem.Id, armazem.Nome, armazem.Cidade, armazem.Ativo, tenantId, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Armazém criado com sucesso!", new { armazem.Id });
        }
    }

    public class AtualizarWmsArmazemCommandHandler : ICommandHandler<AtualizarWmsArmazemCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarWmsArmazemCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarWmsArmazemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var armazem = await _context.WmsArmazens.FirstOrDefaultAsync(a => a.Id == request.Id && a.DeletadoEm == null, cancellationToken);
            if (armazem == null)
                return CommandResult.Falha("Armazém não encontrado.");

            armazem.Alterar(request.Nome, request.Endereco, request.Cidade, request.Cep, request.Telefone, request.Email, request.Ativo, usuario);
            if (!armazem.IsValid)
                return CommandResult.Falha(armazem.Notifications.Select(n => n.Message), "Dados do armazém são inválidos.");

            // WMS-017: evento após atualizar.
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "estoque.wms.armazem_alterado",
                JsonSerializer.Serialize(new { armazem.Id, armazem.Nome, armazem.Cidade, armazem.Ativo, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Armazém atualizado com sucesso!");
        }
    }

    public class ExcluirWmsArmazemCommandHandler : ICommandHandler<ExcluirWmsArmazemCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ExcluirWmsArmazemCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirWmsArmazemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var armazem = await _context.WmsArmazens.FirstOrDefaultAsync(a => a.Id == request.Id && a.DeletadoEm == null, cancellationToken);
            if (armazem == null)
                return CommandResult.Falha("Armazém não encontrado.");

            // WMS-018: evento ANTES de remover.
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "estoque.wms.armazem_exclusao_solicitada",
                JsonSerializer.Serialize(new { armazem.Id, usuario, data = DateTime.UtcNow })));

            armazem.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Armazém excluído com sucesso!");
        }
    }
}
