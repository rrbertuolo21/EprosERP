using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities
{
    public class WbsItem : EntidadeSaaSBase
    {
        public Guid ProjetoId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public DateTime DataInicio { get; private set; }
        public DateTime DataTermino { get; private set; }
        public decimal PercentualConclusao { get; private set; }
        public decimal PesoPonderado { get; private set; }
        public string Status { get; private set; } = "Pendente"; // Pendente, EmAndamento, Concluido

        protected WbsItem() { } // EF Core

        public WbsItem(
            Guid projetoId,
            string nome,
            string descricao,
            DateTime dataInicio,
            DateTime dataTermino,
            decimal pesoPonderado,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<WbsItem>()
                .Requires()
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O ID do projeto é obrigatório.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O Nome da tarefa é obrigatório.")
                .IsGreaterThan(pesoPonderado, 0, nameof(PesoPonderado), "O Peso ponderado deve ser maior que zero.")
            );

            ProjetoId = projetoId;
            Nome = nome;
            Descricao = descricao;
            DataInicio = dataInicio;
            DataTermino = dataTermino;
            PesoPonderado = pesoPonderado;
            PercentualConclusao = 0;
            Status = "Pendente";
        }

        public void AtualizarProgresso(decimal novoPercentual, string usuario)
        {
            if (novoPercentual < 0 || novoPercentual > 100)
            {
                AddNotification(nameof(PercentualConclusao), "O percentual de conclusão deve ser entre 0 e 100.");
                return;
            }

            PercentualConclusao = novoPercentual;

            if (PercentualConclusao == 0)
            {
                Status = "Pendente";
            }
            else if (PercentualConclusao == 100)
            {
                Status = "Concluido";
            }
            else
            {
                Status = "EmAndamento";
            }

            MarcarAlterado(usuario);
        }
    }
}
