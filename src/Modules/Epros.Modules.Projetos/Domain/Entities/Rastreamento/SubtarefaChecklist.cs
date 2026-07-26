using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Rastreamento
{
    /// <summary>
    /// Item de verificacao/decomposicao de uma tarefa. Origem: EF PRJ-RST 4.4 (prj_rst_subtarefa).
    /// </summary>
    public class SubtarefaChecklist : EntidadeSaaSBase
    {
        public Guid TarefaId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public Guid? ResponsavelId { get; private set; }
        public DateTime? DataPrevista { get; private set; }
        public bool Concluido { get; private set; }

        protected SubtarefaChecklist() { } // EF Core

        public SubtarefaChecklist(Guid tarefaId, string nome, Guid? responsavelId, DateTime? dataPrevista, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<SubtarefaChecklist>()
                .Requires()
                .AreNotEquals(tarefaId, Guid.Empty, nameof(TarefaId), "A tarefa e obrigatoria. [Origem: SubtarefaChecklist]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do item de checklist e obrigatorio. [Origem: SubtarefaChecklist]"));

            TarefaId = tarefaId;
            Nome = nome;
            ResponsavelId = responsavelId;
            DataPrevista = dataPrevista;
            Concluido = false;
        }

        public void MarcarConcluido(bool concluido, string usuario)
        {
            Concluido = concluido;
            MarcarAlterado(usuario);
        }
    }
}
