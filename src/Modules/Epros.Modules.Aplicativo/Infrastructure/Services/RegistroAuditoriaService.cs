using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Infrastructure.Services
{
    /// <summary>
    /// TRANSVERSAL T8 — implementação central da trilha de auditoria imutável.
    /// Grava um <see cref="RegistroAuditoria"/> (append-only) no contexto de plataforma. O tenant e o
    /// usuário são resolvidos do contexto atual; a RLS da tabela (WITH CHECK tenant) garante que o
    /// registro pertence ao tenant corrente. Nenhum caminho de update/delete é exposto.
    /// </summary>
    public class RegistroAuditoriaService : IRegistroAuditoriaService
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistroAuditoriaService(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task RegistrarAsync(
            string entidade,
            string entidadeId,
            string acao,
            string? valoresAntes = null,
            string? valoresDepois = null,
            CancellationToken cancellationToken = default)
        {
            var registro = RegistroAuditoria.Criar(
                tenantId: _tenantProvider.GetTenantId(),
                entidade: entidade,
                entidadeId: entidadeId,
                acao: acao,
                valoresAntes: valoresAntes,
                valoresDepois: valoresDepois,
                usuario: _currentUser.GetUserId(),
                ipOrigem: null);

            _context.Set<RegistroAuditoria>().Add(registro);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
