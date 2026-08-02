using System.Threading;
using System.Threading.Tasks;

namespace Epros.Shared.Application.Contracts
{
    /// <summary>
    /// TRANSVERSAL T8 — porta única para gravar a trilha de auditoria imutável central.
    /// Qualquer módulo injeta esta interface para registrar antes→depois sem manter tabela
    /// *_historico própria. A implementação persiste em <c>RegistroAuditoria</c> (append-only).
    /// </summary>
    public interface IRegistroAuditoriaService
    {
        /// <summary>
        /// Grava um registro de auditoria. <paramref name="valoresAntes"/> e
        /// <paramref name="valoresDepois"/> são snapshots serializados (JSON) — null quando não
        /// se aplica (criação não tem "antes"; exclusão não tem "depois").
        /// Usuário/IP são resolvidos do contexto atual quando não informados.
        /// </summary>
        Task RegistrarAsync(
            string entidade,
            string entidadeId,
            string acao,
            string? valoresAntes = null,
            string? valoresDepois = null,
            CancellationToken cancellationToken = default);
    }
}
