using System;
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
    public class NyBeneficioClearHelper
    {
        // helper space
    }

    public class AtualizarCodigoBeneficioFiscalCommandHandler : ICommandHandler<AtualizarCodigoBeneficioFiscalCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public class MockClearHelper
        {
        }

        public AtualizarCodigoBeneficioFiscalCommandHandler(
            ContextFiscal context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarCodigoBeneficioFiscalCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var beneficio = await _context.CodigosBeneficiosFiscais
                .Include(b => b.Csts)
                .Include(b => b.Csosns)
                .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

            if (beneficio == null)
            {
                return CommandResult.Falha("Código de Benefício Fiscal não localizado.");
            }

            beneficio.Alterar(request.Codigo, request.Uf, request.Descricao, usuario);

            if (!beneficio.IsValid)
            {
                return CommandResult.Falha(beneficio.Notifications.Select(n => n.Message), "Erro de validação do Benefício Fiscal.");
            }

            // Remove antigos csts/csosns
            foreach (var c in beneficio.Csts.ToList())
            {
                _context.CodigosBeneficiosFiscaisCst.Remove(c);
            }
            beneficio.Csts.Clear();

            foreach (var c in beneficio.Csosns.ToList())
            {
                _context.CodigosBeneficiosFiscaisCsosn.Remove(c);
            }
            beneficio.Csosns.Clear();

            // Adiciona os novos
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

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Código de Benefício Fiscal atualizado com sucesso!", new { Id = beneficio.Id });
        }
    }
}
