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
        // D-REG-05 — obrigação com nome, framework de origem e prazo próprio (EF×código #10).
        public string? Nome { get; private set; }
        public string? Framework { get; private set; } // LGPD, SOX, SPED, eSocial, PCI, ISO27001...
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataVencimento { get; private set; }
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

        /// <summary>
        /// D-REG-05 — define os dados da obrigação (nome, framework de origem e o prazo próprio).
        /// O catálogo obrigação→prazo→evidência por framework depende de validação (PEDIDO-GRC-02);
        /// aqui só se persiste o que o operador informou. // valida-jurídico (aplicabilidade do framework).
        /// </summary>
        public void DefinirObrigacao(string? nome, string? framework, DateTime? dataInicio, DateTime? dataVencimento, string usuario)
        {
            if (dataInicio != null && dataVencimento != null && dataVencimento < dataInicio)
            {
                AddNotification(nameof(DataVencimento), "O vencimento da obrigacao nao pode ser anterior ao inicio.");
                return;
            }
            Nome = nome;
            Framework = framework;
            DataInicio = dataInicio;
            DataVencimento = dataVencimento;
            MarcarAlterado(usuario);
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
