using System.ComponentModel;

namespace Epros.Modules.Vendas.Domain.Enums
{
    // ============================================================================
    // Enums do submódulo Portal do Cliente (VEN-PCL).
    // Fonte funcional: EF_7_VENDAS_PORTAL_DO_CLIENTE_V1 (§16). Enums locais do módulo.
    // ============================================================================

    /// <summary>Estado de acesso do usuário externo. EF §16.1 (status).</summary>
    public enum EPortalUsuarioStatus
    {
        [Description("Ativo")]
        Ativo = 0,
        [Description("Inativo")]
        Inativo = 1,
        [Description("Bloqueado")]
        Bloqueado = 2
    }

    /// <summary>Estado do formulário web. EF §16.2 (status).</summary>
    public enum EPortalFormularioStatus
    {
        [Description("Rascunho")]
        Rascunho = 0,
        [Description("Publicado")]
        Publicado = 1,
        [Description("Inativo")]
        Inativo = 2
    }

    /// <summary>Estado da solicitação do cliente. EF §16.4 (status).</summary>
    public enum EPortalSolicitacaoStatus
    {
        [Description("Aberta")]
        Aberta = 0,
        [Description("Em atendimento")]
        EmAtendimento = 1,
        [Description("Respondida")]
        Respondida = 2,
        [Description("Encerrada")]
        Encerrada = 3,
        [Description("Cancelada")]
        Cancelada = 4
    }

    /// <summary>Recurso do portal para permissão/auditoria. EF §16.5 (recurso).</summary>
    public enum EPortalRecurso
    {
        [Description("Pedidos")]
        Pedidos = 0,
        [Description("Documentos fiscais")]
        DocumentosFiscais = 1,
        [Description("Títulos")]
        Titulos = 2,
        [Description("Rastreio")]
        Rastreio = 3,
        [Description("Formulários")]
        Formularios = 4,
        [Description("Solicitações")]
        Solicitacoes = 5,
        [Description("Usuários")]
        Usuarios = 6
    }
}
