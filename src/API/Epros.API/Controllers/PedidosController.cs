using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/aplicativo/pedidos")]
    [Produces("application/json")]
    public class PedidosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PedidosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarPedidoSaaSCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50)
        {
            var query = new ListarPedidosClienteQuery(pageIndex, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("{pedidoId:guid}/checkout")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> IniciarCheckout(Guid pedidoId)
        {
            var command = new IniciarCheckoutCommand(pedidoId);
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("transferencia")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EnviarTransferencia([FromBody] RegistrarTransferenciaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("transferencia/analisar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AnalisarTransferencia([FromBody] AnalisarTransferenciaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet("transferencia/pendentes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarTransferenciasPendentes([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50)
        {
            var query = new ObterTransferenciasPendentesQuery(pageIndex, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("webhook")]
        [AllowAnonymous] // Webhook externo de pagamento: validado por assinatura no handler, não por token.
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ProcessarWebhook([FromBody] ProcessarWebhookPagamentoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }
    }
}
