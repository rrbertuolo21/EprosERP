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
    /// <summary>Consultas do submódulo Comércio Exterior (CD1). Tenant + soft-delete pelo filtro global.</summary>
    public record ObterRateioLandedConfigQuery(Guid? EmpresaId = null) : IRequest<CommandResult>;

    public class ObterRateioLandedConfigQueryHandler : IRequestHandler<ObterRateioLandedConfigQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ObterRateioLandedConfigQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterRateioLandedConfigQuery request, CancellationToken cancellationToken)
        {
            var config = await _context.ComprasImportacaoRateioConfigs.AsNoTracking()
                .FirstOrDefaultAsync(c => c.EmpresaId == request.EmpresaId, cancellationToken);

            // Sem config persistida → devolve o padrão SEGURO (desligado), sem materializar (NF-02).
            if (config == null)
                return CommandResult.Ok("OK", new
                {
                    EmpresaId = request.EmpresaId,
                    Habilitado = false,
                    IncluirTributos = false,
                    IncluirFrete = true,
                    IncluirDespesas = false,
                    Metodo = Domain.Enums.ERateioLandedMetodo.PorValor,
                    Padrao = true
                });

            return CommandResult.Ok("OK", new
            {
                config.Id,
                config.EmpresaId,
                config.Habilitado,
                config.IncluirTributos,
                config.IncluirFrete,
                config.IncluirDespesas,
                config.Metodo,
                Padrao = false
            });
        }
    }
}
