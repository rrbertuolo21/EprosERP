using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Rastreabilidade de Lote e Serialização (EST-RLT). Lotes, seriais, bloqueio/desbloqueio, recall,
    /// sugestão FEFO (D11 — sugere, não trava) e genealogia. ABAC conforme EF (estoque.rlt.*).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/estoque-rastreabilidade")]
    public class RastreabilidadeLotesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RastreabilidadeLotesController(IMediator mediator) => _mediator = mediator;

        // ---------- Lotes ----------

        [HttpGet("lotes")]
        [AbacAuthorize("EstoqueRastreabilidade", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarLotes([FromQuery] Guid? produtoId, [FromQuery] EStatusLoteRastreabilidade? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var r = await _mediator.Send(new ListarLotesQuery(produtoId, status, pagina, tamanhoPagina), ct);
            return r.Sucesso ? Ok(r) : BadRequest(r);
        }

        [HttpGet("lotes/{id:guid}")]
        [AbacAuthorize("EstoqueRastreabilidade", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterLote(Guid id, CancellationToken ct)
        {
            var r = await _mediator.Send(new ObterLotePorIdQuery(id), ct);
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpPost("lotes")]
        [AbacAuthorize("EstoqueRastreabilidade", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CriarLote([FromBody] CriarLoteCommand command, CancellationToken ct)
        {
            var r = await _mediator.Send(command, ct);
            return r.Sucesso ? Created(string.Empty, r) : UnprocessableEntity(r);
        }

        [HttpPost("lotes/{id:guid}/bloquear")]
        [AbacAuthorize("EstoqueRastreabilidade", "Bloquear")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> BloquearLote(Guid id, [FromBody] BloquearLoteCommand command, CancellationToken ct)
        {
            if (id != command.LoteId) return BadRequest("O ID da rota não corresponde ao corpo.");
            var r = await _mediator.Send(command, ct);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("lotes/{id:guid}/desbloquear")]
        [AbacAuthorize("EstoqueRastreabilidade", "Bloquear")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DesbloquearLote(Guid id, [FromBody] DesbloquearLoteCommand command, CancellationToken ct)
        {
            if (id != command.LoteId) return BadRequest("O ID da rota não corresponde ao corpo.");
            var r = await _mediator.Send(command, ct);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpGet("lotes/{id:guid}/genealogia")]
        [AbacAuthorize("EstoqueRastreabilidade", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Genealogia(Guid id, CancellationToken ct)
        {
            var r = await _mediator.Send(new GenealogiaLoteQuery(id), ct);
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpGet("fefo")]
        [AbacAuthorize("EstoqueRastreabilidade", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> SugerirFefo([FromQuery] Guid empresaId, [FromQuery] Guid produtoId, [FromQuery] decimal quantidade, CancellationToken ct)
        {
            var r = await _mediator.Send(new SugerirLoteFefoQuery(empresaId, produtoId, quantidade), ct);
            return r.Sucesso ? Ok(r) : BadRequest(r);
        }

        // ---------- Seriais ----------

        [HttpGet("seriais")]
        [AbacAuthorize("EstoqueRastreabilidade", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarSeriais([FromQuery] Guid? produtoId, [FromQuery] EStatusNumeroSerial? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var r = await _mediator.Send(new ListarNumerosSeriaisQuery(produtoId, status, pagina, tamanhoPagina), ct);
            return r.Sucesso ? Ok(r) : BadRequest(r);
        }

        [HttpPost("seriais")]
        [AbacAuthorize("EstoqueRastreabilidade", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RegistrarSerial([FromBody] RegistrarNumeroSerialCommand command, CancellationToken ct)
        {
            var r = await _mediator.Send(command, ct);
            return r.Sucesso ? Created(string.Empty, r) : UnprocessableEntity(r);
        }

        // ---------- Recall ----------

        [HttpPost("recalls")]
        [AbacAuthorize("EstoqueRastreabilidade", "Recall")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AbrirRecall([FromBody] AbrirRecallLoteCommand command, CancellationToken ct)
        {
            var r = await _mediator.Send(command, ct);
            return r.Sucesso ? Created(string.Empty, r) : UnprocessableEntity(r);
        }

        [HttpPost("recalls/{recallId:guid}/concluir")]
        [AbacAuthorize("EstoqueRastreabilidade", "Recall")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ConcluirRecall(Guid recallId, CancellationToken ct)
        {
            var r = await _mediator.Send(new ConcluirRecallLoteCommand(recallId), ct);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }
    }
}
