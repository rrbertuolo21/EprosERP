namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    // Enums de Status/Situação portados do legado (D5). As colunas correspondentes permanecem
    // como texto no banco (HasConversion<string>() no ContextGestaoClientes), por isso os nomes
    // dos membros são idênticos aos valores string já persistidos — a migração de dados é neutra
    // (a coluna continua varchar; muda apenas a tipagem no domínio).

    /// <summary>Duração/vigência de um <c>Plano</c> do catálogo (EF 1.01: Duration vitalícia/mensal/anual).</summary>
    public enum PlanoDuration
    {
        Vitalicia,
        Mensal,
        Anual
    }

    /// <summary>Status de uma <c>Fatura</c> SaaS.</summary>
    public enum FaturaStatus
    {
        Pendente,
        Paga,
        Cancelada,
        Atrasada,
        // 1.08E — Fatura estornada (refund do pagamento do ciclo). Persistido como string
        // (HasConversion<string>), portanto o novo membro NÃO exige alteração de schema na coluna Status.
        Estornada
    }

    /// <summary>Status de um <c>PagamentoFatura</c>.</summary>
    public enum PagamentoFaturaStatus
    {
        Pending,
        Paid,
        Expired,
        Failed,
        // 1.08E — Pagamento estornado (refund no gateway). Persistido como string
        // (HasConversion<string>), portanto o novo membro NÃO exige alteração de schema na coluna Status.
        Refunded
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

    /// <summary>
    /// 1.08I — Status de um lançamento de <c>ReconhecimentoReceita</c> (competência).
    /// <c>Pendente</c> = receita diferida ainda não apropriada (passivo de contrato / receita a apropriar,
    /// CPC 47 item 106). <c>Reconhecido</c> = apropriada como receita do período (competência, CPC 47 item 35(a)).
    /// Persistido como string (HasConversion&lt;string&gt;).
    /// </summary>
    public enum ReconhecimentoReceitaStatus
    {
        Pendente,
        Reconhecido
    }

    /// <summary>
    /// 1.08I — Base de cálculo da comissão de parceiro. ⚠️ PARÂMETRO comercial (valida contador) —
    /// skill Negocio-acumulado/financeiro RN07: não há norma única, é decisão de contrato.
    /// Persistido como string.
    /// </summary>
    public enum BaseComissao
    {
        /// <summary>Valor cheio cobrado do cliente (default seguro do mecanismo).</summary>
        Bruto,
        /// <summary>Valor após tarifas/descontos do gateway (bruto − tarifa).</summary>
        Liquido
    }

    /// <summary>
    /// 1.08I — Momento de reconhecimento da comissão. ⚠️ PARÂMETRO (valida contador) —
    /// skill Negocio-acumulado/financeiro RN08: duas práticas legítimas (competência × caixa).
    /// Persistido como string.
    /// </summary>
    public enum MomentoComissao
    {
        /// <summary>Casada com a receita reconhecida (RN08-a — capitalizar/amortizar, CPC 47 itens 91–94).</summary>
        Competencia,
        /// <summary>No recebimento/liquidação (RN08-b — default seguro, mais simples operacionalmente).</summary>
        Caixa
    }

    /// <summary>
    /// 1.08J — Status de uma <c>NfseMensalidade</c> (necessidade de NFS-e da mensalidade SaaS por competência).
    /// <c>Pendente</c> = necessidade registrada, ainda NÃO emitida (default do MECANISMO — não há provedor
    /// municipal / certificado / alíquota configurados; ver overlay `negocio-siser`, hoje VAZIO).
    /// <c>Emitida</c> = NFS-e efetivamente autorizada pelo provedor municipal (número fiscal REAL registrado).
    /// <c>Dispensada</c> = competência dispensada de emissão por decisão do operador (ex.: fatura estornada/cancelada).
    /// <c>Erro</c> = provedor retornou falha na tentativa de emissão (mantém rastro do motivo).
    /// ⛔ O mecanismo NUNCA passa de <c>Pendente</c> sozinho: a emissão REAL depende de alíquota/subitem/município/
    /// certificado/provedor (LC 116/2003 item 1.05/1.03; overlay + contador). Persistido como string.
    /// </summary>
    public enum NfseMensalidadeStatus
    {
        Pendente,
        Emitida,
        Dispensada,
        Erro
    }
}
