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
    [Route("api/v1/codigos-beneficios-fiscais")]
    [Produces("application/json")]
    public class CodigosBeneficiosFiscaisController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CodigosBeneficiosFiscaisController(IMediator mediator)
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
            var query = new ListarCodigosBeneficiosFiscaisQuery(localizar, pagina, tamanhoPagina);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id)
        {
            var query = new ObterCodigoBeneficioFiscalPorIdQuery(id);
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
        public async Task<IActionResult> Criar([FromBody] CriarCodigoBeneficioFiscalCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Created(string.Empty, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarCodigoBeneficioFiscalCommand command)
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

        /// <summary>Exclui (soft-delete) um Código de Benefício Fiscal. Fiel ao DELETE do legado.</summary>
        /// <param name="id">Identificador do código de benefício fiscal.</param>
        /// <returns>Resultado da operação.</returns>
        /// <response code="200">Código de benefício fiscal excluído.</response>
        /// <response code="422">Código de benefício fiscal não localizado.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Deletar(Guid id)
        {
            var result = await _mediator.Send(new DeletarCodigoBeneficioFiscalCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
