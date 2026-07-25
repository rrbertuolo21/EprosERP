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
    public class AvaliarRiscoCommandHandler : ICommandHandler<AvaliarRiscoCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AvaliarRiscoCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AvaliarRiscoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var riscoExiste = await _context.RiscosCorporativos.AnyAsync(r => r.Id == request.RiscoId, cancellationToken);
            if (!riscoExiste)
            {
                return CommandResult.Falha("Risco corporativo nao encontrado.");
            }

            var avaliacao = new AvaliacaoRisco(request.RiscoId, request.Probabilidade, request.Impacto, request.Justificativa, tenantId, usuario);
            if (!avaliacao.IsValid)
            {
                return CommandResult.Falha(avaliacao.Notifications.Select(n => n.Message));
            }

            _context.AvaliacoesRisco.Add(avaliacao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Avaliacao de risco registrada com sucesso!", new { AvaliacaoId = avaliacao.Id, avaliacao.Score });
        }
    }

    public class CriarPlanoAcaoRiscoCommandHandler : ICommandHandler<CriarPlanoAcaoRiscoCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPlanoAcaoRiscoCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPlanoAcaoRiscoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var riscoExiste = await _context.RiscosCorporativos.AnyAsync(r => r.Id == request.RiscoId, cancellationToken);
            if (!riscoExiste)
            {
                return CommandResult.Falha("Risco corporativo nao encontrado.");
            }

            var plano = new PlanoAcaoRisco(request.RiscoId, request.Descricao, request.ResponsavelId, request.Prazo, tenantId, usuario);
            if (!plano.IsValid)
            {
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message));
            }

            _context.PlanosAcaoRisco.Add(plano);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Plano de acao do risco criado com sucesso!", new { PlanoAcaoId = plano.Id, plano.Status });
        }
    }

    public class VincularControleMitigadorCommandHandler : ICommandHandler<VincularControleMitigadorCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public VincularControleMitigadorCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(VincularControleMitigadorCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var riscoExiste = await _context.RiscosCorporativos.AnyAsync(r => r.Id == request.RiscoId, cancellationToken);
            if (!riscoExiste)
            {
                return CommandResult.Falha("Risco corporativo nao encontrado.");
            }

            var controleExiste = await _context.ControlesInternos.AnyAsync(c => c.Id == request.ControleId, cancellationToken);
            if (!controleExiste)
            {
                return CommandResult.Falha("Controle interno nao encontrado.");
            }

            // Unicidade: mesmo controle nao pode ser vinculado duas vezes ao mesmo risco.
            var jaVinculado = await _context.ControlesMitigadores
                .AnyAsync(cm => cm.RiscoId == request.RiscoId && cm.ControleId == request.ControleId, cancellationToken);
            if (jaVinculado)
            {
                return CommandResult.Falha("Este controle ja esta vinculado a este risco.");
            }

            var vinculo = new ControleMitigador(request.RiscoId, request.ControleId, request.Efetividade, request.Observacao, tenantId, usuario);
            if (!vinculo.IsValid)
            {
                return CommandResult.Falha(vinculo.Notifications.Select(n => n.Message));
            }

            _context.ControlesMitigadores.Add(vinculo);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Controle mitigador vinculado ao risco com sucesso!", new { VinculoId = vinculo.Id });
        }
    }
}
