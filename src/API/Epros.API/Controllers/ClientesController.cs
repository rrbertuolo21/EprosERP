using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/plataforma/[controller]")]
    [Produces("application/json")]
    public class ClientesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarClienteCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/ativar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Ativar(Guid id)
        {
            var result = await _mediator.Send(new AtivarClienteCommand(id));

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/suspender")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Suspender(Guid id)
        {
            var result = await _mediator.Send(new SuspenderClienteCommand(id));

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new ObterClienteDetalhadoQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarClienteDetalhadoCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("O ID na URL não corresponde ao ID no corpo do comando.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("planos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarPlano([FromBody] CriarPlanoCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("faturas")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> GerarFatura([FromBody] GerarFaturaCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("faturas/{id}/baixar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> BaixarFatura(Guid id)
        {
            var result = await _mediator.Send(new BaixarFaturaCommand(id));

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpGet("sync/delta")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterSyncDelta([FromQuery] DateTime since)
        {
            var result = await _mediator.Send(new ObterClientesSyncQuery(since));
            return Ok(result);
        }

        [HttpPost("webhooks/mercadopago")]
        [AllowAnonymous] // Webhook externo (gateway MercadoPago): autenticado por assinatura (X-Signature), não por token.
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ProcessarWebhook(
            [FromBody] ProcessarWebhookPagamentoCommand command,
            [FromHeader(Name = "X-Signature")] string? xSignature = null,
            [FromHeader(Name = "Signature")] string? signature = null)
        {
            var sig = !string.IsNullOrEmpty(xSignature) ? xSignature : signature;
            var commandWithSig = command with { Signature = sig };

            var result = await _mediator.Send(commandWithSig);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }
    }
}
