using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-GOS — Anexo da ficha de produção (prd_gos_ficha_anexo).</summary>
    public class FichaProducaoAnexo : EntidadeSaaSBase
    {
        public Guid FichaProducaoId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? Tipo { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public DateTime DataHora { get; private set; }

        protected FichaProducaoAnexo() { } // EF Core

        public FichaProducaoAnexo(Guid fichaProducaoId, Guid arquivoId, string usuarioId, string tenantId, string criadoPor, string? tipo = null)
            : base(tenantId, criadoPor)
        {
            FichaProducaoId = fichaProducaoId;
            ArquivoId = arquivoId;
            UsuarioId = usuarioId;
            Tipo = tipo;
            DataHora = DateTime.UtcNow;

            AddNotifications(new Contract<FichaProducaoAnexo>()
                .Requires()
                .AreNotEquals(fichaProducaoId, Guid.Empty, nameof(FichaProducaoId), "A ficha de produção é obrigatória [Origem: FichaProducaoAnexo].")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo controlado é obrigatório [Origem: FichaProducaoAnexo].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: FichaProducaoAnexo].")
            );
        }
    }
}
