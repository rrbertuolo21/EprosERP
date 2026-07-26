using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Epros.API.Security;
using MediatR;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/plataforma/perfis-acesso")]
    public class PerfisAcessoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PerfisAcessoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AbacAuthorize("PerfilAcesso", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] string? localizar,
            [FromQuery] bool? ativo,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20)
        {
            var result = await _mediator.Send(new ListarPerfisAcessoQuery(localizar, ativo, pagina, tamanhoPagina));
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("PerfilAcesso", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id)
        {
            var result = await _mediator.Send(new ObterPerfilAcessoPorIdQuery(id));
            if (!result.Sucesso)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [AbacAuthorize("PerfilAcesso", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarPerfilAcessoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            var createdId = ((dynamic)result.Dados!).PerfilAcessoId;
            return CreatedAtAction(nameof(ObterPorId), new { id = createdId }, result);
        }

        [HttpPut("{id:guid}")]
        [AbacAuthorize("PerfilAcesso", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPerfilAcessoCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("O ID da rota não coincide com o ID do corpo da requisição.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [AbacAuthorize("PerfilAcesso", "Excluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> Excluir(Guid id)
        {
            var result = await _mediator.Send(new DeletarPerfilAcessoCommand(id));
            if (!result.Sucesso)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }
}
