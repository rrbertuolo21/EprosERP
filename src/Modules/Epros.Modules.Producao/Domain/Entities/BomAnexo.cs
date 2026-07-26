using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-BOM — Anexo formal da estrutura (prd_bom_anexo). Exige arquivo controlado (aceite 19).</summary>
    public class BomAnexo : EntidadeSaaSBase
    {
        public Guid EstruturaId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? Descricao { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public DateTime AnexadoEm { get; private set; }

        protected BomAnexo() { } // EF Core

        public BomAnexo(Guid estruturaId, Guid arquivoId, string usuarioId, string tenantId, string criadoPor, string? descricao = null)
            : base(tenantId, criadoPor)
        {
            EstruturaId = estruturaId;
            ArquivoId = arquivoId;
            UsuarioId = usuarioId;
            Descricao = descricao;
            AnexadoEm = DateTime.UtcNow;

            AddNotifications(new Contract<BomAnexo>()
                .Requires()
                .AreNotEquals(estruturaId, Guid.Empty, nameof(EstruturaId), "A estrutura é obrigatória [Origem: BomAnexo].")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo controlado é obrigatório [Origem: BomAnexo].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: BomAnexo].")
            );
        }
    }
}
