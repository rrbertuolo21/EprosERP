using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Pedidos de compra e tipos de pedido (EST-SC-001, EF Sourcing §9.3). Controller fino: apenas MediatR.
    /// Protegido por ABAC (recurso EstoqueSourcing) — nega por padrão (submódulo desabilitado).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/pedidos-compra")]
    public class PedidosCompraController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PedidosCompraController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("EstoqueSourcing", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] Guid? fornecedorId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarScPedidosCompraQuery(fornecedorId, pagina, tamanhoPagina), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("EstoqueSourcing", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterScPedidoCompraPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("EstoqueSourcing", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarScPedidoCompraCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("tipos")]
        [AbacAuthorize("EstoqueSourcing", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CriarTipo([FromBody] CriarScTipoPedidoCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }
    }
}
