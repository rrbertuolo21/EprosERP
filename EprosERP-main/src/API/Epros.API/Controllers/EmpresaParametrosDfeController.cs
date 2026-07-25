using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Parâmetros DF-e da empresa (série/número/CSC de NF-e e NFC-e).
    /// Espelha o CQRS já existente em GestaoClientes; controller fino (só MediatR).
    /// Compatível com o legado: api/v1/empresas/{empresaId}/parametros-dfe.
    /// </summary>
    [ApiController]
    [Route("api/v1/cadastros/empresas/{empresaId:guid}/parametros-dfe")]
    [Produces("application/json")]
    public class EmpresaParametrosDfeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmpresaParametrosDfeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Obter(Guid empresaId)
        {
            var dto = await _mediator.Send(new ObterEmpresaParametrosDfeQuery(empresaId));
            if (dto == null)
            {
                return NotFound("Parâmetros DF-e não encontrados para esta empresa.");
            }
            return Ok(dto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar(Guid empresaId, [FromBody] CriarEmpresaParametrosDfeCommand command)
        {
            if (empresaId != command.EmpresaId)
            {
                return BadRequest("O ID da empresa na rota não coincide com o ID do corpo da requisição.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Created(string.Empty, result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid empresaId, [FromBody] AtualizarEmpresaParametrosDfeCommand command)
        {
            if (empresaId != command.EmpresaId)
            {
                return BadRequest("O ID da empresa na rota não coincide com o ID do corpo da requisição.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Deletar(Guid empresaId)
        {
            var result = await _mediator.Send(new DeletarEmpresaParametrosDfeCommand(empresaId));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }
    }
}
