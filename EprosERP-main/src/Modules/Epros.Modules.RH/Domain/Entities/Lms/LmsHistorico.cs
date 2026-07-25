using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-LMS, tabela rh_lms_historico). Fidelidade campo a campo.</summary>
    public partial class LmsHistorico : EntidadeSaaSBase
    {
        public string Entidade { get; private set; } = string.Empty;
        public Guid EntidadeId { get; private set; }
        public string Evento { get; private set; } = string.Empty;
        public string? ValorAnteriorJson { get; private set; }
        public string? ValorNovoJson { get; private set; }
        public Guid UsuarioId { get; private set; }
        public DateTime DataEvento { get; private set; }
        public string? Observacao { get; private set; }

        protected LmsHistorico() { } // EF Core

        public LmsHistorico(
            string entidade,
            Guid entidadeId,
            string evento,
            string? valorAnteriorJson,
            string? valorNovoJson,
            Guid usuarioId,
            DateTime dataEvento,
            string? observacao,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Entidade = entidade;
            EntidadeId = entidadeId;
            Evento = evento;
            ValorAnteriorJson = valorAnteriorJson;
            ValorNovoJson = valorNovoJson;
            UsuarioId = usuarioId;
            DataEvento = dataEvento;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<LmsHistorico>().Requires();
            contract.IsNotNullOrEmpty(Entidade, nameof(Entidade), "O campo Entidade e obrigatorio.");
            contract.AreNotEquals(EntidadeId, Guid.Empty, nameof(EntidadeId), "O campo EntidadeId e obrigatorio.");
            contract.IsNotNullOrEmpty(Evento, nameof(Evento), "O campo Evento e obrigatorio.");
            contract.AreNotEquals(UsuarioId, Guid.Empty, nameof(UsuarioId), "O campo UsuarioId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
