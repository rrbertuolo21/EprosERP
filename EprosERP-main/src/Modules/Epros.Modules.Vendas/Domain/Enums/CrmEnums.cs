using System.ComponentModel;

namespace Epros.Modules.Vendas.Domain.Enums
{
    // ============================================================================
    // Enums do submódulo CRM (VEN-CRM).
    // Fonte funcional: EF_7_VENDAS_CRM_V1 (seções 5, 7, 10).
    // Enums locais do módulo (mesmo padrão de VendasEnums.cs). Tipados — nunca string solta.
    // ============================================================================

    /// <summary>Tipo da etapa dentro do pipeline. EF §10.2 (tipo_etapa Lead/Oportunidade).</summary>
    public enum ECrmTipoEtapa
    {
        [Description("Lead")]
        Lead = 0,
        [Description("Oportunidade")]
        Oportunidade = 1
    }

    /// <summary>Estado funcional do lead. EF §5.1.</summary>
    public enum ECrmLeadStatus
    {
        [Description("Novo")]
        Novo = 0,
        [Description("Em qualificação")]
        EmQualificacao = 1,
        [Description("Convertido")]
        Convertido = 2,
        [Description("Arquivado")]
        Arquivado = 3
    }

    /// <summary>Controle de arquivamento. EF §10.5 (estado_arquivo) e §10.18.</summary>
    public enum ECrmEstadoArquivo
    {
        [Description("Ativo")]
        Ativo = 0,
        [Description("Arquivado")]
        Arquivado = 1
    }

    /// <summary>Estado funcional da oportunidade. EF §5.2.</summary>
    public enum ECrmOportunidadeStatus
    {
        [Description("Ativa")]
        Ativa = 0,
        [Description("Ganha")]
        Ganha = 1,
        [Description("Perdida")]
        Perdida = 2,
        [Description("Arquivada")]
        Arquivada = 3
    }

    /// <summary>Tipo de participante da oportunidade. EF §10.8 (tipo_participante).</summary>
    public enum ECrmTipoParticipante
    {
        [Description("Cliente")]
        Cliente = 0,
        [Description("Usuário")]
        Usuario = 1
    }

    /// <summary>Tipo funcional de atividade do CRM. EF §10.9 (tipo_atividade).</summary>
    public enum ECrmTipoAtividade
    {
        [Description("Tarefa")]
        Tarefa = 0,
        [Description("Chamada")]
        Chamada = 1,
        [Description("Reunião")]
        Reuniao = 2,
        [Description("Email")]
        Email = 3,
        [Description("Nota")]
        Nota = 4,
        [Description("Discussão")]
        Discussao = 5,
        [Description("Arquivo")]
        Arquivo = 6,
        [Description("Log")]
        Log = 7
    }

    /// <summary>Entidade de contexto da atividade. EF §10.9 (entidade_tipo).</summary>
    public enum ECrmEntidadeContexto
    {
        [Description("Lead")]
        Lead = 0,
        [Description("Oportunidade")]
        Oportunidade = 1,
        [Description("Ticket")]
        Ticket = 2,
        [Description("Cliente")]
        Cliente = 3,
        [Description("Campanha")]
        Campanha = 4
    }

    /// <summary>Tipo de chamada. EF §7 CRM-032.</summary>
    public enum ECrmTipoChamada
    {
        [Description("Entrada")]
        Entrada = 0,
        [Description("Saída")]
        Saida = 1
    }

    /// <summary>Prioridade de tarefa/ticket. EF CRM-030/CRM-061.</summary>
    public enum ECrmPrioridade
    {
        [Description("Baixa")]
        Baixa = 0,
        [Description("Média")]
        Media = 1,
        [Description("Alta")]
        Alta = 2,
        [Description("Normal")]
        Normal = 3,
        [Description("Urgente")]
        Urgente = 4
    }

    /// <summary>Status de atividade. EF §5.3/§5.4 e CRM-031.</summary>
    public enum ECrmStatusAtividade
    {
        [Description("Em andamento")]
        EmAndamento = 0,
        [Description("Completa")]
        Completa = 1,
        [Description("Adiada")]
        Adiada = 2,
        [Description("Planejada")]
        Planejada = 3,
        [Description("Realizada")]
        Realizada = 4
    }

    /// <summary>Tipo de membro de lista de público. EF §10.12 (tipo_membro).</summary>
    public enum ECrmTipoMembro
    {
        [Description("Lead")]
        Lead = 0,
        [Description("Contato")]
        Contato = 1,
        [Description("Cliente")]
        Cliente = 2,
        [Description("Usuário")]
        Usuario = 3,
        [Description("Alvo")]
        Alvo = 4
    }

    /// <summary>Origem da resposta/ticket. EF §10.18/§10.20 (origem).</summary>
    public enum ECrmOrigemAtendimento
    {
        [Description("Web")]
        Web = 0,
        [Description("Email")]
        Email = 1,
        [Description("Interna")]
        Interna = 2
    }

    /// <summary>Tipo de resposta de ticket. EF §10.20 (tipo).</summary>
    public enum ECrmTipoRespostaTicket
    {
        [Description("Resposta")]
        Resposta = 0,
        [Description("Nota interna")]
        NotaInterna = 1
    }

    /// <summary>Status do pagamento Pix relacional. EF §5.6/§10.24.</summary>
    public enum ECrmStatusPagamentoPix
    {
        [Description("Pendente")]
        Pendente = 0,
        [Description("Aprovado")]
        Aprovado = 1,
        [Description("Rejeitado")]
        Rejeitado = 2,
        [Description("Outro")]
        Outro = 3
    }
}
