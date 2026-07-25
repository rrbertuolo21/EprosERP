using System;
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
    [Route("api/v1/cfops")]
    [Produces("application/json")]
    public class CfopsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CfopsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar(
            [FromQuery] string? localizar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20)
        {
            var query = new ListarCfopsQuery(localizar, pagina, tamanhoPagina);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id)
        {
            var query = new ObterCfopPorIdQuery(id);
            var result = await _mediator.Send(query);
            if (!result.Sucesso)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarCfopCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            var createdId = ((dynamic)result.Dados!).Id;
            return CreatedAtAction(nameof(ObterPorId), new { id = createdId }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarCfopCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("O ID da rota não coincide com o ID do comando.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        /// <summary>Exclui (soft-delete) um CFOP. Fiel ao DELETE do legado <c>api/v1/cfops/{id}</c>.</summary>
        /// <param name="id">Identificador do CFOP.</param>
        /// <returns>Resultado da operação.</returns>
        /// <response code="200">CFOP excluído.</response>
        /// <response code="422">CFOP não localizado.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Deletar(Guid id)
        {
            var result = await _mediator.Send(new DeletarCfopCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
