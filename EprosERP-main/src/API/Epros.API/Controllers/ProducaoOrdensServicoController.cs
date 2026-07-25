using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// PRD-GOS — Ordens de Serviço / Ficha de Produção customizada vinculada à venda.
    /// Controller fino. Protegido por ABAC (recurso "ProducaoOrdensServico"). Sobe DESABILITADO por padrão.
    /// Filtro de tenant global (ContextBase).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/producao/fichas")]
    public class ProducaoOrdensServicoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProducaoOrdensServicoController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("ProducaoOrdensServico", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar(
            [FromQuery] int? situacao,
            [FromQuery] Guid? pessoaId,
            [FromQuery] Guid? vendaId,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListarFichasProducaoQuery(situacao, pessoaId, vendaId, pagina, tamanhoPagina), ct);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ProducaoOrdensServico", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ObterFichaProducaoPorIdQuery(id), ct);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("ProducaoOrdensServico", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarFichaProducaoCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}/configuracao")]
        [AbacAuthorize("ProducaoOrdensServico", "Atualizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AtualizarConfiguracao(Guid id, [FromBody] AtualizarConfiguracaoFichaProducaoCommand command, CancellationToken ct)
        {
            if (id != command.Id)
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/iniciar")]
        [AbacAuthorize("ProducaoOrdensServico", "Atualizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Iniciar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new IniciarProducaoFichaCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/concluir")]
        [AbacAuthorize("ProducaoOrdensServico", "Atualizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Concluir(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ConcluirFichaProducaoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
