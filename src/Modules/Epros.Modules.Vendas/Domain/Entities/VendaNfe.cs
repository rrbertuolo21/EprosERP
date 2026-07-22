using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaNfe (dados de emissão/transmissão da NF-e).
    /// FK long -> Guid; VO Documento -> string?; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaNfe : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public long Numero { get; private set; }
        public int Serie { get; private set; }
        public DateTime DataHoraEmissao { get; private set; } = DateTime.UtcNow;
        public DateTime? DataHoraSaida { get; private set; }
        public EDocumentoFiscalStatus StatusInterno { get; set; }
        public int StatusSefaz { get; private set; }
        public string? Chave { get; private set; }
        public string? Protocolo { get; private set; }
        public string? Xml { get; private set; }
        public string? UltimoRetornoMensagemSefaz { get; private set; }
        public DateTime DataHoraCancelamento { get; private set; }
        public string? ProtocoloCancelamento { get; private set; }
        public int StatusSefazCancelamento { get; private set; }
        public string? MotivoCancelamento { get; private set; }
        public string? XmlCancelamento { get; private set; }
        public bool EmbuteFrete { get; private set; }
        public bool EmbuteSeguro { get; private set; }
        public bool EmbuteAcrescimo { get; private set; }
        public bool EmbuteOutro { get; private set; }

        // Navegações intra-módulo
        public Venda Venda { get; private set; } = null!;
        public VendaNfeIntermediador? Intermediador { get; private set; }
        public VendaNfeExportacao? VendaNfeExportacao { get; private set; }

        private readonly List<VendaNfeCartaCorrecao> _cartasCorrecoes = new();
        public IReadOnlyCollection<VendaNfeCartaCorrecao> CartasCorrecoes => _cartasCorrecoes.AsReadOnly();

        protected VendaNfe() { } // EF Core

        public VendaNfe(Guid vendaId, long numero, int serie, DateTime? dataHoraSaida, string tenantId, string criadoPor, bool embuteFrete = true, bool embuteSeguro = true, bool embuteAcrescimo = true, bool embuteOutro = true)
            : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            Numero = numero;
            Serie = serie;
            DataHoraSaida = dataHoraSaida;
            StatusInterno = EDocumentoFiscalStatus.Recebido;
            EmbuteFrete = embuteFrete;
            EmbuteSeguro = embuteSeguro;
            EmbuteAcrescimo = embuteAcrescimo;
            EmbuteOutro = embuteOutro;
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

        /// <summary>
        /// Porte fiel de VendaNfe.AtualizarNumeroSerie. Só permite atualizar se ainda não
        /// foi transmitido/autorizado ou cancelado.
        /// </summary>
        public void AtualizarNumeroSerie(int serie, long numero, DateTime? dataHoraSaida, string alteradoPor)
        {
            if (StatusInterno == EDocumentoFiscalStatus.Autorizado || StatusInterno == EDocumentoFiscalStatus.Cancelado)
                return;

            Serie = serie;
            Numero = numero;
            DataHoraSaida = dataHoraSaida;
            MarcarAlterado(alteradoPor);
        }

        public void Rejeitar(int statusSefaz, string ultimoRetornoMensagemSefaz, string alteradoPor)
        {
            StatusInterno = EDocumentoFiscalStatus.Rejeitado;
            StatusSefaz = statusSefaz;
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

        public void AdicionarCartaCorrecao(ETipoAmbiente ambiente, string textoCorrecao, string criadoPor)
        {
            _cartasCorrecoes.Add(new VendaNfeCartaCorrecao(Id, textoCorrecao, TenantId, criadoPor));
        }

        public void CorrigirCartaCorrecao(int sequenciaEvento)
        {
            var cc = _cartasCorrecoes.LastOrDefault();
            cc?.Corrigir(sequenciaEvento);
        }

        public void RejeitarCartaCorrecao(int statusSefaz, string? motivoRejeicaoSefaz)
        {
            var cc = _cartasCorrecoes.LastOrDefault();
            cc?.Rejeitar(0, statusSefaz, motivoRejeicaoSefaz);
        }

        public void AdicionarIntermediador(string documento, string? identificadorIntermediador, string criadoPor)
        {
            Intermediador = new VendaNfeIntermediador(Id, documento, identificadorIntermediador, TenantId, criadoPor);
        }

        public void AlterarIntermediador(string documento, string? identificadorIntermediador, string alteradoPor)
        {
            Intermediador?.Alterar(documento, identificadorIntermediador, alteradoPor);
        }

        public void AdicionarExportacao(EEstado ufSaidaPais, string localExportacao, string? localDespacho, string criadoPor)
        {
            VendaNfeExportacao = new VendaNfeExportacao(Id, ufSaidaPais, localExportacao, localDespacho, TenantId, criadoPor);
        }

        public void AlterarExportacao(EEstado ufSaidaPais, string localExportacao, string? localDespacho, string alteradoPor)
        {
            VendaNfeExportacao?.Alterar(ufSaidaPais, localExportacao, localDespacho, alteradoPor);
        }

        private static string TruncarMensagemSefaz(string mensagem)
        {
            mensagem ??= string.Empty;
            return mensagem.Length > 300 ? mensagem[0..300] : mensagem;
        }
    }
}
