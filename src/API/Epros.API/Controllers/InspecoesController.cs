using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Qualidade.Application.Commands;
using Epros.Modules.Qualidade.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/qualidade/inspecoes")]
    [Produces("application/json")]
    public class InspecoesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InspecoesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("pendentes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterPendentes()
        {
            var result = await _mediator.Send(new ObterInspecoesPendentesQuery());
            return Ok(result);
        }

        [HttpPost("registrar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarResultado([FromBody] RegistrarResultadoInspecaoCommand command)
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
