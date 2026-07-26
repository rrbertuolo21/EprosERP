using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    public class EmissaoCarbono : EntidadeSaaSBase
    {
        public string FonteEmissao { get; private set; } = string.Empty;
        public int Escopo { get; private set; } // 1, 2, 3
        public string CategoriaGhg { get; private set; } = string.Empty; // CombustaoEstacionaria, BensAdquiridos, etc
        public decimal QuantidadeConsumo { get; private set; }
        public string UnidadeMedida { get; private set; } = string.Empty; // Litro, KWh, Km, PC
        public decimal FatorEmissao { get; private set; } // kg CO2e por unidade
        public decimal TotalCo2e { get; private set; } // QuantidadeConsumo * FatorEmissao
        public DateTime DataTransacao { get; private set; }

        protected EmissaoCarbono() { } // EF Core

        public EmissaoCarbono(
            string fonteEmissao,
            int escopo,
            string categoriaGhg,
            decimal quantidadeConsumo,
            string unidadeMedida,
            decimal fatorEmissao,
            DateTime dataTransacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<EmissaoCarbono>()
                .Requires()
                .IsNotNullOrEmpty(fonteEmissao, nameof(FonteEmissao), "A fonte de emissão é obrigatória.")
                .IsTrue(escopo == 1 || escopo == 2 || escopo == 3, nameof(Escopo), "O Escopo deve ser 1, 2 ou 3.")
                .IsNotNullOrEmpty(categoriaGhg, nameof(CategoriaGhg), "A categoria GHG Protocol é obrigatória.")
                .IsGreaterThan(quantidadeConsumo, 0, nameof(QuantidadeConsumo), "A quantidade de consumo deve ser maior que zero.")
                .IsNotNullOrEmpty(unidadeMedida, nameof(UnidadeMedida), "A unidade de medida é obrigatória.")
                .IsGreaterThan(fatorEmissao, -0.001m, nameof(FatorEmissao), "O fator de emissão não pode ser negativo.")
            );

            FonteEmissao = fonteEmissao;
            Escopo = escopo;
            CategoriaGhg = categoriaGhg;
            QuantidadeConsumo = quantidadeConsumo;
            UnidadeMedida = unidadeMedida;
            FatorEmissao = fatorEmissao;
            TotalCo2e = quantidadeConsumo * FatorEmissao;
            DataTransacao = dataTransacao.Date;
        }
    }
}
