using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Portfolio
{
    /// <summary>
    /// Agregado raiz de portfolio. Origem: EF PRJ-PRT 14.1 (prj_portfolio).
    /// PRJ-PRT-RN-002 (codigo unico por tenant, 30), RN-003 (descricao 500), RN-004 (responsavel),
    /// RN-005 (nasce em Rascunho), RN-006..RN-011 (workflow), RN-008 (rejeicao exige motivo),
    /// RN-012 (auditoria), RN-013 (lock otimista por versao), RN-016 (ranking manual exige justificativa),
    /// RN-019 (sem exclusao fisica).
    /// </summary>
    public class Portfolio : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public EProjetoWorkflowStatus Status { get; private set; } = EProjetoWorkflowStatus.Rascunho;
        public Guid ResponsavelId { get; private set; }
        public string? TipoPortfolio { get; private set; }
        public string? Justificativa { get; private set; }
        public decimal? ScoreTotal { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public int Versao { get; private set; }
        public string? MotivoRejeicao { get; private set; }

        public List<PortfolioItem> Itens { get; private set; } = new();
        public List<HistoricoPortfolio> Historicos { get; private set; } = new();
        public List<AnexoPortfolio> Anexos { get; private set; } = new();

        protected Portfolio() { } // EF Core

        public Portfolio(
            string codigo,
            string descricao,
            Guid responsavelId,
            string? tipoPortfolio,
            string? justificativa,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Portfolio>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo do portfolio e obrigatorio. [Origem: Portfolio]")
                .IsLowerOrEqualsThan(codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres. [Origem: Portfolio]")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do portfolio e obrigatoria. [Origem: Portfolio]")
                .IsLowerOrEqualsThan(descricao?.Length ?? 0, 500, nameof(Descricao), "A descricao deve ter no maximo 500 caracteres. [Origem: Portfolio]")
                .AreNotEquals(responsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio. [Origem: Portfolio]"));

            Codigo = codigo ?? string.Empty;
            Descricao = descricao ?? string.Empty;
            ResponsavelId = responsavelId;
            TipoPortfolio = tipoPortfolio;
            Justificativa = justificativa;
            Status = EProjetoWorkflowStatus.Rascunho; // RN-005
            DataCriacao = DateTime.UtcNow;
            Versao = 1;
        }

        private void RegistrarHistorico(EAcaoPortfolio acao, Guid usuarioId, Guid? itemId, string? motivo, string usuario)
        {
            Historicos.Add(new HistoricoPortfolio(Id, itemId, acao, usuarioId, null, motivo, TenantId, usuario));
        }

        /// <summary>RN-PRT-014: itens possuem sequencia; RN-016: ranking manual exige justificativa.</summary>
        public void AdicionarItem(
            int sequencia,
            string tipoItem,
            Guid? projetoId,
            Guid? programaId,
            string? titulo,
            decimal? valorEstimado,
            decimal? esforcoEstimado,
            decimal? capacidadeRequerida,
            decimal? npv,
            decimal? payback,
            decimal? alinhamentoEstrategico,
            decimal? risco,
            decimal? score,
            string? justificativaPrioridade,
            string? observacao,
            string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Rascunho && Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "Itens so podem ser mantidos com o portfolio em estado editavel. [Origem: Portfolio]");
                return;
            }
            var item = new PortfolioItem(Id, sequencia, tipoItem, projetoId, programaId, titulo,
                valorEstimado, esforcoEstimado, capacidadeRequerida, npv, payback,
                alinhamentoEstrategico, risco, score, justificativaPrioridade, observacao, TenantId, usuario);
            if (!item.IsValid)
            {
                AddNotifications(item.Notifications);
                return;
            }
            Itens.Add(item);
            MarcarAlterado(usuario);
        }

        public void AnexarDocumento(Guid arquivoId, Guid? itemId, string? tipoAnexo, string usuario)
        {
            var anexo = new AnexoPortfolio(Id, itemId, arquivoId, tipoAnexo, TenantId, usuario);
            if (!anexo.IsValid)
            {
                AddNotifications(anexo.Notifications);
                return;
            }
            Anexos.Add(anexo);
            MarcarAlterado(usuario);
        }

        /// <summary>PRJ-PRT-RN-006: submissao de Rascunho -> EmAnalise.</summary>
        public void Submeter(Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Rascunho)
            {
                AddNotification(nameof(Status), "So e possivel submeter portfolio em Rascunho. [Origem: Portfolio]");
                return;
            }
            Status = EProjetoWorkflowStatus.EmAnalise;
            RegistrarHistorico(EAcaoPortfolio.Alterado, usuarioId, null, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>PRJ-PRT-RN-007: aprovacao de EmAnalise -> Ativo.</summary>
        public void Aprovar(Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A aprovacao somente ocorre a partir de EmAnalise. [Origem: Portfolio]");
                return;
            }
            Status = EProjetoWorkflowStatus.Ativo;
            RegistrarHistorico(EAcaoPortfolio.Aprovado, usuarioId, null, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>PRJ-PRT-RN-008: rejeicao retorna para Rascunho com motivo.</summary>
        public void Rejeitar(string motivo, Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A rejeicao somente ocorre a partir de EmAnalise. [Origem: Portfolio]");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoRejeicao), "A rejeicao exige motivo. [Origem: Portfolio]");
                return;
            }
            MotivoRejeicao = motivo;
            Status = EProjetoWorkflowStatus.Rascunho;
            RegistrarHistorico(EAcaoPortfolio.Rejeitado, usuarioId, null, motivo, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>PRJ-PRT-RN-009: Ativo -> Suspenso.</summary>
        public void Suspender(Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Ativo)
            {
                AddNotification(nameof(Status), "So e possivel suspender portfolio Ativo. [Origem: Portfolio]");
                return;
            }
            Status = EProjetoWorkflowStatus.Suspenso;
            RegistrarHistorico(EAcaoPortfolio.Suspenso, usuarioId, null, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>PRJ-PRT-RN-011: Suspenso -> Ativo.</summary>
        public void Retomar(Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Suspenso)
            {
                AddNotification(nameof(Status), "So e possivel retomar portfolio Suspenso. [Origem: Portfolio]");
                return;
            }
            Status = EProjetoWorkflowStatus.Ativo;
            RegistrarHistorico(EAcaoPortfolio.Reativado, usuarioId, null, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>PRJ-PRT-RN-009: Ativo -> Encerrado.</summary>
        public void Encerrar(Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Ativo)
            {
                AddNotification(nameof(Status), "So e possivel encerrar portfolio Ativo. [Origem: Portfolio]");
                return;
            }
            Status = EProjetoWorkflowStatus.Encerrado;
            RegistrarHistorico(EAcaoPortfolio.Encerrado, usuarioId, null, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>PRJ-PRT-RN-009: Ativo -> Inativo.</summary>
        public void Inativar(Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Ativo)
            {
                AddNotification(nameof(Status), "So e possivel inativar portfolio Ativo. [Origem: Portfolio]");
                return;
            }
            Status = EProjetoWorkflowStatus.Inativo;
            RegistrarHistorico(EAcaoPortfolio.Inativado, usuarioId, null, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>PRJ-PRT-RN-010: Inativo -> Ativo.</summary>
        public void Reativar(Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Inativo)
            {
                AddNotification(nameof(Status), "So e possivel reativar portfolio Inativo. [Origem: Portfolio]");
                return;
            }
            Status = EProjetoWorkflowStatus.Ativo;
            RegistrarHistorico(EAcaoPortfolio.Reativado, usuarioId, null, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>PRJ-PRT-RN-016: ranking manual exige justificativa. Consolida score do portfolio.</summary>
        public void PriorizarManual(decimal? scoreTotal, string justificativa, Guid usuarioId, string usuario)
        {
            if (string.IsNullOrWhiteSpace(justificativa))
            {
                AddNotification(nameof(Justificativa), "A priorizacao manual exige justificativa. [Origem: Portfolio]");
                return;
            }
            ScoreTotal = scoreTotal;
            Justificativa = justificativa;
            RegistrarHistorico(EAcaoPortfolio.Alterado, usuarioId, null, justificativa, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>PRJ-PRT-RN-018: evento executivo apenas quando Ativo.</summary>
        public bool PodePublicarEventoExecutivo() => Status == EProjetoWorkflowStatus.Ativo;
    }
}
