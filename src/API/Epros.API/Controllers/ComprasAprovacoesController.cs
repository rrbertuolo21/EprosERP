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
    /// Ciclo de aprovação por alçada de compras (CD3 / EF SOURCING §6.2). Solicitar → aprovar/reprovar
    /// por nível. Controller fino: apenas MediatR. Protegido por ABAC (ComprasAprovacao.*). Submódulo
    /// novo — sobe desabilitado (ABAC nega por padrão).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/compras-aprovacoes")]
    public class ComprasAprovacoesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ComprasAprovacoesController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("ComprasAprovacao", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] ListarPedidosAprovacaoCompraQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ComprasAprovacao", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterPedidoAprovacaoCompraPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("solicitar")]
        [AbacAuthorize("ComprasAprovacao", "Solicitar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Solicitar([FromBody] SolicitarAprovacaoCompraCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/aprovar")]
        [AbacAuthorize("ComprasAprovacao", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id, [FromBody] AprovarNivelAprovacaoCompraCommand? body, CancellationToken cancellationToken)
        {
            var command = new AprovarNivelAprovacaoCompraCommand(id, body?.Justificativa);
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/reprovar")]
        [AbacAuthorize("ComprasAprovacao", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Reprovar(Guid id, [FromBody] ReprovarNivelAprovacaoCompraCommand? body, CancellationToken cancellationToken)
        {
            var command = new ReprovarNivelAprovacaoCompraCommand(id, body?.Justificativa);
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/cancelar")]
        [AbacAuthorize("ComprasAprovacao", "Cancelar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Cancelar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CancelarAprovacaoCompraCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
