namespace Epros.Modules.Projetos.Domain.Enums
{
    /// <summary>
    /// Workflow canonico compartilhado pelos submodulos de Projetos (Orcamento, Recursos, Faturamento).
    /// Origem: EFs PRJ-ORC/PRJ-REC/PRJ-FAT secao 12 (Estados).
    /// </summary>
    public enum EProjetoWorkflowStatus
    {
        Rascunho = 0,
        EmAnalise = 1,
        Ativo = 2,
        Suspenso = 3,
        Encerrado = 4,
        Inativo = 5,
        Cancelado = 6
    }

    /// <summary>Tipo do agregado de projeto. Origem: RN-DEF-012 (agregado unico com tipo).</summary>
    public enum ETipoProjeto
    {
        Normal = 0,
        Template = 1
    }

    /// <summary>Status do marco orcamentario. Origem: RN-ORC-006.</summary>
    public enum EMarcoStatus
    {
        Incomplete = 0,
        Complete = 1
    }

    /// <summary>Tipo de cobranca do orcamento. Origem: RN-ORC-009.</summary>
    public enum EBillingType
    {
        Hourly = 0,
        Fixed = 1
    }

    /// <summary>Tipo de apontamento de timesheet. Origem: RN-REC-004.</summary>
    public enum ETimesheetTipo
    {
        ClockInOut = 0,
        Project = 1,
        Manual = 2
    }

    /// <summary>Ciclo de vida da tarefa operacional. Origem: EF PRJ-RST secao 6.1.</summary>
    public enum ETarefaEstado
    {
        Planejada = 0,
        EmExecucao = 1,
        Bloqueada = 2,
        Concluida = 3,
        Adiada = 4,
        Cancelada = 5,
        Arquivada = 6
    }

    /// <summary>Tipo de dependencia entre tarefas. Origem: EF PRJ-RST 4.5.</summary>
    public enum ETipoDependencia
    {
        FimInicio = 0,
        InicioInicio = 1,
        FimFim = 2,
        InicioFim = 3
    }

    /// <summary>Modalidade de faturamento de projeto. Origem: EF PRJ-FAT 11.1.</summary>
    public enum EModalidadeFaturamento
    {
        Marco = 0,
        TM = 1,
        Despesa = 2,
        PrecoFixo = 3
    }

    /// <summary>Tipo de item de faturamento. Origem: EF PRJ-FAT 11.2.</summary>
    public enum ETipoItemFaturamento
    {
        Marco = 0,
        Hora = 1,
        Despesa = 2,
        Parcela = 3,
        Ajuste = 4
    }
}
