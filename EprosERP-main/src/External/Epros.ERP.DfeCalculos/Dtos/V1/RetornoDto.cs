namespace Epros.ERP.DfeCalculos.Dtos.V1
{
    public record RetornoErro400(string Message);
    public record RetornoErros422(string[] Messages);
    public record RetornoEmissao200(string Protocolo, string Chave, string Xml, decimal ImpostoFederal, decimal ImpostoMunicipal, decimal ImpostoEstadual, IEnumerable<RetornoNcmImposto> retornoNcmImpostos, string Observacao);
    public record RetornoCancelamento200(string Message, string XmlEventoCancelamento);
    public record RetornoStatus200(string Codigo, string Message);
    public record RetornoConsultaProtocolo200(string Codigo, string Message, string xmlAutorizacao, string xml);
    public record RetornoInutilizacao200(string Codigo, string Message, string Protocolo, string Xml);
    public record RetornoCartaCorrecao200(string Codigo, string Message, string Xml);
    public record RetornoInutilizacaoObter200(DateTime DataInutilizacao, string Uf, string Documento, int Ano, int Serie, long NrNfInicial, long NrNfFinal, string modeloDocumento, int status, string Justificativa, string xml);
    public record RetornoNcmImposto(string ncm, string execao, decimal aliqNacionalFederal, decimal aliqImportadoFederal, decimal aliqEstadual, decimal aliqMunicipal, string versao);
    public record RetornoXmlLocalizadorExternoId(string Message, string Xml, string LocalizadorExternoId);
}

