using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Producao.Application.Queries;
using Epros.Modules.Producao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Handlers
{
    public class ObterOrdensProducaoQueryHandler : IQueryHandler<ObterOrdensProducaoQuery, CommandResult>
    {
        private readonly ContextProducao _context;

        public ObterOrdensProducaoQueryHandler(ContextProducao context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterOrdensProducaoQuery request, CancellationToken cancellationToken)
        {
            var ordens = await _context.OrdensProducao
                .Include(o => o.Apontamentos)
                .OrderByDescending(o => o.DataAbertura)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Ordens de produção listadas com sucesso!", ordens);
        }
    }

    public class ObterListaMateriaisQueryHandler : IQueryHandler<ObterListaMateriaisQuery, CommandResult>
    {
        private readonly ContextProducao _context;

        public ObterListaMateriaisQueryHandler(ContextProducao context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterListaMateriaisQuery request, CancellationToken cancellationToken)
        {
            var lista = await _context.ListasMateriais
                .Include(l => l.Itens)
                .FirstOrDefaultAsync(l => l.ProdutoAcabadoSku == request.ProdutoAcabadoSku && l.Ativa, cancellationToken);

            if (lista == null)
            {
                return CommandResult.Falha($"Não foi encontrada nenhuma ficha técnica (BOM) ativa para o SKU '{request.ProdutoAcabadoSku}'.");
            }

            return CommandResult.Ok("Lista de materiais obtida com sucesso!", lista);
        }
    }
}
