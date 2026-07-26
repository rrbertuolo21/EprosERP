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
    /// RH-LMS — Treinamento e Certificacoes. Controller fino (apenas MediatR).
    /// Submodulo sobe desabilitado: ABAC nega por padrao (nenhuma permissao "RhTreinamento" semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/rh/treinamento")]
    [Produces("application/json")]
    public class RhTreinamentoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RhTreinamentoController(IMediator mediator) => _mediator = mediator;

        [HttpGet("treinamentos")]
        [AbacAuthorize("RhTreinamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarTreinamentos()
            => Ok(await _mediator.Send(new ListarTreinamentosQuery()));

        [HttpPost("treinamentos")]
        [AbacAuthorize("RhTreinamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarTreinamento([FromBody] CriarTreinamentoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("tarefas/{id}/concluir")]
        [AbacAuthorize("RhTreinamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ConcluirTarefa(Guid id)
        {
            var result = await _mediator.Send(new ConcluirTarefaTreinamentoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("feedbacks")]
        [AbacAuthorize("RhTreinamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarFeedback([FromBody] RegistrarFeedbackTarefaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("certificacoes")]
        [AbacAuthorize("RhTreinamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarCertificacoes()
            => Ok(await _mediator.Send(new ListarCertificacoesQuery()));
    }
}
