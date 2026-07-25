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
    /// PRD-EST — Estimativa industrial. Controller fino. Protegido por ABAC (recurso "ProducaoEstimativa").
    /// Sobe DESABILITADO por padrão. Filtro de tenant global (ContextBase).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/producao/estimativas")]
    public class ProducaoEstimativasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProducaoEstimativasController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("ProducaoEstimativa", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListarEstimativasQuery(status, pagina, tamanhoPagina), ct);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ProducaoEstimativa", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ObterEstimativaPorIdQuery(id), ct);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("ProducaoEstimativa", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarEstimativaCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/submeter")]
        [AbacAuthorize("ProducaoEstimativa", "Submeter")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Submeter(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SubmeterEstimativaCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/aprovar")]
        [AbacAuthorize("ProducaoEstimativa", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new AprovarEstimativaCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/rejeitar")]
        [AbacAuthorize("ProducaoEstimativa", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Rejeitar(Guid id, [FromBody] string motivo, CancellationToken ct)
        {
            var result = await _mediator.Send(new RejeitarEstimativaCommand(id, motivo), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/converter")]
        [AbacAuthorize("ProducaoEstimativa", "Converter")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Converter(Guid id, [FromBody] ConverterEstimativaCommand command, CancellationToken ct)
        {
            if (id != command.Id)
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/inativar")]
        [AbacAuthorize("ProducaoEstimativa", "Gerenciar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Inativar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new InativarEstimativaCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/reativar")]
        [AbacAuthorize("ProducaoEstimativa", "Gerenciar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Reativar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ReativarEstimativaCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/encerrar")]
        [AbacAuthorize("ProducaoEstimativa", "Gerenciar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Encerrar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new EncerrarEstimativaCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
