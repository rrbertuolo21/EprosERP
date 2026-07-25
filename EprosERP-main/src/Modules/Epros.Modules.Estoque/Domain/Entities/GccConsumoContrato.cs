using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Consumo de contrato de compra por pedido/compra (EF Gestão de Contratos de Compra §16.4
    /// `gcc_consumo_contrato`). GCC-007: compra referencia contrato para consumo de saldo.
    /// CompraId é referência externa por FK Guid (módulo Compras). Modelo proposto por autoria (§22).
    /// </summary>
    public class GccConsumoContrato : EntidadeSaaSBase
    {
        public Guid ContratoCompraId { get; private set; }
        public Guid ContratoCompraItemId { get; private set; }
        public Guid? CompraId { get; private set; }
        public decimal QuantidadeConsumida { get; private set; }
        public decimal ValorConsumido { get; private set; }
        public DateTime DataConsumo { get; private set; }

        // Navegação intra-módulo
        public GccContratoCompra? Contrato { get; private set; }

        protected GccConsumoContrato() { } // EF Core

        public GccConsumoContrato(Guid contratoCompraId, Guid contratoCompraItemId, Guid? compraId, decimal quantidadeConsumida, decimal valorConsumido, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ContratoCompraId = contratoCompraId;
            ContratoCompraItemId = contratoCompraItemId;
            CompraId = compraId;
            QuantidadeConsumida = quantidadeConsumida;
            ValorConsumido = valorConsumido;
            DataConsumo = DateTime.UtcNow;
            Validar();
        }

        public void Validar()
        {
            Clear();
            if (ContratoCompraId == Guid.Empty)
                AddNotification("ContratoCompraId", "O contrato é obrigatório [Origem: GccConsumoContrato]");
            if (ContratoCompraItemId == Guid.Empty)
                AddNotification("ContratoCompraItemId", "O item contratual é obrigatório [Origem: GccConsumoContrato]");
            if (QuantidadeConsumida <= 0m)
                AddNotification("QuantidadeConsumida", "A quantidade consumida deve ser maior que zero [Origem: GccConsumoContrato]");
        }
    }
}
