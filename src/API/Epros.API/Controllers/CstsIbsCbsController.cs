using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/fiscal/csts-ibs-cbs")]
    public class CstsIbsCbsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CstsIbsCbsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar(
            [FromQuery] string? localizar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarCstsIbsCbsQuery(localizar, pagina, tamanhoPagina), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterCstIbsCbsPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarCstIbsCbsCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarCstIbsCbsCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
            {
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
            }

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Deletar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarCstIbsCbsCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ----- Classificações Tributárias (+ Anexos) — filhas do CST IBS/CBS -----

        [HttpPost("{id:guid}/classificacoes-tributarias")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarClassificacaoTributaria(Guid id, [FromBody] ClassificacaoTributariaInput classe, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new AdicionarClassificacaoTributariaCommand(id, classe), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}/classificacoes-tributarias/{classificacaoTributariaId:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RemoverClassificacaoTributaria(Guid id, Guid classificacaoTributariaId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RemoverClassificacaoTributariaCommand(id, classificacaoTributariaId), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
