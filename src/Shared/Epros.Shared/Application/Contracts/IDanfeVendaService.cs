namespace Epros.Shared.Application.Contracts
{
    /// <summary>
    /// Contrato cross-module para geração do DANFE/cupom de PRÉ-VISUALIZAÇÃO (sem autorização)
    /// de uma venda, a partir do documento fiscal a ela vinculado (módulo Fiscal).
    ///
    /// Porte de VendaNfeController.gerar-danfe-sem-autorizacao: o legado transmite o rascunho e
    /// pede o PDF "sem autorização" à API DFe. No novo desenho, o módulo Vendas NÃO referencia o
    /// módulo Fiscal — ele apenas guarda o <c>DocumentoFiscalId</c> (Guid FK). A geração do PDF é
    /// responsabilidade do Fiscal (que conhece o <c>DocumentoFiscal</c> + IDanfeService), exposta
    /// aqui como interface em Shared para o handler de Vendas consumir por contrato.
    ///
    /// HONESTIDADE FISCAL: o PDF produzido é marcado como "SEM VALOR FISCAL" (pré-visualização) —
    /// nunca aparenta autorização inexistente.
    /// </summary>
    public interface IDanfeVendaService
    {
        /// <summary>
        /// Gera os bytes do PDF de pré-visualização do DANFE/cupom a partir do documento fiscal
        /// vinculado à venda. Retorna <c>null</c> quando não há implementação/documento disponível
        /// (o handler traduz em falha amigável, sem lançar).
        /// </summary>
        /// <param name="documentoFiscalId">FK do documento fiscal (módulo Fiscal) vinculado à venda.</param>
        Task<DanfeVendaPdf?> GerarPreviewPorDocumentoFiscalIdAsync(Guid documentoFiscalId, CancellationToken cancellationToken = default);
    }

    /// <summary>DTO de retorno do PDF de pré-visualização do DANFE (bytes + nome do arquivo).</summary>
    public sealed record DanfeVendaPdf(byte[] Conteudo, string NomeArquivo);
}
