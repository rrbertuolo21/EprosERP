using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_pedido_desligamento). Fidelidade campo a campo.</summary>
    public partial class DevPedidoDesligamento : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public DateTime? UltimoDiaTrabalho { get; private set; }
        public string? Motivo { get; private set; }
        public string? Descricao { get; private set; }
        public string? Status { get; private set; }
        public string? Documento { get; private set; }
        public Guid? AprovadoPor { get; private set; }

        protected DevPedidoDesligamento() { } // EF Core

        public DevPedidoDesligamento(
            Guid colaboradorId,
            DateTime? ultimoDiaTrabalho,
            string? motivo,
            string? descricao,
            string? status,
            string? documento,
            Guid? aprovadoPor,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            UltimoDiaTrabalho = ultimoDiaTrabalho;
            Motivo = motivo;
            Descricao = descricao;
            Status = status;
            Documento = documento;
            AprovadoPor = aprovadoPor;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<DevPedidoDesligamento>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
