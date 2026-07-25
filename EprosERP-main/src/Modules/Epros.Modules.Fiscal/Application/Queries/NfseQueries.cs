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
    /// <summary>Obtém a configuração do provedor NFS-e do município. Fiel ao legado <c>nfse/config</c>.</summary>
    public record ObterConfigNfseQuery(int CodigoMunicipioIbge) : IQuery<CommandResult>;

    /// <summary>Lista as NFS-e emitidas (histórico), paginado, com filtro opcional por status.</summary>
    public record ListarNotasServicoQuery(
        string? Status = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public class ObterConfigNfseQueryHandler : IRequestHandler<ObterConfigNfseQuery, CommandResult>
    {
        private readonly INfseFiscalService _nfseService;
        public ObterConfigNfseQueryHandler(INfseFiscalService nfseService) => _nfseService = nfseService;

        public Task<CommandResult> Handle(ObterConfigNfseQuery request, CancellationToken cancellationToken)
        {
            var cfg = _nfseService.ObterConfiguracao(request.CodigoMunicipioIbge);
            if (!cfg.Sucesso)
                return Task.FromResult(CommandResult.Falha(cfg.Motivo));

            return Task.FromResult(CommandResult.Ok("OK", new
            {
                cfg.Provedor,
                cfg.CodigoMunicipioIbge
            }));
        }
    }

    public class ListarNotasServicoQueryHandler : IRequestHandler<ListarNotasServicoQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarNotasServicoQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarNotasServicoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.NotasServicoEletronicas.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(n => n.Status == request.Status);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(n => n.DataEmissao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(n => new
                {
                    n.Id,
                    n.RpsNumero,
                    n.RpsSerie,
                    n.NumeroNfse,
                    n.CodigoVerificacao,
                    n.Status,
                    n.Protocolo,
                    n.PrestadorDocumento,
                    n.TomadorDocumento,
                    n.ValorServicos,
                    n.DataEmissao,
                    n.DataAutorizacao,
                    urlXml = n.XmlCaminho,
                    urlPdf = n.PdfCaminho
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }
}
