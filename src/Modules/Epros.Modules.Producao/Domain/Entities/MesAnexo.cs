using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-MES — Anexo formal da ordem (prd_mes_anexo). Exige arquivo controlado (aceite MES §9).</summary>
    public class MesAnexo : EntidadeSaaSBase
    {
        public Guid OrdemId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? Descricao { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public DateTime AnexadoEm { get; private set; }

        protected MesAnexo() { } // EF Core

        public MesAnexo(Guid ordemId, Guid arquivoId, string usuarioId, string tenantId, string criadoPor, string? descricao = null)
            : base(tenantId, criadoPor)
        {
            OrdemId = ordemId;
            ArquivoId = arquivoId;
            UsuarioId = usuarioId;
            Descricao = descricao;
            AnexadoEm = DateTime.UtcNow;

            AddNotifications(new Contract<MesAnexo>()
                .Requires()
                .AreNotEquals(ordemId, Guid.Empty, nameof(OrdemId), "A ordem é obrigatória [Origem: MesAnexo].")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo controlado é obrigatório [Origem: MesAnexo].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: MesAnexo].")
            );
        }
    }
}
