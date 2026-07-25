using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Definicao
{
    /// <summary>
    /// Estrutura inicial de tarefa copiavel em template/duplicacao. Origem: EF PRJ-DEF 11.6 (prj_def_tarefa_modelo).
    /// RN-DEF-014: estrutura copiada preserva contexto; execucao detalhada fica em PRJ-RST.
    /// </summary>
    public class ProjetoTarefaModelo : EntidadeSaaSBase
    {
        public Guid ProjetoId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public int TarefaSequencial { get; private set; }
        public int? TarefaPaiSequencial { get; private set; }
        public decimal Duracao { get; private set; }
        public string? UnidadeDuracao { get; private set; }
        public int PercentualCompleto { get; private set; }

        protected ProjetoTarefaModelo() { } // EF Core

        public ProjetoTarefaModelo(
            Guid projetoId,
            string nome,
            int tarefaSequencial,
            int? tarefaPaiSequencial,
            decimal duracao,
            string? unidadeDuracao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ProjetoTarefaModelo>()
                .Requires()
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O projeto e obrigatorio. [Origem: ProjetoTarefaModelo]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome da tarefa modelo e obrigatorio. [Origem: ProjetoTarefaModelo]"));

            ProjetoId = projetoId;
            Nome = nome;
            TarefaSequencial = tarefaSequencial;
            TarefaPaiSequencial = tarefaPaiSequencial;
            Duracao = duracao;
            UnidadeDuracao = unidadeDuracao;
            PercentualCompleto = 0;
        }
    }
}
