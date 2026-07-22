using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    public class PlanoDeContasFinanceiroItem : EntidadeSaaSBase
    {
        /// <summary>Sequência legada (SequenciaTenantId) — somente exibição/UX. Não substitui o Id (Guid).</summary>
        public long? SequenciaExibicao { get; private set; }

        public Guid PlanoDeContasFinanceiroId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public ETipoDetalhamento TipoDetalhamento { get; private set; }
        public bool MovimentaCaixa { get; private set; }

        public PlanoDeContasFinanceiro PlanoDeContasFinanceiro { get; private set; } = null!;

        protected PlanoDeContasFinanceiroItem() { } // EF Core

        public PlanoDeContasFinanceiroItem(Guid planoDeContasFinanceiroId, string codigo, string descricao, ETipoDetalhamento tipoDetalhamento, bool movimentaCaixa, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PlanoDeContasFinanceiroId = planoDeContasFinanceiroId;
            Codigo = codigo;
            Descricao = descricao;
            TipoDetalhamento = tipoDetalhamento;
            MovimentaCaixa = movimentaCaixa;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<PlanoDeContasFinanceiroItem>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código é obrigatório.")
                .IsLowerOrEqualsThan(Codigo?.Length ?? 0, 50, nameof(Codigo), "O campo Codigo deve ter no máximo 50 caracteres [Origem: PlanoDeContasFinanceiroItem]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição é obrigatória.")
                .IsLowerOrEqualsThan(Descricao?.Length ?? 0, 100, nameof(Descricao), "O campo Descricao deve ter no máximo 100 caracteres [Origem: PlanoDeContasFinanceiroItem]")
            );
        }

        public void Alterar(string codigo, string descricao, ETipoDetalhamento tipoDetalhamento, bool movimentaCaixa, string usuario)
        {
            Codigo = codigo;
            Descricao = descricao;
            TipoDetalhamento = tipoDetalhamento;
            MovimentaCaixa = movimentaCaixa;
            MarcarAlterado(usuario);
            Validar();
        }
    }
}
