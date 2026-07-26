using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/aplicativo/assinaturas")]
    [Produces("application/json")]
    public class AssinaturasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AssinaturasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("vigente")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterVigente()
        {
            var query = new ObterAssinaturaVigenteQuery();
            var result = await _mediator.Send(query);
            if (result == null)
            {
                return NotFound(new { Mensagem = "Nenhuma assinatura ativa ou trial configurada para este inquilino." });
            }
            return Ok(result);
        }

        [HttpPost("contratar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Contratar([FromBody] ContratarPlanoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet("faturas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarFaturas([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50)
        {
            var query = new ListarFaturasTenantQuery(pageIndex, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("faturas/{faturaId:guid}/pix")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> GerarPix(Guid faturaId)
        {
            var command = new GerarPixFaturaCommand(faturaId);
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }
    }
}
