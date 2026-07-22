using System;
using Epros.Modules.Fiscal.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class ConfiguracaoImpressaoNfce : EntidadeSaaSBase
    {
        // FK para Empresa (outro módulo) — referenciada por Guid, sem navegação cruzada.
        public Guid EmpresaId { get; private set; }
        public ENfceDetalheVendaNormal? DetalheVendaNormal { get; private set; }
        public ENfceDetalheVendaContingencia? DetalheVendaContingencia { get; private set; }
        public bool? ImprimeDescontoItem { get; private set; }
        public bool? ImprimeFoneEmitente { get; private set; }
        public float? MargemEsquerda { get; private set; }
        public float? MargemDireita { get; private set; }
        public ENfceModoImpressao? ModoImpressao { get; private set; }
        public ENfceLayoutQrCode? NfceLayoutQrCode { get; private set; }
        public EVersaoQrCode? VersaoQrCode { get; private set; }
        public bool? SegundaViaContingencia { get; private set; }

        protected ConfiguracaoImpressaoNfce() { } // EF Core

        public ConfiguracaoImpressaoNfce(
            Guid empresaId,
            ENfceDetalheVendaNormal? detalheVendaNormal,
            ENfceDetalheVendaContingencia? detalheVendaContingencia,
            bool? imprimeDescontoItem,
            bool? imprimeFoneEmitente,
            float? margemEsquerda,
            float? margemDireita,
            ENfceModoImpressao? modoImpressao,
            ENfceLayoutQrCode? nfceLayoutQrCode,
            EVersaoQrCode? versaoQrCode,
            bool? segundaViaContingencia,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            DetalheVendaNormal = detalheVendaNormal ?? ENfceDetalheVendaNormal.UmaLinha;
            DetalheVendaContingencia = detalheVendaContingencia ?? ENfceDetalheVendaContingencia.Completo;
            ImprimeDescontoItem = imprimeDescontoItem ?? false;
            ImprimeFoneEmitente = imprimeFoneEmitente ?? false;
            MargemEsquerda = margemEsquerda ?? 5;
            MargemDireita = margemDireita ?? 0;
            ModoImpressao = modoImpressao ?? ENfceModoImpressao.UnicaPagina;
            NfceLayoutQrCode = nfceLayoutQrCode ?? ENfceLayoutQrCode.Abaixo;
            VersaoQrCode = versaoQrCode ?? EVersaoQrCode.QrCodeVersao1;
            SegundaViaContingencia = segundaViaContingencia ?? false;
            Validar();
        }

        public void Alterar(
            ENfceDetalheVendaNormal? detalheVendaNormal,
            ENfceDetalheVendaContingencia? detalheVendaContingencia,
            bool? imprimeDescontoItem,
            bool? imprimeFoneEmitente,
            float? margemEsquerda,
            float? margemDireita,
            ENfceModoImpressao? modoImpressao,
            ENfceLayoutQrCode? nfceLayoutQrCode,
            EVersaoQrCode? versaoQrCode,
            bool? segundaViaContingencia,
            string alteradoPor)
        {
            DetalheVendaNormal = detalheVendaNormal ?? ENfceDetalheVendaNormal.UmaLinha;
            DetalheVendaContingencia = detalheVendaContingencia ?? ENfceDetalheVendaContingencia.Completo;
            ImprimeDescontoItem = imprimeDescontoItem ?? false;
            ImprimeFoneEmitente = imprimeFoneEmitente ?? false;
            MargemEsquerda = margemEsquerda ?? 5;
            MargemDireita = margemDireita ?? 0;
            ModoImpressao = modoImpressao ?? ENfceModoImpressao.UnicaPagina;
            NfceLayoutQrCode = nfceLayoutQrCode ?? ENfceLayoutQrCode.Abaixo;
            VersaoQrCode = versaoQrCode ?? EVersaoQrCode.QrCodeVersao1;
            SegundaViaContingencia = segundaViaContingencia ?? false;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
        }
    }
}
