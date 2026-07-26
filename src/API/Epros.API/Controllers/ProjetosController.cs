using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/projetos")]
    [Produces("application/json")]
    public class ProjetosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjetosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarProjeto([FromBody] CriarProjetoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("{id}/wbs")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarWbsItem(Guid id, [FromBody] AdicionarWbsItemRequest request)
        {
            var command = new AdicionarWbsItemCommand(
                id,
                request.Nome,
                request.Descricao,
                request.DataInicio,
                request.DataTermino,
                request.PesoPonderado
            );
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        public record AdicionarWbsItemRequest(
            string Nome,
            string Descricao,
            DateTime DataInicio,
            DateTime DataTermino,
            decimal PesoPonderado
        );

        [HttpPost("{id}/alocacoes")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AlocarRecurso(Guid id, [FromBody] AlocarRecursoRequest request)
        {
            var command = new AlocarRecursoCommand(
                id,
                request.ColaboradorId,
                request.Funcao,
                request.CustoHora,
                request.HorasPlanejadas
            );
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        public record AlocarRecursoRequest(
            Guid ColaboradorId,
            string Funcao,
            decimal CustoHora,
            decimal HorasPlanejadas
        );

        [HttpPost("{id}/wbs/{wbsItemId}/progresso")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AtualizarProgressoTarefa(Guid id, Guid wbsItemId, [FromBody] AtualizarProgressoTarefaRequest request)
        {
            var command = new AtualizarProgressoTarefaCommand(
                id,
                wbsItemId,
                request.PercentualConclusao
            );
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        public record AtualizarProgressoTarefaRequest(decimal PercentualConclusao);

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarProjetos()
        {
            var result = await _mediator.Send(new ObterProjetosQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterProjeto(Guid id)
        {
            var result = await _mediator.Send(new ObterProjetoPorIdQuery(id));
            if (!result.Sucesso) return NotFound(result);
            return Ok(result);
        }
    }
}
