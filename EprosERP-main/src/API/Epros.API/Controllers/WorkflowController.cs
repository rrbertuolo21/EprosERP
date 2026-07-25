using System;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.API.Security;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>Motor de Workflow genérico (PLT-WF). Aprovações por alçada, tarefas humanas e histórico.</summary>
    /// <remarks>
    /// Autorização por papel (ABAC): recurso "Workflow" com ações Ler/Criar/Aprovar/Gerenciar.
    /// Submódulo sobe desabilitado — a permissão não é semeada, então o ABAC nega por padrão até a
    /// liberação explícita por tenant. [Origem: EF WORKFLOW §7.1 / WF-CA-002..006]
    /// </remarks>
    [ApiController]
    [Route("api/v1/workflow")]
    [Produces("application/json")]
    public class WorkflowController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WorkflowController(IMediator mediator) => _mediator = mediator;

        // ---------- Definições ----------

        [HttpGet("definicoes")]
        [AbacAuthorize("Workflow", "Ler")]
        public async Task<IActionResult> ListarDefinicoes([FromQuery] string? modulo)
            => Ok(await _mediator.Send(new ListarWfDefinicoesQuery(modulo)));

        [HttpPost("definicoes")]
        [AbacAuthorize("Workflow", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> CriarDefinicao([FromBody] CriarWfDefinicaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("definicoes/estados")]
        [AbacAuthorize("Workflow", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> AdicionarEstado([FromBody] AdicionarWfEstadoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("definicoes/transicoes")]
        [AbacAuthorize("Workflow", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> AdicionarTransicao([FromBody] AdicionarWfTransicaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("definicoes/{definicaoId:guid}/ativar")]
        [AbacAuthorize("Workflow", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> AtivarDefinicao([FromRoute] Guid definicaoId)
        {
            var result = await _mediator.Send(new AtivarWfDefinicaoCommand(definicaoId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Instâncias ----------

        [HttpGet("instancias")]
        [AbacAuthorize("Workflow", "Ler")]
        public async Task<IActionResult> ListarInstancias([FromQuery] EWfInstanciaStatus? status, [FromQuery] string? entidadeTipo, [FromQuery] Guid? responsavelUsuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, [FromQuery] DateTime? desde = null, [FromQuery] DateTime? ate = null)
            => Ok(await _mediator.Send(new ListarWfInstanciasQuery(status, entidadeTipo, responsavelUsuarioId, pagina, tamanhoPagina, desde, ate)));

        [HttpGet("instancias/{instanciaId:guid}")]
        [AbacAuthorize("Workflow", "Ler")]
        public async Task<IActionResult> ObterInstancia([FromRoute] Guid instanciaId)
        {
            var result = await _mediator.Send(new ObterWfInstanciaPorIdQuery(instanciaId));
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("instancias/{instanciaId:guid}/historico")]
        [AbacAuthorize("Workflow", "Ler")]
        public async Task<IActionResult> ListarHistorico([FromRoute] Guid instanciaId)
            => Ok(await _mediator.Send(new ListarWfHistoricoQuery(instanciaId)));

        [HttpPost("instancias")]
        [AbacAuthorize("Workflow", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarInstancia([FromBody] CriarWfInstanciaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("instancias/{instanciaId:guid}/transicionar")]
        [AbacAuthorize("Workflow", "Aprovar")]
        public async Task<ActionResult<CommandResult>> Transicionar([FromRoute] Guid instanciaId, [FromBody] TransicionarWfInstanciaCommand command)
        {
            if (instanciaId != command.InstanciaId) return BadRequest(CommandResult.Falha("O id da rota difere do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Tarefas ----------

        [HttpGet("tarefas")]
        [AbacAuthorize("Workflow", "Ler")]
        public async Task<IActionResult> ListarTarefas([FromQuery] Guid? instanciaId, [FromQuery] Guid? responsavelUsuarioId)
            => Ok(await _mediator.Send(new ListarWfTarefasQuery(instanciaId, responsavelUsuarioId)));

        [HttpPost("tarefas")]
        [AbacAuthorize("Workflow", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> CriarTarefa([FromBody] CriarWfTarefaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("tarefas/{tarefaId:guid}/concluir")]
        [AbacAuthorize("Workflow", "Aprovar")]
        public async Task<ActionResult<CommandResult>> ConcluirTarefa([FromRoute] Guid tarefaId)
        {
            var result = await _mediator.Send(new ConcluirWfTarefaCommand(tarefaId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Solicitações ----------

        [HttpPost("solicitacoes")]
        [AbacAuthorize("Workflow", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarSolicitacao([FromBody] CriarWfSolicitacaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("solicitacoes/{solicitacaoId:guid}/decidir")]
        [AbacAuthorize("Workflow", "Aprovar")]
        public async Task<ActionResult<CommandResult>> DecidirSolicitacao([FromRoute] Guid solicitacaoId, [FromBody] DecidirWfSolicitacaoCommand command)
        {
            if (solicitacaoId != command.SolicitacaoId) return BadRequest(CommandResult.Falha("O id da rota difere do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Agendamentos (§7.4) ----------

        [HttpGet("agendamentos")]
        [AbacAuthorize("Workflow", "Ler")]
        public async Task<IActionResult> ListarAgendamentos([FromQuery] bool? ativo)
            => Ok(await _mediator.Send(new ListarWfAgendamentosQuery(ativo)));

        [HttpPost("agendamentos")]
        [AbacAuthorize("Workflow", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> CriarAgendamento([FromBody] CriarWfAgendamentoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("agendamentos/{agendamentoId:guid}")]
        [AbacAuthorize("Workflow", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> AlterarAgendamento([FromRoute] Guid agendamentoId, [FromBody] AlterarWfAgendamentoCommand command)
        {
            if (agendamentoId != command.AgendamentoId) return BadRequest(CommandResult.Falha("O id da rota difere do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("agendamentos/{agendamentoId:guid}/ativar")]
        [AbacAuthorize("Workflow", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> AtivarAgendamento([FromRoute] Guid agendamentoId)
        {
            var result = await _mediator.Send(new AtivarWfAgendamentoCommand(agendamentoId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("agendamentos/{agendamentoId:guid}/desativar")]
        [AbacAuthorize("Workflow", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> DesativarAgendamento([FromRoute] Guid agendamentoId)
        {
            var result = await _mediator.Send(new DesativarWfAgendamentoCommand(agendamentoId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("agendamentos/enfileirar")]
        [AbacAuthorize("Workflow", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> EnfileirarJobs()
        {
            var result = await _mediator.Send(new EnfileirarJobsAgendadosCommand());
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Fila de jobs (§7.5 / §13 subpainel) ----------

        [HttpGet("jobs")]
        [AbacAuthorize("Workflow", "Ler")]
        public async Task<IActionResult> ListarJobs([FromQuery] Guid? agendamentoId, [FromQuery] EWfJobStatus? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarWfJobsQuery(agendamentoId, status, pagina, tamanhoPagina)));

        [HttpGet("jobs/{jobId:guid}/tentativas")]
        [AbacAuthorize("Workflow", "Ler")]
        public async Task<IActionResult> ListarTentativas([FromRoute] Guid jobId)
            => Ok(await _mediator.Send(new ListarWfJobTentativasQuery(jobId)));

        [HttpPost("jobs/{jobId:guid}/iniciar")]
        [AbacAuthorize("Workflow", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> IniciarJob([FromRoute] Guid jobId)
        {
            var result = await _mediator.Send(new IniciarWfJobCommand(jobId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("jobs/{jobId:guid}/sucesso")]
        [AbacAuthorize("Workflow", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> ResolverJobSucesso([FromRoute] Guid jobId, [FromBody] string? log)
        {
            var result = await _mediator.Send(new ResolverWfJobSucessoCommand(jobId, log));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("jobs/{jobId:guid}/falha")]
        [AbacAuthorize("Workflow", "Gerenciar")]
        public async Task<ActionResult<CommandResult>> ResolverJobFalha([FromRoute] Guid jobId, [FromBody] string? log)
        {
            var result = await _mediator.Send(new ResolverWfJobFalhaCommand(jobId, log));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Painel gestor (§7.6.5-6) ----------

        [HttpGet("painel")]
        [AbacAuthorize("Workflow", "Ler")]
        public async Task<IActionResult> Painel()
            => Ok(await _mediator.Send(new PainelWorkflowQuery()));
    }
}
