using System.ComponentModel;
namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Tipo de referência do documento do fornecedor (EF Portal do Fornecedor §15.8 `referencia_tipo`).</summary>
    public enum EReferenciaDocumentoFornecedor
    {
        [Description("Cotação")] Cotacao = 0,
        [Description("Pedido")] Pedido = 1,
        [Description("Pré-aviso")] PreAviso = 2,
        [Description("Outro")] Outro = 3
    }
}
