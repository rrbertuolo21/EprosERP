using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.GestaoClientes.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class ReguaCobrancaJob : IJob
    {
        private readonly ContextGestaoClientes _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReguaCobrancaJob(
            ContextGestaoClientes context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var hoje = DateTime.UtcNow.Date;
            Console.WriteLine($"[Quartz] Iniciando o Job ReguaCobrancaJob às {hoje}");

            // Busca todas as faturas em aberto (ignora RLS do tenant)
            var faturasAbertas = await _context.Faturas
                .IgnoreQueryFilters()
                .Where(f => f.Status != FaturaStatus.Paga && f.Status != FaturaStatus.Cancelada && f.DeletadoEm == null)
                .ToListAsync();

            Console.WriteLine($"[Quartz] Encontradas {faturasAbertas.Count} faturas ativas em aberto.");

            foreach (var fatura in faturasAbertas)
            {
                var vencimento = fatura.DataVencimento.Date;
                var diferencaDias = (vencimento - hoje).Days;

                string? tipoAlerta = null;
                string? mensagem = null;

                switch (diferencaDias)
                {
                    case 3:
                        tipoAlerta = "D-3";
                        mensagem = $"Aviso pré-vencimento: Sua fatura de R$ {fatura.Valor:N2} vence em 3 dias ({vencimento:dd/MM/yyyy}). Evite a suspensão dos serviços.";
                        break;
                    case 1:
                        tipoAlerta = "D-1";
                        mensagem = $"Aviso de vencimento amanhã: Sua fatura de R$ {fatura.Valor:N2} vence amanhã ({vencimento:dd/MM/yyyy}). Segue link para pagamento via Pix.";
                        break;
                    case -1:
                        tipoAlerta = "D+1";
                        mensagem = $"Lembrete de fatura vencida: Identificamos que a fatura de R$ {fatura.Valor:N2} vencida em {vencimento:dd/MM/yyyy} ainda não foi paga. Regularize seu plano.";
                        break;
                    case -5:
                        tipoAlerta = "D+5";
                        mensagem = $"Alerta de atraso: Sua fatura de R$ {fatura.Valor:N2} está vencida há 5 dias. Efetue o pagamento para evitar o bloqueio dos serviços.";
                        break;
                    case -10:
                        tipoAlerta = "D+10";
                        mensagem = $"Alerta crítico de atraso: Sua fatura de R$ {fatura.Valor:N2} está vencida há 10 dias. O sistema poderá ser suspenso em breve.";
                        break;
                    case -14:
                        tipoAlerta = "D+14";
                        mensagem = $"AVISO FINAL DE SUSPENSÃO: Sua fatura de R$ {fatura.Valor:N2} está vencida há 14 dias. Seus serviços serão suspensos/bloqueados em 24 horas caso não haja regularização.";
                        break;
                }

                if (tipoAlerta != null && mensagem != null)
                {
                    // Simula o contexto do request HTTP com o TenantId específico para caso queira auditar logs/salvamento
                    var httpContext = new DefaultHttpContext();
                    httpContext.Items["TenantId"] = fatura.TenantId;
                    _httpContextAccessor.HttpContext = httpContext;

                    var pixCopiaECola = $"00020101021226830014br.gov.bcb.pix2561api.mercadopago.com/v1/payments/{fatura.Id}";

                    var payload = new
                    {
                        FaturaId = fatura.Id,
                        ClienteId = fatura.ClienteId,
                        Valor = fatura.Valor,
                        DataVencimento = fatura.DataVencimento,
                        DiasDiferenca = diferencaDias,
                        TipoAlerta = tipoAlerta,
                        PixCopiaECola = pixCopiaECola,
                        Mensagem = mensagem,
                        EnviadoEm = DateTime.UtcNow
                    };

                    var payloadJson = JsonSerializer.Serialize(payload);
                    var outboxMessage = new OutboxMessage(fatura.TenantId, "FaturaAlertaCobrancaEvent", payloadJson);
                    
                    _context.OutboxMessages.Add(outboxMessage);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"[Quartz] Alerta {tipoAlerta} enfileirado no Outbox para a FaturaId {fatura.Id} do TenantId {fatura.TenantId}.");
                }
            }

            Console.WriteLine("[Quartz] Job ReguaCobrancaJob encerrado.");
        }
    }
}
