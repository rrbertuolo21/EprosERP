using System.Threading.Tasks;

namespace Epros.Shared.Application.Contracts
{
    /// <summary>
    /// TRANSVERSAL T5 / TC-04 — ROTAÇÃO de segredos do cofre central.
    /// O cofre (<see cref="ISegredoCofreService"/>) já cifra/decifra; a lacuna real fechada aqui
    /// é a ROTAÇÃO: re-encriptar (rewrap) um ciphertext existente sob a versão de chave corrente,
    /// sem expor o texto plano. Interface separada para não quebrar consumidores atuais do cofre.
    /// Ver DECISOES-TRANSVERSAIS.md · T5 e MODELO_DADOS.md · VAULT_SEGREDO(rotacionado_em).
    /// </summary>
    public interface ISegredoRotacaoService
    {
        /// <summary>
        /// Re-encripta um ciphertext para a versão de chave mais recente e devolve o novo ciphertext.
        /// No cofre Vault usa o rewrap do Transit (não decifra em claro no cliente); no fallback local
        /// re-encripta com nonce novo. Idempotente do ponto de vista do valor (o texto plano é o mesmo).
        /// </summary>
        Task<string> RotacionarAsync(string ciphertext);

        /// <summary>
        /// Indica se um ciphertext deveria ser rotacionado dado um limite de idade em dias,
        /// a partir do instante da última rotação/gravação informado.
        /// Auxiliar de política (o agendamento/persistência de datas fica no dono do segredo).
        /// </summary>
        bool PrecisaRotacionar(System.DateTime rotacionadoEm, int idadeMaximaDias);
    }
}
