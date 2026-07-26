using System;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/plataforma")]
    [Produces("application/json")]
    public class MenuCatalogoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MenuCatalogoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("menus/paginado")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 25)
        {
            var query = new ListarMenusQuery(pagina, tamanhoPagina);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("menus/{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id)
        {
            var query = new ObterMenuPorIdQuery(id);
            var result = await _mediator.Send(query);
            if (!result.Sucesso)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }
}
