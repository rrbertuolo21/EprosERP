using System.ComponentModel;

namespace Epros.Modules.Financeiro.Domain.Enums
{
    /// <summary>Origem funcional da taxa de câmbio (EF FIN-CAM §10.4 cam_taxa_cambio).</summary>
    public enum EOrigemTaxaCambio
    {
        [Description("Manual")] Manual = 0,
        [Description("PTAX")] Ptax = 1
    }

    /// <summary>Estado da exposição cambial (EF FIN-CAM §10.5 / §9).</summary>
    public enum EStatusExposicaoCambial
    {
        [Description("Aberta")] Aberta = 0,
        [Description("Hedgeada")] Hedgeada = 1,
        [Description("Encerrada")] Encerrada = 2,
        [Description("Excluída")] Excluida = 3
    }

    /// <summary>Estado da reavaliação de títulos em moeda estrangeira (EF FIN-CAM §10.6).</summary>
    public enum EStatusReavaliacaoTitulo
    {
        [Description("Rascunho")] Rascunho = 0,
        [Description("Aprovada")] Aprovada = 1,
        [Description("Contabilizada")] Contabilizada = 2,
        [Description("Cancelada")] Cancelada = 3
    }
}
