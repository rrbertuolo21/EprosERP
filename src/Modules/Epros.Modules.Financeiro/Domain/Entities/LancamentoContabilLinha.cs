using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Linha de débito ou crédito de um lançamento contábil (EF FIN-CGL §11 cgl_lancamento_linha).
    /// </summary>
    public class LancamentoContabilLinha : EntidadeSaaSBase
    {
        public Guid LancamentoContabilId { get; private set; }
        public Guid ContaContabilId { get; private set; }
        public decimal Debito { get; private set; }
        public decimal Credito { get; private set; }
        public string? Historico { get; private set; }

        public LancamentoContabil LancamentoContabil { get; private set; } = null!;

        protected LancamentoContabilLinha() { } // EF Core

        public LancamentoContabilLinha(Guid lancamentoContabilId, Guid contaContabilId, decimal debito, decimal credito, string? historico, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            LancamentoContabilId = lancamentoContabilId;
            ContaContabilId = contaContabilId;
            Debito = debito;
            Credito = credito;
            Historico = historico;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LancamentoContabilLinha>()
                .Requires()
                .AreNotEquals(ContaContabilId, Guid.Empty, nameof(ContaContabilId), "A conta contábil da linha é obrigatória.")
                .IsGreaterOrEqualsThan(Debito, 0m, nameof(Debito), "O débito não pode ser negativo.")
                .IsGreaterOrEqualsThan(Credito, 0m, nameof(Credito), "O crédito não pode ser negativo.")
            );
        }
    }
}
