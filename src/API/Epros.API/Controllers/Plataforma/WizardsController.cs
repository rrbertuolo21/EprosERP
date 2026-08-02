using System;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Aplicativo.Application.Plataforma.Wizards;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers.Plataforma
{
    /// <summary>
    /// PLT · WIZARDS (PD-04) — form dinâmico + wizard multi-etapa + builder. Controller fino.
    /// ABAC desabilitado por padrão. Canal público sanitiza entradas.
    /// </summary>
    [ApiController]
    [Route("api/v1/plt/wizards")]
    [Produces("application/json")]
    public class WizardsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public WizardsController(IMediator mediator) => _mediator = mediator;

        private ActionResult<CommandResult> Resultado(CommandResult r) => r.Sucesso ? Ok(r) : UnprocessableEntity(r);

        [HttpGet("definicoes")]
        [AbacAuthorize("Wizards", "Ler")]
        public async Task<IActionResult> ListarDefinicoes([FromQuery] bool apenasAtivos = false)
            => Ok(await _mediator.Send(new ObterDefinicoesWizardQuery(apenasAtivos)));

        [HttpGet("definicoes/{id}")]
        [AbacAuthorize("Wizards", "Ler")]
        public async Task<IActionResult> ObterDefinicao(Guid id)
            => Ok(await _mediator.Send(new ObterDefinicaoWizardPorIdQuery(id)));

        [HttpPost("definicoes")]
        [AbacAuthorize("Wizards", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarDefinicao([FromBody] CriarDefinicaoWizardCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("etapas")]
        [AbacAuthorize("Wizards", "Editar")]
        public async Task<ActionResult<CommandResult>> AdicionarEtapa([FromBody] AdicionarEtapaWizardCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("campos")]
        [AbacAuthorize("Wizards", "Editar")]
        public async Task<ActionResult<CommandResult>> AdicionarCampo([FromBody] AdicionarCampoWizardCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("definicoes/publicar")]
        [AbacAuthorize("Wizards", "Publicar")]
        public async Task<ActionResult<CommandResult>> Publicar([FromBody] PublicarDefinicaoWizardCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("execucoes")]
        [AbacAuthorize("Wizards", "Executar")]
        public async Task<ActionResult<CommandResult>> Iniciar([FromBody] IniciarExecucaoWizardCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("execucoes/responder")]
        [AbacAuthorize("Wizards", "Executar")]
        public async Task<ActionResult<CommandResult>> Responder([FromBody] ResponderEtapaWizardCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("execucoes")]
        [AbacAuthorize("Wizards", "Ler")]
        public async Task<IActionResult> ListarExecucoes([FromQuery] Guid? definicaoId, [FromQuery] string? status)
            => Ok(await _mediator.Send(new ObterExecucoesWizardQuery(definicaoId, status)));
    }
}
