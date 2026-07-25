using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.Aplicativo.Infrastructure.Jobs
{
    /// <summary>
    /// Job de importação/upload (PLT-UPL): remove partes temporárias de upload fracionado com mais de
    /// 3 dias (EF UPLOAD 7.2.4 / UPL-CA-004). Deve ser agendado no Program.cs na consolidação.
    /// </summary>
    [DisallowConcurrentExecution]
    public class LimpezaPartesAntigasJob : IJob
    {
        private readonly ContextAplicativo _context;

        public LimpezaPartesAntigasJob(ContextAplicativo context)
        {
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando LimpezaPartesAntigasJob...");

            var dataCorte = DateTime.UtcNow.AddDays(-3);

            var partesAntigas = await _context.UplUploadPartes
                .IgnoreQueryFilters()
                .Where(p => p.DeletadoEm == null && p.CriadoEm < dataCorte)
                .ToListAsync();

            if (partesAntigas.Any())
            {
                foreach (var parte in partesAntigas)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parte.CaminhoTemporario) && File.Exists(parte.CaminhoTemporario))
                        {
                            File.Delete(parte.CaminhoTemporario);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Quartz] Falha ao remover arquivo temporário {parte.CaminhoTemporario}: {ex.Message}");
                    }

                    parte.Deletar("system");
                }

                Console.WriteLine($"[Quartz] Removendo {partesAntigas.Count} partes de upload com mais de 3 dias.");
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("[Quartz] LimpezaPartesAntigasJob concluído.");
        }
    }
}
