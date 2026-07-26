using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// RH-SSO — Saude e Seguranca Ocupacional. Controller fino (apenas MediatR).
    /// Dados medicos sensiveis. Submodulo sobe desabilitado: ABAC nega por padrao
    /// (nenhuma permissao "RhSaudeSeguranca" semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/rh/sso")]
    [Produces("application/json")]
    public class RhSaudeController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RhSaudeController(IMediator mediator) => _mediator = mediator;

        [HttpGet("ppp")]
        [AbacAuthorize("RhSaudeSeguranca", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPpps()
            => Ok(await _mediator.Send(new ListarPppsQuery()));

        [HttpPost("ppp")]
        [AbacAuthorize("RhSaudeSeguranca", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarPpp([FromBody] CriarPppCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("exames")]
        [AbacAuthorize("RhSaudeSeguranca", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarExame([FromBody] RegistrarExameMedicoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
