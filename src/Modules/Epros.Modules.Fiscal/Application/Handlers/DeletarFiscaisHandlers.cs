using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    /// <summary>Exclui (soft-delete) um CFOP. Fiel ao DELETE do legado <c>CfopController</c>.</summary>
    public class DeletarCfopCommandHandler : ICommandHandler<DeletarCfopCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public DeletarCfopCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarCfopCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var cfop = await _context.Cfops.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (cfop == null)
                return CommandResult.Falha("CFOP não localizado.");

            cfop.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("CFOP deletado com sucesso!");
        }
    }

    /// <summary>Exclui (soft-delete) um Tipo de Operação Fiscal. Fiel ao DELETE do legado <c>TipoOperacaoFiscalController</c>.</summary>
    public class DeletarTipoOperacaoFiscalCommandHandler : ICommandHandler<DeletarTipoOperacaoFiscalCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public DeletarTipoOperacaoFiscalCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarTipoOperacaoFiscalCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var operacao = await _context.TiposOperacoesFiscais.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
            if (operacao == null)
                return CommandResult.Falha("Tipo de operação fiscal não localizado.");

            operacao.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Tipo de operação fiscal deletado com sucesso!");
        }
    }

    /// <summary>Exclui (soft-delete) um Código de Benefício Fiscal. Fiel ao DELETE do legado <c>CodigoBeneficioFiscalController</c>.</summary>
    public class DeletarCodigoBeneficioFiscalCommandHandler : ICommandHandler<DeletarCodigoBeneficioFiscalCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public DeletarCodigoBeneficioFiscalCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarCodigoBeneficioFiscalCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var beneficio = await _context.CodigosBeneficiosFiscais.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
            if (beneficio == null)
                return CommandResult.Falha("Código de benefício fiscal não localizado.");

            beneficio.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Código de benefício fiscal deletado com sucesso!");
        }
    }
}
