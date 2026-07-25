using System.ComponentModel;

namespace Epros.Modules.Financeiro.Domain.Enums
{
    /// <summary>Estado de versão orçamentária, período e budget (EF FIN-PO §7.1 / §12).</summary>
    public enum EStatusOrcamento
    {
        [Description("Rascunho")] Rascunho = 0,
        [Description("Aprovado")] Aprovado = 1,
        [Description("Ativo")] Ativo = 2,
        [Description("Encerrado")] Encerrado = 3
    }

    /// <summary>Estado da meta (EF FIN-PO §7.2 / §12.10).</summary>
    public enum EStatusMeta
    {
        [Description("Rascunho")] Rascunho = 0,
        [Description("Ativa")] Ativa = 1
    }

    /// <summary>Escopo da categoria de meta (EF FIN-PO §12.9).</summary>
    public enum EEscopoMeta
    {
        [Description("Qualquer")] Any = 0,
        [Description("Próprio")] Own = 1
    }

    /// <summary>Estado do milestone de meta (EF FIN-PO §12.11).</summary>
    public enum EStatusMilestone
    {
        [Description("Pendente")] Pendente = 0,
        [Description("Concluído")] Concluido = 1
    }
}
