using System.Collections.Generic;

namespace Epros.Modules.Fiscal.Application.Services
{
    /// <summary>
    /// Dados NEUTROS do emitente necessários para montar o XML e transmitir à SEFAZ.
    /// Não referencia entidades de domínio nem o namespace legado — é preenchido por
    /// quem conhece a empresa emitente (config/tenant) e injetado via
    /// <see cref="IEmitenteFiscalProvider"/>.
    /// </summary>
    public class EmitenteFiscalDto
    {
        /// <summary>CNPJ (14) ou CPF (11) do emitente, somente dígitos.</summary>
        public string Documento { get; set; } = string.Empty;

        /// <summary>Regime tributário (CRT): 1=SimplesNacional, 2=SimplesExcessoSublimite, 3=RegimeNormal, 4=SimplesMei.</summary>
        public int RegimeTributario { get; set; } = 3;

        public string RazaoSocial { get; set; } = string.Empty;
        public string? NomeFantasia { get; set; }
        public string? InscricaoEstadual { get; set; }
        public string? InscricaoMunicipal { get; set; }
        public string? Cnae { get; set; }
        public string? Telefone { get; set; }
        public string? LinkLogoMarca { get; set; }

        // --- Endereço do emitente (obrigatório para o XML) ---
        /// <summary>Sigla da UF do emitente, ex.: "SP".</summary>
        public string Uf { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = string.Empty;
        public int CodMunicipioIbge { get; set; }
        public string NomeMunicipio { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;

        // --- CSC (NFC-e) ---
        public string? Csc { get; set; }
        public string? CscId { get; set; }

        // --- Certificado digital A1 ---
        public CertificadoDigitalDto? Certificado { get; set; }
    }

    /// <summary>
    /// Certificado digital A1 em bytes + senha (modelo neutro; sem dependência de banco/arquivo).
    /// Origem esperada: <c>EmpresaCertificado</c> (bytes + senha) ou upload direto.
    /// </summary>
    public class CertificadoDigitalDto
    {
        /// <summary>Conteúdo do arquivo .pfx/.p12.</summary>
        public byte[] ArrayBytes { get; set; } = System.Array.Empty<byte>();

        /// <summary>Senha do certificado.</summary>
        public string Senha { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dados neutros do destinatário para o XML. Opcional (NFC-e pode ser sem destinatário).
    /// </summary>
    public class DestinatarioFiscalDto
    {
        public string Documento { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string? InscricaoEstadual { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }

        /// <summary>Indicador de IE do destinatário: 1=Contribuinte, 2=Isento, 9=NaoContribuinte.</summary>
        public int IndicadorIeDestinatario { get; set; } = 9;

        /// <summary>Indicador de consumidor final: 0=Normal, 1=ConsumidorFinal.</summary>
        public int ConsumidorFinal { get; set; } = 1;

        // --- Endereço do destinatário (opcional) ---
        public string? Uf { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public int CodMunicipioIbge { get; set; }
        public string? NomeMunicipio { get; set; }
        public string? Cep { get; set; }
    }

    /// <summary>
    /// Contexto fiscal completo de uma transmissão: emitente (+cert) e destinatário.
    /// O adapter de transmissão obtém isto do <see cref="IEmitenteFiscalProvider"/>,
    /// já que <c>DocumentoFiscal</c> não carrega esses dados.
    /// </summary>
    public class ContextoTransmissaoFiscal
    {
        public EmitenteFiscalDto Emitente { get; set; } = new();
        public DestinatarioFiscalDto? Destinatario { get; set; }
    }

    /// <summary>
    /// Fornece o contexto fiscal (emitente + certificado) para transmissão à SEFAZ,
    /// a partir do documento a ser emitido. Implementações reais buscam da configuração
    /// da empresa/tenant. Retorna <c>null</c> quando não há emitente/certificado configurado
    /// (o adapter então degrada de forma controlada — ver <c>MotorLegadoFiscalService</c>).
    /// </summary>
    public interface IEmitenteFiscalProvider
    {
        /// <summary>
        /// Resolve o contexto de transmissão para o documento informado.
        /// Retorna <c>null</c> se não houver emitente/certificado configurado.
        /// </summary>
        ContextoTransmissaoFiscal? ObterContexto(Domain.Entities.DocumentoFiscal documento);

        /// <summary>
        /// Resolve o contexto de transmissão pelo documento (CNPJ/CPF) do emitente + ambiente.
        /// Usado por consultas online que não têm uma nota persistida (status de serviço, consulta de
        /// protocolo por chave). Retorna <c>null</c> se não houver emitente/certificado configurado.
        /// </summary>
        ContextoTransmissaoFiscal? ObterContextoPorDocumento(string documentoEmitente, string modelo, int ambiente);
    }
}
