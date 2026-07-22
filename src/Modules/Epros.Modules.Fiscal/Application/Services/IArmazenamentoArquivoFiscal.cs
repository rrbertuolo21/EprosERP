using System.Threading;
using System.Threading.Tasks;

namespace Epros.Modules.Fiscal.Application.Services
{
    /// <summary>
    /// Abstração de armazenamento de artefatos fiscais (XML autorizado, PDF do DANFE/cupom).
    ///
    /// Implementação atual: sistema de arquivos local (<c>ArmazenamentoArquivoFiscalLocal</c>).
    /// Seam pronto para uma futura implementação MinIO/S3 (basta registrar outra implementação no DI):
    /// o retorno é o "caminho"/chave persistido em <c>DocumentoFiscal.XmlCaminho</c>/<c>PdfCaminho</c>.
    /// </summary>
    public interface IArmazenamentoArquivoFiscal
    {
        /// <summary>Salva bytes e devolve o caminho/URI persistível. <paramref name="chaveLogica"/> agrupa por documento.</summary>
        Task<string> SalvarAsync(string chaveLogica, string nomeArquivo, byte[] conteudo, string contentType, CancellationToken ct = default);

        /// <summary>Lê os bytes de um caminho previamente salvo. Retorna null se não existir.</summary>
        Task<byte[]?> LerAsync(string caminho, CancellationToken ct = default);
    }
}
