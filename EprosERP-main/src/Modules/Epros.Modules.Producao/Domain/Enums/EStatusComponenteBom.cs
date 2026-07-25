namespace Epros.Modules.Producao.Domain.Enums
{
    /// <summary>Status operacional da linha de componente da estrutura (BOM-EF §6).</summary>
    public enum EStatusComponenteBom
    {
        Rascunho = 0,
        Ativo = 1,
        Inativo = 2
    }

    /// <summary>Status genérico Ativo/Inativo para grupos, instruções e vínculos (BOM-EF §6).</summary>
    public enum EStatusAtivoInativo
    {
        Ativo = 0,
        Inativo = 1
    }
}
