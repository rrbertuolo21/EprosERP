using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Encerramento
{
    /// <summary>
    /// Agregado raiz do encerramento de projeto. Origem: EF PRJ-ENC 11.1 (prj_enc_encerramento).
    /// RN-ENC-002 (codigo unico por tenant), RN-ENC-003 (descricao obrigatoria), RN-ENC-004 (responsavel obrigatorio),
    /// RN-ENC-005 (lock otimista por versao), RN-ENC-006/007 (status final do projeto no dominio valido),
    /// RN-ENC-008 (arquivamento e estado governado), RN-ENC-012/015 (exclusao/edicao pos-encerramento controladas).
    /// Workflow (secao 12): Rascunho -> EmAnalise -> Ativo -> Suspenso/Encerrado/Inativo.
    /// </summary>
    public class EncerramentoProjeto : EntidadeSaaSBase
    {
        public Guid ProjetoId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public EProjetoWorkflowStatus Status { get; private set; } = EProjetoWorkflowStatus.Rascunho;
        public EStatusFinalProjeto? StatusFinalProjeto { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public int Versao { get; private set; }
        public string? MotivoRejeicao { get; private set; }

        public List<ItemEncerramento> Itens { get; private set; } = new();
        public List<HistoricoEncerramento> Historicos { get; private set; } = new();
        public List<AnexoEncerramento> Anexos { get; private set; } = new();

        protected EncerramentoProjeto() { } // EF Core

        public EncerramentoProjeto(
            Guid projetoId,
            string codigo,
            string descricao,
            Guid responsavelId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<EncerramentoProjeto>()
                .Requires()
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O projeto e obrigatorio. [Origem: EncerramentoProjeto]")
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo do encerramento e obrigatorio. [Origem: EncerramentoProjeto]")
                .IsLowerOrEqualsThan(codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres. [Origem: EncerramentoProjeto]")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do encerramento e obrigatoria. [Origem: EncerramentoProjeto]")
                .IsLowerOrEqualsThan(descricao?.Length ?? 0, 500, nameof(Descricao), "A descricao deve ter no maximo 500 caracteres. [Origem: EncerramentoProjeto]")
                .AreNotEquals(responsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio. [Origem: EncerramentoProjeto]"));

            ProjetoId = projetoId;
            Codigo = codigo ?? string.Empty;
            Descricao = descricao ?? string.Empty;
            ResponsavelId = responsavelId;
            Status = EProjetoWorkflowStatus.Rascunho;
            DataCriacao = DateTime.UtcNow;
            Versao = 1;
        }

        public void AdicionarItem(int sequencia, decimal? quantidade, string? observacao, string usuario)
        {
            // RN-ENC-015: itens editaveis antes da aprovacao.
            if (Status != EProjetoWorkflowStatus.Rascunho)
            {
                AddNotification(nameof(Status), "Itens so podem ser adicionados enquanto o encerramento esta em Rascunho. [Origem: EncerramentoProjeto]");
                return;
            }
            var item = new ItemEncerramento(Id, sequencia, quantidade, observacao, TenantId, usuario);
            if (!item.IsValid)
            {
                AddNotifications(item.Notifications);
                return;
            }
            Itens.Add(item);
            MarcarAlterado(usuario);
        }

        public void AnexarDocumento(Guid arquivoId, string usuario)
        {
            var anexo = new AnexoEncerramento(Id, arquivoId, TenantId, usuario);
            if (!anexo.IsValid)
            {
                AddNotifications(anexo.Notifications);
                return;
            }
            Anexos.Add(anexo);
            MarcarAlterado(usuario);
        }

        private void RegistrarHistorico(EAcaoEncerramento acao, Guid usuarioId, string? payloadJson, string usuario)
        {
            Historicos.Add(new HistoricoEncerramento(Id, acao, usuarioId, payloadJson, null, TenantId, usuario));
        }

        /// <summary>RN-ENC (secao 12): submissao exige rascunho com obrigatorios.</summary>
        public void Submeter(Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Rascunho)
            {
                AddNotification(nameof(Status), "So e possivel submeter encerramento em Rascunho. [Origem: EncerramentoProjeto]");
                return;
            }
            Status = EProjetoWorkflowStatus.EmAnalise;
            RegistrarHistorico(EAcaoEncerramento.Submetido, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>RN-ENC-006/007/009: aprovar aplica status final valido ao projeto.</summary>
        public void Aprovar(EStatusFinalProjeto statusFinalProjeto, Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A aprovacao somente ocorre a partir de EmAnalise. [Origem: EncerramentoProjeto]");
                return;
            }
            StatusFinalProjeto = statusFinalProjeto;
            Status = EProjetoWorkflowStatus.Ativo;
            RegistrarHistorico(EAcaoEncerramento.Aprovado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>RN-ENC (secao 8): rejeicao exige motivo; retorna a Rascunho.</summary>
        public void Rejeitar(string motivo, Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A rejeicao somente ocorre a partir de EmAnalise. [Origem: EncerramentoProjeto]");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoRejeicao), "A rejeicao exige motivo. [Origem: EncerramentoProjeto]");
                return;
            }
            MotivoRejeicao = motivo;
            Status = EProjetoWorkflowStatus.Rascunho;
            RegistrarHistorico(EAcaoEncerramento.Rejeitado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        public void Suspender(Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Ativo)
            {
                AddNotification(nameof(Status), "So e possivel suspender encerramento Ativo. [Origem: EncerramentoProjeto]");
                return;
            }
            Status = EProjetoWorkflowStatus.Suspenso;
            RegistrarHistorico(EAcaoEncerramento.Suspenso, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        public void Retomar(Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Suspenso)
            {
                AddNotification(nameof(Status), "So e possivel retomar encerramento Suspenso. [Origem: EncerramentoProjeto]");
                return;
            }
            Status = EProjetoWorkflowStatus.Ativo;
            RegistrarHistorico(EAcaoEncerramento.Retomado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>RN-ENC-012/015: encerramento formal exige motivo auditavel.</summary>
        public void Encerrar(string motivo, Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Ativo)
            {
                AddNotification(nameof(Status), "So e possivel encerrar um processo Ativo. [Origem: EncerramentoProjeto]");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(Status), "O encerramento exige motivo. [Origem: EncerramentoProjeto]");
                return;
            }
            Status = EProjetoWorkflowStatus.Encerrado;
            RegistrarHistorico(EAcaoEncerramento.Encerrado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>RN-ENC-008: arquivamento e estado governado, nao exclusao fisica.</summary>
        public void Arquivar(Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Ativo && Status != EProjetoWorkflowStatus.Encerrado)
            {
                AddNotification(nameof(Status), "So e possivel arquivar encerramento Ativo ou Encerrado. [Origem: EncerramentoProjeto]");
                return;
            }
            StatusFinalProjeto = EStatusFinalProjeto.Arquivado;
            RegistrarHistorico(EAcaoEncerramento.Arquivado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        public void Inativar(Guid usuarioId, string usuario)
        {
            Status = EProjetoWorkflowStatus.Inativo;
            RegistrarHistorico(EAcaoEncerramento.Inativado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }
    }
}
