using System;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>Governança de upgrade/versão do Super Admin (APP-TEN-010). Maker-checker + rollback.</summary>
    [ApiController]
    [Route("api/v1/super-admin/upgrades")]
    [Produces("application/json")]
    public class GovernancaVersaoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GovernancaVersaoController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> Listar()
            => Ok(await _mediator.Send(new ListarSolicitacoesUpgradeQuery()));

        [HttpPost]
        public async Task<ActionResult<CommandResult>> Solicitar([FromBody] SolicitarUpgradeVersaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{solicitacaoId:guid}/aprovar")]
        public async Task<ActionResult<CommandResult>> Aprovar([FromRoute] Guid solicitacaoId, [FromBody] AprovarUpgradeVersaoCommand? body)
        {
            var result = await _mediator.Send(new AprovarUpgradeVersaoCommand(solicitacaoId, body?.Comentario));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{solicitacaoId:guid}/rejeitar")]
        public async Task<ActionResult<CommandResult>> Rejeitar([FromRoute] Guid solicitacaoId, [FromBody] RejeitarUpgradeVersaoCommand? body)
        {
            var result = await _mediator.Send(new RejeitarUpgradeVersaoCommand(solicitacaoId, body?.Comentario));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{solicitacaoId:guid}/executar")]
        public async Task<ActionResult<CommandResult>> Executar([FromRoute] Guid solicitacaoId)
        {
            var result = await _mediator.Send(new ExecutarUpgradeVersaoCommand(solicitacaoId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{solicitacaoId:guid}/rollback")]
        public async Task<ActionResult<CommandResult>> Rollback([FromRoute] Guid solicitacaoId, [FromBody] AplicarRollbackUpgradeCommand? body)
        {
            var result = await _mediator.Send(new AplicarRollbackUpgradeCommand(solicitacaoId, body?.Log));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
