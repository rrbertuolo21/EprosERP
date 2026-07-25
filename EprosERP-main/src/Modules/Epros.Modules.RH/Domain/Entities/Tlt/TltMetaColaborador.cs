using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_meta_colaborador). Fidelidade campo a campo.</summary>
    public partial class TltMetaColaborador : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid? TipoMetaId { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public decimal? Alvo { get; private set; }
        public decimal Progresso { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public Guid? CriadoPorId { get; private set; }
        public Guid OwnerId { get; private set; }

        protected TltMetaColaborador() { } // EF Core

        public TltMetaColaborador(
            Guid colaboradorId,
            Guid? tipoMetaId,
            string titulo,
            string? descricao,
            DateTime dataInicio,
            DateTime dataFim,
            decimal? alvo,
            decimal progresso,
            string status,
            Guid? criadoPorId,
            Guid ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            TipoMetaId = tipoMetaId;
            Titulo = titulo;
            Descricao = descricao;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Alvo = alvo;
            Progresso = progresso;
            Status = status;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltMetaColaborador>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Titulo, nameof(Titulo), "O campo Titulo e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            contract.AreNotEquals(OwnerId, Guid.Empty, nameof(OwnerId), "O campo OwnerId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
