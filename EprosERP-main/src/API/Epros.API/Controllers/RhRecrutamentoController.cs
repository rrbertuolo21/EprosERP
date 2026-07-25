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
    /// RH-REC — Recrutamento. Controller fino (apenas MediatR).
    /// Submodulo sobe desabilitado: ABAC nega por padrao (nenhuma permissao "RhRecrutamento" semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/rh/recrutamento")]
    [Produces("application/json")]
    public class RhRecrutamentoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RhRecrutamentoController(IMediator mediator) => _mediator = mediator;

        [HttpGet("vagas")]
        [AbacAuthorize("RhRecrutamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarVagas()
            => Ok(await _mediator.Send(new ListarVagasQuery()));

        [HttpPost("vagas")]
        [AbacAuthorize("RhRecrutamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarVaga([FromBody] CriarVagaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("vagas/{id}/publicar")]
        [AbacAuthorize("RhRecrutamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> PublicarVaga(Guid id)
        {
            var result = await _mediator.Send(new PublicarVagaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("candidatos")]
        [AbacAuthorize("RhRecrutamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarCandidatos()
            => Ok(await _mediator.Send(new ListarCandidatosQuery()));

        [HttpPost("candidatos")]
        [AbacAuthorize("RhRecrutamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarCandidatura([FromBody] RegistrarCandidaturaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("feedbacks")]
        [AbacAuthorize("RhRecrutamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarFeedback([FromBody] RegistrarFeedbackEntrevistaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
