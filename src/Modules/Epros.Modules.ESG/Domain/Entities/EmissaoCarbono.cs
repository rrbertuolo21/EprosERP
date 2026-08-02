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

        // Rastreabilidade do fator (RN-GHG NF-01/A-01): quando a emissao e calculada a partir do
        // catalogo versionado esg.ghg_fator_emissao, guarda-se codigo/versao/fonte do fator usado.
        public string? FatorCodigo { get; private set; }
        public string? FatorVersao { get; private set; }
        public string? FatorFonte { get; private set; }

        // Regra #0 (esg-carbono): sem fator oficial vigente NAO se emite numero inventado.
        // A emissao entra como "pendente de fator" (FatorEmissao=0, TotalCo2e=0) ate a homologacao.
        public bool FatorPendente { get; private set; }

        protected EmissaoCarbono() { } // EF Core

        /// <summary>
        /// Emissao com fator informado diretamente (registro manual / dado ja homologado).
        /// </summary>
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
            Validar(fonteEmissao, escopo, categoriaGhg, quantidadeConsumo, unidadeMedida, fatorEmissao);

            FonteEmissao = fonteEmissao;
            Escopo = escopo;
            CategoriaGhg = categoriaGhg;
            QuantidadeConsumo = quantidadeConsumo;
            UnidadeMedida = unidadeMedida;
            FatorEmissao = fatorEmissao;
            TotalCo2e = quantidadeConsumo * FatorEmissao;
            DataTransacao = dataTransacao.Date;
            FatorPendente = false;
        }

        private void Validar(string fonteEmissao, int escopo, string categoriaGhg, decimal quantidadeConsumo, string unidadeMedida, decimal fatorEmissao)
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
        }

        /// <summary>
        /// Emissao calculada a partir de um fator do catalogo versionado (esg.ghg_fator_emissao).
        /// emissao = quantidade × fator.Valor; guarda codigo/versao/fonte para reprodutibilidade (RN-GHG).
        /// </summary>
        public static EmissaoCarbono CalculadaComFator(
            string fonteEmissao,
            int escopo,
            string categoriaGhg,
            decimal quantidadeConsumo,
            string unidadeMedida,
            FatorEmissaoGee fator,
            DateTime dataTransacao,
            string tenantId,
            string criadoPor)
        {
            if (fator == null) throw new ArgumentNullException(nameof(fator));

            var emissao = new EmissaoCarbono(
                fonteEmissao, escopo, categoriaGhg, quantidadeConsumo, unidadeMedida,
                fator.Valor, dataTransacao, tenantId, criadoPor);

            emissao.FatorCodigo = fator.Codigo;
            emissao.FatorVersao = fator.Versao;
            emissao.FatorFonte = fator.FonteReferencia;
            emissao.FatorPendente = false;
            return emissao;
        }

        /// <summary>
        /// Regra #0: nao ha fator oficial vigente para a atividade. A emissao e registrada como
        /// PENDENTE DE FATOR — sem numero inventado (FatorEmissao=0, TotalCo2e=0) — apontando o codigo
        /// de fator esperado. Fica visivel para homologacao/ingestao, nunca contamina a consolidacao.
        /// </summary>
        public static EmissaoCarbono PendenteDeFator(
            string fonteEmissao,
            int escopo,
            string categoriaGhg,
            decimal quantidadeConsumo,
            string unidadeMedida,
            string fatorCodigoEsperado,
            DateTime dataTransacao,
            string tenantId,
            string criadoPor)
        {
            var emissao = new EmissaoCarbono(
                fonteEmissao, escopo, categoriaGhg, quantidadeConsumo, unidadeMedida,
                0m, dataTransacao, tenantId, criadoPor);

            emissao.FatorCodigo = fatorCodigoEsperado;
            emissao.FatorVersao = null;
            emissao.FatorFonte = null;
            emissao.FatorPendente = true;
            emissao.TotalCo2e = 0m; // Regra #0: nao emite numero inventado.
            return emissao;
        }
    }
}
