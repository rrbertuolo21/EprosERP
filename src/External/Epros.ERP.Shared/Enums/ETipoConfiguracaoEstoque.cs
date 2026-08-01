using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoConfiguracaoEstoque
    {
        [Description("Não Bloquear")] //(Permite gerar pedidos e vendas no estoque vai salvando o saldo mesmo que fique negativo)
        NaoBloquear = 0,

        [Description("Bloquear Venda")] //(quando informar produto negativo mostra aviso, mas nesse cenário permite fazer pedido)
        BloquearVenda = 1,

        [Description("Bloquear Venda e Pedidos")] //(Impede venda pedido em caso de estoque negativo)
        BloquearVendaEPedido = 2,

        [Description("Permitir Venda com Aviso")] //(caso não haja estoque mostra um alerta para o usuário mas não bloqueia a venda)
        PermitirVendaComAviso = 3
    }
}
