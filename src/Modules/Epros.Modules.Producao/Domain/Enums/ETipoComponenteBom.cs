namespace Epros.Modules.Producao.Domain.Enums
{
    /// <summary>
    /// PRD-BOM — Tipo funcional do componente da estrutura (PD6 · DP-BOM-010/011/012/013).
    /// Governa o comportamento na explosão multinível:
    /// - Normal: componente consumido diretamente (folha ou submontagem estocada).
    /// - Alternativo: pode substituir outro componente do mesmo grupo (escolha na explosão).
    /// - Substituto: usado apenas quando o principal está indisponível.
    /// - Submontagem: possui estrutura própria; a explosão desce um nível.
    /// - Fantasma: não é estocado nem consumido como item — a explosão o "atravessa"
    ///   e consome diretamente os componentes da sua submontagem (item fantasma).
    /// </summary>
    public enum ETipoComponenteBom
    {
        Normal = 0,
        Alternativo = 1,
        Substituto = 2,
        Submontagem = 3,
        Fantasma = 4
    }
}
