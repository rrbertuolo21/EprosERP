using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    /// <summary>
    /// Governança de upgrade/versão do Super Admin (APP-TEN-010 / REG-015 / REG-019).
    /// Formaliza o pedido de atualização com segregação de funções (maker-checker), bloqueio de
    /// reexecução acidental e registro de rollback, gateando o <see cref="UpdateLog"/>.
    /// </summary>
    public class SolicitacaoUpgradeVersao : EntidadeSaaSBase
    {
        public string VersaoAtual { get; private set; } = string.Empty;
        public string VersaoAlvo { get; private set; } = string.Empty;
        public string Motivo { get; private set; } = string.Empty;
        public EStatusUpgradeVersao Status { get; private set; }
        public string? SolicitadoPor { get; private set; }
        public string? AprovadoPor { get; private set; }
        public string? Comentario { get; private set; }
        public DateTime? AprovadoEm { get; private set; }
        public DateTime? ExecutadoEm { get; private set; }
        public string? Log { get; private set; }
        public bool RollbackDisponivel { get; private set; }

        protected SolicitacaoUpgradeVersao() { } // EF Core

        public SolicitacaoUpgradeVersao(
            string versaoAtual,
            string versaoAlvo,
            string motivo,
            string solicitadoPor,
            bool rollbackDisponivel,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            VersaoAtual = versaoAtual;
            VersaoAlvo = versaoAlvo;
            Motivo = motivo;
            SolicitadoPor = solicitadoPor;
            RollbackDisponivel = rollbackDisponivel;
            Status = EStatusUpgradeVersao.Solicitado;
            Validar();
        }

        public void Aprovar(string aprovadoPor, string? comentario, string alteradoPor)
        {
            Clear();
            if (Status != EStatusUpgradeVersao.Solicitado)
            {
                AddNotification(nameof(Status), "Só é possível aprovar um upgrade em estado Solicitado [Origem: SolicitacaoUpgradeVersao]");
                return;
            }
            if (!string.IsNullOrWhiteSpace(SolicitadoPor) && aprovadoPor == SolicitadoPor)
            {
                AddNotification(nameof(AprovadoPor), "O aprovador do upgrade deve ser diferente do solicitante (segregação de funções) [Origem: SolicitacaoUpgradeVersao]");
                return;
            }
            Status = EStatusUpgradeVersao.Aprovado;
            AprovadoPor = aprovadoPor;
            Comentario = comentario;
            AprovadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Rejeitar(string aprovadoPor, string? comentario, string alteradoPor)
        {
            Clear();
            if (Status != EStatusUpgradeVersao.Solicitado)
            {
                AddNotification(nameof(Status), "Só é possível rejeitar um upgrade em estado Solicitado [Origem: SolicitacaoUpgradeVersao]");
                return;
            }
            Status = EStatusUpgradeVersao.Rejeitado;
            AprovadoPor = aprovadoPor;
            Comentario = comentario;
            AprovadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void IniciarExecucao(string alteradoPor)
        {
            Clear();
            if (Status != EStatusUpgradeVersao.Aprovado)
            {
                AddNotification(nameof(Status), "O upgrade só pode ser executado após aprovação (bloqueio de reexecução) [Origem: SolicitacaoUpgradeVersao]");
                return;
            }
            Status = EStatusUpgradeVersao.EmExecucao;
            MarcarAlterado(alteradoPor);
        }

        public void ConcluirComSucesso(string? log, string alteradoPor)
        {
            Status = EStatusUpgradeVersao.Concluido;
            Log = log;
            ExecutadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void RegistrarFalha(string? log, string alteradoPor)
        {
            Status = EStatusUpgradeVersao.Falho;
            Log = log;
            ExecutadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void AplicarRollback(string? log, string alteradoPor)
        {
            Clear();
            if (Status != EStatusUpgradeVersao.Falho && Status != EStatusUpgradeVersao.Concluido)
            {
                AddNotification(nameof(Status), "O rollback só é permitido após execução (Concluído ou Falho) [Origem: SolicitacaoUpgradeVersao]");
                return;
            }
            if (!RollbackDisponivel)
            {
                AddNotification(nameof(RollbackDisponivel), "Não há ponto de rollback disponível para este upgrade [Origem: SolicitacaoUpgradeVersao]");
                return;
            }
            Status = EStatusUpgradeVersao.RollbackAplicado;
            Log = log;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<SolicitacaoUpgradeVersao>()
                .Requires()
                .IsNotNullOrEmpty(VersaoAtual, nameof(VersaoAtual), "A versão atual é obrigatória [Origem: SolicitacaoUpgradeVersao]")
                .IsNotNullOrEmpty(VersaoAlvo, nameof(VersaoAlvo), "A versão alvo é obrigatória [Origem: SolicitacaoUpgradeVersao]")
                .IsNotNullOrEmpty(Motivo, nameof(Motivo), "O motivo do upgrade é obrigatório [Origem: SolicitacaoUpgradeVersao]"));
        }
    }
}
