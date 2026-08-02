using System;
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
    /// ESG-TSU (Transporte Sustentavel) — construido no V1 (CD1).
    /// Controller fino: apenas MediatR. Protegido por ABAC (recurso "EsgTransporte").
    /// Calculo CO2e/tkm usa fator compartilhado com o GHG (NF-07); sem fator vigente fica pendente.
    /// </summary>
    [ApiController]
    [Route("api/v1/esg/tsu")]
    [Produces("application/json")]
    public class EsgTsuController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EsgTsuController(IMediator mediator) => _mediator = mediator;

        [HttpGet("registros")]
        [AbacAuthorize("EsgTransporte", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarRegistros()
            => Ok(await _mediator.Send(new ListarRegistrosTsuQuery()));

        [HttpGet("trechos/{id:guid}/calculos")]
        [AbacAuthorize("EsgTransporte", "Ler")]
        public async Task<IActionResult> ListarCalculos(Guid id)
            => Ok(await _mediator.Send(new ListarCalculosTsuQuery(id)));

        [HttpGet("registros/{id:guid}/metas-modal")]
        [AbacAuthorize("EsgTransporte", "Ler")]
        public async Task<IActionResult> ListarMetas(Guid id)
            => Ok(await _mediator.Send(new ListarMetasModalTsuQuery(id)));

        [HttpPost("registros")]
        [AbacAuthorize("EsgTransporte", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarRegistro([FromBody] CriarRegistroTsuCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("registros/{id:guid}/submeter")]
        [AbacAuthorize("EsgTransporte", "Editar")]
        public async Task<ActionResult<CommandResult>> Submeter(Guid id)
        {
            var result = await _mediator.Send(new SubmeterRegistroTsuCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("registros/{id:guid}/aprovar")]
        [AbacAuthorize("EsgTransporte", "Editar")]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id)
        {
            var result = await _mediator.Send(new AprovarRegistroTsuCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("operacoes")]
        [AbacAuthorize("EsgTransporte", "Criar")]
        public async Task<ActionResult<CommandResult>> AdicionarOperacao([FromBody] AdicionarOperacaoTsuCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("trechos")]
        [AbacAuthorize("EsgTransporte", "Criar")]
        public async Task<ActionResult<CommandResult>> AdicionarTrecho([FromBody] AdicionarTrechoTsuCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("calcular")]
        [AbacAuthorize("EsgTransporte", "Criar")]
        public async Task<ActionResult<CommandResult>> Calcular([FromBody] CalcularEmissaoTsuCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("metas-modal")]
        [AbacAuthorize("EsgTransporte", "Criar")]
        public async Task<ActionResult<CommandResult>> DefinirMeta([FromBody] DefinirMetaModalTsuCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("medicoes-modal")]
        [AbacAuthorize("EsgTransporte", "Criar")]
        public async Task<ActionResult<CommandResult>> RegistrarMedicao([FromBody] RegistrarMedicaoModalTsuCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
