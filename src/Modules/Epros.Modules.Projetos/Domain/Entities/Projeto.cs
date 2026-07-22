using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities
{
    public class Projeto : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public Guid ClienteId { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime? DataTermino { get; private set; }
        public decimal OrcamentoTotal { get; private set; }
        public decimal CustoAcumulado { get; private set; }
        public decimal PercentualConclusao { get; private set; }
        public string Status { get; private set; } = "Planejado"; // Planejado, EmAndamento, Suspenso, Concluido

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
            AddNotifications(new Contract<Projeto>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O Nome do projeto é obrigatório.")
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "O Cliente do projeto é obrigatório.")
                .IsGreaterThan(orcamentoTotal, 0, nameof(OrcamentoTotal), "O Orçamento total deve ser maior que zero.")
            );

            Nome = nome;
            Descricao = descricao;
            ClienteId = clienteId;
            DataInicio = dataInicio;
            DataTermino = dataTermino;
            OrcamentoTotal = orcamentoTotal;
            CustoAcumulado = 0;
            PercentualConclusao = 0;
            Status = "Planejado";
        }

        public void IniciarProjeto(string usuario)
        {
            if (Status != "Planejado")
            {
                AddNotification(nameof(Status), "O projeto só pode ser iniciado a partir do status Planejado.");
                return;
            }
            Status = "EmAndamento";
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

            if (PercentualConclusao >= 100m && Status == "EmAndamento")
            {
                Status = "Concluido";
            }
        }
    }
}
