using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Movimentos manuais de estoque (EST-MVM-001). Controller fino: apenas MediatR.
    /// Protegido por ABAC conforme EF §18 (estoque.movimentacao.*). Filtro de tenant é global (ContextBase).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/estoque-movimentos-manuais")]
    public class EstoqueMovimentosManuaisController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EstoqueMovimentosManuaisController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AbacAuthorize("EstoqueMovimentacao", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> Listar(
            [FromQuery] Guid? produtoId,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarEstoqueMovimentosManuaisQuery(produtoId, pagina, tamanhoPagina), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("EstoqueMovimentacao", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterEstoqueMovimentoManualPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("EstoqueMovimentacao", "Criar")]
        [AbacAuthorize("EstoqueMovimentacao", "Aplicar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarEstoqueMovimentoManualCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        [AbacAuthorize("EstoqueMovimentacao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarEstoqueMovimentoManualCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}")]
        [AbacAuthorize("EstoqueMovimentacao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Deletar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarEstoqueMovimentoManualCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Estorno controlado de um movimento já aplicado (MVM-030). Exige motivo.</summary>
        [HttpPost("{id:guid}/estornar")]
        [AbacAuthorize("EstoqueMovimentacao", "Estornar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Estornar(Guid id, [FromBody] EstornarEstoqueMovimentoManualCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
