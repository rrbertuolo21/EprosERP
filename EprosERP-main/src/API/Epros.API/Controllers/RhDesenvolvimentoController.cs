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
    /// RH-DEV — Desenvolvimento de Funcionarios. Controller fino (apenas MediatR).
    /// Submodulo sobe desabilitado: ABAC nega por padrao (nenhuma permissao "RhDesenvolvimento" semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/rh/desenvolvimento")]
    [Produces("application/json")]
    public class RhDesenvolvimentoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RhDesenvolvimentoController(IMediator mediator) => _mediator = mediator;

        [HttpGet("promocoes")]
        [AbacAuthorize("RhDesenvolvimento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPromocoes()
            => Ok(await _mediator.Send(new ListarPromocoesQuery()));

        [HttpPost("promocoes")]
        [AbacAuthorize("RhDesenvolvimento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarPromocao([FromBody] RegistrarPromocaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("promocoes/{id}/aprovar")]
        [AbacAuthorize("RhDesenvolvimento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AprovarPromocao(Guid id)
        {
            var result = await _mediator.Send(new AprovarPromocaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("promocoes/{id}/rejeitar")]
        [AbacAuthorize("RhDesenvolvimento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RejeitarPromocao(Guid id)
        {
            var result = await _mediator.Send(new RejeitarPromocaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("advertencias")]
        [AbacAuthorize("RhDesenvolvimento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarAdvertencias()
            => Ok(await _mediator.Send(new ListarAdvertenciasQuery()));

        [HttpPost("advertencias")]
        [AbacAuthorize("RhDesenvolvimento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarAdvertencia([FromBody] RegistrarAdvertenciaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("desligamentos")]
        [AbacAuthorize("RhDesenvolvimento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarDesligamento([FromBody] RegistrarDesligamentoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
