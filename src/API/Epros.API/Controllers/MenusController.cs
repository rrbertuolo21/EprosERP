using System;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
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
    public class MenusController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MenusController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("menus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterCatalogoMenus()
        {
            var query = new ObterArvoreCompletaMenusQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
