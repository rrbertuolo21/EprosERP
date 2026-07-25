using System.ComponentModel;

namespace Epros.Modules.Vendas.Domain.Enums
{
    // ============================================================================
    // Enums do submódulo Garantias (VEN-GAR).
    // Fonte funcional: EF_7_VENDAS_GARANTIAS_V1 (§5, §10). Enums locais do módulo.
    // GAR-007: o material informa que tipo_duracao é enumerado mas NÃO informa os
    // valores; os valores abaixo são nota de autoria (proposta implantável) e devem
    // ser validados pela Siser antes de uso em produção.
    // ============================================================================

    /// <summary>Unidade de duração da garantia. GAR-006/GAR-007 (domínio não informado no material — nota de autoria).</summary>
    public enum EGarantiaTipoDuracao
    {
        [Description("Dias")]
        Dias = 0,
        [Description("Meses")]
        Meses = 1,
        [Description("Anos")]
        Anos = 2
    }

    /// <summary>Situação funcional da cobertura aplicada. EF §5.2.</summary>
    public enum EGarantiaSituacaoCobertura
    {
        [Description("Indeterminada")]
        Indeterminada = 0,
        [Description("Vigente")]
        Vigente = 1,
        [Description("Vencida")]
        Vencida = 2
    }

    /// <summary>Entidade alterada no histórico de garantia. EF §10.3 (entidade_tipo).</summary>
    public enum EGarantiaEntidadeTipo
    {
        [Description("Política")]
        Politica = 0,
        [Description("Cobertura")]
        Cobertura = 1
    }

    /// <summary>Evento funcional de auditoria de garantia. EF §10.3 (evento). GAR-011.</summary>
    public enum EGarantiaEvento
    {
        [Description("Criação")]
        Criacao = 0,
        [Description("Edição")]
        Edicao = 1,
        [Description("Inativação")]
        Inativacao = 2,
        [Description("Exclusão")]
        Exclusao = 3,
        [Description("Aplicação")]
        Aplicacao = 4
    }
}
