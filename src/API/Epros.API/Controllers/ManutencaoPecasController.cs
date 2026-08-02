using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Application.Queries;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>
    /// MAN-PEC — Pecas de Reposicao. Controller fino. ABAC nega por padrao.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/manutencao/pecas-reposicao")]
    public class ManutencaoPecasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ManutencaoPecasController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("ManutencaoPecas", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListarRegistrosPecaReposicaoQuery(pagina, tamanhoPagina), ct);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ManutencaoPecas", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ObterRegistroPecaReposicaoPorIdQuery(id), ct);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("ManutencaoPecas", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarRegistroPecaReposicaoCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/itens")]
        [AbacAuthorize("ManutencaoPecas", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarItem(Guid id, [FromBody] AdicionarItemPecaCommand command, CancellationToken ct)
        {
            if (id != command.RegistroId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("politicas")]
        [AbacAuthorize("ManutencaoPecas", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DefinirPolitica([FromBody] DefinirPoliticaReposicaoCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        // MAN-PEC D22 — reservar peca (ciclo reservar -> consumir).
        [HttpPost("itens/{itemId:guid}/reservar")]
        [AbacAuthorize("ManutencaoPecas", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Reservar(Guid itemId, [FromBody] ReservarPecaCommand command, CancellationToken ct)
        {
            if (itemId != command.ItemId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // MAN-PEC D22 — baixar peca (consumo, delta, idempotente; move estoque pelo motor unico).
        [HttpPost("itens/{itemId:guid}/baixar")]
        [AbacAuthorize("ManutencaoPecas", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Baixar(Guid itemId, [FromBody] BaixarPecaCommand command, CancellationToken ct)
        {
            if (itemId != command.ItemId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // MAN-PEC D22 — devolver peca (movimento inverso).
        [HttpPost("itens/{itemId:guid}/devolver")]
        [AbacAuthorize("ManutencaoPecas", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Devolver(Guid itemId, [FromBody] DevolverPecaCommand command, CancellationToken ct)
        {
            if (itemId != command.ItemId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
