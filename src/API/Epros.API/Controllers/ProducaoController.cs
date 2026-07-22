using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/producao")]
    [Produces("application/json")]
    public class ProducaoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProducaoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("bom")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarBOM([FromBody] CriarListaMateriaisCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpGet("bom/{sku}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterBOM(string sku)
        {
            var result = await _mediator.Send(new ObterListaMateriaisQuery(sku));
            if (!result.Sucesso) return NotFound(result);
            return Ok(result);
        }

        [HttpPost("ordens")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AbrirOrdemProducao([FromBody] AbrirOrdemProducaoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpGet("ordens")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarOrdensProducao()
        {
            var result = await _mediator.Send(new ObterOrdensProducaoQuery());
            return Ok(result);
        }

        [HttpPost("ordens/{id}/iniciar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> IniciarOrdemProducao(Guid id)
        {
            var result = await _mediator.Send(new IniciarProducaoCommand(id));
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        public record RegistrarApontamentoRequest(decimal QuantidadeApontada, decimal QuantidadeRefugada, string Operador);

        [HttpPost("ordens/{id}/apontar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarApontamento(Guid id, [FromBody] RegistrarApontamentoRequest request)
        {
            var command = new RegistrarApontamentoCommand(id, request.QuantidadeApontada, request.QuantidadeRefugada, request.Operador);
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("ordens/{id}/encerrar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EncerrarOrdemProducao(Guid id)
        {
            var result = await _mediator.Send(new EncerrarOrdemProducaoCommand(id));
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }
    }
}
