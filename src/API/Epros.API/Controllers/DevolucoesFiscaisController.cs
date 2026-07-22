using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Devoluções fiscais (NF-e de devolução). Controller fino: apenas MediatR, sem DbContext.
    /// Fiel ao fluxo da EF_DEVOLUCAO_FISCAL: criar (NOVO), transmitir, cancelar, corrigir, listar/consultar.
    /// </summary>
    [ApiController]
    [Route("api/v1/fiscal/devolucoes")]
    public class DevolucoesFiscaisController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DevolucoesFiscaisController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] int? estado,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20)
        {
            var result = await _mediator.Send(new ListarDevolucoesFiscaisQuery(estado, pagina, tamanhoPagina));
            return Ok(result.Dados);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var result = await _mediator.Send(new ObterDevolucaoFiscalPorIdQuery(id));
            if (!result.Sucesso)
            {
                return NotFound(result.Mensagem);
            }
            return Ok(result.Dados);
        }

        [HttpPost]
        [AbacAuthorize("DocumentosFiscais", "Criar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarDevolucaoFiscalCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Created(string.Empty, result);
        }

        [HttpPost("{id:guid}/transmitir")]
        [AbacAuthorize("DocumentosFiscais", "Criar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Transmitir(Guid id)
        {
            var result = await _mediator.Send(new TransmitirDevolucaoFiscalCommand(id));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("{id:guid}/cancelar")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Cancelar(Guid id, [FromBody] CancelarDevolucaoFiscalCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("O identificador da rota difere do corpo da requisição.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("{id:guid}/corrigir")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Corrigir(Guid id, [FromBody] CorrigirDevolucaoFiscalCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("O identificador da rota difere do corpo da requisição.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }
    }
}
