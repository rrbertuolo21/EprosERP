using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class OrdemServicoDms : EntidadeSaaSBase
    {
        public string NumeroOs { get; private set; } = string.Empty;
        public string VeiculoChassi { get; private set; } = string.Empty;
        public string DescricaoInconveniente { get; private set; } = string.Empty;
        public decimal ValorPecas { get; private set; }
        public decimal ValorMaoDeObra { get; private set; }
        public decimal ValorTotal { get; private set; }
        public string Status { get; private set; } = "Aberta"; // Aberta, AguardandoPecas, Pronta, Fechada
        public bool ReclamacaoGarantia { get; private set; }
        public string StatusGarantia { get; private set; } = "Nenhuma"; // Nenhuma, Solicitada, Aprovada, Rejeitada

        protected OrdemServicoDms() { } // EF Core

        public OrdemServicoDms(
            string numeroOs,
            string veiculoChassi,
            string descricaoInconveniente,
            decimal valorPecas,
            decimal valorMaoDeObra,
            bool reclamacaoGarantia,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<OrdemServicoDms>()
                .Requires()
                .IsNotNullOrEmpty(numeroOs, nameof(NumeroOs), "O número da ordem de serviço é obrigatório.")
                .IsNotNullOrEmpty(veiculoChassi, nameof(VeiculoChassi), "O número de chassi do veículo é obrigatório.")
                .AreEquals(veiculoChassi?.Length ?? 0, 17, nameof(VeiculoChassi), "O número de chassi do veículo deve possuir exatamente 17 caracteres.")
                .IsNotNullOrEmpty(descricaoInconveniente, nameof(DescricaoInconveniente), "A descrição do inconveniente reclamado é obrigatória.")
                .IsGreaterThan(valorPecas, -0.001m, nameof(ValorPecas), "O valor de peças não pode ser negativo.")
                .IsGreaterThan(valorMaoDeObra, -0.001m, nameof(ValorMaoDeObra), "O valor de mão de obra não pode ser negativo.")
            );

            NumeroOs = numeroOs;
            VeiculoChassi = veiculoChassi?.ToUpperInvariant() ?? string.Empty;
            DescricaoInconveniente = descricaoInconveniente;
            ValorPecas = valorPecas;
            ValorMaoDeObra = valorMaoDeObra;
            ValorTotal = valorPecas + valorMaoDeObra;
            ReclamacaoGarantia = reclamacaoGarantia;
            StatusGarantia = reclamacaoGarantia ? "Solicitada" : "Nenhuma";
            Status = "Aberta";
        }

        public void AtualizarValores(decimal valorPecas, decimal valorMaoDeObra, string usuario)
        {
            if (Status == "Fechada")
            {
                AddNotification(nameof(Status), "Não é possível alterar valores em uma ordem de serviço fechada.");
                return;
            }

            ValorPecas = valorPecas;
            ValorMaoDeObra = valorMaoDeObra;
            ValorTotal = valorPecas + valorMaoDeObra;
            MarcarAlterado(usuario);
        }

        public void Fechar(string usuario)
        {
            if (Status == "Fechada")
            {
                AddNotification(nameof(Status), "A ordem de serviço já se encontra fechada.");
                return;
            }

            Status = "Fechada";
            MarcarAlterado(usuario);
        }

        public void AtualizarStatusGarantia(string statusGarantia, string usuario)
        {
            if (!ReclamacaoGarantia)
            {
                AddNotification(nameof(ReclamacaoGarantia), "Essa ordem de serviço não possui reclamação de garantia registrada.");
                return;
            }

            StatusGarantia = statusGarantia;
            MarcarAlterado(usuario);
        }
    }
}
