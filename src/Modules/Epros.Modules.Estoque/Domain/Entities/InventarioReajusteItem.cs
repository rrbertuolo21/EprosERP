using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Detalhe de reajuste de estoque (EF Inventário §16.4). INV-012: exige produto;
    /// INV-013: pode registrar valor original e valor reajustado.
    /// </summary>
    public class InventarioReajusteItem : EntidadeSaaSBase
    {
        public Guid ReajusteId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal? ValorOriginal { get; private set; }
        public decimal? ValorReajuste { get; private set; }

        public InventarioReajuste? Reajuste { get; private set; }

        protected InventarioReajusteItem() { } // EF Core

        public InventarioReajusteItem(Guid reajusteId, Guid produtoId, decimal? valorOriginal, decimal? valorReajuste, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ReajusteId = reajusteId;
            ProdutoId = produtoId;
            ValorOriginal = valorOriginal;
            ValorReajuste = valorReajuste;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<InventarioReajusteItem>()
                .Requires()
                .IsNotEmpty(ProdutoId, nameof(ProdutoId), "O produto do reajuste é obrigatório [INV-012] [Origem: InventarioReajusteItem]"));
        }
    }
}
