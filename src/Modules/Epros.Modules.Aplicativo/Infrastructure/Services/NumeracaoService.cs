using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Infrastructure.Services
{
    /// <summary>
    /// TRANSVERSAL T9 — implementação central do serviço de numeração por (tenant, tipo de documento).
    /// Atomicidade sem gap/duplicidade via UPSERT com RETURNING no Postgres, numa ÚNICA instrução
    /// (não é read-modify-write, então não colide com o lock otimista xmin). A instrução é emitida via
    /// EF (o <c>TenantRlsInterceptor</c> propaga app.current_tenant_id → a RLS/WITH CHECK da tabela é
    /// respeitada). Substitui o padrão CountAsync (corrida) usado por vários módulos.
    /// </summary>
    public class NumeracaoService : INumeracaoService
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public NumeracaoService(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<long> ProximoNumeroAsync(
            string tipoDocumento,
            long valorInicial = 1,
            CancellationToken cancellationToken = default)
        {
            var tenant = _tenantProvider.GetTenantId();
            var user = _currentUser.GetUserId() ?? "system";

            // INSERT inicial entrega `valorInicial`; em conflito (linha já existe) incrementa +1.
            // RETURNING devolve o número a usar. A coluna precisa vir aliás "Value" p/ o SqlQueryRaw<long>.
            const string sql = @"
INSERT INTO aplicativo.sequencias_numeracao
    (id, sync_id, tenant_id, sync_version, criado_em, criado_por, tipo_documento, ultimo_valor)
VALUES ({0}, {1}, {2}, 0, (now() at time zone 'utc'), {3}, {4}, {5})
ON CONFLICT (tenant_id, tipo_documento)
DO UPDATE SET ultimo_valor = sequencias_numeracao.ultimo_valor + 1
RETURNING ultimo_valor AS ""Value"";";

            // ToListAsync (enumeração pura) envia o SQL verbatim — sem envelopar em subconsulta,
            // o que permitiria INSERT...RETURNING. Um SingleAsync com predicado enrolaria o SQL.
            var resultado = await _context.Database
                .SqlQueryRaw<long>(
                    sql,
                    System.Guid.NewGuid(),
                    System.Guid.NewGuid(),
                    tenant,
                    user,
                    tipoDocumento,
                    valorInicial)
                .ToListAsync(cancellationToken);

            return resultado.Single();
        }
    }
}
