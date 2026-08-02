using System.Threading;
using System.Threading.Tasks;

namespace Epros.Modules.Aplicativo.Application.Contracts
{
    /// <summary>
    /// PLT · GED (T10) — abstração ÚNICA de armazenamento de conteúdo binário do GED canônico.
    /// O provider real (MinIO/S3/disco/servidor de storage) é DEPENDÊNCIA DE AMBIENTE e entra por
    /// overlay/config. O fluxo de registro, hash, dedupe e versionamento é construído sobre esta
    /// interface — sem provider configurado, a implementação padrão devolve uma referência opaca
    /// determinística (documento fica registrado, mas o byte físico não é persistido em storage real).
    /// </summary>
    public interface IArmazenamentoDocumentoService
    {
        /// <summary>Persiste o conteúdo e devolve a referência opaca de storage (StorageRef).</summary>
        Task<string> ArmazenarAsync(byte[] conteudo, string chaveLogica, CancellationToken cancellationToken = default);

        /// <summary>Recupera o conteúdo a partir da referência de storage, quando disponível.</summary>
        Task<byte[]?> ObterAsync(string storageRef, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove o byte físico. REG-021: o chamador (GED) só invoca quando nenhum outro registro
        /// lógico ativo aponta para o mesmo hash (dedupe preserva bytes compartilhados).
        /// </summary>
        Task RemoverAsync(string storageRef, CancellationToken cancellationToken = default);

        /// <summary>True quando há provider de storage real configurado no ambiente.</summary>
        bool ProviderConfigurado { get; }
    }
}
