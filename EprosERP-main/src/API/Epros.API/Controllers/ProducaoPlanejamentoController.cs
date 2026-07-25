using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// PRD-PLN — Planejamento de Produção. Controller fino. Protegido por ABAC (recurso "ProducaoPlanejamento").
    /// Sobe DESABILITADO por padrão. Filtro de tenant global (ContextBase).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/producao/planejamentos")]
    public class ProducaoPlanejamentoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProducaoPlanejamentoController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("ProducaoPlanejamento", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListarPlanejamentosProducaoQuery(status, pagina, tamanhoPagina), ct);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ProducaoPlanejamento", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ObterPlanejamentoProducaoPorIdQuery(id), ct);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("ProducaoPlanejamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarPlanejamentoProducaoCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/snapshots")]
        [AbacAuthorize("ProducaoPlanejamento", "Atualizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarSnapshot(Guid id, [FromBody] AdicionarSnapshotPlanejamentoCommand command, CancellationToken ct)
        {
            if (id != command.PlanejamentoId)
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/submeter")]
        [AbacAuthorize("ProducaoPlanejamento", "Submeter")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Submeter(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SubmeterPlanejamentoProducaoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/aprovar")]
        [AbacAuthorize("ProducaoPlanejamento", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new AprovarPlanejamentoProducaoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/rejeitar")]
        [AbacAuthorize("ProducaoPlanejamento", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Rejeitar(Guid id, [FromBody] string motivo, CancellationToken ct)
        {
            var result = await _mediator.Send(new RejeitarPlanejamentoProducaoCommand(id, motivo), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/inativar")]
        [AbacAuthorize("ProducaoPlanejamento", "Gerenciar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Inativar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new InativarPlanejamentoProducaoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/reativar")]
        [AbacAuthorize("ProducaoPlanejamento", "Gerenciar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Reativar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ReativarPlanejamentoProducaoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/encerrar")]
        [AbacAuthorize("ProducaoPlanejamento", "Gerenciar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Encerrar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new EncerrarPlanejamentoProducaoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
