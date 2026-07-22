using System.ComponentModel;

namespace Epros.Shared.Domain.Enums
{
    public enum ETipoAtendimento
    {
        [Description("Não se aplica")]
        pcNao = 0,

        [Description("Operação presencial")]
        pcPresencial = 1,

        [Description("Operação não presencial, pela Internet")]
        pcInternet = 2,

        [Description("Operação não presencial, Teleatendimento")]
        pcTeleatendimento = 3,

        [Description("NFC-e em operação com entrega a domicílio")]
        pcEntregaDomicilio = 4,

        [Description("Operação presencial, fora do estabelecimento")]
        pcPresencialForaEstabelecimento = 5,

        [Description("Operação não presencial, outros")]
        pcOutros = 9
    }
}
