using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-CIA — Plano de auditoria (grc_cia_plano_auditoria). Planeja auditorias e ciclos.
    /// Fiel a EF_13_GRC_CONTROLES_INTERNOS_E_AUDITORIA_V1 (secoes 10.2 e 12).
    /// </summary>
    public class PlanoAuditoria : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Titulo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public string? Ciclo { get; private set; } // Anual, Semestral, Trimestral...
        // Rascunho, EmAnalise, Ativo, Suspenso, Encerrado, Inativo
        public string Status { get; private set; } = "Rascunho";
        public string? MotivoUltimaTransicao { get; private set; }

        protected PlanoAuditoria() { } // EF Core

        public PlanoAuditoria(
            string codigo,
            string titulo,
            string descricao,
            string? ciclo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PlanoAuditoria>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo do plano de auditoria e obrigatorio.")
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O titulo do plano de auditoria e obrigatorio.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do plano de auditoria e obrigatoria.")
            );

            Codigo = codigo;
            Titulo = titulo;
            Descricao = descricao;
            Ciclo = ciclo;
            Status = "Rascunho";
        }

        public void Submeter(string usuario)
        {
            if (Status != "Rascunho")
            {
                AddNotification(nameof(Status), "Somente planos em rascunho podem ser submetidos.");
                return;
            }
            Status = "EmAnalise";
            MarcarAlterado(usuario);
        }

        public void Ativar(string usuario)
        {
            if (Status != "EmAnalise")
            {
                AddNotification(nameof(Status), "Somente planos em analise podem ser ativados.");
                return;
            }
            Status = "Ativo";
            MarcarAlterado(usuario);
        }

        public void Encerrar(string motivo, string usuario)
        {
            if (Status != "Ativo" && Status != "Suspenso")
            {
                AddNotification(nameof(Status), "Somente planos ativos ou suspensos podem ser encerrados.");
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
