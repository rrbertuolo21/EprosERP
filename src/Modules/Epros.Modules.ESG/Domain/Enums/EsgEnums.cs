namespace Epros.Modules.ESG.Domain.Enums
{
    /// <summary>
    /// Ciclo de vida comum aos agregados ESG (secao 12 das EFs GHG/EHS/ECO/REL).
    /// Estados: Rascunho -> EmAnalise -> Ativo -> (Suspenso|Encerrado|Inativo).
    /// </summary>
    public enum EStatusWorkflowEsg
    {
        Rascunho = 0,
        EmAnalise = 1,
        Ativo = 2,
        Suspenso = 3,
        Encerrado = 4,
        Inativo = 5
    }

    /// <summary>Proveniencia do dado de atividade (GHG 11.2 Atividade.OrigemDado).</summary>
    public enum EOrigemDadoGhg
    {
        Manual = 0,
        Lote = 1,
        Integracao = 2
    }
}
