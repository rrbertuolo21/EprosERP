using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Status da unidade serializada (EF Rastreabilidade §7.4 `rlt_numero_serial.status`).</summary>
    public enum EStatusNumeroSerial
    {
        [Description("Disponível")] Disponivel = 0,
        [Description("Reservado")] Reservado = 1,
        [Description("Consumido")] Consumido = 2,
        [Description("Bloqueado")] Bloqueado = 3,
        [Description("Cancelado")] Cancelado = 4
    }
}
