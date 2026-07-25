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
    /// PRD-MES — Execução de Manufatura MES. Controller fino: apenas MediatR.
    /// Protegido por ABAC (recurso "ProducaoMes"). Sobe DESABILITADO por padrão (ABAC nega sem permissão semeada).
    /// Filtro de tenant é global (ContextBase).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/producao/mes/ordens")]
    public class ProducaoMesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProducaoMesController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("ProducaoMes", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListarMesOrdensQuery(status, pagina, tamanhoPagina), ct);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ProducaoMes", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ObterMesOrdemPorIdQuery(id), ct);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("ProducaoMes", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarMesOrdemCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/itens")]
        [AbacAuthorize("ProducaoMes", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> AdicionarItem(Guid id, [FromBody] AdicionarMesOrdemItemCommand command, CancellationToken ct)
        {
            var cmd = command with { OrdemId = id };
            var result = await _mediator.Send(cmd, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("itens/{itemId:guid}/producao")]
        [AbacAuthorize("ProducaoMes", "Apontar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> RegistrarProducao(Guid itemId, [FromBody] RegistrarMesProducaoItemCommand command, CancellationToken ct)
        {
            var cmd = command with { ItemId = itemId };
            var result = await _mediator.Send(cmd, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("servicos")]
        [AbacAuthorize("ProducaoMes", "Apontar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        public async Task<IActionResult> ApontarServico([FromBody] ApontarMesServicoCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("servicos/equipamentos")]
        [AbacAuthorize("ProducaoMes", "Apontar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> VincularEquipamento([FromBody] VincularMesEquipamentoCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/finalizar")]
        [AbacAuthorize("ProducaoMes", "Finalizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Finalizar(Guid id, [FromBody] FinalizarMesOrdemCommand command, CancellationToken ct)
        {
            var cmd = command with { OrdemId = id };
            var result = await _mediator.Send(cmd, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/submeter")]
        [AbacAuthorize("ProducaoMes", "Submeter")]
        public async Task<ActionResult<CommandResult>> Submeter(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SubmeterMesOrdemCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/aprovar")]
        [AbacAuthorize("ProducaoMes", "Aprovar")]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new AprovarMesOrdemCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/rejeitar")]
        [AbacAuthorize("ProducaoMes", "Aprovar")]
        public async Task<ActionResult<CommandResult>> Rejeitar(Guid id, [FromBody] string motivo, CancellationToken ct)
        {
            var result = await _mediator.Send(new RejeitarMesOrdemCommand(id, motivo), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/inativar")]
        [AbacAuthorize("ProducaoMes", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> Inativar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new InativarMesOrdemCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/reativar")]
        [AbacAuthorize("ProducaoMes", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> Reativar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ReativarMesOrdemCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/encerrar")]
        [AbacAuthorize("ProducaoMes", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> Encerrar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new EncerrarMesOrdemCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("parametros")]
        [AbacAuthorize("ProducaoMes", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> SalvarParametros([FromBody] SalvarMesParametroCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
