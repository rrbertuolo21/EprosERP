using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    /// <summary>
    /// Handler responsável por atualizar (ou criar) as configurações globais da Landing Page.
    /// Operação restrita ao tenant do sistema ("system" / Siser).
    /// </summary>
    public class AtualizarLandingPageSettingsCommandHandler : ICommandHandler<AtualizarLandingPageSettingsCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        /// <summary>Inicializa o handler com o contexto de dados, o provedor de tenant e o usuário atual.</summary>
        public AtualizarLandingPageSettingsCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Atualiza o registro único de Landing Page se existir; caso contrário, cria um novo.
        /// Retorna falha se o tenant corrente não for o tenant do sistema.
        /// </summary>
        public async Task<CommandResult> Handle(AtualizarLandingPageSettingsCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var landingExistente = await _context.LandingPagesSettings.FirstOrDefaultAsync(cancellationToken);

            if (landingExistente != null)
            {
                landingExistente.AtualizarSettings(request.SettingsJson, alteradoPor);
            }
            else
            {
                var novaLanding = new LandingPageSettings(request.SettingsJson, "system", alteradoPor);
                _context.LandingPagesSettings.Add(novaLanding);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Configurações da Landing Page atualizadas com sucesso!");
        }
    }

    /// <summary>
    /// Handler responsável por atualizar (ou criar) as configurações de Marketplace por módulo.
    /// Operação restrita ao tenant do sistema ("system" / Siser).
    /// </summary>
    public class AtualizarMarketplaceSettingsCommandHandler : ICommandHandler<AtualizarMarketplaceSettingsCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        /// <summary>Inicializa o handler com o contexto de dados, o provedor de tenant e o usuário atual.</summary>
        public AtualizarMarketplaceSettingsCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Atualiza as configurações de Marketplace do módulo informado se existirem; caso contrário, cria um novo registro.
        /// Retorna falha se o tenant corrente não for o tenant do sistema.
        /// </summary>
        public async Task<CommandResult> Handle(AtualizarMarketplaceSettingsCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var marketExistente = await _context.MarketplacesSettings
                .FirstOrDefaultAsync(m => m.Modulo == request.Modulo && m.DeletadoEm == null, cancellationToken);

            if (marketExistente != null)
            {
                marketExistente.AtualizarSettings(request.SettingsJson, alteradoPor);
            }
            else
            {
                var novoMarket = new MarketplaceSettings(request.Modulo, request.SettingsJson, "system", alteradoPor);
                _context.MarketplacesSettings.Add(novoMarket);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Configurações do Marketplace atualizadas com sucesso!");
        }
    }
}
