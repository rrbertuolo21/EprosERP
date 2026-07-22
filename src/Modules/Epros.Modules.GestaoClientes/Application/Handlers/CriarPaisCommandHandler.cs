using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Cria Pais.</summary>
    public class CriarPaisCommandHandler : ICommandHandler<CriarPaisCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public CriarPaisCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPaisCommand request, CancellationToken cancellationToken)
        {
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var existeIso2 = await _context.Paises.AnyAsync(p => p.CodigoIsoAlpha2 == request.CodigoIsoAlpha2.ToUpper(), cancellationToken);
            if (existeIso2)
                return CommandResult.Falha(new[] { "Código ISO Alpha-2 já cadastrado." }, "Erro de validação");

            var existeIso3 = await _context.Paises.AnyAsync(p => p.CodigoIsoAlpha3 == request.CodigoIsoAlpha3.ToUpper(), cancellationToken);
            if (existeIso3)
                return CommandResult.Falha(new[] { "Código ISO Alpha-3 já cadastrado." }, "Erro de validação");

            var pais = new Pais(
                request.Nome,
                request.CodigoIsoAlpha2,
                request.CodigoIsoAlpha3,
                request.CodigoNumerico,
                request.Capital,
                request.CodigoDiscagem,
                criadoPor
            );

            if (!pais.IsValid)
            {
                var erros = pais.Notifications.Select(n => n.Message).Distinct();
                return CommandResult.Falha(erros, "Invariantes de domínio do País não foram atendidas.");
            }

            _context.Paises.Add(pais);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("País cadastrado com sucesso!", new { PaisId = pais.Id });
        }
    }
}
