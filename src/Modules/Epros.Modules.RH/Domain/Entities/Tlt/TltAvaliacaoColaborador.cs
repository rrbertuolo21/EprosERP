using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_avaliacao_colaborador). Fidelidade campo a campo.</summary>
    public partial class TltAvaliacaoColaborador : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid AvaliadorId { get; private set; }
        public Guid CicloAvaliacaoId { get; private set; }
        public DateTime? DataAvaliacao { get; private set; }
        public DateTime? DataConclusao { get; private set; }
        public string? NotasJson { get; private set; }
        public decimal? MediaNota { get; private set; }
        public string? PontosFortes { get; private set; }
        public string? PontosMelhoria { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public Guid? CriadoPorId { get; private set; }
        public Guid OwnerId { get; private set; }

        protected TltAvaliacaoColaborador() { } // EF Core

        public TltAvaliacaoColaborador(
            Guid colaboradorId,
            Guid avaliadorId,
            Guid cicloAvaliacaoId,
            DateTime? dataAvaliacao,
            DateTime? dataConclusao,
            string? notasJson,
            decimal? mediaNota,
            string? pontosFortes,
            string? pontosMelhoria,
            string status,
            Guid? criadoPorId,
            Guid ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            AvaliadorId = avaliadorId;
            CicloAvaliacaoId = cicloAvaliacaoId;
            DataAvaliacao = dataAvaliacao;
            DataConclusao = dataConclusao;
            NotasJson = notasJson;
            MediaNota = mediaNota;
            PontosFortes = pontosFortes;
            PontosMelhoria = pontosMelhoria;
            Status = status;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltAvaliacaoColaborador>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.AreNotEquals(AvaliadorId, Guid.Empty, nameof(AvaliadorId), "O campo AvaliadorId e obrigatorio.");
            contract.AreNotEquals(CicloAvaliacaoId, Guid.Empty, nameof(CicloAvaliacaoId), "O campo CicloAvaliacaoId e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            contract.AreNotEquals(OwnerId, Guid.Empty, nameof(OwnerId), "O campo OwnerId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
