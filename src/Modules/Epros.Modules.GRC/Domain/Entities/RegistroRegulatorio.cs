using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-REG — Registro regulatorio (grc_reg_registro). Controla obrigacao, norma,
    /// responsavel e status. Fiel a EF_13_GRC_COMPLIANCE_REGULATORIO_V1 (secoes 10.2 e 12).
    /// </summary>
    public class RegistroRegulatorio : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public string? Norma { get; private set; }
        public Guid ResponsavelId { get; private set; }
        // Rascunho, EmAnalise, Ativo, Suspenso, Encerrado, Inativo
        public string Status { get; private set; } = "Rascunho";
        public string? MotivoUltimaTransicao { get; private set; }

        protected RegistroRegulatorio() { } // EF Core

        public RegistroRegulatorio(
            string codigo,
            string descricao,
            string? norma,
            Guid responsavelId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RegistroRegulatorio>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo do registro regulatorio e obrigatorio.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do registro regulatorio e obrigatoria.")
                .IsTrue(responsavelId != Guid.Empty, nameof(ResponsavelId), "O responsavel pelo registro e obrigatorio.")
            );

            Codigo = codigo;
            Descricao = descricao;
            Norma = norma;
            ResponsavelId = responsavelId;
            Status = "Rascunho";
        }

        public void Submeter(string usuario)
        {
            if (Status != "Rascunho")
            {
                AddNotification(nameof(Status), "Somente registros em rascunho podem ser submetidos.");
                return;
            }
            Status = "EmAnalise";
            MarcarAlterado(usuario);
        }

        public void Ativar(string usuario)
        {
            if (Status != "EmAnalise")
            {
                AddNotification(nameof(Status), "Somente registros em analise podem ser ativados.");
                return;
            }
            Status = "Ativo";
            MarcarAlterado(usuario);
        }

        public void Suspender(string motivo, string usuario)
        {
            if (Status != "Ativo")
            {
                AddNotification(nameof(Status), "Somente registros ativos podem ser suspensos.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoUltimaTransicao), "O motivo da suspensao e obrigatorio.");
                return;
            }
            Status = "Suspenso";
            MotivoUltimaTransicao = motivo;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string motivo, string usuario)
        {
            if (Status != "Ativo" && Status != "Suspenso")
            {
                AddNotification(nameof(Status), "Somente registros ativos ou suspensos podem ser encerrados.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoUltimaTransicao), "O motivo do encerramento e obrigatorio.");
                return;
            }
            Status = "Encerrado";
            MotivoUltimaTransicao = motivo;
            MarcarAlterado(usuario);
        }
    }
}
