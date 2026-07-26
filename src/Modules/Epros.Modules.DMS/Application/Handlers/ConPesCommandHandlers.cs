using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Domain.Entities;
using Epros.Modules.DMS.Infrastructure.Data;

namespace Epros.Modules.DMS.Application.Handlers
{
    public class CriarPecaReposicaoCommandHandler : ICommandHandler<CriarPecaReposicaoCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPecaReposicaoCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPecaReposicaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var peca = new PecaReposicao(
                request.ProdutoId,
                request.FamiliaTecnica,
                request.Criticidade,
                tenantId,
                usuario
            );

            if (!peca.IsValid)
            {
                return CommandResult.Falha(peca.Notifications.Select(n => n.Message));
            }

            _context.PecasReposicao.Add(peca);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Peça de reposição cadastrada com sucesso!", new { peca.Id });
        }
    }

    public class CriarDemandaPecaCommandHandler : ICommandHandler<CriarDemandaPecaCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarDemandaPecaCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarDemandaPecaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var demanda = new DemandaPeca(
                request.PecaId,
                request.LocalId,
                request.OrigemTipo,
                request.OrigemId,
                request.ItemOrigemId,
                request.Quantidade,
                request.Prazo,
                tenantId,
                usuario
            );

            if (!demanda.IsValid)
            {
                return CommandResult.Falha(demanda.Notifications.Select(n => n.Message));
            }

            _context.DemandasPeca.Add(demanda);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Demanda de peça registrada com sucesso!", new { demanda.Id });
        }
    }

    public class CriarReservaPecaCommandHandler : ICommandHandler<CriarReservaPecaCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarReservaPecaCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarReservaPecaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var reserva = new ReservaPeca(
                request.DemandaId,
                request.PecaId,
                request.LocalId,
                request.QuantidadeReservada,
                tenantId,
                usuario
            );

            if (!reserva.IsValid)
            {
                return CommandResult.Falha(reserva.Notifications.Select(n => n.Message));
            }

            _context.ReservasPeca.Add(reserva);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Reserva de peça registrada com sucesso!", new { reserva.Id });
        }
    }
}
