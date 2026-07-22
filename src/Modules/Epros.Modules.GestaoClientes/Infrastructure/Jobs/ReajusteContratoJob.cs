using System;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.GestaoClientes.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class ReajusteContratoJob : IJob
    {
        private readonly ContextGestaoClientes _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReajusteContratoJob(
            ContextGestaoClientes context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var hoje = DateTime.UtcNow.Date;
            Console.WriteLine($"[Quartz] Iniciando o Job ReajusteContratoJob às {hoje}");

            // 1. Varrer configurações globais para obter percentual e nome do índice (ou usar fallback)
            var configPercentual = await _context.ConfiguracoesGlobais
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Chave == "reajuste_indice_percentual");

            var configNome = await _context.ConfiguracoesGlobais
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Chave == "reajuste_indice_nome");

            decimal percentualIndice = 4.5m; // Fallback
            string nomeIndice = "IGP-M"; // Fallback

            if (configPercentual != null)
            {
                var valorLimpo = configPercentual.Valor.Replace(",", ".");
                if (decimal.TryParse(valorLimpo, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p))
                {
                    percentualIndice = p;
                }
            }

            if (configNome != null && !string.IsNullOrEmpty(configNome.Valor))
            {
                nomeIndice = configNome.Valor;
            }

            // 2. Buscar todas as composições ativas reajustáveis (ignora filtros de tenant)
            var composicoesReajustaveis = await _context.ComposicoesFaturamento
                .IgnoreQueryFilters()
                .Where(c => c.PodeReajustar && (c.DataFinal == null || c.DataFinal > hoje) && c.DeletadoEm == null)
                .ToListAsync();

            Console.WriteLine($"[Quartz] Encontradas {composicoesReajustaveis.Count} composições de faturamento reajustáveis ativas.");

            foreach (var composicao in composicoesReajustaveis)
            {
                // Buscar último histórico de reajuste para essa composição
                var ultimoReajuste = await _context.HistoricosReajustes
                    .IgnoreQueryFilters()
                    .Where(h => h.ComposicaoId == composicao.Id && h.DeletadoEm == null)
                    .OrderByDescending(h => h.CriadoEm)
                    .FirstOrDefaultAsync();

                // Determina data base do reajuste
                DateTime dataBase = ultimoReajuste?.CriadoEm.Date ?? composicao.DataInicial.Date;

                // Se completou aniversário de 12 meses (ou seja, passaram-se 12 meses ou mais da data base)
                if (hoje >= dataBase.AddMonths(12))
                {
                    // Simula o contexto do request HTTP com o TenantId específico
                    var httpContext = new DefaultHttpContext();
                    httpContext.Items["TenantId"] = composicao.TenantId;
                    _httpContextAccessor.HttpContext = httpContext;

                    var valorAntigo = composicao.Valor;
                    var novoValor = Math.Round(valorAntigo * (1 + (percentualIndice / 100m)), 2);

                    composicao.ReajustarPreco(novoValor, "sistema_reajuste");
                    _context.ComposicoesFaturamento.Update(composicao);

                    var historico = new HistoricoReajuste(
                        composicao.Id,
                        $"Reajuste automático anual pelo índice {nomeIndice}",
                        valorAntigo,
                        novoValor,
                        percentualIndice,
                        nomeIndice,
                        composicao.TenantId,
                        "sistema_reajuste"
                    );

                    _context.HistoricosReajustes.Add(historico);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"[Quartz] Composição {composicao.Id} (Tenant {composicao.TenantId}) reajustada de R$ {valorAntigo:N2} para R$ {novoValor:N2} pelo índice {nomeIndice} ({percentualIndice}%).");
                }
            }

            Console.WriteLine("[Quartz] Job ReajusteContratoJob encerrado.");
        }
    }
}
