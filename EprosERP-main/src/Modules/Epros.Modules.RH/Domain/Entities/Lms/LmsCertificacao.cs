using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-LMS, tabela rh_lms_certificacao). Fidelidade campo a campo.</summary>
    public partial class LmsCertificacao : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid? TreinamentoId { get; private set; }
        public string? CodigoCertificacao { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public DateTime? DataEmissao { get; private set; }
        public DateTime? DataValidade { get; private set; }
        public bool Obrigatoria { get; private set; }
        public string Status { get; private set; } = string.Empty;

        protected LmsCertificacao() { } // EF Core

        public LmsCertificacao(
            Guid colaboradorId,
            Guid? treinamentoId,
            string? codigoCertificacao,
            string descricao,
            DateTime? dataEmissao,
            DateTime? dataValidade,
            bool obrigatoria,
            string status,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            TreinamentoId = treinamentoId;
            CodigoCertificacao = codigoCertificacao;
            Descricao = descricao;
            DataEmissao = dataEmissao;
            DataValidade = dataValidade;
            Obrigatoria = obrigatoria;
            Status = status;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<LmsCertificacao>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Descricao, nameof(Descricao), "O campo Descricao e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
