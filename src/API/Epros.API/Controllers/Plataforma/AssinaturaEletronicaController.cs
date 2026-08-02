using System;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Aplicativo.Application.Plataforma.Assinatura;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers.Plataforma
{
    /// <summary>
    /// PLT · ASSINATURA ELETRÔNICA ICP (PD-02 / T10) — documental, construída do zero. Controller fino.
    /// Rotas internas exigem ABAC; a rota pública de assinatura por link é anônima mas exige token válido
    /// não revogado (o próprio token é a credencial). Sem provedor ICP configurado, o documento
    /// permanece pendente — a validade jurídica nunca é forjada.
    /// </summary>
    [ApiController]
    [Route("api/v1/plt/assinatura")]
    [Produces("application/json")]
    public class AssinaturaEletronicaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AssinaturaEletronicaController(IMediator mediator) => _mediator = mediator;

        private ActionResult<CommandResult> Resultado(CommandResult r) => r.Sucesso ? Ok(r) : UnprocessableEntity(r);

        [HttpGet("solicitacoes")]
        [AbacAuthorize("AssinaturaEletronica", "Ler")]
        public async Task<IActionResult> Listar([FromQuery] string? estado, [FromQuery] Guid? documentoId)
            => Ok(await _mediator.Send(new ObterSolicitacoesAssinaturaQuery(estado, documentoId)));

        [HttpGet("solicitacoes/{id}")]
        [AbacAuthorize("AssinaturaEletronica", "Ler")]
        public async Task<IActionResult> ObterPorId(Guid id)
            => Ok(await _mediator.Send(new ObterSolicitacaoAssinaturaPorIdQuery(id)));

        [HttpPost("solicitacoes")]
        [AbacAuthorize("AssinaturaEletronica", "Solicitar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Solicitar([FromBody] SolicitarAssinaturasCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("assinar")]
        [AbacAuthorize("AssinaturaEletronica", "Assinar")]
        public async Task<ActionResult<CommandResult>> Assinar([FromBody] RegistrarAssinaturaCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("recusar")]
        [AbacAuthorize("AssinaturaEletronica", "Assinar")]
        public async Task<ActionResult<CommandResult>> Recusar([FromBody] RecusarAssinaturaCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("links/revogar")]
        [AbacAuthorize("AssinaturaEletronica", "Revogar")]
        public async Task<ActionResult<CommandResult>> RevogarLink([FromBody] RevogarLinkPublicoCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("politicas")]
        [AbacAuthorize("AssinaturaEletronica", "Configurar")]
        public async Task<ActionResult<CommandResult>> DefinirPolitica([FromBody] DefinirPoliticaAssinaturaCommand command)
            => Resultado(await _mediator.Send(command));

        /// <summary>Assinatura pública por link (signatário externo). Token é a credencial.</summary>
        [HttpPost("publico/assinar")]
        [AllowAnonymous]
        public async Task<ActionResult<CommandResult>> AssinarPublico([FromBody] AssinarPorLinkPublicoCommand command)
            => Resultado(await _mediator.Send(command));
    }
}
