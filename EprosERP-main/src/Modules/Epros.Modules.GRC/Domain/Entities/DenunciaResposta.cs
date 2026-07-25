using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-DEN — Resposta (grc_den_resposta). Registra mensagens internas ou visiveis ao
    /// denunciante. Fiel a EF_13_GRC_INVESTIGACOES_E_DENUNCIAS_V1 (secao 11.3).
    /// Preserva message, is_internal, created_by. RN-DEN-005: resposta interna nao visivel ao denunciante.
    /// </summary>
    public class DenunciaResposta : EntidadeSaaSBase
    {
        public Guid DenunciaId { get; private set; }
        public string Mensagem { get; private set; } = string.Empty; // origem material: message
        public bool Interna { get; private set; } // origem material: is_internal
        public Guid? CriadoPorId { get; private set; } // origem material: created_by
        public DateTime DataCriacao { get; private set; }

        protected DenunciaResposta() { } // EF Core

        public DenunciaResposta(
            Guid denunciaId,
            string mensagem,
            bool interna,
            Guid? criadoPorId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DenunciaResposta>()
                .Requires()
                .IsTrue(denunciaId != Guid.Empty, nameof(DenunciaId), "A denúncia da resposta é obrigatória.")
                .IsNotNullOrEmpty(mensagem, nameof(Mensagem), "A mensagem da resposta não pode ser vazia.")
            );

            DenunciaId = denunciaId;
            Mensagem = mensagem;
            Interna = interna;
            CriadoPorId = criadoPorId;
            DataCriacao = DateTime.UtcNow;
        }
    }
}
