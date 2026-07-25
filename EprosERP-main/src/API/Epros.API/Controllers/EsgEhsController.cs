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
    /// ESG-EHS (Gestao Ambiental / Saude e Seguranca do Trabalho).
    /// Controller fino: apenas MediatR. Protegido por ABAC (recurso "EsgGestaoAmbiental").
    /// Submodulo novo: sobe desabilitado (ABAC nega por padrao ate a permissao ser semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/esg/ehs")]
    [Produces("application/json")]
    public class EsgEhsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EsgEhsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("registros")]
        [AbacAuthorize("EsgGestaoAmbiental", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarRegistros()
            => Ok(await _mediator.Send(new ListarRegistrosEhsQuery()));

        [HttpGet("incidentes")]
        [AbacAuthorize("EsgGestaoAmbiental", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarIncidentes()
            => Ok(await _mediator.Send(new ListarIncidentesQuery()));

        [HttpGet("licencas")]
        [AbacAuthorize("EsgGestaoAmbiental", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarLicencas()
            => Ok(await _mediator.Send(new ListarLicencasAmbientaisQuery()));

        [HttpPost("registros")]
        [AbacAuthorize("EsgGestaoAmbiental", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarRegistro([FromBody] CriarRegistroEhsCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("atividades")]
        [AbacAuthorize("EsgGestaoAmbiental", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarAtividade([FromBody] RegistrarAtividadeOcupacionalCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("fatores-risco")]
        [AbacAuthorize("EsgGestaoAmbiental", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarFatorRisco([FromBody] RegistrarFatorRiscoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("licencas")]
        [AbacAuthorize("EsgGestaoAmbiental", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarLicenca([FromBody] RegistrarLicencaAmbientalCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("residuos")]
        [AbacAuthorize("EsgGestaoAmbiental", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarResiduo([FromBody] RegistrarResiduoMovimentoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("incidentes")]
        [AbacAuthorize("EsgGestaoAmbiental", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarIncidente([FromBody] RegistrarIncidenteCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("acoes-corretivas")]
        [AbacAuthorize("EsgGestaoAmbiental", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarAcaoCorretiva([FromBody] RegistrarAcaoCorretivaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
