using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Application.Queries;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>
    /// MAN-PAR — Gestao de Paradas. Controller fino. ABAC nega por padrao.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/manutencao/paradas")]
    public class ManutencaoParadasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ManutencaoParadasController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("ManutencaoParadas", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListarParadasQuery(pagina, tamanhoPagina), ct);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ManutencaoParadas", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ObterParadaPorIdQuery(id), ct);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("ManutencaoParadas", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Registrar([FromBody] RegistrarParadaCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/finalizar")]
        [AbacAuthorize("ManutencaoParadas", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Finalizar(Guid id, [FromBody] FinalizarParadaCommand command, CancellationToken ct)
        {
            if (id != command.ParadaId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("motivos")]
        [AbacAuthorize("ManutencaoParadas", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CriarMotivo([FromBody] CriarMotivoParadaCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        // T5 — gera a OS corretiva canonica (man_trb_ordem_servico, origem=Corretiva) da parada.
        [HttpPost("{id:guid}/gerar-os-corretiva")]
        [AbacAuthorize("ManutencaoParadas", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GerarOsCorretiva(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GerarOsCorretivaParadaCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // MAN-PAR D16 — motor de indicadores (MTTR/MTBF/Disponibilidade) sob demanda.
        [HttpPost("{id:guid}/indicadores/calcular")]
        [AbacAuthorize("ManutencaoParadas", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CalcularIndicadores(Guid id, [FromBody] CalcularIndicadoresParadaCommand command, CancellationToken ct)
        {
            if (id != command.ParadaId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
