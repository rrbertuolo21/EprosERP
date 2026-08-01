using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Application.Documentos;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>1.08F — PDF da fatura. Reusa a projeção da <see cref="ObterFaturaPorIdQueryHandler"/>.</summary>
    public class ObterFaturaPdfQueryHandler : IQueryHandler<ObterFaturaPdfQuery, DocumentoRenderizado?>
    {
        private readonly ContextGestaoClientes _context;
        private readonly IDocumentoFinanceiroRenderer _renderer;

        public ObterFaturaPdfQueryHandler(ContextGestaoClientes context, IDocumentoFinanceiroRenderer renderer)
        {
            _context = context;
            _renderer = renderer;
        }

        public async Task<DocumentoRenderizado?> Handle(ObterFaturaPdfQuery request, CancellationToken cancellationToken)
        {
            var fatura = await new ObterFaturaPorIdQueryHandler(_context)
                .Handle(new ObterFaturaPorIdQuery(request.FaturaId), cancellationToken);
            if (fatura == null) return null;
            return _renderer.RenderFatura(fatura);
        }
    }

    /// <summary>1.08F — PDF do recibo mais recente da fatura. Reusa a projeção do recibo.</summary>
    public class ObterReciboPdfQueryHandler : IQueryHandler<ObterReciboPdfQuery, DocumentoRenderizado?>
    {
        private readonly ContextGestaoClientes _context;
        private readonly IDocumentoFinanceiroRenderer _renderer;

        public ObterReciboPdfQueryHandler(ContextGestaoClientes context, IDocumentoFinanceiroRenderer renderer)
        {
            _context = context;
            _renderer = renderer;
        }

        public async Task<DocumentoRenderizado?> Handle(ObterReciboPdfQuery request, CancellationToken cancellationToken)
        {
            var recibo = await new ObterReciboPorFaturaQueryHandler(_context)
                .Handle(new ObterReciboPorFaturaQuery(request.FaturaId), cancellationToken);
            if (recibo == null) return null;
            return _renderer.RenderRecibo(recibo);
        }
    }

    /// <summary>1.08F — Expõe a URL do PDF do boleto que o gateway já hospeda (não gera boleto do zero).</summary>
    public class ObterBoletoLinkQueryHandler : IQueryHandler<ObterBoletoLinkQuery, BoletoLinkDto?>
    {
        private readonly ContextGestaoClientes _context;

        public ObterBoletoLinkQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<BoletoLinkDto?> Handle(ObterBoletoLinkQuery request, CancellationToken cancellationToken)
        {
            var pagamento = await _context.PagamentosFaturas
                .Where(p => p.FaturaId == request.FaturaId && p.TipoPagamento == "Boleto")
                .OrderByDescending(p => p.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);

            if (pagamento == null || string.IsNullOrWhiteSpace(pagamento.UrlBoleto))
                return null;

            return new BoletoLinkDto(request.FaturaId, pagamento.UrlBoleto, pagamento.LinhaDigitavel);
        }
    }
}
