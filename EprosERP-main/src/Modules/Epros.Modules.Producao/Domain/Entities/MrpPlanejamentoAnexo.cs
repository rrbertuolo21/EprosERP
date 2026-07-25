using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-MRP — Anexo formal do ciclo MRP/IBP (prd_mrp_planejamento_anexo). Exige arquivo controlado.</summary>
    public class MrpPlanejamentoAnexo : EntidadeSaaSBase
    {
        public Guid PlanejamentoId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? Descricao { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public DateTime AnexadoEm { get; private set; }

        protected MrpPlanejamentoAnexo() { } // EF Core

        public MrpPlanejamentoAnexo(Guid planejamentoId, Guid arquivoId, string usuarioId, string tenantId, string criadoPor, string? descricao = null)
            : base(tenantId, criadoPor)
        {
            PlanejamentoId = planejamentoId;
            ArquivoId = arquivoId;
            UsuarioId = usuarioId;
            Descricao = descricao;
            AnexadoEm = DateTime.UtcNow;

            AddNotifications(new Contract<MrpPlanejamentoAnexo>()
                .Requires()
                .AreNotEquals(planejamentoId, Guid.Empty, nameof(PlanejamentoId), "O planejamento é obrigatório [Origem: MrpPlanejamentoAnexo].")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo controlado é obrigatório [Origem: MrpPlanejamentoAnexo].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: MrpPlanejamentoAnexo].")
            );
        }
    }
}
