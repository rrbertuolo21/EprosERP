using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class InstallationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InstallationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/v1/installation/state
        [HttpGet("state")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetState()
        {
            var result = await _mediator.Send(new ObterInstalacaoStateQuery());
            return Ok(result);
        }

        // GET api/v1/installation/check-requirements
        [HttpGet("check-requirements")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckRequirements()
        {
            var result = await _mediator.Send(new VerificarRequisitosQuery());
            return Ok(result);
        }

        // POST api/v1/installation/execute
        [HttpPost("execute")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Execute([FromBody] ExecutarInstalacaoCommand command)
        {
            // Verifica se a instalação já foi executada para rechaçar de imediato
            var state = await _mediator.Send(new ObterInstalacaoStateQuery());
            if (state.IsCompleted)
            {
                return BadRequest("A instalação já foi concluída. Reexecução bloqueada.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        // POST api/v1/installation/upgrade
        [HttpPost("upgrade")]
        [AbacAuthorize("SuperAdmin", "Configurar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Upgrade([FromBody] ExecutarAtualizacaoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        // GET api/v1/installation/upgrades-log
        [HttpGet("upgrades-log")]
        [AbacAuthorize("SuperAdmin", "Configurar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUpgradesLog()
        {
            var result = await _mediator.Send(new ListarUpdateLogsQuery());
            return Ok(result);
        }
    }
}
