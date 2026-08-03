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
    /// Gestão de Contratos de Compra (EST-GCC). Controller fino: apenas MediatR. Protegido por ABAC
    /// (estoque.gcc.*). Submódulo sobe desabilitado — ABAC nega por padrão. Filtro de tenant é global.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/estoque-gcc-contratos")]
    public class GccContratosCompraController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GccContratosCompraController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AbacAuthorize("EstoqueGcc", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] ListarGccContratosCompraQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("EstoqueGcc", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterGccContratoCompraPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("EstoqueGcc", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarGccContratoCompraCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        [AbacAuthorize("EstoqueGcc", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarGccContratoCompraCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/enviar-aprovacao")]
        [AbacAuthorize("EstoqueGcc", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> EnviarAprovacao(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new EnviarGccContratoParaAprovacaoCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/aprovar")]
        [AbacAuthorize("EstoqueGcc", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new AprovarGccContratoCompraCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("consumos")]
        [AbacAuthorize("EstoqueGcc", "Consumir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> RegistrarConsumo([FromBody] RegistrarGccConsumoContratoCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>CD5 — registra e aplica um aditivo (preço/quantidade/vigência/condições) no contrato aprovado.</summary>
        [HttpPost("{id:guid}/aditivos")]
        [AbacAuthorize("EstoqueGcc", "Aditar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarAditivo(Guid id, [FromBody] RegistrarGccAditivoCommand body, CancellationToken cancellationToken)
        {
            var command = body with { ContratoCompraId = id };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>CD5 — performance do contrato: aderência (consumido/comprometido), saldo e vigência.</summary>
        [HttpGet("{id:guid}/performance")]
        [AbacAuthorize("EstoqueGcc", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> Performance(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new PerformanceGccContratoQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }
}
