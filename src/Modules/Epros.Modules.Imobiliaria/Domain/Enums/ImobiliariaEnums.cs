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

    /// <summary>
    /// Tipo de garantia da locacao (ID6/NF-04). Tratamento financeiro da caucao
    /// permanece DESLIGADO ate ratificacao do contador (NF-04).
    /// </summary>
    public enum ETipoGarantia
    {
        Fiador = 0,
        Caucao = 1,
        SeguroFianca = 2,
        GarantiaBancaria = 3
    }

    /// <summary>Estado da garantia (ID6): ativa, substituida ou liberada.</summary>
    public enum EStatusGarantia
    {
        Ativa = 0,
        Substituida = 1,
        Liberada = 2
    }

    /// <summary>
    /// Tipo de lancamento de cobranca do aluguel (ID8/NF-01). Compoe a chave de
    /// idempotencia (locacao+competencia+tipo). O titulo/baixa vivem no CONTAS_RECEBER.
    /// </summary>
    public enum ETipoCobrancaAluguel
    {
        Aluguel = 0,
        Encargo = 1,
        Multa = 2,
        Reajuste = 3
    }

    /// <summary>
    /// Situacao da cobranca REFLETIDA do FINANCEIRO (nao governada aqui) — ID8/NF-01.
    /// </summary>
    public enum EStatusCobrancaAluguel
    {
        EmAberto = 0,
        Parcial = 1,
        Pago = 2,
        Estornado = 3
    }

    /// <summary>Tipo de proposta imobiliaria (ID2). Dono = IMOBILIARIA.</summary>
    public enum ETipoProposta
    {
        Locacao = 0,
        Aquisicao = 1
    }

    /// <summary>Ciclo de vida da proposta (ID2/T3).</summary>
    public enum EStatusProposta
    {
        Rascunho = 0,
        Aprovada = 1,
        Rejeitada = 2,
        Expirada = 3,
        Convertida = 4
    }

    /// <summary>Papel da parte na proposta (ID2).</summary>
    public enum EPapelParteProposta
    {
        Proponente = 0,
        Interessado = 1
    }
}
