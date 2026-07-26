using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Item (baixa/parcela recebida) de um título a receber do agregado <see cref="ContasAReceber"/>.
    /// Porte fiel do legado Financeiros/ContasAReceberItem.
    /// </summary>
    public class ContasAReceberItem : EntidadeSaaSBase
    {
        public Guid ContasAReceberId { get; private set; }
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
        public decimal ValorAReceber { get; private set; }
        public DateTime DataRecebimento { get; private set; }

        // EF (navegação intra-módulo)
        public ContasAReceber ContasAReceber { get; private set; } = null!;
        public PlanoDeContasFinanceiroItem PlanoDeContasFinanceiroItem { get; private set; } = null!;
        public ContaBancaria? ContaBancaria { get; private set; }

        protected ContasAReceberItem() { } // EF Core

        public ContasAReceberItem(Guid contasAReceberId, Guid planoDeContasFinanceiroItemId, Guid? contaBancariaId,
                                  ETipoPagamento tipoPagamento, decimal valorParcela, decimal valorPago,
                                  decimal valorDesconto, decimal valorMulta, decimal valorJuros, decimal valorAcrescimo,
                                  DateTime dataRecebimento, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            ContasAReceberId = contasAReceberId;
            PlanoDeContasFinanceiroItemId = planoDeContasFinanceiroItemId;
            ContaBancariaId = contaBancariaId;
            TipoPagamento = tipoPagamento;
            ValorParcela = valorParcela;
            ValorPago = valorPago;
            ValorDesconto = valorDesconto;
            ValorMulta = valorMulta;
            ValorJuros = valorJuros;
            ValorAcrescimo = valorAcrescimo;
            DataRecebimento = dataRecebimento;

            Validar();

            CalcularValorAReceber();
            CalcularValorTroco();
        }

        public void Validar()
        {
            AddNotifications(new Contract<ContasAReceberItem>()
                .Requires()
                .AreNotEquals(PlanoDeContasFinanceiroItemId, Guid.Empty, nameof(PlanoDeContasFinanceiroItemId), "O campo PlanoDeContasFinanceiroItemId é obrigatório [Origem: ContasAReceberItem]")
                .IsTrue(Enum.IsDefined(typeof(ETipoPagamento), TipoPagamento), nameof(TipoPagamento), "TipoPagamento não consta na lista [Origem: ContasAReceberItem]")
            );
        }

        public void Alterar(Guid planoDeContasFinanceiroItemId, Guid? contaBancariaId, ETipoPagamento tipoPagamento,
                            decimal valorParcela, decimal valorPago, decimal valorDesconto, decimal valorMulta,
                            decimal valorJuros, decimal valorAcrescimo, DateTime dataRecebimento, string alteradoPor)
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
            DataRecebimento = dataRecebimento;
            MarcarAlterado(alteradoPor);

            Validar();

            CalcularValorAReceber();
            CalcularValorTroco();
        }

        public void CalcularValorAReceber()
        {
            ValorAReceber = (ValorParcela + ValorMulta + ValorJuros + ValorAcrescimo) - ValorDesconto;
        }

        public void CalcularValorTroco()
        {
            if (ValorPago > ValorAReceber)
            {
                ValorTroco = ValorPago - ValorAReceber;
            }
        }
    }
}
