using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class SimularExecucaoMassaGlobalCommandHandler : ICommandHandler<SimularExecucaoMassaGlobalCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IMediator _mediator;

        public SimularExecucaoMassaGlobalCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IMediator mediator)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _mediator = mediator;
        }

        public async Task<CommandResult> Handle(SimularExecucaoMassaGlobalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var execucao = await _context.ExecucoesMassaGlobal
                .FirstOrDefaultAsync(e => e.Id == request.ExecucaoMassaGlobalId && e.DeletadoEm == null, cancellationToken);

            if (execucao == null)
            {
                return CommandResult.Falha(new[] { "Execução em massa não encontrada." });
            }

            var tipoComando = Type.GetType(execucao.CommandType);
            if (tipoComando == null)
            {
                return CommandResult.Falha(new[] { "Não foi possível carregar o tipo do comando original." });
            }

            // Injeta a flag Simular = true no JSON para a desserialização
            var jsonModificado = execucao.ActionPayload
                .Replace("\"Simular\":false", "\"Simular\":true")
                .Replace("\"Simular\": false", "\"Simular\": true");

            object? comandoInstancia;
            try
            {
                comandoInstancia = JsonSerializer.Deserialize(jsonModificado, tipoComando);
            }
            catch (Exception ex)
            {
                return CommandResult.Falha(new[] { $"Falha ao desserializar o payload do comando: {ex.Message}" });
            }

            if (comandoInstancia == null)
            {
                return CommandResult.Falha(new[] { "O comando desserializado é nulo." });
            }

            // Força via reflexão a flag Simular = true e Aprovado = false caso o JSON replace falhe por formatação
            var backingFieldSimular = tipoComando.GetField("<Simular>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            backingFieldSimular?.SetValue(comandoInstancia, true);

            var backingFieldAprovado = tipoComando.GetField("<Aprovado>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            backingFieldAprovado?.SetValue(comandoInstancia, false);

            try
            {
                // Dispara o comando original via MediatR dinamicamente
                var sendMethod = _mediator.GetType().GetMethods()
                    .First(m => m.Name == "Send" && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(object));
                
                var taskResult = (Task<object>)sendMethod.Invoke(_mediator, new[] { comandoInstancia, cancellationToken })!;
                var resultObj = await taskResult;
                var result = (CommandResult)resultObj;

                string logSimulacao = $"[SIMULAÇÃO EM SANDBOX COMPLETA COM ROLLBACK]\n";
                if (result.Sucesso)
                {
                    logSimulacao += $"Status: Sucesso\n";
                    if (result.Dados != null)
                    {
                        var dadosJson = JsonSerializer.Serialize(result.Dados);
                        logSimulacao += $"Retorno: {dadosJson}\n";
                    }
                    logSimulacao += $"Mensagem: {result.Mensagem}";

                    execucao.RegistrarSimulacao(logSimulacao, alteradoPor);
                }
                else
                {
                    logSimulacao += $"Status: Falha\n";
                    logSimulacao += $"Erros: {string.Join(", ", result.Erros)}\n";
                    logSimulacao += $"Mensagem: {result.Mensagem}";

                    execucao.RegistrarFalha(logSimulacao, alteradoPor);
                }
            }
            catch (Exception ex)
            {
                string logErro = $"[SIMULAÇÃO EM SANDBOX FALHOU COM ERRO]\n";
                logErro += $"Exceção: {ex.GetType().Name}\n";
                logErro += $"Erro: {ex.Message}\n";
                logErro += $"Stack: {ex.StackTrace}";

                execucao.RegistrarFalha(logErro, alteradoPor);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Simulação de sandbox concluída!", new { ExecucaoStatus = execucao.Status, Log = execucao.ResultadoLog });
        }
    }
}
