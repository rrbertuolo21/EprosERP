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
    public class CriarPlanoGarantiaCommandHandler : ICommandHandler<CriarPlanoGarantiaCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPlanoGarantiaCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPlanoGarantiaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var plano = new PlanoGarantia(
                request.Codigo,
                request.Nome,
                request.Descricao,
                request.Duracao,
                request.DuracaoTipo,
                tenantId,
                usuario
            );

            if (!plano.IsValid)
            {
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message));
            }

            _context.PlanosGarantia.Add(plano);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Plano de garantia cadastrado com sucesso!", new { plano.Id });
        }
    }

    public class CriarVeiculoGarantiaCommandHandler : ICommandHandler<CriarVeiculoGarantiaCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarVeiculoGarantiaCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarVeiculoGarantiaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var veiculoGarantia = new VeiculoGarantia(
                request.VeiculoId,
                request.VendaId,
                request.ChassiVin,
                request.PlanoVersaoId,
                request.DataEntrega,
                request.InicioVigencia,
                request.FimVigencia,
                request.QuilometragemInicio,
                request.QuilometragemLimite,
                tenantId,
                usuario
            );

            if (!veiculoGarantia.IsValid)
            {
                return CommandResult.Falha(veiculoGarantia.Notifications.Select(n => n.Message));
            }

            _context.VeiculosGarantia.Add(veiculoGarantia);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Garantia do veículo registrada com sucesso!", new { veiculoGarantia.Id });
        }
    }

    public class CriarSolicitacaoGarantiaCommandHandler : ICommandHandler<CriarSolicitacaoGarantiaCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarSolicitacaoGarantiaCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarSolicitacaoGarantiaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var solicitacao = new SolicitacaoGarantia(
                request.VeiculoGarantiaId,
                request.Protocolo,
                request.DataOcorrencia,
                request.Quilometragem,
                request.Sintoma,
                request.RelatoCliente,
                request.OrdemServicoId,
                tenantId,
                usuario
            );

            if (!solicitacao.IsValid)
            {
                return CommandResult.Falha(solicitacao.Notifications.Select(n => n.Message));
            }

            _context.SolicitacoesGarantia.Add(solicitacao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Solicitação de garantia registrada com sucesso!", new { solicitacao.Id });
        }
    }
}
