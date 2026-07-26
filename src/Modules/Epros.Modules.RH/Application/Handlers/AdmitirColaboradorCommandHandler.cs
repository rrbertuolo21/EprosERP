using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Domain.Entities;
using Epros.Modules.RH.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.RH.Application.Handlers
{
    public class AdmitirColaboradorCommandHandler : ICommandHandler<AdmitirColaboradorCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdmitirColaboradorCommandHandler(
            ContextRH context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdmitirColaboradorCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // Verificar se o CPF já está cadastrado
            var cpfExiste = await _context.Colaboradores
                .AnyAsync(c => c.Cpf == request.Cpf, cancellationToken);

            if (cpfExiste)
            {
                return CommandResult.Falha("Já existe um colaborador cadastrado com este CPF.");
            }

            var colaborador = new Colaborador(
                request.Nome,
                request.Cpf,
                request.Email,
                request.Cargo,
                request.Departamento,
                request.SalarioBase,
                request.DataAdmissao,
                tenantId,
                usuario
            );

            if (!colaborador.IsValid)
            {
                return CommandResult.Falha(colaborador.Notifications.Select(n => n.Message));
            }

            _context.Colaboradores.Add(colaborador);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Colaborador admitido com sucesso!", new { ColaboradorId = colaborador.Id });
        }
    }
}
