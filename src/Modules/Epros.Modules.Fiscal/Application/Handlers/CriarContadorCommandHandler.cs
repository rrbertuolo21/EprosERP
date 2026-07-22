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
    public class CriarContadorCommandHandler : ICommandHandler<CriarContadorCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarContadorCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarContadorCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var contador = new Contador(
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
                tenantId,
                usuario
            );

            if (!contador.IsValid)
            {
                return CommandResult.Falha(contador.Notifications.Select(n => n.Message), "Erro de validação do Contador.");
            }

            _context.Contadores.Add(contador);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Contador criado com sucesso!", new { Id = contador.Id });
        }
    }
}
