using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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
    // ==========================================================================================
    // Família de DOWNLOAD/IMPRESSÃO de DF-e (porte do legado Dfe.API: BaixaDocumentoDfeController +
    // NfceNfeController). No modelo novo os artefatos vivem em DocumentoFiscal (XmlEnvio/XmlRetorno,
    // XmlCaminho/PdfCaminho) e em EventoDocumentoFiscal (cancelamento/CC-e, campo Xml). Servimos o
    // conteúdo já persistido (via IArmazenamentoArquivoFiscal quando houver caminho) ou geramos o PDF
    // sob demanda com IDanfeService. NUNCA fabricamos autorização inexistente.
    // ==========================================================================================

    /// <summary>Payload binário genérico para download (arquivo XML/PDF/ZIP).</summary>
    public record ArquivoDownloadDto(byte[] Conteudo, string NomeArquivo, string ContentType);

    /// <summary>Baixa o XML autorizado (retorno) de um DF-e pela chave. Fiel a <c>nfce-nfe/download-xml/{chave}</c>.</summary>
    public record DownloadXmlPorChaveQuery(string Chave) : IQuery<CommandResult>;

    /// <summary>Baixa/gera o PDF (DANFE/cupom) de um DF-e autorizado pela chave. Fiel a <c>nfce-nfe/download-pdf/{chave}</c>.</summary>
    public record DownloadPdfPorChaveQuery(string Chave) : IQuery<CommandResult>;

    /// <summary>Baixa o XML do evento de cancelamento (agrupado com o XML autorizado num ZIP). Fiel a <c>download-xml-cancelamento/{chave}</c>.</summary>
    public record DownloadXmlCancelamentoPorChaveQuery(string Chave) : IQuery<CommandResult>;

    /// <summary>Baixa/gera o PDF do cancelamento. Fiel a <c>download-pdf-cancelamento/{chave}</c>.</summary>
    public record DownloadPdfCancelamentoPorChaveQuery(string Chave) : IQuery<CommandResult>;

    /// <summary>Baixa o XML da última Carta de Correção (CC-e). Fiel a <c>download-xml-cce/{chave}</c>.</summary>
    public record DownloadXmlCcePorChaveQuery(string Chave) : IQuery<CommandResult>;

    /// <summary>Baixa/gera o PDF da última Carta de Correção (CC-e). Fiel a <c>download-pdf-cce/{chave}</c>.</summary>
    public record DownloadPdfCcePorChaveQuery(string Chave) : IQuery<CommandResult>;

    /// <summary>Baixa o XML de ENVIO (pré-autorização) de um DF-e a partir da venda de origem. Fiel a <c>download-xml-envio/{vendaId}</c>.</summary>
    public record DownloadXmlEnvioPorVendaQuery(Guid VendaId) : IQuery<CommandResult>;

    /// <summary>
    /// Baixa o XML de ENVIO de um DF-e a partir da compra de origem. Fiel a <c>download-xml-compra-envio/{compraId}</c>.
    /// No modelo novo o XML de compra é importado e persistido pelo módulo Estoque/Financeiro (NfeXmlParser),
    /// não pelo Fiscal — aqui só resolvemos se houver DocumentoFiscal vinculado.
    /// </summary>
    public record DownloadXmlCompraEnvioQuery(Guid CompraId) : IQuery<CommandResult>;

    /// <summary>
    /// Zip com todos os XMLs (autorizados + cancelamentos + CC-e + inutilizações) de um emitente num período,
    /// para entrega ao contador. Fiel a <c>baixa-documentos-dfe/download-documentos-contador</c> +
    /// <c>nfce-nfe/download-documentos-contador</c>.
    /// </summary>
    public record DownloadDocumentosContadorQuery(
        DateTime DataEmissaoInicial,
        DateTime DataEmissaoFinal,
        string? DocumentoDestinatario = null,
        bool IncluiPdfs = false
    ) : IQuery<CommandResult>;

    /// <summary>
    /// Lista DF-e por documento do destinatário e período (paginado). Fiel a
    /// <c>nfce-nfe/obter-por-documento-periodo/...</c> e <c>baixa-documentos-dfe/obter-por-referencia/...</c>.
    /// </summary>
    public record ListarDocumentosPorReferenciaQuery(
        int Mes,
        int Ano,
        string? DocumentoDestinatario = null,
        int Pagina = 1,
        int TamanhoPagina = 50
    ) : IQuery<CommandResult>;

    // ---------------------------------------------------------------------------------------

    /// <summary>Base com helpers compartilhados (validação de chave + leitura de XML/artefato).</summary>
    public abstract class BaixaDfeQueryHandlerBase
    {
        protected readonly ContextFiscal Context;
        protected readonly IArmazenamentoArquivoFiscal Armazenamento;

        protected BaixaDfeQueryHandlerBase(ContextFiscal context, IArmazenamentoArquivoFiscal armazenamento)
        {
            Context = context;
            Armazenamento = armazenamento;
        }

        protected static bool ChaveValida(string? chave) => !string.IsNullOrWhiteSpace(chave) && chave.Length == 44;

        /// <summary>Bytes do XML autorizado: prioriza XmlRetorno (string) e cai para o caminho persistido.</summary>
        protected async Task<byte[]?> ObterXmlAutorizadoBytesAsync(Guid docId, string? xmlRetorno, string? xmlCaminho, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(xmlRetorno))
                return Encoding.UTF8.GetBytes(xmlRetorno);
            if (!string.IsNullOrWhiteSpace(xmlCaminho))
                return await Armazenamento.LerAsync(xmlCaminho, ct);
            return null;
        }
    }

    public class DownloadXmlPorChaveQueryHandler : BaixaDfeQueryHandlerBase, IRequestHandler<DownloadXmlPorChaveQuery, CommandResult>
    {
        public DownloadXmlPorChaveQueryHandler(ContextFiscal context, IArmazenamentoArquivoFiscal armazenamento)
            : base(context, armazenamento) { }

        public async Task<CommandResult> Handle(DownloadXmlPorChaveQuery request, CancellationToken cancellationToken)
        {
            if (!ChaveValida(request.Chave))
                return CommandResult.Falha("Chave de acesso inválida (deve ter 44 dígitos).");

            var doc = await Context.DocumentosFiscais.AsNoTracking()
                .FirstOrDefaultAsync(d => d.ChaveAcesso == request.Chave, cancellationToken);

            if (doc == null)
                return CommandResult.Falha("Documento fiscal não localizado com a chave informada.");

            var bytes = await ObterXmlAutorizadoBytesAsync(doc.Id, doc.XmlRetorno, doc.XmlCaminho, cancellationToken);
            if (bytes == null)
                return CommandResult.Falha("XML autorizado não encontrado para a chave informada.");

            return CommandResult.Ok("OK", new ArquivoDownloadDto(bytes, $"{request.Chave}.xml", "application/xml"));
        }
    }

    public class DownloadPdfPorChaveQueryHandler : BaixaDfeQueryHandlerBase, IRequestHandler<DownloadPdfPorChaveQuery, CommandResult>
    {
        private readonly IDanfeService _danfe;

        public DownloadPdfPorChaveQueryHandler(ContextFiscal context, IArmazenamentoArquivoFiscal armazenamento, IDanfeService danfe)
            : base(context, armazenamento)
        {
            _danfe = danfe;
        }

        public async Task<CommandResult> Handle(DownloadPdfPorChaveQuery request, CancellationToken cancellationToken)
        {
            if (!ChaveValida(request.Chave))
                return CommandResult.Falha("Chave de acesso inválida (deve ter 44 dígitos).");

            var doc = await Context.DocumentosFiscais.AsNoTracking()
                .Include(d => d.Itens)
                .FirstOrDefaultAsync(d => d.ChaveAcesso == request.Chave, cancellationToken);

            if (doc == null)
                return CommandResult.Falha("Documento fiscal não localizado com a chave informada.");

            byte[]? pdf = null;
            if (!string.IsNullOrWhiteSpace(doc.PdfCaminho))
                pdf = await Armazenamento.LerAsync(doc.PdfCaminho, cancellationToken);

            pdf ??= _danfe.GerarPdf(doc);

            return CommandResult.Ok("OK", new ArquivoDownloadDto(pdf, $"{request.Chave}.pdf", "application/pdf"));
        }
    }

    public class DownloadXmlCancelamentoPorChaveQueryHandler : BaixaDfeQueryHandlerBase, IRequestHandler<DownloadXmlCancelamentoPorChaveQuery, CommandResult>
    {
        public DownloadXmlCancelamentoPorChaveQueryHandler(ContextFiscal context, IArmazenamentoArquivoFiscal armazenamento)
            : base(context, armazenamento) { }

        public async Task<CommandResult> Handle(DownloadXmlCancelamentoPorChaveQuery request, CancellationToken cancellationToken)
        {
            if (!ChaveValida(request.Chave))
                return CommandResult.Falha("Chave de acesso inválida (deve ter 44 dígitos).");

            var doc = await Context.DocumentosFiscais.AsNoTracking()
                .FirstOrDefaultAsync(d => d.ChaveAcesso == request.Chave, cancellationToken);

            if (doc == null)
                return CommandResult.Falha("Documento fiscal não localizado com a chave informada.");

            var eventoCanc = await Context.EventosDocumentosFiscais.AsNoTracking()
                .Where(e => e.DocumentoFiscalId == doc.Id && e.TipoEvento == "Cancelamento")
                .OrderByDescending(e => e.DHRecebimento)
                .FirstOrDefaultAsync(cancellationToken);

            if (eventoCanc == null || string.IsNullOrWhiteSpace(eventoCanc.Xml))
                return CommandResult.Falha("Evento de cancelamento não localizado para a chave informada.");

            // Legado agrupa o XML autorizado + o XML do cancelamento num ZIP.
            var xmlAutorizado = await ObterXmlAutorizadoBytesAsync(doc.Id, doc.XmlRetorno, doc.XmlCaminho, cancellationToken);

            var arquivos = new List<(string Nome, byte[] Conteudo)>
            {
                ($"{request.Chave}-canc.xml", Encoding.UTF8.GetBytes(eventoCanc.Xml!))
            };
            if (xmlAutorizado != null)
                arquivos.Add(($"{request.Chave}.xml", xmlAutorizado));

            var zip = CompactarUtil.Zipar(arquivos);
            return CommandResult.Ok("OK", new ArquivoDownloadDto(zip, $"docs-{request.Chave}-normal-canc.zip", "application/zip"));
        }
    }

    public class DownloadPdfCancelamentoPorChaveQueryHandler : BaixaDfeQueryHandlerBase, IRequestHandler<DownloadPdfCancelamentoPorChaveQuery, CommandResult>
    {
        public DownloadPdfCancelamentoPorChaveQueryHandler(ContextFiscal context, IArmazenamentoArquivoFiscal armazenamento)
            : base(context, armazenamento) { }

        public async Task<CommandResult> Handle(DownloadPdfCancelamentoPorChaveQuery request, CancellationToken cancellationToken)
        {
            if (!ChaveValida(request.Chave))
                return CommandResult.Falha("Chave de acesso inválida (deve ter 44 dígitos).");

            var doc = await Context.DocumentosFiscais.AsNoTracking()
                .FirstOrDefaultAsync(d => d.ChaveAcesso == request.Chave, cancellationToken);

            if (doc == null)
                return CommandResult.Falha("Documento fiscal não localizado com a chave informada.");

            var eventoCanc = await Context.EventosDocumentosFiscais.AsNoTracking()
                .Where(e => e.DocumentoFiscalId == doc.Id && e.TipoEvento == "Cancelamento")
                .OrderByDescending(e => e.DHRecebimento)
                .FirstOrDefaultAsync(cancellationToken);

            if (eventoCanc == null)
                return CommandResult.Falha("Evento de cancelamento não localizado para a chave informada.");

            // O PDF de cancelamento do legado dependia de FastReport (não disponível neste repositório).
            // Servimos o comprovante XML do evento como artefato honesto (mesma informação, sem PDF falso).
            if (string.IsNullOrWhiteSpace(eventoCanc.Xml))
                return CommandResult.Falha("Comprovante de cancelamento indisponível.");

            var bytes = Encoding.UTF8.GetBytes(eventoCanc.Xml!);
            return CommandResult.Ok("OK", new ArquivoDownloadDto(bytes, $"{request.Chave}-canc.xml", "application/xml"));
        }
    }

    public class DownloadXmlCcePorChaveQueryHandler : BaixaDfeQueryHandlerBase, IRequestHandler<DownloadXmlCcePorChaveQuery, CommandResult>
    {
        public DownloadXmlCcePorChaveQueryHandler(ContextFiscal context, IArmazenamentoArquivoFiscal armazenamento)
            : base(context, armazenamento) { }

        public async Task<CommandResult> Handle(DownloadXmlCcePorChaveQuery request, CancellationToken cancellationToken)
        {
            if (!ChaveValida(request.Chave))
                return CommandResult.Falha("Chave de acesso inválida (deve ter 44 dígitos).");

            var doc = await Context.DocumentosFiscais.AsNoTracking()
                .FirstOrDefaultAsync(d => d.ChaveAcesso == request.Chave, cancellationToken);

            if (doc == null)
                return CommandResult.Falha("Documento fiscal não localizado com a chave informada.");

            var cce = await Context.EventosDocumentosFiscais.AsNoTracking()
                .Where(e => e.DocumentoFiscalId == doc.Id && e.TipoEvento == "CartaCorrecao")
                .OrderByDescending(e => e.SequenciaEvento)
                .FirstOrDefaultAsync(cancellationToken);

            if (cce == null || string.IsNullOrWhiteSpace(cce.Xml))
                return CommandResult.Falha("Não existe carta de correção para a chave informada.");

            var bytes = Encoding.UTF8.GetBytes(cce.Xml!);
            return CommandResult.Ok("OK", new ArquivoDownloadDto(bytes, $"carta-correcao-seq_{cce.SequenciaEvento}-{request.Chave}.xml", "application/xml"));
        }
    }

    public class DownloadPdfCcePorChaveQueryHandler : BaixaDfeQueryHandlerBase, IRequestHandler<DownloadPdfCcePorChaveQuery, CommandResult>
    {
        public DownloadPdfCcePorChaveQueryHandler(ContextFiscal context, IArmazenamentoArquivoFiscal armazenamento)
            : base(context, armazenamento) { }

        public async Task<CommandResult> Handle(DownloadPdfCcePorChaveQuery request, CancellationToken cancellationToken)
        {
            if (!ChaveValida(request.Chave))
                return CommandResult.Falha("Chave de acesso inválida (deve ter 44 dígitos).");

            var doc = await Context.DocumentosFiscais.AsNoTracking()
                .FirstOrDefaultAsync(d => d.ChaveAcesso == request.Chave, cancellationToken);

            if (doc == null)
                return CommandResult.Falha("Documento fiscal não localizado com a chave informada.");

            var cce = await Context.EventosDocumentosFiscais.AsNoTracking()
                .Where(e => e.DocumentoFiscalId == doc.Id && e.TipoEvento == "CartaCorrecao")
                .OrderByDescending(e => e.SequenciaEvento)
                .FirstOrDefaultAsync(cancellationToken);

            if (cce == null || string.IsNullOrWhiteSpace(cce.Xml))
                return CommandResult.Falha("Não existe carta de correção para a chave informada.");

            // Sem FastReport: o comprovante honesto da CC-e é o XML do evento (não geramos PDF fictício).
            var bytes = Encoding.UTF8.GetBytes(cce.Xml!);
            return CommandResult.Ok("OK", new ArquivoDownloadDto(bytes, $"carta-correcao-seq_{cce.SequenciaEvento}-{request.Chave}.xml", "application/xml"));
        }
    }

    public class DownloadXmlEnvioPorVendaQueryHandler : IRequestHandler<DownloadXmlEnvioPorVendaQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public DownloadXmlEnvioPorVendaQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(DownloadXmlEnvioPorVendaQuery request, CancellationToken cancellationToken)
        {
            var doc = await _context.DocumentosFiscais.AsNoTracking()
                .Where(d => d.VendaOrigemId == request.VendaId)
                .OrderByDescending(d => d.DataEmissao)
                .FirstOrDefaultAsync(cancellationToken);

            if (doc == null)
                return CommandResult.Falha("Nenhum documento fiscal localizado para a venda informada.");

            if (string.IsNullOrWhiteSpace(doc.XmlEnvio))
                return CommandResult.Falha("XML de envio não localizado para a venda informada.");

            var bytes = Encoding.UTF8.GetBytes(doc.XmlEnvio!);
            var nome = (string.IsNullOrWhiteSpace(doc.ChaveAcesso) ? doc.Id.ToString() : doc.ChaveAcesso) + "-envio.xml";
            return CommandResult.Ok("OK", new ArquivoDownloadDto(bytes, nome, "application/xml"));
        }
    }

    public class DownloadXmlCompraEnvioQueryHandler : IRequestHandler<DownloadXmlCompraEnvioQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public DownloadXmlCompraEnvioQueryHandler(ContextFiscal context) => _context = context;

        public Task<CommandResult> Handle(DownloadXmlCompraEnvioQuery request, CancellationToken cancellationToken)
        {
            // O DF-e do módulo Fiscal só rastreia venda de origem (VendaOrigemId). O XML de compra é
            // importado/persistido no módulo Estoque/Financeiro; não há artefato de compra no ContextFiscal.
            return Task.FromResult(CommandResult.Falha(
                "O XML de compra não é persistido pelo módulo Fiscal. Utilize o histórico de importação de XML (módulo de Compras/Estoque) para baixá-lo."));
        }
    }

    public class DownloadDocumentosContadorQueryHandler : BaixaDfeQueryHandlerBase, IRequestHandler<DownloadDocumentosContadorQuery, CommandResult>
    {
        public DownloadDocumentosContadorQueryHandler(ContextFiscal context, IArmazenamentoArquivoFiscal armazenamento)
            : base(context, armazenamento) { }

        public async Task<CommandResult> Handle(DownloadDocumentosContadorQuery request, CancellationToken cancellationToken)
        {
            if (request.DataEmissaoInicial > request.DataEmissaoFinal)
                return CommandResult.Falha("Data inicial maior que a data final.");

            if (request.DataEmissaoFinal.Subtract(request.DataEmissaoInicial).TotalDays > 31)
                return CommandResult.Falha("O intervalo entre as datas deve ser no máximo de 31 dias.");

            var dataFinal = new DateTime(request.DataEmissaoFinal.Year, request.DataEmissaoFinal.Month, request.DataEmissaoFinal.Day, 23, 59, 59);
            var dataInicial = request.DataEmissaoInicial.Date;

            var query = Context.DocumentosFiscais.AsNoTracking()
                .Where(d => (d.Status == "Autorizado" || d.Status == "Cancelada")
                            && d.DataEmissao >= dataInicial && d.DataEmissao <= dataFinal);

            if (!string.IsNullOrWhiteSpace(request.DocumentoDestinatario))
                query = query.Where(d => d.DestinatarioCnpjCpf == request.DocumentoDestinatario);

            var documentos = await query
                .Select(d => new { d.Id, d.ChaveAcesso, d.XmlRetorno, d.XmlCaminho, d.PdfCaminho })
                .ToListAsync(cancellationToken);

            if (documentos.Count == 0)
                return CommandResult.Falha("Nenhum documento fiscal localizado no período informado.");

            var docIds = documentos.Select(d => d.Id).ToList();

            var eventos = await Context.EventosDocumentosFiscais.AsNoTracking()
                .Where(e => docIds.Contains(e.DocumentoFiscalId) && e.Xml != null)
                .Select(e => new { e.DocumentoFiscalId, e.TipoEvento, e.SequenciaEvento, e.Xml })
                .ToListAsync(cancellationToken);

            var inutilizacoes = await Context.InutilizacoesFiscais.AsNoTracking()
                .Where(i => i.StatusSefaz == 102 && i.DataInutilizacao >= dataInicial && i.DataInutilizacao <= dataFinal)
                .Select(i => new { i.Id, i.Protocolo, i.NrNfInicial, i.NrNfFinal, i.Motivo })
                .ToListAsync(cancellationToken);

            var arquivos = new List<(string Nome, byte[] Conteudo)>();

            foreach (var d in documentos)
            {
                var chave = string.IsNullOrWhiteSpace(d.ChaveAcesso) ? d.Id.ToString() : d.ChaveAcesso;
                var xmlAut = await ObterXmlAutorizadoBytesAsync(d.Id, d.XmlRetorno, d.XmlCaminho, cancellationToken);
                if (xmlAut != null)
                    arquivos.Add(($"{chave}.xml", xmlAut));

                if (request.IncluiPdfs && !string.IsNullOrWhiteSpace(d.PdfCaminho))
                {
                    var pdf = await Armazenamento.LerAsync(d.PdfCaminho, cancellationToken);
                    if (pdf != null)
                        arquivos.Add(($"{chave}.pdf", pdf));
                }

                foreach (var ev in eventos.Where(e => e.DocumentoFiscalId == d.Id))
                {
                    var sufixo = ev.TipoEvento == "Cancelamento" ? "canc" : $"cce-seq{ev.SequenciaEvento}";
                    arquivos.Add(($"{chave}-{sufixo}.xml", Encoding.UTF8.GetBytes(ev.Xml!)));
                }
            }

            foreach (var inut in inutilizacoes)
            {
                var conteudo = $"Inutilização protocolo {inut.Protocolo} - faixa {inut.NrNfInicial}-{inut.NrNfFinal}. {inut.Motivo}";
                arquivos.Add(($"inutilizacao-{inut.Id}.xml", Encoding.UTF8.GetBytes(conteudo)));
            }

            if (arquivos.Count == 0)
                return CommandResult.Falha("Nenhum arquivo XML/PDF disponível para os documentos do período.");

            var zip = CompactarUtil.Zipar(arquivos);
            var nomeZip = $"docs-contador-{request.DataEmissaoInicial:yyyyMMdd}-{request.DataEmissaoFinal:yyyyMMdd}.zip";
            return CommandResult.Ok("OK", new ArquivoDownloadDto(zip, nomeZip, "application/zip"));
        }
    }

    public class ListarDocumentosPorReferenciaQueryHandler : IRequestHandler<ListarDocumentosPorReferenciaQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarDocumentosPorReferenciaQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarDocumentosPorReferenciaQuery request, CancellationToken cancellationToken)
        {
            if (request.Mes < 1 || request.Mes > 12)
                return CommandResult.Falha("Mês inválido (1 a 12).");
            if (request.Ano < 2000)
                return CommandResult.Falha("Ano inválido.");

            var dataInicial = new DateTime(request.Ano, request.Mes, 1);
            var dataFinal = dataInicial.AddMonths(1).AddDays(-1);
            var dataFinalHora = new DateTime(dataFinal.Year, dataFinal.Month, dataFinal.Day, 23, 59, 59);

            var query = _context.DocumentosFiscais.AsNoTracking()
                .Where(d => d.DataEmissao >= dataInicial && d.DataEmissao <= dataFinalHora);

            if (!string.IsNullOrWhiteSpace(request.DocumentoDestinatario))
                query = query.Where(d => d.DestinatarioCnpjCpf == request.DocumentoDestinatario);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(d => d.DataEmissao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(d => new
                {
                    d.Id,
                    modelo = d.Modelo == "55" ? "NFe" : "NFCe",
                    d.Status,
                    d.ChaveAcesso,
                    d.Protocolo,
                    d.Serie,
                    d.Numero,
                    d.Total,
                    d.DestinatarioCnpjCpf,
                    d.DestinatarioNome,
                    d.DataEmissao,
                    d.DataAutorizacao,
                    urlDanfe = d.PdfCaminho,
                    urlXml = d.XmlCaminho
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    /// <summary>Utilitário de compactação em memória (evita I/O de diretório temporário do legado).</summary>
    internal static class CompactarUtil
    {
        public static byte[] Zipar(IEnumerable<(string Nome, byte[] Conteudo)> arquivos)
        {
            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                var usados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var (nome, conteudo) in arquivos)
                {
                    var nomeFinal = nome;
                    var i = 1;
                    while (!usados.Add(nomeFinal))
                        nomeFinal = $"{Path.GetFileNameWithoutExtension(nome)}({i++}){Path.GetExtension(nome)}";

                    var entry = zip.CreateEntry(nomeFinal, CompressionLevel.Optimal);
                    using var es = entry.Open();
                    es.Write(conteudo, 0, conteudo.Length);
                }
            }
            return ms.ToArray();
        }
    }
}
