using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/manutencao")]
    [Produces("application/json")]
    public class ManutencaoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ManutencaoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("equipamentos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarEquipamento([FromBody] CriarEquipamentoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("ordens")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AbrirOrdem([FromBody] AbrirOrdemManutencaoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("ordens/{id}/pecas")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarPeca(Guid id, [FromBody] AdicionarPecaRequest request)
        {
            var command = new AdicionarPecaOrdemCommand(id, request.ProdutoId, request.Quantidade);
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        public record AdicionarPecaRequest(Guid ProdutoId, decimal Quantidade);

        [HttpPost("ordens/{id}/concluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ConcluirOrdem(Guid id, [FromBody] ConcluirOrdemRequest request)
        {
            var command = new ConcluirOrdemManutencaoCommand(id, request.DescricaoServicoExecutado, request.CustoMaoObra);
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        public record ConcluirOrdemRequest(string DescricaoServicoExecutado, decimal CustoMaoObra);

        [HttpGet("equipamentos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarEquipamentos()
        {
            var result = await _mediator.Send(new ObterEquipamentosQuery());
            return Ok(result);
        }

        [HttpGet("ordens")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarOrdens()
        {
            var result = await _mediator.Send(new ObterOrdensManutencaoQuery());
            return Ok(result);
        }
    }
}
