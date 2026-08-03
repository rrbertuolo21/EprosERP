using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Services
{
    /// <summary>
    /// ANTI-DUPLA-CONTAGEM (Gap LDE): guarda de idempotência do CRÉDITO de estoque de uma COMPRA.
    ///
    /// Existem dois caminhos para a mesma mercadoria comprada:
    ///   1) LANÇAMENTO FISCAL (LancarCompra / LancarCompraFiscal / ImportarCompraXml) — é a ÚNICA autoridade
    ///      que credita o estoque, pelo MOTOR ÚNICO, gerando um FatoGeradorEstoque com CompraId preenchido.
    ///   2) LOGÍSTICA DE ENTRADA (LDE) — registra o RECEBIMENTO FÍSICO e apenas PUBLICA eventos
    ///      (MercadoriaRecebida, LdeEntradaConfirmada). A LDE NÃO credita o kardex, para não creditar de
    ///      novo o que o lançamento fiscal já creditou.
    ///
    /// A chave de idempotência do crédito é a ORIGEM = CompraId (origem+item+recebimento colapsam na compra,
    /// pois o crédito é um único fato gerador por compra que cobre todos os itens). Qualquer caminho que for
    /// creditar estoque de uma compra deve antes consultar <see cref="JaCreditadaAsync"/>; se já creditada,
    /// não credita de novo.
    /// </summary>
    public static class EstoqueCreditoCompra
    {
        /// <summary>
        /// True se o estoque desta compra JÁ foi creditado (existe FatoGeradorEstoque com este CompraId).
        /// Usa IgnoreQueryFilters porque a checagem é sobre o fato gerador do próprio tenant já resolvido
        /// pelo chamador — evita depender do filtro global em fluxos de consumidor/outbox.
        /// </summary>
        public static Task<bool> JaCreditadaAsync(ContextEstoque context, Guid compraId, CancellationToken ct)
        {
            if (compraId == Guid.Empty) return Task.FromResult(false);
            return context.FatosGeradoresEstoque
                .IgnoreQueryFilters()
                .AnyAsync(f => f.CompraId == compraId, ct);
        }
    }
}
