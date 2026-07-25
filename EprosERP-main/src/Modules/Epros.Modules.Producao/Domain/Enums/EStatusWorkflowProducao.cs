namespace Epros.Modules.Producao.Domain.Enums
{
    /// <summary>
    /// Ciclo de vida comum aos registros com workflow de aprovação da Produção
    /// (BOM, Custos, Planejamento, Estimativa). Fiel às EFs: Rascunho → EmAnalise → Ativo → Inativo/Encerrado.
    /// </summary>
    public enum EStatusWorkflowProducao
    {
        Rascunho = 0,
        EmAnalise = 1,
        Ativo = 2,
        Inativo = 3,
        Encerrado = 4
    }
}
