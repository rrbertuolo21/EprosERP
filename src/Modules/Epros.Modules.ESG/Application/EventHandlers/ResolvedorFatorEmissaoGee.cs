using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Modules.ESG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Application.EventHandlers
{
    /// <summary>
    /// Resolve o fator de emissao VIGENTE no catalogo versionado esg.ghg_fator_emissao para um
    /// codigo/tenant/data. Substitui a antiga constante hardcoded (NF-01/A-01): sem fator vigente,
    /// retorna null e o chamador registra a emissao como "pendente de fator" (Regra #0 — nao inventa).
    ///
    /// Precedencia (RN-GHG NF-03, default seguro — // valida-contador): entre candidatos vigentes na
    /// data, vence a MAIOR versao (mais recente homologada). Filtragem por tenant e soft-delete e
    /// explicita (IgnoreQueryFilters) porque o handler roda sobre o tenant do evento, nao do ambiente.
    /// </summary>
    public static class ResolvedorFatorEmissaoGee
    {
        public static async Task<FatorEmissaoGee?> ResolverVigenteAsync(
            ContextESG context,
            string tenantId,
            string codigo,
            DateTime data,
            CancellationToken cancellationToken)
        {
            List<FatorEmissaoGee> candidatos = await context.FatoresEmissaoGee
                .IgnoreQueryFilters()
                .Where(f => f.TenantId == tenantId && f.Codigo == codigo && f.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            return candidatos
                .Where(f => f.VigenteEm(data))
                .OrderByDescending(f => f.Versao)
                .FirstOrDefault();
        }
    }
}
