using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// RH-TLT — Gestao de Talentos. Controller fino (apenas MediatR).
    /// Submodulo sobe desabilitado: ABAC nega por padrao (nenhuma permissao "RhTalentos" semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/rh/talentos")]
    [Produces("application/json")]
    public class RhTalentosController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RhTalentosController(IMediator mediator) => _mediator = mediator;

        [HttpGet("metas")]
        [AbacAuthorize("RhTalentos", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarMetas()
            => Ok(await _mediator.Send(new ListarMetasColaboradorQuery()));

        [HttpPost("metas")]
        [AbacAuthorize("RhTalentos", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarMeta([FromBody] CriarMetaColaboradorCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("licencas")]
        [AbacAuthorize("RhTalentos", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarSolicitacoesLicenca()
            => Ok(await _mediator.Send(new ListarSolicitacoesLicencaQuery()));

        [HttpPost("licencas")]
        [AbacAuthorize("RhTalentos", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarSolicitacaoLicenca([FromBody] RegistrarSolicitacaoLicencaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("licencas/{id}/aprovar")]
        [AbacAuthorize("RhTalentos", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AprovarLicenca(Guid id, [FromBody] AprovarSolicitacaoLicencaBody body)
        {
            var result = await _mediator.Send(new AprovarSolicitacaoLicencaCommand(id, body.AprovadoPorId, body.Comentario));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("licencas/{id}/rejeitar")]
        [AbacAuthorize("RhTalentos", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RejeitarLicenca(Guid id, [FromBody] AprovarSolicitacaoLicencaBody body)
        {
            var result = await _mediator.Send(new RejeitarSolicitacaoLicencaCommand(id, body.AprovadoPorId, body.Comentario));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record AprovarSolicitacaoLicencaBody(Guid AprovadoPorId, string? Comentario);
    }
}
