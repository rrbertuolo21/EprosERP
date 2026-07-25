namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Situação da requisição interna de armazém — EF Gestão de Armazém WMS §9.2 (WMS-030:
    /// domínio controlado). Reaproveita o conceito de requisição interna já existente no módulo.
    /// </summary>
    public enum EStatusWmsRequisicao
    {
        Aberta = 0,
        Separada = 1,
        Entregue = 2,
        Cancelada = 3
    }
}
