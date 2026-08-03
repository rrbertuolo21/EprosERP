using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Producao.Application.Commands;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// PRD — Motores de domínio da Produção expostos sob demanda (antes órfãos, sem caller HTTP):
    /// explosão de BOM, custo por rollup, avaliação de capacidade e sequenciamento (APS).
    /// Controller fino: apenas MediatR. Protegido por ABAC pelos recursos já existentes.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/producao")]
    public class ProducaoMotoresController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProducaoMotoresController(IMediator mediator) => _mediator = mediator;

        /// <summary>Explode a estrutura vigente (multinível) para a quantidade solicitada.</summary>
        [HttpPost("bom/explodir")]
        [AbacAuthorize("ProducaoBom", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExplodirBom([FromBody] ExplodirBomCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Custo previsto por rollup multinível (taxas MOD/CIF: valida-contador).</summary>
        [HttpPost("custos/rollup")]
        [AbacAuthorize("ProducaoCustos", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CalcularCustoRollup([FromBody] CalcularCustoRollupCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Avalia carga × capacidade de um centro de trabalho (gargalo/ATP).</summary>
        [HttpPost("planejamento/capacidade/avaliar")]
        [AbacAuthorize("ProducaoPlanejamento", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AvaliarCapacidade([FromBody] AvaliarCapacidadeCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Sequencia operações por precedência/prioridade com capacidade finita por centro.</summary>
        [HttpPost("esc/sequenciar")]
        [AbacAuthorize("ProducaoEsc", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Sequenciar([FromBody] SequenciarOperacoesCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
