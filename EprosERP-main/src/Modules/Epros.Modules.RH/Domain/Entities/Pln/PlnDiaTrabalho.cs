using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PLN, tabela rh_pln_dia_trabalho). Fidelidade campo a campo.</summary>
    public partial class PlnDiaTrabalho : EntidadeSaaSBase
    {
        public string DiaSemana { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }
        public string? Observacao { get; private set; }

        protected PlnDiaTrabalho() { } // EF Core

        public PlnDiaTrabalho(
            string diaSemana,
            bool ativo,
            string? observacao,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            DiaSemana = diaSemana;
            Ativo = ativo;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PlnDiaTrabalho>().Requires();
            contract.IsNotNullOrEmpty(DiaSemana, nameof(DiaSemana), "O campo DiaSemana e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
