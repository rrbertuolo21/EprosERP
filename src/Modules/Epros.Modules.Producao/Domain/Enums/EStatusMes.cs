namespace Epros.Modules.Producao.Domain.Enums
{
    /// <summary>
    /// PRD-MES — Estado funcional da ordem de produção (MES-EF §6).
    /// Rascunho → EmAnalise → Ativo → (Finalizado | Inativo → Ativo | Encerrado).
    /// Pendente reservado para ordens aguardando liberação operacional.
    /// </summary>
    public enum EStatusOrdemMes
    {
        Rascunho = 0,
        EmAnalise = 1,
        Ativo = 2,
        Pendente = 3,
        Finalizado = 4,
        Inativo = 5,
        Encerrado = 6
    }

    /// <summary>PRD-MES — Estado funcional do item da ordem (MES-EF §6).</summary>
    public enum EStatusItemMes
    {
        Pendente = 0,
        EmProducao = 1,
        Produzido = 2,
        Entregue = 3,
        Cancelado = 4
    }

    /// <summary>PRD-MES — Estado funcional do serviço/apontamento (MES-EF §6).</summary>
    public enum EStatusServicoMes
    {
        Previsto = 0,
        EmExecucao = 1,
        Realizado = 2,
        Cancelado = 3
    }

    /// <summary>PRD-MES — Estado do consumo de material (MES-EF §6).</summary>
    public enum EStatusConsumoMes
    {
        Previsto = 0,
        Consumido = 1,
        Cancelado = 2
    }

    /// <summary>PRD-MES — Tipo funcional do movimento de produção (MES-EF §17 prd_mes_movimento_producao).</summary>
    public enum ETipoMovimentoMes
    {
        EntradaProdutoAcabado = 0,
        ConsumoMaterial = 1
    }

    /// <summary>PRD-MES — Estado do movimento de produção (MES-EF §6).</summary>
    public enum EStatusMovimentoMes
    {
        Pendente = 0,
        Confirmado = 1,
        Cancelado = 2
    }
}
