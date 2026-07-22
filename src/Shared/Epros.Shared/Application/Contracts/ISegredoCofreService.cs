using System.Threading.Tasks;

namespace Epros.Shared.Application.Contracts
{
    /// <summary>
    /// Serviço responsável por realizar operações criptográficas de segredos (Cofre / Vault).
    /// </summary>
    public interface ISegredoCofreService
    {
        /// <summary>
        /// Criptografa o valor informado.
        /// </summary>
        /// <param name="valor">Texto plano a ser criptografado.</param>
        /// <returns>O valor criptografado em formato ciphertext (com prefixo identificando o provedor).</returns>
        Task<string> CriptografarAsync(string valor);

        /// <summary>
        /// Descriptografa o valor criptografado.
        /// </summary>
        /// <param name="ciphertext">O texto criptografado contendo o prefixo do provedor.</param>
        /// <returns>O texto original descriptografado.</returns>
        Task<string> DescriptografarAsync(string ciphertext);
    }
}
