using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class SolicitacaoGarantia : EntidadeSaaSBase
    {
        public Guid VeiculoGarantiaId { get; private set; }
        public string Protocolo { get; private set; } = string.Empty;
        public string Status { get; private set; } = "Aberta";
        public DateTime DataOcorrencia { get; private set; }
        public decimal Quilometragem { get; private set; }
        public string Sintoma { get; private set; } = string.Empty;
        public string RelatoCliente { get; private set; } = string.Empty;
        public Guid? OrdemServicoId { get; private set; }

        protected SolicitacaoGarantia() { } // EF Core

        public SolicitacaoGarantia(
            Guid veiculoGarantiaId,
            string protocolo,
            DateTime dataOcorrencia,
            decimal quilometragem,
            string sintoma,
            string relatoCliente,
            Guid? ordemServicoId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<SolicitacaoGarantia>()
                .Requires()
                .AreNotEquals(veiculoGarantiaId, Guid.Empty, nameof(VeiculoGarantiaId), "A garantia do veículo é obrigatória.")
                .IsNotNullOrEmpty(protocolo, nameof(Protocolo), "O protocolo é obrigatório.")
                .IsGreaterThan(quilometragem, -0.001m, nameof(Quilometragem), "A quilometragem não pode ser negativa.")
                .IsNotNullOrEmpty(sintoma, nameof(Sintoma), "O sintoma é obrigatório.")
                .IsNotNullOrEmpty(relatoCliente, nameof(RelatoCliente), "O relato do cliente é obrigatório.")
            );

            VeiculoGarantiaId = veiculoGarantiaId;
            Protocolo = protocolo;
            DataOcorrencia = dataOcorrencia;
            Quilometragem = quilometragem;
            Sintoma = sintoma;
            RelatoCliente = relatoCliente;
            OrdemServicoId = ordemServicoId;
            Status = "Aberta";
        }

        public void Aprovar(string usuario)
        {
            if (Status != "Aberta")
            {
                AddNotification(nameof(Status), "Apenas solicitações abertas podem ser julgadas.");
                return;
            }

            Status = "Aprovada";
            MarcarAlterado(usuario);
        }

        public void Rejeitar(string usuario)
        {
            if (Status != "Aberta")
            {
                AddNotification(nameof(Status), "Apenas solicitações abertas podem ser julgadas.");
                return;
            }

            Status = "Rejeitada";
            MarcarAlterado(usuario);
        }
    }
}
