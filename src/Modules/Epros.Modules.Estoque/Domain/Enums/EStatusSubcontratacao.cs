namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Status da ordem de subcontratação — EF Subcontratação §7.1 (domínio pendente §15, proposto por autoria).
    /// </summary>
    public enum EStatusSubOrdem
    {
        Aberta = 0,
        EmProcesso = 1,
        Retornada = 2,
        Concluida = 3,
        Cancelada = 4
    }

    /// <summary>Status da remessa de subcontratação — EF §7.3 (domínio pendente, proposto por autoria).</summary>
    public enum EStatusSubEnvio
    {
        Rascunho = 0,
        Enviado = 1,
        Cancelado = 2
    }

    /// <summary>Status do retorno de subcontratação — EF §7.4 (domínio pendente, proposto por autoria).</summary>
    public enum EStatusSubRetorno
    {
        Rascunho = 0,
        Recebido = 1,
        Cancelado = 2
    }
}
