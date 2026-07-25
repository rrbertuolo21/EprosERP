using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Application.Queries;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// PRJ-RST (Planejamento e Rastreamento). Tarefas operacionais, estagios (quadro),
    /// progresso hierarquico e dependencias. ABAC nega por padrao (submodulo novo desabilitado).
    /// </summary>
    [ApiController]
    [Route("api/v1/projetos/rastreamento")]
    [Produces("application/json")]
    public class ProjetosRastreamentoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProjetosRastreamentoController(IMediator mediator) => _mediator = mediator;

        [HttpPost("estagios")]
        [AbacAuthorize("ProjetosRastreamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarEstagio([FromBody] CriarEstagioTarefaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("tarefas")]
        [AbacAuthorize("ProjetosRastreamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarTarefa([FromBody] CriarTarefaProjetoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("tarefas/{id:guid}/mover")]
        [AbacAuthorize("ProjetosRastreamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Mover(Guid id, [FromBody] MoverTarefaRequest request)
        {
            var result = await _mediator.Send(new MoverTarefaQuadroCommand(id, request.EstagioId, request.NovaOrdem));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record MoverTarefaRequest(Guid EstagioId, int NovaOrdem);

        [HttpPost("tarefas/{id:guid}/progresso")]
        [AbacAuthorize("ProjetosRastreamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AtualizarProgresso(Guid id, [FromBody] AtualizarProgressoTarefaRequest request)
        {
            var result = await _mediator.Send(new AtualizarProgressoTarefaProjetoCommand(id, request.PercentualConcluido));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record AtualizarProgressoTarefaRequest(decimal PercentualConcluido);

        [HttpPost("tarefas/{id:guid}/concluir")]
        [AbacAuthorize("ProjetosRastreamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Concluir(Guid id)
        {
            var result = await _mediator.Send(new ConcluirTarefaProjetoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("dependencias")]
        [AbacAuthorize("ProjetosRastreamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarDependencia([FromBody] CriarDependenciaTarefaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("tarefas/projeto/{projetoId:guid}")]
        [AbacAuthorize("ProjetosRastreamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarTarefas(Guid projetoId)
        {
            var result = await _mediator.Send(new ObterTarefasPorProjetoQuery(projetoId));
            return Ok(result);
        }

        [HttpGet("tarefas/{id:guid}")]
        [AbacAuthorize("ProjetosRastreamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterTarefa(Guid id)
        {
            var result = await _mediator.Send(new ObterTarefaPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }
}
