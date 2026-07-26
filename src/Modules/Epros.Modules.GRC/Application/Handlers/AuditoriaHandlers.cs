using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Handlers
{
    public class CriarPlanoAuditoriaCommandHandler : ICommandHandler<CriarPlanoAuditoriaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPlanoAuditoriaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPlanoAuditoriaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.PlanosAuditoria.AnyAsync(p => p.Codigo == request.Codigo, cancellationToken);
            if (duplicado)
            {
                return CommandResult.Falha("Ja existe plano de auditoria com este codigo para a empresa/tenant.");
            }

            var plano = new PlanoAuditoria(request.Codigo, request.Titulo, request.Descricao, request.Ciclo, tenantId, usuario);
            if (!plano.IsValid)
            {
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message));
            }

            _context.PlanosAuditoria.Add(plano);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Plano de auditoria criado com sucesso!", new { PlanoId = plano.Id, plano.Status });
        }
    }

    public class RegistrarTesteControleCommandHandler : ICommandHandler<RegistrarTesteControleCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarTesteControleCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarTesteControleCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var controleExiste = await _context.ControlesInternos.AnyAsync(c => c.Id == request.ControleId, cancellationToken);
            if (!controleExiste)
            {
                return CommandResult.Falha("Controle interno nao encontrado.");
            }

            var teste = new TesteControle(request.ControleId, request.PlanoAuditoriaId, request.Resultado, request.Observacao, tenantId, usuario);
            if (!teste.IsValid)
            {
                return CommandResult.Falha(teste.Notifications.Select(n => n.Message));
            }

            _context.TestesControle.Add(teste);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Teste de controle registrado com sucesso!",
                new { TesteId = teste.Id, teste.Resultado, ExigeAchado = teste.ExigeAchado() });
        }
    }

    public class RegistrarAchadoCommandHandler : ICommandHandler<RegistrarAchadoCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarAchadoCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarAchadoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.TesteControleId.HasValue)
            {
                var testeExiste = await _context.TestesControle.AnyAsync(t => t.Id == request.TesteControleId.Value, cancellationToken);
                if (!testeExiste)
                {
                    return CommandResult.Falha("Teste de controle de origem nao encontrado.");
                }
            }

            var achado = new Achado(request.TesteControleId, request.Titulo, request.Descricao, request.Severidade, request.PrazoRemediacao, tenantId, usuario);
            if (!achado.IsValid)
            {
                return CommandResult.Falha(achado.Notifications.Select(n => n.Message));
            }

            _context.Achados.Add(achado);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Achado de auditoria registrado com sucesso!", new { AchadoId = achado.Id, achado.Severidade, achado.Status });
        }
    }
}
