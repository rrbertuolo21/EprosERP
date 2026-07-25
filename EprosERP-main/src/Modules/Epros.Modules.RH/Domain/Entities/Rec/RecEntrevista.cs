using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-REC). Fidelidade campo a campo.</summary>
    public partial class RecEntrevista : EntidadeSaaSBase
    {
        public DateTime DataAgendada { get; private set; }
        public TimeSpan HoraAgendada { get; private set; }
        public int DuracaoMinutos { get; private set; }
        public string? Local { get; private set; }
        public string? LinkReuniao { get; private set; }
        public string? EntrevistadoresTexto { get; private set; }
        public string EntrevistadoresJson { get; private set; } = string.Empty;
        public int Status { get; private set; }
        public bool FeedbackEnviado { get; private set; }
        public Guid CandidatoId { get; private set; }
        public Guid VagaId { get; private set; }
        public Guid RoundId { get; private set; }
        public Guid TipoEntrevistaId { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected RecEntrevista() { } // EF Core

        public RecEntrevista(
            DateTime dataAgendada,
            TimeSpan horaAgendada,
            int duracaoMinutos,
            string? local,
            string? linkReuniao,
            string? entrevistadoresTexto,
            string entrevistadoresJson,
            int status,
            bool feedbackEnviado,
            Guid candidatoId,
            Guid vagaId,
            Guid roundId,
            Guid tipoEntrevistaId,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            DataAgendada = dataAgendada;
            HoraAgendada = horaAgendada;
            DuracaoMinutos = duracaoMinutos;
            Local = local;
            LinkReuniao = linkReuniao;
            EntrevistadoresTexto = entrevistadoresTexto;
            EntrevistadoresJson = entrevistadoresJson;
            Status = status;
            FeedbackEnviado = feedbackEnviado;
            CandidatoId = candidatoId;
            VagaId = vagaId;
            RoundId = roundId;
            TipoEntrevistaId = tipoEntrevistaId;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<RecEntrevista>().Requires();
            contract.IsNotNullOrEmpty(EntrevistadoresJson, nameof(EntrevistadoresJson), "O campo EntrevistadoresJson e obrigatorio.");
            contract.AreNotEquals(CandidatoId, Guid.Empty, nameof(CandidatoId), "O campo CandidatoId e obrigatorio.");
            contract.AreNotEquals(VagaId, Guid.Empty, nameof(VagaId), "O campo VagaId e obrigatorio.");
            contract.AreNotEquals(RoundId, Guid.Empty, nameof(RoundId), "O campo RoundId e obrigatorio.");
            contract.AreNotEquals(TipoEntrevistaId, Guid.Empty, nameof(TipoEntrevistaId), "O campo TipoEntrevistaId e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
