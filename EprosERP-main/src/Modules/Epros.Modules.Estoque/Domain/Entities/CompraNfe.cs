using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// NF-e vinculada à compra (entrada própria ou nota já emitida pelo fornecedor). Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraNfe.
    /// </summary>
    public class CompraNfe : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
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

        // Navegação intra-módulo
        public CompraNfeIntermediador? Intermediador { get; private set; }
        public ICollection<CompraNfeCartaCorrecao> CartasCorrecoes { get; private set; } = new List<CompraNfeCartaCorrecao>();
        public Compra? Compra { get; private set; }

        protected CompraNfe() { } // EF Core

        /// <summary>Entrada própria (NF-e a emitir).</summary>
        public CompraNfe(Guid compraId, long numero, int serie, DateTime? dataHoraSaida, string tenantId, string criadoPor, bool embuteFrete = true, bool embuteSeguro = true, bool embuteAcrescimo = true, bool embuteOutro = true)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            Numero = numero;
            Serie = serie;
            DataHoraSaida = dataHoraSaida;
            StatusInterno = EDocumentoFiscalStatus.Recebido;
            EmbuteFrete = embuteFrete;
            EmbuteSeguro = embuteSeguro;
            EmbuteAcrescimo = embuteAcrescimo;
            EmbuteOutro = embuteOutro;
        }

        /// <summary>Entrada de mercadoria com NF-e já emitida pelo fornecedor.</summary>
        public CompraNfe(Guid compraId, string? chave, long numero, int serie, DateTime dataHoraEmissao, DateTime? dataHoraSaida, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            Chave = chave;
            Serie = serie;
            Numero = numero;
            DataHoraEmissao = dataHoraEmissao;
            DataHoraSaida = dataHoraSaida;
            StatusInterno = EDocumentoFiscalStatus.Autorizado;
        }

        public void Transmitir(string chave, DateTime dataHoraEmissao, int statusSefaz, string protocolo, string usuario)
        {
            StatusInterno = EDocumentoFiscalStatus.Autorizado;
            Chave = chave;
            DataHoraEmissao = dataHoraEmissao;
            StatusSefaz = statusSefaz;
            Protocolo = protocolo;
            MarcarAlterado(usuario);
        }

        public void PrepararRetransmitirNfce(int serie, long numero, string usuario)
        {
            Serie = serie;
            Numero = numero;
            MarcarAlterado(usuario);
        }

        public void AtualizarNumeroSerie(int serie, long numero, DateTime? dataHoraSaida, string usuario)
        {
            // Só permite atualizar se ainda não foi transmitido/autorizado
            if (StatusInterno == EDocumentoFiscalStatus.Autorizado ||
                StatusInterno == EDocumentoFiscalStatus.Cancelado)
            {
                return; // Não atualiza se já foi autorizado ou cancelado
            }

            Serie = serie;
            Numero = numero;
            DataHoraSaida = dataHoraSaida;
            MarcarAlterado(usuario);
        }

        public void Rejeitar(int statusSefaz, string ultimoRetornoMensagemSefaz, string usuario)
        {
            StatusInterno = EDocumentoFiscalStatus.Rejeitado;
            StatusSefaz = statusSefaz;
            UltimoRetornoMensagemSefaz = ((ultimoRetornoMensagemSefaz ?? "").Length > 300 ? ultimoRetornoMensagemSefaz![0..300] : ultimoRetornoMensagemSefaz);
            MarcarAlterado(usuario);
        }

        public void Cancelar(DateTime dataHoraCancelamento, string protocoloCancelamento, int statusSefazCancelamento, string motivoCancelamento, string usuario)
        {
            if (StatusSefaz != 100) return;
            StatusInterno = EDocumentoFiscalStatus.Cancelado;
            DataHoraCancelamento = dataHoraCancelamento;
            ProtocoloCancelamento = protocoloCancelamento;
            StatusSefazCancelamento = statusSefazCancelamento;
            MotivoCancelamento = motivoCancelamento;
            MarcarAlterado(usuario);
        }

        public void AdicionarCartaCorrecao(ETipoAmbiente ambiente, string textoCorrecao, string usuario)
        {
            CartasCorrecoes.Add(new CompraNfeCartaCorrecao(Id, textoCorrecao, TenantId, usuario));
        }

        public void CorrigirCartaCorrecao(int sequenciaEvento)
        {
            var cc = CartasCorrecoes.Last();
            cc.Corrigir(sequenciaEvento);
        }

        public void RejeitarCartaCorrecao(int statusSefaz, string? motivoRejeicaoSefaz)
        {
            var cc = CartasCorrecoes.Last();
            cc.Rejeitar(0, statusSefaz, motivoRejeicaoSefaz);
        }

        public void AdicionarIntermediador(string documento, string? identificadorIntermediador, string usuario)
        {
            Intermediador = new CompraNfeIntermediador(Id, documento, identificadorIntermediador, TenantId, usuario);
        }
    }
}
