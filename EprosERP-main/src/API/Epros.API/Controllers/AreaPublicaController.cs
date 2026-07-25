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
    [Route("api/v1/public/[controller]")]
    [Produces("application/json")]
    [AllowAnonymous] // Área pública (landing/newsletter/marketing): sem autenticação por design.
    public class AreaPublicaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AreaPublicaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("newsletter/inscrever")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> InscreverNewsletter([FromBody] InscreverNewsletterCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("/api/v1/public/newsletter/opt-out")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CancelarNewsletterPorToken([FromBody] CancelarNewsletterPorTokenCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet("paginas")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarCustomPagesPublicas()
        {
            var result = await _mediator.Send(new ListarCustomPagesQuery());
            return Ok(result);
        }

        [HttpGet("planos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarPlanosPublicos()
        {
            var result = await _mediator.Send(new ListarPlanosPublicosQuery());
            return Ok(result);
        }
    }
}
