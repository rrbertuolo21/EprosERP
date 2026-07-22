namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    // Enums de Status/Situação portados do legado (D5). As colunas correspondentes permanecem
    // como texto no banco (HasConversion<string>() no ContextGestaoClientes), por isso os nomes
    // dos membros são idênticos aos valores string já persistidos — a migração de dados é neutra
    // (a coluna continua varchar; muda apenas a tipagem no domínio).

    /// <summary>Status de uma <c>Fatura</c> SaaS.</summary>
    public enum FaturaStatus
    {
        Pendente,
        Paga,
        Cancelada,
        Atrasada
    }

    /// <summary>Status de um <c>PagamentoFatura</c>.</summary>
    public enum PagamentoFaturaStatus
    {
        Pending,
        Paid,
        Expired,
        Failed
    }

    /// <summary>Status de um <c>PedidoSaaS</c>.</summary>
    public enum PedidoSaaSStatus
    {
        Pending,
        Succeeded,
        Failed,
        Refunded
    }

    /// <summary>Status de um <c>PagamentoTransferencia</c> (comprovante).</summary>
    public enum PagamentoTransferenciaStatus
    {
        Pending,
        Approved,
        Rejected
    }

    /// <summary>Status de uma <c>SessaoPagamento</c> de gateway.</summary>
    public enum SessaoPagamentoStatus
    {
        Pending,
        Completed
    }
}
