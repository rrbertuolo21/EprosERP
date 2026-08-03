using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Forma de aplicação do ajuste pós-inventário.
    /// [DECIDIDO D3] O ajuste é SEMPRE por diferença = (contado − saldo do sistema), preservando a
    /// trilha/kardex; a substituição absoluta do saldo foi DESCARTADA. Enum mantido mono-valor para
    /// deixar a decisão explícita no modelo (INV-015a).
    /// </summary>
    public enum ETipoAplicacaoAjusteInventario
    {
        [Description("Ajuste por diferença")]
        AjustePorDiferenca = 0
    }
}
