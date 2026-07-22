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
    /// <summary>Cria Execucao Massa.</summary>
    public class CriarExecucaoMassaCommandHandler : ICommandHandler<CriarExecucaoMassaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarExecucaoMassaCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarExecucaoMassaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var criadoPor = _currentUser.GetUserId() ?? "system";

            var execucao = new ExecucaoMassa(
                request.TipoOperacao,
                request.Parametros,
                "system",
                criadoPor
            );

            if (!execucao.IsValid)
            {
                var erros = execucao.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Invariantes de domínio da execução em lote não foram atendidas.");
            }

            _context.ExecucoesMassa.Add(execucao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Solicitação de execução em massa criada com sucesso. Aguardando aprovação checker.", new { ExecucaoMassaId = execucao.Id });
        }
    }
}
