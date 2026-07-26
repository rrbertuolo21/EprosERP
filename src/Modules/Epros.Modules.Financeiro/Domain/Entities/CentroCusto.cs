using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Centro de custo em árvore (EF FIN-CMG §11.1 cmg_centro_custo).
    /// Submódulo de evolução — sobe desabilitado (ABAC nega por padrão). Isolamento por tenant via ContextBase.
    /// </summary>
    public class CentroCusto : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public Guid? PaiId { get; private set; }
        public EEstadoCentroCusto Estado { get; private set; } = EEstadoCentroCusto.Ativo;

        public CentroCusto? Pai { get; private set; }

        protected CentroCusto() { } // EF Core

        public CentroCusto(string codigo, string descricao, Guid? paiId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            PaiId = paiId;
            Estado = EEstadoCentroCusto.Ativo;
            Validar();
        }

        public void Alterar(string codigo, string descricao, Guid? paiId, string usuario)
        {
            Codigo = codigo;
            Descricao = descricao;
            PaiId = paiId;
            MarcarAlterado(usuario);
            Validar();
        }

        public void DefinirEstado(EEstadoCentroCusto estado, string usuario) { Estado = estado; MarcarAlterado(usuario); }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<CentroCusto>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código do centro de custo é obrigatório [Origem: CentroCusto]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição do centro de custo é obrigatória [Origem: CentroCusto]"));
            if (PaiId.HasValue && PaiId.Value == Id)
                AddNotification(nameof(PaiId), "O centro de custo não pode ser pai de si mesmo.");
        }
    }
}
