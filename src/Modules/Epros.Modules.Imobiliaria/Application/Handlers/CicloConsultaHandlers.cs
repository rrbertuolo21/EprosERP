using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Imobiliaria.Application.Queries;
using Epros.Modules.Imobiliaria.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Imobiliaria.Application.Handlers
{
    public class ListarCobrancasLocacaoQueryHandler : IQueryHandler<ListarCobrancasLocacaoQuery, CommandResult>
    {
        private readonly ContextImobiliaria _context;
        public ListarCobrancasLocacaoQueryHandler(ContextImobiliaria context) => _context = context;

        public async Task<CommandResult> Handle(ListarCobrancasLocacaoQuery request, CancellationToken cancellationToken)
        {
            var cobrancas = await _context.CobrancasAluguel.AsNoTracking()
                .Where(c => c.LocacaoId == request.LocacaoId)
                .OrderByDescending(c => c.Competencia)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Cobrancas listadas com sucesso!", cobrancas);
        }
    }

    public class ListarGarantiasLocacaoQueryHandler : IQueryHandler<ListarGarantiasLocacaoQuery, CommandResult>
    {
        private readonly ContextImobiliaria _context;
        public ListarGarantiasLocacaoQueryHandler(ContextImobiliaria context) => _context = context;

        public async Task<CommandResult> Handle(ListarGarantiasLocacaoQuery request, CancellationToken cancellationToken)
        {
            var garantias = await _context.LocacaoGarantias.AsNoTracking()
                .Where(g => g.LocacaoId == request.LocacaoId)
                .OrderByDescending(g => g.CriadoEm)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Garantias listadas com sucesso!", garantias);
        }
    }

    public class ListarReajustesLocacaoQueryHandler : IQueryHandler<ListarReajustesLocacaoQuery, CommandResult>
    {
        private readonly ContextImobiliaria _context;
        public ListarReajustesLocacaoQueryHandler(ContextImobiliaria context) => _context = context;

        public async Task<CommandResult> Handle(ListarReajustesLocacaoQuery request, CancellationToken cancellationToken)
        {
            var reajustes = await _context.LocacaoReajustes.AsNoTracking()
                .Where(r => r.LocacaoId == request.LocacaoId)
                .OrderByDescending(r => r.DataBase)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Reajustes listados com sucesso!", reajustes);
        }
    }

    public class ListarPropostasQueryHandler : IQueryHandler<ListarPropostasQuery, CommandResult>
    {
        private readonly ContextImobiliaria _context;
        public ListarPropostasQueryHandler(ContextImobiliaria context) => _context = context;

        public async Task<CommandResult> Handle(ListarPropostasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Propostas.AsNoTracking().AsQueryable();
            if (request.ImovelId.HasValue)
                query = query.Where(p => p.ImovelId == request.ImovelId.Value);

            var propostas = await query.OrderByDescending(p => p.CriadoEm).ToListAsync(cancellationToken);
            return CommandResult.Ok("Propostas listadas com sucesso!", propostas);
        }
    }

    public class ObterPropostaQueryHandler : IQueryHandler<ObterPropostaQuery, CommandResult>
    {
        private readonly ContextImobiliaria _context;
        public ObterPropostaQueryHandler(ContextImobiliaria context) => _context = context;

        public async Task<CommandResult> Handle(ObterPropostaQuery request, CancellationToken cancellationToken)
        {
            var proposta = await _context.Propostas.AsNoTracking()
                .Include(p => p.Partes)
                .FirstOrDefaultAsync(p => p.Id == request.PropostaId, cancellationToken);
            if (proposta is null)
                return CommandResult.Falha("Proposta nao encontrada.");
            return CommandResult.Ok("Proposta recuperada com sucesso!", proposta);
        }
    }
}
