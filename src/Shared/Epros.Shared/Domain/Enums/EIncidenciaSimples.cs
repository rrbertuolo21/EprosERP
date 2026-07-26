using System.ComponentModel;

namespace Epros.Shared.Domain.Enums
{
    public enum EIncidenciaSimples
    {
        [Description("1- Revenda de Produtos Industrializados")]
        RevendaProdutosIndustrializados = 1,

        [Description("2- Revenda de Mercadorias")]
        RevendaMercadorias = 2,

        [Description("3- Locação Bens Moveis")]
        LocacaoBensMoveis = 3,

        [Description("4- Serviço de Transporte de Cargas")]
        ServicoTransporteCargas = 4,

        [Description("5- Serviço de Comunicação")]
        ServicoComunicacao = 5,

        [Description("6- Prestação de Serviços")]
        PrestacaoServicos = 6,

        [Description("7- Exterior")]
        Exterior = 7,

        [Description("9- Não se Aplica")]
        NaoAplica = 9
    }
}
