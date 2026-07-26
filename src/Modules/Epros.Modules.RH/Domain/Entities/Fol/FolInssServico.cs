using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolInssServico : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;

        protected FolInssServico() { } // EF Core

        public FolInssServico(
            string codigo,
            string nome,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Nome = nome;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolInssServico>().Requires();
            contract.IsNotNullOrEmpty(Codigo, nameof(Codigo), "O campo Codigo e obrigatorio.");
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
