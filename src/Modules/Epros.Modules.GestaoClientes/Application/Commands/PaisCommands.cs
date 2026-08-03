using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record AtualizarPaisCommand(Guid Id, string Nome, string? Capital, string? CodigoDiscagem) : ICommand;
    public record InativarPaisCommand(Guid Id, bool Ativo) : ICommand;

    public class AtualizarPaisCommandHandler : ICommandHandler<AtualizarPaisCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public AtualizarPaisCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarPaisCommand request, CancellationToken cancellationToken)
        {
            var pais = await _context.Paises.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == request.Id && p.DeletadoEm == null, cancellationToken);
            if (pais == null) return CommandResult.Falha(new[] { "País não encontrado." });
            pais.Atualizar(request.Nome, request.Capital, request.CodigoDiscagem, _currentUser.GetUserId() ?? "system");
            if (!pais.IsValid) return CommandResult.Falha(pais.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("País atualizado com sucesso!");
        }
    }

    public class InativarPaisCommandHandler : ICommandHandler<InativarPaisCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public InativarPaisCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(InativarPaisCommand request, CancellationToken cancellationToken)
        {
            var pais = await _context.Paises.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == request.Id && p.DeletadoEm == null, cancellationToken);
            if (pais == null) return CommandResult.Falha(new[] { "País não encontrado." });
            var por = _currentUser.GetUserId() ?? "system";
            if (request.Ativo) pais.Reativar(por); else pais.Inativar(por);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok(request.Ativo ? "País reativado." : "País inativado.");
        }
    }
}
