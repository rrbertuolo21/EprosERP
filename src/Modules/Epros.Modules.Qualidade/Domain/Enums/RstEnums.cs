namespace Epros.Modules.Qualidade.Domain.Enums
{
    // ============================================================
    // QLD-RST — Rastreabilidade e Recall
    // ============================================================

    /// <summary>Etapas da campanha de recall (fluxo do dossie D6).</summary>
    public enum ERstEtapaCampanha
    {
        Investigacao = 0,
        Escopo = 1,
        Contencao = 2,
        Comunicacao = 3,
        Recolhimento = 4,
        Disposicao = 5,
        Encerramento = 6,
        Cancelada = 7
    }

    public enum ERstGravidade
    {
        Baixa = 0,
        Media = 1,
        Alta = 2,
        Critica = 3
    }

    /// <summary>Tipo de no na arvore de genealogia (MP -> WIP -> PA).</summary>
    public enum ERstTipoNoGenealogia
    {
        MateriaPrima = 0,
        WIP = 1,
        ProdutoAcabado = 2
    }

    public enum ERstCanalComunicacao
    {
        Cliente = 0,
        Autoridade = 1,
        Interno = 2,
        Publico = 3
    }

    public enum ERstStatusComunicacao
    {
        Rascunho = 0,
        Aprovada = 1,
        Enviada = 2
    }

    public enum ERstStatusRecolhimento
    {
        Pendente = 0,
        EmAndamento = 1,
        Concluido = 2
    }

    public enum ERstTipoDisposicao
    {
        Retrabalho = 0,
        Descarte = 1,
        Devolucao = 2,
        Concessao = 3,
        Reparo = 4
    }
}
