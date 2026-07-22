using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Cria Plano.</summary>
    public class CriarPlanoCommandHandler : ICommandHandler<CriarPlanoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPlanoCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPlanoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var plano = new Plano(
                request.Nome,
                request.Preco,
                null, // GrupoPlanoId
                999, // LimiteUsuarios
                99, // LimiteEmpresas
                null, // RecursosInclusos
                tenantId,
                criadoPor
            );

            if (request.Modulos != null)
            {
                foreach (var moduloNome in request.Modulos)
                {
                    plano.AdicionarModulo(moduloNome, criadoPor);
                }
            }

            // Propagar notificações de validação de filhos (ModuloPlano) para o pai (Plano)
            foreach (var modulo in plano.Modulos)
            {
                if (!modulo.IsValid)
                {
                    plano.AddNotifications(modulo.Notifications);
                }
            }

            if (!plano.IsValid)
            {
                var erros = plano.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Invariantes de domínio do plano não foram atendidas.");
            }

            _context.Planos.Add(plano);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Plano cadastrado com sucesso!", new { PlanoId = plano.Id });
        }
    }
}
