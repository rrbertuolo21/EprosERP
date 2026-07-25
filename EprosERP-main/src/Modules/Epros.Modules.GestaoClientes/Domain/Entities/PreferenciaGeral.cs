using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PreferenciaGeral : EntidadeSaaSBase
    {
        public bool ShowCurrency { get; private set; }
        public bool NegativeCash { get; private set; }
        public bool NegativeStock { get; private set; }
        public StockCalculationMode StockCalculationMode { get; private set; }
        public bool CreditLimit { get; private set; }
        public bool Discount { get; private set; }
        public bool VatOnPurchase { get; private set; }
        public bool VatOnSales { get; private set; }

        protected PreferenciaGeral() { } // EF Core

        public PreferenciaGeral(
            bool showCurrency,
            bool negativeCash,
            bool negativeStock,
            StockCalculationMode stockCalculationMode,
            bool creditLimit,
            bool discount,
            bool vatOnPurchase,
            bool vatOnSales,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PreferenciaGeral>()
                .Requires()
                .IsTrue(Enum.IsDefined(typeof(StockCalculationMode), stockCalculationMode), nameof(StockCalculationMode), "Modo de cálculo de estoque inválido.")
            );

            ShowCurrency = showCurrency;
            NegativeCash = negativeCash;
            NegativeStock = negativeStock;
            StockCalculationMode = stockCalculationMode;
            CreditLimit = creditLimit;
            Discount = discount;
            VatOnPurchase = vatOnPurchase;
            VatOnSales = vatOnSales;
        }

        public void Atualizar(
            bool showCurrency,
            bool negativeCash,
            bool negativeStock,
            StockCalculationMode stockCalculationMode,
            bool creditLimit,
            bool discount,
            bool vatOnPurchase,
            bool vatOnSales,
            string alteradoPor)
        {
            AddNotifications(new Contract<PreferenciaGeral>()
                .Requires()
                .IsTrue(Enum.IsDefined(typeof(StockCalculationMode), stockCalculationMode), nameof(StockCalculationMode), "Modo de cálculo de estoque inválido.")
            );

            if (IsValid)
            {
                ShowCurrency = showCurrency;
                NegativeCash = negativeCash;
                NegativeStock = negativeStock;
                StockCalculationMode = stockCalculationMode;
                CreditLimit = creditLimit;
                Discount = discount;
                VatOnPurchase = vatOnPurchase;
                VatOnSales = vatOnSales;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
