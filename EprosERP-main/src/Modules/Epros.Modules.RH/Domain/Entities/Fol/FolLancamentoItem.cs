using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolLancamentoItem : EntidadeSaaSBase
    {
        public Guid LancamentoId { get; private set; }
        public Guid RubricaId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string Referencia { get; private set; } = string.Empty;
        public decimal? Origem { get; private set; }
        public decimal? Provento { get; private set; }
        public decimal? Desconto { get; private set; }
        public decimal? Valor { get; private set; }
        public string? Observacao { get; private set; }

        protected FolLancamentoItem() { } // EF Core

        public FolLancamentoItem(
            Guid lancamentoId,
            Guid rubricaId,
            string descricao,
            string referencia,
            decimal? origem,
            decimal? provento,
            decimal? desconto,
            decimal? valor,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            LancamentoId = lancamentoId;
            RubricaId = rubricaId;
            Descricao = descricao;
            Referencia = referencia;
            Origem = origem;
            Provento = provento;
            Desconto = desconto;
            Valor = valor;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolLancamentoItem>().Requires();
            contract.AreNotEquals(LancamentoId, Guid.Empty, nameof(LancamentoId), "O campo LancamentoId e obrigatorio.");
            contract.AreNotEquals(RubricaId, Guid.Empty, nameof(RubricaId), "O campo RubricaId e obrigatorio.");
            contract.IsNotNullOrEmpty(Descricao, nameof(Descricao), "O campo Descricao e obrigatorio.");
            contract.IsNotNullOrEmpty(Referencia, nameof(Referencia), "O campo Referencia e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
