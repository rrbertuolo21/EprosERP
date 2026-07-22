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
    public class AtualizarTipoOperacaoFiscalCommandHandler : ICommandHandler<AtualizarTipoOperacaoFiscalCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarTipoOperacaoFiscalCommandHandler(
            ContextFiscal context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarTipoOperacaoFiscalCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var operacao = await _context.TiposOperacoesFiscais
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (operacao == null)
            {
                return CommandResult.Falha("Operação Fiscal não localizada.");
            }

            operacao.Alterar(
                request.TributarioGrupoId,
                request.CfopNfeId,
                request.CfopNfceId,
                request.Descricao,
                request.SobescreveTributacaoNcm,
                request.Finalidade,
                request.Atendimento,
                request.TipoFrete,
                request.TipoMovimento,
                usuario
            );

            if (!operacao.IsValid)
            {
                return CommandResult.Falha(operacao.Notifications.Select(n => n.Message), "Erro de validação de domínio da Operação Fiscal.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Operação Fiscal atualizada com sucesso!", new { Id = operacao.Id });
        }
    }
}
