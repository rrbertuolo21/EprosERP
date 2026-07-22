using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/rh")]
    [Produces("application/json")]
    public class RHController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RHController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("colaboradores")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdmitirColaborador([FromBody] AdmitirColaboradorCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("colaboradores/{id}/desligar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DesligarColaborador(Guid id, [FromBody] DesligarColaboradorRequest request)
        {
            var command = new DesligarColaboradorCommand(id, request.DataDemissao);
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        public record DesligarColaboradorRequest(DateTime DataDemissao);

        [HttpPost("timesheets")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarTimesheet([FromBody] RegistrarTimesheetCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("folhas/processar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ProcessarFolhaPagamento([FromBody] ProcessarFolhaPagamentoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpGet("colaboradores")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarColaboradores()
        {
            var result = await _mediator.Send(new ObterColaboradoresQuery());
            return Ok(result);
        }

        [HttpGet("folhas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarFolhas()
        {
            var result = await _mediator.Send(new ObterFolhasPagamentoQuery());
            return Ok(result);
        }
    }
}
