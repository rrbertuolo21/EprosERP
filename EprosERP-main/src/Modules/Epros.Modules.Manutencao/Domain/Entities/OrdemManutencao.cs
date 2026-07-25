using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Manutencao.Domain.Entities
{
    public class OrdemManutencao : EntidadeSaaSBase
    {
        public Guid EquipamentoId { get; private set; }
        public string Tipo { get; private set; } = string.Empty; // Corretiva, Preventiva, Preditiva
        public string DescricaoProblema { get; private set; } = string.Empty;
        public string DescricaoServicoExecutado { get; private set; } = string.Empty;
        public string Responsavel { get; private set; } = string.Empty;
        public string Status { get; private set; } = "Aberta"; // Aberta, EmExecucao, Concluida, Cancelada
        public DateTime DataAbertura { get; private set; }
        public DateTime? DataConclusao { get; private set; }
        public decimal CustoPecas { get; private set; }
        public decimal CustoMaoObra { get; private set; }

        public List<OrdemManutencaoPeca> Pecas { get; private set; } = new();

        protected OrdemManutencao() { } // EF Core

        public OrdemManutencao(
            Guid equipamentoId,
            string tipo,
            string descricaoProblema,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<OrdemManutencao>()
                .Requires()
                .AreNotEquals(equipamentoId, Guid.Empty, nameof(EquipamentoId), "O ID do equipamento é obrigatório.")
                .IsTrue(tipo == "Corretiva" || tipo == "Preventiva" || tipo == "Preditiva", nameof(Tipo), "O Tipo de OS deve ser 'Corretiva', 'Preventiva' ou 'Preditiva'.")
                .IsNotNullOrEmpty(descricaoProblema, nameof(DescricaoProblema), "A descrição do problema é obrigatória.")
            );

            EquipamentoId = equipamentoId;
            Tipo = tipo;
            DescricaoProblema = descricaoProblema;
            Status = "Aberta";
            DataAbertura = DateTime.UtcNow;
            CustoPecas = 0;
            CustoMaoObra = 0;
        }

        public void IniciarExecucao(string usuario)
        {
            if (Status != "Aberta")
            {
                AddNotification(nameof(Status), "A ordem de serviço só pode ser iniciada a partir do status Aberta.");
                return;
            }
            Status = "EmExecucao";
            MarcarAlterado(usuario);
        }

        public void AdicionarPeca(Guid produtoId, decimal quantidade, string usuario)
        {
            if (Status != "Aberta" && Status != "EmExecucao")
            {
                AddNotification(nameof(Status), "Não é possível adicionar peças a uma ordem finalizada ou cancelada.");
                return;
            }

            var peca = new OrdemManutencaoPeca(Id, produtoId, quantidade, TenantId, usuario);
            if (!peca.IsValid)
            {
                AddNotifications(peca.Notifications);
                return;
            }

            Pecas.Add(peca);
            MarcarAlterado(usuario);
        }

        public void ConcluirOrdem(string servicoExecutado, decimal custoMaoObra, string usuario)
        {
            if (Status != "EmExecucao" && Status != "Aberta")
            {
                AddNotification(nameof(Status), "Apenas ordens abertas ou em execução podem ser concluídas.");
                return;
            }

            if (string.IsNullOrWhiteSpace(servicoExecutado))
            {
                AddNotification(nameof(DescricaoServicoExecutado), "A descrição do serviço executado é obrigatória para conclusão.");
                return;
            }

            Status = "Concluida";
            DescricaoServicoExecutado = servicoExecutado;
            CustoMaoObra = custoMaoObra;
            DataConclusao = DateTime.UtcNow;
            Responsavel = usuario;
            MarcarAlterado(usuario);
        }
    }
}
