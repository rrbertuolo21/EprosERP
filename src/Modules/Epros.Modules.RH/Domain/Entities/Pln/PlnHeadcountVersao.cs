using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PLN, tabela rh_pln_headcount_versao). Fidelidade campo a campo.</summary>
    public partial class PlnHeadcountVersao : EntidadeSaaSBase
    {
        public int? Ano { get; private set; }
        public string? Cenario { get; private set; }
        public string? Status { get; private set; }
        public string? Observacao { get; private set; }

        protected PlnHeadcountVersao() { } // EF Core

        public PlnHeadcountVersao(
            int? ano,
            string? cenario,
            string? status,
            string? observacao,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Ano = ano;
            Cenario = cenario;
            Status = status;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PlnHeadcountVersao>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
