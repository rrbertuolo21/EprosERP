using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Análise e Planejamento de Estoque (EST-APE). Consulta de posição (disponível = saldo − reservado,
    /// status de planejamento), parametrização de mínimo/máximo/custeio (APE-012), reprocessamento de
    /// alertas de reposição/excesso (APE-010/011 — gatilho de requisição de compra) e ciclo de alertas.
    /// ABAC conforme EF §19 (estoque.ape.*).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/estoque-analise")]
    public class AnalisePlanejamentoEstoqueController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AnalisePlanejamentoEstoqueController(IMediator mediator) => _mediator = mediator;

        [HttpGet("posicoes")]
        [AbacAuthorize("EstoqueAnalise", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ConsultarPosicoes([FromQuery] Guid? produtoId, [FromQuery] bool apenasEmAlerta = false, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var r = await _mediator.Send(new ConsultarPosicaoEstoqueQuery(produtoId, apenasEmAlerta, pagina, tamanhoPagina), ct);
            return r.Sucesso ? Ok(r) : BadRequest(r);
        }

        [HttpPut("posicoes/{id:guid}/parametros")]
        [AbacAuthorize("EstoqueAnalise", "Parametrizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Parametrizar(Guid id, [FromBody] ParametrizarPosicaoEstoqueCommand command, CancellationToken ct)
        {
            if (id != command.EstoqueProdutoId) return BadRequest("O ID da rota não corresponde ao corpo.");
            var r = await _mediator.Send(command, ct);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("alertas/reprocessar")]
        [AbacAuthorize("EstoqueAnalise", "ReprocessarAlertas")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ReprocessarAlertas([FromQuery] Guid? empresaId, CancellationToken ct)
        {
            var r = await _mediator.Send(new ReprocessarAlertasEstoqueCommand(empresaId), ct);
            return r.Sucesso ? Ok(r) : BadRequest(r);
        }

        [HttpGet("alertas")]
        [AbacAuthorize("EstoqueAnalise", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarAlertas([FromQuery] EStatusAlertaEstoque? status, [FromQuery] ETipoAlertaEstoque? tipo, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var r = await _mediator.Send(new ListarAlertasEstoqueQuery(status, tipo, pagina, tamanhoPagina), ct);
            return r.Sucesso ? Ok(r) : BadRequest(r);
        }

        [HttpPost("alertas/{id:guid}/resolver")]
        [AbacAuthorize("EstoqueAnalise", "Parametrizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ResolverAlerta(Guid id, [FromQuery] bool ignorar = false, CancellationToken ct = default)
        {
            var r = await _mediator.Send(new ResolverAlertaEstoqueCommand(id, ignorar), ct);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }
    }
}
