using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PLN, tabela rh_pln_turma). Fidelidade campo a campo.</summary>
    public partial class PlnTurma : EntidadeSaaSBase
    {
        public string? Codigo { get; private set; }
        public string? Nome { get; private set; }
        public Guid? EscalaId { get; private set; }
        public bool Ativo { get; private set; }

        protected PlnTurma() { } // EF Core

        public PlnTurma(
            string? codigo,
            string? nome,
            Guid? escalaId,
            bool ativo,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Nome = nome;
            EscalaId = escalaId;
            Ativo = ativo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PlnTurma>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
