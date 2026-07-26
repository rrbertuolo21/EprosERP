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
    /// PRJ-REC (Gestao de Recursos). Apontamentos (timesheet) e alocacoes de recurso.
    /// ABAC nega por padrao (submodulo novo sobe desabilitado).
    /// </summary>
    [ApiController]
    [Route("api/v1/projetos/recursos")]
    [Produces("application/json")]
    public class ProjetosRecursosController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProjetosRecursosController(IMediator mediator) => _mediator = mediator;

        [HttpPost("apontamentos")]
        [AbacAuthorize("ProjetosRecursos", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarApontamento([FromBody] RegistrarApontamentoRequest request)
        {
            var result = await _mediator.Send(new RegistrarApontamentoCommand(
                request.UsuarioId, request.ProjetoId, request.TarefaId, request.Data,
                request.Horas, request.Minutos, request.Notas, request.Tipo));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record RegistrarApontamentoRequest(
            Guid? UsuarioId, Guid? ProjetoId, Guid? TarefaId, DateTime Data,
            int Horas, int Minutos, string? Notas, ETimesheetTipo Tipo);

        [HttpPost("apontamentos/{id:guid}/submeter")]
        [AbacAuthorize("ProjetosRecursos", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> SubmeterApontamento(Guid id)
        {
            var result = await _mediator.Send(new SubmeterApontamentoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("apontamentos/{id:guid}/aprovar")]
        [AbacAuthorize("ProjetosRecursos", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> AprovarApontamento(Guid id)
        {
            var result = await _mediator.Send(new AprovarApontamentoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("alocacoes")]
        [AbacAuthorize("ProjetosRecursos", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarAlocacao([FromBody] CriarAlocacaoRecursoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("apontamentos")]
        [AbacAuthorize("ProjetosRecursos", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarApontamentos([FromQuery] Guid? projetoId, [FromQuery] Guid? usuarioId)
        {
            var result = await _mediator.Send(new ObterApontamentosQuery(projetoId, usuarioId));
            return Ok(result);
        }

        [HttpGet("alocacoes/projeto/{projetoId:guid}")]
        [AbacAuthorize("ProjetosRecursos", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarAlocacoes(Guid projetoId)
        {
            var result = await _mediator.Send(new ObterAlocacoesRecursoQuery(projetoId));
            return Ok(result);
        }
    }
}
