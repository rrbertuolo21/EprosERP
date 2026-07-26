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
    /// PRJ-ENC (Encerramento de Projeto). Processo formal de conclusao, cancelamento e arquivamento.
    /// Controller fino: apenas MediatR. ABAC nega por padrao (submodulo novo sobe desabilitado; sem seed de permissao).
    /// Isolamento de tenant garantido pelo filtro global do ContextProjetos.
    /// </summary>
    [ApiController]
    [Route("api/v1/projetos/encerramento")]
    [Produces("application/json")]
    public class ProjetosEncerramentoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProjetosEncerramentoController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        [AbacAuthorize("ProjetosEncerramento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarEncerramentoProjetoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record AdicionarItemRequest(int Sequencia, decimal? Quantidade, string? Observacao);

        [HttpPost("{id:guid}/itens")]
        [AbacAuthorize("ProjetosEncerramento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarItem(Guid id, [FromBody] AdicionarItemRequest request)
        {
            var result = await _mediator.Send(new AdicionarItemEncerramentoCommand(id, request.Sequencia, request.Quantidade, request.Observacao));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record AnexarDocumentoRequest(Guid ArquivoId);

        [HttpPost("{id:guid}/anexos")]
        [AbacAuthorize("ProjetosEncerramento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Anexar(Guid id, [FromBody] AnexarDocumentoRequest request)
        {
            var result = await _mediator.Send(new AnexarDocumentoEncerramentoCommand(id, request.ArquivoId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/submeter")]
        [AbacAuthorize("ProjetosEncerramento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Submeter(Guid id)
        {
            var result = await _mediator.Send(new SubmeterEncerramentoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record AprovarRequest(EStatusFinalProjeto StatusFinalProjeto);

        [HttpPost("{id:guid}/aprovar")]
        [AbacAuthorize("ProjetosEncerramento", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id, [FromBody] AprovarRequest request)
        {
            var result = await _mediator.Send(new AprovarEncerramentoCommand(id, request.StatusFinalProjeto));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record MotivoRequest(string Motivo);

        [HttpPost("{id:guid}/rejeitar")]
        [AbacAuthorize("ProjetosEncerramento", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Rejeitar(Guid id, [FromBody] MotivoRequest request)
        {
            var result = await _mediator.Send(new RejeitarEncerramentoCommand(id, request.Motivo));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/suspender")]
        [AbacAuthorize("ProjetosEncerramento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Suspender(Guid id)
        {
            var result = await _mediator.Send(new SuspenderEncerramentoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/retomar")]
        [AbacAuthorize("ProjetosEncerramento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Retomar(Guid id)
        {
            var result = await _mediator.Send(new RetomarEncerramentoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/encerrar")]
        [AbacAuthorize("ProjetosEncerramento", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Encerrar(Guid id, [FromBody] MotivoRequest request)
        {
            var result = await _mediator.Send(new EncerrarEncerramentoCommand(id, request.Motivo));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/arquivar")]
        [AbacAuthorize("ProjetosEncerramento", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Arquivar(Guid id)
        {
            var result = await _mediator.Send(new ArquivarEncerramentoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet]
        [AbacAuthorize("ProjetosEncerramento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
        {
            var result = await _mediator.Send(new ObterEncerramentosQuery(status, pagina, tamanhoPagina));
            return Ok(result);
        }

        [HttpGet("projeto/{projetoId:guid}")]
        [AbacAuthorize("ProjetosEncerramento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPorProjeto(Guid projetoId)
        {
            var result = await _mediator.Send(new ObterEncerramentosPorProjetoQuery(projetoId));
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ProjetosEncerramento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> Obter(Guid id)
        {
            var result = await _mediator.Send(new ObterEncerramentoPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }
}
