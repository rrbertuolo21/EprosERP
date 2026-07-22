using System;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>Motor de Workflow genérico (PLT-WF). Aprovações por alçada, tarefas humanas e histórico.</summary>
    [ApiController]
    [Route("api/v1/workflow")]
    [Produces("application/json")]
    public class WorkflowController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WorkflowController(IMediator mediator) => _mediator = mediator;

        // ---------- Definições ----------

        [HttpGet("definicoes")]
        public async Task<IActionResult> ListarDefinicoes([FromQuery] string? modulo)
            => Ok(await _mediator.Send(new ListarWfDefinicoesQuery(modulo)));

        [HttpPost("definicoes")]
        public async Task<ActionResult<CommandResult>> CriarDefinicao([FromBody] CriarWfDefinicaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("definicoes/estados")]
        public async Task<ActionResult<CommandResult>> AdicionarEstado([FromBody] AdicionarWfEstadoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("definicoes/transicoes")]
        public async Task<ActionResult<CommandResult>> AdicionarTransicao([FromBody] AdicionarWfTransicaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("definicoes/{definicaoId:guid}/ativar")]
        public async Task<ActionResult<CommandResult>> AtivarDefinicao([FromRoute] Guid definicaoId)
        {
            var result = await _mediator.Send(new AtivarWfDefinicaoCommand(definicaoId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Instâncias ----------

        [HttpGet("instancias")]
        public async Task<IActionResult> ListarInstancias([FromQuery] EWfInstanciaStatus? status, [FromQuery] string? entidadeTipo, [FromQuery] Guid? responsavelUsuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarWfInstanciasQuery(status, entidadeTipo, responsavelUsuarioId, pagina, tamanhoPagina)));

        [HttpGet("instancias/{instanciaId:guid}")]
        public async Task<IActionResult> ObterInstancia([FromRoute] Guid instanciaId)
        {
            var result = await _mediator.Send(new ObterWfInstanciaPorIdQuery(instanciaId));
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("instancias/{instanciaId:guid}/historico")]
        public async Task<IActionResult> ListarHistorico([FromRoute] Guid instanciaId)
            => Ok(await _mediator.Send(new ListarWfHistoricoQuery(instanciaId)));

        [HttpPost("instancias")]
        public async Task<ActionResult<CommandResult>> CriarInstancia([FromBody] CriarWfInstanciaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("instancias/{instanciaId:guid}/transicionar")]
        public async Task<ActionResult<CommandResult>> Transicionar([FromRoute] Guid instanciaId, [FromBody] TransicionarWfInstanciaCommand command)
        {
            if (instanciaId != command.InstanciaId) return BadRequest(CommandResult.Falha("O id da rota difere do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Tarefas ----------

        [HttpGet("tarefas")]
        public async Task<IActionResult> ListarTarefas([FromQuery] Guid? instanciaId, [FromQuery] Guid? responsavelUsuarioId)
            => Ok(await _mediator.Send(new ListarWfTarefasQuery(instanciaId, responsavelUsuarioId)));

        [HttpPost("tarefas")]
        public async Task<ActionResult<CommandResult>> CriarTarefa([FromBody] CriarWfTarefaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("tarefas/{tarefaId:guid}/concluir")]
        public async Task<ActionResult<CommandResult>> ConcluirTarefa([FromRoute] Guid tarefaId)
        {
            var result = await _mediator.Send(new ConcluirWfTarefaCommand(tarefaId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Solicitações ----------

        [HttpPost("solicitacoes")]
        public async Task<ActionResult<CommandResult>> CriarSolicitacao([FromBody] CriarWfSolicitacaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("solicitacoes/{solicitacaoId:guid}/decidir")]
        public async Task<ActionResult<CommandResult>> DecidirSolicitacao([FromRoute] Guid solicitacaoId, [FromBody] DecidirWfSolicitacaoCommand command)
        {
            if (solicitacaoId != command.SolicitacaoId) return BadRequest(CommandResult.Falha("O id da rota difere do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
