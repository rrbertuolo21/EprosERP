using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_premio). Fidelidade campo a campo.</summary>
    public partial class DevPremio : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid? TipoPremioId { get; private set; }
        public DateTime? DataPremio { get; private set; }
        public string? Descricao { get; private set; }
        public string? Certificado { get; private set; }

        protected DevPremio() { } // EF Core

        public DevPremio(
            Guid colaboradorId,
            Guid? tipoPremioId,
            DateTime? dataPremio,
            string? descricao,
            string? certificado,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            TipoPremioId = tipoPremioId;
            DataPremio = dataPremio;
            Descricao = descricao;
            Certificado = certificado;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<DevPremio>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
