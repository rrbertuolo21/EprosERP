using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-DEN — Participante (grc_den_participante). Registra denunciante, denunciado,
    /// investigador, beneficiario ou testemunha para controle de conflito de interesse.
    /// Fiel a EF_13_GRC_INVESTIGACOES_E_DENUNCIAS_V1 (secao 11.4).
    /// </summary>
    public class DenunciaParticipante : EntidadeSaaSBase
    {
        public Guid DenunciaId { get; private set; }
        public Guid? PessoaId { get; private set; } // pode ficar vazio em anonimato
        // Denunciante, Denunciado, Investigador, Beneficiario, Testemunha
        public string Papel { get; private set; } = string.Empty;
        public string? NomeDeclarado { get; private set; } // relato sem cadastro vinculado
        public bool Sigiloso { get; private set; }

        protected DenunciaParticipante() { } // EF Core

        public DenunciaParticipante(
            Guid denunciaId,
            Guid? pessoaId,
            string papel,
            string? nomeDeclarado,
            bool sigiloso,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DenunciaParticipante>()
                .Requires()
                .IsTrue(denunciaId != Guid.Empty, nameof(DenunciaId), "A denúncia do participante é obrigatória.")
                .IsNotNullOrEmpty(papel, nameof(Papel), "O papel do participante é obrigatório.")
                .IsTrue(
                    papel == "Denunciante" || papel == "Denunciado" || papel == "Investigador" ||
                    papel == "Beneficiario" || papel == "Testemunha",
                    nameof(Papel),
                    "Papel inválido. Use Denunciante, Denunciado, Investigador, Beneficiario ou Testemunha.")
            );

            DenunciaId = denunciaId;
            PessoaId = pessoaId;
            Papel = papel;
            NomeDeclarado = nomeDeclarado;
            Sigiloso = sigiloso;
        }
    }
}
