namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>D5 (WMS) — ciclo de vida da tarefa de separação/conferência.</summary>
    public enum EStatusTarefaSeparacao
    {
        /// <summary>Criada; saldo da posição de origem RESERVADO, aguardando o separador.</summary>
        Pendente = 0,
        /// <summary>Separada fisicamente; aguardando conferência.</summary>
        Separada = 1,
        /// <summary>Conferida; a posição de origem foi baixada (movimento efetivado).</summary>
        Conferida = 2,
        /// <summary>Cancelada; a reserva da posição de origem foi liberada.</summary>
        Cancelada = 3
    }
}
