using System;
using System.Collections.Generic;
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
    /// PRJ-RSK (Gestao de Riscos de Projeto). Registro, kanban, comentarios, workflow e escalonamento.
    /// Controller fino: apenas MediatR. ABAC nega por padrao (submodulo novo sobe desabilitado; sem seed de permissao).
    /// Isolamento de tenant garantido pelo filtro global do ContextProjetos.
    /// </summary>
    [ApiController]
    [Route("api/v1/projetos/riscos")]
    [Produces("application/json")]
    public class ProjetosRiscosController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProjetosRiscosController(IMediator mediator) => _mediator = mediator;

        [HttpPost("estagios")]
        [AbacAuthorize("ProjetosRiscos", "Configurar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarEstagio([FromBody] CriarEstagioRiscoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("estagios")]
        [AbacAuthorize("ProjetosRiscos", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarEstagios()
        {
            var result = await _mediator.Send(new ObterEstagiosRiscoQuery());
            return Ok(result);
        }

        [HttpPost]
        [AbacAuthorize("ProjetosRiscos", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarRiscoProjetoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record MoverRequest(Guid EstagioDestinoId, Guid UsuarioId);

        [HttpPost("{id:guid}/mover")]
        [AbacAuthorize("ProjetosRiscos", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Mover(Guid id, [FromBody] MoverRequest request)
        {
            var result = await _mediator.Send(new MoverRiscoCommand(id, request.EstagioDestinoId, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record ComentarRequest(Guid UsuarioId, string Comentario);

        [HttpPost("{id:guid}/comentarios")]
        [AbacAuthorize("ProjetosRiscos", "Comentar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Comentar(Guid id, [FromBody] ComentarRequest request)
        {
            var result = await _mediator.Send(new ComentarRiscoCommand(id, request.UsuarioId, request.Comentario));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record PrioridadeRequest(EPrioridadeRisco Prioridade, Guid UsuarioId);

        [HttpPost("{id:guid}/prioridade")]
        [AbacAuthorize("ProjetosRiscos", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> AlterarPrioridade(Guid id, [FromBody] PrioridadeRequest request)
        {
            var result = await _mediator.Send(new AlterarPrioridadeRiscoCommand(id, request.Prioridade, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record UsuarioRequest(Guid UsuarioId);

        [HttpPost("{id:guid}/submeter")]
        [AbacAuthorize("ProjetosRiscos", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Submeter(Guid id, [FromBody] UsuarioRequest request)
        {
            var result = await _mediator.Send(new SubmeterRiscoCommand(id, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/aprovar")]
        [AbacAuthorize("ProjetosRiscos", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id, [FromBody] UsuarioRequest request)
        {
            var result = await _mediator.Send(new AprovarRiscoCommand(id, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record RejeitarRequest(string Motivo, Guid UsuarioId);

        [HttpPost("{id:guid}/rejeitar")]
        [AbacAuthorize("ProjetosRiscos", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Rejeitar(Guid id, [FromBody] RejeitarRequest request)
        {
            var result = await _mediator.Send(new RejeitarRiscoCommand(id, request.Motivo, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/escalonar")]
        [AbacAuthorize("ProjetosRiscos", "Escalonar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Escalonar(Guid id, [FromBody] UsuarioRequest request)
        {
            var result = await _mediator.Send(new EscalonarRiscoCommand(id, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/encerrar")]
        [AbacAuthorize("ProjetosRiscos", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Encerrar(Guid id, [FromBody] RejeitarRequest request)
        {
            var result = await _mediator.Send(new EncerrarRiscoCommand(id, request.Motivo, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("projeto/{projetoId:guid}")]
        [AbacAuthorize("ProjetosRiscos", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPorProjeto(Guid projetoId, [FromQuery] string? prioridade)
        {
            var result = await _mediator.Send(new ObterRiscosPorProjetoQuery(projetoId, prioridade));
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ProjetosRiscos", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> Obter(Guid id)
        {
            var result = await _mediator.Send(new ObterRiscoPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }
}
