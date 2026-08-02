namespace Epros.Modules.Producao.Domain.Enums
{
    /// <summary>PRD-MRP — Tipo de sugestão gerada pelo MRP (DP-MRP-016/017).</summary>
    public enum ETipoSugestaoMrp
    {
        Compra = 0,   // item sem estrutura ativa (comprado) → contrato com COMPRAS
        Producao = 1  // item com estrutura ativa (fabricado) → contrato com PLANEJAMENTO
    }

    /// <summary>
    /// PRD-MRP — Estados oficiais da sugestão (DP-MRP-018):
    /// Calculada → PendenteAprovacao → Aprovada → Convertida → Cancelada.
    /// Conversão preserva a identidade e não duplica documento já convertido (aceite PRD-INT-CA-001/002).
    /// </summary>
    public enum EEstadoSugestaoMrp
    {
        Calculada = 0,
        PendenteAprovacao = 1,
        Aprovada = 2,
        Convertida = 3,
        Cancelada = 4
    }
}
