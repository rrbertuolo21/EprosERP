using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    public class FolhaPagamento : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public int MesCompetencia { get; private set; }
        public int AnoCompetencia { get; private set; }
        public decimal SalarioBruto { get; private set; }
        public decimal Descontos { get; private set; }
        public decimal SalarioLiquido { get; private set; }
        public string Status { get; private set; } = "Processada"; // Processada, Paga
        public DateTime DataProcessamento { get; private set; }
        public List<FolhaPagamentoVerba> Verbas { get; private set; } = new();

        protected FolhaPagamento() { } // EF Core

        public FolhaPagamento(
            Guid colaboradorId,
            int mesCompetencia,
            int anoCompetencia,
            decimal salarioBruto,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<FolhaPagamento>()
                .Requires()
                .AreNotEquals(colaboradorId, Guid.Empty, nameof(ColaboradorId), "O ID do colaborador é obrigatório.")
                .IsBetween(mesCompetencia, 1, 12, nameof(MesCompetencia), "O mês de competência deve ser de 1 a 12.")
                .IsBetween(anoCompetencia, 2000, 2100, nameof(AnoCompetencia), "O ano de competência é inválido.")
                .IsGreaterThan(salarioBruto, 0, nameof(SalarioBruto), "O salário bruto deve ser maior que zero.")
            );

            ColaboradorId = colaboradorId;
            MesCompetencia = mesCompetencia;
            AnoCompetencia = anoCompetencia;
            SalarioBruto = salarioBruto;
            Descontos = 0;
            SalarioLiquido = salarioBruto;
            Status = "Processada";
            DataProcessamento = DateTime.UtcNow;
        }

        public void AdicionarVerba(string descricao, string tipo, decimal valor, string criadoPor)
        {
            if (Status != "Processada")
            {
                AddNotification(nameof(Status), "Não é possível adicionar verbas a uma folha que não está no status Processada.");
                return;
            }

            var verba = new FolhaPagamentoVerba(Id, descricao, tipo, valor, TenantId, criadoPor);
            if (!verba.IsValid)
            {
                AddNotifications(verba.Notifications);
                return;
            }

            Verbas.Add(verba);

            if (tipo == "Provento")
            {
                SalarioBruto += valor;
            }
            else if (tipo == "Desconto")
            {
                Descontos += valor;
            }

            SalarioLiquido = SalarioBruto - Descontos;
        }

        public void MarcarComoPaga(string usuario)
        {
            if (Status == "Paga")
            {
                AddNotification(nameof(Status), "Esta folha de pagamento já está paga.");
                return;
            }

            Status = "Paga";
            MarcarAlterado(usuario);
        }
    }
}
