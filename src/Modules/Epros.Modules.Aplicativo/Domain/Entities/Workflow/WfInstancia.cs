using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Workflow
{
    /// <summary>
    /// wf_instancia — execução do workflow sobre um registro de negócio específico. Concentra a
    /// máquina de estados do ciclo de vida principal (Rascunho→EmAnalise→Ativo, etc.). [Origem: EF WORKFLOW 10.6]
    /// </summary>
    public class WfInstancia : EntidadeSaaSBase
    {
        public Guid DefinicaoId { get; private set; }
        public string EntidadeTipo { get; private set; } = string.Empty;
        public string EntidadeIdReferencia { get; private set; } = string.Empty;
        public Guid? EstadoAtualId { get; private set; }
        public Guid? ResponsavelUsuarioId { get; private set; }
        public EWfInstanciaStatus Status { get; private set; }
        // Dados de apoio para o contrato de aprovação por alçada (PLT-WF)
        public string Modulo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public decimal? ValorReferencia { get; private set; }
        public string? CommandType { get; private set; }
        public string? Payload { get; private set; }
        public string? Comentario { get; private set; }
        public Guid? AprovadoPorUsuarioId { get; private set; }
        public string? AprovadoPor { get; private set; }
        public DateTime? DecididoEm { get; private set; }

        protected WfInstancia() { } // EF Core

        public WfInstancia(
            Guid definicaoId,
            string modulo,
            string entidadeTipo,
            string entidadeIdReferencia,
            string descricao,
            Guid? estadoAtualId,
            Guid? responsavelUsuarioId,
            decimal? valorReferencia,
            string? commandType,
            string? payload,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            DefinicaoId = definicaoId;
            Modulo = modulo;
            EntidadeTipo = entidadeTipo;
            EntidadeIdReferencia = entidadeIdReferencia;
            Descricao = descricao;
            EstadoAtualId = estadoAtualId;
            ResponsavelUsuarioId = responsavelUsuarioId;
            ValorReferencia = valorReferencia;
            CommandType = commandType;
            Payload = payload;
            Status = EWfInstanciaStatus.Rascunho;
            Validar();
        }

        public void Submeter(Guid? estadoDestinoId, string alteradoPor)
        {
            Clear();
            if (Status != EWfInstanciaStatus.Rascunho)
            {
                AddNotification(nameof(Status), "A submissão só é permitida a partir do estado Rascunho [Origem: WfInstancia]");
                return;
            }
            Status = EWfInstanciaStatus.EmAnalise;
            if (estadoDestinoId.HasValue) EstadoAtualId = estadoDestinoId;
            MarcarAlterado(alteradoPor);
        }

        public void Aprovar(Guid? estadoDestinoId, Guid? aprovadoPorUsuarioId, string aprovadoPorUserId, string? comentario, string alteradoPor)
        {
            Clear();
            if (Status != EWfInstanciaStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A aprovação só é permitida a partir do estado EmAnalise [Origem: WfInstancia]");
                return;
            }
            if (!string.IsNullOrWhiteSpace(CriadoPor) && aprovadoPorUserId == CriadoPor)
            {
                AddNotification(nameof(AprovadoPor), "O aprovador deve ser diferente do criador (segregação de funções) [Origem: WfInstancia]");
                return;
            }
            Status = EWfInstanciaStatus.Ativo;
            if (estadoDestinoId.HasValue) EstadoAtualId = estadoDestinoId;
            AprovadoPorUsuarioId = aprovadoPorUsuarioId;
            AprovadoPor = aprovadoPorUserId;
            Comentario = comentario;
            DecididoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Rejeitar(Guid? estadoDestinoId, Guid? aprovadoPorUsuarioId, string aprovadoPorUserId, string? comentario, string alteradoPor)
        {
            Clear();
            if (Status != EWfInstanciaStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A rejeição só é permitida a partir do estado EmAnalise [Origem: WfInstancia]");
                return;
            }
            Status = EWfInstanciaStatus.Rejeitado;
            if (estadoDestinoId.HasValue) EstadoAtualId = estadoDestinoId;
            AprovadoPorUsuarioId = aprovadoPorUsuarioId;
            AprovadoPor = aprovadoPorUserId;
            Comentario = comentario;
            DecididoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Inativar(Guid? estadoDestinoId, string alteradoPor)
        {
            Clear();
            if (Status != EWfInstanciaStatus.Ativo)
            {
                AddNotification(nameof(Status), "A inativação só é permitida a partir do estado Ativo [Origem: WfInstancia]");
                return;
            }
            Status = EWfInstanciaStatus.Inativo;
            if (estadoDestinoId.HasValue) EstadoAtualId = estadoDestinoId;
            MarcarAlterado(alteradoPor);
        }

        public void Encerrar(Guid? estadoDestinoId, string alteradoPor)
        {
            Clear();
            if (Status != EWfInstanciaStatus.Ativo)
            {
                AddNotification(nameof(Status), "O encerramento só é permitido a partir do estado Ativo [Origem: WfInstancia]");
                return;
            }
            Status = EWfInstanciaStatus.Encerrado;
            if (estadoDestinoId.HasValue) EstadoAtualId = estadoDestinoId;
            MarcarAlterado(alteradoPor);
        }

        public void Reativar(Guid? estadoDestinoId, string alteradoPor)
        {
            Clear();
            if (Status != EWfInstanciaStatus.Inativo)
            {
                AddNotification(nameof(Status), "A reativação só é permitida a partir do estado Inativo [Origem: WfInstancia]");
                return;
            }
            Status = EWfInstanciaStatus.Ativo;
            if (estadoDestinoId.HasValue) EstadoAtualId = estadoDestinoId;
            MarcarAlterado(alteradoPor);
        }

        public void DefinirResponsavel(Guid? responsavelUsuarioId, string alteradoPor)
        {
            ResponsavelUsuarioId = responsavelUsuarioId;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<WfInstancia>()
                .Requires()
                .AreNotEquals(DefinicaoId, Guid.Empty, nameof(DefinicaoId), "A definição usada é obrigatória [Origem: WfInstancia]")
                .IsNotNullOrEmpty(EntidadeTipo, nameof(EntidadeTipo), "O tipo da entidade controlada é obrigatório [Origem: WfInstancia]")
                .IsNotNullOrEmpty(EntidadeIdReferencia, nameof(EntidadeIdReferencia), "O identificador do registro controlado é obrigatório [Origem: WfInstancia]"));
        }
    }
}
