namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    // Enums de RBAC (APP-TEN-003) e Catálogos Globais SaaS (APP-CAT), portados das EFs
    // 0_APLICATIVO_USUARIOS_E_PAPEIS e 0_APLICATIVO_CATALOGOS_GLOBAIS_SAAS.

    /// <summary>Tipo de papel (RoleType do material: team/client).</summary>
    public enum ETipoPapel
    {
        Equipe = 1,
        Cliente = 2
    }

    /// <summary>Tipo de nível de usuário (LevelType: admin, free, paid, moderator, nonuser).</summary>
    public enum ENivelUsuarioTipo
    {
        Admin = 1,
        Free = 2,
        Paid = 3,
        Moderator = 4,
        NonUser = 5
    }

    /// <summary>Origem de um módulo ativo na resolução de módulos (APP-CAT REG-015/017).</summary>
    public enum EOrigemModuloAtivo
    {
        Plano = 1,
        Baseline = 2,
        Avulso = 3,
        Override = 4
    }
}
