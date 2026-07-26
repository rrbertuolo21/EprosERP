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
    /// Contatos e Inscrições Estaduais de ST (IE-ST) da empresa.
    /// Espelha o CQRS já existente em GestaoClientes; controller fino (só MediatR).
    /// </summary>
    [ApiController]
    [Route("api/v1/cadastros/empresas/{empresaId:guid}")]
    [Produces("application/json")]
    public class EmpresaContatosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmpresaContatosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ---------- Contatos ----------

        [HttpGet("contatos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarContatos(Guid empresaId)
        {
            var contatos = await _mediator.Send(new ListarEmpresaContatosQuery(empresaId));
            return Ok(contatos);
        }

        [HttpPost("contatos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarContato(Guid empresaId, [FromBody] AdicionarEmpresaContatoCommand command)
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

        [HttpPut("contatos/{contatoId:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AtualizarContato(Guid empresaId, Guid contatoId, [FromBody] AtualizarEmpresaContatoCommand command)
        {
            if (empresaId != command.EmpresaId || contatoId != command.ContatoId)
            {
                return BadRequest("Os IDs da rota não coincidem com os IDs do corpo da requisição.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpDelete("contatos/{contatoId:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DeletarContato(Guid empresaId, Guid contatoId)
        {
            var result = await _mediator.Send(new DeletarEmpresaContatoCommand(empresaId, contatoId));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        // ---------- Inscrições Estaduais de ST (IE-ST) ----------

        [HttpGet("inscricoes-estaduais-st")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarIeSts(Guid empresaId)
        {
            var ieSts = await _mediator.Send(new ListarIeStsQuery(empresaId));
            return Ok(ieSts);
        }

        [HttpPost("inscricoes-estaduais-st")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarIeSt(Guid empresaId, [FromBody] AdicionarIeStCommand command)
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

        [HttpPut("inscricoes-estaduais-st/{ieStId:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AtualizarIeSt(Guid empresaId, Guid ieStId, [FromBody] AtualizarIeStCommand command)
        {
            if (empresaId != command.EmpresaId || ieStId != command.IeStId)
            {
                return BadRequest("Os IDs da rota não coincidem com os IDs do corpo da requisição.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpDelete("inscricoes-estaduais-st/{ieStId:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DeletarIeSt(Guid empresaId, Guid ieStId)
        {
            var result = await _mediator.Send(new DeletarIeStCommand(empresaId, ieStId));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }
    }
}
