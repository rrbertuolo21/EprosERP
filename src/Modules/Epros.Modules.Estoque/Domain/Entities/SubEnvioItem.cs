using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Item enviado na remessa (EF Subcontratação §7.3 `sub_envio_item`). SUB-003: registra produtos e
    /// quantidades remetidas. ProdutoId/LoteId/LocalOrigemId são referências externas por FK Guid.
    /// </summary>
    public class SubEnvioItem : EntidadeSaaSBase
    {
        public Guid EnvioId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal QuantidadeEnviada { get; private set; }
        public Guid? LoteId { get; private set; }
        public Guid? LocalOrigemId { get; private set; }

        // Navegação intra-módulo
        public SubEnvio? Envio { get; private set; }

        protected SubEnvioItem() { } // EF Core

        public SubEnvioItem(Guid envioId, Guid produtoId, decimal quantidadeEnviada, Guid? loteId, Guid? localOrigemId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EnvioId = envioId;
            ProdutoId = produtoId;
            QuantidadeEnviada = quantidadeEnviada;
            LoteId = loteId;
            LocalOrigemId = localOrigemId;
            Validar();
        }

        public void Validar()
        {
            Clear();
            if (ProdutoId == Guid.Empty)
                AddNotification("ProdutoId", "O produto/material enviado é obrigatório [SUB-003] [Origem: SubEnvioItem]");
            if (QuantidadeEnviada <= 0m)
                AddNotification("QuantidadeEnviada", "A quantidade enviada deve ser maior que zero [SUB-003] [Origem: SubEnvioItem]");
        }
    }
}
