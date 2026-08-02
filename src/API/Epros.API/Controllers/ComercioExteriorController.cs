using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Submódulo Comércio Exterior / Importação (CD1 / EF COMERCIO_EXTERIOR). Dados de comércio exterior na
    /// compra (incoterm/moeda/câmbio), parâmetro de rateio landed (desligado por padrão — NF-02) e
    /// nacionalização (entrada Estoque D1 + títulos financeiros). Controller fino: apenas MediatR.
    /// Protegido por ABAC (ComercioExterior.*) — nega por padrão. Tributos/base/CFOP = valida-contador.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/compras-comercio-exterior")]
    public class ComercioExteriorController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ComercioExteriorController(IMediator mediator) => _mediator = mediator;

        [HttpPut("compras/{compraId:guid}")]
        [AbacAuthorize("ComercioExterior", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DefinirComercioExterior(Guid compraId, [FromBody] DefinirComercioExteriorCompraCommand body, CancellationToken cancellationToken)
        {
            var command = body with { CompraId = compraId };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("rateio-landed")]
        [AbacAuthorize("ComercioExterior", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ObterRateioLanded([FromQuery] Guid? empresaId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterRateioLandedConfigQuery(empresaId), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpPut("rateio-landed")]
        [AbacAuthorize("ComercioExterior", "Configurar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> SalvarRateioLanded([FromBody] SalvarRateioLandedConfigCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("compras/{compraId:guid}/nacionalizar")]
        [AbacAuthorize("ComercioExterior", "Nacionalizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Nacionalizar(Guid compraId, [FromBody] NacionalizarImportacaoCommand? body, CancellationToken cancellationToken)
        {
            var command = (body ?? new NacionalizarImportacaoCommand(compraId)) with { CompraId = compraId };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
