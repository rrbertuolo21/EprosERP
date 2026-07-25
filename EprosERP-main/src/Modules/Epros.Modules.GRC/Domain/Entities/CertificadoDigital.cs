using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-REG — Certificado digital (grc_reg_certificado_digital). Controla certificado,
    /// validade, empresa e origem. Fiel a EF_13_GRC_COMPLIANCE_REGULATORIO_V1 (secoes 10.2, 10.4 e 12).
    /// SEGURANCA: a senha/PFX do certificado NAO e armazenada nesta entidade (lacuna registrada
    /// na EF 10.7). Guarda-se apenas metadados de conformidade.
    /// </summary>
    public class CertificadoDigital : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string Cnpj { get; private set; } = string.Empty;
        public string Serial { get; private set; } = string.Empty;
        public string Tipo { get; private set; } = "A1"; // A1, A3
        public string Origem { get; private set; } = "Empresa"; // Empresa, EscritorioContador
        public DateTime DataValidade { get; private set; }
        // Rascunho, Validado, Ativo, Vencido, Revogado, Inativo
        public string Status { get; private set; } = "Rascunho";
        public string? MotivoRevogacao { get; private set; }

        protected CertificadoDigital() { } // EF Core

        public CertificadoDigital(
            Guid empresaId,
            string cnpj,
            string serial,
            string tipo,
            string origem,
            DateTime dataValidade,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<CertificadoDigital>()
                .Requires()
                .IsTrue(empresaId != Guid.Empty, nameof(EmpresaId), "A empresa do certificado e obrigatoria.")
                .IsNotNullOrEmpty(cnpj, nameof(Cnpj), "O CNPJ do certificado e obrigatorio.")
                .IsNotNullOrEmpty(serial, nameof(Serial), "O serial do certificado e obrigatorio.")
                .IsTrue(tipo == "A1" || tipo == "A3", nameof(Tipo), "O tipo do certificado deve ser 'A1' ou 'A3'.")
                .IsTrue(origem == "Empresa" || origem == "EscritorioContador", nameof(Origem),
                    "A origem deve ser 'Empresa' ou 'EscritorioContador'.")
            );

            EmpresaId = empresaId;
            Cnpj = cnpj;
            Serial = serial;
            Tipo = tipo;
            Origem = origem;
            DataValidade = dataValidade;
            Status = "Rascunho";
        }

        public void Validar(string usuario)
        {
            if (Status != "Rascunho")
            {
                AddNotification(nameof(Status), "Somente certificados em rascunho podem ser validados.");
                return;
            }
            Status = "Validado";
            MarcarAlterado(usuario);
        }

        public void Ativar(string usuario)
        {
            if (Status != "Validado")
            {
                AddNotification(nameof(Status), "Somente certificados validados podem ser ativados.");
                return;
            }
            Status = "Ativo";
            MarcarAlterado(usuario);
        }

        public void MarcarVencido(string usuario)
        {
            if (Status != "Ativo")
            {
                AddNotification(nameof(Status), "Somente certificados ativos podem ser marcados como vencidos.");
                return;
            }
            Status = "Vencido";
            MarcarAlterado(usuario);
        }

        public void Revogar(string motivo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoRevogacao), "O motivo da revogacao e obrigatorio.");
                return;
            }
            Status = "Revogado";
            MotivoRevogacao = motivo;
            MarcarAlterado(usuario);
        }
    }
}
