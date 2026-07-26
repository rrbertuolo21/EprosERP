using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_transferencia). Fidelidade campo a campo.</summary>
    public partial class WfmTransferencia : EntidadeSaaSBase
    {
        public Guid? ColaboradorId { get; private set; }
        public DateTime? DataTransferencia { get; private set; }
        public DateTime? DataEfetiva { get; private set; }
        public string? Motivo { get; private set; }
        public string? Status { get; private set; }
        public string? Documento { get; private set; }
        public Guid? FilialOrigemId { get; private set; }
        public Guid? DepartamentoOrigemId { get; private set; }
        public Guid? CargoOrigemId { get; private set; }
        public Guid? FilialDestinoId { get; private set; }
        public Guid? DepartamentoDestinoId { get; private set; }
        public Guid? CargoDestinoId { get; private set; }
        public Guid? AprovadoPorId { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected WfmTransferencia() { } // EF Core

        public WfmTransferencia(
            Guid? colaboradorId,
            DateTime? dataTransferencia,
            DateTime? dataEfetiva,
            string? motivo,
            string? status,
            string? documento,
            Guid? filialOrigemId,
            Guid? departamentoOrigemId,
            Guid? cargoOrigemId,
            Guid? filialDestinoId,
            Guid? departamentoDestinoId,
            Guid? cargoDestinoId,
            Guid? aprovadoPorId,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            DataTransferencia = dataTransferencia;
            DataEfetiva = dataEfetiva;
            Motivo = motivo;
            Status = status;
            Documento = documento;
            FilialOrigemId = filialOrigemId;
            DepartamentoOrigemId = departamentoOrigemId;
            CargoOrigemId = cargoOrigemId;
            FilialDestinoId = filialDestinoId;
            DepartamentoDestinoId = departamentoDestinoId;
            CargoDestinoId = cargoDestinoId;
            AprovadoPorId = aprovadoPorId;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmTransferencia>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
