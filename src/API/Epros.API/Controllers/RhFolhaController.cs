using System;
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
    /// RH-FP — Folha de Pagamento e Beneficios. Controller fino (apenas MediatR).
    /// Submodulo sobe desabilitado: ABAC nega por padrao (nenhuma permissao "RhFolha" semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/rh/folha")]
    [Produces("application/json")]
    public class RhFolhaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RhFolhaController(IMediator mediator) => _mediator = mediator;

        [HttpGet("rubricas")]
        [AbacAuthorize("RhFolha", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarRubricas()
            => Ok(await _mediator.Send(new ListarRubricasQuery()));

        [HttpPost("rubricas")]
        [AbacAuthorize("RhFolha", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarRubrica([FromBody] CriarRubricaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("competencias")]
        [AbacAuthorize("RhFolha", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarCompetencias()
            => Ok(await _mediator.Send(new ListarCompetenciasQuery()));

        [HttpPost("competencias")]
        [AbacAuthorize("RhFolha", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AbrirCompetencia([FromBody] AbrirCompetenciaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("competencias/{id}/fechar")]
        [AbacAuthorize("RhFolha", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> FecharCompetencia(Guid id)
        {
            var result = await _mediator.Send(new FecharCompetenciaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
