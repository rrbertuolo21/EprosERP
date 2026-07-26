using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-REG — Validacao de certificado (grc_reg_validacao_certificado). Registra o resultado
    /// (sucesso/falha) e mensagens de cada validacao. Registro imutavel de auditoria.
    /// Fiel a EF_13_GRC_COMPLIANCE_REGULATORIO_V1 (secao 10.2/10.4).
    /// </summary>
    public class ValidacaoCertificado : EntidadeSaaSBase
    {
        public Guid CertificadoId { get; private set; }
        public DateTime DataHora { get; private set; }
        public string Resultado { get; private set; } = "Sucesso"; // Sucesso, Falha
        public string? Mensagem { get; private set; }

        protected ValidacaoCertificado() { } // EF Core

        public ValidacaoCertificado(
            Guid certificadoId,
            string resultado,
            string? mensagem,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ValidacaoCertificado>()
                .Requires()
                .IsTrue(certificadoId != Guid.Empty, nameof(CertificadoId), "O certificado da validacao e obrigatorio.")
                .IsTrue(resultado == "Sucesso" || resultado == "Falha", nameof(Resultado),
                    "O resultado da validacao deve ser 'Sucesso' ou 'Falha'.")
            );

            CertificadoId = certificadoId;
            Resultado = resultado;
            Mensagem = mensagem;
            DataHora = DateTime.UtcNow;
        }
    }
}
