using System;
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
    /// <summary>Processa Faturamento Recorrente.</summary>
    public class ProcessarFaturamentoRecorrenteCommandHandler : ICommandHandler<ProcessarFaturamentoRecorrenteCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ProcessarFaturamentoRecorrenteCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ProcessarFaturamentoRecorrenteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";
            var referencia = request.Referencia ?? DateTime.UtcNow;

            Console.WriteLine($"[Billing] Processando faturamento recorrente para o Tenant {tenantId} na data {referencia}");

            // Busca os contratos ativos do tenant que iniciaram antes ou na data de referência
            var contratos = await _context.Contratos
                .Include(c => c.Itens)
                .Where(c => c.Ativo && c.DataInicio <= referencia && (c.DataFim == null || c.DataFim >= referencia))
                .ToListAsync(cancellationToken);

            var faturasGeradas = 0;

            foreach (var contrato in contratos)
            {
                // Verifica se já foi faturado no mesmo mês e ano da data de referência
                var jaFaturadoEsteMes = contrato.UltimoFaturamento.HasValue &&
                                        contrato.UltimoFaturamento.Value.Month == referencia.Month &&
                                        contrato.UltimoFaturamento.Value.Year == referencia.Year;

                // Verifica se o dia de faturamento chegou ou já passou no mês de referência
                var diaFaturamentoChegou = referencia.Day >= contrato.DiaVencimento;

                if (!jaFaturadoEsteMes && diaFaturamentoChegou)
                {
                    try
                    {
                        var dataVencimento = new DateTime(referencia.Year, referencia.Month, contrato.DiaVencimento);

                        var fatura = new Fatura(
                            contrato.ClienteId,
                            contrato.ValorRecorrente,
                            dataVencimento,
                            tenantId,
                            criadoPor
                        );

                        if (fatura.IsValid)
                        {
                            _context.Faturas.Add(fatura);
                            contrato.AtualizarFaturamento(referencia, criadoPor);
                            faturasGeradas++;

                            Console.WriteLine($"[Billing] Fatura recorrente de R$ {contrato.ValorRecorrente} gerada para o Cliente {contrato.ClienteId} com vencimento em {dataVencimento}");
                        }
                        else
                        {
                            var erros = string.Join(", ", fatura.Notifications.Select(n => n.Message));
                            Console.WriteLine($"[Billing] [Aviso] Falha ao validar invariantes da fatura para o Contrato {contrato.Id}: {erros}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Billing] [Erro] Erro ao processar faturamento para o Contrato {contrato.Id}: {ex.Message}");
                    }
                }
            }

            if (faturasGeradas > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return CommandResult.Ok("Faturamento recorrente concluído.", new System.Collections.Generic.Dictionary<string, int> { { "TotalFaturasGeradas", faturasGeradas } });
        }
    }
}
