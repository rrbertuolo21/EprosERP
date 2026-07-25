namespace Epros.Modules.Aplicativo.Domain.Enums
{
    /// <summary>
    /// Situação do fluxo de governança de upgrade de versão do Super Admin (APP-TEN-010).
    /// Maker-checker: solicitação → aprovação/rejeição → execução → conclusão/falha/rollback.
    /// </summary>
    public enum EStatusUpgradeVersao
    {
        Solicitado = 0,
        Aprovado = 1,
        Rejeitado = 2,
        EmExecucao = 3,
        Concluido = 4,
        Falho = 5,
        RollbackAplicado = 6
    }
}
