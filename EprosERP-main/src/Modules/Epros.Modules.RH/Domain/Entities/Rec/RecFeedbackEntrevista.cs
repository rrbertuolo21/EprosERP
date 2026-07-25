using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-REC). Fidelidade campo a campo.</summary>
    public partial class RecFeedbackEntrevista : EntidadeSaaSBase
    {
        public decimal NotaTecnica { get; private set; }
        public decimal NotaComunicacao { get; private set; }
        public decimal NotaAderenciaCultural { get; private set; }
        public decimal NotaGeral { get; private set; }
        public string? PontosFortes { get; private set; }
        public string? PontosFracos { get; private set; }
        public string? Comentarios { get; private set; }
        public string Recomendacao { get; private set; } = string.Empty;
        public Guid EntrevistaId { get; private set; }
        public string EntrevistadoresJson { get; private set; } = string.Empty;
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected RecFeedbackEntrevista() { } // EF Core

        public RecFeedbackEntrevista(
            decimal notaTecnica,
            decimal notaComunicacao,
            decimal notaAderenciaCultural,
            decimal notaGeral,
            string? pontosFortes,
            string? pontosFracos,
            string? comentarios,
            string recomendacao,
            Guid entrevistaId,
            string entrevistadoresJson,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            NotaTecnica = notaTecnica;
            NotaComunicacao = notaComunicacao;
            NotaAderenciaCultural = notaAderenciaCultural;
            NotaGeral = notaGeral;
            PontosFortes = pontosFortes;
            PontosFracos = pontosFracos;
            Comentarios = comentarios;
            Recomendacao = recomendacao;
            EntrevistaId = entrevistaId;
            EntrevistadoresJson = entrevistadoresJson;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<RecFeedbackEntrevista>().Requires();
            contract.IsNotNullOrEmpty(Recomendacao, nameof(Recomendacao), "O campo Recomendacao e obrigatorio.");
            contract.AreNotEquals(EntrevistaId, Guid.Empty, nameof(EntrevistaId), "O campo EntrevistaId e obrigatorio.");
            contract.IsNotNullOrEmpty(EntrevistadoresJson, nameof(EntrevistadoresJson), "O campo EntrevistadoresJson e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
