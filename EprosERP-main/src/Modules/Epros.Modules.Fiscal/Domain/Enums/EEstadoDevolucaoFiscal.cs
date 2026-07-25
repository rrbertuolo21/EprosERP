namespace Epros.Modules.Fiscal.Domain.Enums
{
    /// <summary>
    /// Estados operacionais da devolução fiscal (EF_DEVOLUCAO_FISCAL, seção 13).
    /// Valores fiéis ao domínio comprovado no material legado: 0=NOVO, 1=APROVADO, 2=REJEITADO, 3=CANCELADO.
    /// </summary>
    public enum EEstadoDevolucaoFiscal
    {
        Novo = 0,
        Aprovado = 1,
        Rejeitado = 2,
        Cancelado = 3
    }
}
