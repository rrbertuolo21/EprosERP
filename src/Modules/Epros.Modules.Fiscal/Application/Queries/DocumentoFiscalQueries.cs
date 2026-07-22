using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Queries
{
    /// <summary>Lista documentos fiscais paginados, com filtro opcional por status.</summary>
    public record ListarDocumentosFiscaisQuery(
        string? Status = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    /// <summary>Obtém um documento fiscal (com itens) pelo Id.</summary>
    public record ObterDocumentoFiscalPorIdQuery(Guid Id) : IQuery<CommandResult>;

    /// <summary>Obtém um documento fiscal (com itens) pela chave de acesso (44 dígitos).</summary>
    public record ObterDocumentoFiscalPorChaveQuery(string Chave) : IQuery<CommandResult>;

    /// <summary>Obtém o XML de retorno (autorização) de um documento fiscal pelo Id.</summary>
    public record ObterXmlDocumentoFiscalQuery(Guid Id) : IQuery<CommandResult>;

    /// <summary>
    /// Gera (ou recupera) o PDF do DANFE (NF-e) / cupom (NFC-e) de um documento pelo Id.
    /// Dados retornados em <c>CommandResult.Dados</c>: <c>{ Conteudo(byte[]), NomeArquivo(string) }</c>.
    /// </summary>
    public record ObterDanfePdfQuery(Guid Id) : IQuery<CommandResult>;

    /// <summary>Payload do PDF do DANFE/cupom.</summary>
    public record DanfePdfDto(byte[] Conteudo, string NomeArquivo);

    /// <summary>Obtém o XML de retorno (autorização) de um documento fiscal pela chave de acesso.</summary>
    public record ObterXmlDocumentoFiscalPorChaveQuery(string Chave) : IQuery<CommandResult>;

    public class ListarDocumentosFiscaisQueryHandler : IRequestHandler<ListarDocumentosFiscaisQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ListarDocumentosFiscaisQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ListarDocumentosFiscaisQuery request, CancellationToken cancellationToken)
        {
            var query = _context.DocumentosFiscais
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(d => d.Status == request.Status);
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(d => d.DataEmissao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(d => new
                {
                    id = d.Id,
                    modelo = d.Modelo == "55" ? "NFe" : "NFCe",
                    status = d.Status,
                    chaveAcesso = d.ChaveAcesso,
                    protocolo = d.Protocolo,
                    urlDanfe = d.PdfCaminho,
                    urlXml = d.XmlCaminho,
                    // TODO(F7 contingência): a emissão em contingência (troca de tpEmis para SVC-AN/SVC-RS
                    // ou EPEC, assinatura offline e fila de reenvio) NÃO está implementada — apenas as flags
                    // de IMPRESSÃO de contingência (ConfiguracaoImpressaoNfce) existem. Enquanto o motor não
                    // emitir com tpEmis!=1, este indicador permanece false honestamente. Ao implementar,
                    // derivar de um campo persistido em DocumentoFiscal (ex.: TipoEmissao) e não de constante.
                    emContingencia = false,
                    autorizadoEm = d.DataAutorizacao,
                    total = d.Total,
                    destinatarioCnpjCpf = d.DestinatarioCnpjCpf,
                    destinatarioNome = d.DestinatarioNome,
                    dataEmissao = d.DataEmissao
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterDocumentoFiscalPorIdQueryHandler : IRequestHandler<ObterDocumentoFiscalPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ObterDocumentoFiscalPorIdQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterDocumentoFiscalPorIdQuery request, CancellationToken cancellationToken)
        {
            var doc = await _context.DocumentosFiscais
                .AsNoTracking()
                .Include(d => d.Itens)
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

            if (doc == null)
            {
                return CommandResult.Falha("Documento fiscal não localizado.");
            }

            return CommandResult.Ok("OK", new
            {
                id = doc.Id,
                modelo = doc.Modelo == "55" ? "NFe" : "NFCe",
                status = doc.Status,
                chaveAcesso = doc.ChaveAcesso,
                protocolo = doc.Protocolo,
                urlDanfe = doc.PdfCaminho,
                urlXml = doc.XmlCaminho,
                emContingencia = false,
                autorizadoEm = doc.DataAutorizacao
            });
        }
    }

    public class ObterDocumentoFiscalPorChaveQueryHandler : IRequestHandler<ObterDocumentoFiscalPorChaveQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ObterDocumentoFiscalPorChaveQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterDocumentoFiscalPorChaveQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Chave) || request.Chave.Length != 44)
            {
                return CommandResult.Falha("A chave de acesso deve ter exatamente 44 dígitos.");
            }

            var doc = await _context.DocumentosFiscais
                .AsNoTracking()
                .Include(d => d.Itens)
                .FirstOrDefaultAsync(d => d.ChaveAcesso == request.Chave, cancellationToken);

            if (doc == null)
            {
                return CommandResult.Falha("Documento fiscal não localizado com a chave informada.");
            }

            return CommandResult.Ok("OK", new
            {
                id = doc.Id,
                modelo = doc.Modelo == "55" ? "NFe" : "NFCe",
                status = doc.Status,
                chaveAcesso = doc.ChaveAcesso,
                protocolo = doc.Protocolo,
                urlDanfe = doc.PdfCaminho,
                urlXml = doc.XmlCaminho,
                emContingencia = false,
                autorizadoEm = doc.DataAutorizacao
            });
        }
    }

    public class ObterXmlDocumentoFiscalQueryHandler : IRequestHandler<ObterXmlDocumentoFiscalQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ObterXmlDocumentoFiscalQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterXmlDocumentoFiscalQuery request, CancellationToken cancellationToken)
        {
            var doc = await _context.DocumentosFiscais
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

            if (doc == null || string.IsNullOrEmpty(doc.XmlRetorno))
            {
                return CommandResult.Falha("XML do documento fiscal não encontrado.");
            }

            return CommandResult.Ok("OK", doc.XmlRetorno);
        }
    }

    public class ObterXmlDocumentoFiscalPorChaveQueryHandler : IRequestHandler<ObterXmlDocumentoFiscalPorChaveQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ObterXmlDocumentoFiscalPorChaveQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterXmlDocumentoFiscalPorChaveQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Chave) || request.Chave.Length != 44)
            {
                return CommandResult.Falha("A chave de acesso deve ter exatamente 44 dígitos.");
            }

            var doc = await _context.DocumentosFiscais
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.ChaveAcesso == request.Chave, cancellationToken);

            if (doc == null || string.IsNullOrEmpty(doc.XmlRetorno))
            {
                return CommandResult.Falha("XML do documento fiscal não encontrado para a chave informada.");
            }

            return CommandResult.Ok("OK", doc.XmlRetorno);
        }
    }

    public class ObterDanfePdfQueryHandler : IRequestHandler<ObterDanfePdfQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        private readonly IDanfeService _danfeService;
        private readonly IArmazenamentoArquivoFiscal _armazenamento;

        public ObterDanfePdfQueryHandler(
            ContextFiscal context,
            IDanfeService danfeService,
            IArmazenamentoArquivoFiscal armazenamento)
        {
            _context = context;
            _danfeService = danfeService;
            _armazenamento = armazenamento;
        }

        public async Task<CommandResult> Handle(ObterDanfePdfQuery request, CancellationToken cancellationToken)
        {
            var doc = await _context.DocumentosFiscais
                .AsNoTracking()
                .Include(d => d.Itens)
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

            if (doc == null)
                return CommandResult.Falha("Documento fiscal não localizado.");

            // Se já houver PDF persistido, tenta reaproveitar; senão, gera sob demanda.
            byte[]? pdf = null;
            if (!string.IsNullOrWhiteSpace(doc.PdfCaminho))
                pdf = await _armazenamento.LerAsync(doc.PdfCaminho, cancellationToken);

            pdf ??= _danfeService.GerarPdf(doc);

            var nome = (string.IsNullOrWhiteSpace(doc.ChaveAcesso) ? doc.Id.ToString() : doc.ChaveAcesso) + ".pdf";
            return CommandResult.Ok("OK", new DanfePdfDto(pdf, nome));
        }
    }
}
