using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Domain.Entities;
using Epros.Modules.Projetos.Infrastructure.Data;

namespace Epros.Modules.Projetos.Application.Handlers
{
    public class CriarProjetoCommandHandler : ICommandHandler<CriarProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarProjetoCommandHandler(
            ContextProjetos context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarProjetoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var projeto = new Projeto(
                request.Nome,
                request.Descricao,
                request.ClienteId,
                request.DataInicio,
                request.DataTermino,
                request.OrcamentoTotal,
                tenantId,
                usuario
            );

            if (!projeto.IsValid)
            {
                return CommandResult.Falha(projeto.Notifications.Select(n => n.Message));
            }

            _context.Projetos.Add(projeto);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Projeto criado com sucesso!", new { ProjetoId = projeto.Id });
        }
    }
}
