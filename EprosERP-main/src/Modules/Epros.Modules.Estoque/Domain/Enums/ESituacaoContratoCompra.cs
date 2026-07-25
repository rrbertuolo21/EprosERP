namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Situação do contrato de compra — EF Gestão de Contratos de Compra §11 (domínio proposto por autoria,
    /// pendente de validação humana §9/§21).
    /// </summary>
    public enum ESituacaoContratoCompra
    {
        Rascunho = 0,
        EmAprovacao = 1,
        Aprovado = 2,
        Suspenso = 3,
        Encerrado = 4,
        Cancelado = 5,
        Expirado = 6
    }
}
