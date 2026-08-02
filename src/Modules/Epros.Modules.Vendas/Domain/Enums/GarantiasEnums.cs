using System.ComponentModel;

namespace Epros.Modules.Vendas.Domain.Enums
{
    // ============================================================================
    // Enums do submódulo Garantias (VEN-GAR).
    // Fonte funcional: EF_7_VENDAS_GARANTIAS_V1 (§0.1, §5, §10). Enums locais do módulo.
    //
    // CORREÇÃO INV-01 (ratificada pela EF §0.1/GAR-007): a garantia passa a ter DUAS
    // dimensões de vigência — tempo (Dias/Meses/Anos) + uso (km/horas) — e o vencimento
    // é o que ocorrer primeiro. O domínio de tempo {Dias,Meses,Anos} deixa de ser nota
    // de autoria e passa a ser RATIFICADO (é a unidade_tempo). A lista final de
    // unidade_uso por vertical (ex.: "horas de motor" p/ máquina) permanece
    // valida-humano (Siser) antes de gravar garantias veiculares reais.
    // ============================================================================

    /// <summary>Unidade da dimensão de TEMPO da garantia (unidade_tempo). GAR-007 — domínio {Dias,Meses,Anos} ratificado (EF §0.1).</summary>
    public enum EGarantiaTipoDuracao
    {
        [Description("Dias")]
        Dias = 0,
        [Description("Meses")]
        Meses = 1,
        [Description("Anos")]
        Anos = 2
    }

    /// <summary>Unidade da dimensão de USO da garantia (unidade_uso). GAR-007 — domínio {km,horas}; lista final por vertical valida-humano (Siser).</summary>
    public enum EGarantiaUnidadeUso
    {
        [Description("Sem uso")]
        Nenhuma = 0,
        [Description("Quilômetros")]
        Km = 1,
        [Description("Horas")]
        Horas = 2
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
