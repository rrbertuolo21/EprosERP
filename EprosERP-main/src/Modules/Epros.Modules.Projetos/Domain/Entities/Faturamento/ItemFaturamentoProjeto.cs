using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Faturamento
{
    /// <summary>
    /// Linha faturavel do faturamento de projeto. Origem: EF PRJ-FAT 11.2 (prj_faturamento_projeto_item).
    /// RN-FAT-011/012/013: origem (despesa/hora/marco) deve estar aprovada/justificada.
    /// </summary>
    public class ItemFaturamentoProjeto : EntidadeSaaSBase
    {
        public Guid FaturamentoProjetoId { get; private set; }
        public int Sequencia { get; private set; }
        public decimal? Quantidade { get; private set; }
        public string? Observacao { get; private set; }
        public ETipoItemFaturamento? TipoItem { get; private set; }
        public decimal? ValorUnitario { get; private set; }
        public decimal? ValorTotal { get; private set; }
        public string? OrigemTipo { get; private set; }
        public Guid? OrigemId { get; private set; }

        protected ItemFaturamentoProjeto() { } // EF Core

        public ItemFaturamentoProjeto(
            Guid faturamentoProjetoId,
            int sequencia,
            decimal? quantidade,
            string? observacao,
            ETipoItemFaturamento? tipoItem,
            decimal? valorUnitario,
            decimal? valorTotal,
            string? origemTipo,
            Guid? origemId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ItemFaturamentoProjeto>()
                .Requires()
                .AreNotEquals(faturamentoProjetoId, Guid.Empty, nameof(FaturamentoProjetoId), "O faturamento e obrigatorio. [Origem: ItemFaturamentoProjeto]")
                .IsGreaterThan(sequencia, 0, nameof(Sequencia), "A sequencia do item deve ser positiva. [Origem: ItemFaturamentoProjeto]"));

            FaturamentoProjetoId = faturamentoProjetoId;
            Sequencia = sequencia;
            Quantidade = quantidade;
            Observacao = observacao;
            TipoItem = tipoItem;
            ValorUnitario = valorUnitario;
            // Se valor total nao informado, deriva de quantidade * valor unitario.
            ValorTotal = valorTotal ?? (quantidade.HasValue && valorUnitario.HasValue ? quantidade * valorUnitario : valorTotal);
            OrigemTipo = origemTipo;
            OrigemId = origemId;
        }
    }
}
