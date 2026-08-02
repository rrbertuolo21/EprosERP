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
    /// Submódulo Devolução de Compra (CD4 / EF DEVOLUCAO_DE_COMPRA). Registra a devolução de mercadoria ao
    /// fornecedor (total/parcial) referenciando a compra de origem. Rascunho não gera efeito (DEV-003);
    /// confirmar publica saída de estoque (motor único D1) + estorno financeiro idempotente (DEV-006).
    /// Controller fino: apenas MediatR. Protegido por ABAC (DevolucaoCompra.*) — nega por padrão.
    /// CFOP/sentido = valida-contador (NF-06).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/compras-devolucoes")]
    public class DevolucoesCompraController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DevolucoesCompraController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("DevolucaoCompra", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] ListarDevolucoesCompraQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("DevolucaoCompra", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterDevolucaoCompraPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("DevolucaoCompra", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarDevolucaoCompraCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        [AbacAuthorize("DevolucaoCompra", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Atualizar(Guid id, [FromBody] AtualizarDevolucaoCompraCommand body, CancellationToken cancellationToken)
        {
            var command = body with { Id = id };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/confirmar")]
        [AbacAuthorize("DevolucaoCompra", "Confirmar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Confirmar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ConfirmarDevolucaoCompraCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/cancelar")]
        [AbacAuthorize("DevolucaoCompra", "Cancelar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Cancelar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CancelarDevolucaoCompraCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}")]
        [AbacAuthorize("DevolucaoCompra", "Excluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Excluir(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ExcluirDevolucaoCompraCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
