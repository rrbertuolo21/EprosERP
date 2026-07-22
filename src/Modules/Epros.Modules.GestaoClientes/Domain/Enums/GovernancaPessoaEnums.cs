namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    // Enums de governança do submódulo Pessoa e Organização (CAD-PEM), portados fielmente
    // da EF 1_CADASTROS_BASE_PESSOA_E_ORGANIZACAO seção 7. Prefixo E conforme convenção.

    /// <summary>REG-PEM: tipo de relação entre parceiros/pessoas.</summary>
    public enum ETipoRelacaoParceiro
    {
        MatrizFilial = 1,
        GrupoEconomico = 2,
        Representante = 3,
        Controladora = 4,
        ContatoDaConta = 5
    }

    /// <summary>Situação de um candidato a duplicata.</summary>
    public enum EStatusDuplicata
    {
        Pendente = 1,
        Confirmada = 2,
        Descartada = 3,
        Mesclada = 4
    }

    /// <summary>Estratégia de matching da regra de deduplicação.</summary>
    public enum EEstrategiaMatch
    {
        Exata = 1,
        Fuzzy = 2,
        Fonetica = 3
    }

    /// <summary>Finalidade do tratamento de dados pessoais (LGPD).</summary>
    public enum EFinalidadeDados
    {
        Marketing = 1,
        Cobranca = 2,
        Contrato = 3,
        ObrigacaoLegal = 4,
        Suporte = 5,
        Fiscal = 6
    }

    /// <summary>Base legal de privacidade (LGPD).</summary>
    public enum EBaseLegalPrivacidade
    {
        Consentimento = 1,
        Contrato = 2,
        ObrigacaoLegal = 3,
        LegitimoInteresse = 4,
        ExercicioRegularDireitos = 5
    }

    /// <summary>Tipo de solicitação do titular (DSAR).</summary>
    public enum ETipoSolicitacaoTitular
    {
        Acesso = 1,
        Correcao = 2,
        Exclusao = 3,
        Portabilidade = 4,
        Anonimizacao = 5
    }

    /// <summary>Situação da solicitação do titular.</summary>
    public enum EStatusSolicitacaoTitular
    {
        Aberta = 1,
        EmAndamento = 2,
        Concluida = 3,
        Negada = 4
    }

    /// <summary>Tipo de identificador fiscal por país/jurisdição.</summary>
    public enum ETipoIdFiscal
    {
        CPF = 1,
        CNPJ = 2,
        VAT = 3,
        EIN = 4,
        TaxID = 5,
        ISO6523 = 6,
        Outro = 7
    }

    /// <summary>Tipo de período de prazo de pagamento de fornecedor.</summary>
    public enum ETipoPeriodoPagamento
    {
        Dias = 1,
        Meses = 2
    }

    /// <summary>Motivo de bloqueio de parceiro (KYC/compliance).</summary>
    public enum EMotivoBloqueioParceiro
    {
        Litigio = 1,
        CertidaoVencida = 2,
        Sancao = 3,
        Compliance = 4,
        Credito = 5,
        Manual = 6
    }

    /// <summary>Situação de um lote/linha de importação de pessoas.</summary>
    public enum EStatusImportacao
    {
        Pendente = 1,
        Processando = 2,
        Concluida = 3,
        ConcluidaComErros = 4,
        Rejeitada = 5
    }

    /// <summary>Situação de uma linha de importação de pessoas.</summary>
    public enum EStatusLinhaImportacao
    {
        Pendente = 1,
        Aceita = 2,
        Rejeitada = 3
    }
}
