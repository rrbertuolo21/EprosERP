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
    /// Submódulo Comércio Exterior / Importação (CD1 / EF COMERCIO_EXTERIOR). Dados de comércio exterior na
    /// compra (incoterm/moeda/câmbio), parâmetro de rateio landed (desligado por padrão — NF-02) e
    /// nacionalização (entrada Estoque D1 + títulos financeiros). Controller fino: apenas MediatR.
    /// Protegido por ABAC (ComercioExterior.*) — nega por padrão. Tributos/base/CFOP = valida-contador.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/compras-comercio-exterior")]
    public class ComercioExteriorController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ComercioExteriorController(IMediator mediator) => _mediator = mediator;

        [HttpPut("compras/{compraId:guid}")]
        [AbacAuthorize("ComercioExterior", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DefinirComercioExterior(Guid compraId, [FromBody] DefinirComercioExteriorCompraCommand body, CancellationToken cancellationToken)
        {
            var command = body with { CompraId = compraId };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("rateio-landed")]
        [AbacAuthorize("ComercioExterior", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ObterRateioLanded([FromQuery] Guid? empresaId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterRateioLandedConfigQuery(empresaId), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpPut("rateio-landed")]
        [AbacAuthorize("ComercioExterior", "Configurar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> SalvarRateioLanded([FromBody] SalvarRateioLandedConfigCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("compras/{compraId:guid}/nacionalizar")]
        [AbacAuthorize("ComercioExterior", "Nacionalizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Nacionalizar(Guid compraId, [FromBody] NacionalizarImportacaoCommand? body, CancellationToken cancellationToken)
        {
            var command = (body ?? new NacionalizarImportacaoCommand(compraId)) with { CompraId = compraId };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ===================== Declaração de Importação (DI) por item + adições (CEX-001..023) =====================
        // valida-contador: ValorAFRMM/ValorDesconto são factuais (informados pelo despacho aduaneiro/contador).

        [HttpGet("itens/{compraItemId:guid}/declaracoes")]
        [AbacAuthorize("ComercioExterior", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarDeclaracoes(Guid compraItemId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterDeclaracoesImportacaoPorItemQuery(compraItemId), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpPost("itens/{compraItemId:guid}/declaracoes")]
        [AbacAuthorize("ComercioExterior", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarDeclaracao(Guid compraItemId, [FromBody] RegistrarDeclaracaoImportacaoCommand body, CancellationToken cancellationToken)
        {
            var command = body with { CompraItemId = compraItemId };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("declaracoes/{declaracaoId:guid}")]
        [AbacAuthorize("ComercioExterior", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AlterarDeclaracao(Guid declaracaoId, [FromBody] AlterarDeclaracaoImportacaoCommand body, CancellationToken cancellationToken)
        {
            var command = body with { Id = declaracaoId };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("declaracoes/{declaracaoId:guid}")]
        [AbacAuthorize("ComercioExterior", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirDeclaracao(Guid declaracaoId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ExcluirDeclaracaoImportacaoCommand(declaracaoId), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("declaracoes/{declaracaoId:guid}/adicoes")]
        [AbacAuthorize("ComercioExterior", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarAdicao(Guid declaracaoId, [FromBody] AdicionarAdicaoImportacaoCommand body, CancellationToken cancellationToken)
        {
            var command = body with { DeclaracaoImportacaoId = declaracaoId };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("declaracoes/{declaracaoId:guid}/adicoes/{adicaoId:guid}")]
        [AbacAuthorize("ComercioExterior", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AlterarAdicao(Guid declaracaoId, Guid adicaoId, [FromBody] AlterarAdicaoImportacaoCommand body, CancellationToken cancellationToken)
        {
            var command = body with { DeclaracaoImportacaoId = declaracaoId, AdicaoId = adicaoId };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("declaracoes/{declaracaoId:guid}/adicoes/{adicaoId:guid}")]
        [AbacAuthorize("ComercioExterior", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirAdicao(Guid declaracaoId, Guid adicaoId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ExcluirAdicaoImportacaoCommand(declaracaoId, adicaoId), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
