using System;
using System.Collections.Generic;
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
    /// PRD-MRP — MRP / Planejamento Integrado IBP. Controller fino: apenas MediatR.
    /// Protegido por ABAC (recurso "ProducaoMrp"). Sobe DESABILITADO por padrão.
    /// O motor MRP (explosão BOM via BomExplosaoService, netting e sugestões) é disparado por
    /// <see cref="CalcularMrpCommand"/> em POST {id}/calcular; o ciclo de sugestão é exposto abaixo.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/producao/mrp/planejamentos")]
    public class ProducaoMrpController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProducaoMrpController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("ProducaoMrp", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListarMrpPlanejamentosQuery(status, pagina, tamanhoPagina), ct);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ProducaoMrp", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ObterMrpPlanejamentoPorIdQuery(id), ct);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("ProducaoMrp", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarMrpPlanejamentoCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/submeter")]
        [AbacAuthorize("ProducaoMrp", "Submeter")]
        public async Task<ActionResult<CommandResult>> Submeter(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SubmeterMrpPlanejamentoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/aprovar")]
        [AbacAuthorize("ProducaoMrp", "Aprovar")]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new AprovarMrpPlanejamentoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/rejeitar")]
        [AbacAuthorize("ProducaoMrp", "Aprovar")]
        public async Task<ActionResult<CommandResult>> Rejeitar(Guid id, [FromBody] string motivo, CancellationToken ct)
        {
            var result = await _mediator.Send(new RejeitarMrpPlanejamentoCommand(id, motivo), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/inativar")]
        [AbacAuthorize("ProducaoMrp", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> Inativar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new InativarMrpPlanejamentoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/reativar")]
        [AbacAuthorize("ProducaoMrp", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> Reativar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ReativarMrpPlanejamentoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/encerrar")]
        [AbacAuthorize("ProducaoMrp", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> Encerrar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new EncerrarMrpPlanejamentoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ===================== Motor MRP (PD3/PD21) =====================

        /// <summary>Dispara o motor MRP: explode a BOM vigente, faz o netting e persiste necessidades/sugestões.</summary>
        [HttpPost("{id:guid}/calcular")]
        [AbacAuthorize("ProducaoMrp", "Calcular")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Calcular(Guid id, [FromBody] CalcularMrpRequest request, CancellationToken ct)
        {
            var command = new CalcularMrpCommand(id, request.Demandas, request.Parametros);
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record CalcularMrpRequest(
            List<MrpDemandaDto> Demandas,
            List<MrpParametroItemDto>? Parametros = null);

        [HttpPost("sugestoes/{sugestaoId:guid}/submeter")]
        [AbacAuthorize("ProducaoMrp", "Submeter")]
        public async Task<ActionResult<CommandResult>> SubmeterSugestao(Guid sugestaoId, CancellationToken ct)
        {
            var result = await _mediator.Send(new SubmeterSugestaoMrpCommand(sugestaoId), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("sugestoes/{sugestaoId:guid}/aprovar")]
        [AbacAuthorize("ProducaoMrp", "Aprovar")]
        public async Task<ActionResult<CommandResult>> AprovarSugestao(Guid sugestaoId, CancellationToken ct)
        {
            var result = await _mediator.Send(new AprovarSugestaoMrpCommand(sugestaoId), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("sugestoes/{sugestaoId:guid}/converter")]
        [AbacAuthorize("ProducaoMrp", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> ConverterSugestao(Guid sugestaoId, [FromBody] ConverterSugestaoRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new ConverterSugestaoMrpCommand(sugestaoId, request.DocumentoGeradoId), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record ConverterSugestaoRequest(Guid DocumentoGeradoId);

        [HttpPost("sugestoes/{sugestaoId:guid}/cancelar")]
        [AbacAuthorize("ProducaoMrp", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> CancelarSugestao(Guid sugestaoId, [FromBody] string motivo, CancellationToken ct)
        {
            var result = await _mediator.Send(new CancelarSugestaoMrpCommand(sugestaoId, motivo), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
