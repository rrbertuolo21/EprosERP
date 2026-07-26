using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class DefinirSystemSettingCommandHandler : ICommandHandler<DefinirSystemSettingCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DefinirSystemSettingCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirSystemSettingCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var operador = _currentUser.GetUserId() ?? "system";

            var settingExistente = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Chave == request.Chave && s.Escopo == request.Escopo && s.DeletadoEm == null, cancellationToken);

            if (settingExistente != null)
            {
                settingExistente.AtualizarValor(request.Valor, operador);

                if (!settingExistente.IsValid)
                {
                    var erros = settingExistente.Notifications.Select(n => n.Message);
                    return CommandResult.Falha(erros, "Erro ao validar a atualização da configuração.");
                }
            }
            else
            {
                var novoSetting = new SystemSetting(
                    chave: request.Chave,
                    valor: request.Valor,
                    escopo: request.Escopo,
                    ehSegredo: request.IsSecret,
                    tenantId: "system",
                    criadoPor: operador
                );

                if (!novoSetting.IsValid)
                {
                    var erros = novoSetting.Notifications.Select(n => n.Message);
                    return CommandResult.Falha(erros, "Erro ao validar a criação da configuração.");
                }

                _context.SystemSettings.Add(novoSetting);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Configuração definida com sucesso!");
        }
    }
}
