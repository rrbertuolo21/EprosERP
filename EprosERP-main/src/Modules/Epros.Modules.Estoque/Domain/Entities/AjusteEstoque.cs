using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Cabeçalho de ajuste de estoque (EF Movimentação Manual e Ajustes §15.10).
    /// LocalId referencia o módulo WMS por FK Guid (sem navegação cruzada).
    /// MVM-025 / VAL-MVM-015: ajuste exige motivo (Observacao) e auditoria.
    /// </summary>
    public class AjusteEstoque : EntidadeSaaSBase
    {
        public Guid? LocalId { get; private set; }
        public DateTime DataAjuste { get; private set; }
        public ETipoAjusteEstoque TipoAjuste { get; private set; }
        public decimal? ValorTotal { get; private set; }
        public decimal? ValorRecuperado { get; private set; }
        public string Observacao { get; private set; } = string.Empty;
        public EStatusRegistroEstoque Situacao { get; private set; } = EStatusRegistroEstoque.Rascunho;

        // Navegação intra-módulo
        public ICollection<AjusteEstoqueItem> Itens { get; private set; } = new List<AjusteEstoqueItem>();

        protected AjusteEstoque() { } // EF Core

        public AjusteEstoque(Guid? localId, DateTime dataAjuste, ETipoAjusteEstoque tipoAjuste, decimal? valorTotal, decimal? valorRecuperado, string observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            LocalId = localId;
            DataAjuste = dataAjuste;
            TipoAjuste = tipoAjuste;
            ValorTotal = valorTotal;
            ValorRecuperado = valorRecuperado;
            Observacao = observacao ?? string.Empty;
            Situacao = EStatusRegistroEstoque.Rascunho;
            Validar();
        }

        /// <summary>MVM-025: motivo (observação) obrigatório para ajuste direto de saldo.</summary>
        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<AjusteEstoque>()
                .Requires()
                .IsNotNullOrEmpty(Observacao, nameof(Observacao), "O motivo/observação do ajuste é obrigatório [MVM-025] [Origem: AjusteEstoque]"));
        }

        public void AdicionarItem(AjusteEstoqueItem item) => Itens.Add(item);

        public void Aplicar(string usuario)
        {
            Situacao = EStatusRegistroEstoque.Aplicado;
            MarcarAlterado(usuario);
        }

        public void Estornar(string usuario)
        {
            Situacao = EStatusRegistroEstoque.Estornado;
            MarcarAlterado(usuario);
        }
    }
}
