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
    /// MAN-PDT — Manutencao Preditiva (monitoramento por condicao). Controller fino (apenas MediatR).
    /// Protegido por ABAC (EF secao seguranca). Filtro de tenant e global (ContextBase).
    /// Submodulo novo sobe desabilitado: ABAC nega por padrao; nenhuma permissao e semeada.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/manutencao/preditiva")]
    public class ManutencaoPreditivaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ManutencaoPreditivaController(IMediator mediator) => _mediator = mediator;

        [HttpGet("monitoramentos")]
        [AbacAuthorize("ManutencaoPreditiva", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListarMonitoramentosPreditivosQuery(pagina, tamanhoPagina), ct);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("monitoramentos/{id:guid}")]
        [AbacAuthorize("ManutencaoPreditiva", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ObterMonitoramentoPreditivoPorIdQuery(id), ct);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpGet("alarmes")]
        [AbacAuthorize("ManutencaoPreditiva", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarAlarmes([FromQuery] Guid? monitoramentoId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListarAlarmesPreditivosQuery(monitoramentoId, pagina, tamanhoPagina), ct);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpPost("monitoramentos")]
        [AbacAuthorize("ManutencaoPreditiva", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarMonitoramentoPreditivoCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("monitoramentos/{id:guid}/pontos-medicao")]
        [AbacAuthorize("ManutencaoPreditiva", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarPonto(Guid id, [FromBody] AdicionarPontoMedicaoCommand command, CancellationToken ct)
        {
            if (id != command.MonitoramentoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("pontos-medicao/{pontoId:guid}/regras")]
        [AbacAuthorize("ManutencaoPreditiva", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarRegra(Guid pontoId, [FromBody] AdicionarRegraMonitoramentoCommand command, CancellationToken ct)
        {
            if (pontoId != command.PontoMedicaoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("pontos-medicao/{pontoId:guid}/leituras")]
        [AbacAuthorize("ManutencaoPreditiva", "Registrar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RegistrarLeitura(Guid pontoId, [FromBody] RegistrarLeituraCondicaoCommand command, CancellationToken ct)
        {
            if (pontoId != command.PontoMedicaoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("alarmes")]
        [AbacAuthorize("ManutencaoPreditiva", "Registrar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DispararAlarme([FromBody] DispararAlarmePreditivoCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("alarmes/{alarmeId:guid}/converter-ordem")]
        [AbacAuthorize("ManutencaoPreditiva", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ConverterEmOrdem(Guid alarmeId, [FromBody] ConverterAlarmeEmOrdemCommand command, CancellationToken ct)
        {
            if (alarmeId != command.AlarmeId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("alarmes/{alarmeId:guid}/descartar")]
        [AbacAuthorize("ManutencaoPreditiva", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DescartarAlarme(Guid alarmeId, [FromBody] DescartarAlarmePreditivoCommand command, CancellationToken ct)
        {
            if (alarmeId != command.AlarmeId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("monitoramentos/{id:guid}/submeter")]
        [AbacAuthorize("ManutencaoPreditiva", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Submeter(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SubmeterMonitoramentoPreditivoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("monitoramentos/{id:guid}/aprovar")]
        [AbacAuthorize("ManutencaoPreditiva", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Aprovar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new AprovarMonitoramentoPreditivoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("monitoramentos/{id:guid}/suspender")]
        [AbacAuthorize("ManutencaoPreditiva", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Suspender(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SuspenderMonitoramentoPreditivoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("monitoramentos/{id:guid}/encerrar")]
        [AbacAuthorize("ManutencaoPreditiva", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Encerrar(Guid id, [FromBody] EncerrarMonitoramentoPreditivoCommand command, CancellationToken ct)
        {
            if (id != command.MonitoramentoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
