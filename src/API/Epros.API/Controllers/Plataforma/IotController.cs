using System;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Aplicativo.Application.Plataforma.Iot;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers.Plataforma
{
    /// <summary>
    /// PLT · IoT — dispositivos/sensores e ingestão de leitura (série temporal). Leitura fora da faixa
    /// emite condição p/ Manutenção preditiva (não é ordem). Controller fino. ABAC desabilitado por padrão.
    /// </summary>
    [ApiController]
    [Route("api/v1/plt/iot")]
    [Produces("application/json")]
    public class IotController : ControllerBase
    {
        private readonly IMediator _mediator;
        public IotController(IMediator mediator) => _mediator = mediator;

        private ActionResult<CommandResult> Resultado(CommandResult r) => r.Sucesso ? Ok(r) : UnprocessableEntity(r);

        [HttpGet("dispositivos")]
        [AbacAuthorize("Iot", "Ler")]
        public async Task<IActionResult> ListarDispositivos([FromQuery] bool apenasAtivos = false)
            => Ok(await _mediator.Send(new ObterDispositivosIotQuery(apenasAtivos)));

        [HttpPost("dispositivos")]
        [AbacAuthorize("Iot", "Registrar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarDispositivo([FromBody] RegistrarDispositivoIotCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("dispositivos/vincular-ativo")]
        [AbacAuthorize("Iot", "Editar")]
        public async Task<ActionResult<CommandResult>> VincularAtivo([FromBody] VincularDispositivoAtivoCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("dispositivos/{dispositivoId}/sensores")]
        [AbacAuthorize("Iot", "Ler")]
        public async Task<IActionResult> ListarSensores(Guid dispositivoId)
            => Ok(await _mediator.Send(new ObterSensoresQuery(dispositivoId)));

        [HttpPost("sensores")]
        [AbacAuthorize("Iot", "Registrar")]
        public async Task<ActionResult<CommandResult>> RegistrarSensor([FromBody] RegistrarSensorIotCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("leituras")]
        [AbacAuthorize("Iot", "Ingerir")]
        public async Task<ActionResult<CommandResult>> Ingerir([FromBody] IngestarLeituraCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("sensores/{sensorId}/leituras")]
        [AbacAuthorize("Iot", "Ler")]
        public async Task<IActionResult> ListarLeituras(Guid sensorId, [FromQuery] DateTime? desde, [FromQuery] bool apenasForaFaixa = false)
            => Ok(await _mediator.Send(new ObterLeiturasQuery(sensorId, desde, apenasForaFaixa)));

        [HttpGet("leituras/vencidas")]
        [AbacAuthorize("Iot", "Ler")]
        public async Task<IActionResult> LeiturasVencidas()
            => Ok(await _mediator.Send(new ObterLeiturasVencidasQuery()));
    }
}
