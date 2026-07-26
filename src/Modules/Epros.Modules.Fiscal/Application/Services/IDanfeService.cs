using Epros.Modules.Fiscal.Domain.Entities;

namespace Epros.Modules.Fiscal.Application.Services
{
    /// <summary>
    /// Gera o PDF do documento auxiliar (DANFE para NF-e modelo 55, cupom para NFC-e modelo 65)
    /// a partir de um <see cref="DocumentoFiscal"/> autorizado.
    ///
    /// IMPORTANTE (honestidade fiscal): só produz PDF para documentos AUTORIZADOS (com chave de acesso
    /// e protocolo). Para documentos não autorizados, gera um PDF marcado como "SEM VALOR FISCAL"
    /// (pré-visualização) — nunca um documento que aparente autorização inexistente.
    /// </summary>
    public interface IDanfeService
    {
        /// <summary>Gera o PDF do DANFE (modelo 55) ou cupom NFC-e (modelo 65). Retorna os bytes do PDF.</summary>
        byte[] GerarPdf(DocumentoFiscal documento);
    }
}
