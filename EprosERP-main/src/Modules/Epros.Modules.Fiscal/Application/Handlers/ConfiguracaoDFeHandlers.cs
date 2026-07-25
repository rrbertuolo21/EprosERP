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
    public class CriarConfiguracaoDFeCommandHandler : ICommandHandler<CriarConfiguracaoDFeCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarConfiguracaoDFeCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarConfiguracaoDFeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var config = new ConfiguracaoDFe(
                request.EmpresaId,
                request.NFeSerieProducao, request.NFeUltimoNrProducao, request.NFeSerieHomologacao, request.NFeUltimoNrHomologacao,
                request.NfceCscProducao, request.NfceIdCscProducao, request.NfceSerieProducao, request.NfceUltimoNrProducao,
                request.NfceCscHomologacao, request.NfceIdCscHomologacao, request.NfceSerieHomologacao, request.NfceUltimoNrHomologacao,
                tenantId, usuario);

            if (!config.IsValid)
                return CommandResult.Falha(config.Notifications.Select(n => n.Message), "Erro de validação da Configuração DF-e.");

            _context.ConfiguracoesDFe.Add(config);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configuração DF-e criada com sucesso!", new { config.Id });
        }
    }

    public class AtualizarConfiguracaoDFeCommandHandler : ICommandHandler<AtualizarConfiguracaoDFeCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarConfiguracaoDFeCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarConfiguracaoDFeCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var config = await _context.ConfiguracoesDFe.FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletadoEm == null, cancellationToken);
            if (config == null)
                return CommandResult.Falha("Configuração DF-e não localizada.");

            config.Alterar(
                request.NFeSerieProducao, request.NFeUltimoNrProducao, request.NFeSerieHomologacao, request.NFeUltimoNrHomologacao,
                request.NfceCscProducao, request.NfceIdCscProducao, request.NfceSerieProducao, request.NfceUltimoNrProducao,
                request.NfceCscHomologacao, request.NfceIdCscHomologacao, request.NfceSerieHomologacao, request.NfceUltimoNrHomologacao,
                usuario);

            if (!config.IsValid)
                return CommandResult.Falha(config.Notifications.Select(n => n.Message), "Erro de validação da Configuração DF-e.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configuração DF-e atualizada com sucesso!", new { config.Id });
        }
    }

    public class DeletarConfiguracaoDFeCommandHandler : ICommandHandler<DeletarConfiguracaoDFeCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarConfiguracaoDFeCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarConfiguracaoDFeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var config = await _context.ConfiguracoesDFe.FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == tenantId && c.DeletadoEm == null, cancellationToken);
            if (config == null)
                return CommandResult.Falha("Configuração DF-e não encontrada.");

            config.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configuração DF-e deletada com sucesso!");
        }
    }
}
