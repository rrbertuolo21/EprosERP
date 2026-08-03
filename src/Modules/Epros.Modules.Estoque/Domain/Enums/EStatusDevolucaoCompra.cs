namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Situação da devolução de compra (CD4 / EF DEVOLUCAO_DE_COMPRA §5, mapeada ao status canônico T3).
    /// Rascunho: em edição, SEM efeito de estoque/fiscal/financeiro (DEV-003). Confirmada: efetivada,
    /// gera saída de estoque por evento (motor único D1, DEV-004) + efeito fiscal + financeiro
    /// idempotente (DEV-006). Cancelada: compensa os efeitos, sem apagar o original (padrão TEC-C6).
    /// Mapeamento canônico: Rascunho↔Rascunho, Confirmada↔Ativo, Cancelada↔Cancelado.
    /// </summary>
    public enum EStatusDevolucaoCompra
    {
        Rascunho = 0,
        Confirmada = 1,
        Cancelada = 2
    }
}
