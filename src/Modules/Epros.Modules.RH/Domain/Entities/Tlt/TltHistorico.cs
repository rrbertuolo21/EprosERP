using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_historico). Fidelidade campo a campo.</summary>
    public partial class TltHistorico : EntidadeSaaSBase
    {
        public string Entidade { get; private set; } = string.Empty;
        public Guid EntidadeId { get; private set; }
        public string Acao { get; private set; } = string.Empty;
        public Guid? UsuarioId { get; private set; }
        public DateTime DataHora { get; private set; }
        public string? Detalhe { get; private set; }

        protected TltHistorico() { } // EF Core

        public TltHistorico(
            string entidade,
            Guid entidadeId,
            string acao,
            Guid? usuarioId,
            DateTime dataHora,
            string? detalhe,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Entidade = entidade;
            EntidadeId = entidadeId;
            Acao = acao;
            UsuarioId = usuarioId;
            DataHora = dataHora;
            Detalhe = detalhe;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltHistorico>().Requires();
            contract.IsNotNullOrEmpty(Entidade, nameof(Entidade), "O campo Entidade e obrigatorio.");
            contract.AreNotEquals(EntidadeId, Guid.Empty, nameof(EntidadeId), "O campo EntidadeId e obrigatorio.");
            contract.IsNotNullOrEmpty(Acao, nameof(Acao), "O campo Acao e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
