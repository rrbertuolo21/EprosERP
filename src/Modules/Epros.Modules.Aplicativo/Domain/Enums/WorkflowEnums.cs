namespace Epros.Modules.Aplicativo.Domain.Enums
{
    /// <summary>Situação de uma definição de workflow (wf_definicao).</summary>
    public enum EWfDefinicaoStatus
    {
        Rascunho = 0,
        Ativo = 1,
        Inativo = 2
    }

    /// <summary>Situação funcional de uma instância de workflow (wf_instancia).</summary>
    public enum EWfInstanciaStatus
    {
        Rascunho = 0,
        EmAnalise = 1,
        Ativo = 2,
        Rejeitado = 3,
        Inativo = 4,
        Encerrado = 5
    }

    /// <summary>Situação de uma tarefa humana (wf_tarefa).</summary>
    public enum EWfTarefaStatus
    {
        Aberta = 0,
        EmExecucao = 1,
        Concluida = 2,
        Cancelada = 3
    }

    /// <summary>Evento permitido em uma transição (wf_transicao.evento).</summary>
    public enum EWfEvento
    {
        Submeter = 0,
        Aprovar = 1,
        Rejeitar = 2,
        Inativar = 3,
        Encerrar = 4,
        Reativar = 5
    }

    /// <summary>Papel/permissão exigida por uma transição.</summary>
    public enum EWfPermissao
    {
        Operador = 0,
        Aprovador = 1,
        Gestor = 2
    }

    /// <summary>Status de uma solicitação com aprovação (wf_solicitacao). Preserva pending/approved/rejected do material.</summary>
    public enum EWfSolicitacaoStatus
    {
        Pendente = 0,
        Aprovada = 1,
        Rejeitada = 2
    }

    /// <summary>Situação de um job agendado (wf_job).</summary>
    public enum EWfJobStatus
    {
        Pendente = 0,
        EmExecucao = 1,
        Sucesso = 2,
        Falha = 3,
        Adiado = 4,
        FalhaFinal = 5
    }

    /// <summary>Resultado de uma tentativa de job (wf_job_tentativa).</summary>
    public enum EWfJobTentativaStatus
    {
        Sucesso = 0,
        Falha = 1,
        Retry = 2,
        FalhaFinal = 3
    }
}
