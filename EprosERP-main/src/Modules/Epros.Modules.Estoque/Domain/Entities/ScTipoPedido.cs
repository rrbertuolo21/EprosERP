using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Classificação do pedido de compra (EF Sourcing e Compras §4 `sc_tipo_pedido`).
    /// </summary>
    public class ScTipoPedido : EntidadeSaaSBase
    {
        public string Descricao { get; private set; } = string.Empty;
        public bool Ativo { get; private set; } = true;

        protected ScTipoPedido() { } // EF Core

        public ScTipoPedido(string descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Descricao = descricao ?? string.Empty;
            Ativo = true;
            Validar();
        }

        public void Alterar(string descricao, bool ativo, string alteradoPor)
        {
            Descricao = descricao ?? string.Empty;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ScTipoPedido>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição do tipo de pedido é obrigatória [Origem: ScTipoPedido]"));
        }
    }
}
