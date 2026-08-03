using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Portfolio
{
    /// <summary>
    /// Item do portfolio (programa, projeto ou iniciativa candidata). Origem: EF PRJ-PRT 14.2 (prj_portfolio_item).
    /// PRJ-PRT-RN-014 (sequencia obrigatoria), RN-016 (justificativa obrigatoria no ranking manual),
    /// RN-019 (remocao logica via Ativo). Criterios financeiros (NPV/payback) sao lacunas controladas.
    /// </summary>
    public class PortfolioItem : EntidadeSaaSBase
    {
        public Guid PortfolioId { get; private set; }
        public int Sequencia { get; private set; }
        public string TipoItem { get; private set; } = string.Empty;
        public Guid? ProjetoId { get; private set; }
        public Guid? ProgramaId { get; private set; }
        public string? Titulo { get; private set; }
        public decimal? ValorEstimado { get; private set; }
        public decimal? EsforcoEstimado { get; private set; }
        public decimal? CapacidadeRequerida { get; private set; }
        public decimal? Npv { get; private set; }
        public decimal? Payback { get; private set; }
        public decimal? AlinhamentoEstrategico { get; private set; }
        public decimal? Risco { get; private set; }
        public decimal? Score { get; private set; }
        public string? JustificativaPrioridade { get; private set; }
        public string? Observacao { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected PortfolioItem() { } // EF Core

        public PortfolioItem(
            Guid portfolioId,
            int sequencia,
            string tipoItem,
            Guid? projetoId,
            Guid? programaId,
            string? titulo,
            decimal? valorEstimado,
            decimal? esforcoEstimado,
            decimal? capacidadeRequerida,
            decimal? npv,
            decimal? payback,
            decimal? alinhamentoEstrategico,
            decimal? risco,
            decimal? score,
            string? justificativaPrioridade,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PortfolioItem>()
                .Requires()
                .AreNotEquals(portfolioId, Guid.Empty, nameof(PortfolioId), "O portfolio e obrigatorio. [Origem: PortfolioItem]")
                .IsGreaterOrEqualsThan(sequencia, 1, nameof(Sequencia), "A sequencia do item deve ser maior ou igual a 1. [Origem: PortfolioItem]")
                .IsNotNullOrEmpty(tipoItem, nameof(TipoItem), "O tipo do item e obrigatorio. [Origem: PortfolioItem]"));

            PortfolioId = portfolioId;
            Sequencia = sequencia;
            TipoItem = tipoItem ?? string.Empty;
            ProjetoId = projetoId;
            ProgramaId = programaId;
            Titulo = titulo;
            ValorEstimado = valorEstimado;
            EsforcoEstimado = esforcoEstimado;
            CapacidadeRequerida = capacidadeRequerida;
            Npv = npv;
            Payback = payback;
            AlinhamentoEstrategico = alinhamentoEstrategico;
            Risco = risco;
            Score = score;
            JustificativaPrioridade = justificativaPrioridade;
            Observacao = observacao;
            Ativo = true;
        }

        /// <summary>RN-019: remocao logica; nao ha exclusao fisica.</summary>
        public void RemoverLogicamente(string usuario)
        {
            Ativo = false;
            MarcarAlterado(usuario);
        }

        /// <summary>
        /// DP-PRT-score — calcula e grava o Score do item por MÉDIA PONDERADA dos fatores presentes.
        /// Benefícios (NPV, AlinhamentoEstratégico) contribuem positivamente; custos (Payback, Risco)
        /// penalizam. Fatores nulos são ignorados (peso zero). Score = Σ(peso·fator·sinal)/Σpeso.
        /// A orientação/escala esperada dos fatores é decisão de produto (parametrizável); os VALORES de
        /// NPV/Payback são valida-contador (DP-PRT-006). Aqui só se aplica a agregação técnica.
        /// </summary>
        public decimal CalcularScore(PesosPortfolio pesos)
        {
            decimal somaPesos = 0m;
            decimal somaPonderada = 0m;

            void Acumular(decimal? fator, decimal peso, int sinal)
            {
                if (fator.HasValue && peso != 0m)
                {
                    somaPonderada += peso * fator.Value * sinal;
                    somaPesos += peso;
                }
            }

            Acumular(Npv, pesos.PesoNpv, +1);
            Acumular(AlinhamentoEstrategico, pesos.PesoAlinhamento, +1);
            Acumular(Payback, pesos.PesoPayback, -1);
            Acumular(Risco, pesos.PesoRisco, -1);

            var score = somaPesos != 0m ? System.Math.Round(somaPonderada / somaPesos, 4) : 0m;
            Score = score;
            return score;
        }
    }
}
