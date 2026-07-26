using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// ESG-ECO (Economia Circular: devolucoes, fluxo circular, triagem, destino, metas e medicoes).
    /// Controller fino: apenas MediatR. Protegido por ABAC (recurso "EsgEconomiaCircular").
    /// Submodulo novo: sobe desabilitado (ABAC nega por padrao ate a permissao ser semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/esg/eco")]
    [Produces("application/json")]
    public class EsgEcoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EsgEcoController(IMediator mediator) => _mediator = mediator;

        [HttpGet("devolucoes")]
        [AbacAuthorize("EsgEconomiaCircular", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarDevolucoes()
            => Ok(await _mediator.Send(new ListarDevolucoesEcoQuery()));

        [HttpGet("fluxos")]
        [AbacAuthorize("EsgEconomiaCircular", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarFluxos()
            => Ok(await _mediator.Send(new ListarFluxosCircularesQuery()));

        [HttpPost("devolucoes")]
        [AbacAuthorize("EsgEconomiaCircular", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ImportarDevolucao([FromBody] ImportarDevolucaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("fluxos")]
        [AbacAuthorize("EsgEconomiaCircular", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarFluxo([FromBody] CriarFluxoCircularCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("triagens")]
        [AbacAuthorize("EsgEconomiaCircular", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarTriagem([FromBody] RegistrarTriagemCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("destinos")]
        [AbacAuthorize("EsgEconomiaCircular", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarDestino([FromBody] RegistrarDestinoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("metas")]
        [AbacAuthorize("EsgEconomiaCircular", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DefinirMeta([FromBody] DefinirMetaCircularCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("medicoes")]
        [AbacAuthorize("EsgEconomiaCircular", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarMedicao([FromBody] RegistrarMedicaoCircularCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
