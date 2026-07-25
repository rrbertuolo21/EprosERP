using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-ESC — Anexo formal da programação (prd_esc_anexo). ESC-REG-022: exige arquivo controlado.</summary>
    public class EscAnexo : EntidadeSaaSBase
    {
        public Guid ProgramacaoId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? Descricao { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public DateTime DataAnexo { get; private set; }

        protected EscAnexo() { } // EF Core

        public EscAnexo(Guid programacaoId, Guid arquivoId, string usuarioId, string tenantId, string criadoPor, string? descricao = null)
            : base(tenantId, criadoPor)
        {
            ProgramacaoId = programacaoId;
            ArquivoId = arquivoId;
            UsuarioId = usuarioId;
            Descricao = descricao;
            DataAnexo = DateTime.UtcNow;

            AddNotifications(new Contract<EscAnexo>()
                .Requires()
                .AreNotEquals(programacaoId, Guid.Empty, nameof(ProgramacaoId), "A programação é obrigatória [Origem: EscAnexo].")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo controlado é obrigatório [Origem: EscAnexo]. (ESC-REG-022)")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: EscAnexo].")
            );
        }
    }
}
