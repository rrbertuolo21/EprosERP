using System;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Aplicativo.Application.Plataforma.Sdk;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers.Plataforma
{
    /// <summary>
    /// PLT · SDK/EXTENSÕES (recorte PD-09) — chaves de API públicas com escopos. Controller fino.
    /// O segredo só aparece na resposta de criação. ABAC desabilitado por padrão.
    /// </summary>
    [ApiController]
    [Route("api/v1/plt/sdk")]
    [Produces("application/json")]
    public class SdkController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SdkController(IMediator mediator) => _mediator = mediator;

        private ActionResult<CommandResult> Resultado(CommandResult r) => r.Sucesso ? Ok(r) : UnprocessableEntity(r);

        [HttpGet("escopos")]
        [AbacAuthorize("SdkChavesApi", "Ler")]
        public async Task<IActionResult> Escopos()
            => Ok(await _mediator.Send(new ObterEscoposDisponiveisQuery()));

        [HttpGet("chaves")]
        [AbacAuthorize("SdkChavesApi", "Ler")]
        public async Task<IActionResult> ListarChaves([FromQuery] bool apenasAtivas = false)
            => Ok(await _mediator.Send(new ObterChavesApiQuery(apenasAtivas)));

        [HttpPost("chaves")]
        [AbacAuthorize("SdkChavesApi", "Gerar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> GerarChave([FromBody] GerarChaveApiCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("chaves/revogar")]
        [AbacAuthorize("SdkChavesApi", "Revogar")]
        public async Task<ActionResult<CommandResult>> RevogarChave([FromBody] RevogarChaveApiCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("chaves/validar")]
        [AbacAuthorize("SdkChavesApi", "Validar")]
        public async Task<IActionResult> Validar([FromBody] ValidarChaveApiQuery query)
            => Ok(await _mediator.Send(query));
    }
}
