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
    public class CriarCustomPageCommandHandler : ICommandHandler<CriarCustomPageCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCustomPageCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCustomPageCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var criadoPor = _currentUser.GetUserId() ?? "system";

            // Verifica slug único
            var slugExiste = await _context.CustomPages
                .AnyAsync(p => p.Slug == request.Slug && p.DeletadoEm == null, cancellationToken);

            if (slugExiste)
            {
                return CommandResult.Falha(new[] { "Já existe uma página cadastrada com este slug." });
            }

            var pagina = new CustomPage(
                slug: request.Slug,
                conteudo: request.Conteudo,
                status: "Rascunho",
                tenantId: "system",
                criadoPor: criadoPor
            );

            if (!pagina.IsValid)
            {
                var erros = pagina.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao validar a criação da página customizada.");
            }

            _context.CustomPages.Add(pagina);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Página customizada criada em rascunho com sucesso!", new { CustomPageId = pagina.Id });
        }
    }

    public class PublicarCustomPageCommandHandler : ICommandHandler<PublicarCustomPageCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public PublicarCustomPageCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(PublicarCustomPageCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var pagina = await _context.CustomPages
                .FirstOrDefaultAsync(p => p.Id == request.CustomPageId && p.DeletadoEm == null, cancellationToken);

            if (pagina == null)
            {
                return CommandResult.Falha(new[] { "Página customizada não encontrada." });
            }

            pagina.Publicar(alteradoPor);

            if (!pagina.IsValid)
            {
                var erros = pagina.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao validar a publicação da página.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Página customizada publicada com sucesso!");
        }
    }

    public class DefinirRascunhoCustomPageCommandHandler : ICommandHandler<DefinirRascunhoCustomPageCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DefinirRascunhoCustomPageCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirRascunhoCustomPageCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var pagina = await _context.CustomPages
                .FirstOrDefaultAsync(p => p.Id == request.CustomPageId && p.DeletadoEm == null, cancellationToken);

            if (pagina == null)
            {
                return CommandResult.Falha(new[] { "Página customizada não encontrada." });
            }

            pagina.DefinirComoRascunho(alteradoPor);

            if (!pagina.IsValid)
            {
                var erros = pagina.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao redefinir a página para rascunho.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Página customizada redefinida para rascunho com sucesso!");
        }
    }
}
