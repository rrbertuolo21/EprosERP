using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class OrdemServicoManutencao : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public Guid VeiculoId { get; private set; }
        public string ChassiVin { get; private set; } = string.Empty;
        public string Placa { get; private set; } = string.Empty;
        public decimal QuilometragemEntrada { get; private set; }
        public string TipoAtendimento { get; private set; } = "Cliente";
        public Guid ConsultorId { get; private set; }
        public Guid UnidadeId { get; private set; }
        public string StatusOficina { get; private set; } = "Recebida"; // Recebida, EmExecucao, Encerrada
        public DateTime DataAbertura { get; private set; }
        public DateTime? PrevisaoEntrega { get; private set; }

        protected OrdemServicoManutencao() { } // EF Core

        public OrdemServicoManutencao(
            Guid pessoaId,
            Guid? produtoId,
            Guid veiculoId,
            string chassiVin,
            string placa,
            decimal quilometragemEntrada,
            Guid consultorId,
            Guid unidadeId,
            DateTime dataAbertura,
            DateTime? previsaoEntrega,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<OrdemServicoManutencao>()
                .Requires()
                .AreNotEquals(pessoaId, Guid.Empty, nameof(PessoaId), "A pessoa/cliente é obrigatória na ordem de serviço.")
                .AreNotEquals(veiculoId, Guid.Empty, nameof(VeiculoId), "O veículo é obrigatório na ordem de serviço.")
                .IsNotNullOrEmpty(chassiVin, nameof(ChassiVin), "O número de chassi (VIN) do veículo é obrigatório.")
                .AreEquals(chassiVin?.Length ?? 0, 17, nameof(ChassiVin), "O número de chassi (VIN) do veículo deve possuir exatamente 17 caracteres.")
                .IsNotNullOrEmpty(placa, nameof(Placa), "A placa do veículo é obrigatória.")
                .IsGreaterThan(quilometragemEntrada, -0.001m, nameof(QuilometragemEntrada), "A quilometragem de entrada não pode ser negativa.")
                .AreNotEquals(consultorId, Guid.Empty, nameof(ConsultorId), "O consultor é obrigatório na ordem de serviço.")
                .AreNotEquals(unidadeId, Guid.Empty, nameof(UnidadeId), "A unidade é obrigatória na ordem de serviço.")
            );

            PessoaId = pessoaId;
            ProdutoId = produtoId;
            VeiculoId = veiculoId;
            ChassiVin = chassiVin?.ToUpperInvariant() ?? string.Empty;
            Placa = placa;
            QuilometragemEntrada = quilometragemEntrada;
            TipoAtendimento = "Cliente";
            ConsultorId = consultorId;
            UnidadeId = unidadeId;
            StatusOficina = "Recebida";
            DataAbertura = dataAbertura;
            PrevisaoEntrega = previsaoEntrega;
        }

        public void AtualizarStatus(string status, string usuario)
        {
            StatusOficina = status;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            if (StatusOficina == "Encerrada")
            {
                AddNotification(nameof(StatusOficina), "OS já encerrada.");
                return;
            }

            StatusOficina = "Encerrada";
            MarcarAlterado(usuario);
        }
    }
}
