using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    public class CriarServicoCommandHandler : ICommandHandler<CriarServicoCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarServicoCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarServicoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var servico = new Servico(
                request.EmpresaId,
                request.UnidadeMedidaId,
                request.CodigoServicoSefazId,
                request.Codigo,
                request.Descricao,
                request.Valor,
                request.InformacaoAdicional,
                request.ServicoAtivo,
                request.Cnae,
                request.CodigoNbs,
                request.IndicadorIss,
                request.IndicadorIncentivo,
                request.CstIbsCbs,
                request.CClassTrib,
                request.AliquotaIss,
                request.AliquotaIssRetido,
                request.AliquotaIrrfRetido,
                request.AliquotaInss,
                request.CstPisCofins,
                request.AliquotaPis,
                request.AliquotaCofins,
                request.CalcularRetencao,
                request.AnexoSimplesNacional,
                tenantId,
                usuario
            );

            if (!servico.IsValid)
            {
                return CommandResult.Falha(servico.Notifications.Select(n => n.Message), "Erro de validação do Serviço.");
            }

            _context.Servicos.Add(servico);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Serviço criado com sucesso!", new { Id = servico.Id });
        }
    }
}
