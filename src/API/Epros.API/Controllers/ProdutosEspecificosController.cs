using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/produtos-especificos")]
    public class ProdutosEspecificosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProdutosEspecificosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> Listar(
            [FromQuery] Guid? produtoId,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarProdutosEspecificosQuery(produtoId, pagina, tamanhoPagina), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterProdutoEspecificoPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarProdutoEspecificoCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarProdutoEspecificoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Deletar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarProdutoEspecificoCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
