using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-SOD — Excecao SoD (grc_sod_excecao). Autorizacao temporaria de um conflito, com prazo
    /// e aprovador. Fiel a EF_13_GRC_SEGREGACAO_DE_FUNCOES_SOD_V1 (secoes 10.2, 10.4 e 12).
    /// Constraint: DataFim e obrigatoria (excecao precisa de prazo).
    /// </summary>
    public class ExcecaoSoD : EntidadeSaaSBase
    {
        public Guid ViolacaoId { get; private set; }
        public string Justificativa { get; private set; } = string.Empty;
        // D-SOD-02 — quem solicita a excecao (autoaprovacao proibida: solicitante != aprovador).
        public Guid? SolicitanteId { get; private set; }
        public Guid? AprovadorId { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        // EmAnalise, Aprovada, Vencida, Revogada, Encerrada, Renovada
        public string Status { get; private set; } = "EmAnalise";
        // D-SOD-02 — controle compensatorio obrigatorio (referencia a ControleInterno) + descricao livre.
        public Guid? ControleCompensatorioId { get; private set; }
        public string? ControleCompensatorio { get; private set; }
        public int Renovacoes { get; private set; }

        protected ExcecaoSoD() { } // EF Core

        public ExcecaoSoD(
            Guid violacaoId,
            string justificativa,
            Guid? solicitanteId,
            DateTime dataInicio,
            DateTime dataFim,
            Guid? controleCompensatorioId,
            string? controleCompensatorio,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ExcecaoSoD>()
                .Requires()
                .IsTrue(violacaoId != Guid.Empty, nameof(ViolacaoId), "A violacao da excecao e obrigatoria.")
                .IsNotNullOrEmpty(justificativa, nameof(Justificativa), "A justificativa da excecao e obrigatoria.")
                // Constraint 10.4: excecao precisa de prazo (DataFim) e nao pode ser anterior ao inicio.
                .IsTrue(dataFim > dataInicio, nameof(DataFim), "A data fim da excecao deve ser posterior a data inicio.")
                // D-SOD-02: exceção SEM controle compensatório é o antipadrão — exige ao menos uma das formas.
                .IsTrue(controleCompensatorioId != null || !string.IsNullOrWhiteSpace(controleCompensatorio),
                    nameof(ControleCompensatorio), "A excecao exige um controle compensatorio (referencia ou descricao).")
            );

            ViolacaoId = violacaoId;
            Justificativa = justificativa;
            SolicitanteId = solicitanteId;
            DataInicio = dataInicio;
            DataFim = dataFim;
            ControleCompensatorioId = controleCompensatorioId;
            ControleCompensatorio = controleCompensatorio;
            Status = "EmAnalise";
            Renovacoes = 0;
        }

        public void Aprovar(Guid aprovadorId, string usuario)
        {
            if (Status != "EmAnalise")
            {
                AddNotification(nameof(Status), "Somente excecoes em analise podem ser aprovadas.");
                return;
            }
            if (aprovadorId == Guid.Empty)
            {
                AddNotification(nameof(AprovadorId), "O aprovador da excecao e obrigatorio.");
                return;
            }
            // D-SOD-02 / regra de ouro SoD: autoaprovacao proibida (solicitante != aprovador).
            if (SolicitanteId != null && SolicitanteId == aprovadorId)
            {
                AddNotification(nameof(AprovadorId), "Autoaprovacao proibida: o aprovador nao pode ser o solicitante da excecao.");
                return;
            }
            Status = "Aprovada";
            AprovadorId = aprovadorId;
            MarcarAlterado(usuario);
        }

        /// <summary>
        /// D-SOD-02 — renovação auditada da exceção (novo prazo). O prazo máximo por parâmetro
        /// (SOD_PRAZO_MAX_EXCECAO) é validado no handler; aqui só estende a vigência de uma exceção aprovada.
        /// </summary>
        public void Renovar(DateTime novaDataFim, string usuario)
        {
            if (Status != "Aprovada" && Status != "Vencida")
            {
                AddNotification(nameof(Status), "Somente excecoes aprovadas ou vencidas podem ser renovadas.");
                return;
            }
            if (novaDataFim <= DataFim)
            {
                AddNotification(nameof(DataFim), "A nova data fim deve ser posterior a data fim atual.");
                return;
            }
            DataFim = novaDataFim;
            Status = "Aprovada";
            Renovacoes++;
            MarcarAlterado(usuario);
        }

        public void Rejeitar(string usuario)
        {
            if (Status != "EmAnalise")
            {
                AddNotification(nameof(Status), "Somente excecoes em analise podem ser rejeitadas.");
                return;
            }
            Status = "Revogada";
            MarcarAlterado(usuario);
        }

        public void MarcarVencida(string usuario)
        {
            if (Status != "Aprovada")
            {
                AddNotification(nameof(Status), "Somente excecoes aprovadas podem vencer.");
                return;
            }
            Status = "Vencida";
            MarcarAlterado(usuario);
        }

        public void Revogar(string usuario)
        {
            Status = "Revogada";
            MarcarAlterado(usuario);
        }

        public bool EstaVigente(DateTime referencia) =>
            Status == "Aprovada" && DataInicio <= referencia && DataFim >= referencia;
    }
}
