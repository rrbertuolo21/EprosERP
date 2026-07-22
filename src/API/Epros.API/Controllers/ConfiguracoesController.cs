using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;

using Epros.API.Security;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/plataforma/[controller]")]
    [AbacAuthorize("SuperAdmin", "Configurar")]
    [Produces("application/json")]
    public class ConfiguracoesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConfiguracoesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Definir([FromBody] DefinirConfiguracaoGlobalCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                if (result.Erros.Any(e => e.Contains("Acesso Proibido")))
                {
                    return StatusCode(403, result);
                }
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpGet("{chave}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Obter(string chave)
        {
            var result = await _mediator.Send(new ObterConfiguracaoGlobalQuery(chave));

            if (!result.Sucesso)
            {
                if (result.Erros.Any(e => e.Contains("Acesso Proibido")))
                {
                    return StatusCode(403, result);
                }
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}
