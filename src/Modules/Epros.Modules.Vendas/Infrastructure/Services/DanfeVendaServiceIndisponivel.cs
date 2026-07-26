using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Infrastructure.Services
{
    /// <summary>
    /// Implementação padrão (fallback) de <see cref="IDanfeVendaService"/> registrada quando o
    /// adaptador do módulo Fiscal ainda não está disponível. Retorna sempre <c>null</c> — o handler
    /// de Vendas traduz em falha amigável ("DANFE indisponível: vincule/emita o documento fiscal").
    ///
    /// O módulo Vendas não referencia o Fiscal; portanto o binding real (que usa o IDanfeService do
    /// Fiscal + o DocumentoFiscal) é registrado na camada de composição (API) quando existir. Este
    /// fallback mantém o módulo/solução compiláveis e o contrato satisfeito sem acoplamento indevido.
    /// </summary>
    public sealed class DanfeVendaServiceIndisponivel : IDanfeVendaService
    {
        public Task<DanfeVendaPdf?> GerarPreviewPorDocumentoFiscalIdAsync(Guid documentoFiscalId, CancellationToken cancellationToken = default)
            => Task.FromResult<DanfeVendaPdf?>(null);
    }
}
