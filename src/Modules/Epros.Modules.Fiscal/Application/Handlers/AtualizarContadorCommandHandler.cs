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
    public class AtualizarContadorCommandHandler : ICommandHandler<AtualizarContadorCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarContadorCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarContadorCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var contador = await _context.Contadores
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == tenantId && c.DeletadoEm == null, cancellationToken);

            if (contador == null)
            {
                return CommandResult.Falha("Contador não encontrado.");
            }

            contador.Alterar(
                request.RazaoSocial,
                request.NomeContador,
                request.Cpf,
                request.Cnpj,
                request.NumeroCrc,
                request.UfCrc,
                request.DataVencimentoCrc,
                request.Qualificacao,
                request.Funcao,
                request.Telefone,
                request.Email,
                request.PermissaoTransmissao,
                request.Ativo,
                request.Logradouro,
                request.Numero,
                request.Complemento,
                request.Bairro,
                request.Cep,
                request.MunicipioId,
                request.Uf,
                usuario
            );

            if (!contador.IsValid)
            {
                return CommandResult.Falha(contador.Notifications.Select(n => n.Message), "Erro de validação ao atualizar Contador.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Contador atualizado com sucesso!");
        }
    }
}
