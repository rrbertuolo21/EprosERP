using System;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.GestaoClientes.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class VerificarFaturasVencidasJob : IJob
    {
        private readonly ContextGestaoClientes _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VerificarFaturasVencidasJob(
            ContextGestaoClientes context,
            IMediator mediator,
            IHttpContextAccessor _httpContextAccessor)
        {
            _context = context;
            _mediator = mediator;
            this._httpContextAccessor = _httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var hoje = DateTime.UtcNow;

            // Busca todas as faturas Pendentes vencidas (ignorando o filtro de tenant do HTTP local)
            var faturasVencidas = await _context.Faturas
                .IgnoreQueryFilters()
                .Where(f => f.Status == FaturaStatus.Pendente && f.DataVencimento < hoje && f.DeletadoEm == null)
                .ToListAsync();

            foreach (var fatura in faturasVencidas)
            {
                // Injeta um HttpContext simulado para que o TenantProvider scoped saiba qual tenant está sendo alterado
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = fatura.TenantId;
                _httpContextAccessor.HttpContext = httpContext;

                // Transiciona o status da fatura para "Atrasada"
                fatura.MarcarAtrasada("sistema_quartz");
                _context.Faturas.Update(fatura);
                await _context.SaveChangesAsync();

                // Se o atraso for maior que 15 dias, envia o comando de suspensão do cliente
                // (1.01: alinhado à regra de inadimplência de 15 dias já usada no login — AuthHandlers).
                if ((hoje - fatura.DataVencimento).TotalDays > 15)
                {
                    var result = await _mediator.Send(new SuspenderClienteCommand(fatura.ClienteId));
                    if (!result.Sucesso)
                    {
                        Console.WriteLine($"[Quartz] Falha ao suspender cliente {fatura.ClienteId}: {result.Mensagem}");
                    }
                }
            }
        }
    }
}
