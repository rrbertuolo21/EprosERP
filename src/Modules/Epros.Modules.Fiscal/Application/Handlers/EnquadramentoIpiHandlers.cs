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
    public class CriarEnquadramentoIpiCommandHandler : ICommandHandler<CriarEnquadramentoIpiCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEnquadramentoIpiCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarEnquadramentoIpiCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var enq = new EnquadramentoIpi(request.Codigo, request.Descricao, request.TipoOperacao, usuario);
            if (!enq.IsValid)
                return CommandResult.Falha(enq.Notifications.Select(n => n.Message), "Erro de validação do Enquadramento IPI.");

            _context.EnquadramentosIpi.Add(enq);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Enquadramento IPI criado com sucesso!", new { enq.Id });
        }
    }

    public class AtualizarEnquadramentoIpiCommandHandler : ICommandHandler<AtualizarEnquadramentoIpiCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarEnquadramentoIpiCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarEnquadramentoIpiCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var enq = await _context.EnquadramentosIpi.FirstOrDefaultAsync(e => e.Id == request.Id && e.DeletadoEm == null, cancellationToken);
            if (enq == null)
                return CommandResult.Falha("Enquadramento IPI não localizado.");

            enq.Alterar(request.Codigo, request.Descricao, request.TipoOperacao, usuario);
            if (!enq.IsValid)
                return CommandResult.Falha(enq.Notifications.Select(n => n.Message), "Erro de validação do Enquadramento IPI.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Enquadramento IPI atualizado com sucesso!", new { enq.Id });
        }
    }

    public class DeletarEnquadramentoIpiCommandHandler : ICommandHandler<DeletarEnquadramentoIpiCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarEnquadramentoIpiCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarEnquadramentoIpiCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var enq = await _context.EnquadramentosIpi.FirstOrDefaultAsync(e => e.Id == request.Id && e.DeletadoEm == null, cancellationToken);
            if (enq == null)
                return CommandResult.Falha("Enquadramento IPI não encontrado.");

            enq.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Enquadramento IPI deletado com sucesso!");
        }
    }
}
