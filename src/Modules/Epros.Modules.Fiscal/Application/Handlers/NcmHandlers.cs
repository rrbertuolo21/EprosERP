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
    public class CriarNcmCommandHandler : ICommandHandler<CriarNcmCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarNcmCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarNcmCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var ncm = new Ncm(request.CodigoNcm, request.Descricao, request.DataInicio, request.DataFim,
                request.TipoAtoIni, request.NumeroAtoIni, request.AnoAtoIni, usuario);

            if (!ncm.IsValid)
                return CommandResult.Falha(ncm.Notifications.Select(n => n.Message), "Erro de validação do NCM.");

            _context.Ncms.Add(ncm);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("NCM criado com sucesso!", new { ncm.Id });
        }
    }

    public class AtualizarNcmCommandHandler : ICommandHandler<AtualizarNcmCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarNcmCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarNcmCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var ncm = await _context.Ncms.FirstOrDefaultAsync(n => n.Id == request.Id && n.DeletadoEm == null, cancellationToken);
            if (ncm == null)
                return CommandResult.Falha("NCM não localizado.");

            ncm.Alterar(request.CodigoNcm, request.Descricao, request.DataInicio, request.DataFim,
                request.TipoAtoIni, request.NumeroAtoIni, request.AnoAtoIni, usuario);

            if (!ncm.IsValid)
                return CommandResult.Falha(ncm.Notifications.Select(n => n.Message), "Erro de validação do NCM.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("NCM atualizado com sucesso!", new { ncm.Id });
        }
    }

    public class DeletarNcmCommandHandler : ICommandHandler<DeletarNcmCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarNcmCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarNcmCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var ncm = await _context.Ncms.FirstOrDefaultAsync(n => n.Id == request.Id && n.DeletadoEm == null, cancellationToken);
            if (ncm == null)
                return CommandResult.Falha("NCM não encontrado.");

            ncm.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("NCM deletado com sucesso!");
        }
    }
}
