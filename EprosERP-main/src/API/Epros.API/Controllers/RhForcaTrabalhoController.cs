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
    /// RH-WFM — Gestao da Forca de Trabalho. Controller fino (apenas MediatR).
    /// Submodulo sobe desabilitado: ABAC nega por padrao (nenhuma permissao "RhForcaTrabalho" semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/rh/forca-trabalho")]
    [Produces("application/json")]
    public class RhForcaTrabalhoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RhForcaTrabalhoController(IMediator mediator) => _mediator = mediator;

        [HttpGet("colaboradores")]
        [AbacAuthorize("RhForcaTrabalho", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarColaboradores()
            => Ok(await _mediator.Send(new ListarWfmColaboradoresQuery()));

        [HttpPost("colaboradores")]
        [AbacAuthorize("RhForcaTrabalho", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Admitir([FromBody] AdmitirWfmColaboradorCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("colaboradores/{id}/demitir")]
        [AbacAuthorize("RhForcaTrabalho", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Demitir(Guid id)
        {
            var result = await _mediator.Send(new DemitirWfmColaboradorCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("comissoes")]
        [AbacAuthorize("RhForcaTrabalho", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DefinirComissao([FromBody] DefinirComissaoColaboradorCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
