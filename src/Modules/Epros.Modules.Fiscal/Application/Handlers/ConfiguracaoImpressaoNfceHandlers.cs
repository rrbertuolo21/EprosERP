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
    public class CriarConfiguracaoImpressaoNfceCommandHandler : ICommandHandler<CriarConfiguracaoImpressaoNfceCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarConfiguracaoImpressaoNfceCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarConfiguracaoImpressaoNfceCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var config = new ConfiguracaoImpressaoNfce(
                request.EmpresaId, request.DetalheVendaNormal, request.DetalheVendaContingencia,
                request.ImprimeDescontoItem, request.ImprimeFoneEmitente, request.MargemEsquerda, request.MargemDireita,
                request.ModoImpressao, request.NfceLayoutQrCode, request.VersaoQrCode, request.SegundaViaContingencia,
                tenantId, usuario);

            if (!config.IsValid)
                return CommandResult.Falha(config.Notifications.Select(n => n.Message), "Erro de validação da Configuração de Impressão NFCe.");

            _context.ConfiguracoesImpressaoNfce.Add(config);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configuração de Impressão NFCe criada com sucesso!", new { config.Id });
        }
    }

    public class AtualizarConfiguracaoImpressaoNfceCommandHandler : ICommandHandler<AtualizarConfiguracaoImpressaoNfceCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarConfiguracaoImpressaoNfceCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarConfiguracaoImpressaoNfceCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var config = await _context.ConfiguracoesImpressaoNfce.FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletadoEm == null, cancellationToken);
            if (config == null)
                return CommandResult.Falha("Configuração de Impressão NFCe não localizada.");

            config.Alterar(
                request.DetalheVendaNormal, request.DetalheVendaContingencia,
                request.ImprimeDescontoItem, request.ImprimeFoneEmitente, request.MargemEsquerda, request.MargemDireita,
                request.ModoImpressao, request.NfceLayoutQrCode, request.VersaoQrCode, request.SegundaViaContingencia, usuario);

            if (!config.IsValid)
                return CommandResult.Falha(config.Notifications.Select(n => n.Message), "Erro de validação da Configuração de Impressão NFCe.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configuração de Impressão NFCe atualizada com sucesso!", new { config.Id });
        }
    }

    public class DeletarConfiguracaoImpressaoNfceCommandHandler : ICommandHandler<DeletarConfiguracaoImpressaoNfceCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarConfiguracaoImpressaoNfceCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarConfiguracaoImpressaoNfceCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var config = await _context.ConfiguracoesImpressaoNfce.FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == tenantId && c.DeletadoEm == null, cancellationToken);
            if (config == null)
                return CommandResult.Falha("Configuração de Impressão NFCe não encontrada.");

            config.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configuração de Impressão NFCe deletada com sucesso!");
        }
    }
}
