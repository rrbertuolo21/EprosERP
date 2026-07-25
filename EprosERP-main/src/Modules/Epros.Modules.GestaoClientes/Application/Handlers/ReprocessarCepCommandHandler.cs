using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using MediatR;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Reprocessa Cep.</summary>
    public class ReprocessarCepCommandHandler : ICommandHandler<ReprocessarCepCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly IMediator _mediator;

        public ReprocessarCepCommandHandler(ContextGestaoClientes context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<CommandResult> Handle(ReprocessarCepCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Cep))
                return CommandResult.Falha(new[] { "O CEP é obrigatório." }, "Erro de validação");

            var cepNormalizado = request.Cep.Replace("-", "").Replace(" ", "").Trim();

            // 1. Obter Brasil
            var brasil = await _context.Paises.FirstOrDefaultAsync(p => p.CodigoIsoAlpha2 == "BR", cancellationToken);
            if (brasil == null)
                return CommandResult.Falha(new[] { "País Brasil não cadastrado." }, "Erro de configuração");

            // 2. Localizar no cache (incluindo falhas)
            var cache = await _context.CodigosPostaisCache
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.PaisId == brasil.Id && c.CodigoPostal == cepNormalizado, cancellationToken);

            if (cache != null)
            {
                // Remove a entrada com falha (ou sucesso) para limpar o cache e reprocessar
                _context.CodigosPostaisCache.Remove(cache);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 3. Chamar a Query de consulta de CEP para disparar nova busca oficial e recriar o cache
            var resultado = await _mediator.Send(new ConsultarCepQuery(cepNormalizado), cancellationToken);

            if (resultado == null)
            {
                return CommandResult.Falha(new[] { "A busca externa continuou falhando para este CEP." }, "Erro de reprocessamento");
            }

            return CommandResult.Ok("CEP reprocessado e gravado em cache com sucesso!", resultado);
        }
    }
}
