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
    /// Regras de alçada de aprovação de compras (CD3 / EF SOURCING §5.8). Controller fino: apenas MediatR.
    /// Protegido por ABAC (ComprasAlcada.*). Submódulo novo — sobe desabilitado (ABAC nega por padrão).
    /// Namespace/rota lógicos de COMPRAS (extração física do assembly é refactor futuro).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/compras-alcadas")]
    public class ComprasAlcadasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ComprasAlcadasController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("ComprasAlcada", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] ListarComprasAlcadaRegrasQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpPost]
        [AbacAuthorize("ComprasAlcada", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarComprasAlcadaRegraCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        [AbacAuthorize("ComprasAlcada", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarComprasAlcadaRegraCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}")]
        [AbacAuthorize("ComprasAlcada", "Excluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Excluir(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ExcluirComprasAlcadaRegraCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }
}
