using System.Threading.Tasks;
using Epros.Modules.Fiscal.Domain.Entities;

namespace Epros.Modules.Fiscal.Application.Services
{
    public interface IHerculesFiscalService
    {
        Task<RetornoEmissaoDto> EmitirAsync(DocumentoFiscal documento);
        Task<RetornoCancelamentoDto> CancelarAsync(DocumentoFiscal documento, string justificativa);

        /// <summary>
        /// Envia evento de Carta de Correção Eletrônica (CC-e) para a NF-e/NFC-e autorizada.
        /// </summary>
        Task<RetornoEventoDto> CartaCorrecaoAsync(DocumentoFiscal documento, string textoCorrecao, int sequenciaEvento);

        /// <summary>
        /// Inutiliza uma faixa de numeração (numeros pulados/não usados) na SEFAZ.
        /// Não recebe DocumentoFiscal pois inutilização é sobre faixa, não sobre nota emitida.
        /// </summary>
        Task<RetornoInutilizacaoDto> InutilizarAsync(InutilizacaoFiscalRequest request);

        /// <summary>
        /// Verifica o status do serviço (NfeStatusServico) da SEFAZ para o emitente/UF/ambiente informados.
        /// Fiel ao legado <c>NfceNfeController.VerificarStatus</c>. cStat 107 = serviço em operação.
        /// </summary>
        Task<RetornoConsultaSefazDto> VerificarStatusServicoAsync(ConsultaStatusServicoRequest request);

        /// <summary>
        /// Consulta o protocolo/situação de uma NF-e/NFC-e pela chave de acesso (NfeConsultaProtocolo).
        /// Fiel ao legado <c>NfceNfeController.ConsultaProtocolo</c>. cStat 100 = autorizada.
        /// </summary>
        Task<RetornoConsultaSefazDto> ConsultarProtocoloAsync(ConsultaProtocoloRequest request);
    }

    /// <summary>Requisição de verificação de status do serviço SEFAZ.</summary>
    public class ConsultaStatusServicoRequest
    {
        /// <summary>Documento (CNPJ/CPF) do emitente — usado para resolver certificado/emitente.</summary>
        public string Documento { get; set; } = string.Empty;

        /// <summary>Modelo: 55 (NF-e) ou 65 (NFC-e).</summary>
        public string Modelo { get; set; } = "55";

        /// <summary>1=Produção, 2=Homologação.</summary>
        public int Ambiente { get; set; } = 2;
    }

    /// <summary>Requisição de consulta de protocolo (situação da nota) por chave.</summary>
    public class ConsultaProtocoloRequest
    {
        public string Chave { get; set; } = string.Empty;

        /// <summary>1=Produção, 2=Homologação.</summary>
        public int Ambiente { get; set; } = 2;

        /// <summary>Modelo: 55 (NF-e) ou 65 (NFC-e).</summary>
        public string Modelo { get; set; } = "55";
    }

    /// <summary>Retorno genérico de consulta à SEFAZ (status de serviço / protocolo).</summary>
    public class RetornoConsultaSefazDto
    {
        public bool Sucesso { get; set; }
        public int StatusSefaz { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string XmlRetorno { get; set; } = string.Empty;
    }

    public class RetornoEmissaoDto
    {
        public bool Sucesso { get; set; }
        public string ChaveAcesso { get; set; } = string.Empty;
        public string Protocolo { get; set; } = string.Empty;
        public string Recibo { get; set; } = string.Empty;
        public int StatusSefaz { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string XmlEnvio { get; set; } = string.Empty;
        public string XmlRetorno { get; set; } = string.Empty;
        public string PdfCaminho { get; set; } = string.Empty;
        public string XmlCaminho { get; set; } = string.Empty;
    }

    public class RetornoCancelamentoDto
    {
        public bool Sucesso { get; set; }
        public int StatusSefaz { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string Protocolo { get; set; } = string.Empty;
        public string XmlRetorno { get; set; } = string.Empty;
    }

    /// <summary>Retorno genérico de evento (CC-e).</summary>
    public class RetornoEventoDto
    {
        public bool Sucesso { get; set; }
        public int StatusSefaz { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string Protocolo { get; set; } = string.Empty;
        public string XmlRetorno { get; set; } = string.Empty;
    }

    /// <summary>Requisição de inutilização de faixa de numeração.</summary>
    public class InutilizacaoFiscalRequest
    {
        /// <summary>Modelo: 55 (NF-e) ou 65 (NFC-e).</summary>
        public string Modelo { get; set; } = "55";

        /// <summary>1=Produção, 2=Homologação.</summary>
        public int Ambiente { get; set; } = 2;

        public int Serie { get; set; }
        public long NumeroInicial { get; set; }
        public long NumeroFinal { get; set; }

        /// <summary>Ano (2 dígitos) da inutilização.</summary>
        public int Ano { get; set; }

        public string Justificativa { get; set; } = string.Empty;
    }

    public class RetornoInutilizacaoDto
    {
        public bool Sucesso { get; set; }
        public int StatusSefaz { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string Protocolo { get; set; } = string.Empty;
        public string XmlRetorno { get; set; } = string.Empty;
    }
}
