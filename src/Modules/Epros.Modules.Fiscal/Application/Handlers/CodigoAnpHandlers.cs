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
    public class CriarCodigoAnpCommandHandler : ICommandHandler<CriarCodigoAnpCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCodigoAnpCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarCodigoAnpCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var anp = new CodigoAnp(request.Codigo, request.Descricao, request.DataInicioVigencia, request.DataFinalVigencia, usuario);
            if (!anp.IsValid)
                return CommandResult.Falha(anp.Notifications.Select(n => n.Message), "Erro de validação do Código ANP.");

            _context.CodigosAnp.Add(anp);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Código ANP criado com sucesso!", new { anp.Id });
        }
    }

    public class AtualizarCodigoAnpCommandHandler : ICommandHandler<AtualizarCodigoAnpCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarCodigoAnpCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarCodigoAnpCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var anp = await _context.CodigosAnp.FirstOrDefaultAsync(a => a.Id == request.Id && a.DeletadoEm == null, cancellationToken);
            if (anp == null)
                return CommandResult.Falha("Código ANP não localizado.");

            anp.Alterar(request.Codigo, request.Descricao, request.DataInicioVigencia, request.DataFinalVigencia, usuario);
            if (!anp.IsValid)
                return CommandResult.Falha(anp.Notifications.Select(n => n.Message), "Erro de validação do Código ANP.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Código ANP atualizado com sucesso!", new { anp.Id });
        }
    }

    public class DeletarCodigoAnpCommandHandler : ICommandHandler<DeletarCodigoAnpCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarCodigoAnpCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarCodigoAnpCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var anp = await _context.CodigosAnp.FirstOrDefaultAsync(a => a.Id == request.Id && a.DeletadoEm == null, cancellationToken);
            if (anp == null)
                return CommandResult.Falha("Código ANP não encontrado.");

            anp.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Código ANP deletado com sucesso!");
        }
    }
}
