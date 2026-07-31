using Epros.Modules.Financeiro.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Painel operacional do TENANT (submódulo 1.03 DASHBOARD_E_LAYOUT).
    /// Base: /api/v1/dashboard
    ///
    /// Compõe indicadores financeiros-raiz (a receber, a pagar, receita/despesa do mês,
    /// saldo, série mensal e últimas transações) reusando os agregados fiéis do módulo
    /// Financeiro. TenantId/RLS aplicados pelo InquilinoSaaSMiddleware + QueryFilter.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DashboardController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Retorna o painel operacional financeiro do tenant: totais em aberto,
        /// receita/despesa do mês, saldo do caixa do mês, série dos últimos 6 meses
        /// e as últimas transações (títulos a receber/pagar).
        /// </summary>
        [HttpGet("operacional")]
        [AbacAuthorize("FluxoCaixa", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> Operacional(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterDashboardOperacionalQuery(), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }
    }
}
