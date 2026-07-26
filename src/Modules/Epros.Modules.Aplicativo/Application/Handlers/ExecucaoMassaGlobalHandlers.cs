using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class CriarExecucaoMassaGlobalCommandHandler : ICommandHandler<CriarExecucaoMassaGlobalCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarExecucaoMassaGlobalCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarExecucaoMassaGlobalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var criadoPor = _currentUser.GetUserId() ?? "system";

            var execucao = new ExecucaoMassaGlobal(
                descricao: request.Descricao,
                actionPayload: request.ActionPayload,
                status: "Draft",
                tenantId: "system",
                criadoPor: criadoPor
            );

            if (!execucao.IsValid)
            {
                var erros = execucao.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao validar a criação da execução em massa.");
            }

            _context.ExecucoesMassaGlobal.Add(execucao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Execução em massa criada em rascunho com sucesso!", new { ExecucaoMassaGlobalId = execucao.Id });
        }
    }

    public class AtivarExecucaoMassaGlobalCommandHandler : ICommandHandler<AtivarExecucaoMassaGlobalCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtivarExecucaoMassaGlobalCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtivarExecucaoMassaGlobalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var aprovadorUserId = _currentUser.GetUserId() ?? "system";
            var aprovadorId = Guid.NewGuid(); // Simulado ou buscando perfil correspondente se aplicável

            var execucao = await _context.ExecucoesMassaGlobal
                .FirstOrDefaultAsync(e => e.Id == request.ExecucaoMassaGlobalId && e.DeletadoEm == null, cancellationToken);

            if (execucao == null)
            {
                return CommandResult.Falha(new[] { "Execução em massa não encontrada." });
            }

            // Invoca ativação no domínio com lógica Maker-Checker
            execucao.Ativar(aprovadorId, aprovadorUserId, aprovadorUserId);

            if (!execucao.IsValid)
            {
                var erros = execucao.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro de validação ou Maker-Checker violado.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Execução em massa ativada com sucesso!");
        }
    }

    public class ConcluirExecucaoMassaGlobalCommandHandler : ICommandHandler<ConcluirExecucaoMassaGlobalCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IMediator _mediator;

        public ConcluirExecucaoMassaGlobalCommandHandler(
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

        public async Task<CommandResult> Handle(ConcluirExecucaoMassaGlobalCommand request, CancellationToken cancellationToken)
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

            execucao.Concluir(alteradoPor);

            if (!execucao.IsValid)
            {
                var erros = execucao.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao concluir a execução em massa.");
            }

            // Executa dinamicamente o comando original se houver
            if (!string.IsNullOrEmpty(execucao.CommandType))
            {
                var tipoComando = Type.GetType(execucao.CommandType);
                if (tipoComando == null)
                {
                    return CommandResult.Falha(new[] { "Não foi possível carregar o tipo do comando original." });
                }

                // Injeta a flag Aprovado = true no JSON
                var jsonModificado = execucao.ActionPayload
                    .Replace("\"Aprovado\":false", "\"Aprovado\":true")
                    .Replace("\"Aprovado\": false", "\"Aprovado\": true")
                    .Replace("\"Simular\":true", "\"Simular\":false")
                    .Replace("\"Simular\": true", "\"Simular\": false");

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

                // Força via reflexão as propriedades para execução de verdade
                var backingFieldAprovado = tipoComando.GetField("<Aprovado>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                backingFieldAprovado?.SetValue(comandoInstancia, true);

                var backingFieldSimular = tipoComando.GetField("<Simular>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                backingFieldSimular?.SetValue(comandoInstancia, false);

                try
                {
                    // Dispara o comando original via MediatR dinamicamente
                    var sendMethod = _mediator.GetType().GetMethods()
                        .First(m => m.Name == "Send" && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(object));
                    
                    var taskResult = (Task<object>)sendMethod.Invoke(_mediator, new[] { comandoInstancia, cancellationToken })!;
                    var resultObj = await taskResult;
                    var result = (CommandResult)resultObj;

                    string logExecucao = $"[EXECUÇÃO EM PRODUÇÃO COMPLETA]\n";
                    if (result.Sucesso)
                    {
                        logExecucao += $"Status: Sucesso\n";
                        if (result.Dados != null)
                        {
                            var dadosJson = JsonSerializer.Serialize(result.Dados);
                            logExecucao += $"Retorno: {dadosJson}\n";
                        }
                        logExecucao += $"Mensagem: {result.Mensagem}";

                        execucao.ConcluirComSucesso(logExecucao, alteradoPor);
                    }
                    else
                    {
                        logExecucao += $"Status: Falha\n";
                        logExecucao += $"Erros: {string.Join(", ", result.Erros)}\n";
                        logExecucao += $"Mensagem: {result.Mensagem}";

                        execucao.RegistrarFalha(logExecucao, alteradoPor);
                    }
                }
                catch (Exception ex)
                {
                    string logErro = $"[EXECUÇÃO EM PRODUÇÃO FALHOU COM ERRO]\n";
                    logErro += $"Exceção: {ex.GetType().Name}\n";
                    logErro += $"Erro: {ex.Message}\n";
                    logErro += $"Stack: {ex.StackTrace}";

                    execucao.RegistrarFalha(logErro, alteradoPor);
                }
            }
            else
            {
                execucao.ConcluirComSucesso("Concluído com sucesso (sem comando associado).", alteradoPor);
            }

            await _context.SaveChangesAsync(cancellationToken);

            if (execucao.Status == "Failed")
            {
                return CommandResult.Falha(
                    erros: new[] { "A execução definitiva em massa falhou no processamento do comando." },
                    mensagem: "A execução falhou.",
                    dados: new { Log = execucao.ResultadoLog }
                );
            }

            return CommandResult.Ok("Execução em massa concluída com sucesso!", new { Log = execucao.ResultadoLog });
        }
    }
}
