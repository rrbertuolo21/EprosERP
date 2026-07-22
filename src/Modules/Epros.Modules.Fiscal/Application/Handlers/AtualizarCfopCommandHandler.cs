using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    public class DefaultAtualizarCfopCommandHandler : ICommandHandler<AtualizarCfopCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public DefaultAtualizarCfopCommandHandler(
            ContextFiscal context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarCfopCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var cfop = await _context.Cfops.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (cfop == null)
            {
                return CommandResult.Falha("CFOP não localizado.");
            }

            cfop.Alterar(
                request.CfopCorrelacao,
                request.Descricao,
                request.NaturezaOperacao,
                request.IntegraFaturamento,
                request.IndicadorNfe,
                request.IndicadorComunicacao,
                request.IndicadorTransporte,
                request.IndicadorDevolucao,
                request.IndicadorRetorno,
                request.IndicadorAnulacao,
                request.IndicadorRemessa,
                request.IndicadorCombustivel,
                request.IndicadorTransferencia,
                request.IndicadorNfce,
                request.IndicadorCiap,
                request.IndicadorUsoConsumo,
                request.IndicadorUsoSemOperacao,
                request.IndicadorSt,
                request.IndicadorMei,
                request.IncidenciaSimples,
                request.CfopDevolucao,
                usuario
            );

            if (!cfop.IsValid)
            {
                return CommandResult.Falha(cfop.Notifications.Select(n => n.Message), "Erro de validação de domínio do CFOP.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("CFOP atualizado com sucesso!", new { Id = cfop.Id });
        }
    }
}
