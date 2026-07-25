using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Rastreamento
{
    /// <summary>
    /// Dependencia funcional entre tarefas. Origem: EF PRJ-RST 4.5 (prj_rst_dependencia).
    /// PRJ-RST-RN-013: dependencia impeditiva aberta bloqueia conclusao da tarefa dependente.
    /// </summary>
    public class DependenciaTarefa : EntidadeSaaSBase
    {
        public Guid TarefaDependenteId { get; private set; }
        public Guid TarefaPredecessoraId { get; private set; }
        public ETipoDependencia TipoDependencia { get; private set; }
        public bool Bloqueada { get; private set; }
        public string? Observacao { get; private set; }

        protected DependenciaTarefa() { } // EF Core

        public DependenciaTarefa(
            Guid tarefaDependenteId,
            Guid tarefaPredecessoraId,
            ETipoDependencia tipoDependencia,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DependenciaTarefa>()
                .Requires()
                .AreNotEquals(tarefaDependenteId, Guid.Empty, nameof(TarefaDependenteId), "A tarefa dependente e obrigatoria. [Origem: DependenciaTarefa]")
                .AreNotEquals(tarefaPredecessoraId, Guid.Empty, nameof(TarefaPredecessoraId), "A tarefa predecessora e obrigatoria. [Origem: DependenciaTarefa]"));

            if (tarefaDependenteId == tarefaPredecessoraId)
                AddNotification(nameof(TarefaPredecessoraId), "Uma tarefa nao pode depender de si mesma. [Origem: DependenciaTarefa]");

            TarefaDependenteId = tarefaDependenteId;
            TarefaPredecessoraId = tarefaPredecessoraId;
            TipoDependencia = tipoDependencia;
            Bloqueada = true;
            Observacao = observacao;
        }

        public void Liberar(string usuario)
        {
            Bloqueada = false;
            MarcarAlterado(usuario);
        }
    }
}
