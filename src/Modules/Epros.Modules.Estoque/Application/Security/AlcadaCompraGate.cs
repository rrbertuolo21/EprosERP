using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Estoque.Application.Security
{
    /// <summary>
    /// CD3 / SRC-008 — gate de alçada de compras. Uma compra cuja origem está sob workflow de aprovação
    /// (ComprasPedidoAprovacao) só pode ser EFETIVADA quando o pedido de aprovação estiver APROVADO.
    /// Antes, nenhum handler de lançamento/faturamento de compra verificava a aprovação (P1).
    ///
    /// A checagem incide sobre a origem informada (AprovacaoOrigemId = id do pedido de compra/compra sob
    /// alçada). Quando a origem não é informada, não há referência de alçada a validar — o fluxo segue
    /// (o wiring do front que carrega a referência da origem é resíduo — ver DECISOES-PENDENTES-RAFAEL).
    /// </summary>
    public static class AlcadaCompraGate
    {
        /// <summary>
        /// Retorna erro (bloqueia a efetivação) quando a origem tem um pedido de aprovação de alçada que
        /// NÃO está aprovado (pendente/reprovado/cancelado). Retorna null quando pode prosseguir.
        /// </summary>
        public static async Task<CommandResult?> GarantirAprovadaAsync(
            ContextEstoque context,
            Guid? aprovacaoOrigemId,
            CancellationToken cancellationToken)
        {
            if (!aprovacaoOrigemId.HasValue || aprovacaoOrigemId.Value == Guid.Empty)
                return null;

            var pedido = await context.ComprasPedidosAprovacao.AsNoTracking()
                .Where(p => p.OrigemId == aprovacaoOrigemId.Value && p.DeletadoEm == null)
                .OrderByDescending(p => p.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);

            // Origem declarada sob alçada, mas sem workflow: não pode efetivar sem passar pela aprovação.
            if (pedido == null)
                return CommandResult.Falha(
                    "Compra sob alçada sem pedido de aprovação registrado [CD3/SRC-008].",
                    mensagem: "Efetivação bloqueada por alçada de compras.", block: true);

            if (pedido.Status == EStatusPedidoAprovacaoCompra.Reprovado)
                return CommandResult.Falha(
                    "Compra bloqueada: aprovação de alçada REPROVADA [SRC-008].",
                    mensagem: "Efetivação bloqueada por alçada de compras.", block: true);

            if (!pedido.FoiAprovado())
                return CommandResult.Falha(
                    "Compra bloqueada: aprovação de alçada ainda pendente [CD3/SRC-008].",
                    mensagem: "Efetivação bloqueada por alçada de compras.", block: true);

            return null;
        }
    }
}
