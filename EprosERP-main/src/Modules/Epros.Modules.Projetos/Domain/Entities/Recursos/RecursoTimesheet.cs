using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Recursos
{
    /// <summary>
    /// Apontamento de horas de recurso. Origem: EF PRJ-REC 11.1 (prj_recurso_timesheet).
    /// RN-REC-001 (data valida), RN-REC-002 (horas 0..12), RN-REC-003 (minutos 1..60),
    /// RN-REC-004 (tipo), RN-REC-005 (projeto obrigatorio quando type=project).
    /// </summary>
    public class RecursoTimesheet : EntidadeSaaSBase
    {
        public Guid? UsuarioId { get; private set; }
        public Guid? ProjetoId { get; private set; }
        public Guid? TarefaId { get; private set; }
        public DateTime Data { get; private set; }
        public int Horas { get; private set; }
        public int Minutos { get; private set; }
        public string? Notas { get; private set; }
        public ETimesheetTipo Tipo { get; private set; }
        public EProjetoWorkflowStatus Status { get; private set; } = EProjetoWorkflowStatus.Rascunho;
        public int Versao { get; private set; }

        protected RecursoTimesheet() { } // EF Core

        public RecursoTimesheet(
            Guid? usuarioId,
            Guid? projetoId,
            Guid? tarefaId,
            DateTime data,
            int horas,
            int minutos,
            string? notas,
            ETimesheetTipo tipo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            if (horas < 0 || horas > 12)
                AddNotification(nameof(Horas), "As horas devem estar entre 0 e 12. [Origem: RecursoTimesheet]");

            if (minutos < 1 || minutos > 60)
                AddNotification(nameof(Minutos), "Os minutos devem estar entre 1 e 60. [Origem: RecursoTimesheet]");

            if (data == default)
                AddNotification(nameof(Data), "A data do apontamento e obrigatoria. [Origem: RecursoTimesheet]");

            if (tipo == ETimesheetTipo.Project && (projetoId == null || projetoId == Guid.Empty))
                AddNotification(nameof(ProjetoId), "O projeto e obrigatorio quando o tipo e Project. [Origem: RecursoTimesheet]");

            UsuarioId = usuarioId;
            ProjetoId = projetoId;
            TarefaId = tarefaId;
            Data = data;
            Horas = horas;
            Minutos = minutos;
            Notas = notas;
            Tipo = tipo;
            Status = EProjetoWorkflowStatus.Rascunho;
            Versao = 1;
        }

        public void Aprovar(string usuario)
        {
            if (Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A aprovacao so ocorre a partir de EmAnalise. [Origem: RecursoTimesheet]");
                return;
            }
            Status = EProjetoWorkflowStatus.Ativo;
            MarcarAlterado(usuario);
        }

        public void Submeter(string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Rascunho)
            {
                AddNotification(nameof(Status), "So e possivel submeter apontamento em Rascunho. [Origem: RecursoTimesheet]");
                return;
            }
            Status = EProjetoWorkflowStatus.EmAnalise;
            MarcarAlterado(usuario);
        }
    }
}
