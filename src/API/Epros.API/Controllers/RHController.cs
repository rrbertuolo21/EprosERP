using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Application.Queries;
using Epros.Shared.Application.Models;
using Epros.API.Security;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/rh")]
    [Produces("application/json")]
    public class RHController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RHController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("colaboradores")]
        [AbacAuthorize("RhColaborador", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdmitirColaborador([FromBody] AdmitirColaboradorCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("colaboradores/{id}/desligar")]
        [AbacAuthorize("RhColaborador", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DesligarColaborador(Guid id, [FromBody] DesligarColaboradorRequest request)
        {
            var command = new DesligarColaboradorCommand(
                id,
                request.DataDemissao,
                request.TipoDesligamento,
                request.SaldoFgtsDepositado,
                request.TemFeriasVencidas,
                request.RemuneracaoFeriasVencidas,
                request.MediaVariaveis,
                request.NumDependentes,
                request.PensaoAlimenticia);
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        /// <summary>Desligamento. Informando <c>TipoDesligamento</c>, a rescisão é apurada pelo motor (RN-07).</summary>
        public record DesligarColaboradorRequest(
            DateTime DataDemissao,
            Epros.Modules.RH.Domain.Folha.Calculo.TipoDesligamento? TipoDesligamento = null,
            decimal SaldoFgtsDepositado = 0m,
            bool TemFeriasVencidas = false,
            decimal RemuneracaoFeriasVencidas = 0m,
            decimal MediaVariaveis = 0m,
            int NumDependentes = 0,
            decimal PensaoAlimenticia = 0m);

        [HttpPost("timesheets")]
        [AbacAuthorize("RhPonto", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarTimesheet([FromBody] RegistrarTimesheetCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("folhas/processar")]
        [AbacAuthorize("RhFolha", "Processar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ProcessarFolhaPagamento([FromBody] ProcessarFolhaPagamentoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpGet("colaboradores")]
        [AbacAuthorize("RhColaborador", "Ler")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarColaboradores()
        {
            var result = await _mediator.Send(new ObterColaboradoresQuery());
            return Ok(result);
        }

        [HttpGet("folhas")]
        [AbacAuthorize("RhFolha", "Ler")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarFolhas()
        {
            var result = await _mediator.Send(new ObterFolhasPagamentoQuery());
            return Ok(result);
        }
    }
}
