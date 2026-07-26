using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Encerramento
{
    /// <summary>
    /// Item/checklist do encerramento. Origem: EF PRJ-ENC 11.2 (prj_enc_encerramento_item).
    /// Sequencia ordena o checklist; quantidade e observacao sao opcionais.
    /// </summary>
    public class ItemEncerramento : EntidadeSaaSBase
    {
        public Guid EncerramentoId { get; private set; }
        public int Sequencia { get; private set; }
        public decimal? Quantidade { get; private set; }
        public string? Observacao { get; private set; }

        protected ItemEncerramento() { } // EF Core

        public ItemEncerramento(
            Guid encerramentoId,
            int sequencia,
            decimal? quantidade,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ItemEncerramento>()
                .Requires()
                .AreNotEquals(encerramentoId, Guid.Empty, nameof(EncerramentoId), "O encerramento e obrigatorio. [Origem: ItemEncerramento]")
                .IsGreaterOrEqualsThan(sequencia, 1, nameof(Sequencia), "A sequencia do item deve ser maior ou igual a 1. [Origem: ItemEncerramento]"));

            EncerramentoId = encerramentoId;
            Sequencia = sequencia;
            Quantidade = quantidade;
            Observacao = observacao;
        }
    }
}
