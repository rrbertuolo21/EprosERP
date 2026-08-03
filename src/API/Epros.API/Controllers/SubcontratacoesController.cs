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
    /// Subcontratação (EST-SUB). Controller fino: apenas MediatR. Protegido por ABAC (estoque.sub.*).
    /// Submódulo sobe desabilitado — ABAC nega por padrão. Filtro de tenant é global. Modelo proposto por
    /// autoria; documento fiscal/CFOP vêm do motor fiscal (SUB-008), nunca calculados aqui.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/estoque-subcontratacoes")]
    public class SubcontratacoesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubcontratacoesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AbacAuthorize("EstoqueSub", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] Guid? fornecedorId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarSubOrdensQuery(fornecedorId, pagina, tamanhoPagina), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("EstoqueSub", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterSubOrdemPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("EstoqueSub", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarSubOrdemCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/enviar")]
        [AbacAuthorize("EstoqueSub", "Enviar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Enviar(Guid id, [FromBody] RegistrarSubEnvioCommand command, CancellationToken cancellationToken)
        {
            if (id != command.OrdemId)
                return BadRequest("O ID da rota não corresponde à ordem do corpo da requisição.");

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/retornar")]
        [AbacAuthorize("EstoqueSub", "Retornar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Retornar(Guid id, [FromBody] RegistrarSubRetornoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.OrdemId)
                return BadRequest("O ID da rota não corresponde à ordem do corpo da requisição.");

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("saldos-terceiros")]
        [AbacAuthorize("EstoqueSub", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> SaldosTerceiros([FromQuery] Guid? fornecedorId, [FromQuery] Guid? produtoId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ListarSubSaldosTerceiroQuery(fornecedorId, produtoId), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>SUB-009 — cobra o serviço de beneficiamento; gera a compra do serviço (via evento).</summary>
        [HttpPost("{id:guid}/servico-cobranca")]
        [AbacAuthorize("EstoqueSub", "Cobrar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RegistrarServicoCobranca(Guid id, [FromBody] RegistrarSubServicoCobrancaCommand body, CancellationToken cancellationToken)
        {
            var command = body with { OrdemId = id };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>SUB-008 — registra o documento fiscal de remessa/retorno; CFOP parametrizado (valida-contador).</summary>
        [HttpPost("{id:guid}/documentos-fiscais")]
        [AbacAuthorize("EstoqueSub", "Faturar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RegistrarDocumentoFiscal(Guid id, [FromBody] RegistrarSubDocumentoFiscalCommand body, CancellationToken cancellationToken)
        {
            var command = body with { OrdemId = id };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
