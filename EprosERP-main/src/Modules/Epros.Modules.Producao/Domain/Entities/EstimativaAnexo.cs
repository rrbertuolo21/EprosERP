using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-EST — Anexo formal da estimativa (prd_est_anexo).</summary>
    public class EstimativaAnexo : EntidadeSaaSBase
    {
        public Guid EstimativaId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? Descricao { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public DateTime DataAnexo { get; private set; }

        protected EstimativaAnexo() { } // EF Core

        public EstimativaAnexo(Guid estimativaId, Guid arquivoId, string usuarioId, string tenantId, string criadoPor, string? descricao = null)
            : base(tenantId, criadoPor)
        {
            EstimativaId = estimativaId;
            ArquivoId = arquivoId;
            UsuarioId = usuarioId;
            Descricao = descricao;
            DataAnexo = DateTime.UtcNow;

            AddNotifications(new Contract<EstimativaAnexo>()
                .Requires()
                .AreNotEquals(estimativaId, Guid.Empty, nameof(EstimativaId), "A estimativa é obrigatória [Origem: EstimativaAnexo].")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo controlado é obrigatório [Origem: EstimativaAnexo].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: EstimativaAnexo].")
            );
        }
    }
}
