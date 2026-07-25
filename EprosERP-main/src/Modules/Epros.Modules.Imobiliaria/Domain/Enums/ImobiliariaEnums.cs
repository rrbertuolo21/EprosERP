namespace Epros.Modules.Imobiliaria.Domain.Enums
{
    /// <summary>
    /// Ciclo de vida do imovel (EF GESTAO_IMOBILIARIA secao 12).
    /// Proposta do agente encaminhada a validacao humana.
    /// </summary>
    public enum EStatusImovel
    {
        EmCadastro = 0,
        Disponivel = 1,
        Locado = 2,
        Inativo = 3
    }

    /// <summary>
    /// Ciclo de vida da locacao (EF GESTAO_IMOBILIARIA secao 12).
    /// </summary>
    public enum EStatusLocacao
    {
        EmElaboracao = 0,
        Vigente = 1,
        Encerrada = 2,
        Cancelada = 3
    }

    /// <summary>
    /// Papel da parte vinculada a locacao.
    /// </summary>
    public enum EPapelParteLocacao
    {
        Locatario = 0,
        Fiador = 1
    }
}
