using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Risco
{
    /// <summary>
    /// Agregado raiz do risco/issue de projeto. Origem: EF PRJ-RSK 11.1 (prj_risco_projeto / ProjectBug).
    /// RN-RSK-001 (projeto obrigatorio), RN-RSK-002 (titulo max 255), RN-RSK-003 (prioridade Low/Medium/High),
    /// RN-RSK-004 (min 1 responsavel), RN-RSK-005 (estagio obrigatorio), RN-RSK-006 (descricao obrigatoria),
    /// RN-RSK-007 (movimentacao valida estagio destino), RN-RSK-013 (sem exclusao fisica com historico),
    /// RN-RSK-015 (rejeicao exige motivo), RN-RSK-016 (acoes geram historico).
    /// Probabilidade/Impacto/Resposta/RiscoResidual sao lacunas controladas (DP-RSK-002/003/004).
    /// </summary>
    public class RiscoProjeto : EntidadeSaaSBase
    {
        public Guid ProjetoId { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public EPrioridadeRisco Prioridade { get; private set; } = EPrioridadeRisco.Medium;
        public string Descricao { get; private set; } = string.Empty;
        public Guid EstagioId { get; private set; }
        public Guid? CriadorId { get; private set; }
        public EProjetoWorkflowStatus Status { get; private set; } = EProjetoWorkflowStatus.Rascunho;
        public int? Probabilidade { get; private set; }
        public int? Impacto { get; private set; }
        public ERespostaRisco? Resposta { get; private set; }
        public string? RiscoResidual { get; private set; }
        public string? MotivoRejeicao { get; private set; }

        public List<ResponsavelRisco> Responsaveis { get; private set; } = new();
        public List<ComentarioRisco> Comentarios { get; private set; } = new();
        public List<HistoricoRisco> Historicos { get; private set; } = new();
        public List<AnexoRisco> Anexos { get; private set; } = new();

        protected RiscoProjeto() { } // EF Core

        public RiscoProjeto(
            Guid projetoId,
            string titulo,
            EPrioridadeRisco prioridade,
            string descricao,
            Guid estagioId,
            IEnumerable<Guid> responsaveis,
            Guid? criadorId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            var listaResponsaveis = (responsaveis ?? Enumerable.Empty<Guid>()).Where(r => r != Guid.Empty).Distinct().ToList();

            AddNotifications(new Contract<RiscoProjeto>()
                .Requires()
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O projeto e obrigatorio. [Origem: RiscoProjeto]")
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O titulo do risco e obrigatorio. [Origem: RiscoProjeto]")
                .IsLowerOrEqualsThan(titulo?.Length ?? 0, 255, nameof(Titulo), "O titulo deve ter no maximo 255 caracteres. [Origem: RiscoProjeto]")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do risco e obrigatoria. [Origem: RiscoProjeto]")
                .AreNotEquals(estagioId, Guid.Empty, nameof(EstagioId), "O estagio e obrigatorio. [Origem: RiscoProjeto]"));

            // RN-RSK-004: responsaveis obrigatorios, minimo 1.
            if (!listaResponsaveis.Any())
                AddNotification(nameof(Responsaveis), "Informe ao menos um responsavel pelo risco. [Origem: RiscoProjeto]");

            ProjetoId = projetoId;
            Titulo = titulo ?? string.Empty;
            Prioridade = prioridade;
            Descricao = descricao ?? string.Empty;
            EstagioId = estagioId;
            CriadorId = criadorId;
            Status = EProjetoWorkflowStatus.Rascunho;

            foreach (var responsavelId in listaResponsaveis)
                Responsaveis.Add(new ResponsavelRisco(Id, responsavelId, TenantId, criadoPor));
        }

        private void RegistrarHistorico(EAcaoRisco acao, Guid usuarioId, string? payloadJson, string usuario)
        {
            Historicos.Add(new HistoricoRisco(Id, acao, usuarioId, payloadJson, null, TenantId, usuario));
        }

        /// <summary>RN-RSK-007: mover valida estagio destino (existencia validada no handler).</summary>
        public void Mover(Guid estagioDestinoId, Guid usuarioId, string usuario)
        {
            if (estagioDestinoId == Guid.Empty)
            {
                AddNotification(nameof(EstagioId), "Estagio destino invalido. [Origem: RiscoProjeto]");
                return;
            }
            if (Status == EProjetoWorkflowStatus.Encerrado)
            {
                AddNotification(nameof(Status), "Risco encerrado nao pode ser movido sem reabertura. [Origem: RiscoProjeto]");
                return;
            }
            EstagioId = estagioDestinoId;
            RegistrarHistorico(EAcaoRisco.Movido, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>RN-RSK-011: comentario obrigatorio e textual.</summary>
        public void Comentar(Guid usuarioId, string comentario, string usuario)
        {
            var novo = new ComentarioRisco(Id, usuarioId, comentario, TenantId, usuario);
            if (!novo.IsValid)
            {
                AddNotifications(novo.Notifications);
                return;
            }
            Comentarios.Add(novo);
            RegistrarHistorico(EAcaoRisco.Comentado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        public void AlterarPrioridade(EPrioridadeRisco prioridade, Guid usuarioId, string usuario)
        {
            Prioridade = prioridade;
            RegistrarHistorico(EAcaoRisco.Alterado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        public void Submeter(Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Rascunho)
            {
                AddNotification(nameof(Status), "So e possivel submeter risco em Rascunho. [Origem: RiscoProjeto]");
                return;
            }
            Status = EProjetoWorkflowStatus.EmAnalise;
            RegistrarHistorico(EAcaoRisco.Alterado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        public void Aprovar(Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A aprovacao somente ocorre a partir de EmAnalise. [Origem: RiscoProjeto]");
                return;
            }
            Status = EProjetoWorkflowStatus.Ativo;
            RegistrarHistorico(EAcaoRisco.Alterado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>RN-RSK-015: rejeicao exige motivo; retorna a Rascunho.</summary>
        public void Rejeitar(string motivo, Guid usuarioId, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A rejeicao somente ocorre a partir de EmAnalise. [Origem: RiscoProjeto]");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoRejeicao), "A rejeicao exige motivo. [Origem: RiscoProjeto]");
                return;
            }
            MotivoRejeicao = motivo;
            Status = EProjetoWorkflowStatus.Rascunho;
            RegistrarHistorico(EAcaoRisco.Alterado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        /// <summary>RN-RSK-005: escalonamento nao transfere propriedade operacional (RN-RSK-014).</summary>
        public void Escalonar(Guid usuarioId, string usuario)
        {
            RegistrarHistorico(EAcaoRisco.Escalonado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        public void Encerrar(string motivo, Guid usuarioId, string usuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(Status), "O encerramento exige motivo. [Origem: RiscoProjeto]");
                return;
            }
            Status = EProjetoWorkflowStatus.Encerrado;
            RegistrarHistorico(EAcaoRisco.Encerrado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }

        public void Inativar(Guid usuarioId, string usuario)
        {
            Status = EProjetoWorkflowStatus.Inativo;
            RegistrarHistorico(EAcaoRisco.Alterado, usuarioId, null, usuario);
            MarcarAlterado(usuario);
        }
    }
}
