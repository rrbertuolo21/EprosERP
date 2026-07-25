using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_historico). Fidelidade campo a campo.</summary>
    public partial class DevHistorico : EntidadeSaaSBase
    {
        public Guid? ColaboradorId { get; private set; }
        public string Entidade { get; private set; } = string.Empty;
        public Guid EntidadeId { get; private set; }
        public string Evento { get; private set; } = string.Empty;
        public DateTime DataHora { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string? Detalhe { get; private set; }

        protected DevHistorico() { } // EF Core

        public DevHistorico(
            Guid? colaboradorId,
            string entidade,
            Guid entidadeId,
            string evento,
            DateTime dataHora,
            Guid usuarioId,
            string? detalhe,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            Entidade = entidade;
            EntidadeId = entidadeId;
            Evento = evento;
            DataHora = dataHora;
            UsuarioId = usuarioId;
            Detalhe = detalhe;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<DevHistorico>().Requires();
            contract.IsNotNullOrEmpty(Entidade, nameof(Entidade), "O campo Entidade e obrigatorio.");
            contract.AreNotEquals(EntidadeId, Guid.Empty, nameof(EntidadeId), "O campo EntidadeId e obrigatorio.");
            contract.IsNotNullOrEmpty(Evento, nameof(Evento), "O campo Evento e obrigatorio.");
            contract.AreNotEquals(UsuarioId, Guid.Empty, nameof(UsuarioId), "O campo UsuarioId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
