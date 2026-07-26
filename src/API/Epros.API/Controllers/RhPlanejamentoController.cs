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
    /// RH-PLN — Planejamento de RH. Controller fino (apenas MediatR).
    /// Submodulo sobe desabilitado: ABAC nega por padrao (nenhuma permissao "RhPlanejamento" semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/rh/planejamento")]
    [Produces("application/json")]
    public class RhPlanejamentoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RhPlanejamentoController(IMediator mediator) => _mediator = mediator;

        [HttpGet("turnos")]
        [AbacAuthorize("RhPlanejamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarTurnos()
            => Ok(await _mediator.Send(new ListarTurnosQuery()));

        [HttpPost("turnos")]
        [AbacAuthorize("RhPlanejamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarTurno([FromBody] CriarTurnoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("feriados")]
        [AbacAuthorize("RhPlanejamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarFeriados()
            => Ok(await _mediator.Send(new ListarFeriadosQuery()));

        [HttpPost("feriados")]
        [AbacAuthorize("RhPlanejamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarFeriado([FromBody] CriarFeriadoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("headcount")]
        [AbacAuthorize("RhPlanejamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarHeadcount()
            => Ok(await _mediator.Send(new ListarHeadcountQuery()));

        [HttpPost("headcount/itens")]
        [AbacAuthorize("RhPlanejamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DefinirHeadcountItem([FromBody] DefinirHeadcountItemCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
