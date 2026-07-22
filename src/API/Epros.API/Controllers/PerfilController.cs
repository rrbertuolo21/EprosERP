using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/plataforma/[controller]")]
    [Produces("application/json")]
    public class PerfilController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUser _currentUser;

        public PerfilController(IMediator mediator, ICurrentUser currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        [HttpGet("menu")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ObterMenu()
        {
            var userId = _currentUser.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(CommandResult.Falha(new[] { "Usuário não autenticado." }));
            }

            var result = await _mediator.Send(new ObterMenuDinamicoQuery(userId));

            if (!result.Sucesso)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("testar-desconto")]
        [AbacAuthorize("Desconto", "Aplicar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public IActionResult TestarDesconto([FromQuery] decimal percentual)
        {
            return Ok(CommandResult.Ok("Acesso autorizado. Desconto dentro do limite permitido.", new { DescontoAplicado = percentual }));
        }
    }
}
