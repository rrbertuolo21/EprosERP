using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-PLN — Anexo do planejamento (prd_pln_planejamento_anexo). Arquivo controlado no GED.</summary>
    public class PlanejamentoAnexo : EntidadeSaaSBase
    {
        public Guid PlanejamentoId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public DateTime AnexadoEm { get; private set; }

        protected PlanejamentoAnexo() { } // EF Core

        public PlanejamentoAnexo(Guid planejamentoId, Guid arquivoId, string usuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PlanejamentoId = planejamentoId;
            ArquivoId = arquivoId;
            UsuarioId = usuarioId;
            AnexadoEm = DateTime.UtcNow;

            AddNotifications(new Contract<PlanejamentoAnexo>()
                .Requires()
                .AreNotEquals(planejamentoId, Guid.Empty, nameof(PlanejamentoId), "O planejamento é obrigatório [Origem: PlanejamentoAnexo].")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo controlado é obrigatório [Origem: PlanejamentoAnexo].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: PlanejamentoAnexo].")
            );
        }
    }
}
