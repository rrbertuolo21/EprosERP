using System.IO;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Fiscal.Application.Commands
{
    /// <summary>
    /// Comando de loader (carga em massa) de uma tabela de referência fiscal a partir de um arquivo
    /// enviado (<c>POST .../atualizar</c>, IFormFile). O conteúdo do arquivo é transportado como
    /// <see cref="Stream"/> já aberto; o handler correspondente faz o parse e a persistência/atualização.
    /// Fiel ao mecanismo de carga do legado (loaders de NCM/CFOP-padrão/Código de Serviço/FCP/IBPT),
    /// que era como as tabelas NCM/CEST e alíquotas eram populadas.
    /// </summary>
    /// <param name="Conteudo">Stream do arquivo enviado (CSV/TXT/JSON conforme a tabela).</param>
    /// <param name="NomeArquivo">Nome original do arquivo (para inferir o formato quando necessário).</param>
    public record AtualizarTabelaNcmCommand(Stream Conteudo, string NomeArquivo) : ICommand;

    /// <summary>Loader da tabela de CFOP-padrão (nacional) a partir de arquivo CSV/TXT.</summary>
    /// <param name="Conteudo">Stream do arquivo enviado.</param>
    /// <param name="NomeArquivo">Nome original do arquivo.</param>
    public record AtualizarTabelaCfopPadraoCommand(Stream Conteudo, string NomeArquivo) : ICommand;

    /// <summary>Loader da tabela de Códigos de Serviço da SEFAZ a partir de arquivo CSV/TXT.</summary>
    /// <param name="Conteudo">Stream do arquivo enviado.</param>
    /// <param name="NomeArquivo">Nome original do arquivo.</param>
    public record AtualizarTabelaCodigoServicoSefazCommand(Stream Conteudo, string NomeArquivo) : ICommand;

    /// <summary>Loader da tabela de alíquotas de FCP por UF a partir de arquivo CSV/TXT.</summary>
    /// <param name="Conteudo">Stream do arquivo enviado.</param>
    /// <param name="NomeArquivo">Nome original do arquivo.</param>
    public record AtualizarTabelaFcpAliquotaUfCommand(Stream Conteudo, string NomeArquivo) : ICommand;

    /// <summary>Loader da tabela nacional de alíquotas IBPT (por NCM/UF) a partir de arquivo CSV/TXT.</summary>
    /// <param name="Conteudo">Stream do arquivo enviado.</param>
    /// <param name="NomeArquivo">Nome original do arquivo.</param>
    public record AtualizarTabelaIbptCommand(Stream Conteudo, string NomeArquivo) : ICommand;
}
