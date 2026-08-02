using Epros.Modules.ESG.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>
    /// Base dos agregados-raiz ESG que seguem o ciclo canonico unico (T3 / secao 12 das EFs):
    /// Rascunho → EmAnalise → Ativo → {Suspenso ↔ Ativo | Encerrado | Inativo}.
    /// Reune as transicoes que hoje sao duplicadas em InventarioGee/RegistroEhs; usada pelos
    /// submodulos novos (TSU/DIV) para nao reinventar o workflow.
    /// </summary>
    public abstract class AgregadoEsgComWorkflow : EntidadeSaaSBase
    {
        public EStatusWorkflowEsg Status { get; protected set; } = EStatusWorkflowEsg.Rascunho;

        protected AgregadoEsgComWorkflow() { }
        protected AgregadoEsgComWorkflow(string tenantId, string criadoPor) : base(tenantId, criadoPor) { }

        public void Submeter(string usuario)
        {
            if (Status != EStatusWorkflowEsg.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas registros em rascunho podem ser submetidos.");
                return;
            }
            Status = EStatusWorkflowEsg.EmAnalise;
            MarcarAlterado(usuario);
        }

        public void Aprovar(string usuario)
        {
            if (Status != EStatusWorkflowEsg.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas registros em analise podem ser aprovados.");
                return;
            }
            Status = EStatusWorkflowEsg.Ativo;
            MarcarAlterado(usuario);
        }

        public void Rejeitar(string usuario)
        {
            if (Status != EStatusWorkflowEsg.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas registros em analise podem ser rejeitados.");
                return;
            }
            Status = EStatusWorkflowEsg.Rascunho;
            MarcarAlterado(usuario);
        }

        public void Suspender(string usuario)
        {
            if (Status != EStatusWorkflowEsg.Ativo)
            {
                AddNotification(nameof(Status), "Apenas registros ativos podem ser suspensos.");
                return;
            }
            Status = EStatusWorkflowEsg.Suspenso;
            MarcarAlterado(usuario);
        }

        public void Reativar(string usuario)
        {
            if (Status != EStatusWorkflowEsg.Suspenso && Status != EStatusWorkflowEsg.Inativo)
            {
                AddNotification(nameof(Status), "Apenas registros suspensos ou inativos podem ser reativados.");
                return;
            }
            Status = EStatusWorkflowEsg.Ativo;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            if (Status != EStatusWorkflowEsg.Ativo)
            {
                AddNotification(nameof(Status), "Apenas registros ativos podem ser encerrados.");
                return;
            }
            Status = EStatusWorkflowEsg.Encerrado;
            MarcarAlterado(usuario);
        }

        public void Inativar(string usuario)
        {
            if (Status == EStatusWorkflowEsg.Encerrado)
            {
                AddNotification(nameof(Status), "Registros encerrados nao podem ser inativados.");
                return;
            }
            Status = EStatusWorkflowEsg.Inativo;
            MarcarAlterado(usuario);
        }
    }
}
