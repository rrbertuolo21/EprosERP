namespace Epros.Modules.Producao.Domain.Enums
{
    /// <summary>PRD-GOS — Situação operacional simplificada da ficha de produção (GOS-EF §11.1/§12.1).</summary>
    public enum ESituacaoFichaProducao
    {
        AguardandoPagamento = 1,
        EmProducao = 2,
        Concluido = 3
    }

    /// <summary>PRD-GOS — Configuração de logomarca da ficha (GOS-EF §11.1).</summary>
    public enum ELogomarcaFichaProducao
    {
        SemLogo = 1,
        Bordada = 2,
        Carimbada = 3
    }
}
