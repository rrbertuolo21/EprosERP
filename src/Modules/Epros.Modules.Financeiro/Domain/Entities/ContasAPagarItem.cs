using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Item (baixa/parcela paga) de um título a pagar do agregado <see cref="ContasAPagar"/>.
    /// Porte fiel do legado Financeiros/ContasAPagarItem.
    /// </summary>
    public class ContasAPagarItem : EntidadeSaaSBase
    {
        public Guid ContasAPagarId { get; private set; }
        public Guid PlanoDeContasFinanceiroItemId { get; private set; }
        public Guid? ContaBancariaId { get; private set; }
        public ETipoPagamento TipoPagamento { get; private set; }
        public decimal ValorParcela { get; private set; }
        public decimal ValorPago { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public decimal ValorMulta { get; private set; }
        public decimal ValorJuros { get; private set; }
        public decimal ValorTroco { get; private set; }
        public decimal ValorAcrescimo { get; private set; }
        public decimal ValorAPagar { get; private set; }
        public DateTime DataPagamento { get; private set; }

        // EF (navegação intra-módulo)
        public ContasAPagar ContasAPagar { get; private set; } = null!;
        public PlanoDeContasFinanceiroItem PlanoDeContasFinanceiroItem { get; private set; } = null!;
        public ContaBancaria? ContaBancaria { get; private set; }

        protected ContasAPagarItem() { } // EF Core

        public ContasAPagarItem(Guid contasAPagarId, Guid planoDeContasFinanceiroItemId, Guid? contaBancariaId,
                                ETipoPagamento tipoPagamento, decimal valorParcela, decimal valorPago,
                                decimal valorDesconto, decimal valorMulta, decimal valorJuros, decimal valorAcrescimo,
                                DateTime dataPagamento, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            ContasAPagarId = contasAPagarId;
            PlanoDeContasFinanceiroItemId = planoDeContasFinanceiroItemId;
            ContaBancariaId = contaBancariaId;
            TipoPagamento = tipoPagamento;
            ValorParcela = valorParcela;
            ValorPago = valorPago;
            ValorDesconto = valorDesconto;
            ValorMulta = valorMulta;
            ValorJuros = valorJuros;
            ValorAcrescimo = valorAcrescimo;
            DataPagamento = dataPagamento;

            Validar();

            CalcularValorAPagar();
            CalcularValorTroco();
        }

        public void Validar()
        {
            AddNotifications(new Contract<ContasAPagarItem>()
                .Requires()
                .AreNotEquals(PlanoDeContasFinanceiroItemId, Guid.Empty, nameof(PlanoDeContasFinanceiroItemId), "O campo PlanoDeContasFinanceiroItemId é obrigatório [Origem: ContasAPagarItem]")
                .IsTrue(Enum.IsDefined(typeof(ETipoPagamento), TipoPagamento), nameof(TipoPagamento), "TipoPagamento não consta na lista [Origem: ContasAPagarItem]")
            );
        }

        public void Alterar(Guid planoDeContasFinanceiroItemId, Guid? contaBancariaId, ETipoPagamento tipoPagamento,
                            decimal valorParcela, decimal valorPago, decimal valorDesconto, decimal valorMulta,
                            decimal valorJuros, decimal valorAcrescimo, DateTime dataPagamento, string alteradoPor)
        {
            PlanoDeContasFinanceiroItemId = planoDeContasFinanceiroItemId;
            ContaBancariaId = contaBancariaId;
            TipoPagamento = tipoPagamento;
            ValorParcela = valorParcela;
            ValorPago = valorPago;
            ValorDesconto = valorDesconto;
            ValorMulta = valorMulta;
            ValorJuros = valorJuros;
            ValorAcrescimo = valorAcrescimo;
            DataPagamento = dataPagamento;
            MarcarAlterado(alteradoPor);

            Validar();

            CalcularValorAPagar();
            CalcularValorTroco();
        }

        public void CalcularValorAPagar()
        {
            ValorAPagar = (ValorParcela + ValorMulta + ValorJuros + ValorAcrescimo) - ValorDesconto;
        }

        public void CalcularValorTroco()
        {
            if (ValorPago > ValorAPagar)
            {
                ValorTroco = ValorPago - ValorAPagar;
            }
        }
    }
}
