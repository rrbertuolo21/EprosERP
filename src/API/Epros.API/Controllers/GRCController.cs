using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/grc")]
    [Produces("application/json")]
    public class GRCController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GRCController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("riscos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarRisco([FromBody] RegistrarRiscoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("controles")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarControle([FromBody] CriarControleCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("denuncias")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarDenuncia([FromBody] RegistrarDenunciaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("denuncias/{id}/julgar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> JulgarDenuncia(Guid id, [FromBody] JulgarDenunciaRequest request)
        {
            var command = new JulgarDenunciaCommand(id, request.StatusFinal, request.ParecerFinal);
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        public record JulgarDenunciaRequest(string StatusFinal, string ParecerFinal);

        [HttpPost("incidentes")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AbrirIncidente([FromBody] AbrirIncidenteCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpGet("riscos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarRiscos()
        {
            var result = await _mediator.Send(new ObterRiscosQuery());
            return Ok(result);
        }

        [HttpGet("denuncias")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarDenuncias()
        {
            var result = await _mediator.Send(new ObterDenunciasQuery());
            return Ok(result);
        }

        [HttpGet("incidentes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarIncidentes()
        {
            var result = await _mediator.Send(new ObterIncidentesQuery());
            return Ok(result);
        }
    }
}
