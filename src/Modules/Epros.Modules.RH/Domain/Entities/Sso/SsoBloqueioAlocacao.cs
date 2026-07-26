using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-SSO). Fidelidade campo a campo.</summary>
    public partial class SsoBloqueioAlocacao : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public string Origem { get; private set; } = string.Empty;
        public Guid? OrigemId { get; private set; }
        public DateTime DataBloqueio { get; private set; }
        public DateTime? DataResolucao { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public string? Observacao { get; private set; }

        protected SsoBloqueioAlocacao() { } // EF Core

        public SsoBloqueioAlocacao(
            Guid colaboradorId,
            string origem,
            Guid? origemId,
            DateTime dataBloqueio,
            DateTime? dataResolucao,
            string status,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            Origem = origem;
            OrigemId = origemId;
            DataBloqueio = dataBloqueio;
            DataResolucao = dataResolucao;
            Status = status;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<SsoBloqueioAlocacao>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Origem, nameof(Origem), "O campo Origem e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
