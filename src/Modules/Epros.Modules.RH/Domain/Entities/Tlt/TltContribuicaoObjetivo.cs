using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_contribuicao_objetivo). Fidelidade campo a campo.</summary>
    public partial class TltContribuicaoObjetivo : EntidadeSaaSBase
    {
        public Guid? ObjetivoId { get; private set; }
        public DateTime? DataContribuicao { get; private set; }
        public decimal? ValorContribuicao { get; private set; }
        public string? TipoContribuicao { get; private set; }
        public string? TipoReferencia { get; private set; }
        public Guid? ReferenciaId { get; private set; }
        public string? Notas { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected TltContribuicaoObjetivo() { } // EF Core

        public TltContribuicaoObjetivo(
            Guid? objetivoId,
            DateTime? dataContribuicao,
            decimal? valorContribuicao,
            string? tipoContribuicao,
            string? tipoReferencia,
            Guid? referenciaId,
            string? notas,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ObjetivoId = objetivoId;
            DataContribuicao = dataContribuicao;
            ValorContribuicao = valorContribuicao;
            TipoContribuicao = tipoContribuicao;
            TipoReferencia = tipoReferencia;
            ReferenciaId = referenciaId;
            Notas = notas;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltContribuicaoObjetivo>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
