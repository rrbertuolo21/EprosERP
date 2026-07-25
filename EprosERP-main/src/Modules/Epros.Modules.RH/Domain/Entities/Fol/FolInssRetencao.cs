using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolInssRetencao : EntidadeSaaSBase
    {
        public Guid? InssId { get; private set; }
        public Guid ServicoInssId { get; private set; }
        public decimal? ValorMensal { get; private set; }
        public decimal? Valor13 { get; private set; }

        protected FolInssRetencao() { } // EF Core

        public FolInssRetencao(
            Guid? inssId,
            Guid servicoInssId,
            decimal? valorMensal,
            decimal? valor13,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            InssId = inssId;
            ServicoInssId = servicoInssId;
            ValorMensal = valorMensal;
            Valor13 = valor13;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolInssRetencao>().Requires();
            contract.AreNotEquals(ServicoInssId, Guid.Empty, nameof(ServicoInssId), "O campo ServicoInssId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
