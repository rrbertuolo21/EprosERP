using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>APP-TEN-003 §11.8: define grant/deny direto de capacidade ao usuário (upsert). REG-040/041.</summary>
    public class DefinirUsuarioCapacidadeCommandHandler : ICommandHandler<DefinirUsuarioCapacidadeCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DefinirUsuarioCapacidadeCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirUsuarioCapacidadeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var capacidadeExiste = await _context.Capacidades.AnyAsync(c => c.Id == request.CapacidadeId && c.TenantId == tenantId, cancellationToken);
            if (!capacidadeExiste)
                return CommandResult.Falha(new[] { "Capacidade não encontrada." }, "Erro de validação");

            var existente = await _context.UsuariosCapacidades
                .FirstOrDefaultAsync(uc => uc.UsuarioId == request.UsuarioId && uc.CapacidadeId == request.CapacidadeId && uc.TenantId == tenantId, cancellationToken);

            if (existente != null)
            {
                existente.AlterarConcessao(request.Granted, userId);
                await _context.SaveChangesAsync(cancellationToken);
                return CommandResult.Ok("Concessão atualizada com sucesso!", new { existente.Id, existente.Granted });
            }

            var novo = new UsuarioCapacidade(request.UsuarioId, request.CapacidadeId, request.Granted, tenantId, userId);
            if (!novo.IsValid)
                return CommandResult.Falha(novo.Notifications.Select(n => n.Message).Distinct(), "Erro de validação");

            _context.UsuariosCapacidades.Add(novo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Concessão registrada com sucesso!", new { novo.Id, novo.Granted });
        }
    }

    /// <summary>Remove um grant/deny direto de capacidade.</summary>
    public class RemoverUsuarioCapacidadeCommandHandler : ICommandHandler<RemoverUsuarioCapacidadeCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public RemoverUsuarioCapacidadeCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(RemoverUsuarioCapacidadeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var item = await _context.UsuariosCapacidades.FirstOrDefaultAsync(uc => uc.Id == request.Id && uc.TenantId == tenantId, cancellationToken);
            if (item == null)
                return CommandResult.Falha(new[] { "Concessão não encontrada." });

            _context.UsuariosCapacidades.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Concessão removida com sucesso!", new { request.Id });
        }
    }

    // HistoricoLogin e SessaoImpersonacao (APP-TEN-003 §11.9/§11.13) pertencem ao módulo
    // Aplicativo (Identity); os handlers correspondentes vivem em Epros.Modules.Aplicativo.
}
