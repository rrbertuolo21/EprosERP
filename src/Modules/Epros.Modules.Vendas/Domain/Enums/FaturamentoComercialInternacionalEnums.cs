using System.ComponentModel;

namespace Epros.Modules.Vendas.Domain.Enums
{
    // ============================================================================
    // Enums do submódulo Faturamento Comercial Internacional (VEN-FCI).
    // Fonte: EF_7_VENDAS_FATURAMENTO_COMERCIAL_INTERNACIONAL_V1 (§5, §7, §10).
    // FCI-001: documento COMERCIAL internacional — NÃO é documento fiscal brasileiro (NF-e).
    // INV-08 (pendente Siser): tipos 8/9/10 e ano financeiro default 1 são "observados no material".
    // ============================================================================

    /// <summary>Tipo do documento comercial. FCI-003/004/005 (8=cotação, 9=fatura, 10=nota de crédito).</summary>
    public enum EFciTipoDocumento
    {
        [Description("Cotação prevista")]
        Cotacao = 8,
        [Description("Fatura comercial")]
        Fatura = 9,
        [Description("Nota de crédito comercial")]
        NotaCredito = 10
    }

    /// <summary>Estado funcional do documento comercial. EF §5.1/§5.2.</summary>
    public enum EFciStatus
    {
        [Description("Rascunho")]
        Rascunho = 0,
        [Description("Aprovada")]
        Aprovada = 1,
        [Description("Paga")]
        Paga = 2,
        [Description("Excluída")]
        Excluida = 3
    }

    /// <summary>Tipo de desconto do documento. FCI-023 (padrão observado: percentual).</summary>
    public enum EFciTipoDesconto
    {
        [Description("Percentual")]
        Percentual = 0,
        [Description("Valor")]
        Valor = 1
    }

    /// <summary>Classificação funcional do lançamento de razão. EF §10.7.</summary>
    public enum EFciRazaoCategoria
    {
        [Description("Item")]
        Item = 0,
        [Description("Contas a receber")]
        ContasReceber = 1,
        [Description("Imposto")]
        Imposto = 2,
        [Description("Desconto")]
        Desconto = 3
    }

    /// <summary>Tipo de saída de documento gerado. EF §10.8.</summary>
    public enum EFciTipoSaidaPdf
    {
        [Description("Impressão")]
        Impressao = 0,
        [Description("PDF")]
        Pdf = 1
    }

    /// <summary>Entidade afetada no histórico. EF §10.9.</summary>
    public enum EFciEntidadeTipo
    {
        [Description("Documento")]
        Documento = 0,
        [Description("Item")]
        Item = 1,
        [Description("Imposto")]
        Imposto = 2,
        [Description("Preferência")]
        Preferencia = 3,
        [Description("Configuração")]
        Configuracao = 4,
        [Description("PDF")]
        Pdf = 5
    }

    /// <summary>Evento de histórico. EF §10.9 / FCI-064.</summary>
    public enum EFciEvento
    {
        [Description("Criação")]
        Criacao = 0,
        [Description("Edição")]
        Edicao = 1,
        [Description("Aprovação")]
        Aprovacao = 2,
        [Description("Exclusão")]
        Exclusao = 3,
        [Description("Impressão")]
        Impressao = 4,
        [Description("PDF")]
        Pdf = 5
    }
}
