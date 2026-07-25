using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    public class ListaMateriais : EntidadeSaaSBase
    {
        public string ProdutoAcabadoSku { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public string Versao { get; private set; } = "1.0";
        public bool Ativa { get; private set; } = true;
        public List<BomItem> Itens { get; private set; } = new();

        protected ListaMateriais() { } // EF Core

        public ListaMateriais(string produtoAcabadoSku, string descricao, string versao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ListaMateriais>()
                .Requires()
                .IsNotNullOrEmpty(produtoAcabadoSku, nameof(ProdutoAcabadoSku), "O SKU do produto acabado é obrigatório.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição é obrigatória.")
                .IsNotNullOrEmpty(versao, nameof(Versao), "A versão é obrigatória.")
            );

            ProdutoAcabadoSku = produtoAcabadoSku;
            Descricao = descricao;
            Versao = versao;
            Ativa = true;
        }

        public void AdicionarItem(string insumoSku, decimal quantidadeNecessaria, string unidadeMedida, string criadoPor)
        {
            var item = new BomItem(Id, insumoSku, quantidadeNecessaria, unidadeMedida, TenantId, criadoPor);
            if (!item.IsValid)
            {
                AddNotifications(item.Notifications);
                return;
            }
            Itens.Add(item);
        }

        public void Inativar(string usuario)
        {
            Ativa = false;
            MarcarAlterado(usuario);
        }
    }
}
