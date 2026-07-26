using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class VeiculoGarantia : EntidadeSaaSBase
    {
        public Guid VeiculoId { get; private set; }
        public Guid VendaId { get; private set; }
        public string ChassiVin { get; private set; } = string.Empty; // VIN de 17 caracteres
        public Guid PlanoVersaoId { get; private set; }
        public DateTime DataEntrega { get; private set; }
        public DateTime InicioVigencia { get; private set; }
        public DateTime FimVigencia { get; private set; }
        public decimal? QuilometragemInicio { get; private set; }
        public decimal? QuilometragemLimite { get; private set; }
        public string Status { get; private set; } = "Ativa";

        protected VeiculoGarantia() { } // EF Core

        public VeiculoGarantia(
            Guid veiculoId,
            Guid vendaId,
            string chassiVin,
            Guid planoVersaoId,
            DateTime dataEntrega,
            DateTime inicioVigencia,
            DateTime fimVigencia,
            decimal? quilometragemInicio,
            decimal? quilometragemLimite,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<VeiculoGarantia>()
                .Requires()
                .AreNotEquals(veiculoId, Guid.Empty, nameof(VeiculoId), "O veículo é obrigatório.")
                .AreNotEquals(vendaId, Guid.Empty, nameof(VendaId), "A venda é obrigatória.")
                .IsNotNullOrEmpty(chassiVin, nameof(ChassiVin), "O chassi/VIN é obrigatório.")
                .AreEquals(chassiVin?.Length ?? 0, 17, nameof(ChassiVin), "O chassi/VIN deve possuir exatamente 17 caracteres.")
                .AreNotEquals(planoVersaoId, Guid.Empty, nameof(PlanoVersaoId), "A versão do plano é obrigatória.")
            );

            if (fimVigencia <= inicioVigencia)
            {
                AddNotification(nameof(FimVigencia), "A data de fim da vigência deve ser posterior ao início.");
            }

            VeiculoId = veiculoId;
            VendaId = vendaId;
            ChassiVin = chassiVin?.ToUpperInvariant() ?? string.Empty;
            PlanoVersaoId = planoVersaoId;
            DataEntrega = dataEntrega;
            InicioVigencia = inicioVigencia;
            FimVigencia = fimVigencia;
            QuilometragemInicio = quilometragemInicio;
            QuilometragemLimite = quilometragemLimite;
            Status = "Ativa";
        }

        public void Encerrar(string usuario)
        {
            Status = "Encerrada";
            MarcarAlterado(usuario);
        }
    }
}
