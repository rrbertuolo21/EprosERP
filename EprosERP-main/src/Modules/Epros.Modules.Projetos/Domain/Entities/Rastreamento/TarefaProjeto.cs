using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Rastreamento
{
    /// <summary>
    /// Tarefa operacional executavel dentro de um projeto. Origem: EF PRJ-RST 4.2 (prj_rst_tarefa).
    /// PRJ-RST-RN-001 (projeto obrigatorio), RN-002 (titulo max 255), RN-008 (datas coerentes),
    /// RN-009 (percentual 0..100), RN-013 (bloqueada nao conclui).
    /// </summary>
    public class TarefaProjeto : EntidadeSaaSBase
    {
        public Guid ProjetoId { get; private set; }
        public Guid? MarcoId { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public Guid? EstagioId { get; private set; }
        public ETarefaEstado Estado { get; private set; } = ETarefaEstado.Planejada;
        public string? Prioridade { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataTermino { get; private set; }
        public decimal? Duracao { get; private set; }
        public decimal? EsforcoEstimado { get; private set; }
        public decimal? EsforcoRealizado { get; private set; }
        public decimal PercentualConcluido { get; private set; }
        public Guid? TarefaSuperiorId { get; private set; }
        public bool IndicadorMarco { get; private set; }
        public string? Visibilidade { get; private set; }
        public bool Ativo { get; private set; } = true;
        public int Ordem { get; private set; }

        protected TarefaProjeto() { } // EF Core

        public TarefaProjeto(
            Guid projetoId,
            string titulo,
            string? descricao,
            Guid? estagioId,
            Guid? marcoId,
            string? prioridade,
            DateTime? dataInicio,
            DateTime? dataTermino,
            decimal? duracao,
            decimal? esforcoEstimado,
            Guid? tarefaSuperiorId,
            bool indicadorMarco,
            string? visibilidade,
            int ordem,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<TarefaProjeto>()
                .Requires()
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O projeto e obrigatorio. [Origem: TarefaProjeto]")
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O titulo da tarefa e obrigatorio. [Origem: TarefaProjeto]")
                .IsLowerOrEqualsThan(titulo?.Length ?? 0, 255, nameof(Titulo), "O titulo deve ter no maximo 255 caracteres. [Origem: TarefaProjeto]"));

            if (dataInicio.HasValue && dataTermino.HasValue && dataTermino < dataInicio)
                AddNotification(nameof(DataTermino), "A data de termino nao pode ser anterior a data de inicio. [Origem: TarefaProjeto]");

            ProjetoId = projetoId;
            Titulo = titulo ?? string.Empty;
            Descricao = descricao;
            EstagioId = estagioId;
            MarcoId = marcoId;
            Prioridade = prioridade;
            DataInicio = dataInicio;
            DataTermino = dataTermino;
            Duracao = duracao;
            EsforcoEstimado = esforcoEstimado;
            EsforcoRealizado = 0;
            PercentualConcluido = 0;
            TarefaSuperiorId = tarefaSuperiorId;
            IndicadorMarco = indicadorMarco;
            Visibilidade = visibilidade;
            Estado = ETarefaEstado.Planejada;
            Ativo = true;
            Ordem = ordem;
        }

        /// <summary>PRJ-RST-RN-019: mover no quadro atualiza estagio e posicao.</summary>
        public void MoverNoQuadro(Guid estagioId, int novaOrdem, bool estagioConclui, string usuario)
        {
            EstagioId = estagioId;
            Ordem = novaOrdem;
            if (estagioConclui)
            {
                PercentualConcluido = 100;
                Estado = ETarefaEstado.Concluida;
            }
            else if (Estado == ETarefaEstado.Planejada)
            {
                Estado = ETarefaEstado.EmExecucao;
            }
            MarcarAlterado(usuario);
        }

        /// <summary>PRJ-RST-RN-009: percentual 0..100.</summary>
        public void AtualizarProgresso(decimal percentual, string usuario)
        {
            if (percentual < 0 || percentual > 100)
            {
                AddNotification(nameof(PercentualConcluido), "O percentual concluido deve estar entre 0 e 100. [Origem: TarefaProjeto]");
                return;
            }

            PercentualConcluido = percentual;
            if (percentual == 0) Estado = ETarefaEstado.Planejada;
            else if (percentual == 100) Estado = ETarefaEstado.Concluida;
            else if (Estado != ETarefaEstado.Bloqueada) Estado = ETarefaEstado.EmExecucao;

            MarcarAlterado(usuario);
        }

        /// <summary>PRJ-RST-RN-013: tarefa bloqueada por dependencia pendente nao pode concluir.</summary>
        public void Concluir(bool possuiDependenciaAberta, string usuario)
        {
            if (possuiDependenciaAberta || Estado == ETarefaEstado.Bloqueada)
            {
                AddNotification(nameof(Estado), "Tarefa bloqueada por dependencia pendente nao pode ser concluida. [Origem: TarefaProjeto]");
                return;
            }
            PercentualConcluido = 100;
            Estado = ETarefaEstado.Concluida;
            MarcarAlterado(usuario);
        }

        public void Bloquear(string usuario)
        {
            Estado = ETarefaEstado.Bloqueada;
            MarcarAlterado(usuario);
        }

        /// <summary>PRJ-RST-RN-020: arquivar remove das visoes ativas sem excluir historico.</summary>
        public void Arquivar(string usuario)
        {
            Ativo = false;
            Estado = ETarefaEstado.Arquivada;
            MarcarAlterado(usuario);
        }
    }
}
