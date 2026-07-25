using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GRC.Application.Queries;
using Epros.Modules.GRC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Handlers
{
    public class ObterCategoriasDenunciaQueryHandler : IQueryHandler<ObterCategoriasDenunciaQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterCategoriasDenunciaQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterCategoriasDenunciaQuery request, CancellationToken cancellationToken)
        {
            var categorias = await _context.DenunciaCategorias.OrderBy(c => c.Nome).ToListAsync(cancellationToken);
            return CommandResult.Ok("Categorias listadas com sucesso!", categorias);
        }
    }

    public class ObterDenunciaDetalheQueryHandler : IQueryHandler<ObterDenunciaDetalheQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterDenunciaDetalheQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterDenunciaDetalheQuery request, CancellationToken cancellationToken)
        {
            var denuncia = await _context.Denuncias.FirstOrDefaultAsync(d => d.Id == request.DenunciaId, cancellationToken);
            if (denuncia == null)
            {
                return CommandResult.Falha("Denúncia não encontrada.");
            }
            return CommandResult.Ok("Denúncia obtida com sucesso!", denuncia);
        }
    }

    public class ObterRespostasDenunciaQueryHandler : IQueryHandler<ObterRespostasDenunciaQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterRespostasDenunciaQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterRespostasDenunciaQuery request, CancellationToken cancellationToken)
        {
            // RN-DEN-005: quando IncluirInternas=false, respostas internas ficam ocultas.
            var query = _context.DenunciaRespostas.Where(r => r.DenunciaId == request.DenunciaId);
            if (!request.IncluirInternas)
            {
                query = query.Where(r => !r.Interna);
            }
            var respostas = await query.OrderBy(r => r.DataCriacao).ToListAsync(cancellationToken);
            return CommandResult.Ok("Respostas listadas com sucesso!", respostas);
        }
    }

    public class ObterParticipantesDenunciaQueryHandler : IQueryHandler<ObterParticipantesDenunciaQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterParticipantesDenunciaQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterParticipantesDenunciaQuery request, CancellationToken cancellationToken)
        {
            var participantes = await _context.DenunciaParticipantes
                .Where(p => p.DenunciaId == request.DenunciaId)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Participantes listados com sucesso!", participantes);
        }
    }

    public class ObterInvestigacoesDenunciaQueryHandler : IQueryHandler<ObterInvestigacoesDenunciaQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterInvestigacoesDenunciaQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterInvestigacoesDenunciaQuery request, CancellationToken cancellationToken)
        {
            var investigacoes = await _context.DenunciaInvestigacoes
                .Where(i => i.DenunciaId == request.DenunciaId)
                .OrderByDescending(i => i.DataInicio)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Investigações listadas com sucesso!", investigacoes);
        }
    }

    public class ObterParametrosDenunciaQueryHandler : IQueryHandler<ObterParametrosDenunciaQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterParametrosDenunciaQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterParametrosDenunciaQuery request, CancellationToken cancellationToken)
        {
            var parametros = await _context.DenunciaParametros.OrderBy(p => p.Chave).ToListAsync(cancellationToken);
            return CommandResult.Ok("Parâmetros listados com sucesso!", parametros);
        }
    }
}
