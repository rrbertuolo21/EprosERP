using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-REC). Fidelidade campo a campo.</summary>
    public partial class RecAvaliacaoCandidato : EntidadeSaaSBase
    {
        public string NomeAvaliacao { get; private set; } = string.Empty;
        public decimal Pontuacao { get; private set; }
        public decimal PontuacaoMaxima { get; private set; }
        public string Resultado { get; private set; } = string.Empty;
        public string? Comentarios { get; private set; }
        public DateTime DataAvaliacao { get; private set; }
        public Guid CandidatoId { get; private set; }
        public Guid ConduzidaPorUsuarioId { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected RecAvaliacaoCandidato() { } // EF Core

        public RecAvaliacaoCandidato(
            string nomeAvaliacao,
            decimal pontuacao,
            decimal pontuacaoMaxima,
            string resultado,
            string? comentarios,
            DateTime dataAvaliacao,
            Guid candidatoId,
            Guid conduzidaPorUsuarioId,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            NomeAvaliacao = nomeAvaliacao;
            Pontuacao = pontuacao;
            PontuacaoMaxima = pontuacaoMaxima;
            Resultado = resultado;
            Comentarios = comentarios;
            DataAvaliacao = dataAvaliacao;
            CandidatoId = candidatoId;
            ConduzidaPorUsuarioId = conduzidaPorUsuarioId;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<RecAvaliacaoCandidato>().Requires();
            contract.IsNotNullOrEmpty(NomeAvaliacao, nameof(NomeAvaliacao), "O campo NomeAvaliacao e obrigatorio.");
            contract.IsNotNullOrEmpty(Resultado, nameof(Resultado), "O campo Resultado e obrigatorio.");
            contract.AreNotEquals(CandidatoId, Guid.Empty, nameof(CandidatoId), "O campo CandidatoId e obrigatorio.");
            contract.AreNotEquals(ConduzidaPorUsuarioId, Guid.Empty, nameof(ConduzidaPorUsuarioId), "O campo ConduzidaPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
