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
    /// ESG-REL (Relatorios ESG: frameworks, requisitos, itens, indicadores e snapshots).
    /// Controller fino: apenas MediatR. Protegido por ABAC (recurso "EsgRelatorios").
    /// Submodulo novo: sobe desabilitado (ABAC nega por padrao ate a permissao ser semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/esg/relatorios")]
    [Produces("application/json")]
    public class EsgRelController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EsgRelController(IMediator mediator) => _mediator = mediator;

        [HttpGet("frameworks")]
        [AbacAuthorize("EsgRelatorios", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarFrameworks()
            => Ok(await _mediator.Send(new ListarFrameworksRelQuery()));

        [HttpGet("{relatorioId:guid}/itens")]
        [AbacAuthorize("EsgRelatorios", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarItens(Guid relatorioId)
            => Ok(await _mediator.Send(new ListarItensRelatorioQuery(relatorioId)));

        [HttpPost("frameworks")]
        [AbacAuthorize("EsgRelatorios", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarFramework([FromBody] CriarFrameworkRelCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("requisitos")]
        [AbacAuthorize("EsgRelatorios", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarRequisito([FromBody] AdicionarRequisitoRelCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("itens")]
        [AbacAuthorize("EsgRelatorios", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarItem([FromBody] AdicionarItemRelatorioCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("indicadores")]
        [AbacAuthorize("EsgRelatorios", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> VincularIndicador([FromBody] VincularIndicadorReferenciaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("snapshots")]
        [AbacAuthorize("EsgRelatorios", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CapturarSnapshot([FromBody] CapturarSnapshotIndicadorCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("pendencias")]
        [AbacAuthorize("EsgRelatorios", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AbrirPendencia([FromBody] AbrirPendenciaRelatorioCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
