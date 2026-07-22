using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Domain.Entities.Workflow;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Infrastructure.Behaviors
{
    public class MakerCheckerPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public MakerCheckerPipelineBehavior(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is IComandoRisco comandoRisco)
            {
                // 1. Cenário de Simulação (Dry-Run / Sandbox) com Rollback
                if (comandoRisco.Simular)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                    try
                    {
                        var response = await next();
                        // Rollback obrigatório para simular sem persistir
                        await transaction.RollbackAsync(cancellationToken);
                        return response;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw;
                    }
                }

                // 2. Cenário de Gravação/Retenção (Maker-Checker declarativo via Workflow)
                if (!comandoRisco.Aprovado)
                {
                    var criadoPor = _currentUser.GetUserId() ?? "system";
                    var tenantId = _tenantProvider.GetTenantId();
                    var payload = JsonSerializer.Serialize(request);
                    var commandType = request.GetType().AssemblyQualifiedName ?? request.GetType().FullName ?? "";
                    var entidadeTipo = request.GetType().Name;
                    var descricao = comandoRisco.ObterDescricao();

                    // Retenção legada (execução em massa) mantida por compatibilidade.
                    var execucao = new ExecucaoMassaGlobal(
                        descricao: descricao,
                        actionPayload: payload,
                        status: "Draft",
                        tenantId: "system",
                        criadoPor: criadoPor,
                        commandType: commandType
                    );
                    _context.ExecucoesMassaGlobal.Add(execucao);

                    // Aprovação declarativa: se existir uma definição de workflow ativa para este comando
                    // (por entidade específica ou catch-all '*'), cria a instância e publica AprovacaoSolicitada.
                    var definicao = await _context.WfDefinicoes
                        .Where(d => d.Status == EWfDefinicaoStatus.Ativo && d.DeletadoEm == null &&
                                    (d.Entidade == entidadeTipo || d.Entidade == "*"))
                        .OrderByDescending(d => d.Entidade == entidadeTipo)
                        .FirstOrDefaultAsync(cancellationToken);

                    Guid? instanciaId = null;
                    if (definicao != null)
                    {
                        var instancia = new WfInstancia(
                            definicaoId: definicao.Id,
                            modulo: definicao.Modulo,
                            entidadeTipo: entidadeTipo,
                            entidadeIdReferencia: execucao.Id.ToString(),
                            descricao: descricao,
                            estadoAtualId: null,
                            responsavelUsuarioId: null,
                            valorReferencia: null,
                            commandType: commandType,
                            payload: payload,
                            tenantId: tenantId,
                            criadoPor: criadoPor);

                        if (instancia.IsValid)
                        {
                            instancia.Submeter(null, criadoPor);
                            _context.WfInstancias.Add(instancia);
                            instanciaId = instancia.Id;

                            var evento = new AprovacaoSolicitadaEventNotification(
                                InstanciaId: instancia.Id,
                                DefinicaoId: definicao.Id,
                                Modulo: definicao.Modulo,
                                EntidadeTipo: entidadeTipo,
                                EntidadeId: execucao.Id.ToString(),
                                Descricao: descricao,
                                ValorReferencia: null,
                                CommandType: commandType,
                                Payload: payload,
                                SolicitadoPor: criadoPor,
                                TenantId: tenantId,
                                UserId: criadoPor);

                            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "AprovacaoSolicitada", JsonSerializer.Serialize(evento)));
                        }
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    if (typeof(TResponse) == typeof(CommandResult))
                    {
                        // Mantém o contrato existente (Dados = Guid da execução retida); a instância de
                        // workflow declarativa é rastreável via WfInstancia/Outbox.
                        var result = CommandResult.Falha(
                            erros: new[] { "Comando retido para aprovação por alçada (Maker-Checker via Workflow)." },
                            mensagem: "Comando retido para aprovação.",
                            dados: execucao.Id
                        );
                        return (result as TResponse)!;
                    }

                    throw new InvalidOperationException("Comando de risco retido para aprovação (Maker-Checker).");
                }
            }

            return await next();
        }
    }
}
