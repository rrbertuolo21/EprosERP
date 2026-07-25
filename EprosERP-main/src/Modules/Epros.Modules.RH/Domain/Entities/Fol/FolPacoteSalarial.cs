using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolPacoteSalarial : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }
        public decimal? ValorTotal { get; private set; }
        public string? Narrativa { get; private set; }

        protected FolPacoteSalarial() { } // EF Core

        public FolPacoteSalarial(
            string nome,
            bool ativo,
            decimal? valorTotal,
            string? narrativa,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Ativo = ativo;
            ValorTotal = valorTotal;
            Narrativa = narrativa;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolPacoteSalarial>().Requires();
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
