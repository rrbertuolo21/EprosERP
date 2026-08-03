using System.Threading;
using System.Threading.Tasks;

namespace Epros.Shared.Application.Contracts
{
    /// <summary>
    /// TRANSVERSAL T9 — SERVIÇO CENTRAL DE NUMERAÇÃO por tenant + tipo de documento.
    /// Sequência ATÔMICA, sem gap e sem duplicidade sob concorrência (NF, pedido, contrato,
    /// fatura, OS…). Substitui o padrão <c>CountAsync</c> (que sofre corrida) usado por vários
    /// módulos. Ver DECISOES-TRANSVERSAIS.md · T9 e DECISOES_IMPLANTACAO_V1.md · TC-11/NF-05.
    /// </summary>
    public interface INumeracaoService
    {
        /// <summary>
        /// Reserva e devolve o próximo número da sequência (tenant corrente, <paramref name="tipoDocumento"/>).
        /// A operação é atômica: dois chamadores concorrentes nunca recebem o mesmo número e não há buraco.
        /// A primeira chamada de um tipo inicia em <paramref name="valorInicial"/> (default 1).
        /// </summary>
        Task<long> ProximoNumeroAsync(
            string tipoDocumento,
            long valorInicial = 1,
            CancellationToken cancellationToken = default);
    }
}
