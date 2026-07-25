using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    public class CriarCodigoBeneficioFiscalCommandHandler : ICommandHandler<CriarCodigoBeneficioFiscalCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCodigoBeneficioFiscalCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCodigoBeneficioFiscalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var beneficio = new CodigoBeneficioFiscal(
                request.Codigo,
                request.Uf,
                request.Descricao,
                tenantId,
                usuario
            );

            if (!beneficio.IsValid)
            {
                return CommandResult.Falha(beneficio.Notifications.Select(n => n.Message), "Erro de validação do Benefício Fiscal.");
            }

            if (request.Csts != null)
            {
                foreach (var cst in request.Csts)
                {
                    beneficio.AdicionarCst(cst, usuario);
                }
            }

            if (request.Csosns != null)
            {
                foreach (var csosn in request.Csosns)
                {
                    beneficio.AdicionarCsosn(csosn, usuario);
                }
            }

            _context.CodigosBeneficiosFiscais.Add(beneficio);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Código de Benefício Fiscal criado com sucesso!", new { Id = beneficio.Id });
        }
    }
}
