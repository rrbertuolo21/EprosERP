namespace Epros.Modules.Qualidade.Domain.Enums
{
    // ============================================================
    // QLD-QPS — Qualidade de Fornecedor (Parceiro de Suprimento)
    // ============================================================

    /// <summary>Ciclo de homologacao do fornecedor (RN-QPS-004: homologacao != cadastro comercial).</summary>
    public enum EQpsStatusHomologacao
    {
        Pendente = 0,
        Homologado = 1,
        Bloqueado = 2,
        ReHomologacao = 3,
        Reprovado = 4
    }

    public enum EQpsTipoBloqueio
    {
        Manual = 0,
        NcrRecorrente = 1,
        DocumentoVencido = 2,
        ScoreAbaixoLimite = 3
    }

    public enum EQpsTipoDocumento
    {
        Certificado = 0,
        Licenca = 1,
        Iso = 2,
        Contrato = 3,
        FichaTecnica = 4,
        Outro = 5
    }

    /// <summary>Disciplinas do metodo 8D (D1..D8).</summary>
    public enum EQps8dDisciplina
    {
        D1_Equipe = 1,
        D2_Problema = 2,
        D3_Contencao = 3,
        D4_CausaRaiz = 4,
        D5_AcaoCorretiva = 5,
        D6_Implementacao = 6,
        D7_Prevencao = 7,
        D8_Encerramento = 8
    }

    public enum EQps8dStatus
    {
        Aberto = 0,
        EmAndamento = 1,
        Concluido = 2,
        Cancelado = 3
    }

    public enum EQps8dStatusAcao
    {
        Pendente = 0,
        EmExecucao = 1,
        Concluida = 2,
        Cancelada = 3
    }
}
