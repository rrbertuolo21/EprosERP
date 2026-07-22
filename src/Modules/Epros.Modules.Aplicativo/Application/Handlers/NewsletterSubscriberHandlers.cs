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
    public class InscreverNewsletterCommandHandler : ICommandHandler<InscreverNewsletterCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public InscreverNewsletterCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(InscreverNewsletterCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var criadoPor = _currentUser.GetUserId() ?? "system";

            // Verifica se o e-mail já está inscrito
            var inscritoExistente = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(n => n.Email == request.Email && n.DeletadoEm == null, cancellationToken);

            if (inscritoExistente != null)
            {
                if (!inscritoExistente.Ativo)
                {
                    // Se existia mas estava inativo, reativa
                    inscritoExistente.ReativarInscricao(criadoPor);
                    await _context.SaveChangesAsync(cancellationToken);
                    return CommandResult.Ok("Inscrição de newsletter reativada com sucesso!");
                }

                return CommandResult.Ok("E-mail já está inscrito na newsletter.");
            }

            var novoSub = new NewsletterSubscriber(
                email: request.Email,
                consentimentoLGPD: request.ConsentimentoLGPD,
                termosVersao: request.TermosVersao,
                ipRegistro: request.IpRegistro,
                tenantId: "system",
                criadoPor: criadoPor
            );

            if (!novoSub.IsValid)
            {
                var erros = novoSub.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao validar a inscrição de newsletter.");
            }

            _context.NewsletterSubscribers.Add(novoSub);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Inscrição realizada com sucesso na newsletter!", new { NewsletterSubscriberId = novoSub.Id });
        }
    }

    public class CancelarNewsletterCommandHandler : ICommandHandler<CancelarNewsletterCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CancelarNewsletterCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CancelarNewsletterCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(n => n.Id == request.NewsletterSubscriberId && n.DeletadoEm == null, cancellationToken);

            if (subscriber == null)
            {
                return CommandResult.Falha(new[] { "Assinante de newsletter não encontrado." });
            }

            subscriber.CancelarInscricao(alteradoPor);

            if (!subscriber.IsValid)
            {
                var erros = subscriber.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao processar o cancelamento da inscrição.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Inscrição cancelada com sucesso (Opt-Out).");
        }
    }

    public class ReativarNewsletterCommandHandler : ICommandHandler<ReativarNewsletterCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ReativarNewsletterCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ReativarNewsletterCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(n => n.Id == request.NewsletterSubscriberId && n.DeletadoEm == null, cancellationToken);

            if (subscriber == null)
            {
                return CommandResult.Falha(new[] { "Assinante de newsletter não encontrado." });
            }

            subscriber.ReativarInscricao(alteradoPor);

            if (!subscriber.IsValid)
            {
                var erros = subscriber.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao processar a reativação da inscrição.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Inscrição reativada com sucesso (Opt-In).");
        }
    }

    public class CancelarNewsletterPorTokenCommandHandler : ICommandHandler<CancelarNewsletterPorTokenCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;

        public CancelarNewsletterPorTokenCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(CancelarNewsletterPorTokenCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(n => n.TokenDescadastro == request.TokenDescadastro && n.DeletadoEm == null, cancellationToken);

            if (subscriber == null)
            {
                return CommandResult.Falha(new[] { "Assinante de newsletter não encontrado com o token fornecido." });
            }

            subscriber.CancelarInscricao("opt-out-token");

            if (!subscriber.IsValid)
            {
                var erros = subscriber.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao processar o cancelamento da inscrição via token.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Inscrição cancelada com sucesso (Opt-Out).");
        }
    }
}
