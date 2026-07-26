using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class ExecutarAjustePlanosLoteCommandHandler : ICommandHandler<ExecutarAjustePlanosLoteCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ICurrentUser _currentUser;

        public ExecutarAjustePlanosLoteCommandHandler(ContextAplicativo context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExecutarAjustePlanosLoteCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";
            var settings = await _context.SystemSettings
                .Where(s => s.Escopo == "global" && s.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            int modificados = 0;
            string log = "Iniciando reajuste de configurações globais em lote:\n";

            foreach (var setting in settings)
            {
                if (decimal.TryParse(setting.Valor, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var valorDecimal))
                {
                    var novoValor = valorDecimal * (1 + (request.PercentualAjuste / 100));
                    var novoValorStr = novoValor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                    log += $"- Setting {setting.Chave}: alterado de {setting.Valor} para {novoValorStr}\n";
                    setting.AtualizarValor(novoValorStr, alteradoPor);
                    modificados++;
                }
                else
                {
                    log += $"- Setting {setting.Chave}: valor '{setting.Valor}' não é numérico, ignorado.\n";
                }
            }

            log += $"Total de configurações ajustadas: {modificados}.";

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Reajuste concluído com sucesso!", new { Log = log, ModificadosCount = modificados });
        }
    }
}
