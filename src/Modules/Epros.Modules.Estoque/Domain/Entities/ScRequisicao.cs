using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Solicitação interna de compra (EF Sourcing e Compras §5.9 `sc_requisicao`).
    /// TipoRequisicaoId e ColaboradorId são FKs Guid (colaborador é referência externa — sem navegação cruzada).
    /// SC-056/057/058: gravação recria itens; exclusão remove itens; seleção carrega itens.
    /// </summary>
    public class ScRequisicao : EntidadeSaaSBase
    {
        public Guid TipoRequisicaoId { get; private set; }
        public Guid ColaboradorId { get; private set; }
        public DateTime? DataRequisicao { get; private set; }

        // Navegação intra-módulo
        public ScTipoRequisicao? TipoRequisicao { get; private set; }
        public ICollection<ScRequisicaoItem> Itens { get; private set; } = new List<ScRequisicaoItem>();

        protected ScRequisicao() { } // EF Core

        public ScRequisicao(Guid tipoRequisicaoId, Guid colaboradorId, DateTime? dataRequisicao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TipoRequisicaoId = tipoRequisicaoId;
            ColaboradorId = colaboradorId;
            DataRequisicao = dataRequisicao;
            Validar();
        }

        public void Alterar(Guid tipoRequisicaoId, Guid colaboradorId, DateTime? dataRequisicao, string alteradoPor)
        {
            TipoRequisicaoId = tipoRequisicaoId;
            ColaboradorId = colaboradorId;
            DataRequisicao = dataRequisicao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void AdicionarItem(ScRequisicaoItem item) => Itens.Add(item);

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ScRequisicao>()
                .Requires()
                .AreNotEquals(TipoRequisicaoId, Guid.Empty, nameof(TipoRequisicaoId), "O tipo de requisição é obrigatório [Origem: ScRequisicao]")
                .AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O colaborador solicitante é obrigatório [Origem: ScRequisicao]"));
        }
    }
}
