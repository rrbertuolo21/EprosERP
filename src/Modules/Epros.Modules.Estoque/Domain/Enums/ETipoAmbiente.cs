using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Ambiente de emissão (produção/homologação). Porte fiel do legado Epros.ERP.Shared.Enums.ETipoAmbiente.
    /// </summary>
    public enum ETipoAmbiente
    {
        [Description("Produção")]
        Producao = 1,

        [Description("Homologação")]
        Homologacao = 2
    }
}
