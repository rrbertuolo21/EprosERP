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
    /// MAN-CRV — Confiabilidade e Revisao (FMEA/RCM). Controller fino (apenas MediatR).
    /// Protegido por ABAC (EF secao seguranca). Filtro de tenant e global (ContextBase).
    /// Submodulo novo sobe desabilitado: ABAC nega por padrao; nenhuma permissao e semeada.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/manutencao/confiabilidade")]
    public class ManutencaoConfiabilidadeController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ManutencaoConfiabilidadeController(IMediator mediator) => _mediator = mediator;

        [HttpGet("revisoes")]
        [AbacAuthorize("ManutencaoConfiabilidade", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListarRevisoesConfiabilidadeQuery(pagina, tamanhoPagina), ct);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("revisoes/{id:guid}")]
        [AbacAuthorize("ManutencaoConfiabilidade", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ObterRevisaoConfiabilidadePorIdQuery(id), ct);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("revisoes")]
        [AbacAuthorize("ManutencaoConfiabilidade", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarRevisaoConfiabilidadeCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("revisoes/{id:guid}/modos-falha")]
        [AbacAuthorize("ManutencaoConfiabilidade", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarModoFalha(Guid id, [FromBody] AdicionarModoFalhaCommand command, CancellationToken ct)
        {
            if (id != command.RevisaoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("revisoes/{id:guid}/indicadores")]
        [AbacAuthorize("ManutencaoConfiabilidade", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RegistrarIndicador(Guid id, [FromBody] RegistrarIndicadorConfiabilidadeCommand command, CancellationToken ct)
        {
            if (id != command.RevisaoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("revisoes/{id:guid}/recomendacoes")]
        [AbacAuthorize("ManutencaoConfiabilidade", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RecomendarEstrategia(Guid id, [FromBody] RecomendarEstrategiaCommand command, CancellationToken ct)
        {
            if (id != command.RevisaoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // MAN-CRV D24 — motor: calcula MTTR/MTBF/Disponibilidade lendo Paradas (nao entrada manual).
        [HttpPost("revisoes/{id:guid}/indicadores/calcular")]
        [AbacAuthorize("ManutencaoConfiabilidade", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CalcularIndicadores(Guid id, [FromBody] CalcularIndicadoresConfiabilidadeCommand command, CancellationToken ct)
        {
            if (id != command.RevisaoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("revisoes/{id:guid}/submeter")]
        [AbacAuthorize("ManutencaoConfiabilidade", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Submeter(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SubmeterRevisaoConfiabilidadeCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("revisoes/{id:guid}/aprovar")]
        [AbacAuthorize("ManutencaoConfiabilidade", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Aprovar(Guid id, [FromBody] AprovarRevisaoConfiabilidadeCommand command, CancellationToken ct)
        {
            if (id != command.RevisaoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("revisoes/{id:guid}/rejeitar")]
        [AbacAuthorize("ManutencaoConfiabilidade", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Rejeitar(Guid id, [FromBody] RejeitarRevisaoConfiabilidadeCommand command, CancellationToken ct)
        {
            if (id != command.RevisaoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("revisoes/{id:guid}/suspender")]
        [AbacAuthorize("ManutencaoConfiabilidade", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Suspender(Guid id, [FromBody] SuspenderRevisaoConfiabilidadeCommand command, CancellationToken ct)
        {
            if (id != command.RevisaoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("revisoes/{id:guid}/retomar")]
        [AbacAuthorize("ManutencaoConfiabilidade", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Retomar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new RetomarRevisaoConfiabilidadeCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("revisoes/{id:guid}/encerrar")]
        [AbacAuthorize("ManutencaoConfiabilidade", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Encerrar(Guid id, [FromBody] EncerrarRevisaoConfiabilidadeCommand command, CancellationToken ct)
        {
            if (id != command.RevisaoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
