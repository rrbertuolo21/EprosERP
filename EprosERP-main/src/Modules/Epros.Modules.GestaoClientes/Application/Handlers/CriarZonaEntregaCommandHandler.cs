using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Cria Zona Entrega.</summary>
    public class CriarZonaEntregaCommandHandler : ICommandHandler<CriarZonaEntregaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarZonaEntregaCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarZonaEntregaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var existeNome = await _context.ZonasEntrega.AnyAsync(z => z.Nome == request.Nome && z.TenantId == tenantId, cancellationToken);
            if (existeNome)
                return CommandResult.Falha(new[] { "Zona de entrega com esse nome já cadastrada." }, "Erro de validação");

            // Validar sobreposição de faixas de CEP (REG-041)
            var zonasExistentes = await _context.ZonasEntrega
                .Where(z => z.TenantId == tenantId && z.Ativo && z.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            foreach (var z in zonasExistentes)
            {
                if (string.Compare(request.CepInicio, z.CepFim) <= 0 && string.Compare(request.CepFim, z.CepInicio) >= 0)
                {
                    return CommandResult.Falha(new[] { $"A faixa de CEP informada ({request.CepInicio} a {request.CepFim}) se sobrepõe com a zona de entrega '{z.Nome}' ({z.CepInicio} a {z.CepFim})." }, "Erro de validação");
                }
            }

            var zona = new ZonaEntrega(
                request.Nome,
                request.CepInicio,
                request.CepFim,
                tenantId,
                criadoPor
            );

            if (!zona.IsValid)
            {
                var erros = zona.Notifications.Select(n => n.Message).Distinct();
                return CommandResult.Falha(erros, "Invariantes de domínio da Zona de Entrega não foram atendidas.");
            }

            _context.ZonasEntrega.Add(zona);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Zona de entrega cadastrada com sucesso!", new { ZonaEntregaId = zona.Id });
        }
    }
}
