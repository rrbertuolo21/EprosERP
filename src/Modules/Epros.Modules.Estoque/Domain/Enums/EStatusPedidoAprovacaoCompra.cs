namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Situação da instância de aprovação por alçada de compras (CD3 / EF SOURCING §6.2).
    /// Pendente enquanto houver nível não decidido; Aprovado quando todos os níveis aprovam;
    /// Reprovado assim que qualquer nível reprova (bloqueia o pedido — SRC-008). Mapeamento canônico:
    /// Pendente↔EmAnalise, Aprovado↔Ativo, Reprovado/Cancelado↔Cancelado (T3).
    /// </summary>
    public enum EStatusPedidoAprovacaoCompra
    {
        Pendente = 1,
        Aprovado = 2,
        Reprovado = 3,
        Cancelado = 4
    }
}
