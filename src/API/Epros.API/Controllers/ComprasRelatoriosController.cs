using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Estoque.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Relatórios de COMPRAS (CD7 — pacote completo): curva ABC de fornecedor, savings de cotação, lead time
    /// e aderência de alçada. Controller fino: apenas MediatR. Protegido por ABAC (ComprasRelatorio.*).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/compras-relatorios")]
    public class ComprasRelatoriosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ComprasRelatoriosController(IMediator mediator) => _mediator = mediator;

        [HttpGet("curva-abc-fornecedor")]
        [AbacAuthorize("ComprasRelatorio", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> CurvaAbcFornecedor([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CurvaAbcFornecedorQuery(dataInicio, dataFim), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("savings-cotacao")]
        [AbacAuthorize("ComprasRelatorio", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> SavingsCotacao(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new SavingsCotacaoQuery(), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("lead-time")]
        [AbacAuthorize("ComprasRelatorio", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> LeadTime([FromQuery] Guid? fornecedorId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new LeadTimeComprasQuery(fornecedorId), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("aderencia-alcada")]
        [AbacAuthorize("ComprasRelatorio", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> AderenciaAlcada(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new AderenciaAlcadaQuery(), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }
    }
}
