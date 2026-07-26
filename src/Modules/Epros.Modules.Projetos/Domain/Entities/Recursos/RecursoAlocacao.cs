using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Recursos
{
    /// <summary>
    /// Alocacao de recurso a projeto/tarefa com capacidade planejada. Origem: EF PRJ-REC 11.2 (prj_recurso_alocacao).
    /// RN-REC-010: alocacao deve referenciar usuario e (quando aplicavel) tarefa existentes.
    /// Entidade distinta da AlocacaoRecurso simples pre-existente (agregado enriquecido do submodulo REC).
    /// </summary>
    public class RecursoAlocacao : EntidadeSaaSBase
    {
        public Guid RecursoId { get; private set; }
        public Guid ProjetoId { get; private set; }
        public Guid? TarefaId { get; private set; }
        public string? PapelNoProjeto { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public decimal? CargaPlanejadaHoras { get; private set; }
        public EProjetoWorkflowStatus Status { get; private set; } = EProjetoWorkflowStatus.Rascunho;

        protected RecursoAlocacao() { } // EF Core

        public RecursoAlocacao(
            Guid recursoId,
            Guid projetoId,
            Guid? tarefaId,
            string? papelNoProjeto,
            DateTime? dataInicio,
            DateTime? dataFim,
            decimal? cargaPlanejadaHoras,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RecursoAlocacao>()
                .Requires()
                .AreNotEquals(recursoId, Guid.Empty, nameof(RecursoId), "O recurso e obrigatorio. [Origem: RecursoAlocacao]")
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O projeto e obrigatorio. [Origem: RecursoAlocacao]"));

            if (dataInicio.HasValue && dataFim.HasValue && dataFim < dataInicio)
                AddNotification(nameof(DataFim), "A data final nao pode ser anterior a data inicial. [Origem: RecursoAlocacao]");

            RecursoId = recursoId;
            ProjetoId = projetoId;
            TarefaId = tarefaId;
            PapelNoProjeto = papelNoProjeto;
            DataInicio = dataInicio;
            DataFim = dataFim;
            CargaPlanejadaHoras = cargaPlanejadaHoras;
            Status = EProjetoWorkflowStatus.Rascunho;
        }
    }
}
