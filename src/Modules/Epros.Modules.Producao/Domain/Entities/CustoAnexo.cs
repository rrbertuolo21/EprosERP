using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-CST — Anexo formal do registro de custo (prd_cst_anexo).</summary>
    public class CustoAnexo : EntidadeSaaSBase
    {
        public Guid CustoProducaoId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? Descricao { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public DateTime DataAnexo { get; private set; }

        protected CustoAnexo() { } // EF Core

        public CustoAnexo(Guid custoProducaoId, Guid arquivoId, string usuarioId, string tenantId, string criadoPor, string? descricao = null)
            : base(tenantId, criadoPor)
        {
            CustoProducaoId = custoProducaoId;
            ArquivoId = arquivoId;
            UsuarioId = usuarioId;
            Descricao = descricao;
            DataAnexo = DateTime.UtcNow;

            AddNotifications(new Contract<CustoAnexo>()
                .Requires()
                .AreNotEquals(custoProducaoId, Guid.Empty, nameof(CustoProducaoId), "O custo de produção é obrigatório [Origem: CustoAnexo].")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo controlado é obrigatório [Origem: CustoAnexo].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: CustoAnexo].")
            );
        }
    }
}
