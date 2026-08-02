namespace Epros.Modules.Projetos.Domain.Entities.Portfolio
{
    /// <summary>
    /// DP-PRT-score — pesos da média ponderada de priorização de portfólio (parametrizáveis em
    /// prj_portfolio_parametro). Fatores de benefício (NPV, Alinhamento) somam; fatores de custo
    /// (Payback, Risco) penalizam. Origem: DECISOES_IMPLANTACAO_V1 · seção 3 (DP-PRT-score).
    /// </summary>
    public readonly record struct PesosPortfolio(
        decimal PesoNpv = 1m,
        decimal PesoPayback = 1m,
        decimal PesoAlinhamento = 1m,
        decimal PesoRisco = 1m)
    {
        public static PesosPortfolio Padrao => new(1m, 1m, 1m, 1m);
    }
}
