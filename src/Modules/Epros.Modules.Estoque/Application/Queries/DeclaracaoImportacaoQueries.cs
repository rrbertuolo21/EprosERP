using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    /// <summary>
    /// Consulta das Declarações de Importação (DI) de um item de compra, com suas adições
    /// (CD1 / EF COMERCIO_EXTERIOR, CEX-001..023). Tenant + soft-delete pelo filtro global.
    /// </summary>
    public record ObterDeclaracoesImportacaoPorItemQuery(Guid CompraItemId) : IRequest<CommandResult>;

    public class ObterDeclaracoesImportacaoPorItemQueryHandler : IRequestHandler<ObterDeclaracoesImportacaoPorItemQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ObterDeclaracoesImportacaoPorItemQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterDeclaracoesImportacaoPorItemQuery request, CancellationToken cancellationToken)
        {
            var declaracoes = await _context.CompraItemImportacoes.AsNoTracking()
                .Where(d => d.CompraItemId == request.CompraItemId)
                .Select(d => new
                {
                    d.Id,
                    d.CompraItemId,
                    d.NumeroDeclaracaoImportacao,
                    d.DataDeclaracaoImportacao,
                    d.LocalDesembaraco,
                    d.UfDesembaraco,
                    d.DataDesembaraco,
                    d.TipoViaTransporte,
                    d.ValorAFRMM,          // valida-contador (factual)
                    d.TipoIntermedio,
                    d.Cnpj,
                    d.Cpf,
                    d.UfTerceiro,
                    d.CodigoExportador,
                    Adicoes = d.Adicoes.Select(a => new
                    {
                        a.Id,
                        a.NumeroAdicao,
                        a.NumeroSequencialAdicao,
                        a.CodigoFabricante,
                        a.ValorDesconto,   // valida-contador (factual)
                        a.NumeroAtoConcessorio
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", declaracoes);
        }
    }
}
