using System;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.Aplicativo.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class ExpurgarNewsletterInativaJob : IJob
    {
        private readonly ContextAplicativo _context;

        public ExpurgarNewsletterInativaJob(ContextAplicativo context)
        {
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando ExpurgarNewsletterInativaJob...");

            var dataCorte = DateTime.UtcNow.AddDays(-30);

            // Localizar assinantes inativos há mais de 30 dias
            var inativosParaExpurgo = await _context.NewsletterSubscribers
                .IgnoreQueryFilters()
                .Where(n => !n.Ativo && n.DesativadoEm != null && n.DesativadoEm < dataCorte)
                .ToListAsync();

            if (inativosParaExpurgo.Any())
            {
                Console.WriteLine($"[Quartz] Expurgando fisicamente {inativosParaExpurgo.Count} assinantes de newsletter inativos/desativados há mais de 30 dias.");
                _context.NewsletterSubscribers.RemoveRange(inativosParaExpurgo);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("[Quartz] ExpurgarNewsletterInativaJob concluído.");
        }
    }
}
