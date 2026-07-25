using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntRelogio : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string Localizacao { get; private set; } = string.Empty;
        public string Marca { get; private set; } = string.Empty;
        public string Fabricante { get; private set; } = string.Empty;
        public string NumeroSerie { get; private set; } = string.Empty;
        public string Utilizacao { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }

        protected PntRelogio() { } // EF Core

        public PntRelogio(
            Guid empresaId,
            string localizacao,
            string marca,
            string fabricante,
            string numeroSerie,
            string utilizacao,
            bool ativo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Localizacao = localizacao;
            Marca = marca;
            Fabricante = fabricante;
            NumeroSerie = numeroSerie;
            Utilizacao = utilizacao;
            Ativo = ativo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntRelogio>().Requires();
            contract.AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "O campo EmpresaId e obrigatorio.");
            contract.IsNotNullOrEmpty(Localizacao, nameof(Localizacao), "O campo Localizacao e obrigatorio.");
            contract.IsNotNullOrEmpty(Marca, nameof(Marca), "O campo Marca e obrigatorio.");
            contract.IsNotNullOrEmpty(Fabricante, nameof(Fabricante), "O campo Fabricante e obrigatorio.");
            contract.IsNotNullOrEmpty(NumeroSerie, nameof(NumeroSerie), "O campo NumeroSerie e obrigatorio.");
            contract.IsNotNullOrEmpty(Utilizacao, nameof(Utilizacao), "O campo Utilizacao e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
