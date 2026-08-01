using System;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.GestaoClientes.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class ProcessarFaturamentoRecorrenteJob : IJob
    {
        private readonly ContextGestaoClientes _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProcessarFaturamentoRecorrenteJob(
            ContextGestaoClientes context,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var hoje = DateTime.UtcNow;
            Console.WriteLine($"[Quartz] Iniciando o Job ProcessarFaturamentoRecorrenteJob às {hoje}");

            // Busca todos os contratos ativos que precisam de faturamento (ignora RLS/Query Filters de tenant)
            var contratosAtivos = await _context.Contratos
                .IgnoreQueryFilters()
                .Where(c => c.Ativo && c.DataInicio <= hoje && (c.DataFim == null || c.DataFim >= hoje) && c.DeletadoEm == null)
                .ToListAsync();

            // Filtra e agrupa os inquilinos que possuem contratos pendentes de faturamento hoje
            var tenantsParaFaturar = contratosAtivos
                .Where(c =>
                {
                    var jaFaturadoEsteMes = c.UltimoFaturamento.HasValue &&
                                            c.UltimoFaturamento.Value.Month == hoje.Month &&
                                            c.UltimoFaturamento.Value.Year == hoje.Year;

                    var diaChegou = hoje.Day >= c.DiaVencimento;

                    return !jaFaturadoEsteMes && diaChegou;
                })
                .Select(c => c.TenantId)
                .Distinct()
                .ToList();

            Console.WriteLine($"[Quartz] Encontrados {tenantsParaFaturar.Count} tenants com faturamentos pendentes hoje.");

            foreach (var tenantId in tenantsParaFaturar)
            {
                try
                {
                    // Simula o contexto do request HTTP com o TenantId específico
                    var httpContext = new DefaultHttpContext();
                    httpContext.Items["TenantId"] = tenantId;
                    _httpContextAccessor.HttpContext = httpContext;

                    Console.WriteLine($"[Quartz] Disparando processamento de faturamento para o Tenant {tenantId}");

                    var result = await _mediator.Send(new ProcessarFaturamentoRecorrenteCommand(hoje));

                    if (!result.Sucesso)
                    {
                        var erros = string.Join(", ", result.Erros);
                        Console.WriteLine($"[Quartz] Falha ao faturar contratos do Tenant {tenantId}: {erros}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Quartz] [Erro] Exceção crítica ao faturar Tenant {tenantId}: {ex.Message}");
                }
            }

            Console.WriteLine("[Quartz] Job ProcessarFaturamentoRecorrenteJob encerrado.");
        }
    }
}
