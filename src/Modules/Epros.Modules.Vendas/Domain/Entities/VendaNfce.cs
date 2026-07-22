using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaNfce (dados de emissão/transmissão da NFC-e).
    /// FK long -> Guid; herda EntidadeSaaSBase. O documento fiscal transmitido em si vive
    /// no módulo Fiscal (DocumentoFiscal); aqui ficam os dados de controle da venda.
    /// </summary>
    public class VendaNfce : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public long Numero { get; private set; }
        public int Serie { get; private set; }
        public string? IdCsc { get; private set; }
        public string? Csc { get; private set; }
        public EDocumentoFiscalStatus StatusInterno { get; private set; }
        public string? Chave { get; private set; }
        public DateTime DataHoraEmissao { get; private set; } = DateTime.UtcNow;
        public int StatusSefaz { get; private set; }
        public string? Protocolo { get; private set; }
        public string? Xml { get; private set; }
        public string? UltimoRetornoMensagemSefaz { get; private set; }
        public DateTime? DataHoraCancelamento { get; private set; }
        public string? ProtocoloCancelamento { get; private set; }
        public int StatusSefazCancelamento { get; private set; }
        public string? MotivoCancelamento { get; private set; }
        public string? XmlCancelamento { get; private set; }

        // Navegação intra-módulo
        public Venda Venda { get; private set; } = null!;

        protected VendaNfce() { } // EF Core

        public VendaNfce(Guid vendaId, int serie, long numero, string idCsc, string csc, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            Serie = serie;
            Numero = numero;
            IdCsc = idCsc;
            Csc = csc;
            StatusInterno = EDocumentoFiscalStatus.Recebido;
        }

        public void Transmitir(string chave, DateTime dataHoraEmissao, int statusSefaz, string protocolo, string alteradoPor)
        {
            StatusInterno = EDocumentoFiscalStatus.Autorizado;
            Chave = chave;
            DataHoraEmissao = dataHoraEmissao;
            StatusSefaz = statusSefaz;
            Protocolo = protocolo;
            MarcarAlterado(alteradoPor);
        }

        public void PrepararRetransmitirNfce(int serie, long numero, string alteradoPor)
        {
            Serie = serie;
            Numero = numero;
            MarcarAlterado(alteradoPor);
        }

        public void Rejeitar(int statusSefaz, string ultimoRetornoMensagemSefaz, string alteradoPor)
        {
            StatusInterno = EDocumentoFiscalStatus.Rejeitado;
            StatusSefaz = statusSefaz;
            UltimoRetornoMensagemSefaz = TruncarMensagemSefaz(ultimoRetornoMensagemSefaz);
            MarcarAlterado(alteradoPor);
        }

        public void RegistrarTimeOut(string ultimoRetornoMensagemSefaz, string alteradoPor)
        {
            StatusInterno = EDocumentoFiscalStatus.Recebido;
            UltimoRetornoMensagemSefaz = TruncarMensagemSefaz(ultimoRetornoMensagemSefaz);
            MarcarAlterado(alteradoPor);
        }

        public void Cancelar(DateTime dataHoraCancelamento, string protocoloCancelamento, int statusSefazCancelamento, string motivoCancelamento, string alteradoPor)
        {
            if (StatusSefaz != 100) return;
            StatusInterno = EDocumentoFiscalStatus.Cancelado;
            DataHoraCancelamento = dataHoraCancelamento;
            ProtocoloCancelamento = protocoloCancelamento;
            StatusSefazCancelamento = statusSefazCancelamento;
            MotivoCancelamento = motivoCancelamento;
            MarcarAlterado(alteradoPor);
        }

        private static string TruncarMensagemSefaz(string mensagem)
        {
            mensagem ??= string.Empty;
            return mensagem.Length > 300 ? mensagem[0..300] : mensagem;
        }
    }
}
