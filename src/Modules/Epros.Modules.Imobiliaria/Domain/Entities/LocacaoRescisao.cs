using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Rescisao/encerramento da locacao (ID7). Registra motivo, data, multa proporcional e a
    /// vistoria de saida. O CALCULO da multa proporcional e valida-contador (NF-02): a IMOBILIARIA
    /// persiste o valor informado/ratificado, sem inventar formula de proporcionalidade.
    /// </summary>
    public class LocacaoRescisao : EntidadeSaaSBase
    {
        public Guid LocacaoId { get; private set; }
        public string Motivo { get; private set; } = string.Empty;
        public DateTime DataRescisao { get; private set; }
        /// <summary>Aviso previo cumprido (dias). Informativo para o encerramento financeiro.</summary>
        public int? AvisoPrevioDias { get; private set; }
        /// <summary>Multa proporcional (valida-contador — NF-02). Default 0 quando nao aplicavel.</summary>
        public decimal MultaProporcional { get; private set; }
        public Guid? VistoriaSaidaId { get; private set; }

        protected LocacaoRescisao() { } // EF Core

        public LocacaoRescisao(
            Guid locacaoId,
            string motivo,
            DateTime dataRescisao,
            int? avisoPrevioDias,
            decimal multaProporcional,
            Guid? vistoriaSaidaId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            LocacaoId = locacaoId;
            Motivo = motivo;
            DataRescisao = dataRescisao.Date;
            AvisoPrevioDias = avisoPrevioDias;
            MultaProporcional = multaProporcional;
            VistoriaSaidaId = vistoriaSaidaId;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LocacaoRescisao>()
                .Requires()
                .AreNotEquals(LocacaoId, Guid.Empty, nameof(LocacaoId),
                    "A rescisao exige locacao. [Origem: LocacaoRescisao]")
                .IsNotNullOrEmpty(Motivo, nameof(Motivo),
                    "O motivo da rescisao e obrigatorio. [Origem: LocacaoRescisao]")
                .IsGreaterOrEqualsThan(MultaProporcional, 0, nameof(MultaProporcional),
                    "A multa nao pode ser negativa. [Origem: LocacaoRescisao] (NF-02)"));
        }
    }
}
