using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class OrcamentoManutencao : EntidadeSaaSBase
    {
        public Guid OrdemServicoId { get; private set; }
        public int Versao { get; private set; } = 1;
        public DateTime Validade { get; private set; }
        public decimal ValorTotal { get; private set; }
        public string Status { get; private set; } = "EmAnalise"; // EmAnalise, Aprovado, Rejeitado

        protected OrcamentoManutencao() { } // EF Core

        public OrcamentoManutencao(
            Guid ordemServicoId,
            DateTime validade,
            decimal valorTotal,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<OrcamentoManutencao>()
                .Requires()
                .AreNotEquals(ordemServicoId, Guid.Empty, nameof(OrdemServicoId), "A ordem de serviço é obrigatória no orçamento.")
                .IsGreaterThan(valorTotal, -0.001m, nameof(ValorTotal), "O valor total do orçamento não pode ser negativo.")
            );

            OrdemServicoId = ordemServicoId;
            Versao = 1;
            Validade = validade;
            ValorTotal = valorTotal;
            Status = "EmAnalise";
        }

        public void Aprovar(string usuario)
        {
            if (Status != "EmAnalise")
            {
                AddNotification(nameof(Status), "Apenas orçamentos em análise podem ser aprovados.");
                return;
            }

            Status = "Aprovado";
            MarcarAlterado(usuario);
        }

        public void Rejeitar(string usuario)
        {
            Status = "Rejeitado";
            MarcarAlterado(usuario);
        }
    }
}
