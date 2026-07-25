using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// PRJ-PRT (Portfolio e Priorizacao). Selecao, priorizacao, workflow de aprovacao e capacidade x demanda.
    /// Controller fino: apenas MediatR. ABAC nega por padrao (submodulo novo sobe desabilitado; sem seed de permissao).
    /// Isolamento de tenant garantido pelo filtro global do ContextProjetos.
    /// </summary>
    [ApiController]
    [Route("api/v1/projetos/portfolio")]
    [Produces("application/json")]
    public class ProjetosPortfolioController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProjetosPortfolioController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        [AbacAuthorize("ProjetosPortfolio", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarPortfolioCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/itens")]
        [AbacAuthorize("ProjetosPortfolio", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> AdicionarItem(Guid id, [FromBody] AdicionarItemPortfolioBody body)
        {
            var result = await _mediator.Send(new AdicionarItemPortfolioCommand(
                id, body.Sequencia, body.TipoItem, body.ProjetoId, body.ProgramaId, body.Titulo,
                body.ValorEstimado, body.EsforcoEstimado, body.CapacidadeRequerida, body.Npv, body.Payback,
                body.AlinhamentoEstrategico, body.Risco, body.Score, body.JustificativaPrioridade, body.Observacao));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record AdicionarItemPortfolioBody(
            int Sequencia,
            string TipoItem,
            Guid? ProjetoId,
            Guid? ProgramaId,
            string? Titulo,
            decimal? ValorEstimado,
            decimal? EsforcoEstimado,
            decimal? CapacidadeRequerida,
            decimal? Npv,
            decimal? Payback,
            decimal? AlinhamentoEstrategico,
            decimal? Risco,
            decimal? Score,
            string? JustificativaPrioridade,
            string? Observacao);

        public record PriorizarRequest(decimal? ScoreTotal, string Justificativa, Guid UsuarioId);

        [HttpPost("{id:guid}/priorizar")]
        [AbacAuthorize("ProjetosPortfolio", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Priorizar(Guid id, [FromBody] PriorizarRequest request)
        {
            var result = await _mediator.Send(new PriorizarPortfolioManualCommand(id, request.ScoreTotal, request.Justificativa, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record AnexarPortfolioRequest(Guid? ItemId, Guid ArquivoId, string? TipoAnexo);

        [HttpPost("{id:guid}/anexos")]
        [AbacAuthorize("ProjetosPortfolio", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Anexar(Guid id, [FromBody] AnexarPortfolioRequest request)
        {
            var result = await _mediator.Send(new AnexarDocumentoPortfolioCommand(id, request.ItemId, request.ArquivoId, request.TipoAnexo));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record UsuarioRequest(Guid UsuarioId);

        [HttpPost("{id:guid}/submeter")]
        [AbacAuthorize("ProjetosPortfolio", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Submeter(Guid id, [FromBody] UsuarioRequest request)
        {
            var result = await _mediator.Send(new SubmeterPortfolioCommand(id, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/aprovar")]
        [AbacAuthorize("ProjetosPortfolio", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id, [FromBody] UsuarioRequest request)
        {
            var result = await _mediator.Send(new AprovarPortfolioCommand(id, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record RejeitarRequest(string Motivo, Guid UsuarioId);

        [HttpPost("{id:guid}/rejeitar")]
        [AbacAuthorize("ProjetosPortfolio", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Rejeitar(Guid id, [FromBody] RejeitarRequest request)
        {
            var result = await _mediator.Send(new RejeitarPortfolioCommand(id, request.Motivo, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/suspender")]
        [AbacAuthorize("ProjetosPortfolio", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Suspender(Guid id, [FromBody] UsuarioRequest request)
        {
            var result = await _mediator.Send(new SuspenderPortfolioCommand(id, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/retomar")]
        [AbacAuthorize("ProjetosPortfolio", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Retomar(Guid id, [FromBody] UsuarioRequest request)
        {
            var result = await _mediator.Send(new RetomarPortfolioCommand(id, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/encerrar")]
        [AbacAuthorize("ProjetosPortfolio", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Encerrar(Guid id, [FromBody] UsuarioRequest request)
        {
            var result = await _mediator.Send(new EncerrarPortfolioCommand(id, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/inativar")]
        [AbacAuthorize("ProjetosPortfolio", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Inativar(Guid id, [FromBody] UsuarioRequest request)
        {
            var result = await _mediator.Send(new InativarPortfolioCommand(id, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/reativar")]
        [AbacAuthorize("ProjetosPortfolio", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Reativar(Guid id, [FromBody] UsuarioRequest request)
        {
            var result = await _mediator.Send(new ReativarPortfolioCommand(id, request.UsuarioId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet]
        [AbacAuthorize("ProjetosPortfolio", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
        {
            var result = await _mediator.Send(new ObterPortfoliosQuery(status, pagina, tamanhoPagina));
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ProjetosPortfolio", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> Obter(Guid id)
        {
            var result = await _mediator.Send(new ObterPortfolioPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }
}
