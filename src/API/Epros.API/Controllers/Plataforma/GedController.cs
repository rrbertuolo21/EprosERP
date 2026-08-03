using System;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Aplicativo.Application.Plataforma.Ged;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers.Plataforma
{
    /// <summary>
    /// PLT · GED (T10) — repositório documental canônico único. Controller fino (apenas MediatR).
    /// Toda rota exige ABAC (recurso, ação); o submódulo sobe DESABILITADO (nenhuma permissão semeada →
    /// ABAC nega por padrão — feature flag por permissão).
    /// </summary>
    [ApiController]
    [Route("api/v1/plt/ged")]
    [Produces("application/json")]
    public class GedController : ControllerBase
    {
        private readonly IMediator _mediator;
        public GedController(IMediator mediator) => _mediator = mediator;

        private ActionResult<CommandResult> Resultado(CommandResult r) => r.Sucesso ? Ok(r) : UnprocessableEntity(r);

        [HttpGet("documentos")]
        [AbacAuthorize("GedDocumentos", "Ler")]
        public async Task<IActionResult> Listar([FromQuery] string? tipo, [FromQuery] string? entidadeTipo, [FromQuery] string? entidadeId)
            => Ok(await _mediator.Send(new ObterDocumentosGedQuery(tipo, entidadeTipo, entidadeId)));

        [HttpGet("documentos/{id}")]
        [AbacAuthorize("GedDocumentos", "Ler")]
        public async Task<IActionResult> ObterPorId(Guid id)
            => Ok(await _mediator.Send(new ObterDocumentoGedPorIdQuery(id)));

        [HttpGet("documentos/busca")]
        [AbacAuthorize("GedDocumentos", "Ler")]
        public async Task<IActionResult> Buscar([FromQuery] string termo)
            => Ok(await _mediator.Send(new BuscarDocumentosGedQuery(termo)));

        [HttpPost("documentos")]
        [AbacAuthorize("GedDocumentos", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Registrar([FromBody] RegistrarDocumentoGedCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("documentos/versoes")]
        [AbacAuthorize("GedDocumentos", "Editar")]
        public async Task<ActionResult<CommandResult>> NovaVersao([FromBody] RegistrarNovaVersaoDocumentoGedCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("documentos/vinculos")]
        [AbacAuthorize("GedDocumentos", "Editar")]
        public async Task<ActionResult<CommandResult>> Vincular([FromBody] VincularDocumentoGedCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("hashes-bloqueados")]
        [AbacAuthorize("GedModeracao", "Bloquear")]
        public async Task<ActionResult<CommandResult>> BloquearHash([FromBody] BloquearHashGedCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("retencao/politicas")]
        [AbacAuthorize("GedRetencao", "Definir")]
        public async Task<ActionResult<CommandResult>> DefinirRetencao([FromBody] DefinirPoliticaRetencaoGedCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("retencao/vencidos")]
        [AbacAuthorize("GedRetencao", "Ler")]
        public async Task<IActionResult> RetencaoVencida()
            => Ok(await _mediator.Send(new ObterDocumentosRetencaoVencidaQuery()));
    }
}
