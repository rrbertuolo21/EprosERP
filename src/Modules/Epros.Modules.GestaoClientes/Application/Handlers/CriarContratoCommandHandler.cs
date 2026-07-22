using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Cria Contrato.</summary>
    public class CriarContratoCommandHandler : ICommandHandler<CriarContratoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarContratoCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarContratoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            // Verifica se o cliente existe e pertence ao tenant
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == request.ClienteId && c.TenantId == tenantId && c.DeletadoEm == null, cancellationToken);

            if (cliente == null)
            {
                return CommandResult.Falha(new[] { "Cliente não encontrado ou inativo no tenant atual." });
            }

            // Instancia a entidade rica com validações internas de domínio (Flunt)
            var contrato = new Contrato(
                request.ClienteId,
                request.DiaVencimento,
                request.DataInicio,
                request.DataFim,
                tenantId,
                criadoPor
            );

            // Adiciona os itens ao contrato
            foreach (var item in request.Itens)
            {
                contrato.AdicionarItem(item.Descricao, item.Quantidade, item.ValorUnitario, criadoPor);
            }

            // Valida as invariantes do domínio (Flunt)
            if (!contrato.IsValid)
            {
                var erros = contrato.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Invariantes de domínio do contrato não foram atendidas.");
            }

            // Persiste no banco usando o ContextGestaoClientes
            _context.Contratos.Add(contrato);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Contrato cadastrado com sucesso!", new { ContratoId = contrato.Id, ValorRecorrente = contrato.ValorRecorrente });
        }
    }
}
