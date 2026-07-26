using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_desligamento). Fidelidade campo a campo.</summary>
    public partial class DevDesligamento : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid? TipoDesligamentoId { get; private set; }
        public DateTime? DataAviso { get; private set; }
        public DateTime? DataDesligamento { get; private set; }
        public string? Motivo { get; private set; }
        public string? Descricao { get; private set; }
        public string? Documento { get; private set; }
        public string? Status { get; private set; }
        public Guid? AprovadoPor { get; private set; }

        protected DevDesligamento() { } // EF Core

        public DevDesligamento(
            Guid colaboradorId,
            Guid? tipoDesligamentoId,
            DateTime? dataAviso,
            DateTime? dataDesligamento,
            string? motivo,
            string? descricao,
            string? documento,
            string? status,
            Guid? aprovadoPor,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            TipoDesligamentoId = tipoDesligamentoId;
            DataAviso = dataAviso;
            DataDesligamento = dataDesligamento;
            Motivo = motivo;
            Descricao = descricao;
            Documento = documento;
            Status = status;
            AprovadoPor = aprovadoPor;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<DevDesligamento>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
