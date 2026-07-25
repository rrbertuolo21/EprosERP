using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>
    /// Importa uma NF-e de entrada a partir do conteúdo do seu XML: faz o parse (NfeXmlParser),
    /// monta o agregado de compra e persiste via <see cref="LancarCompraFiscalCommand"/>.
    /// </summary>
    /// <param name="ConteudoXml">Conteúdo textual do arquivo XML da NF-e de entrada.</param>
    public record ImportarCompraXmlCommand(string ConteudoXml) : ICommand;
}
