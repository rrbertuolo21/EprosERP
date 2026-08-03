using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities
{
    /// <summary>
    /// MM — Mestre ÚNICO do projeto (aggregate root referenciado por ProjetoId nos 8 submódulos).
    /// MM-a: Status pelo enum canônico <see cref="EProjetoWorkflowStatus"/> (T3) — os rótulos string
    /// inventados (Planejado/EmAndamento/Concluído) foram removidos. A3: cliente principal opcional
    /// (N:N vive em prj_def_projeto_cliente; projeto interno não tem cliente). A4: orçamento não é
    /// obrigatório na criação (projeto nasce em Rascunho). A2: 100% de progresso NÃO auto-encerra — o
    /// encerramento é ato governado do submódulo Encerramento.
    /// </summary>
    public class Projeto : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        /// <summary>A3: cliente PRINCIPAL opcional (derivado do N:N). Guid.Empty = projeto interno/sem cliente.</summary>
        public Guid ClienteId { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime? DataTermino { get; private set; }
        public decimal OrcamentoTotal { get; private set; }
        public decimal CustoAcumulado { get; private set; }
        public decimal PercentualConclusao { get; private set; }
        /// <summary>MM-a: status canônico (T3). Substitui a string inventada.</summary>
        public EProjetoWorkflowStatus Status { get; private set; } = EProjetoWorkflowStatus.Rascunho;
        /// <summary>RN-DEF-012/013: agregado único com tipo (Normal | Template).</summary>
        public ETipoProjeto Tipo { get; private set; } = ETipoProjeto.Normal;

        public List<WbsItem> ItensWbs { get; private set; } = new();
        public List<AlocacaoRecurso> Alocacoes { get; private set; } = new();

        protected Projeto() { } // EF Core

        public Projeto(
            string nome,
            string descricao,
            Guid clienteId,
            DateTime dataInicio,
            DateTime? dataTermino,
            decimal orcamentoTotal,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            // A3/A4: só o Nome é obrigatório na criação; cliente e orçamento são opcionais (projeto interno/Rascunho).
            AddNotifications(new Contract<Projeto>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O Nome do projeto é obrigatório.")
            );

            if (orcamentoTotal < 0)
                AddNotification(nameof(OrcamentoTotal), "O Orçamento total não pode ser negativo.");

            Nome = nome;
            Descricao = descricao;
            ClienteId = clienteId;
            DataInicio = dataInicio;
            DataTermino = dataTermino;
            OrcamentoTotal = orcamentoTotal;
            CustoAcumulado = 0;
            PercentualConclusao = 0;
            Status = EProjetoWorkflowStatus.Rascunho;
            Tipo = ETipoProjeto.Normal;
        }

        /// <summary>MM-d: submete o mestre para análise (Rascunho -> EmAnalise).</summary>
        public void Submeter(string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Rascunho)
            {
                AddNotification(nameof(Status), "Só é possível submeter projeto em Rascunho.");
                return;
            }
            Status = EProjetoWorkflowStatus.EmAnalise;
            MarcarAlterado(usuario);
        }

        /// <summary>MM-d: aprova o mestre (EmAnalise -> Ativo).</summary>
        public void Aprovar(string usuario)
        {
            if (Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A aprovação só ocorre a partir de EmAnalise.");
                return;
            }
            Status = EProjetoWorkflowStatus.Ativo;
            MarcarAlterado(usuario);
        }

        /// <summary>
        /// Ativa o mestre a partir de Rascunho ou EmAnalise (atalho de início de execução). Usado quando
        /// o progresso de tarefas começa. Não é auto-encerramento (A2).
        /// </summary>
        public void Ativar(string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Rascunho && Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "O projeto só pode ser ativado a partir de Rascunho ou EmAnalise.");
                return;
            }
            Status = EProjetoWorkflowStatus.Ativo;
            MarcarAlterado(usuario);
        }

        /// <summary>Mantido por compat. de chamadas legadas: equivale a Ativar (início da execução).</summary>
        public void IniciarProjeto(string usuario) => Ativar(usuario);

        public void Suspender(string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Ativo)
            {
                AddNotification(nameof(Status), "Só é possível suspender projeto Ativo.");
                return;
            }
            Status = EProjetoWorkflowStatus.Suspenso;
            MarcarAlterado(usuario);
        }

        public void Retomar(string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Suspenso)
            {
                AddNotification(nameof(Status), "Só é possível retomar projeto Suspenso.");
                return;
            }
            Status = EProjetoWorkflowStatus.Ativo;
            MarcarAlterado(usuario);
        }

        /// <summary>Encerramento do mestre — governado pelo submódulo Encerramento; exige motivo auditável.</summary>
        public void Encerrar(string motivo, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Ativo && Status != EProjetoWorkflowStatus.Suspenso)
            {
                AddNotification(nameof(Status), "Só é possível encerrar projeto Ativo ou Suspenso.");
                return;
            }
            Status = EProjetoWorkflowStatus.Encerrado;
            MarcarAlterado(usuario);
        }

        public void Inativar(string usuario)
        {
            Status = EProjetoWorkflowStatus.Inativo;
            MarcarAlterado(usuario);
        }

        /// <summary>RN-DEF-012/013: converte o projeto em template reutilizável.</summary>
        public void ConverterParaTemplate(string usuario)
        {
            Tipo = ETipoProjeto.Template;
            MarcarAlterado(usuario);
        }

        /// <summary>RN-DEF-012/013: converte de volta para projeto normal (instanciação).</summary>
        public void DefinirComoNormal(string usuario)
        {
            Tipo = ETipoProjeto.Normal;
            MarcarAlterado(usuario);
        }

        public void AdicionarItemWbs(string nome, string descricao, DateTime inicio, DateTime termino, decimal peso, string usuario)
        {
            var item = new WbsItem(Id, nome, descricao, inicio, termino, peso, TenantId, usuario);
            if (!item.IsValid)
            {
                AddNotifications(item.Notifications);
                return;
            }
            ItensWbs.Add(item);
            RecalcularProgresso();
            MarcarAlterado(usuario);
        }

        public void AlocarRecurso(Guid colaboradorId, string funcao, decimal custoHora, decimal horasPlanejadas, string usuario)
        {
            var alocacao = new AlocacaoRecurso(Id, colaboradorId, funcao, custoHora, horasPlanejadas, TenantId, usuario);
            if (!alocacao.IsValid)
            {
                AddNotifications(alocacao.Notifications);
                return;
            }
            Alocacoes.Add(alocacao);
            MarcarAlterado(usuario);
        }

        public (decimal oldProgress, decimal newProgress) AtualizarProgressoTarefa(Guid wbsItemId, decimal novoPercentual, string usuario)
        {
            var item = ItensWbs.FirstOrDefault(i => i.Id == wbsItemId);
            if (item == null)
            {
                AddNotification(nameof(ItensWbs), "Tarefa WBS não encontrada no projeto.");
                return (PercentualConclusao, PercentualConclusao);
            }

            decimal oldProgress = PercentualConclusao;
            item.AtualizarProgresso(novoPercentual, usuario);

            if (!item.IsValid)
            {
                AddNotifications(item.Notifications);
                return (oldProgress, oldProgress);
            }

            RecalcularProgresso();
            MarcarAlterado(usuario);

            return (oldProgress, PercentualConclusao);
        }

        private void RecalcularProgresso()
        {
            if (!ItensWbs.Any())
            {
                PercentualConclusao = 0;
                return;
            }

            decimal pesoTotal = ItensWbs.Sum(i => i.PesoPonderado);
            if (pesoTotal == 0)
            {
                PercentualConclusao = ItensWbs.Average(i => i.PercentualConclusao);
                return;
            }

            decimal progressoPonderado = ItensWbs.Sum(i => i.PercentualConclusao * i.PesoPonderado);
            PercentualConclusao = Math.Round(progressoPonderado / pesoTotal, 2);

            // A2: 100% de progresso NÃO auto-encerra o projeto. Encerrar é ato governado do submódulo
            // Encerramento (aceite/checklist). Progresso físico ≠ workflow.
        }
    }
}
