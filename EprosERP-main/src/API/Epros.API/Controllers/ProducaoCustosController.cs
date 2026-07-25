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
    /// PRD-CST — Custos de Produção. Controller fino. Protegido por ABAC (recurso "ProducaoCustos").
    /// Sobe DESABILITADO por padrão. Filtro de tenant global (ContextBase).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/producao/custos")]
    public class ProducaoCustosController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProducaoCustosController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("ProducaoCustos", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListarCustosProducaoQuery(status, pagina, tamanhoPagina), ct);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ProducaoCustos", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ObterCustoProducaoPorIdQuery(id), ct);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("ProducaoCustos", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarCustoProducaoCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/submeter")]
        [AbacAuthorize("ProducaoCustos", "Submeter")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Submeter(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SubmeterCustoProducaoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/aprovar")]
        [AbacAuthorize("ProducaoCustos", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new AprovarCustoProducaoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/rejeitar")]
        [AbacAuthorize("ProducaoCustos", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Rejeitar(Guid id, [FromBody] string motivo, CancellationToken ct)
        {
            var result = await _mediator.Send(new RejeitarCustoProducaoCommand(id, motivo), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/inativar")]
        [AbacAuthorize("ProducaoCustos", "Gerenciar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Inativar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new InativarCustoProducaoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/reativar")]
        [AbacAuthorize("ProducaoCustos", "Gerenciar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Reativar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ReativarCustoProducaoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/encerrar")]
        [AbacAuthorize("ProducaoCustos", "Gerenciar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Encerrar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new EncerrarCustoProducaoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
