using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Financeiro.Domain.Entities;

namespace Epros.Modules.Financeiro.Domain.Services
{
    /// <summary>Um par débito×crédito de uma regra de contabilização (uma conta debitada, uma creditada, um valor).</summary>
    public sealed record PernaContabil(Guid ContaDebitoId, Guid ContaCreditoId, decimal Valor);

    /// <summary>
    /// Regra de contabilização de um fato/evento: como o evento vira débito(s) × crédito(s).
    /// O MAPEAMENTO evento→conta do plano é config do cliente/contador (// valida-contador) e chega
    /// aqui pronto; o motor só garante a partida dobrada.
    /// </summary>
    public sealed record RegraContabilizacao(string Historico, IReadOnlyList<PernaContabil> Pernas);

    /// <summary>
    /// Motor evento→partida (contabilização automática) do FIN-CGL. Dado um fato gerador de integração
    /// (compra lançada, venda faturada, folha processada, projeto faturado, depreciação…), o valor e
    /// uma <see cref="RegraContabilizacao"/>, produz um <see cref="LancamentoContabil"/> BALANCEADO por
    /// partida dobrada — débito = crédito (cita Negocio-acumulado/contabil).
    ///
    /// O lançamento nasce em Rascunho (não impacta saldos até Confirmar), preservando o ledger
    /// append-only. A escolha das CONTAS é parametrização contábil (// valida-contador); o motor nunca
    /// inventa conta nem desbalanceia.
    /// </summary>
    public static class MotorContabilizacao
    {
        /// <summary>Partida simples: um débito e um crédito de mesmo valor.</summary>
        public static LancamentoContabil GerarPartidaSimples(
            Guid? periodoContabilId, string? numeroLancamento, DateTime data,
            Guid contaDebitoId, Guid contaCreditoId, decimal valor,
            string historico, string tenantId, string usuario)
            => Gerar(periodoContabilId, numeroLancamento, data,
                new RegraContabilizacao(historico, new[] { new PernaContabil(contaDebitoId, contaCreditoId, valor) }),
                tenantId, usuario);

        /// <summary>
        /// Gera o lançamento a partir da regra (pode ter várias pernas). Cada perna adiciona uma linha
        /// de débito e uma de crédito de igual valor, mantendo o total balanceado por construção.
        /// </summary>
        public static LancamentoContabil Gerar(
            Guid? periodoContabilId, string? numeroLancamento, DateTime data,
            RegraContabilizacao regra, string tenantId, string usuario)
        {
            if (regra?.Pernas == null || regra.Pernas.Count == 0)
                throw new ArgumentException("Regra de contabilização sem pernas.", nameof(regra));
            if (regra.Pernas.Any(p => p.Valor <= 0m))
                throw new ArgumentException("Valor de contabilização deve ser maior que zero.", nameof(regra));

            var lanc = new LancamentoContabil(periodoContabilId, numeroLancamento, data, regra.Historico, tenantId, usuario);
            foreach (var perna in regra.Pernas)
            {
                lanc.AdicionarLinha(perna.ContaDebitoId, perna.Valor, 0m, regra.Historico, usuario);
                lanc.AdicionarLinha(perna.ContaCreditoId, 0m, perna.Valor, regra.Historico, usuario);
            }
            return lanc;
        }
    }
}
