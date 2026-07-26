using System.ComponentModel;

namespace Epros.Modules.Vendas.Domain.Enums
{
    // ============================================================================
    // Enums do submódulo Gestão de Contratos de Venda (VEN-GCV).
    // Fonte funcional: EF_7_VENDAS_GESTAO_DE_CONTRATOS_DE_VENDA_V1 (§10, §14).
    // GCV-015 / Nota de autoria §14.1: domínio de status normalizado pela EF (o material
    // trazia códigos/nomes divergentes). Este é o domínio Epros oficial.
    // ============================================================================

    /// <summary>Status do contrato. EF §10.2 (status) — domínio Epros normalizado (GCV-015).</summary>
    public enum EContratoStatus
    {
        [Description("Rascunho")]
        Rascunho = 0,
        [Description("Aguardando assinaturas")]
        AguardandoAssinaturas = 1,
        [Description("Ativo")]
        Ativo = 2,
        [Description("Expirado")]
        Expirado = 3,
        [Description("Arquivado")]
        Arquivado = 4,
        [Description("Cancelado")]
        Cancelado = 5
    }

    /// <summary>Origem do documento. EF §10.2 (tipo_origem). GCV-008: vazio → Contrato.</summary>
    public enum EContratoTipoOrigem
    {
        [Description("Contrato")]
        Contrato = 0,
        [Description("Modelo")]
        Modelo = 1,
        [Description("Outro")]
        Outro = 2
    }

    /// <summary>Status da renovação. EF §10.7 (status). GCV-029.</summary>
    public enum EContratoRenovacaoStatus
    {
        [Description("Rascunho")]
        Rascunho = 0,
        [Description("Pendente")]
        Pendente = 1,
        [Description("Aprovada")]
        Aprovada = 2,
        [Description("Ativa")]
        Ativa = 3,
        [Description("Expirada")]
        Expirada = 4,
        [Description("Cancelada")]
        Cancelada = 5
    }

    /// <summary>Parte assinante. EF §10.8 (parte). GCV-035.</summary>
    public enum EContratoParteAssinatura
    {
        [Description("Empresa")]
        Empresa = 0,
        [Description("Cliente")]
        Cliente = 1,
        [Description("Convidado")]
        Convidado = 2
    }

    /// <summary>Tipo de assinatura. EF §10.8 (tipo_assinatura). GCV-031.</summary>
    public enum EContratoTipoAssinatura
    {
        [Description("Digital")]
        Digital = 0,
        [Description("Desenhada")]
        Desenhada = 1
    }

    /// <summary>Evento de histórico de contrato. EF §10.10 (evento).</summary>
    public enum EContratoEvento
    {
        [Description("Criação")]
        Criacao = 0,
        [Description("Edição")]
        Edicao = 1,
        [Description("Publicação")]
        Publicacao = 2,
        [Description("Assinatura")]
        Assinatura = 3,
        [Description("Exclusão")]
        Exclusao = 4,
        [Description("Renovação")]
        Renovacao = 5,
        [Description("Automação")]
        Automacao = 6
    }
}
