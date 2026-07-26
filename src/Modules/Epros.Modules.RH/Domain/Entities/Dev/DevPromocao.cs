using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_promocao). Fidelidade campo a campo.</summary>
    public partial class DevPromocao : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid? FilialAnteriorId { get; private set; }
        public Guid? DepartamentoAnteriorId { get; private set; }
        public Guid? CargoAnteriorId { get; private set; }
        public Guid? FilialAtualId { get; private set; }
        public Guid? DepartamentoAtualId { get; private set; }
        public Guid? CargoAtualId { get; private set; }
        public DateTime? DataEfetiva { get; private set; }
        public string? Motivo { get; private set; }
        public string? Documento { get; private set; }
        public string Status { get; private set; } = string.Empty;

        protected DevPromocao() { } // EF Core

        public DevPromocao(
            Guid colaboradorId,
            Guid? filialAnteriorId,
            Guid? departamentoAnteriorId,
            Guid? cargoAnteriorId,
            Guid? filialAtualId,
            Guid? departamentoAtualId,
            Guid? cargoAtualId,
            DateTime? dataEfetiva,
            string? motivo,
            string? documento,
            string status,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            FilialAnteriorId = filialAnteriorId;
            DepartamentoAnteriorId = departamentoAnteriorId;
            CargoAnteriorId = cargoAnteriorId;
            FilialAtualId = filialAtualId;
            DepartamentoAtualId = departamentoAtualId;
            CargoAtualId = cargoAtualId;
            DataEfetiva = dataEfetiva;
            Motivo = motivo;
            Documento = documento;
            Status = status;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<DevPromocao>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
