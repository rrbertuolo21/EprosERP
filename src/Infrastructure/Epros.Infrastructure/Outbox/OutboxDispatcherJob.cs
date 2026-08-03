using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Epros.Infrastructure.Outbox
{
    /// <summary>
    /// TRANSVERSAL T1 — Job Quartz genérico que roda o <see cref="OutboxDispatcher"/> central contra a
    /// tabela <c>outbox_messages</c> de um <typeparamref name="TContext"/> específico (um por schema com
    /// eventos a rotear). Registrado no <c>Program.cs</c> como <c>OutboxDispatcherJob&lt;ContextX&gt;</c>.
    ///
    /// <see cref="DisallowConcurrentExecutionAttribute"/> garante um único despacho por vez por schema
    /// (evita dupla entrega/dupla-contagem).
    /// </summary>
    [DisallowConcurrentExecution]
    public class OutboxDispatcherJob<TContext> : IJob where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly OutboxDispatcher _dispatcher;
        private readonly ILogger<OutboxDispatcherJob<TContext>> _logger;

        public OutboxDispatcherJob(
            TContext context,
            OutboxDispatcher dispatcher,
            ILogger<OutboxDispatcherJob<TContext>> logger)
        {
            _context = context;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var ct = context?.CancellationToken ?? default;
            var roteadas = await _dispatcher.ProcessAsync(_context, ct);
            if (roteadas > 0)
                _logger.LogInformation("[OutboxDispatcher] {Context}: {Count} mensagem(ns) roteada(s).",
                    typeof(TContext).Name, roteadas);
        }
    }
}
