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
        public Guid? AprovadorId { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        // EmAnalise, Aprovada, Vencida, Revogada, Encerrada, Renovada
        public string Status { get; private set; } = "EmAnalise";
        public string? ControleCompensatorio { get; private set; }

        protected ExcecaoSoD() { } // EF Core

        public ExcecaoSoD(
            Guid violacaoId,
            string justificativa,
            DateTime dataInicio,
            DateTime dataFim,
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
            );

            ViolacaoId = violacaoId;
            Justificativa = justificativa;
            DataInicio = dataInicio;
            DataFim = dataFim;
            ControleCompensatorio = controleCompensatorio;
            Status = "EmAnalise";
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
            Status = "Aprovada";
            AprovadorId = aprovadorId;
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
