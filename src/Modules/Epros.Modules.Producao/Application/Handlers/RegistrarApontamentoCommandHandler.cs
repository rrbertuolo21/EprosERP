using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Handlers
{
    public class RegistrarApontamentoCommandHandler : ICommandHandler<RegistrarApontamentoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ICurrentUser _currentUser;

        public RegistrarApontamentoCommandHandler(
            ContextProducao context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarApontamentoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = await _context.OrdensProducao
                .Include(o => o.Apontamentos)
                .FirstOrDefaultAsync(o => o.Id == request.OrdemProducaoId, cancellationToken);

            if (ordem == null)
            {
                return CommandResult.Falha("Ordem de Produção não encontrada.");
            }

            ordem.RegistrarApontamento(request.QuantidadeApontada, request.QuantidadeRefugada, request.Operador, usuario);

            if (!ordem.IsValid)
            {
                return CommandResult.Falha(ordem.Notifications.Select(n => n.Message));
            }

            var novoApontamento = ordem.Apontamentos.LastOrDefault();
            if (novoApontamento != null)
            {
                _context.Entry(novoApontamento).State = EntityState.Added;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Apontamento registrado com sucesso!", new 
            { 
                OrdemProducaoId = ordem.Id, 
                QuantidadeProduzida = ordem.QuantidadeProduzida, 
                QuantidadeRefugada = ordem.QuantidadeRefugada 
            });
        }
    }
}
