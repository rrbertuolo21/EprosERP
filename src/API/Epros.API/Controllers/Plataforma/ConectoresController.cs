using System;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Aplicativo.Application.Plataforma.Conectores;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers.Plataforma
{
    /// <summary>
    /// PLT · CONECTORES/WEBHOOKS (ED-08) — framework geral de webhook. Controller fino. Segredo no
    /// cofre (T5); eventos validados contra o catálogo (T2); entrega com retry/dead-letter (TC-03).
    /// ABAC desabilitado por padrão.
    /// </summary>
    [ApiController]
    [Route("api/v1/plt/conectores")]
    [Produces("application/json")]
    public class ConectoresController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ConectoresController(IMediator mediator) => _mediator = mediator;

        private ActionResult<CommandResult> Resultado(CommandResult r) => r.Sucesso ? Ok(r) : UnprocessableEntity(r);

        [HttpGet("eventos-publicaveis")]
        [AbacAuthorize("ConectoresWebhook", "Ler")]
        public async Task<IActionResult> EventosPublicaveis()
            => Ok(await _mediator.Send(new ObterEventosPublicaveisQuery()));

        [HttpGet("endpoints")]
        [AbacAuthorize("ConectoresWebhook", "Ler")]
        public async Task<IActionResult> ListarEndpoints([FromQuery] bool apenasAtivos = false)
            => Ok(await _mediator.Send(new ObterEndpointsWebhookQuery(apenasAtivos)));

        [HttpPost("endpoints")]
        [AbacAuthorize("ConectoresWebhook", "Registrar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarEndpoint([FromBody] RegistrarEndpointWebhookCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPut("endpoints")]
        [AbacAuthorize("ConectoresWebhook", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarEndpoint([FromBody] AtualizarEndpointWebhookCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("publicar")]
        [AbacAuthorize("ConectoresWebhook", "Publicar")]
        public async Task<ActionResult<CommandResult>> Publicar([FromBody] PublicarEventoWebhookCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("entregas/processar")]
        [AbacAuthorize("ConectoresWebhook", "Processar")]
        public async Task<ActionResult<CommandResult>> ProcessarEntrega([FromBody] ProcessarEntregaWebhookCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("entregas")]
        [AbacAuthorize("ConectoresWebhook", "Ler")]
        public async Task<IActionResult> ListarEntregas([FromQuery] Guid? endpointId, [FromQuery] string? status)
            => Ok(await _mediator.Send(new ObterEntregasWebhookQuery(endpointId, status)));
    }
}
