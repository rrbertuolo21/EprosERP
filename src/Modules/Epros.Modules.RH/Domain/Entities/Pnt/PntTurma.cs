using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntTurma : EntidadeSaaSBase
    {
        public Guid? EscalaId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;

        protected PntTurma() { } // EF Core

        public PntTurma(
            Guid? escalaId,
            string codigo,
            string nome,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EscalaId = escalaId;
            Codigo = codigo;
            Nome = nome;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntTurma>().Requires();
            contract.IsNotNullOrEmpty(Codigo, nameof(Codigo), "O campo Codigo e obrigatorio.");
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
