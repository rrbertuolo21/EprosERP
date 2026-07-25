using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Fiscal.Application.Commands
{
    /// <summary>
    /// Importa uma NF-e / NFC-e JÁ AUTORIZADA a partir do seu XML (procNFe/nfeProc), persistindo-a
    /// como <c>DocumentoFiscal</c> no estado Autorizado, sem transmitir nada à SEFAZ.
    ///
    /// Casos de uso (paridade com o legado <c>salvar-nf-com-xml</c>):
    /// - contingência: a nota foi autorizada por outro emissor/offline e precisa entrar no sistema;
    /// - reconciliação/migração: importar o histórico autorizado direto do XML.
    ///
    /// FALLBACK HONESTO: se o XML não for um documento autorizado (sem protNFe/chave), o comando falha —
    /// nunca marca como autorizado um XML que não tem protocolo real.
    /// </summary>
    public record SalvarDocumentoFiscalComXmlCommand(
        string Xml,
        System.Guid? EmpresaId = null,
        System.Guid? VendaOrigemId = null
    ) : ICommand;

    /// <summary>
    /// Registra o CANCELAMENTO de um documento fiscal a partir de um XML de evento externo
    /// (<c>procEventoNFe</c> / <c>retEnvEvento</c>) — quando o cancelamento foi homologado fora deste
    /// sistema (contingência/reconciliação). Vincula o evento ao documento pela chave de acesso.
    ///
    /// FALLBACK HONESTO: exige cStat de homologação (135/155) e chave correspondente a um documento
    /// existente; caso contrário falha, sem alterar o estado do documento.
    /// </summary>
    public record RegistrarCancelamentoPorXmlCommand(
        string XmlEvento,
        string? Justificativa = null
    ) : ICommand;
}
