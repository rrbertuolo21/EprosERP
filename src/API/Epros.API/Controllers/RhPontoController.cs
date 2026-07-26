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
    /// RH-PNT — Ponto e Jornada. Controller fino (apenas MediatR).
    /// Submodulo sobe desabilitado: ABAC nega por padrao (nenhuma permissao "RhPonto" semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/rh/ponto")]
    [Produces("application/json")]
    public class RhPontoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RhPontoController(IMediator mediator) => _mediator = mediator;

        [HttpGet("marcacoes")]
        [AbacAuthorize("RhPonto", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarMarcacoes()
            => Ok(await _mediator.Send(new ListarMarcacoesQuery()));

        [HttpPost("marcacoes")]
        [AbacAuthorize("RhPonto", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarMarcacao([FromBody] RegistrarMarcacaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("periodos")]
        [AbacAuthorize("RhPonto", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPeriodos()
            => Ok(await _mediator.Send(new ListarPeriodosApuracaoQuery()));

        [HttpPost("periodos")]
        [AbacAuthorize("RhPonto", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AbrirPeriodo([FromBody] AbrirPeriodoApuracaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("periodos/{id}/fechar")]
        [AbacAuthorize("RhPonto", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> FecharPeriodo(Guid id)
        {
            var result = await _mediator.Send(new FecharPeriodoApuracaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
