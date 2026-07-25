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
    /// <summary>Cria Municipio.</summary>
    public class CriarMunicipioCommandHandler : ICommandHandler<CriarMunicipioCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public CriarMunicipioCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarMunicipioCommand request, CancellationToken cancellationToken)
        {
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var existeIbge = await _context.Municipios.AnyAsync(m => m.CodigoIbge == request.CodigoIbge, cancellationToken);
            if (existeIbge)
                return CommandResult.Falha(new[] { "Código IBGE já cadastrado." }, "Erro de validação");

            var municipio = new Municipio(
                request.PaisId,
                request.SubdivisaoId,
                request.Nome,
                request.CodigoIbge,
                request.Latitude,
                request.Longitude,
                criadoPor
            );

            if (!municipio.IsValid)
            {
                var erros = municipio.Notifications.Select(n => n.Message).Distinct();
                return CommandResult.Falha(erros, "Invariantes de domínio do Município não foram atendidas.");
            }

            _context.Municipios.Add(municipio);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Município cadastrado com sucesso!", new { MunicipioId = municipio.Id });
        }
    }
}
