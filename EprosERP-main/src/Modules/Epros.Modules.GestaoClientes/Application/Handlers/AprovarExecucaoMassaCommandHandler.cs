using System;
using System.Linq;
using System.Text.Json;
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
    /// <summary>Aprova Execucao Massa.</summary>
    public class AprovarExecucaoMassaCommandHandler : ICommandHandler<AprovarExecucaoMassaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AprovarExecucaoMassaCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AprovarExecucaoMassaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var userId = _currentUser.GetUserId() ?? "system";

            var execucao = await _context.ExecucoesMassa
                .FirstOrDefaultAsync(e => e.Id == request.ExecucaoMassaId && e.DeletadoEm == null, cancellationToken);

            if (execucao == null)
            {
                return CommandResult.Falha(new[] { "Solicitação de execução em massa não encontrada." });
            }

            // Adiciona a aprovação do checker
            execucao.AdicionarAprovacao(userId, userId);

            if (!execucao.IsValid)
            {
                var erros = execucao.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Não foi possível aprovar a solicitação.");
            }

            var aprovadores = execucao.ObterAprovadores();

            if (aprovadores.Count >= 2)
            {
                var isInMemory = _context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
                using var transaction = isInMemory ? null : await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    string logResult = "";
                    string finalStatus = "Executado";

                    if (execucao.TipoOperacao.Equals("AtualizarPrecosPlanos", StringComparison.OrdinalIgnoreCase))
                    {
                        var document = JsonDocument.Parse(execucao.Parametros);
                        var percentual = document.RootElement.GetProperty("Percentual").GetDecimal();

                        // Lista todos os planos (ignorando filtros de inquilino para atualizar todos do sistema)
                        var planos = await _context.Planos
                            .IgnoreQueryFilters()
                            .Where(p => p.Ativo && p.DeletadoEm == null)
                            .ToListAsync(cancellationToken);

                        int count = 0;
                        foreach (var plano in planos)
                        {
                            var precoAntigo = plano.Preco;
                            var novoPreco = Math.Round(plano.Preco * (1 + percentual / 100), 2);
                            plano.AtualizarPreco(novoPreco, userId);
                            logResult += $"Plano '{plano.Nome}' (ID: {plano.Id}) atualizado de R$ {precoAntigo} para R$ {novoPreco}.\n";
                            count++;
                        }

                        logResult += $"Total de {count} planos atualizados com acréscimo de {percentual}%.";
                    }
                    else if (execucao.TipoOperacao.Equals("SuspenderInadimplentes", StringComparison.OrdinalIgnoreCase))
                    {
                        var document = JsonDocument.Parse(execucao.Parametros);
                        var diasAtraso = document.RootElement.GetProperty("DiasAtraso").GetInt32();
                        var limite = DateTime.UtcNow.AddDays(-diasAtraso);

                        // Busca faturas vencidas antes da data limite
                        var faturasAtrasadas = await _context.Faturas
                            .IgnoreQueryFilters()
                            .Where(f => (f.Status == FaturaStatus.Pendente || f.Status == FaturaStatus.Atrasada) && f.DataVencimento < limite && f.DeletadoEm == null)
                            .ToListAsync(cancellationToken);

                        var clienteIds = faturasAtrasadas.Select(f => f.ClienteId).Distinct().ToList();

                        int count = 0;
                        foreach (var id in clienteIds)
                        {
                            var cliente = await _context.Clientes
                                .IgnoreQueryFilters()
                                .FirstOrDefaultAsync(c => c.Id == id && c.Ativo && c.DeletadoEm == null, cancellationToken);

                            if (cliente != null)
                            {
                                cliente.Inativar(userId);
                                logResult += $"Cliente '{cliente.RazaoSocial}' (ID: {cliente.Id}) suspenso por atraso superior a {diasAtraso} dias.\n";
                                count++;
                            }
                        }

                        logResult += $"Total de {count} clientes inadimplentes suspensos.";
                    }
                    else
                    {
                        finalStatus = "Falho";
                        logResult = $"Erro: Tipo de operação '{execucao.TipoOperacao}' desconhecido.";
                    }

                    execucao.Executar(logResult, finalStatus, userId);
                    await _context.SaveChangesAsync(cancellationToken);
                    
                    if (transaction != null)
                    {
                        await transaction.CommitAsync(cancellationToken);
                    }

                    return CommandResult.Ok($"Execução em massa realizada com sucesso (Status: {finalStatus}).", new { Log = logResult });
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                    }
                    execucao.Executar($"Erro na execução: {ex.Message}", "Falho", userId);
                    await _context.SaveChangesAsync(cancellationToken);
                    return CommandResult.Falha(new[] { $"Erro durante processamento da transação em massa: {ex.Message}" });
                }
            }
            else
            {
                // Apenas salvou a aprovação pendente de mais assinaturas
                await _context.SaveChangesAsync(cancellationToken);
                return CommandResult.Ok("Aprovação registrada com sucesso. Aguardando segunda aprovação.", new { Aprovadores = aprovadores });
            }
        }
    }
}
