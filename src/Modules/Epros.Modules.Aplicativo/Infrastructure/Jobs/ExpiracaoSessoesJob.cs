using System;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.Aplicativo.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class ExpiracaoSessoesJob : IJob
    {
        private readonly ContextAplicativo _context;

        public ExpiracaoSessoesJob(ContextAplicativo context)
        {
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando ExpiracaoSessoesJob...");

            var dataCorteTimeout = DateTime.UtcNow.AddHours(-2);
            var dataCorteLGPD = DateTime.UtcNow.AddDays(-90);

            // 1. Timeout forçado de sessões de impersonação (2 horas)
            var sessoesImpersonacaoExpiradas = await _context.SessoesImpersonacao
                .IgnoreQueryFilters()
                .Where(s => s.FimEm == null && s.InicioEm < dataCorteTimeout)
                .ToListAsync();

            if (sessoesImpersonacaoExpiradas.Any())
            {
                Console.WriteLine($"[Quartz] Expirando forçadamente {sessoesImpersonacaoExpiradas.Count} sessões de impersonação ativas há mais de 2 horas.");
                foreach (var sessao in sessoesImpersonacaoExpiradas)
                {
                    sessao.Encerrar("system_job");
                }
                await _context.SaveChangesAsync();
            }

            // 2. Expurgo físico (Hard Delete) de logs de impersonação encerrados há mais de 90 dias
            var sessoesImpersonacaoParaExpurgo = await _context.SessoesImpersonacao
                .IgnoreQueryFilters()
                .Where(s => (s.FimEm != null && s.FimEm < dataCorteLGPD) || (s.DeletadoEm != null && s.DeletadoEm < dataCorteLGPD))
                .ToListAsync();

            if (sessoesImpersonacaoParaExpurgo.Any())
            {
                Console.WriteLine($"[Quartz] Expurgando fisicamente {sessoesImpersonacaoParaExpurgo.Count} sessões de impersonação com mais de 90 dias.");
                _context.SessoesImpersonacao.RemoveRange(sessoesImpersonacaoParaExpurgo);
            }

            // 3. Expurgo físico (Hard Delete) de logs de sessões de usuário encerradas/expiradas há mais de 90 dias
            var sessoesUsuarioParaExpurgo = await _context.SessoesUsuarios
                .IgnoreQueryFilters()
                .Where(s => s.Expiracao < dataCorteLGPD || (s.DeletadoEm != null && s.DeletadoEm < dataCorteLGPD))
                .ToListAsync();

            if (sessoesUsuarioParaExpurgo.Any())
            {
                Console.WriteLine($"[Quartz] Expurgando fisicamente {sessoesUsuarioParaExpurgo.Count} sessões de usuário expiradas/deletadas há mais de 90 dias.");
                _context.SessoesUsuarios.RemoveRange(sessoesUsuarioParaExpurgo);
            }

            if (sessoesImpersonacaoParaExpurgo.Any() || sessoesUsuarioParaExpurgo.Any())
            {
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("[Quartz] ExpiracaoSessoesJob concluído.");
        }
    }
}
