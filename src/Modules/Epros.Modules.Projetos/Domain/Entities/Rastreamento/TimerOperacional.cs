using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Rastreamento
{
    /// <summary>
    /// Intervalo de acompanhamento operacional de uma tarefa. Origem: EF PRJ-RST 4.7 (prj_rst_timer).
    /// O fechamento formal de horas pertence a PRJ-REC (Gestao de Recursos).
    /// </summary>
    public class TimerOperacional : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public Guid ProjetoId { get; private set; }
        public Guid TarefaId { get; private set; }
        public DateTime Inicio { get; private set; }
        public DateTime? Fim { get; private set; }
        public decimal? Duracao { get; private set; }
        public string? TipoRegistro { get; private set; }
        public string? Observacao { get; private set; }

        protected TimerOperacional() { } // EF Core

        public TimerOperacional(Guid usuarioId, Guid projetoId, Guid tarefaId, DateTime inicio, string? tipoRegistro, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<TimerOperacional>()
                .Requires()
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O usuario e obrigatorio. [Origem: TimerOperacional]")
                .AreNotEquals(tarefaId, Guid.Empty, nameof(TarefaId), "A tarefa e obrigatoria. [Origem: TimerOperacional]"));

            UsuarioId = usuarioId;
            ProjetoId = projetoId;
            TarefaId = tarefaId;
            Inicio = inicio;
            TipoRegistro = tipoRegistro;
            Observacao = observacao;
        }

        /// <summary>PRJ-RST-RN-015: encerrar timer aberto ao concluir tarefa.</summary>
        public void Encerrar(DateTime fim, string usuario)
        {
            if (fim < Inicio)
            {
                AddNotification(nameof(Fim), "O fim do timer nao pode ser anterior ao inicio. [Origem: TimerOperacional]");
                return;
            }
            Fim = fim;
            Duracao = (decimal)(fim - Inicio).TotalHours;
            MarcarAlterado(usuario);
        }
    }
}
