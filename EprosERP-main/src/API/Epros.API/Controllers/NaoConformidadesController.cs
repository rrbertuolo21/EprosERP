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
    [Route("api/v1/qualidade/nao-conformidades")]
    [Produces("application/json")]
    public class NaoConformidadesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NaoConformidadesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("ativas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterAtivas()
        {
            var result = await _mediator.Send(new ObterNaoConformidadesAtivasQuery());
            return Ok(result);
        }

        [HttpPost("tratar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Tratar([FromBody] TratarNaoConformidadeCommand command)
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
