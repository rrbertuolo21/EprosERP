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
    [Route("api/v1")]
    [Produces("application/json")]
    public class OnboardingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OnboardingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("onboarding/empresa/{empresaId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterConfiguracaoEmpresa([FromRoute] Guid empresaId)
        {
            var query = new ObterConfiguracaoEmpresaQuery(empresaId);
            var result = await _mediator.Send(query);
            if (result == null)
            {
                return NotFound(new { Mensagem = "Configurações não encontradas para a empresa informada." });
            }
            return Ok(result);
        }

        [HttpPut("onboarding/empresa")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> SalvarConfiguracaoEmpresa([FromBody] SalvarConfiguracaoEmpresaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet("public/onboarding/idiomas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarIdiomasHabilitados()
        {
            var query = new ListarIdiomasHabilitadosQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("onboarding/idiomas/habilitar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> HabilitarIdioma([FromBody] HabilitarIdiomaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("onboarding/idiomas/desabilitar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DesabilitarIdioma([FromBody] DesabilitarIdiomaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("onboarding/cliente-saas")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarClienteSaaS([FromBody] CriarClienteSaaSOnboardingCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Created(string.Empty, result);
        }

        [HttpGet("onboarding/sessao/contexto")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterContextoSessao([FromQuery] string tenantId, [FromQuery] Guid usuarioId)
        {
            var query = new ObterContextoSessaoQuery(tenantId, usuarioId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
